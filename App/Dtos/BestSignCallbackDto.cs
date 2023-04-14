using Newtonsoft.Json.Linq;

namespace App.Dtos
{
    public class BestSignCallbackDto
    {
        public string Timestamp { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public object? ResponseData { get; set; }
    }
}
