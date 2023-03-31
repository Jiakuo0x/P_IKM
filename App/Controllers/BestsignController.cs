using App.Dtos;

namespace Controllers;

[ApiController]
[Route("api/bestsign")]
public class BestsignController : ControllerBase
{
    public BestsignController()
    {
    }

    [HttpPost]
    [Route("listen")]
    public bool Listen(BestSignCallbackDto dto)
    {
        if(dto.Type == "CONTRACT_SEND_RESULT ")
        {

        }
        else if(dto.Type == "SIGNLE_CONTRACT_SEND_RESULT")
        {

        }
        else if(dto.Type == "OPERATION_COMPLETE")
        {

        }
        else if(dto.Type == "CONTRACT_COMPLETE")
        {

        }
        else if(dto.Type == "CONTRACT_OVERDUE")
        {

        }
        return true;
    }
}