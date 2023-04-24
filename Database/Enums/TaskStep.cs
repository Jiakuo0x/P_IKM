namespace Database.Enums;

/// <summary>
/// Task step in the middleware
/// </summary>
public enum TaskStep
{
    /// <summary>
    /// Unknown for exception
    /// </summary>
    Unknown,

    /// <summary>
    /// Contract creating when the envelope has been analyse, and waiting for create a contract to Bestsign
    /// </summary>
    ContractCreating,
    ContractCreated,

    ContractCancelling,
    ContractCancelled,

    ContractCompleted,
    Failed,

    Completed,
}