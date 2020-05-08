using PowerLinesAnalysisService.Extensions;
using System;
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
            CalculateResultOdds();
            CalculateScoreOdds();
            CalculateRecommendations();
            return matchOdds;
        }

        private decimal ConvertProbabilityToOdds(decimal probability)
        {
            return Math.Round(DecimalExtensions.SafeDivide(1, probability), 2);
        }

        private void CalculateResultOdds()
        {
            CalculateResultHomeOdds();
            CalculateResultDrawOdds();
            CalculateResultAwayOdds();
        }

        private void CalculateResultHomeOdds()
        {
            matchOdds.Home = ConvertProbabilityToOdds(GetResultProbability('H'));
        }

        private void CalculateResultDrawOdds()
        {
            matchOdds.Draw = ConvertProbabilityToOdds(GetResultProbability('D'));
        }

        private void CalculateResultAwayOdds()
        {
            matchOdds.Away = ConvertProbabilityToOdds(GetResultProbability('A'));
        }

        private decimal GetResultProbability(char result)
        {
            return goalDistribution.ScoreProbabilities.Where(x => x.Result == result).Sum(x => x.Probability);
        }

        private void CalculateScoreOdds()
        {
            CalculateHomeGoals();
            CalculateAwayGoals();
            CalculateExpectedGoalsOdds();
        }

        private void CalculateHomeGoals()
        {
            matchOdds.HomeGoals = goalDistribution.HomeGoalProbabilities.OrderByDescending(x => x.Probability).First().Goals;
        }

        private void CalculateAwayGoals()
        {
            matchOdds.AwayGoals = goalDistribution.AwayGoalProbabilities.OrderByDescending(x => x.Probability).First().Goals;
        }

        private decimal GetExpectedGoalsProbability()
        {
            return goalDistribution.ScoreProbabilities.Where(x => x.HomeGoalProbability.Goals == matchOdds.HomeGoals
                && x.AwayGoalProbability.Goals == matchOdds.AwayGoals).First().Probability;
        }

        private void CalculateExpectedGoalsOdds()
        {
            matchOdds.ExpectedGoals = ConvertProbabilityToOdds(GetExpectedGoalsProbability());
        }

        private void CalculateRecommendations()
        {
            decimal threshold = 0.55M;
            decimal lowerThreshold = 0.5M;
            var prediction = CalculatePrediction();
            var predictionProbability = GetResultProbability(prediction);
            if (predictionProbability > threshold)
            {
                matchOdds.Recommended = prediction;
            }
            if (predictionProbability > lowerThreshold)
            {
                matchOdds.LowerRecommended = prediction;
            }

        }

        private char CalculatePrediction()
        {
            var homeProbability = GetResultProbability('H');
            var drawProbability = GetResultProbability('D');
            var awayProbability = GetResultProbability('A');

            if (homeProbability > drawProbability && homeProbability > awayProbability)
            {
                return "H";
            }
            if (drawProbability > homeProbability && drawProbability > awayProbability)
            {
                return "D";
            }
            if (awayProbability > homeProbability && awayProbability > drawProbability)
            {
                return "A";
            }
            return "X";
        }
    }
}
