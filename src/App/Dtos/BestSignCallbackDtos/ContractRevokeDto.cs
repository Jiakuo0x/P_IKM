namespace App.Dtos.BestSignCallbackDtos;

public class ContractRevokeDto
{
    /// <summary>
    /// The contract id in Bestsign
    /// </summary>
    public string ContractId { get; set; } = null!;

    /// <summary>
    /// The revoke reason of the contract
    /// </summary>
    public string? RevokeReason { get; set; }

    /// <summary>
    /// The user account of operator
    /// </summary>
    public string UserAccount { get; set; } = null!;

    /// <summary>
    /// The enterprise name of operator
    /// </summary>
    public string EnterpriseName { get; set; } = null!;

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
