using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
public static class BestSign
{
    private const string _host = "https://api.bestsign.cn";
    private const string _clientId = "1684721491016928566";
    private const string _clientSecret = "7017f25f0c9242c2bf17018a1951d794";
    public static async Task<string> GetToken()
    {
        var client = new HttpClient();

        var postData = new
        {
            clientId = _clientId,
            clientSecret = _clientSecret,
        };

        HttpContent content = new StringContent(
            JsonConvert.SerializeObject(postData),
            System.Text.Encoding.UTF8,
            "application/json");

        var resposneMessage = await client.PostAsync(_host + "/api/oa2/client-credentials/token", content);

        var responseString = await resposneMessage.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiResponse<Token>>(responseString);
        return result!.Data.AccessToken;
    }

    public static async Task<T> Post<T>(string url, object data)
    {
        HttpClient client = new HttpClient();
        await GenerateSignature(client, url, data);

        string requestMessage = JsonConvert.SerializeObject(data);

        HttpContent content = new StringContent(requestMessage, Encoding.UTF8, "application/json");
        var resposneMessage = await client.PostAsync(_host + url, content);
        var apiResponse = await resposneMessage.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiResponse<T>>(apiResponse);

        if (result is null)
            throw new Exception($"[BestSign Error] [Return Value Error] URL:{client.BaseAddress}");

        if (result.Code != "0")
            throw new Exception($"[BestSign Error] Code:{result.Code} Message:{result.Message}");

        return result.Data;
    }
    public static async Task<Stream> PostAsStream(string url, object data)
    {
        HttpClient client = new HttpClient();
        await GenerateSignature(client, url, data);

        string requestMessage = JsonConvert.SerializeObject(data);

        HttpContent content = new StringContent(requestMessage, Encoding.UTF8, "application/json");
        var resposneMessage = await client.PostAsync(_host + url, content);
        var apiResponse = await resposneMessage.Content.ReadAsStreamAsync();

        return apiResponse;
    }
    public static async Task GenerateSignature(HttpClient client, string requestUri, object? requestBody = null)
    {
        string timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

        string requestBodyMd5 = string.Empty;
        if (requestBody is not null)
        {
            var requestBodyStr = JsonConvert.SerializeObject(requestBody);
            var md5Hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(requestBodyStr));
            requestBodyMd5 = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
        }

        client.DefaultRequestHeaders.Add("bestsign-client-id", _clientId);
        client.DefaultRequestHeaders.Add("bestsign-sign-timestamp", timestamp);
        client.DefaultRequestHeaders.Add("bestsign-signature-type", "RSA256");

        string signStr = $"bestsign-client-id={_clientId}";
        signStr += $"bestsign-sign-timestamp={timestamp}";
        signStr += $"bestsign-signature-type=RSA256";
        signStr += $"request-body={requestBodyMd5}";
        signStr += $"uri={requestUri}";

