using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PowerLinesAnalysisService.Data;
using PowerLinesAnalysisService.Models;

namespace PowerLinesAnalysisService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public ResultController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public ActionResult<IEnumerable<Result>> Get()
        {
            return dbContext.Results;
        }
    }
}
