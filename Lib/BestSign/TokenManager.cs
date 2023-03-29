using Lib.BestSign.Dtos;

namespace Lib.BestSign;

public class TokenManager
{
    private readonly IOptions<Configuration> _options;
    public TokenManager(IOptions<Configuration> options)
    {
        _options = options;
    }
    public async Task<Token> GetToken()
    {
        var client = new HttpClient();

        var resposneMessage = await client.PostAsJsonAsync(_options.Value.ServerHost + "/api/oa2/client-credentials/token", new
        {
            clientId = _options.Value.ClientId,
            clientSecret = _options.Value.ClientSecret
        });

        ApiResponse<Token>? apiResponse = await resposneMessage.Content.ReadFromJsonAsync<ApiResponse<Token>>();

        if (apiResponse is null)
            throw new Exception("Failed to get token from BestSign");

        apiResponse.Data.ExpirationTime = DateTimeOffset.FromUnixTimeMilliseconds(apiResponse.Data.Expiration);
        return apiResponse.Data;
    }
}