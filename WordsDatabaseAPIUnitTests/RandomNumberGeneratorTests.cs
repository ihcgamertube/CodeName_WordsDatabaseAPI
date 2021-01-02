using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using WordsDatabaseAPI.Utillities;

namespace WordsDatabaseAPIUnitTests
{
    [TestClass]
    public class RandomNumberGeneratorTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_ThrowException_When_MaxValueIsLower()
        {
            RandomNumberGenerator.GenerateRandomNumber(5, 3);
        }

        [TestMethod]
        public void Should_Randomize_When_ValuesValid()
        {
            uint maxValue = 5;
            uint number = RandomNumberGenerator.GenerateRandomNumber(maxValue);
            Assert.IsTrue(number > 0 && number <= maxValue);

            uint lowValue = 0;
            maxValue = 10;
            number = RandomNumberGenerator.GenerateRandomNumber(lowValue, maxValue);
            Assert.IsTrue(number >= lowValue && number <= maxValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_ThrowException_When_RangeIsInvalid()
        {
            uint[] randomNumbers = RandomNumberGenerator.GenerateRandomNumbers(5, 50);
        }

        [TestMethod]
        public void Should_RandomizeMultipleDistinctNumbers_When_RangeIsValid()
        {
            uint[] randomNumbers = RandomNumberGenerator.GenerateRandomNumbers(50, 5);
            var afterDistinct = randomNumbers.Distinct();
            var afterDistinctCount = afterDistinct.Count();
            Assert.IsTrue(afterDistinctCount == randomNumbers.Count());
        }
    }
}
