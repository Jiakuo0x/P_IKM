using Database.Enums;
using Database.Models;
using Services;
using System.Data;

namespace Jobs;

/// <summary>
/// The job is for creating a contract in Bestsign
/// </summary>
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
            // Create a dependency injection lifecycle
            using (var scope = _scopeFactory.CreateScope())
            {
                // Retrieve relevant objects from the dependency injection container
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
                    await Task.Delay(TimeSpan.FromSeconds(10));
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

                // Check if the envelope requires an electronic signature. If electronic signature is not required, skip processing this envelope.
                // This method also modify the step of the task
                if (await IsEStampRequire(createContractModel) is false) continue;

                var contract = await CreateContract(createContractModel);

                _taskService.UpdateTaskContractId(task.Id, contract.ContractId);
            }
            catch (Exception ex)
            {
                _taskService.LogError(task.Id, ex.Message);
            }
        }
    }

    /// <summary>
    /// Check if the envelope requires an electronic signature. If electronic signature is not required, skip processing this envelope.
    /// This method also modify the step of the task.
    /// </summary>
    /// <param name="createContractModel">The model of creating contract</param>
    /// <returns>Whether an electronic signature is required?</returns>
    /// <exception cref="Exception">The form data 'eStamp' was not found in the envelope</exception>
    protected async Task<bool> IsEStampRequire(CreateContractModel createContractModel)
    {
        var eStampRequire = MatchParameterMapping(createContractModel, BestSignDataType.Tab_eStampRequire);
        if (eStampRequire is null) throw new Exception("System Error: Not found the FormData 'eStamp'.");

        // If the value of from data 'eStamp' is not 'e-Stamp', then it is not required
        if (eStampRequire != "e-Stamp")
        {
            _taskService.LogInfo(createContractModel.Task.Id, "e-Stamp is not required.");
            _taskService.ChangeStep(createContractModel.Task.Id, TaskStep.Completed);

            // Remove the listener to allow the envelope to continue to flow
            await _docuSign.RemoveListener(createContractModel.Envelope.EnvelopeId);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Retrieve the template mapping configuration that match the envelope
    /// </summary>
    /// <param name="createContractModel">The model of creating contract</param>
    /// <returns>Template mapping configuration</returns>
    /// <exception cref="Exception">The custom field 'eStamp Type' was not found in the envelope</exception>
    protected TemplateMapping MatchTemplateMapping(CreateContractModel createContractModel)
    {
        var envelopeType = createContractModel.Envelope
            .CustomFields.ListCustomFields.SingleOrDefault(i => i.Name == "eStamp Type");
        if (envelopeType == null) throw new Exception("System Error: Not found the custom field 'eStamp Type'.");

        var templateMapping = _templateMappingService.GetMappingByDocuSignId(envelopeType.Value);

        return templateMapping;
    }

    /// <summary>
    /// Create contract in Bestsign
    /// </summary>
    /// <param name="createContractModel">The model of creating contract</param>
    /// <returns>The task of function</returns>
    protected async Task<CreateContractSuccessModel> CreateContract(CreateContractModel createContractModel)
    {
        // Generate the object parameters required to create the contract
        var documents = await CreateContractDocuments(createContractModel);
        var sender = CreateContractSender(createContractModel);
        var roles = CreateContractRoles(createContractModel);

        // Send an HTTP requeset to Bestsign to create the contract
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
    /// <summary>
    /// Generate the document object parameters required to create the contract
    /// </summary>
    /// <param name="createContractModel">The model of creating contract</param>
    /// <returns>The task of function</returns>
    /// <exception cref="Exception">No file requiring a signature was found</exception>
    protected async Task<object> CreateContractDocuments(CreateContractModel createContractModel)
    {
        List<object> documents = new();
        Dictionary<string, object>? mainDocument = null;
        List<object> attachments = new();
        List<object> privateLetterFileInfos = new();

        // Iterate through the documents in the DocuSign envelope
        foreach (var document in createContractModel.Envelope.EnvelopeDocuments)
        {
            // If the document name is 'Summary', then skip this document
            if (document.Name == "Summary") continue;

            // Download the document file
            var docFile = await _docuSign.DownloadDocument(createContractModel.Envelope.EnvelopeId, document.DocumentId);

            // Decrypt the document file
            var docContent = _documentService.DecryptDocument(docFile);

            // Generate the locations in the document where signatures are required
            var appendingSignLables = await AppendingSignLables(createContractModel, document);

            var item = new Dictionary<string, object>();
            // If there is no contract document yet, and the current document requires a signature, create the current document as the contract document
            if (mainDocument is null && appendingSignLables is not null)
            {
                item.Add("documentId", createContractModel.TemplateMapping!.BestSignConfiguration.DocumentId);
                item.Add("fileName", ConvertDocumentFileName(document.Name));
                item.Add("contractConfig", new
                {
                    contractTitle = ConvertDocumentFileName(document.Name),
                });
                item.Add("appendingSignLabels", appendingSignLables);
                item.Add("descriptionFields", GetDocumentDescriptionFields(createContractModel));
                item.Add("content", docContent);
                mainDocument = item;
            }
            // If there is already a contract document and the current document requires a signature, create the current document as a contract attachment
            else if (mainDocument is not null && appendingSignLables is not null)
            {
                item.Add("fileName", ConvertDocumentFileName(document.Name));
                item.Add("content", docContent);
                item.Add("appendingSignLabels", appendingSignLables);
                attachments.Add(item);
            }
            // If the document does not require a signature, create temporary data that will become the contracting Party A's signing instructions
            else
            {
                item.Add("fileName", ConvertDocumentFileName(document.Name));
                item.Add("content", docContent);
                privateLetterFileInfos.Add(item);
            }
        }
        if (mainDocument is null) throw new Exception("System Error: Not found the main contract.");

        if(attachments.Count > 0)
            mainDocument.Add("attachments", attachments);

        // Store the created signing instructions data temporarily for later processing
        createContractModel.PrivateLetterFileInfos = privateLetterFileInfos;

        documents.Add(mainDocument);
        return documents;

        // Convert envelope document file name to contract document file name
        string ConvertDocumentFileName(string fileName)
        {
            var index = fileName.LastIndexOf(".");
            var result = fileName.Substring(0, index);
            return result + ".pdf";
        }
    }

    /// <summary>
    /// Generate the locations in the document where signatures are required
    /// </summary>
    /// <param name="createContractModel">The model of creating contract</param>
    /// <param name="document">Envelope document</param>
    /// <returns>The task of the function</returns>
    protected async Task<List<Object>?> AppendingSignLables(CreateContractModel createContractModel, EnvelopeDocument document)
    {
        List<Object> result = new();

        // Get document tabs
        var docTabs = await _docuSign.GetDocumentTabs(createContractModel.Envelope.EnvelopeId, document.DocumentId);

        // Find the Party A stamp table name
        var aStampTabelName = MatchParameterMapping(createContractModel, BestSignDataType.AStampHere);
        if (!string.IsNullOrEmpty(aStampTabelName))
        {
            // Find all the location that needs to be signed by Party A
            var aStamps = docTabs.SignHereTabs?.Where(i => i.TabLabel == aStampTabelName).ToList() ?? new();
            foreach (var aStamp in aStamps)
            {
                var page = document.Pages.First(i => i.Sequence == aStamp.PageNumber);
                result.Add(new
                {
                    x = GetXPosition(aStamp.XPosition, page.Width),
                    y = GetYPosition(aStamp.YPosition, page.Height),
                    pageNumber = int.Parse(aStamp.PageNumber),
                    roleName = "IKEA",
                    type = "SEAL",
                });
            }
        }

        // Find the Party B stamp table name
        var bStampTabelName = MatchParameterMapping(createContractModel, BestSignDataType.BStampHere);
        if (!string.IsNullOrEmpty(bStampTabelName))
        {
            // Find all the location that needs to be signed by Party B
            var bStamps = docTabs.SignHereTabs?.Where(i => i.TabLabel == bStampTabelName).ToList() ?? new();
            foreach (var bStamp in bStamps)
            {
                var page = document.Pages.First(i => i.Sequence == bStamp.PageNumber);
                result.Add(new
                {
                    x = GetXPosition(bStamp.XPosition, page.Width),
                    y = GetYPosition(bStamp.YPosition, page.Height),
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

    /// <summary>
    /// Generate the custom description of the document
    /// </summary>
    /// <param name="createContractModel">The model of the creating contract</param>
    /// <returns></returns>
    protected object GetDocumentDescriptionFields(CreateContractModel createContractModel)
    {
        List<object> result = new List<object>();

        var parameterMappings = createContractModel.TemplateMapping?.ParameterMappings
            .Where(i => i.BestSignDataType == BestSignDataType.DescriptionFields) ?? new List<ParameterMapping>();

        // Iterate through the mapping configuration
        foreach (var parameterMapping in parameterMappings)
        {
            Dictionary<string, string> item = new Dictionary<string, string>();
            item.Add("fieldName", parameterMapping.BestSignDataName);
            item.Add("fieldValue", MatchParameterMapping(parameterMapping, createContractModel));
            result.Add(item);
        }
        return result;
    }

    /// <summary>
    /// Convert the x-coordinate of DocuSign to the x-coordinate of Bestsign
    /// </summary>
    /// <param name="xPosition">X-coordinate</param>
    /// <param name="width">Width of page</param>
    /// <returns></returns>
    protected double GetXPosition(string xPosition, string width)
    {
        var x = double.Parse(xPosition) / double.Parse(width) - 0.1;
        return x;
    }

    /// <summary>
    /// Convert the y-coordinate of DocuSign to the y-coordinate of Bestsign
    /// </summary>
    /// <param name="yPosition">Y-coordinate</param>
    /// <param name="height">Height of page</param>
    /// <returns></returns>
    protected double GetYPosition(string yPosition, string height)
    {
        var y = 1 - (double.Parse(yPosition) / double.Parse(height)) - 0.1;
        return y;
    }
    #endregion


    /// <summary>
    /// Generate the object data of contract sender
    /// </summary>
    /// <param name="createContractModel">The model of the creating contract</param>
    /// <returns>The object data</returns>
    /// <exception cref="Exception">Account, enterprise name, or business line of sender not found</exception>
    protected object CreateContractSender(CreateContractModel createContractModel)
    {
        var account = MatchParameterMapping(createContractModel, BestSignDataType.SenderAccount);
        if (account is null) throw new Exception("System Error: Not found the sender account in mapping.");

        var enterpriseName = MatchParameterMapping(createContractModel, BestSignDataType.SenderEnterpriseName);
        if (enterpriseName is null) throw new Exception("System Error: Not found the enterprise name of sender.");

        var businessLine = MatchParameterMapping(createContractModel, BestSignDataType.SenderBusinessLine);
        if (businessLine is null) throw new Exception("System Error: Not found the business line of sender.");

        var result = new
        {
            account = account,
            enterpriseName = enterpriseName,
            bizName = businessLine,
        };

        return result;
    }

    /// <summary>
    /// Generate the object data of contract roles
    /// </summary>
    /// <param name="createContractModel">The model of the creating contract</param>
    /// <returns>The object data</returns>
    /// <exception cref="Exception">Not found the information of Part A</exception>
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

        // If there are non-signed documents, attach them to the contracting Party A's signing instruction
        if (createContractModel.PrivateLetterFileInfos.Count > 0)
            roleA.Add("communicateInfo", new
            {
                privateLetter = "其他非盖章文件",
                privateLetterFileInfos = createContractModel.PrivateLetterFileInfos,
            });

        result.Add(roleA);

        var roleBAccount = MatchParameterMapping(createContractModel, BestSignDataType.RoleBAccount);
        var roleBCompanyName = MatchParameterMapping(createContractModel, BestSignDataType.RoleBCompanyName);

        // If the Party B is configured in the template, add the Party B role
        if (!string.IsNullOrEmpty(roleBCompanyName))
        {
            Dictionary<string, object> roleB = new();
            // If the Party B has account information configured, add it
            if (!string.IsNullOrEmpty(roleBAccount))
            {
                roleB.Add("userInfo", new
                {
                    userAccount = roleBAccount,
                    enterpriseName = roleBCompanyName,
                });
            }
            // If there is no account information for the Party B in the form data, then create an agent signing 
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

    /// <summary>
    /// Match the value mapped to the parameter
    /// </summary>
    /// <param name="createContractModel">The model of the creating contract</param>
    /// <param name="bestSignDataType">The type of The Bestsign data source</param>
    /// <returns>Matching value</returns>
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

    /// <summary>
    /// Match the value mapped to the parameter
    /// </summary>
    /// <param name="mapping">The parameter mapping object</param>
    /// <param name="createContractModel">The model of the creating contract</param>
    /// <returns>Matching value</returns>
    /// <exception cref="Exception">No matching value</exception>
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
        // If it is a checkbob group, concatenate the selected checkboxes in the format of "aa;bb"
        else if(mapping.DocuSignDataType == DocuSignDataType.CheckboxGroup)
        {
            List<string> checkboxNames = new();

            var formData = createContractModel.EnvelopeFormData.FormData;
            var formDataItem = formData.FirstOrDefault(i => i.Name == mapping.DocuSignDataName);
            if (formDataItem is null) throw new Exception($"Not found the FormDataItem: {mapping.DocuSignDataName}.");
            var checkboxs = formDataItem.Value.Split(";");
            foreach(var checkbox in checkboxs)
            {
                var checkboxValue = checkbox.Split(":");
                if (checkboxValue.Length > 1 && checkboxValue[1] == "X")
                    checkboxNames.Add(checkboxValue[0]);
            }

            var result = string.Join(';', checkboxNames);
            return result;
        }
        else
        {
            throw new Exception($"Unsupported data type in mapping: {mapping.DocuSignDataType}");
        }
    }
}

/// <summary>
/// The model of the creating contract
/// </summary>
public class CreateContractModel
{
    /// <summary>
    /// The task of electronic signature
    /// </summary>
    public ElectronicSignatureTask Task { get; set; } = null!;

    /// <summary>
    /// The configuration of the template mapping
    /// </summary>
    public TemplateMapping? TemplateMapping { get; set; }

    /// <summary>
    /// The envelope of DocuSign
    /// </summary>
    public Envelope Envelope { get; set; } = null!;

    /// <summary>
    /// The envelope form data of DocuSign envelope
    /// </summary>
    public EnvelopeFormData EnvelopeFormData { get; set; } = null!;

    /// <summary>
    /// The temporary signing instruction list object stored here is to avoid wasting performance caused by repeated retrieve
    /// </summary>
    public List<object> PrivateLetterFileInfos { get; set; } = null!;
}

/// <summary>
/// The Bestsign callback model of creating contract successful
/// </summary>
public class CreateContractSuccessModel
{
    /// <summary>
    /// The Bestsign contract ID
    /// </summary>
    public string ContractId { get; set; } = null!;
}