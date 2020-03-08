using Microsoft.EntityFrameworkCore;
using PowerLinesAnalysisService.Models;

namespace PowerLinesAnalysisService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Result> Results { get; set; } 
    }
}
