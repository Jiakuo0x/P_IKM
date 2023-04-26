using System.ComponentModel.DataAnnotations.Schema;
using Database.Enums;

namespace Database.Models;

/// <summary>
/// The mapping setting of the template
/// </summary>
public class TemplateMapping : EntityBase
{
    /// <summary>
    /// The DocuSign template ID
    /// </summary>
    public string DocuSignTemplateId { get; set; } = string.Empty;

    /// <summary>
    /// The Bestsign template ID
    /// </summary>
    public string BestSignTemplateId { get; set; } = string.Empty;

    /// <summary>
    /// The Bestsign configuration in JSON format that is saved in the database
    /// </summary>
    public string BestSignConfigurationString { get; set; } = string.Empty;

    /// <summary>
    /// The parameter mappings in JSON format that is saved in the database
    /// </summary>
    public string ParameterMappingsString { get; set; } = string.Empty;

    /// <summary>
    /// The Bestsign configuration deserialized by JSON
    /// </summary>
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

    /// <summary>
    /// The parameter mappings deserialyzed by JSON
    /// </summary>
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

/// <summary>
/// The model of parameter mapping
/// </summary>
public class ParameterMapping
{
    /// <summary>
    /// From data type in DocuSign
    /// </summary>
    public DocuSignDataType DocuSignDataType { get; set; }

    /// <summary>
    /// From data name in DocuSign
    /// </summary>
    public string DocuSignDataName { get; set; } = string.Empty;

    /// <summary>
    /// Target data type in Bestsign
    /// </summary>
    public BestSignDataType BestSignDataType { get; set; }

    /// <summary>
    /// Target data name in Bestsign
    /// </summary>
    public string BestSignDataName { get; set; } = string.Empty;
}

/// <summary>
/// The model of Bestsign template configuration
/// </summary>
public class BestSignTemplateConfiguration
{
    /// <summary>
    /// The document ID in Bestsign template
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;
}