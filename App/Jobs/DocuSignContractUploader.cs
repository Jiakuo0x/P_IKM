using Database.Enums;
using Services;
using System.IO.Compression;

namespace Jobs;

/// <summary>
/// The job is for uploading documents from a Bestsign contract to a DocuSign envelope
/// </summary>
public class DocuSignContractUploader : BackgroundService
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private TaskService _taskService = null!;
    private DocuSignService _docuSignService = null!;
    private BestSignService _bestSignService = null!;
    public DocuSignContractUploader(
        ILogger<EmailSender> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            // Create a dependency injection lifecycle
            using (var scope =  _scopeFactory.CreateScope())
            {
                // Retrieve relevant objects from the dependency injection container
                _taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
                _docuSignService = scope.ServiceProvider.GetRequiredService<DocuSignService>();
                _bestSignService = scope.ServiceProvider.GetRequiredService<BestSignService>();
                try
                {
                    await DoWork();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DocuSignContractUploader");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
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