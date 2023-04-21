using DocuSign.eSign.Client;

namespace Lib.DocuSign;

public class ClientManager
{
    private readonly IOptions<Configuration> _options;
    private readonly Azure.KeyVaultManager _keyVaultManager;

    public ClientManager(
        IOptions<Configuration> options,
        Azure.KeyVaultManager keyVaultManager)
    {
        _options = options;
        _keyVaultManager = keyVaultManager;
    }

    public DocuSignClient GetClient()
    {
        var client = new DocuSignClient(_options.Value.ApiBase);

        var privateKey = _keyVaultManager.GetDocuSignSecret();

        client.RequestJWTUserToken(
                _options.Value.ClientId,
                _options.Value.UserId,
                _options.Value.AuthServer,
                System.Text.Encoding.Default.GetBytes(privateKey),
                 1);

        return client;
    }
}