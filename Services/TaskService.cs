using Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class TaskService
{
    private readonly DbContext _db;
    public TaskService(DbContext db)
    {
        _db = db;
    }

    public List<ElectronicSignatureTask> GetTasksByStep(TaskStep step)
    {
        var tasks = _db.Set<ElectronicSignatureTask>()
            .Where(i => i.CurrentStep == TaskStep.ContractCancelling)
            .AsNoTracking().ToList();
        return tasks;
    }

    public ElectronicSignatureTask? GetTaskByBestSignContractId(string contractId)
    {
        var task = _db.Set<ElectronicSignatureTask>().AsNoTracking().SingleOrDefault(i => i.BestSignContractId == contractId);
        return task;
    }

    public void LogInfo(int taskId, string info)
    {
        var task = _db.Set<ElectronicSignatureTask>().AsNoTracking().Single(i => i.Id == taskId);

        _db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
        {
            TaskId = task.Id,
            Step = task.CurrentStep,
            Log = $"[Info] {info}",
        });
        _db.SaveChanges();
    }

    public void LogError(int taskId, string error, int maxErrorCount = 5)
    {
        var task = _db.Set<ElectronicSignatureTask>().Single(i => i.Id == taskId);

        _db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
        {
            TaskId = task.Id,
            Step = task.CurrentStep,
            Log = $"[Error] {error}",
        });
        task.Counter++;
        _db.SaveChanges();

        if (task.Counter >= 5)
        {
            ChangeStep(taskId, TaskStep.Failed);
        }
    }

    public void ChangeStep(int taskId, TaskStep newStep)
    {
        var task = _db.Set<ElectronicSignatureTask>().Single(i => i.Id == taskId);

        _db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
        {
            TaskId = task.Id,
            Step = task.CurrentStep,
            Log = $"[Task Step Change] {task.CurrentStep} -> {newStep}",
        });
        task.CurrentStep = newStep;
        task.Counter = 0;

        _db.SaveChanges();
    }
}
