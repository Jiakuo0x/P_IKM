using DocuSign.eSign.Model;
using Database.Models;
using Database.Enums;
using Lib.BestSign.Dtos;
using Services;

namespace Jobs;

public class ContactCanceller : BackgroundService
{
    private readonly ILogger<DocuSignReader> _logger;
    private readonly TaskService _taskService;
    private readonly BestSignService _bestSign;
    public ContactCanceller(
        ILogger<DocuSignReader> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;

        var provider = serviceScopeFactory.CreateScope().ServiceProvider;
        _taskService = provider.GetRequiredService<TaskService>();
        _bestSign = provider.GetRequiredService<BestSignService>();
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
                _logger.LogError(ex, "Error in ContactCanceller");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
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
                var apiResponse = await _bestSign.Post<object>($"/api/contracts/{task.BestSignContractId}/revoke", new
                {
                    revokeReason = "The system has cancelled the contract because the relevant envelope of DocuSign has been cancelled",
                });
                _taskService.ChangeStep(task.Id, TaskStep.ContractCancelled);
            }
            catch (Exception ex)
            {
                _taskService.LogError(task.Id, ex.Message);
            }
        }
    }
}
