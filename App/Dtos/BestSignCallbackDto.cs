namespace App.Dtos
{
    public class BestSignCallbackDto
    {
        public string Timestamp { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ResponseData { get; set; }
        public string Result { get; set; } = string.Empty;
    }
}
