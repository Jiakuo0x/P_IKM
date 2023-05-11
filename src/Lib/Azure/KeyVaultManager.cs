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
    private string? _docuSignSecret = null;

    /// <summary>
    /// Get secret of DocuSign
    /// </summary>
    /// <returns>Secret of DocuSign</returns>
    public string GetDocuSignSecret()
    {
        // (Debug for local)
        //return """
        //    -----BEGIN RSA PRIVATE KEY-----
        //    MIIEowIBAAKCAQEAlt5btJZBSYpeTXjhuEmmevufe3ryq50xdjbpI97qnAySc6Kx
        //    t6x2lf5GgVssOXHlTRVi79Tr1o8vRKNjDPJOi8ET1p1aie+l5iHiYyZj2yyfD9fv
        //    71e+asA8vnDVtIiDObZ3D+bYLodDFrEb9knHZZmYCndDvKGwXy1OELVwdrJ638Zz
        //    2p4lqFHHJKHE6wOfV8K6BvD2+9JHdaeqQRg0uTmI4fqhQIpavFOfJjWmxgTvK4nB
        //    uztCl0HINXgRGOeH/jWnd/OuXvu8gx7fa0/+8yjmrZXwkAOqnVci8M3uJue3biUE
        //    MJRJpc0v5EdKz3pJcEAF5vL0Z5joH6lVIthzYQIDAQABAoIBAASxr3D4RqWNa68f
        //    Mzq2KlSXDqVbZ0CF5NuUpf73n4OydArea+29YBH9Qt+64MhHlG0oQxdLBl/himsP
        //    mfRCd8VeZ3BQHgu+2QHgWdH6IeilKnlmORdqOsj8gtQg4G1V3NHHYExhqaYTXFGE
        //    IJmycV5//5yK86CwKs1gGsoA5mcy/+pLYwownfUje5kVx0+/YqC8cFAJyJHREYai
        //    WtmvYSW450F2E0Fq5C9LQuv0tUh/7AWVqwmF+nPD4K6wsf/IfnuTkYPNRH3uf1gu
        //    dW1D1tZxGiWpVdF0jxD80JaOurV0IzpjAYifSmeJf/KlDyPFdyl8PDih0cPcC2nr
        //    CDRq4XcCgYEA3f+AR2pp8cYIfsFjEtqJuufw96tdaX9q4g4MaNQm8vKEqoK43K+B
        //    o4bA6r8pDTJSsgbv4YXuttzRQ17+sTAKB+WyaPKwwhe8Rky99Z1HNTFYCsBI28t5
        //    MWS35RDptxa2xXylbKoIEkClz9q0jnbmkiBf2nEzjl9t2g81iZHdt5cCgYEArfni
        //    mV1TyLLc1PcQsjL9MOQxF6gCTtnrA0Pw/hpeCcHt7o8y6eeauZJFJzlvTIdUUMPR
        //    unH171sVetX5iG0HJeuJK8r6pavsmsyOSNCTGfIIbUz+gTPgq581K9mGLFGVAdjP
        //    1ovBOx7PYdO9BXwrJRuZTGg/FCqRA4xY9pWmy8cCgYB7dMGD9bvhRr4mr6lHLN13
        //    YdFyCoyyRLfN6v4ftgvLA++fW38uyzOPGzth0Nkli5zNgGoawv7UFs0RaFy/cPXD
        //    GowzLPP7nHOJrNffJY4aGMzbfb+G7AsD2v0hmFxBA5K1FPJyEcTXUbhkdT4AFEN5
        //    dCOaOWXwgUV4BQlC7imdFQKBgHpgIn+EgVHUVrfKzki6yxRf/xRHzs/OQ5x5ZwQm
        //    Ye11Jzs+KS8VBeXwuIn9wYdQTgO9qkH+tWLXbAWKi8rl/jgzNLrEPYjZpUXCC3e2
        //    lzKR6FGR7hfN+QRfqdQdX16/SBQTgSbGCXbfljqW6Qf5rpOclTmEvpId2wFm8JEK
        //    9VezAoGBAIzTYL6GO+rfu1+AP6ozmZkAwSSVtMPCFGpfpuLQ3aoJPOgIg6mlMADC
        //    hqEK5BTIx4N4THCrVfE3VhI6kRI9/UWZJlnsOTAx/9dt/hO2G1xpJBN1vP4FaXNT
        //    1fdGfZIXh+KC3K+teRuSFrLjSmkmnBtHFXtFG9N97wmBNyNUSJMp
        //    -----END RSA PRIVATE KEY-----
        //""";

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
    private string? _bestSignSecret = null;

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
