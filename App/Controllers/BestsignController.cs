using App.Dtos;
using Database.Enums;
using Dtos.BestSignCallbackDto;
using Services;

namespace Controllers;

[ApiController]
[Route("api/bestsign")]
public class BestsignController : ControllerBase
{
    private readonly ILogger<BestsignController> _logger;
    public BestsignController(ILogger<BestsignController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("listen")]
    public object Listen([FromServices] TaskService taskService, [FromServices] DocuSignService docuSignService,
        [FromBody] BestSignCallbackDto dto)
    {
        _logger.LogInformation($"[BestSign] [Callback] {JsonConvert.SerializeObject(dto)}");

        if (dto.Type == "CONTRACT_SEND_RESULT")
        {
            ContractSendResultDto result = JsonConvert.DeserializeObject<ContractSendResultDto>(dto.ResponseData!.ToString()!)!;
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);
            if (task is not null)
            {
                taskService.LogInfo(task.Id, "Contract creation completed.");
                docuSignService.UpdateComment(task.DocuSignEnvelopeId, "Contract creation completed.").GetAwaiter().GetResult();
            }
        }
        else if (dto.Type == "SIGNLE_CONTRACT_SEND_RESULT")
        {
        }
        else if (dto.Type == "OPERATION_COMPLETE")
        {
            OperationCompleteDto result = JsonConvert.DeserializeObject<OperationCompleteDto>(dto.ResponseData!.ToString()!)!;
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);
            if (task is not null)
            {
                taskService.LogInfo(task.Id, $"Contract signed by {result.UserAccount} - Contract signing");
                docuSignService.UpdateComment(task.DocuSignEnvelopeId, $"Contract signed by {result.UserAccount} - Contract signing").GetAwaiter().GetResult();
            }
        }
        else if (dto.Type == "CONTRACT_COMPLETE")
        {
            ContractCompleteDto result = JsonConvert.DeserializeObject<ContractCompleteDto>(dto.ResponseData!.ToString()!)!;
            foreach (var contractId in result.ContractIds)
            {
                var task = taskService.GetTaskByBestSignContractId(contractId);
                if (task is not null)
                {
                    taskService.LogInfo(task.Id, "Contract signing completed");
                    taskService.ChangeStep(task.Id, TaskStep.ContractCompleted);
                    docuSignService.UpdateComment(task.DocuSignEnvelopeId, "Contract signing completed").GetAwaiter().GetResult();
                }
            }
        }
        else if (dto.Type == "CONTRACT_OVERDUE")
        {
            ContractOverdueDto result = JsonConvert.DeserializeObject<ContractOverdueDto>(dto.ResponseData!.ToString()!)!;
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);
            if (task is not null)
            {
                taskService.LogInfo(task.Id, "The contract has expired");
                taskService.ChangeStep(task.Id, TaskStep.Failed);
                docuSignService.UpdateComment(task.DocuSignEnvelopeId, "The contract has expired").GetAwaiter().GetResult();
            }
        }

        return new
        {
            Code = "200",
            Message = "success",
        };
    }
}