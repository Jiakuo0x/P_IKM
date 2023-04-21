using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Lib.Azure;

public class KeyVaultManager
{
    private readonly IOptions<KeyVaultConfiguration> _options;
    public KeyVaultManager(IOptions<KeyVaultConfiguration> options)
    {
        _options = options;
    }

    private string? _docuSignSecret = null;
    public string GetDocuSignSecret()
    {
        if (_docuSignSecret is null)
            _docuSignSecret = GetSecret(_options.Value.DocuSignSecretName, _options.Value.DocuSignKeyVaultUrl);
        return _docuSignSecret;
    }

    private string? _bestSignSecret = null;
    public string GetBestSignSecret()
    {
        if (_bestSignSecret is null)
            _bestSignSecret = GetSecret(_options.Value.BestSignSecretName, _options.Value.BestSignKeyVaultUrl);
        return _bestSignSecret;
    }
    public string GetSecret(string secretName, string keyVaultUrl)
    {
        var credential = new DefaultAzureCredential();
        var client = new SecretClient(new Uri(keyVaultUrl), credential);

        Response<KeyVaultSecret> secret = client.GetSecret(secretName);
        return secret.Value.Value;
    }
}
