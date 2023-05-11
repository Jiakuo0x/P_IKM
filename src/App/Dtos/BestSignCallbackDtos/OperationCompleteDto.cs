namespace Dtos.BestSignCallbackDto;

/// <summary>
/// The data transfer object of operation completion
/// </summary>
public class OperationCompleteDto
{
    /// <summary>
    /// The contract id in Bestsign
    /// </summary>
    public string ContractId { get; set; } = null!;

    /// <summary>
    /// The role name of operator
    /// </summary>
    public string RoleName { get; set; } = null!;

    /// <summary>
    /// The user account of operator
    /// </summary>
    public string UserAccount { get; set; } = null!;

    /// <summary>
    /// The enterprise name of operator
    /// </summary>
    public string EnterpriseName { get; set; } = null!;

    /// <summary>
    /// The user type of operator
    /// </summary>
    public string UserType { get; set; } = null!;

    /// <summary>
    /// The operation type
    /// </summary>
    public string OperationType { get; set; } = null!;

    /// <summary>
    /// The operation status
    /// </summary>
    public string? OperationStatus {  get; set; }

    /// <summary>
    /// The sign type of the operation
    /// </summary>
    public string SignType { get; set; } = null!;

    /// <summary>
    /// The message of the operation
    /// </summary>
    public string Message { get; set; } = null!;

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