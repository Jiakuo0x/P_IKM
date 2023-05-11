using Database.Enums;

namespace Database.Models;

/// <summary>
/// The task log of eletronic signature
/// </summary>
public class ElectronicSignatureTaskLog : EntityBase<Guid>
{
    /// <summary>
    /// The task ID of electronic signature
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Log corresponding to the task step
    /// </summary>
    public TaskStep Step { get; set; } = TaskStep.Unknown;

    /// <summary>
    /// Log content
    /// </summary>
    public string Log { get; set; } = string.Empty;
}