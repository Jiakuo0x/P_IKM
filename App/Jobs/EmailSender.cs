namespace Jobs;
using Database.Models;
using Database.Enums;
using Services;
using System.Text;

public class EmailSender : BackgroundService
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private EmailService _emailService = null!;
    private TaskService _taskService = null!;
    private DocuSignService _docuSignService = null!;
    public EmailSender(
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
            using (var scope = _scopeFactory.CreateScope())
            {
                _emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                _taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
                _docuSignService = scope.ServiceProvider.GetRequiredService<DocuSignService>();
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
    }
    protected async Task DoWork()
    {
        var tasks = _taskService.GetTasksByStep(TaskStep.Failed).ToList();
        foreach (var task in tasks)
        {
            var envelope = await _docuSignService.GetEnvelopeAsync(task.DocuSignEnvelopeId);
            var recipients = envelope.Recipients;

            var taskLogs = _taskService.GetTaskLogs(task.Id);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Your contract is failed");
            sb.AppendLine("Contract Info:");
            sb.AppendLine("DocuSign Envelope Id: " + task.DocuSignEnvelopeId);
            sb.AppendLine("BestSign Contract Id:" + task.BestSignContractId);
            sb.AppendLine("System Task Id: " + task.Id);
            sb.AppendLine("Please contact us for more information");
            sb.AppendLine("====================================");
            foreach (var log in taskLogs)
            {
                sb.AppendLine($"[{log.Step}]: {log.Log} - {log.Created}");
            }

            var firstSigner = recipients.Signers.Select(i => new { i.Email, Order = int.Parse(i.RoutingOrder) }).MinBy(i => i.Order);
            if (firstSigner != null)
            {
                _emailService.SendEmail(firstSigner.Email, "Your contract is failed", sb.ToString());
            }
            else
            {
                _taskService.LogError(task.Id, "Failed to send email notification. - Signer not found.");
            }

            await _docuSignService.VoidedEnvelope(task.DocuSignEnvelopeId, sb.ToString());
            _taskService.ChangeStep(task.Id, TaskStep.Completed);
        }
    }
}