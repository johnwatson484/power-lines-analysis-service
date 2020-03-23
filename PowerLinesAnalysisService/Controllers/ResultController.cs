using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PowerLinesAnalysisService.Data;
using System.Linq;

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
        public ActionResult<DateTime> LastResultDate()
        {
            return dbContext.Results.Max(x => x.Created);
        }
    }
}
