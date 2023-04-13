using Database.Enums;
using Services;

namespace Jobs;

public class DocuSignConractUploader : BackgroundService
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly EmailService _emailService;
    private readonly TaskService _taskService;
    private readonly DocuSignService _docuSignService;
    public DocuSignConractUploader(
        ILogger<EmailSender> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
        _emailService = serviceProvider.GetRequiredService<EmailService>();
        _taskService = serviceProvider.GetRequiredService<TaskService>();
        _docuSignService = serviceProvider.GetRequiredService<DocuSignService>();
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

        }
    }
}