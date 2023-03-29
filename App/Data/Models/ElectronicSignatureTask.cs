namespace Data.Models;
using Data.Enums;

public class ElectronicSignatureTask : EntityBase
{
    public string DocuSignEnvelopeId { get; set; } = string.Empty;
    public string? BestSignContractId { get; set; }
    public TaskStep CurrentStep { get; set; } = Enums.TaskStep.Unknown;
    public int Counter { get; set; }
}