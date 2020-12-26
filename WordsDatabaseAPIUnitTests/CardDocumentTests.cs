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
            CardDocument document = new CardDocument(id, word);

            word = "Hi~Bro!";
            document = new CardDocument(id, word);

            word = "3rio!";
            document = new CardDocument(id, word);

            word = "";
            document = new CardDocument(id, word);

            word = "   ";
            document = new CardDocument(id, word);
        }

        [TestMethod]
        public void Should_Succeed_When_WordValid()
        {
            string word = "Test";
            CardDocument document = new CardDocument(1, word);
            Assert.IsNotNull(document);
        }

        [TestMethod]
        public void Should_CreateCardBasedOnWord_When_DatabaseIsValid()
        {
            MongoHandler mongoHandler = new MongoHandler(TestingConsts.DB_INFO);
            CardDocument document = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Test").Result;
            Assert.IsNotNull(document);
            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Should_ThrowArgumentNullException_When_DatabaseIsInvalid()
        {
            MongoHandler mongoHandler = null;

            try
            {
                CardDocument document = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Test").Result;
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }
    }
}
