namespace Jobs;
using Database.Models;
using Database.Enums;
using Services;
using System.Text;

public class EmailSender : BackgroundService
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly EmailService _emailService;
    private readonly TaskService _taskService;
    private readonly DocuSignService _docuSignService;
    public EmailSender(
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
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }
    protected async Task DoWork()
    {
        var tasks = _taskService.GetTasksByStep(TaskStep.Failed).ToList();
        foreach(var task in tasks)
        {
            var envelope = await _docuSignService.GetEnvelopeAsync(task.DocuSignEnvelopeId);
            var toEmail = envelope.Sender.Email;
            var taskLogs = _taskService.GetTaskLogs(task.Id);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Your contract is failed");
            sb.AppendLine("Please contact us for more information");
            sb.AppendLine("====================================");
            foreach(var log in taskLogs)
            {
                sb.AppendLine($"[{log.Step}]: {log.Log} - {log.Created}");
            }
            _emailService.SendEmail(toEmail, "Your contract is failed", sb.ToString());

            _taskService.ChangeStep(task.Id, TaskStep.Completed);
        }
    }
}