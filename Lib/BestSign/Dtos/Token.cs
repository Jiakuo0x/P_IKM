namespace Lib.BestSign.Dtos;

public class Token 
{
    public string TokenType { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string ExpiresIn { get; set; } = string.Empty;
    public long Expiration { get; set; }
    public DateTimeOffset ExpirationTime { get; set; }
}