using PowerLinesAnalysisService.Models;

namespace PowerLinesAnalysisService.Analysis;

public interface IAnalysisService
{
    MatchOdds GetMatchOdds(Fixture fixture);
}
