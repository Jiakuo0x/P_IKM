using Lib.Azure;
using Services;

namespace Controllers;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("keyvault")]
    public object TestKeyVault([FromServices] KeyVaultManager keyVaultManager)
    {
        return new
        {
            BestSign = keyVaultManager.GetBestSignSecret(),
            DocuSign = keyVaultManager.GetDocuSignSecret()
        };
    }

    [HttpGet("docusign")]
    public async Task<object> TestDocuSign([FromServices] DocuSignService docuSign)
    {
        var res = await docuSign.MatchEnvelopes();
        return res;
    }

    [HttpGet("bestsign")]
    public async Task<object> TestBestSign([FromServices] BestSignService bestSign)
    {
        await bestSign.Post<object>("/api/contracts/remind", new
        {
            contractId = "task.BestSignContractId",
        });
        return "ok";
    }
}