using System.Security.Cryptography;
using System.Text;
using Lib.BestSign.Common;
using Lib.BestSign.Dtos;

namespace Lib.BestSign;

public class ApiClient
{
    private readonly IOptions<Configuration> _options;
    private readonly TokenManager _tokenManager;
    public ApiClient(IOptions<Configuration> options, TokenManager tokenManager)
    {
        _options = options;
        _tokenManager = tokenManager;
    }
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

        var encryptedBase64 = Encrypt.SignStr(signStr, _options.Value.PrivateKey);

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

        //test
        if (!File.Exists("requestMessage.json")) File.Create("requestMessage.json");
        File.WriteAllText("requestMessage.json", requestMessage);

        HttpContent content = new StringContent(requestMessage, Encoding.UTF8, "application/json");
        var resposneMessage = await client.PostAsync(_options.Value.ServerHost + url, content);
        var apiResponse = await resposneMessage.Content.ReadFromJsonAsync<ApiResponse<T>>();
        

        if (apiResponse is null)
            throw new Exception($"[BestSign Error] [Return Value Error] URL:{client.BaseAddress}");

        if (apiResponse.Code != "0")
            throw new Exception($"[BestSign Error] Code:{apiResponse.Code} Message:{apiResponse.Message}");

        return apiResponse.Data;
    }
}