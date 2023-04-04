namespace Database.Enums;

public enum TaskStep
{
    Unknown,

    ContractCreating,
    ContractCreated,
    ContractOverdue,

    ContractCancelling,
    ContractCancelled,

    Completed,

    Failed,
}