using Services;

namespace Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    public TestController()
    {
    }

    [HttpGet("send-email")]
    public bool SendEmail(EmailService emailService)
    {
        emailService.SendEmail("jiakuo.zhang@quest-global.com", "Test Title", "Test Content");
        return true;
    }
}