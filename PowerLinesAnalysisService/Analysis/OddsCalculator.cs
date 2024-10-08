using Microsoft.Extensions.Options;
using PowerLinesAnalysisService.Extensions;
using PowerLinesAnalysisService.Models;

namespace PowerLinesAnalysisService.Analysis;

public class OddsCalculator(int fixtureId, GoalDistribution goalDistribution, ThresholdOptions thresholdOptions)
{
    readonly GoalDistribution goalDistribution = goalDistribution;
    readonly ThresholdOptions thresholdOptions = thresholdOptions;
    readonly MatchOdds matchOdds = new MatchOdds(fixtureId);

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
        return goalDistribution.ScoreProbabilities.First(x => x.HomeGoalProbability.Goals == matchOdds.HomeGoals
            && x.AwayGoalProbability.Goals == matchOdds.AwayGoals).Probability;
    }

    private void CalculateExpectedGoalsOdds()
    {
        matchOdds.ExpectedGoals = ConvertProbabilityToOdds(GetExpectedGoalsProbability());
    }

    private void CalculateRecommendations()
    {
        var prediction = CalculatePrediction();
        var predictionProbability = GetResultProbability(prediction);
        if (predictionProbability > thresholdOptions.Higher)
        {
            matchOdds.Recommended = Char.ToString(prediction);
        }
        if (predictionProbability > thresholdOptions.Lower)
        {
            matchOdds.LowerRecommended = Char.ToString(prediction);
        }
    }

    private char CalculatePrediction()
    {
        var homeProbability = GetResultProbability('H');
        var drawProbability = GetResultProbability('D');
        var awayProbability = GetResultProbability('A');

        if (homeProbability > drawProbability && homeProbability > awayProbability)
        {
            return 'H';
        }
        if (drawProbability > homeProbability && drawProbability > awayProbability)
        {
            return 'D';
        }
        if (awayProbability > homeProbability && awayProbability > drawProbability)
        {
            return 'A';
        }
        return 'X';
    }
}
