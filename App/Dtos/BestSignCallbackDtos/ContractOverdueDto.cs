namespace Dtos.BestSignCallbackDto;

/// <summary>
/// The data transfer object of contract overdue
/// </summary>
public class ContractOverdueDto
{
    /// <summary>
    /// The contract id in Bestsign
    /// </summary>
    public string ContractId { get; set; } = null!;

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