namespace Lib.Email;

/// <summary>
/// The configuration of email sender is specified in appsettings.json
/// </summary>
public class Configuration
{
    /// <summary>
    /// The host of the email sender
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// The port of the email sender
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Whether the email sender uses SSL
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// The username of the email sender
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password of the email sender
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The email sender name
    /// </summary>
    public string SenderName { get; set; } = string.Empty;
}