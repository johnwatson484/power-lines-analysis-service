using System.Collections.Generic;
using PowerLinesAnalysisService.Models;
using System.Linq;

namespace PowerLinesAnalysisService.Analysis
{
    public class OddsCalculator
    {
        GoalDistribution goalDistribution;
        MatchOdds matchOdds;

        public OddsCalculator(int fixtureId, GoalDistribution goalDistribution)
        {
            this.goalDistribution = goalDistribution;
            matchOdds = new MatchOdds(fixtureId);
        }

        public MatchOdds GetMatchOdds()
        {
            var homeProbability = goalDistribution.ScoreProbabilities.Where(x => x.Result == 'H').Sum(x => x.Probability);
            matchOdds.Home = ConvertProbabilityToOdds(homeProbability);
            var drawProbability = goalDistribution.ScoreProbabilities.Where(x => x.Result == 'D').Sum(x => x.Probability);
            matchOdds.Draw = ConvertProbabilityToOdds(drawProbability);
            var awayProbability = goalDistribution.ScoreProbabilities.Where(x => x.Result == 'A').Sum(x => x.Probability);
            matchOdds.Away = ConvertProbabilityToOdds(awayProbability);

            matchOdds.HomeGoals = goalDistribution.HomeGoalProbabilities.OrderByDescending(x => x.Probability).First().Goals;
            matchOdds.AwayGoals = goalDistribution.AwayGoalProbabilities.OrderByDescending(x => x.Probability).First().Goals;

            var expectedGoalsProbability = goalDistribution.ScoreProbabilities.Where(x=>x.HomeGoalProbability.Goals == matchOdds.HomeGoals 
                && x.AwayGoalProbability.Goals == matchOdds.AwayGoals).First().Probability;
            matchOdds.ExpectedGoals = ConvertProbabilityToOdds(expectedGoalsProbability);

            return matchOdds;
        }

        private decimal ConvertProbabilityToOdds(decimal probability)
        {
            return decimal.Divide(1, probability);
        }
    }
}
