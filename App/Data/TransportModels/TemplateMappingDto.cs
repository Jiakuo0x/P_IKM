namespace App.Data.TransportModels
{
    public class TemplateMappingDto
    {
        public string DocuSignTemplateId { get; set; } = string.Empty;
        public string BestSignTemplateId { get; set; } = string.Empty;
        public List<ParameterMappingDto> ParameterMappings { get; set; } = new List<ParameterMappingDto>();
    }
}
