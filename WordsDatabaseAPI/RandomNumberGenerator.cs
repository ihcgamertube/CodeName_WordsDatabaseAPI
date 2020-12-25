using System;
using System.Collections.Generic;
using System.Text;

namespace WordsDatabaseAPI
{
    public static class RandomNumberGenerator
    {
        private static Random randomizer = new Random();

        public static int GenerateRandomNumber(int maxValue)
        {
            return GenerateRandomNumber(1, maxValue);
        }

        public static int GenerateRandomNumber(int minValue, int maxValue)
        {
            return randomizer.Next(minValue, maxValue);
        }
    }
}
