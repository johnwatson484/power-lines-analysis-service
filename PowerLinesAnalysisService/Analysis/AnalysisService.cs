using System;
using PowerLinesAnalysisService.Models;

namespace PowerLinesAnalysisService.Analysis
{
    public class AnalysisService : IAnalysisService
    {
        public MatchOdds GetMatchOdds(Fixture fixture)
        {
            return new MatchOdds();
        }
    }
}
