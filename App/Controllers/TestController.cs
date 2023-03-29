namespace Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    public TestController()
    {
    }

    [HttpGet]
    public bool Test()
    {
        return true;
    }
}