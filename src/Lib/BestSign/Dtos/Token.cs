namespace Lib.BestSign.Dtos;

/// <summary>
/// Bestsign request token
/// </summary>
public class Token 
{
    /// <summary>
    /// Token type
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Expiration time timestamp
    /// </summary>
    public string ExpiresIn { get; set; } = string.Empty;

    /// <summary>
    /// Expiration time timestamp
    /// </summary>
    public long Expiration { get; set; }

    /// <summary>
    /// Expiration time
    /// </summary>
    public DateTimeOffset ExpirationTime { get; set; }
}