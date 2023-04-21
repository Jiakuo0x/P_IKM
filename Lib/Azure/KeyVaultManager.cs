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
    public string GetDocuSignSecret()
    {
        return GetSecret(_options.Value.DocuSignSecretName, _options.Value.DocuSignKeyVaultUrl);
    }
    public string GetBestSignSecret()
    {
        return GetSecret(_options.Value.BestSignSecretName, _options.Value.BestSignKeyVaultUrl);
    }
    public string GetSecret(string secretName, string keyVaultUrl)
    {
        var credential = new DefaultAzureCredential();
        var client = new SecretClient(new Uri(keyVaultUrl), credential);

        Response<KeyVaultSecret> secret = client.GetSecret(secretName);
        return secret.Value.Value;
    }
}
