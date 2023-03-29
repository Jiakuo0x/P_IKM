namespace Lib.BestSign;

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