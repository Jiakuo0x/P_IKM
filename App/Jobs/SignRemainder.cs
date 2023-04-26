using Database.Enums;
using Services;

namespace Jobs;

/// <summary>
/// The job is for reminding signers who need to sign the contract in Bestsign
/// </summary>
public class SignRemainder : BackgroundService
{
    private readonly ILogger<SignRemainder> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private TaskService _taskService = null!;
    private BestSignService _bestSign = null!;

    public SignRemainder(
        ILogger<SignRemainder> logger,
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
                _taskService = scope.ServiceProvider.GetRequiredService<TaskService>();
                _bestSign = scope.ServiceProvider.GetRequiredService<BestSignService>();
                try
                {
                    await DoWork();
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Error in SignRemainder");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromDays(1));
                }
            }
        }
    }

    protected async Task DoWork()
    {
        var tasks = _taskService.GetTasksByStep(TaskStep.ContractCancelled);
        foreach (var task in tasks)
        {
            try
            {
                // If the contract has not been created in Bestsign, then skip this task
                if (task.BestSignContractId == null)
                    continue;

                // If the time elapsed since the last update of the task is within 15 days of the current time, then skip this task
                if (task.LastUpdated.AddDays(15) < DateTime.Now)
                    continue;

                // Call the Bestsign API to send a signing reminder
                await _bestSign.Post<object>("/api/contracts/remind", new
                {
                    contractId = task.BestSignContractId,
                });
            }
            catch(Exception ex)
            {
                _taskService.LogInfo(task.Id, ex.Message);
            }
        }
    }
}