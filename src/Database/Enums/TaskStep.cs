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
    /// When the envelope has been parsed and is waiting for the creation of a Bsetsign Contract, the status is ContractCreating.
    /// </summary>
    ContractCreating,

    /// <summary>
    /// When the contract has been created in Bestsign and is waiting for internal processing within the Bestsign system, the status is ContractCreated.
    /// </summary>
    ContractCreated,

    /// <summary>
    /// When an envelope is voided within DocuSign System, the status becomes ContractCancelling.
    /// And the system waits for cancellation of the corresponding contract within the Bestsign system.
    /// </summary>
    ContractCancelling,

    /// <summary>
    /// When the status is ContractCancelling and the corresponding contract in Bestsign system has been cancelled, the status changes to ContractCancelled.
    /// </summary>
    ContractCancelled,

    /// <summary>
    /// When the contract is signed and completed in Bestsign system, the status is ContractComplete.
    /// </summary>
    ContractCompleted,

    /// <summary>
    /// When a step in the system encounters five consecutive errors during runtime, the status becomes Failed.
    /// And the system waits to send email notifications to relevant personnel.
    /// </summary>
    Failed,

    /// <summary>
    /// When the task has ended, the status is Completed. This status indicates the final state.
    /// </summary>
    Completed,
}