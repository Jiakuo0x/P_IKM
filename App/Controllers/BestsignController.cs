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
    public bool Listen([FromServices] TaskService taskService, DocuSignService docuSignService,
        [FromBody] BestSignCallbackDto dto)
    {
        if(dto.Type == "CONTRACT_SEND_RESULT")
        {
            ContractSendResultDto result = JsonConvert.DeserializeObject<ContractSendResultDto>(dto.ResponseData!)!;
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);
            taskService.LogInfo(task.Id, dto.ResponseData!);
            docuSignService.AddComment(task.DocuSignEnvelopeId, "BestSign Contract Sent").GetAwaiter().GetResult();
        }
        else if(dto.Type == "SIGNLE_CONTRACT_SEND_RESULT")
        {
            _logger.LogInformation($"SIGNLE_CONTRACT_SEND_RESULT: {dto.ResponseData}");
        }
        else if(dto.Type == "OPERATION_COMPLETE")
        {
            OperationCompleteDto result = JsonConvert.DeserializeObject<OperationCompleteDto>(dto.ResponseData!)!;
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);
            taskService.LogInfo(task.Id, dto.ResponseData!);
            docuSignService.AddComment(task.DocuSignEnvelopeId, "BestSign Contract Signed").GetAwaiter().GetResult();
        }
        else if(dto.Type == "CONTRACT_COMPLETE")
        {
            ContractCompleteDto result = JsonConvert.DeserializeObject<ContractCompleteDto>(dto.ResponseData!)!;
            foreach(var contractId in result.ContractIds)
            {
                var task = taskService.GetTaskByBestSignContractId(contractId);
                taskService.ChangeStep(task.Id, TaskStep.Completed);
                docuSignService.AddComment(task.DocuSignEnvelopeId, "BestSign Contract Completed").GetAwaiter().GetResult();
            }
        }
        else if(dto.Type == "CONTRACT_OVERDUE")
        {
            ContractOverdueDto result = JsonConvert.DeserializeObject<ContractOverdueDto>(dto.ResponseData!)!;
            var task = taskService.GetTaskByBestSignContractId(result.ContractId);
            taskService.ChangeStep(task.Id, TaskStep.ContractOverdue);
            docuSignService.AddComment(task.DocuSignEnvelopeId, "BestSign Contract Overdue").GetAwaiter().GetResult();
        }

        _logger.LogInformation($"[BestSign Error] [Unknown Type] {JsonConvert.SerializeObject(dto)}");
        return true;
    }
}