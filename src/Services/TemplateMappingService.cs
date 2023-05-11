namespace Services;

/// <summary>
/// Template mapping service
/// </summary>
public class TemplateMappingService
{
    private readonly DbContext _db;
    public TemplateMappingService(DbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Retrieve the mapping configuration of the template based on the DocuSign template ID
    /// </summary>
    /// <param name="docuSignId">DocuSign template ID</param>
    /// <returns>The mapping configuration</returns>
    /// <exception cref="Exception">Not found the mappping</exception>
    public TemplateMapping GetMappingByDocuSignId(string docuSignId)
    {
        var result = _db.Set<TemplateMapping>().SingleOrDefault(i => i.DocuSignTemplateId == docuSignId);
        if (result is null)
            throw new Exception($"System Error: Not found the template mapping [{docuSignId}] in the system.");
        return result;
    }
}