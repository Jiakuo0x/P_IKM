using App.Data.TransportModels;
using Data.Models;

namespace Controllers;

[ApiController]
[Route("api/mapping")]
public class MappingController : ControllerBase
{
    private readonly DbContext _dbContext;
    public MappingController(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IResult GetMappings()
    {
        var templateMappings = _dbContext.Set<TemplateMapping>()
            .AsNoTracking().ToList();
        var result = templateMappings.Select(i => new
        {
            TemplateMappingId = i.Id,
            i.DocuSignTemplateId,
            i.BestSignTemplateId,
            i.ParameterMappings,
        });
        return Results.Ok(result);
    }

    [HttpPost]
    public IResult CreateMapping(TemplateMappingDto dto)
    {
        if (_dbContext.Set<TemplateMapping>().Any(i => i.DocuSignTemplateId == dto.DocuSignTemplateId))
            return Results.BadRequest("The template of the envelope has been exists.");
        var templateMapping = new TemplateMapping
        {
            DocuSignTemplateId = dto.DocuSignTemplateId,
            BestSignTemplateId = dto.BestSignTemplateId,
            ParameterMappings = dto.ParameterMappings.Select(i => new ParameterMapping
            {
                DocuSignDataType = i.DocuSignDataType,
                DocuSignDataName = i.DocuSignDataName,
                BestSignDataName = i.BestSignDataName,
            }).ToList(),
        };
        _dbContext.Set<TemplateMapping>().Add(templateMapping);
        _dbContext.SaveChanges();
        return Results.Ok();
    }

    [HttpPut]
    public IResult ResetMapping([FromBody] TemplateMappingDto dto)
    {
        var templateMapping = _dbContext.Set<TemplateMapping>().SingleOrDefault(i => i.DocuSignTemplateId == dto.DocuSignTemplateId);
        if (templateMapping == null)
            return Results.BadRequest("Not found the template of the envelope.");
        if (templateMapping.BestSignTemplateId != dto.BestSignTemplateId)
            templateMapping.BestSignTemplateId = dto.BestSignTemplateId;
        templateMapping.ParameterMappings = dto.ParameterMappings.Select(i => new ParameterMapping
        {
            DocuSignDataType = i.DocuSignDataType,
            DocuSignDataName = i.DocuSignDataName,
            BestSignDataName = i.BestSignDataName,
        }).ToList();
        _dbContext.SaveChanges();
        return Results.Ok();
    }

    [HttpDelete]
    public IResult DeleteMapping(int templateId)
    {
        var templateMapping = _dbContext.Set<TemplateMapping>().SingleOrDefault(i => i.Id == templateId);
        if (templateMapping == null)
            return Results.BadRequest("Not found the matching template.");
        _dbContext.Remove(templateMapping);
        _dbContext.SaveChanges();
        return Results.Ok();
    }
}