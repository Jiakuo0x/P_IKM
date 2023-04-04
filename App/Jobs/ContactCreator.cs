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
            .CustomFields.TextCustomFields.SingleOrDefault(i => i.Name == "Envelope Type");
        if (envelopeType == null) throw new Exception("System Error: Not found the custom field 'Envelope Type'.");

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
            documents = documents,
            roles = roles,
        });

        return apiResponse;

    }

    protected async Task<object> CreateContractDocuments(string envelopeId ,List<EnvelopeDocument> documents)
    {
        List<Object> result = new();
        foreach (var document in documents)
        {
            if (document.Name == "Summary") continue;

            var docFile = await _docuSign.DownloadDocument(envelopeId, document.DocumentId);
            var docContent = _documentService.DecryptDocument(docFile);
            result.Add(new
            {
                content = docContent,
                fileName = document.Name,
                contractConfig = new
                {
                    contractTitle = "Test Title",
                },
                appendingSignLables = new List<Object>
                {
                    new
                    {
                        x = 0.2,
                        y = 0.2,
                        pageNumber = 1,
                        roleName = "Enterprise",
                        type = "SEAL",
                    },
                    new
                    {
                        x = 0.2,
                        y = 0.8,
                        pageNumber = 1,
                        roleName = "Customer",
                        type = "SIGNATURE",
                    },
                }
            });
        }
        return result;
    }

    protected object CreateContractSender()
    {
        var result = new
        { 
            account = "13511031927",
            enterpriseName = "宜家贸易（中国）有限公司",
            bizName = "宜家贸易（中国）有限公司_DocuSign签核",
        };

        return result;
    }

    protected object CreateContractRoles()
    {
        List<Object> result = new();
        result.Add(new
        {
            roleId = "3277655698381177863",
            userInfo = new
            {
                userAccount = "13511031927",
                enterpriseName = "宜家贸易（中国）有限公司",
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