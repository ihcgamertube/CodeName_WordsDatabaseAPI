using System;
using System.Collections.Generic;
using System.Text;

namespace WordsDatabaseAPI.Utillities
{
    public static class RandomNumberGenerator
    {
        private static Random randomizer = new Random();

        public static uint GenerateRandomNumber(uint maxValue)
        {
            return GenerateRandomNumber(1, maxValue);
        }

        public static uint GenerateRandomNumber(uint minValue, uint maxValue)
        {
            if (minValue > int.MaxValue || maxValue > int.MaxValue)
                throw new ArgumentOutOfRangeException("minValue/maxValue is beyond int32 bounds");
            return (uint)randomizer.Next((int)minValue, (int)maxValue);
        }
    }
}
