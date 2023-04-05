using Database.Models;
using Database.Enums;
using Services;

namespace Jobs;

public class DocuSignReader : BackgroundService
{
    private readonly ILogger<DocuSignReader> _logger;
    private readonly DocuSignService _docuSignService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptions<Lib.DocuSign.Configuration> _docuSignOptions;
    public DocuSignReader(
        ILogger<DocuSignReader> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<Lib.DocuSign.Configuration> docuSignOptions)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _docuSignOptions = docuSignOptions;

        var provider = _serviceScopeFactory.CreateScope().ServiceProvider;
        _docuSignService = provider.GetRequiredService<DocuSignService>();
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
                _logger.LogError(ex, "Error in DocuSignReader");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }

    protected async Task DoWork()
    {
        var envelopes = await _docuSignService.MatchEnvelopes();
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbContext>();
        var envelopesDic = dbContext.Set<ElectronicSignatureTask>()
            .Select(i => new { i.DocuSignEnvelopeId, i.CurrentStep })
            .ToDictionary(i => i.DocuSignEnvelopeId);

        foreach (var envelope in envelopes.Envelopes)
        {
            if (EnvelopeIsPending(envelope) is false) continue;

            if (envelope.Status == "voided")
            {
                if(envelopesDic.ContainsKey(envelope.EnvelopeId) is false) continue;

                var taskStatus = envelopesDic[envelope.EnvelopeId].CurrentStep;
                if (taskStatus is TaskStep.ContractCancelling) continue;
                if (taskStatus is TaskStep.ContractCancelled) continue;

                StartCancelBestsignContract(envelope);
            }
            else
            {
                if(envelopesDic.ContainsKey(envelope.EnvelopeId)) continue;
                StartCreateBestsignContract(envelope);
            }
        }
    }

    protected void StartCreateBestsignContract(Envelope envelope)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbContext>();
        ElectronicSignatureTask task = new ElectronicSignatureTask();
        task.CurrentStep = TaskStep.ContractCreating;
        task.Counter = 0;
        task.DocuSignEnvelopeId = envelope.EnvelopeId;
        dbContext.Add(task);
        dbContext.SaveChanges();
    }

    protected void StartCancelBestsignContract(Envelope envelope)
    {
        var dbContext = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbContext>();
        var task = dbContext.Set<ElectronicSignatureTask>().Single(i => i.DocuSignEnvelopeId == envelope.EnvelopeId);
        task.CurrentStep = TaskStep.ContractCancelling;
        task.Counter = 0;
        dbContext.SaveChanges();
    }

    protected bool EnvelopeIsPending(Envelope envelope)
    {
        var recipients = envelope.Recipients;
        if (recipients is null) return false;

        var listener = recipients.Signers.SingleOrDefault(i => i.Email == _docuSignOptions.Value.ListenEmail && i.RoutingOrder == recipients.CurrentRoutingOrder);
        if (listener is null) return false;

        return true;
    }
}