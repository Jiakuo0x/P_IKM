using Azure.Security.KeyVault.Secrets;
using Azure;
using Azure.Identity;

string secretName = "docusignprivatekey";
string keyVaultUrl = "https://docusign-testvk.vault.azure.cn/";

var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
Response<KeyVaultSecret> secretResponse = await client.GetSecretAsync(secretName);
Console.WriteLine(secretResponse.Value.Value);
