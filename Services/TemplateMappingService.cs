namespace Services;

public class TemplateMappingService
{
    private readonly DbContext _db;
    public TemplateMappingService(DbContext db)
    {
        _db = db;
    }

    public TemplateMapping GetMappingByDocuSignId(string docuSignId)
    {
        var result = _db.Set<TemplateMapping>().SingleOrDefault(i => i.DocuSignTemplateId == docuSignId);
        if (result is null)
            throw new Exception($"System Error: Not found the template mapping [{docuSignId}] in the system.");
        return result;
    }
}