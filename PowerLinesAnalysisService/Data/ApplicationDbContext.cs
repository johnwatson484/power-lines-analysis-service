using Microsoft.EntityFrameworkCore;
using PowerLinesAnalysisService.Models;

namespace PowerLinesAnalysisService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
    public DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Result>()
            .HasIndex(x => new { x.Date, x.HomeTeam, x.AwayTeam }).IsUnique();
    }
}

