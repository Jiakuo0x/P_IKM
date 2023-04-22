using Database.Enums;
using Database.Models;
using Lib.DocuSign;
using Services;

namespace Jobs;

public class ContactCreator : BackgroundService
{
    private readonly ILogger<ContactCreator> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private DocuSignService _docuSign = null!;
    private BestSignService _bestSign = null!;
    private TaskService _taskService = null!;
    private TemplateMappingService _templateMappingService = null!;
    private DocumentService _documentService = null!;

    public ContactCreator(
        ILogger<ContactCreator> logger,
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
                _docuSign = scope.ServiceProvider.GetRequiredService<DocuSignService>();
                _bestSign = scope.ServiceProvider.GetRequiredService<BestSignService>();
                _taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
                _templateMappingService = scope.ServiceProvider.GetRequiredService<TemplateMappingService>();
                _documentService = scope.ServiceProvider.GetRequiredService<DocumentService>();
                try
                {
                    await DoWork();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ContractCreator");
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
        var tasks = _taskService.GetTasksByStep(TaskStep.ContractCreating);
        foreach (var task in tasks)
        {
            try
            {
                CreateContractModel createContractModel = new CreateContractModel { Task = task };

                createContractModel.Envelope = await _docuSign.GetEnvelopeAsync(task.DocuSignEnvelopeId);
                createContractModel.EnvelopeFormData = await _docuSign.GetEnvelopeFormDataAsync(task.DocuSignEnvelopeId);
                createContractModel.TemplateMapping = MatchTemplateMapping(createContractModel);

                if(await IsEStampRequire(createContractModel) is false) continue;

                var contract = await CreateContract(createContractModel);

                _taskService.UpdateTaskContractId(task.Id, contract.ContractId);
                _taskService.ChangeStep(task.Id, TaskStep.ContractCreated);
            }
            catch (Exception ex)
            {
                _taskService.LogError(task.Id, ex.Message);
            }
        }
    }

    protected async Task<bool> IsEStampRequire(CreateContractModel createContractModel)
    {
        var eStampRequire = MatchParameterMapping(createContractModel, BestSignDataType.Tab_eStampRequire);
        if(eStampRequire is null) throw new Exception("System Error: Not found the FormData 'eStamp'.");
        if (eStampRequire != "e-Stamp")
        {
            _taskService.LogInfo(createContractModel.Task.Id, "e-Stamp is not required.");
            _taskService.ChangeStep(createContractModel.Task.Id, TaskStep.Completed);
            await _docuSign.RemoveListener(createContractModel.Envelope.EnvelopeId);
            return false;
        }
        return true;
    }

    protected TemplateMapping MatchTemplateMapping(CreateContractModel createContractModel)
    {
        var envelopeType = createContractModel.Envelope
            .CustomFields.ListCustomFields.SingleOrDefault(i => i.Name == "eStamp Type");
        if (envelopeType == null) throw new Exception("System Error: Not found the custom field 'eStamp Type'.");

        var templateMapping = _templateMappingService.GetMappingByDocuSignId(envelopeType.Value);

        return templateMapping;
    }

    protected async Task<CreateContractSuccessModel> CreateContract(CreateContractModel createContractModel)
    {
        var sender = CreateContractSender(createContractModel);
        var roles = CreateContractRoles(createContractModel);
        var documents = await CreateContractDocuments(createContractModel);

        var apiResponse = await _bestSign.Post<CreateContractSuccessModel>($"/api/templates/send-contracts-sync-v2", new
        {
            sender = sender,
            templateId = createContractModel.TemplateMapping!.BestSignTemplateId,
            roles = roles,
            documents = documents,
        });

        return apiResponse;

    }

    # region CreateContractDocuments
    protected async Task<object> CreateContractDocuments(CreateContractModel createContractModel)
    {
        List<Object> result = new();

        Dictionary<string, object>? mainContract = null;
        List<Object> attachments = new();
        foreach (var document in createContractModel.Envelope.EnvelopeDocuments)
        {
            if (document.Name == "Summary") continue;

            var docFile = await _docuSign.DownloadDocument(createContractModel.Envelope.EnvelopeId, document.DocumentId);
            var docContent = _documentService.DecryptDocument(docFile);

            var appendingSignLables = await AppendingSignLables(createContractModel, document.DocumentId);

            var item = new Dictionary<string, object>();
            if (mainContract is null && appendingSignLables is not null)
            {
                item.Add("documentId", createContractModel.TemplateMapping!.BestSignConfiguration.DocumentId);
                item.Add("fileName", $"{document.Name}.pdf");
                item.Add("contractConfig", new
                {
                    contractTitle = $"{document.Name}.pdf",
                });
                item.Add("appendingSignLabels", appendingSignLables);
                item.Add("descriptionFields", GetDocumentDescriptionFields(createContractModel));
                item.Add("content", docContent);
                mainContract = item;
                result.Add(item);
            }
            else if (mainContract is not null && appendingSignLables is not null)
            {
                item.Add("fileName", $"{document.Name}.pdf");
                item.Add("contractConfig", new
                {
                    contractTitle = $"{document.Name}.pdf",
                });
                item.Add("appendingSignLabels", appendingSignLables);
                item.Add("descriptionFields", GetDocumentDescriptionFields(createContractModel));
                item.Add("content", docContent);
                result.Add(item);
            }
            else
            {
                item.Add("fileName", $"{document.Name}.pdf");
                item.Add("content", docContent);
                attachments.Add(item);
            }
        }
        if (mainContract is null) throw new Exception("System Error: Not found the main contract.");
        mainContract.Add("attachments", attachments);
        return result;
    }

    protected async Task<List<Object>?> AppendingSignLables(CreateContractModel createContractModel, string documentId)
    {
        List<Object> result = new();

        var docTabs = await _docuSign.GetDocumentTabs(createContractModel.Envelope.EnvelopeId, documentId);

        var aStampTabelName = MatchParameterMapping(createContractModel, BestSignDataType.AStampHere);
        if (!string.IsNullOrEmpty(aStampTabelName))
        {
            var aStamps = docTabs.SignHereTabs?.Where(i => i.TabLabel == aStampTabelName).ToList() ?? new();
            foreach (var aStamp in aStamps)
            {
                result.Add(new
                {
                    x = GetXPosition(aStamp.XPosition),
                    y = GetYPosition(aStamp.YPosition),
                    pageNumber = int.Parse(aStamp.PageNumber),
                    roleName = "IKEA",
                    type = "SEAL",
                });
            }
        }

        var bStampTabelName = MatchParameterMapping(createContractModel, BestSignDataType.BStampHere);
        if (!string.IsNullOrEmpty(bStampTabelName))
        {
            var bStamps = docTabs.SignHereTabs?.Where(i => i.TabLabel == bStampTabelName).ToList() ?? new();
            foreach (var bStamp in bStamps)
            {
                result.Add(new
                {
                    x = GetXPosition(bStamp.XPosition),
                    y = GetYPosition(bStamp.YPosition),
                    pageNumber = int.Parse(bStamp.PageNumber),
                    roleName = "Customer",
                    type = "SEAL",
                });
            }
        }
        
        if (result.Count == 0)
            return null;
        return result;
    }
    protected object GetDocumentDescriptionFields(CreateContractModel createContractModel)
    {
        List<object> result = new List<object>();

        var parameterMappings = createContractModel.TemplateMapping?.ParameterMappings
            .Where(i => i.BestSignDataType == BestSignDataType.DescriptionFields) ?? new List<ParameterMapping>();
        foreach (var parameterMapping in parameterMappings)
        {
            Dictionary<string, string> item = new Dictionary<string, string>();
            item.Add("fieldName", parameterMapping.BestSignDataName);
            item.Add("fieldValue", MatchParameterMapping(parameterMapping, createContractModel));
            result.Add(item);
        }
        return result;
    }
    protected double GetXPosition(string xPosition)
    {
        var x = double.Parse(xPosition)/720;
        return x;
    }

    protected double GetYPosition(string yPosition)
    {
        var y = 1 - (double.Parse(yPosition)/720*2);
        return y;
    }
    #endregion


    protected object CreateContractSender(CreateContractModel createContractModel)
    {
        var account = MatchParameterMapping(createContractModel, BestSignDataType.SenderAccount);
        if (account is null) throw new Exception("System Error: Not found the sender account in mapping.");

        var result = new
        {
            account = account,
            enterpriseName = createContractModel.TemplateMapping!.BestSignConfiguration.EnterpriseName,
            bizName = createContractModel.TemplateMapping!.BestSignConfiguration.BusinessLine,
        };

        return result;
    }

    protected object CreateContractRoles(CreateContractModel createContractModel)
    {
        List<Object> result = new();

        var parameterMappings = createContractModel.TemplateMapping?.ParameterMappings;

        var roleAAccount = MatchParameterMapping(createContractModel, BestSignDataType.RoleAAccount);
        if (roleAAccount is null) throw new Exception("System Error: Not found the role A account in mapping.");
        var roleACompanyName = MatchParameterMapping(createContractModel, BestSignDataType.RoleACompanyName);
        if (roleACompanyName is null) throw new Exception("System Error: Not found the role A company name in mapping.");

        result.Add(new
        {
            userInfo = new
            {
                userAccount = roleAAccount,
                enterpriseName = roleACompanyName,
            },
            routeOrder = 2,
            roleName = "IKEA",
            receiverType = "SIGNER",
            userType = "ENTERPRISE",
        });

        var roleBAccount = MatchParameterMapping(createContractModel, BestSignDataType.RoleBAccount);
        var roleBCompanyName = MatchParameterMapping(createContractModel, BestSignDataType.RoleBCompanyName);

        if (!string.IsNullOrEmpty(roleBCompanyName))
        {
            Dictionary<string, object> item = new();
            if (!string.IsNullOrEmpty(roleBAccount))
            {
                item.Add("userInfo", new
                {
                    userAccount = roleBAccount,
                    enterpriseName = roleBCompanyName,
                });
            }
            else
            {
                item.Add("userInfo", new
                {
                    enterpriseName = roleBCompanyName,
                });
                item.Add("proxyClaimer", new
                {
                    ifProxyClaimer = "true"
                });
            }
            item.Add("routeOrder", 1);
            item.Add("roleName", "Customer");
            item.Add("receiverType", "SIGNER");
            item.Add("userType", "ENTERPRISE");
            result.Add(item);
        }

        return result;
    }
    protected string? MatchParameterMapping(CreateContractModel createContractModel, BestSignDataType bestSignDataType)
    {
        var mapping = createContractModel.TemplateMapping?.ParameterMappings
            .FirstOrDefault(i => i.BestSignDataType == bestSignDataType);

        if (mapping is null) return null;

        if (bestSignDataType == BestSignDataType.AStampHere)
            return mapping.DocuSignDataName;
        if (bestSignDataType == BestSignDataType.BStampHere)
            return mapping.DocuSignDataName;
        else
            return MatchParameterMapping(mapping, createContractModel);
    }

    protected string MatchParameterMapping(ParameterMapping mapping, CreateContractModel createContractModel)
    {
        var test = createContractModel.Envelope;
        if (mapping.DocuSignDataType == DocuSignDataType.FormData_Value)
        {
            var formData = createContractModel.EnvelopeFormData.FormData;
            var formDataItem = formData.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            if (formDataItem is null) throw new Exception($"Not found the FormDataItem: {mapping.DocuSignDataName}.");
            return formDataItem.Value;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.FormData_ListSelectedValue)
        {
            var formData = createContractModel.EnvelopeFormData.FormData;
            var formDataItem = formData.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            if (formDataItem is null) throw new Exception($"Not found the FormDataItem: {mapping.DocuSignDataName}.");
            return formDataItem.ListSelectedValue;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.TextCustomField)
        {
            var customFields = createContractModel.Envelope.CustomFields.TextCustomFields;
            var customField = customFields.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            if (customField is null) throw new Exception($"Not found the TextCustomField: {mapping.DocuSignDataName}.");
            return customField.Value;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.ListCustomField)
        {
            var customFields = createContractModel.Envelope.CustomFields.ListCustomFields;
            var customField = customFields.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            if (customField is null) throw new Exception($"Not found the ListCustomField: {mapping.DocuSignDataName}.");
            return customField.Value;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.ApplicantEmail)
        {
            var applicant = createContractModel.Envelope.Recipients.Signers.MinBy(i => int.Parse(i.RoutingOrder));
            if (applicant is null) throw new Exception("Not found the applicant of envelope.");
            return applicant.Email;
        }
        else if (mapping.DocuSignDataType == DocuSignDataType.SenderEmail)
        {
            return createContractModel.Envelope.Sender.Email;
        }
        else
        {
            throw new Exception($"Unsupported data type in mapping: {mapping.DocuSignDataType}");
        }
    }
}

public class CreateContractModel
{
    public ElectronicSignatureTask Task { get; set; } = null!;
    public TemplateMapping? TemplateMapping { get; set; }
    public Envelope Envelope { get; set; } = null!;
    public EnvelopeFormData EnvelopeFormData { get; set; } = null!;
}

public class CreateContractSuccessModel
{
    public string ContractId { get; set; } = null!;
}