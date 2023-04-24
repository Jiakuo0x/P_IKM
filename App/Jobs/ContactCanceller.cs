using DocuSign.eSign.Model;
using Database.Models;
using Database.Enums;
using Lib.BestSign.Dtos;
using Services;

namespace Jobs;

public class ContactCanceller : BackgroundService
{
    private readonly ILogger<DocuSignReader> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private TaskService _taskService = null!;
    private BestSignService _bestSign = null!;
    public ContactCanceller(
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
            using (var scope = _scopeFactory.CreateScope())
            {
                _taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
                _bestSign = scope.ServiceProvider.GetRequiredService<BestSignService>();
                try
                {
                    await DoWork();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ContactCanceller");
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
        var tasks = _taskService.GetTasksByStep(TaskStep.ContractCancelling);

        foreach (var task in tasks)
        {
            try
            {
                if(task.BestSignContractId == null || string.IsNullOrEmpty(task.BestSignContractId))
                    _taskService.ChangeStep(task.Id, TaskStep.ContractCancelled);

                var apiResponse = await _bestSign.Post<object>($"/api/contracts/{task.BestSignContractId}/revoke", new
                {
                    revokeReason = "The system has cancelled the contract because the relevant envelope of DocuSign has been cancelled",
                });
                _taskService.ChangeStep(task.Id, TaskStep.ContractCancelled);
                _taskService.ChangeStep(task.Id, TaskStep.Completed);
            }
            catch (Exception ex)
            {
                _taskService.LogError(task.Id, ex.Message);
            }
        }
    }
}
