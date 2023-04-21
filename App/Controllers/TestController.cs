using Lib.Azure;

namespace Controllers;

public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("test")]
    public object Test()
    {
        return new
        {
            Test = "Test"
        };
    }

    [HttpGet("test/keyvault")]
    public object TestKeyVault([FromServices] KeyVaultManager keyVaultManager)
    {
        return new
        {
            BestSign = keyVaultManager.GetBestSignSecret(),
            DocuSign = keyVaultManager.GetDocuSignSecret()
        };
    }
}