namespace Dtos.BestSignCallbackDto;

public class ContractCompleteDto
{
    public string[] ContractIds { get; set; } = null!;
    public string SenderUserAccount { get; set; } = null!;
    public string SenderBusinessLine { get; set; } = null!;
    public string SenderEnterpriseName { get; set; } = null!;
}