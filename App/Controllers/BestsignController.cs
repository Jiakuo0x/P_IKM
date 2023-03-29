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
    public bool Listen(string name)
    {
        return true;
    }
}