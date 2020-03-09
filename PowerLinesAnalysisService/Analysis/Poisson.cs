using System;

namespace PowerLinesAnalysisService.Analysis
{
    public class Poisson
    {
        const double euler = 2.71828;
        public double GetProbability(int randomVariable, double averageRateOfSuccess)
        {
            return Math.Pow(euler, -2) * Math.Pow(averageRateOfSuccess, randomVariable) / factorial(randomVariable);
        }

        private int factorial(int n)
        {
            int value = 1;
            for (int i = 1; i <= n; i++)
            {
                value *= i;
            }
            return value;
        }
    }
}
