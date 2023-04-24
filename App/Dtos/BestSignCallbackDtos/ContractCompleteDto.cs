namespace Dtos.BestSignCallbackDto;

/// <summary>
/// The data transfer object of contract completion
/// </summary>
public class ContractCompleteDto
{
    /// <summary>
    /// The contract ids in Bestsign
    /// </summary>
    public string[] ContractIds { get; set; } = null!;

    /// <summary>
    /// The user account of contract sender
    /// </summary>
    public string SenderUserAccount { get; set; } = null!;

    /// <summary>
    /// The business line of contract sender
    /// </summary>
    public string SenderBusinessLine { get; set; } = null!;

    /// <summary>
    /// The enterprise name of contract sender
    /// </summary>
    public string SenderEnterpriseName { get; set; } = null!;
}