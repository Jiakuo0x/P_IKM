using Azure.Security.KeyVault.Secrets;
using Azure;
using Azure.Identity;

string keyVaultUrl = "https://docusign-prod-kv.vault.azure.cn/";

string secretName = "docusignprivatekey";
var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
Response<KeyVaultSecret> secretResponse = await client.GetSecretAsync(secretName);
Console.WriteLine("docusignprivatekey:");
Console.WriteLine(secretResponse.Value.Value);

secretName = "docusignbestsign";
client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
secretResponse = await client.GetSecretAsync(secretName);
Console.WriteLine("docusignbestsign:");
Console.WriteLine(secretResponse.Value.Value);