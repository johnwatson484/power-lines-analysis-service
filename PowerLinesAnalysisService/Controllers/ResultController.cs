using Microsoft.AspNetCore.Mvc;
using PowerLinesAnalysisService.Data;
using PowerLinesAnalysisService.Models;
using Microsoft.EntityFrameworkCore;

namespace PowerLinesAnalysisService.Controllers;

[ApiController]
[Route("[controller]")]
public class ResultController(ApplicationDbContext dbContext) : ControllerBase
{
    private readonly ApplicationDbContext dbContext = dbContext;

    [Route("[action]")]
    [HttpGet]
    public ActionResult<LastResultDate> LastResultDate()
    {
        var date = dbContext.Results.AsNoTracking().OrderByDescending(x => x.Created).FirstOrDefault()?.Created;

        return new LastResultDate(date);
    }
}
