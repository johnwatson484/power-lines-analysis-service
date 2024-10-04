namespace PowerLinesAnalysisService.Models;

public class MatchOdds(int fixtureId)
{
    public int FixtureId { get; set; } = fixtureId;
    public decimal Home { get; set; }
    public decimal Draw { get; set; }
    public decimal Away { get; set; }
    public int HomeGoals { get; set; }
    public int AwayGoals { get; set; }
    public decimal ExpectedGoals { get; set; }
    public string Recommended { get; set; } = "X";
    public string LowerRecommended { get; set; } = "X";
    public DateTime Calculated { get; set; } = DateTime.UtcNow;
}

