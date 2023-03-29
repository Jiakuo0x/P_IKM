namespace Data.Enums;

public enum TaskStep
{
    Unknown,

    ContractCreating,
    ContractCreated,
    ContractCreatingFailed,

    ContractCancelling,
    ContractCancelled,
    ContractCancellingFailed,

    ContractCompleted,
}