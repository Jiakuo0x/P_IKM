using Database.Models;
using Database.Enums;
using Services;

namespace Jobs;

public class DocuSignReader : BackgroundService
{
    private readonly ILogger<DocuSignReader> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private DocuSignService _docuSignService = null!;
    private TaskService _taskService = null!;
    public DocuSignReader(
        ILogger<DocuSignReader> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            using(var scope = _scopeFactory.CreateScope())
            {
                _docuSignService = scope.ServiceProvider.GetRequiredService<DocuSignService>();
                _taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
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
    }

    protected async Task DoWork()
    {
        var envelopes = await _docuSignService.MatchEnvelopes();
        var envelopesDic = _taskService.GetAllTasksAsDic();

        foreach (var envelope in envelopes.Envelopes)
        {
            if (_docuSignService.EnvelopeIsPending(envelope) is false) continue;

            if (envelope.Status == "voided")
            {
                if(envelopesDic.ContainsKey(envelope.EnvelopeId) is false) continue;

                var taskStatus = envelopesDic[envelope.EnvelopeId].CurrentStep;
                if (taskStatus is TaskStep.ContractCancelling) continue;
                if (taskStatus is TaskStep.ContractCancelled) continue;

                await _docuSignService.UpdateComment(envelope.EnvelopeId, "The envelope has been voided in DocuSign. Please wait for the middleware to revoke the relevant contract in Bestsign.");
                _taskService.ChangeStep(envelopesDic[envelope.EnvelopeId].Id, TaskStep.ContractCancelling);
            }
            else
            {
                if(envelopesDic.ContainsKey(envelope.EnvelopeId)) continue;
                await _docuSignService.UpdateComment(envelope.EnvelopeId, "The envelope has been detected by IKEA middleware. Please wait for the middleware to create a contract in Bestsign.");
                _taskService.CreateTask(envelope.EnvelopeId);
            }
        }
    }
}