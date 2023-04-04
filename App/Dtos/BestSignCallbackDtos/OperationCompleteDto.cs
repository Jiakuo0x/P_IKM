namespace Dtos.BestSignCallbackDto;

public class OperationCompleteDto
{
    public string ContractId { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public string UserAccount { get; set; } = null!;
    public string EnterpriseName { get; set; } = null!;
    public string UserType { get; set; } = null!;
    public string OperationType { get; set; } = null!;
    public string SignType { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string SenderUserAccount { get; set; } = null!;
    public string SenderBusinessLine { get; set; } = null!;
    public string SenderEnterpriseName { get; set; } = null!;
}