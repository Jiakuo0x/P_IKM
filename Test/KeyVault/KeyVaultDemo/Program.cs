using Azure.Security.KeyVault.Secrets;
using Azure;
using Azure.Identity;

string keyVaultUrl = "https://docusign-prodkv.vault.azure.cn/";

string secretName = "docusignprivatekey1";
var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
Response<KeyVaultSecret> secretResponse = await client.GetSecretAsync(secretName);
Console.WriteLine("docusignprivatekey1:");
Console.WriteLine(secretResponse.Value.Value);

secretName = "bestsign";
client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
secretResponse = await client.GetSecretAsync(secretName);
Console.WriteLine("bestsign:");
Console.WriteLine(secretResponse.Value.Value);