using System;
using System.Collections.Generic;
using System.Linq;
using PowerLinesAnalysisService.Data;
using PowerLinesAnalysisService.Models;

namespace PowerLinesAnalysisService.Analysis
{
    public class AnalysisService : IAnalysisService
    {
        ApplicationDbContext dbContext;
        const int yearsToAnalyse = 6;
        const int maxGoalsPerGame = 5;
        DateTime startDate;
        List<Result> matches;
        GoalDistribution goalDistribution;
        Poisson poisson;

        public AnalysisService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            goalDistribution = new GoalDistribution();
            poisson = new Poisson();
        }

        public MatchOdds GetMatchOdds(Fixture fixture)
        {
            SetStartDate(fixture.Date);
            SetAnalysisMatches(fixture.Division);

            var totalAverageHomeGoals = GetTotalAverageHomeGoals();
            var totalAverageAwayGoals = GetTotalAverageAwayGoals();
            var totalAverageHomeConceded = totalAverageAwayGoals;
            var totalAverageAwayConceded = totalAverageHomeGoals;

            var averageHomeGoals = GetAverageHomeGoals(fixture.HomeTeam);
            var homeAttackStrength = GetAttackStrength(averageHomeGoals, totalAverageHomeGoals);
            var averageAwayConceded = GetAverageAwayConceded(fixture.AwayTeam);
            var awayDefenceStrength = GetDefenceStrength(averageAwayConceded, totalAverageAwayConceded);
            var homeExpectedGoals = GetExpectedGoals(homeAttackStrength, awayDefenceStrength, totalAverageHomeGoals);

            var averageAwayGoals = GetAverageAwayGoals(fixture.AwayTeam);
            var awayAttackStrength = GetAttackStrength(averageAwayGoals, totalAverageAwayGoals);
            var averageHomeConceded = GetAverageHomeConceded(fixture.HomeTeam);
            var homeDefenceStrength = GetDefenceStrength(averageHomeConceded, totalAverageHomeConceded);
            var awayExpectedGoals = GetExpectedGoals(awayAttackStrength, homeDefenceStrength, totalAverageAwayGoals);

            for(int goals = 0; goals <= maxGoalsPerGame; goals++)
            {
                var homeGoalProbability = new GoalProbability(goals, (decimal)poisson.GetProbability(goals, (double)homeExpectedGoals));
                goalDistribution.HomeGoalProbabilities.Add(homeGoalProbability);
                var awayGoalProbability = new GoalProbability(goals, (decimal)poisson.GetProbability(goals, (double)awayExpectedGoals));
                goalDistribution.AwayGoalProbabilities.Add(awayGoalProbability);
            }

            goalDistribution.CalculateDistribution();


            // calc 1x2

            // calc likely score
            return new MatchOdds();
        }

        private void SetStartDate(DateTime fixtureDate)
        {
            // TODO adjust to august
            startDate = fixtureDate.AddYears(-6).Date;
        }

        private void SetAnalysisMatches(string division)
        {
            matches = dbContext.Results.Where(x => x.Division == division && x.Date >= startDate).ToList();
        }

        private decimal GetTotalAverageHomeGoals()
        {
            return decimal.Divide(matches.Sum(x => x.FullTimeHomeGoals), matches.Count);
        }

        private decimal GetTotalAverageAwayGoals()
        {
            return decimal.Divide(matches.Sum(x => x.FullTimeAwayGoals), matches.Count);
        }

        private decimal GetAverageHomeGoals(string homeTeam)
        {
            var homeMatches = matches.Where(x => x.HomeTeam == homeTeam).ToList();
            return decimal.Divide(homeMatches.Sum(x => x.FullTimeHomeGoals), homeMatches.Count);
        }

        private decimal GetAverageAwayGoals(string awayTeam)
        {
            var awayMatches = matches.Where(x => x.AwayTeam == awayTeam).ToList();
            return decimal.Divide(awayMatches.Sum(x => x.FullTimeAwayGoals), awayMatches.Count);
        }
        
        private decimal GetAverageHomeConceded(string homeTeam)
        {
            var homeMatches = matches.Where(x => x.HomeTeam == homeTeam).ToList();
            return decimal.Divide(homeMatches.Sum(x => x.FullTimeAwayGoals), homeMatches.Count);
        }

        private decimal GetAverageAwayConceded(string awayTeam)
        {
            var awayMatches = matches.Where(x => x.AwayTeam == awayTeam).ToList();
            return decimal.Divide(awayMatches.Sum(x => x.FullTimeHomeGoals), awayMatches.Count);
        }

        private decimal GetAttackStrength(decimal averageGoals, decimal totalAverageGoals)
        {
            return decimal.Divide(averageGoals, totalAverageGoals);
        }

        private decimal GetDefenceStrength(decimal averageConceded, decimal totalAverageConceded)
        {
            return decimal.Divide(averageConceded, totalAverageConceded);
        }

        private decimal GetExpectedGoals(decimal teamAttackStrength, decimal oppositionDefenceStrength, decimal totalAverageGoals)
        {
            return teamAttackStrength * oppositionDefenceStrength * totalAverageGoals;
        }
    }
}
