using Database.Enums;
using Services;

namespace Jobs;

public class DocuSignContractUploader : BackgroundService
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly EmailService _emailService;
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
        _emailService = serviceProvider.GetRequiredService<EmailService>();
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
            var result = await _bestSignService.PostAsStream("/api/contracts/download-file", new
            {
                contractIds = new[] { task.BestSignContractId }
            });
            if (!File.Exists("src/res.zip")) 
            {
                var file = File.Create("src/res.zip");
                file.Close();
            }
            File.WriteAllBytes("src/res.zip", result);
        }
    }
}