using System.ComponentModel.DataAnnotations.Schema;
using Database.Enums;

namespace Database.Models;

public class TemplateMapping : EntityBase
{
    public string DocuSignTemplateId { get; set; } = string.Empty;
    public string BestSignTemplateId { get; set; } = string.Empty;

    public string BestSignConfigurationString { get; set; } = string.Empty;
    public string ParameterMappingsString { get; set; } = string.Empty;

    [NotMapped]
    public BestSignTemplateConfiguration  BestSignConfiguration
    {
        get
        {
            return JsonConvert.DeserializeObject<BestSignTemplateConfiguration>(BestSignConfigurationString) ?? new();
        }
        set
        {
            BestSignConfigurationString = JsonConvert.SerializeObject(value);
        }
    }

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
    public BestSignDataType BestSignDataType { get; set; }
    public string BestSignDataName { get; set; } = string.Empty;
}

public class BestSignTemplateConfiguration
{
    public string DocumentId { get; set; } = string.Empty;
}