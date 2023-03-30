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

                    task.BestSignContractId = contract.ContractId;
                    TaskStatusChange(db, task, TaskStep.ContractCreated);
                }
                catch (Exception ex)
                {
                    LogError(db, task, ex.Message);
                }
                db.SaveChanges();
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

        protected void LogError(DbContext db, ElectronicSignatureTask task, string message)
        {
            db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
            {
                TaskId = task.Id,
                Step = task.CurrentStep,
                Log = $"[Error] {message}"
            });
            task.Counter++;

            if (task.Counter >= 5)
            {
                TaskStatusChange(db, task, TaskStep.ContractCreatingFailed);
            }
        }

        protected void TaskStatusChange(DbContext db, ElectronicSignatureTask task, TaskStep to)
        {
            db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
            {
                TaskId = task.Id,
                Step = task.CurrentStep,
                Log = $"[Task Step Change] {task.CurrentStep} -> {to}",
            });
            task.CurrentStep = to;
            task.Counter = 0;
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
