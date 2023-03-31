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
        List<Object> requestDocuments = new();
        foreach (var document in createContractModel.Envelope.EnvelopeDocuments)
        {
            if (document.Name == "Summary") continue;
            var docFile = await _docuSign.DownloadDocument(createContractModel.Envelope.EnvelopeId, document.DocumentId);
            var docContent = _documentService.DecryptDocument(docFile);
            requestDocuments.Add(new
            {
                content = docContent,
                fileName = document.Name,
            });
        }

        var apiResponse = await _bestSign.Post<CreateContractSuccessModel>($"/api/templates/send-contracts-sync-v2", new
        {
            templateId = createContractModel.TemplateMapping!.BestSignTemplateId,
            documents = requestDocuments,
        });

        return apiResponse;
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
}
