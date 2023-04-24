namespace App.Dtos;

/// <summary>
/// The data transfer object of Bestsign callback
/// </summary>
public class BestSignCallbackDto
{
    /// <summary>
    /// The timestamp of the message
    /// </summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>
    /// The client id of the developer
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The type of the message
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The data of the response
    /// </summary>
    public object? ResponseData { get; set; }
}
