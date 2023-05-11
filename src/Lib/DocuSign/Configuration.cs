namespace Lib.DocuSign;

/// <summary>
/// The configuration of DocuSign is specified in appsettings.json
/// </summary>
public class Configuration
{
    /// <summary>
    /// The API base url of DocuSign
    /// </summary>
    public string ApiBase { get; set; } = string.Empty;

    /// <summary>
    /// The client id of DocuSign
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The user id of DocuSign
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The account id of DocuSign
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// The authorization server url of DocuSign
    /// </summary>
    public string AuthServer { get; set; } = string.Empty;

    /// <summary>
    /// The listener email of DocuSign
    /// </summary>
    public string ListenEmail { get; set; } = string.Empty;
}