namespace Lib.BestSign.Dtos;

/// <summary>
/// Bestsign API response body
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Data
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Bestsign API response body
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ApiResponse<T>: ApiResponse
{
    /// <summary>
    /// Data
    /// </summary>
    public new T Data { get; set; } = default!;
}