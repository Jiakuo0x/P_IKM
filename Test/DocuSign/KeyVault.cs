using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

public static class KeyVault
{
    public static async Task<string> GetDocuSignPrivateKey()
    {
        string keyVaultUrl = "https://docusign-testvk.vault.azure.cn/";
        string secretName = "dspk3";
        var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        var secretResponse = await client.GetSecretAsync(secretName);
        return secretResponse.Value.Value;
    }
}