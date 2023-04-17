using Database.Enums;
using Services;
using System.IO.Compression;

namespace Jobs;

public class DocuSignContractUploader : BackgroundService
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TaskService _taskService;
    private readonly DocuSignService _docuSignService;
    private readonly BestSignService _bestSignService;
    public DocuSignContractUploader(
        ILogger<EmailSender> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
        _taskService = serviceProvider.GetRequiredService<TaskService>();
        _docuSignService = serviceProvider.GetRequiredService<DocuSignService>();
        _bestSignService = serviceProvider.GetRequiredService<BestSignService>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                await DoWork();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DocuSignReader");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromDays(1));
            }
        }
    }

    protected async Task DoWork()
    {
        var tasks = _taskService.GetTasksByStep(TaskStep.ContractCompleted);
        foreach (var task in tasks)
        {
            try
            {
                var result = await _bestSignService.PostAsStream("/api/contracts/download-file", new
                {
                    contractIds = new[] { task.BestSignContractId }
                });

                List<Document> documents = new List<Document>();
                ZipArchive zipArchive = new ZipArchive(result);
                int index = 800;
                foreach (var entry in zipArchive.Entries)
                {
                    documents.Add(new Document
                    {
                        DocumentId = $"{index}",
                        Order = $"{index}",
                        DocumentBase64 = ToBase64String(entry.Open()),
                        Name = entry.Name,
                    });

                    index++;
                }

                await _docuSignService.AppendDocuments(task.DocuSignEnvelopeId, documents);
                await _docuSignService.RemoveListener(task.DocuSignEnvelopeId);
                _taskService.ChangeStep(task.Id, TaskStep.Completed);
            }
            catch (Exception ex)
            {
                _taskService.LogError(task.Id, ex.Message);
            }
        }
    }

    protected string ToBase64String(Stream stream)
    {
        byte[] bytes;
        using (MemoryStream ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            bytes = ms.ToArray();
        }
        return Convert.ToBase64String(bytes);
    }
}