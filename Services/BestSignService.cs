using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Lib.BestSign;
using Lib.BestSign.Common;
using Lib.BestSign.Dtos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Services;

/// <summary>
/// BestSign service
/// </summary>
public class BestSignService
{
    private readonly IOptions<Configuration> _options;
    private readonly TokenManager _tokenManager;
    private readonly Lib.Azure.KeyVaultManager _keyVaultManager;
    public BestSignService(
        IOptions<Configuration> options, 
        TokenManager tokenManager,
        Lib.Azure.KeyVaultManager keyVaultManager)
    {
        _options = options;
        _tokenManager = tokenManager;
        _keyVaultManager = keyVaultManager;
    }

    /// <summary>
    /// Generate signature for an HTTP request
    /// </summary>
    /// <param name="client">Http request client</param>
    /// <param name="requestUri">Http request url</param>
    /// <param name="requestBody">Http request body</param>
    /// <returns>Task of the function</returns>
    public async Task GenerateSignature(HttpClient client, string requestUri, object? requestBody = null)
    {
        string timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

        string requestBodyMd5 = string.Empty;
        if (requestBody is not null)
        {
            var requestBodyStr = JsonConvert.SerializeObject(requestBody);
            var md5Hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(requestBodyStr));
            requestBodyMd5 = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
        }

        client.DefaultRequestHeaders.Add("bestsign-client-id", _options.Value.ClientId);
        client.DefaultRequestHeaders.Add("bestsign-sign-timestamp", timestamp);
        client.DefaultRequestHeaders.Add("bestsign-signature-type", "RSA256");

        string signStr = $"bestsign-client-id={_options.Value.ClientId}";
        signStr += $"bestsign-sign-timestamp={timestamp}";
        signStr += $"bestsign-signature-type=RSA256";
        signStr += $"request-body={requestBodyMd5}";
        signStr += $"uri={requestUri}";

        var privateKey = _keyVaultManager.GetBestSignSecret();
        var encryptedBase64 = Encrypt.SignStr(signStr,privateKey);

        client.DefaultRequestHeaders.Add("bestsign-signature", encryptedBase64);

        var token = await _tokenManager.GetToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    public async Task<T> Get<T>(string uri)
    {
        HttpClient client = new HttpClient();
        await GenerateSignature(client, uri);
        var apiResponse = await client.GetFromJsonAsync<ApiResponse<T>>(_options.Value.ServerHost + uri);

        if (apiResponse is null)
            throw new Exception($"[BestSign Error] [Return Value Error] URL:{client.BaseAddress}");

        if (apiResponse.Code != "0")
            throw new Exception($"[BestSign Error] Code:{apiResponse.Code} Message:{apiResponse.Message}");

        return apiResponse.Data;
    }

    public async Task<T> Post<T>(string url, object data)
    {
        HttpClient client = new HttpClient();
        await GenerateSignature(client, url, data);

        string requestMessage = JsonConvert.SerializeObject(data);

        HttpContent content = new StringContent(requestMessage, Encoding.UTF8, "application/json");
        var resposneMessage = await client.PostAsync(_options.Value.ServerHost + url, content);
        var apiResponse = await resposneMessage.Content.ReadFromJsonAsync<ApiResponse<T>>();

        if (apiResponse is null)
            throw new Exception($"[BestSign Error] [Return Value Error] URL:{client.BaseAddress}");

        if (apiResponse.Code != "0")
            throw new Exception($"[BestSign Error] Code:{apiResponse.Code} Message:{apiResponse.Message}");

        return apiResponse.Data;
    }

    public async Task<Stream> PostAsStream(string url, object data)
    {
        HttpClient client = new HttpClient();
        await GenerateSignature(client, url, data);

        string requestMessage = JsonConvert.SerializeObject(data);

        HttpContent content = new StringContent(requestMessage, Encoding.UTF8, "application/json");
        var resposneMessage = await client.PostAsync(_options.Value.ServerHost + url, content);
        var apiResponse = await resposneMessage.Content.ReadAsStreamAsync();

        return apiResponse;
    }
}