        var privateKey = "MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQCB3oHHd82mQeCZMFpGNnA3ZAY3r7MCaprwzPtckFB7yJMpktSUyxGZC8cGSeg6yx6Og9b3TKO0kCkCvjJqoo3Y7kNtHkeL2WU+H6GIZGzikaA6+xTep5lMVdzgqTHQmRAzO3ZaZ9l44Q9get601qSyUMuyLBRBCMZ/NunLvmvjdkX+RlMVV+CvVsvrOUFe/+YgcaBbCqyq3AHfLY7FyVLBsxjUNbPXuewOF2kuO1nKcwX2Z856ID1EHrPFfGH1kfhi5a9uF72uIwKes3OYl9hyznglKHIa04wAfTZFBy7Juqsn5sjizPSryM6cSzGb8hm7ZW1w5u/QGlHX8HPWL5E7AgMBAAECggEAT9548CSU3CftqiibjEzUjKw+SqcKr3TCn09hU1cJuGbtYocDqBKPWxFsMEjpwqbCvyED5olCsLxsDFzOgtg/5mb6HrSdibuEUUWAwXIJqPmokj1yY1Ctrc2sjppfoYtQ9EEgk48Epxu6qjpGlu7e0S9xZr1HI4vzD6/E72HH1lY5fn4u/PsqxeaxXs6M1tN2rogeAC5i4nv8VAEzNgYfJltnq7ielzMU8FMnd40RcxNMUBPzZ1rQyZgufhXEv8LcoSpPVle3VlqwKBm9OyUCunYWn4JxxvS72QWIhSFf2uT4n7doiTp+ND9I+4FCyxKRomN/00ROpPb+l3OMSl76EQKBgQC5HZMbPtvxSt4Z8hNdNGn48Luo56nCpfJ4dglhXk9rLn+FyhF6F15o75oqdM6PrcZDssmVLYOXlspzbrWjH+IHgyEfYM1h0RQ3UPYl9RvHYjzfKnueF6tUmkUx9EeewzQEcnXgsEwTlA/g6mu3fc3OulRFUmDEzGWNHhJaH4pJIwKBgQCzmUQZx/nlCFoI0kiEE9HYD6L8F4Ard3HPlTllhnagRo21lcKGFfDK0CK8ib8mecI5hNCz+lawqaU4d7oEmNQ5ljEOmLAhz0mUvrpq5QmjOVpz5RrRBKi/fVRq6hnrjMYbP9QuMaIR39aR3xt3OCSOJZmzjoU9wziPW3nckuJ1CQKBgCfEebjNrTRN8B7EyXIpc2aeWMI1WlvNiKRmPyeiug79gzm7qjt/T7HdX1Ilm8Zz/3bFrtaUW4OySjW4H982Vzlj6zaxdg3Ae8ZSZz7KRDh2GzuaBcp1rUHUjm7n2ob5ym/2IuqtFPrTwQGBeriFlzQFBVuLEezVOC+zuR+RTQTfAoGAB8LHvpV1pwAFY/wyRgLVPpDuahZZ44b11BKOuGRnG+Xwmbgu/7xS+3CjD6KkL/Vy/ylOG3jl5hxul1IVJ72y7gofdJIEinF6rILRa20aTYNc9UUhM4cQA7ZWv4f+Nr+Oj/7iHFzos/0IOKV7eltiuQHG8otmFpUpDIjXa0CtqIECgYA2l3NmsQCUdtBnA8LJpTsNTujzV3hRglgvd4YI8Oavj6OMPfI/4HK8+6HSKql7bzC8X02OLUCA1AxxWKY0qLQKl8xMBt/nRKGqNXp0axESzQ6AE6qq7dsvHQlO6mTVBPeAD+9aKj3v+yJYCw6Dce0rgZBPlvWkGnQ2L1tvMLtlPQ==";
        var encryptedBase64 = SignStr(signStr,privateKey);

        client.DefaultRequestHeaders.Add("bestsign-signature", encryptedBase64);

        var token = await GetToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static string SignStr(string signStr, string privateKeyStr)
    {
        SHA256 sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(signStr));

        var privateKey = Convert.FromBase64String(privateKeyStr);
        var rsa = new RSACryptoServiceProvider();
        rsa.ImportPkcs8PrivateKey(privateKey, out _);

        var encryptedBytes = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var encryptedBase64 = Convert.ToBase64String(encryptedBytes);
        var encryptedUrlCode = System.Web.HttpUtility.UrlEncode(encryptedBase64, Encoding.UTF8);
        return encryptedUrlCode;
    }
}

public class ApiResponse
{
    public string Code { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
public class ApiResponse<T>: ApiResponse
{
    public new T Data { get; set; } = default!;
}

public class Token 
{
    public string TokenType { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string ExpiresIn { get; set; } = string.Empty;
    public long Expiration { get; set; }
    public DateTimeOffset ExpirationTime { get; set; }
}