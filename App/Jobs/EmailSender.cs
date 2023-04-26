namespace Jobs;
using Database.Models;
using Database.Enums;
using Services;
using System.Text;

/// <summary>
/// The job is for sending emails to relevant recipient for tasks that have errors
/// </summary>
public class EmailSender : BackgroundService
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private TemplateMappingService _templateMappingService = null!;
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
                _templateMappingService = scope.ServiceProvider.GetRequiredService<TemplateMappingService>();
                _emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                _taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
                _docuSignService = scope.ServiceProvider.GetRequiredService<DocuSignService>();
                try
                {
                    await DoWork();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in EmailSender");
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
        var tasks = _taskService.GetTasksByStep(TaskStep.Failed).ToList();
        foreach (var task in tasks)
        {
            var envelope = await _docuSignService.GetEnvelopeAsync(task.DocuSignEnvelopeId);
            var envelopeFormData = await _docuSignService.GetEnvelopeFormDataAsync(task.DocuSignEnvelopeId);
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

            var templateMapping = MatchTemplateMapping(envelope);
            var appendingMappings = templateMapping.ParameterMappings.Where(i => i.BestSignDataType == BestSignDataType.DescriptionFields).ToList();
            foreach (var appendingMapping in appendingMappings)
            {
                var value = MatchParameterMapping(appendingMapping, envelope, envelopeFormData);
                sb.AppendLine($"{appendingMapping.BestSignDataName}:\t{value}");
            }
            sb.AppendLine("====================================");

            foreach (var log in taskLogs)
            {
                sb.AppendLine($"[{log.Step}]: {log.Log} - {log.Created.ToLocalTime()}");
            }

            var firstSigner = recipients.Signers.Select(i => new { i.Email, Order = int.Parse(i.RoutingOrder) }).MinBy(i => i.Order);
            if (firstSigner != null)
            {
                _emailService.SendEmail(firstSigner.Email, "Your contract is failed", sb.ToString());
                var adminEmail = _emailService.NotificationAdminEmail();
                if (adminEmail != null)
                    _emailService.SendEmail(adminEmail, "There is a contract that has failed", sb.ToString());
            }
            else
            {
                _taskService.LogError(task.Id, "Failed to send email notification. - Signer not found.");
            }

            _taskService.ChangeStep(task.Id, TaskStep.Completed);
            await _docuSignService.UpdateComment(task.DocuSignEnvelopeId, "The process has failed.");
            await _docuSignService.VoidedEnvelope(task.DocuSignEnvelopeId, sb.ToString());
        }
    }

    protected TemplateMapping MatchTemplateMapping(Envelope envelope)
    {
        var envelopeType = envelope
            .CustomFields.ListCustomFields.SingleOrDefault(i => i.Name == "eStamp Type");
        if (envelopeType == null) throw new Exception("System Error: Not found the custom field 'eStamp Type'.");

        var templateMapping = _templateMappingService.GetMappingByDocuSignId(envelopeType.Value);

        return templateMapping;
    }

    protected string? MatchParameterMapping(ParameterMapping mapping, Envelope envelope, EnvelopeFormData envelopeFormData)
    {
        if (mapping.DocuSignDataType == DocuSignDataType.FormData_Value)
        {
            var formData = envelopeFormData.FormData;
            var formDataItem = formData.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            return formDataItem?.Value;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.FormData_ListSelectedValue)
        {
            var formData = envelopeFormData.FormData;
            var formDataItem = formData.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            return formDataItem?.ListSelectedValue;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.TextCustomField)
        {
            var customFields = envelope.CustomFields.TextCustomFields;
            var customField = customFields.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            return customField?.Value;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.ListCustomField)
        {
            var customFields = envelope.CustomFields.ListCustomFields;
            var customField = customFields.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            return customField?.Value;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.ApplicantEmail)
        {
            var applicant = envelope.Recipients.Signers.MinBy(i => int.Parse(i.RoutingOrder));
            return applicant?.Email;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.SenderEmail)
        {
            return envelope.Sender.Email;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.CheckboxGroup)
        {
            StringBuilder result = new StringBuilder();

            var formData = envelopeFormData.FormData;
            var formDataItem = formData.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            if (formDataItem is null) return null;
            var checkboxs = formDataItem.Value.Split(";");
            foreach (var checkbox in checkboxs)
            {
                var checkboxValue = checkbox.Split(":");
                if (checkboxValue.Length > 1 && checkboxValue[1] == "X")
                    result.Append(checkboxValue[0]);
            }
            return result.ToString();
        }
        else
        {
            return null;
        }
    }
}