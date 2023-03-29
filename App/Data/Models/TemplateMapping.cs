namespace Data.Models;

using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;


public class TemplateMapping : EntityBase
{
    public string DocuSignTemplateId { get; set; } = string.Empty;
    public string BestSignTemplateId { get; set; } = string.Empty;
    public string ParameterMappingsString { get; set; } = string.Empty;

    [NotMapped]
    public List<ParameterMapping> ParameterMappings
    {
        get
        {
            return JsonConvert.DeserializeObject<List<ParameterMapping>>(ParameterMappingsString) ?? new();
        }
        set
        {
            ParameterMappingsString = JsonConvert.SerializeObject(value);
        }
    }
}

public class ParameterMapping
{
    public DocuSignDataType DocuSignDataType { get; set; }
    public string DocuSignDataName { get; set; } = string.Empty;
    public string BestSignDataName { get; set; } = string.Empty;
}