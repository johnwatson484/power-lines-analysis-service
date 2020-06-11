using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PowerLinesAnalysisService.Data;
using System.Linq;
using PowerLinesAnalysisService.Models;
using Microsoft.EntityFrameworkCore;

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

        [Route("[action]")]
        public ActionResult<LastResultDate> LastResultDate()
        {
            var date = dbContext.Results.AsNoTracking().OrderByDescending(x => x.Created).FirstOrDefault()?.Created;

            return new LastResultDate(date);
        }
    }
}
