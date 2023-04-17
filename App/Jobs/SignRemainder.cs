using Database.Enums;
using Services;

namespace Jobs;

public class SignRemainder : BackgroundService
{
    private readonly ILogger<SignRemainder> _logger;
    private readonly TaskService _taskService;
    private readonly BestSignService _bestSign;

    public SignRemainder(
        ILogger<SignRemainder> logger,
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
                _logger.LogError(ex, "Error in DocuSignReader");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromDays(1));
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
                if (task.BestSignContractId == null)
                    continue;
                if (task.LastUpdated.AddDays(15) < DateTime.Now)
                    continue;

                await _bestSign.Post<object>("/api/contracts/remind", new
                {
                    contractId = task.BestSignContractId,
                });
            }
            catch(Exception ex)
            {
                _taskService.LogError(task.Id, ex.Message);
            }
        }
    }
}