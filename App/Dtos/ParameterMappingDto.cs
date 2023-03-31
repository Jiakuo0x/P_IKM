using Database.Enums;

namespace Dtos
{
    public class ParameterMappingDto
    {
        public DocuSignDataType DocuSignDataType { get; set; }
        public string DocuSignDataName { get; set; } = string.Empty;
        public string BestSignDataName { get; set; } = string.Empty;
    }
}
