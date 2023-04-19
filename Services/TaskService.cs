﻿using Database.Enums;

namespace Services;

public class TaskService
{
    private readonly DbContext _db;
    public TaskService(DbContext db)
    {
        _db = db;
    }
    public void CreateTask(string envelopeId)
    {
        ElectronicSignatureTask task = new ElectronicSignatureTask();
        task.CurrentStep = TaskStep.ContractCreating;
        task.Counter = 0;
        task.DocuSignEnvelopeId = envelopeId;
        _db.Add(task);
        _db.SaveChanges();
    }

    public Dictionary<string, ElectronicSignatureTask> GetAllTasksAsDic()
    {
        var dic = _db.Set<ElectronicSignatureTask>()
            .AsNoTracking()
            .ToDictionary(i => i.DocuSignEnvelopeId);

        return dic;
    }

    public List<ElectronicSignatureTask> GetTasksByStep(TaskStep step)
    {
        var tasks = _db.Set<ElectronicSignatureTask>()
            .Where(i => i.CurrentStep == step)
            .AsNoTracking()
            .ToList();

        return tasks;
    }

    public ElectronicSignatureTask? GetTaskByBestSignContractId(string contractId)
    {
        var task = _db.Set<ElectronicSignatureTask>()
            .AsNoTracking()
            .SingleOrDefault(i => i.BestSignContractId == contractId);

        return task;
    }

    public void UpdateTaskContractId(int taskId, string contractId)
    {
        var task = _db.Set<ElectronicSignatureTask>().Single(i => i.Id == taskId);
        task.BestSignContractId = contractId;
        _db.SaveChanges();
    }

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
