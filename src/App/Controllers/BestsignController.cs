using App.Dtos;
using App.Dtos.BestSignCallbackDtos;
using Database.Enums;
using Dtos.BestSignCallbackDto;
using Services;

namespace Controllers;

/// <summary>
/// Bestsign API Controller
/// </summary>
[ApiController]
[Route("api/bestsign")]
public class BestsignController : ControllerBase
{
    private readonly ILogger<BestsignController> _logger;
    public BestsignController(ILogger<BestsignController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Listen to the API callback from Bestsign
    /// </summary>
    /// <param name="taskService">The task service from dependency injection</param>
    /// <param name="docuSignService">The DocuSign service from dependency injection</param>
    /// <param name="dto">The data transfer object of Bestsign callback</param>
    /// <returns></returns>
    [HttpPost]
    [Route("listen")]
    public async Task<object> Listen([FromServices] TaskService taskService, [FromServices] DocuSignService docuSignService,
        [FromBody] BestSignCallbackDto dto)
    {
        // Record the callback log
        _logger.LogInformation($"[BestSign] [Callback] {JsonConvert.SerializeObject(dto)} - [Body] {dto.ResponseData}");

        // Handing the situation that contract send result
        if (dto.Type == "CONTRACT_SEND_RESULT")
        {
            ContractSendResultDto result = JsonConvert.DeserializeObject<ContractSendResultDto>(dto.ResponseData!.ToString()!)!;

            // Matching the task in the database
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);

            // If the match is successful
            if (task is not null)
            {
                if(task.CurrentStep == TaskStep.ContractCreating)
                {
                    // Record the task log
                    taskService.LogInfo(task.Id, "Contract creation completed.");

                    // Update the task step
                    taskService.ChangeStep(task.Id, TaskStep.ContractCreated);

                    // Update the custom field of DocuSign envelope
                    docuSignService.UpdateComment(task.DocuSignEnvelopeId, "Contract creation completed.").GetAwaiter().GetResult();
                }
                else
                {
                    // Record the task log
                    taskService.LogInfo(task.Id, "Contract creation completed. But the contract status is not matching.");
                }
            }
        }
        // Filter the old version that contract send result
        else if (dto.Type == "SIGNLE_CONTRACT_SEND_RESULT")
        {
        }
        // Handing the situation that operation complete
        else if (dto.Type == "OPERATION_COMPLETE")
        {
            OperationCompleteDto result = JsonConvert.DeserializeObject<OperationCompleteDto>(dto.ResponseData!.ToString()!)!;

            // Matching the task in the database 
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);

            // If the match is successful
            if (task is not null)
            {
                if(task.CurrentStep ==  TaskStep.ContractCreated)
                {
                    // Record the task log
                    taskService.LogInfo(task.Id, $"Contract signed by {result.UserAccount} - Contract signing");

                    if (result.OperationStatus == "SIGN_SUCCEED")
                    {
                        // Update the custom field of DocuSign envelope
                        docuSignService.UpdateComment(task.DocuSignEnvelopeId, $"Contract signed by {result.UserAccount} - Contract signing").GetAwaiter().GetResult();
                    }
                    else if (result.OperationStatus == "SIGN_FAILED")
                    {
                        taskService.LogInfo(task.Id, $"The contract signing has failed by: {result.UserAccount} - {result.EnterpriseName}");
                        taskService.ChangeStep(task.Id, TaskStep.Completed);
                        await docuSignService.VoidedEnvelope(task.DocuSignEnvelopeId, 
                            $"The contract signing has failed by: {result.UserAccount} - {result.EnterpriseName} [{DateTime.Now}]");
                    }
                    else if (result.OperationStatus == "REJECT")
                    {
                        taskService.LogInfo(task.Id, $"The contract has been declined by: {result.UserAccount} - {result.EnterpriseName}");
                        taskService.ChangeStep(task.Id, TaskStep.Completed);
                        await docuSignService.VoidedEnvelope(task.DocuSignEnvelopeId,
                            $"The contract has been declined by: {result.UserAccount} - {result.EnterpriseName} [{DateTime.Now}]");
                    }
                    else
                    {
                        taskService.LogInfo(task.Id, $"The signing status of the unprocessed Bestsign signers: {result.OperationStatus}");
                    }
                }
                else
                {
                    // Record the task log
                    taskService.LogInfo(task.Id, $"Contract signed by {result.UserAccount} - Contract signing. But the contract status is not matching.");
                }
            }
        }
        // Handing the situation that contract complete
        else if (dto.Type == "CONTRACT_COMPLETE")
        {
            ContractCompleteDto result = JsonConvert.DeserializeObject<ContractCompleteDto>(dto.ResponseData!.ToString()!)!;

            foreach (var contractId in result.ContractIds)
            {
                // Matching the task in the database 
                var task = taskService.GetTaskByBestSignContractId(contractId);

                // If the match is successful
                if (task is not null)
                {
                    if(task.CurrentStep == TaskStep.ContractCreated)
                    {
                        // Record the task log 
                        taskService.LogInfo(task.Id, "Contract signing completed");

                        // Update the task step
                        taskService.ChangeStep(task.Id, TaskStep.ContractCompleted);

                        // Update the custom field of DocuSign envelope
                        docuSignService.UpdateComment(task.DocuSignEnvelopeId, "Contract signing completed").GetAwaiter().GetResult();
                    }
                    else
                    {
                        // Record the task log
                        taskService.LogInfo(task.Id, $"Contract signing completed. But the contract status is not matching.");
                    }
                }
            }
        }
        // Handing the situation that contract overdue
        else if (dto.Type == "CONTRACT_OVERDUE")
        {
            ContractOverdueDto result = JsonConvert.DeserializeObject<ContractOverdueDto>(dto.ResponseData!.ToString()!)!;

            // Matching the task in the database 
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);

            // If the match is successful
            if (task is not null)
            {
                if(task.CurrentStep == TaskStep.ContractCreated)
                {
                    // Record the task log
                    taskService.LogInfo(task.Id, "The contract has expired.");

                    // Update the task step
                    taskService.ChangeStep(task.Id, TaskStep.Completed);

                    await docuSignService.VoidedEnvelope(task.DocuSignEnvelopeId,
                            $"The contract has expired. [{DateTime.Now}]");
                }
                else
                {
                    // Record the task log
                    taskService.LogInfo(task.Id, $"The contract has expired. But the contract status is not matching.");
                }
            }
        }
        // Handing the situation that contract revoke
        else if (dto.Type == "CONTRACT_REVOKE")
        {
            ContractRevokeDto result = JsonConvert.DeserializeObject<ContractRevokeDto>(dto.ResponseData!.ToString()!)!;

            // Matching the task in the database 
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);

            // If the match is successful
            if (task is not null)
            {
                if(task.CurrentStep == TaskStep.ContractCreated)
                {
                    // Record the task log
                    taskService.LogInfo(task.Id, $"The contract has been revoked. Reason: {result.RevokeReason} Revoke by: {result.UserAccount} - {result.EnterpriseName}");

                    // Update the task step
                    taskService.ChangeStep(task.Id, TaskStep.Completed);

                    await docuSignService.VoidedEnvelope(task.DocuSignEnvelopeId,
                            $"The contract has been revoked. Reason: {result.RevokeReason} Revoke by: {result.UserAccount} - {result.EnterpriseName} [{DateTime.Now}]");
                }
                else
                {
                    // Record the task log
                    taskService.LogInfo(task.Id, $"The contract has been revoked. But the contract status is not matching.");
                }
            }
        }

        return new
        {
            Code = "200",
            Message = "success",
        };
    }
}