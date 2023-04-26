namespace Lib.BestSign;

/// <summary>
/// The configuration of Bestsign is specified in appsettings.json
/// </summary>
public class Configuration
{
    /// <summary>
    /// The server host of Bestsign
    /// </summary>
    public string ServerHost { get; set; } = string.Empty;

    /// <summary>
    /// The client id of Bestsign
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The client secret of Bestsign
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}