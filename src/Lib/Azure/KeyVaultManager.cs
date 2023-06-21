using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Lib.Azure;

/// <summary>
/// Azure Key Vault manager
/// </summary>
public class KeyVaultManager
{
    private readonly IOptions<KeyVaultConfiguration> _options;
    public KeyVaultManager(IOptions<KeyVaultConfiguration> options)
    {
        _options = options;
    }

    /// <summary>
    /// Secret cache of DocuSign
    /// </summary>
    private static string? _docuSignSecret = null;

    /// <summary>
    /// Get secret of DocuSign
    /// </summary>
    /// <returns>Secret of DocuSign</returns>
    public string GetDocuSignSecret()
    {
        // (Debug for local)
        //return """
        //     -----BEGIN RSA PRIVATE KEY-----
        //     MIIEowIBAAKCAQEAwDHTONfAvZLF1U4tpW68zEWhhssRIdsNfDan2GhTP/5O1zoZ
        //     4SBAkgMTw0FRWVfph1c8FX/V77/0uTRAss4X7JhcY5rgB1DG05IIjh5fZtpmAyIU
        //     4QI34Yx3RZwIu6WEykdlKfRsx59UDWYpLaqDCosUusk6HT6OZF5l8rhru3XuHQ72
        //     3trkR9BYMzZI/r64JMD39Ahv7UGgZ2CZcHXGCkZZHV30HKQOI0Bhq4MPXB6rf7rw
        //     +2VGkbz7wa/iXKRzHin5Ig4c2Zivln08mU1DZDkP8Hjb/cyn9oddoBHyNJVuf/aC
        //     KkoimGp4oQ3dp9DEO6JuJLYnXTlp57pT/6m4IQIDAQABAoIBAAEtcPheadwzeXDb
        //     YwgqNcqIg5hvVzBCbtUdF9xJioHdbNCGDEAViLBoUnXnWRsY4M8LJxdl4QrQQxIN
        //     KUIQhTj2LOyQjOkIkE5Ip6gjah/XrH6I4NDpLhbXUP4BKlRy0OMPysrbSDJqe3XT
        //     +Ne9Is2aS77yTN/eiQRsXQ0pPszkmxOghEByKJFDEZODYpmj+dLsviTGINabKagj
        //     ggI7ZOKXnh86DniqfNlnIUJSrkc05nt+9JOMGP1s71hcW25P/ZLdY/4JbwQy0C+S
        //     7zfvmby5wq7wdObYpAZx6lYo4v39WjMB0+9YVEyR9+0n4vxxon7aikPdtMh05rIV
        //     1GH11wECgYEA72ys0SwqfMqQ4nFHF3suap0DDEHeDL87rX3cdCZOdifZLOieCTAT
        //     dPPUvleJ7kVXKy792jdFNjNV3M8LSTQaldkXRlc5tCUn0fXxNNmxiWCZAbZpHb/h
        //     /J/y1a/mXWhvi/qn246W+nqyDXLgKexZHA9BCX1MCcUjNBw99hlSkIECgYEAzYAY
        //     DaB4GmSuTVa9bhgEat5zFNlLzUea9fKdwQLA/9nRQUrHQZyx2VFwE9ZKHdp0ViD8
        //     loFjPPHIq4tEOzhlK4gRXkRLlqVGNWfVEnGv4sU8u0EhtbvS+ZlgqwKVggqOkOGN
        //     b5LN9mjPcKPdMoTI3e+1VNPZwXeBjs/QLOMhV6ECgYBnQJ+6yWg9TwJylVWKW9Yl
        //     pjbVR2aiaqoq5Ld447g8nmy7QEo/Pht1+V2LBKd4OTEhb2+Mdv+DI1ppEmUUmcLJ
        //     UskFzcIV3Vwx4PW3zBYTX7Q8l9T3PnQQBU6tNuGK8OoCHQPc3L5hoR4+TIc9rS60
        //     uiNbG0z6fajz5SYXhs4jgQKBgBwOxTn3Eu7nmDTPBwYlGVVOEgBiusrIYAv8mDVm
        //     HtvP7ZYwk4wNzOqTB+5tb6krzn6D+njCKb6EdJWXajV7ubR724M3Z+qlnjRkfN8L
        //     zqvzqhsXBXc+scuKeXxf2fHrZxO4+AqnzJ88KDaE6Qqgvpwuwl9WnYCgxHuvPAkk
        //     6cSBAoGBAKwZd8zlUKM4PpSpceKT88EP7yrFR3rQU7jFjGl6WbYLVfSLXM0pPzhu
        //     vcFqV7ukmPCNaPbrb15eD5v4ypu1ZuxTDFj/RaLWw5c0MK3HqnPCRPXG5XQD7VOR
        //     5gaew4nLl3fr9UHJZH/t06mkagbkuKnZKi34hQ070Wt99TSPpwoe
        //    -----END RSA PRIVATE KEY-----
        // """;

        if (_docuSignSecret is null)
        {
            _docuSignSecret = string.Empty;
            _docuSignSecret += "-----BEGIN RSA PRIVATE KEY-----";
            _docuSignSecret += GetSecret(_options.Value.DocuSignSecretName, _options.Value.DocuSignKeyVaultUrl);
            _docuSignSecret += "-----END RSA PRIVATE KEY-----";
        }
        return _docuSignSecret;
    }

