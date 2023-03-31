namespace Database.Enums;

public enum TaskStep
{
    Unknown,

    ContractCreating,
    ContractCreated,

    ContractCancelling,
    ContractCancelled,

    Completed,

    Failed,
}