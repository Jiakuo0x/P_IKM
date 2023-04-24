using Database.Enums;
using Database.Models;
using Lib.DocuSign;
using Services;
using System.Data;

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

                if (await IsEStampRequire(createContractModel) is false) continue;

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
        if (eStampRequire is null) throw new Exception("System Error: Not found the FormData 'eStamp'.");
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
        var documents = await CreateContractDocuments(createContractModel);
        var sender = CreateContractSender(createContractModel);
        var roles = CreateContractRoles(createContractModel);

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
        Dictionary<string, object>? mainDocument = null;
        List<object> attachments = new();
        List<object> privateLetterFileInfos = new();

        foreach (var document in createContractModel.Envelope.EnvelopeDocuments)
        {
            if (document.Name == "Summary") continue;

            var docFile = await _docuSign.DownloadDocument(createContractModel.Envelope.EnvelopeId, document.DocumentId);
            var docContent = _documentService.DecryptDocument(docFile);

            var appendingSignLables = await AppendingSignLables(createContractModel, document.DocumentId);

            var item = new Dictionary<string, object>();
            if (mainDocument is null && appendingSignLables is not null)
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
                mainDocument = item;
            }
            else if (mainDocument is not null && appendingSignLables is not null)
            {
                item.Add("fileName", $"{document.Name}.pdf");
                item.Add("content", docContent);
                attachments.Add(item);
            }
            else
            {
                item.Add("fileName", $"{document.Name}.pdf");
                item.Add("content", docContent);
                privateLetterFileInfos.Add(item);
            }
        }
        if (mainDocument is null) throw new Exception("System Error: Not found the main contract.");
        mainDocument.Add("attachments", attachments);

        createContractModel.PrivateLetterFileInfos = privateLetterFileInfos;

        return mainDocument;
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
        var pX = double.Parse(xPosition);
        var x = pX / 1000;
        return x;
    }

    protected double GetYPosition(string yPosition)
    {
        var pY = double.Parse(yPosition);

        double y;
        y = 1 - (pY / 1000) - 0.1;

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

        Dictionary<string, object> roleA = new();
        roleA.Add("userInfo", new
        {
            userAccount = roleAAccount,
            enterpriseName = roleACompanyName,
        });
        roleA.Add("routeOrder", 2);
        roleA.Add("roleName", "IKEA");
        roleA.Add("receiverType", "SIGNER");
        roleA.Add("userType", "ENTERPRISE");
        if (createContractModel.PrivateLetterFileInfos.Count > 0)
            roleA.Add("communicateInfo", new
            {
                privateLetter = "签约须知内容",
                privateLetterFileInfos = createContractModel.PrivateLetterFileInfos,
            });

        result.Add(roleA);

        var roleBAccount = MatchParameterMapping(createContractModel, BestSignDataType.RoleBAccount);
        var roleBCompanyName = MatchParameterMapping(createContractModel, BestSignDataType.RoleBCompanyName);

        if (!string.IsNullOrEmpty(roleBCompanyName))
        {
            Dictionary<string, object> roleB = new();
            if (!string.IsNullOrEmpty(roleBAccount))
            {
                roleB.Add("userInfo", new
                {
                    userAccount = roleBAccount,
                    enterpriseName = roleBCompanyName,
                });
            }
            else
            {
                roleB.Add("userInfo", new
                {
                    enterpriseName = roleBCompanyName,
                });
                roleB.Add("proxyClaimer", new
                {
                    ifProxyClaimer = "true"
                });
            }
            roleB.Add("routeOrder", 1);
            roleB.Add("roleName", "Customer");
            roleB.Add("receiverType", "SIGNER");
            roleB.Add("userType", "ENTERPRISE");
            result.Add(roleB);
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
    public List<object> PrivateLetterFileInfos { get; set; } = null!;
}

public class CreateContractSuccessModel
{
    public string ContractId { get; set; } = null!;
}