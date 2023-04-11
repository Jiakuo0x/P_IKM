using Database.Enums;
using Database.Models;
using Lib.DocuSign;
using Services;

namespace Jobs;

public class ContactCreator : BackgroundService
{
    private readonly ILogger<ContactCreator> _logger;
    private readonly DbContext _db;
    private readonly DocuSignService _docuSign;
    private readonly Lib.BestSign.ApiClient _bestSign;
    private readonly TaskService _taskService;
    private readonly DocumentService _documentService;

    public ContactCreator(
        ILogger<ContactCreator> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;

        var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
        _db = serviceProvider.GetRequiredService<DbContext>();
        _docuSign = serviceProvider.GetRequiredService<DocuSignService>();
        _bestSign = serviceProvider.GetRequiredService<Lib.BestSign.ApiClient>();
        _taskService = serviceProvider.GetRequiredService<TaskService>();
        _documentService = serviceProvider.GetRequiredService<DocumentService>();
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
                _logger.LogError(ex, "Error in ContractCreator");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }
    protected async Task DoWork()
    {
        var tasks = _db.Set<ElectronicSignatureTask>().Where(i => i.CurrentStep == TaskStep.ContractCreating).ToList();
        foreach (var task in tasks)
        {
            try
            {
                CreateContractModel createContractModel = new CreateContractModel { Task = task };

                createContractModel.Envelope = await _docuSign.GetEnvelopeAsync(task.DocuSignEnvelopeId);
                MatchTemplateMapping(createContractModel);
                var contract = await CreateContract(createContractModel);

                task.BestSignContractId = contract.ContractId;
                _taskService.ChangeStep(task.Id, TaskStep.ContractCreated);
            }
            catch (Exception ex)
            {
                _taskService.LogError(task.Id, ex.Message);
            }
            _db.SaveChanges();
        }
    }

    protected void MatchTemplateMapping(CreateContractModel createContractModel)
    {
        var envelopeType = createContractModel.Envelope
            .CustomFields.ListCustomFields.SingleOrDefault(i => i.Name == "eStamp Type");
        if (envelopeType == null) throw new Exception("System Error: Not found the custom field 'eStamp Type'.");

        var templateMapping = _db.Set<TemplateMapping>().SingleOrDefault(i => i.DocuSignTemplateId == envelopeType.Value);
        if (templateMapping == null) throw new Exception("System Error: Not found the template mapping in the system.");

        createContractModel.TemplateMapping = templateMapping;
    }

    protected async Task<CreateContractSuccessModel> CreateContract(CreateContractModel createContractModel)
    {
        var sender = CreateContractSender();
        var roles = CreateContractRoles();
        var documents = await CreateContractDocuments(createContractModel.Envelope.EnvelopeId, createContractModel.Envelope.EnvelopeDocuments);

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
    protected async Task<object> CreateContractDocuments(string envelopeId, List<EnvelopeDocument> documents)
    {
        List<Object> result = new();

        Dictionary<string, object>? mainContract = null;
        List<Object> attachments = new();
        foreach (var document in documents)
        {
            if (document.Name == "Summary") continue;

            var docFile = await _docuSign.DownloadDocument(envelopeId, document.DocumentId);
            var docContent = _documentService.DecryptDocument(docFile);

            var appendingSignLables = await AppendingSignLables(envelopeId, document.DocumentId);

            var item = new Dictionary<string, object>();
            if (mainContract is null && appendingSignLables is not null)
            {
                item.Add("documentId", "3291961092528299015");
                item.Add("fileName", $"{document.Name}.pdf");
                item.Add("contractConfig", new
                {
                    contractTitle = $"{document.Name}.pdf",
                });
                item.Add("appendingSignLables", appendingSignLables);
                item.Add("attachments", new object());
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
                item.Add("appendingSignLables", appendingSignLables);
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
        mainContract["attachments"] = attachments;
        return result;
    }

    protected async Task<List<Object>?> AppendingSignLables(string envelopeId, string documentId)
    {
        List<Object> result = new();

        var docTabs = await _docuSign.GetDocumentTabs(envelopeId, documentId);
        var aStamp = docTabs.SignHereTabs?.SingleOrDefault(i => i.TabLabel == "A Stamp Here");
        if (aStamp != null)
        {
            result.Add(new
            {
                x = GetXPosition(aStamp.XPosition),
                y = GetYPosition(aStamp.YPosition),
                pageNumber = int.Parse(aStamp.PageNumber),
                roleName = "员工",
                type = "SEAL",
            });
        }

        var bStamp = docTabs.SignHereTabs?.SingleOrDefault(i => i.TabLabel == "B Stamp Here");
        if (bStamp != null)
        {
            result.Add(new
            {
                x = GetXPosition(bStamp.XPosition),
                y = GetYPosition(bStamp.YPosition),
                pageNumber = int.Parse(bStamp.PageNumber),
                roleName = "员工",
                type = "SEAL",
            });
        }
        if (aStamp == null && bStamp == null)
            return null;
        return result;
    }
    protected double GetXPosition(string xPosition)
    {
        var x = double.Parse("0." + xPosition);
        return x;
    }

    protected double GetYPosition(string yPosition)
    {
        var y = double.Parse("0." + yPosition);
        return y;
    }
    #endregion

    protected object CreateContractSender()
    {
        var result = new
        {
            account = "13511031927",
            enterpriseName = "宜家贸易服务（中国）有限公司",
            bizName = "Operation",
        };

        return result;
    }

    protected object CreateContractRoles()
    {
        List<Object> result = new();
        result.Add(new
        {
            roleId = "3291961364587633673",
            userInfo = new
            {
                userAccount = "13511031927",
                enterpriseName = "宜家贸易服务（中国）有限公司",
            },
        });
        // result.Add(new
        // {
        //     roleId = "3277656591180727299",
        //     userInfo = new
        //     {
        //         userAccount = "13511031927",
        //         enterpriseName = "宜家贸易（中国）有限公司",
        //     },
        // });

        return result;
    }


}

public class CreateContractModel
{
    public ElectronicSignatureTask Task { get; set; } = null!;
    public TemplateMapping? TemplateMapping { get; set; }
    public Envelope Envelope { get; set; } = null!;
}

public class CreateContractSuccessModel
{
    public string ContractId { get; set; } = null!;
}