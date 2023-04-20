using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Lib.Azure;

public static class KeyVault
{
    public static string GetSecret(string secretName, string keyVaultUrl)
    {
        var credential = new DefaultAzureCredential();
        var client = new SecretClient(new Uri(keyVaultUrl), credential);

        Response<KeyVaultSecret> secret = client.GetSecret(secretName);
        return secret.Value.Value;
    }
}
