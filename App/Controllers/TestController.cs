using Lib.Azure;
using Services;

namespace Controllers;

//[Delete Flag]
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

    [HttpGet("docusign/{envelopeId}")]
    public async Task<object> TestDocuSign([FromServices] DocuSignService docuSign, string envelopeId)
    {
        var res = await docuSign.GetEnvelopeAsync(envelopeId);
        return res;
    }

    [HttpGet("bestsign/remind/{contractId}")]
    public async Task<object> TestBestSignRemind([FromServices] BestSignService bestSign, string contractId)
    {
        var result = await bestSign.Post<object>("/api/contracts/remind", new
        {
            contractId = contractId,
        });
        return result;
    }
}