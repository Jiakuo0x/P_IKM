namespace Dtos.BestSignCallbackDto;

public class ContractSendResultDto
{
    public string ContractId { get; set; } = null!;
    public string SenderUserAccount { get; set; } = null!;
    public string SenderBusinessLine { get; set; } = null!;
    public string SenderEnterpriseName { get; set; } = null!;
}