using Database.Enums;

namespace Database.Models;

/// <summary>
/// The task of electronic signature
/// </summary>
public class ElectronicSignatureTask : EntityBase
{
    /// <summary>
    /// DocuSign envelope ID
    /// </summary>
    public string DocuSignEnvelopeId { get; set; } = string.Empty;

    /// <summary>
    /// Bestsign contract ID.
    /// When it is empty, it indicates that Bestsign has not yet completed the creation of the contract
    /// </summary>
    public string? BestSignContractId { get; set; }

    /// <summary>
    /// Current task step
    /// </summary>
    public TaskStep CurrentStep { get; set; } = Enums.TaskStep.Unknown;

    /// <summary>
    /// Accumulated error count for the current step.
    /// After reaching 5 errors, the step changes to Failed.
    /// The count is reset after the step changes.
    /// </summary>
    public int Counter { get; set; }
}