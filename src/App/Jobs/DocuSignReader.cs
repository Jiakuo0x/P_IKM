using Database.Models;
using Database.Enums;
using Services;

namespace Jobs;

/// <summary>
/// The job is for reading envelopes in DocuSign and detecting any that need to be processed in the middleware system.
/// </summary>
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
            // Create a dependency injection lifecycle
            using (var scope = _scopeFactory.CreateScope())
            {
                // Retrieve relevant objects from the dependency injection container
                _docuSignService = scope.ServiceProvider.GetRequiredService<DocuSignService>();
                _taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
                try
                {
                    await DoWork();
                }
                catch 
                {
                    // _logger.LogError(ex, "Error in DocuSignReader");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }
    }

    protected async Task DoWork()
    {
        var envelopes = await _docuSignService.MatchEnvelopes();

        // Cache the tasks from the database
        var envelopesDic = _taskService.GetAllTasksAsDic();

        foreach (var envelope in envelopes.Envelopes)
        {
            // If the envelope is not in a pending state, the skip this envelope
            if (_docuSignService.EnvelopeIsPending(envelope) is false) continue;

            var listener = _docuSignService.GetListenerInfo(envelope);
            // If listener role name is "WetInk", then delete the listener and remove all the recipients after the listener
            if (listener.RoleName == "WetInk")
            {
                try
                {
                    var editors = envelope.Recipients.Editors.Where(i => int.Parse(i.RoutingOrder) >= int.Parse(listener.RoutingOrder)).ToList();
                    var signers = envelope.Recipients.Signers.Where(i => int.Parse(i.RoutingOrder) >= int.Parse(listener.RoutingOrder)).ToList();
                    await _docuSignService.RemoveRecipients(envelope.EnvelopeId, signers, editors);
                }
                catch (Exception ex)
                {
                    _taskService.LogError(envelopesDic[envelope.EnvelopeId].Id, ex.Message);
                }
            }
            else
            {
                try
                {
                    // If the envelope status is "voided", first check if there are any related tasks in the database.
                    // Then, check if the current status of the task needs to be processed.
                    // Finally, change the task status, assign it to other tasks for processing, and record the status in the custom field of the envelope
                    if (envelope.Status == "voided")
                    {
                        if (envelopesDic.ContainsKey(envelope.EnvelopeId) is false) continue;

                        var taskStatus = envelopesDic[envelope.EnvelopeId].CurrentStep;
                        if (taskStatus is TaskStep.Unknown) continue;
                        if (taskStatus is TaskStep.ContractCancelling) continue;
                        if (taskStatus is TaskStep.ContractCancelled) continue;
                        if (taskStatus is TaskStep.ContractCancelled) continue;
                        if (taskStatus is TaskStep.Failed) continue;
                        if (taskStatus is TaskStep.Completed) continue;

                        _taskService.ChangeStep(envelopesDic[envelope.EnvelopeId].Id, TaskStep.ContractCancelling);
                        await _docuSignService.UpdateComment(envelope.EnvelopeId, "Waiting to revoke the contract.");
                    }
                    // Check if the envelope needs to be processed. If so, create a task in the database and record the status in the custom field of the envelope
                    else
                    {
                        if (envelopesDic.ContainsKey(envelope.EnvelopeId)) continue;
                        _taskService.CreateTask(envelope.EnvelopeId);
                        await _docuSignService.UpdateComment(envelope.EnvelopeId, "Waiting to create the contract.");
                    }
                }
                catch (Exception ex)
                {
                    if (envelopesDic.ContainsKey(envelope.EnvelopeId))
                    {
                        _taskService.LogInfo(envelopesDic[envelope.EnvelopeId].Id, ex.Message);
                    }
                }
            }
        }
    }
}