using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WordsDatabaseAPI.DatabaseModels;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;

namespace WordsDatabaseAPIUnitTests
{
    [TestClass]
    public class CardDocumentTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_ThrowArgumentException_When_WordInvalid()
        {
            string word = "Hi Bro!";
            uint id = 1;
            CardDocument document = new CardDocument(word);

            word = "Hi~Bro!";
            document = new CardDocument(word);

            word = "3rio!";
            document = new CardDocument(word);

            word = "";
            document = new CardDocument(word);

            word = "   ";
            document = new CardDocument(word);
        }

        [TestMethod]
        public void Should_Succeed_When_WordValid()
        {
            string word = "Test";
            CardDocument document = new CardDocument(word);
            Assert.IsNotNull(document);
        }
    }
}
