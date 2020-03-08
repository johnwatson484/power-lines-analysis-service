using System;
using PowerLinesAnalysisService.Models;

namespace PowerLinesAnalysisService.Analysis
{
    public interface IAnalysisService
    {
        void GetMatchOdds(int fixtureId);
    }
}
