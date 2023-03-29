using Data.Enums;

namespace Data.Models;

public class ElectronicSignatureTaskLog : EntityBase<Guid>
{
    public int TaskId { get; set; }
    public TaskStep Step { get; set; } = TaskStep.Unknown;
    public string Log { get; set; } = string.Empty;
}