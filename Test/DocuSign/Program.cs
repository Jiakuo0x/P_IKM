using DocuSign.eSign.Api;
using DocuSign.eSign.Client;

string privateKey = """
    -----BEGIN RSA PRIVATE KEY-----
    MIIEowIBAAKCAQEAwDHTONfAvZLF1U4tpW68zEWhhssRIdsNfDan2GhTP/5O1zoZ
    4SBAkgMTw0FRWVfph1c8FX/V77/0uTRAss4X7JhcY5rgB1DG05IIjh5fZtpmAyIU
    4QI34Yx3RZwIu6WEykdlKfRsx59UDWYpLaqDCosUusk6HT6OZF5l8rhru3XuHQ72
    3trkR9BYMzZI/r64JMD39Ahv7UGgZ2CZcHXGCkZZHV30HKQOI0Bhq4MPXB6rf7rw
    +2VGkbz7wa/iXKRzHin5Ig4c2Zivln08mU1DZDkP8Hjb/cyn9oddoBHyNJVuf/aC
    KkoimGp4oQ3dp9DEO6JuJLYnXTlp57pT/6m4IQIDAQABAoIBAAEtcPheadwzeXDb
    YwgqNcqIg5hvVzBCbtUdF9xJioHdbNCGDEAViLBoUnXnWRsY4M8LJxdl4QrQQxIN
    KUIQhTj2LOyQjOkIkE5Ip6gjah/XrH6I4NDpLhbXUP4BKlRy0OMPysrbSDJqe3XT
    +Ne9Is2aS77yTN/eiQRsXQ0pPszkmxOghEByKJFDEZODYpmj+dLsviTGINabKagj
    ggI7ZOKXnh86DniqfNlnIUJSrkc05nt+9JOMGP1s71hcW25P/ZLdY/4JbwQy0C+S
    7zfvmby5wq7wdObYpAZx6lYo4v39WjMB0+9YVEyR9+0n4vxxon7aikPdtMh05rIV
    1GH11wECgYEA72ys0SwqfMqQ4nFHF3suap0DDEHeDL87rX3cdCZOdifZLOieCTAT
    dPPUvleJ7kVXKy792jdFNjNV3M8LSTQaldkXRlc5tCUn0fXxNNmxiWCZAbZpHb/h
    /J/y1a/mXWhvi/qn246W+nqyDXLgKexZHA9BCX1MCcUjNBw99hlSkIECgYEAzYAY
    DaB4GmSuTVa9bhgEat5zFNlLzUea9fKdwQLA/9nRQUrHQZyx2VFwE9ZKHdp0ViD8
    loFjPPHIq4tEOzhlK4gRXkRLlqVGNWfVEnGv4sU8u0EhtbvS+ZlgqwKVggqOkOGN
    b5LN9mjPcKPdMoTI3e+1VNPZwXeBjs/QLOMhV6ECgYBnQJ+6yWg9TwJylVWKW9Yl
    pjbVR2aiaqoq5Ld447g8nmy7QEo/Pht1+V2LBKd4OTEhb2+Mdv+DI1ppEmUUmcLJ
    UskFzcIV3Vwx4PW3zBYTX7Q8l9T3PnQQBU6tNuGK8OoCHQPc3L5hoR4+TIc9rS60
    uiNbG0z6fajz5SYXhs4jgQKBgBwOxTn3Eu7nmDTPBwYlGVVOEgBiusrIYAv8mDVm
    HtvP7ZYwk4wNzOqTB+5tb6krzn6D+njCKb6EdJWXajV7ubR724M3Z+qlnjRkfN8L
    zqvzqhsXBXc+scuKeXxf2fHrZxO4+AqnzJ88KDaE6Qqgvpwuwl9WnYCgxHuvPAkk
    6cSBAoGBAKwZd8zlUKM4PpSpceKT88EP7yrFR3rQU7jFjGl6WbYLVfSLXM0pPzhu
    vcFqV7ukmPCNaPbrb15eD5v4ypu1ZuxTDFj/RaLWw5c0MK3HqnPCRPXG5XQD7VOR
    5gaew4nLl3fr9UHJZH/t06mkagbkuKnZKi34hQ070Wt99TSPpwoe
    -----END RSA PRIVATE KEY-----
""";
Console.WriteLine(privateKey);
var client = new DocuSignClient("https://demo.docusign.net/restapi");
client.RequestJWTUserToken(
                "d4e93ff3-6a63-4b8b-8895-ef7297aa0943",
                "dd77674b-38d3-4df8-9d71-ed9e1e4613a1",
                "account-d.docusign.com",
                System.Text.Encoding.Default.GetBytes(privateKey),
                1);

EnvelopesApi envelopesApi = new(client);
var templates = envelopesApi.ListStatusChanges("3363b373-d55d-4487-8828-13154df36834", new EnvelopesApi.ListStatusChangesOptions
{
    fromDate = DateTime.Now.AddDays(-30).ToString("yyyy/MM/dd")
});
Console.WriteLine(templates.Envelopes.Count);


var privateKey2 = "-----BEGIN RSA PRIVATE KEY-----" +
await KeyVault.GetDocuSignPrivateKey() +
"-----END RSA PRIVATE KEY-----";
Console.WriteLine(privateKey2);
var client2 = new DocuSignClient("https://demo.docusign.net/restapi");
client2.RequestJWTUserToken(
                "d4e93ff3-6a63-4b8b-8895-ef7297aa0943",
                "dd77674b-38d3-4df8-9d71-ed9e1e4613a1",
                "account-d.docusign.com",
                System.Text.Encoding.Default.GetBytes(privateKey2),
                1);
envelopesApi = new(client2);
var templates2 = envelopesApi.ListStatusChanges("3363b373-d55d-4487-8828-13154df36834", new EnvelopesApi.ListStatusChangesOptions
{
    fromDate = DateTime.Now.AddDays(-30).ToString("yyyy/MM/dd")
});
Console.WriteLine(templates2.Envelopes.Count);