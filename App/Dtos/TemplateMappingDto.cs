namespace Dtos;
using Database.Enums;
public class TemplateMappingDto
{
    public string DocuSignTemplateId { get; set; } = string.Empty;
    public string BestSignTemplateId { get; set; } = string.Empty;
    public BestSignTemplateConfigurationDto BestSignConfiguration { get; set; } = new();
    public List<ParameterMappingDto> ParameterMappings { get; set; } = new List<ParameterMappingDto>();
}

public class ParameterMappingDto
{
    public DocuSignDataType DocuSignDataType { get; set; }
    public string DocuSignDataName { get; set; } = string.Empty;
    public string BestSignDataName { get; set; } = string.Empty;
}

public class BestSignTemplateConfigurationDto
{
    public string EnterpriseName { get; set; } = string.Empty;
    public string BusinessLine { get; set; } = string.Empty;

    public string DocumentId { get; set; } = string.Empty;

    public string RoleAId { get; set; } = string.Empty;
    public string RoleAName { get; set; } = string.Empty;
    public string RoleAType { get; set; } = string.Empty;

    public string? RoleBId { get; set; }
    public string? RoleBName { get; set; }
    public string? RoleBType { get; set; }
}