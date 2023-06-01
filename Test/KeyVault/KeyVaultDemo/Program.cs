using Azure.Security.KeyVault.Secrets;
using Azure;
using Azure.Identity;

string keyVaultUrl = "https://docusign-testvk.vault.azure.cn/";

string secretName = "docusignprivatekey";
var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
Response<KeyVaultSecret> secretResponse = await client.GetSecretAsync(secretName);
Console.WriteLine("docusignprivatekey:");
Console.WriteLine(secretResponse.Value.Value);

secretName = "dspk3";
client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
secretResponse = await client.GetSecretAsync(secretName);
Console.WriteLine("dspk3:");
Console.WriteLine(secretResponse.Value.Value);

secretName = "bestsignprivatekey";
client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
secretResponse = await client.GetSecretAsync(secretName);
Console.WriteLine("bestsignprivatekey:");
Console.WriteLine(secretResponse.Value.Value);