    /// <summary>
    /// Secret cache of Bestsign
    /// </summary>
    private static string? _bestSignSecret = null;

    /// <summary>
    /// Get secret of Bestsign
    /// </summary>
    /// <returns>Secret of Bestsign</returns>
    public string GetBestSignSecret()
    {
        // (Debug for local)
        //return "MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCy+f6HBwI50+AkAEprzmhU+y171oM06S9rc1/YMNlS77uFA4e/TUzmukpC26zteMpM1FASOgaBqQtAFUE5HKI30AApcFAl8zXDF+dxOUqExvs0HCFcISKJSjQ1b+KHTnIS9xUW6GakhH1hJ4TJDcYaliA5DywO1vImPy0uiYBOmRjQPjTf4uSBrkdHym+fwtq2QFF3PIyQMfEa3Uj1QGPGsw+6JOeoYzsSsHDrX/Oh3//DbzSdZQILIxU1YdD1/5VQB6Dev38fMoB3nyg3ABBKcvT7Q/3w24C/mvSdNlP70RsNnLLoHux4UYlqC++nSZ0nZbDDatLmI13EFRLLhCHtAgMBAAECggEAXAKhk5FK16fRJzDvEZU/ldC7hVq2gVEQC9F4iJA0aarNYIh8FSEMU+GZo92DfWIHvo+3ymcCSU46dmt26IGL891+987BpYDvNqjnVxH4+WHCavu7Or3eH26CgKZQcvclNhLISMqZWiKywmuqnCH0ol5jmnHuWIKwYnFALRiLWdwGS5cCxu0oz/8G43PN8SjQZ862kBjz7GkMzUvj4ZkvzydQ7hdtYaGmQzsSOE0hZ6/XysSYQwq9NkRCWjY8mniRORXg06dMES+YVZrrK0zYaCkrIOrJPV+A/3UJ2uQifNt5Xm1Y9Z+p9D+np5NExq7j1V87+S/iLVXttHzgWo9qEQKBgQDodCCLXVBE0sNUxN1b82h+tSLFdkLrWdwouhA09ZGQBLYVYmrjWwz4q6D76B5S1zhNg9ChvyxDUT/L+ynAGDM+mzRoy/F0d1m5vFNrLPXyF0CsAZF9Ox/MMCplJqkfbVl2Taf/km0lFuVrKFS58o1HM+Xg1FfWywrhXcN4pptuUwKBgQDFGyAyefjWWfh7DFe3FsSpgHAyoIq5wmitjb89iN5hqPN22hwZU3QLmOzUFjI0tT+507DodRmXHc8KJKp5WiB/yPuBbjIRJ/SqXakpGYoyOKe6mU5+Volq1bkg/DRqvVlLYUqgSUJM9VUsF2LtaqupSWWacGW4I00K7KLWDZGmvwKBgQCon50AG/ffhRinRIvjaQYzbEjF/0z6F1yKuraJBF7Mn25KwvMQ7HrTZQVJ148vvuob6PQOcXS1fJoP6anWrHd4AfSZ1N/aAb436zKEO8BKFq4WWKjmtF4TrBkE+W+T2aodFKY60kiUsDBKdJ5JqXbs4OvwFXmG2hGRfIgQ3KNB+wKBgQCJtCvIWAKK7ox0mujlFtkKeproI9UHdlTfe3oyKV8D559AV5zt7KnUOGCsw10MCdydnNGpdbYNJ1wv//HBmj++RtG/WRdNeRart4epGRi9gWtdKCH4jcivhOUzsD+Gmwa5bR1P4h6Z2YRJq4UHzVBkksyTGxdVkAQRL8WLuisZzQKBgHxRcFmPlap8mwDpWhQux0T4MRVXsitKaVRcIvag6BorlD7yCO2xAQoaykw5EEDC3ACgFWXVnGu6yjUlLLszoDPDulTlBhBGG8lrVMh++5JyYU7j+sdWVwiG0OHMwPuB3nPJ7kLCU3WyyHYHSzAeScHUn9DlAK54TTOTH5N+y9Af";

        if (_bestSignSecret is null)
            _bestSignSecret = GetSecret(_options.Value.BestSignSecretName, _options.Value.BestSignKeyVaultUrl);
        return _bestSignSecret;
    }

    /// <summary>
    /// Get secret by Azure Key Vault SDK
    /// </summary>
    /// <param name="secretName">Secret name of Azure Key Vault</param>
    /// <param name="keyVaultUrl">Url of Azure Key Vault</param>
    /// <returns></returns>
    public string GetSecret(string secretName, string keyVaultUrl)
    {
        var credential = new DefaultAzureCredential();
        var client = new SecretClient(new Uri(keyVaultUrl), credential);

        Response<KeyVaultSecret> secret = client.GetSecret(secretName);
        return secret.Value.Value;
    }
}
