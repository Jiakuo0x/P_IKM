using Database.Enums;

namespace Services;

/// <summary>
/// Task service
/// </summary>
public class TaskService
{
    private readonly DbContext _db;
    public TaskService(DbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Create a task.
    /// This means that the system has identified an envelope that needs to be processed.
    /// </summary>
    /// <param name="envelopeId">Envelope ID</param>
    public void CreateTask(string envelopeId)
    {
        ElectronicSignatureTask task = new ElectronicSignatureTask();
        task.CurrentStep = TaskStep.ContractCreating;
        task.Counter = 0;
        task.DocuSignEnvelopeId = envelopeId;
        _db.Add(task);
        _db.SaveChanges();
    }

    /// <summary>
    /// Retrieve all tasks and convert the results to a dictionary.
    /// </summary>
    /// <returns>Tasks dictionary</returns>
    public Dictionary<string, ElectronicSignatureTask> GetAllTasksAsDic()
    {
        var dic = _db.Set<ElectronicSignatureTask>()
            .AsNoTracking()
            .ToDictionary(i => i.DocuSignEnvelopeId);

        return dic;
    }

    /// <summary>
    /// Retrieve the task list based on the task step.
    /// </summary>
    /// <param name="step">Task step</param>
    /// <returns>Task list</returns>
    public List<ElectronicSignatureTask> GetTasksByStep(TaskStep step)
    {
        var tasks = _db.Set<ElectronicSignatureTask>()
            .Where(i => i.CurrentStep == step)
            .AsNoTracking()
            .ToList();

        return tasks;
    }

    /// <summary>
    /// Retrieve the task list based on the Bestsign contract ID
    /// </summary>
    /// <param name="contractId">Bestsign contract ID</param>
    /// <returns>Task list</returns>
    public ElectronicSignatureTask? GetTaskByBestSignContractId(string contractId)
    {
        var task = _db.Set<ElectronicSignatureTask>()
            .AsNoTracking()
            .SingleOrDefault(i => i.BestSignContractId == contractId);

        return task;
    }

    /// <summary>
    /// Update the Bestsign contract ID associated with the task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="contractId">Bestsign contract ID</param>
    public void UpdateTaskContractId(int taskId, string contractId)
    {
        var task = _db.Set<ElectronicSignatureTask>().Single(i => i.Id == taskId);
        task.BestSignContractId = contractId;
        _db.SaveChanges();
    }

    /// <summary>
    /// Record a general log into the database
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="info">Log information</param>
    public void LogInfo(int taskId, string info)
    {
        var task = _db.Set<ElectronicSignatureTask>()
            .AsNoTracking()
            .Select(i => new { i.Id, i.CurrentStep })
            .Single(i => i.Id == taskId);

        _db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
        {
            TaskId = task.Id,
            Step = task.CurrentStep,
            Log = $"[Info] {info}",
        });
        _db.SaveChanges();
    }

    /// <summary>
    /// Record an error log into the database.
    /// If the error count exceeds the maximum allowed count, then change the status of the step to "Failed"
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="error">Error information</param>
    /// <param name="maxErrorCount">Maximum allowed error count</param>
    public void LogError(int taskId, string error, int maxErrorCount = 5)
    {
        var task = _db.Set<ElectronicSignatureTask>()
            .Single(i => i.Id == taskId);

        _db.Set<ElectronicSignatureTaskLog>().Add(new ElectronicSignatureTaskLog
        {
            TaskId = task.Id,
            Step = task.CurrentStep,
            Log = $"[Error] {error}",
        });
        task.Counter++;
        _db.SaveChanges();

        if (task.Counter >= maxErrorCount)
        {
            this.ChangeStep(taskId, TaskStep.Failed);
        }
    }

    /// <summary>
    /// Change the step of the task and reset the error count
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="newStep">New step</param>
    public void ChangeStep(int taskId, TaskStep newStep)
    {
        var task = _db.Set<ElectronicSignatureTask>()
            .Single(i => i.Id == taskId);

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

    /// <summary>
    /// Get task logs
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <returns>Task logs</returns>
    public List<ElectronicSignatureTaskLog> GetTaskLogs(int taskId)
    {
        var logs = _db.Set<ElectronicSignatureTaskLog>()
            .Where(i => i.TaskId == taskId)
            .OrderByDescending(i => i.Created)
            .AsNoTracking()
            .ToList();
        return logs;
    }
}
