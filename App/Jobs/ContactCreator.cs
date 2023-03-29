using Data.Enums;
using Data.Models;
using Lib.DocuSign;

namespace App.Jobs
{
    public class ContactCreator : BackgroundService
    {
        private readonly ILogger<DocuSignReader> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<Lib.DocuSign.Configuration> _docuSignOptions;
        public ContactCreator(
            ILogger<DocuSignReader> logger,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<Lib.DocuSign.Configuration> docuSignOptions)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _docuSignOptions = docuSignOptions;
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
            var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbContext>();
            var tasks = db.Set<ElectronicSignatureTask>().Where(i => i.CurrentStep == TaskStep.ContractCreating).ToList();
            foreach (var task in tasks)
            {
                try
                {
                    CreateContractModel createContractModel = new CreateContractModel { Task = task };

                    MatchEnvelope(createContractModel);
                    MatchTemplateMapping(createContractModel);
                    var contract = await CreateContract(createContractModel);
                    
                    db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
                    {
                        TaskId = task.Id,
                        Step = task.CurrentStep,
                        Log = $"[Task Step Change] {task.CurrentStep} -> {TaskStep.ContractCreated}",
                    });
                    task.BestSignContractId = contract.ContractId;
                    task.CurrentStep = TaskStep.ContractCreated;
                    task.Counter = 0;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Create contract failed. Envelope ID: {task.DocuSignEnvelopeId}");
                    db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
                    {
                        TaskId = task.Id,
                        Step = task.CurrentStep,
                        Log = ex.Message,
                    });
                    task.Counter++;

                    if (task.Counter >= 5)
                    {
                        db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
                        {
                            TaskId = task.Id,
                            Step = task.CurrentStep,
                            Log = $"[Task Step Change] {task.CurrentStep} -> {TaskStep.ContractCreatingFailed}",
                        });
                        task.CurrentStep = TaskStep.ContractCreatingFailed;
                        task.Counter = 0;
                    }
                    db.SaveChanges();
                }
            }
        }

        protected void MatchEnvelope(CreateContractModel createContractModel)
        {
            var docusignService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DocuSignService>();
            var envelope = docusignService.GetEnvelopeAsync(createContractModel.Task.DocuSignEnvelopeId).GetAwaiter().GetResult();
            createContractModel.Envelope = envelope;
        }
        protected void MatchTemplateMapping(CreateContractModel createContractModel)
        {
            var db = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbContext>();
            var envelopeType = createContractModel.Envelope
                .CustomFields.TextCustomFields.SingleOrDefault(i => i.Name == "Envelope Type");
            if (envelopeType == null) throw new Exception("System Error: Not found the custom field 'Envelope Type'.");

            var templateMapping = db.Set<TemplateMapping>().SingleOrDefault(i => i.DocuSignTemplateId == envelopeType.Value);
            if (templateMapping == null) throw new Exception("System Error: Not found the template mapping in the system.");

            createContractModel.TemplateMapping = templateMapping;
        }

        protected async Task<CreateContractSuccessModel> CreateContract(CreateContractModel createContractModel)
        {

            var bestSignApiClient = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<Lib.BestSign.ApiClient>();
            var apiResponse = await bestSignApiClient.Post<CreateContractSuccessModel>($"/api/templates/send-contracts-sync-v2", new
            {
                templateId = createContractModel.TemplateMapping!.BestSignTemplateId,
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
}
