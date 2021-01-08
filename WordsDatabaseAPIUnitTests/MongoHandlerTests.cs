using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using WordsDatabaseAPI.DatabaseModels;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;

namespace WordsDatabaseAPIUnitTests
{
    [TestClass]
    public class MongoHandlerTests
    {
        private MongoHandler mongoHandler;

        [TestInitialize]
        public void MongoHandlerInitiallizer()
        {
            DatabaseInfo info = TestingConsts.DB_INFO;
            mongoHandler = new MongoHandler(info);
            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName); // in case db wasn't cleard previous run
        }

        #region Insert Tests

        [TestMethod]
        public void Should_Succeed_When_AddWordsToDbAsyncAndSync()
        {
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Test").Result;
            mongoHandler.InsertCard(card);
            Assert.IsTrue(mongoHandler.GetDocumentsCount() == 1);

            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Wall").Result;
            mongoHandler.InsertCardAsync(card).Wait();
            Assert.IsTrue(mongoHandler.GetDocumentsCount() == 2);

            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        [TestMethod]
        public void Should_Fail_When_CardDocumentIsInvalid()
        {
            CardDocument card = null;
            mongoHandler.InsertCard(card);
            Assert.IsTrue(mongoHandler.GetDocumentsCount() == 0);
        }

        [TestMethod]
        public void Should_Fail_When_CardDocumentIdAlreadyExists()
        {
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Test").Result;
            mongoHandler.InsertCard(card);
            mongoHandler.InsertCard(card);

            Assert.IsTrue(mongoHandler.GetDocumentsCount() == 1);

            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        #endregion

        #region Remove Tests

        [TestMethod]
        public void Should_FailRemove_When_NoWordsInDB()
        {
            string word = "Shniztel";
            Assert.IsFalse(mongoHandler.RemoveWord(word));
            Assert.IsFalse(mongoHandler.RemoveWordAsync(word).Result);
        }

        [TestMethod]
        public void Should_FailRemove_When_WordNotInDb()
        {
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Test").Result;
            mongoHandler.InsertCard(card);

            string word = "Shniztel";
            Assert.IsFalse(mongoHandler.RemoveWord(word));
            Assert.IsFalse(mongoHandler.RemoveWordAsync(word).Result);

            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        [TestMethod]
        public void Should_RemoveWord_When_WordExistsInDocument()
        {
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Test").Result;
            mongoHandler.InsertCard(card);

            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Screen").Result;
            mongoHandler.InsertCard(card);

            Assert.IsTrue(mongoHandler.RemoveWord("Test") && mongoHandler.GetDocumentsCount() == 1);

            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Test").Result;
            mongoHandler.InsertCard(card);

            Assert.IsTrue(mongoHandler.RemoveWord("Test") && mongoHandler.GetDocumentsCount() == 1);
            
            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        #endregion

        #region Count Tests

        [TestMethod]
        public void Should_Count2Documents_When_Inserted2Documents()
        {
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Test").Result;
            mongoHandler.InsertCard(card);

            CardDocument card2 = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Wall").Result;
            mongoHandler.InsertCard(card2);

            Assert.IsTrue(mongoHandler.GetDocumentsCount() == 2);
            Assert.IsTrue(mongoHandler.GetDocumentsCountAsync().Result == 2);

            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        #endregion

        #region Randomizer Tests

        [TestMethod]
        public void Should_ReturnNull_When_NoWordsInDb()
        {
            Task<CardDocument> task = mongoHandler.FindRandomCardAsync();
            task.Wait();
            Assert.IsNull(task.Result);
        }

        [TestMethod]
        public void Should_GetRandomCard_When_1OrMoreWordsInDb()
        {
            string word = "Test";
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            CardDocument randomCard = mongoHandler.FindRandomCardAsync().Result;
            Assert.IsTrue(randomCard.Word == word);

            word = "Wall";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Random";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Fake";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            randomCard = mongoHandler.FindRandomCardAsync().Result;
            Assert.IsNotNull(randomCard);

            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        [TestMethod]
        public void Should_Fail_When_ZeroCardsRequested()
        {
            Assert.IsNull(mongoHandler.FindMultipleRandomCardsAsync(0).Result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Should_ThrowException_When_NotEnoughWordsInDatabase()
        {
            try
            {
                var cards = mongoHandler.FindMultipleRandomCardsAsync(15).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        [TestMethod]
        public void Should_GenerateMultipleDistictRandomCard_When_RequestValid()
        {
            FillDatabase();

            Task<CardDocument[]> randomCardsTask = mongoHandler.FindMultipleRandomCardsAsync(5);
            randomCardsTask.Wait();

            CardDocument[] randomCards = randomCardsTask.Result;

            foreach (CardDocument randomCard in randomCards)
            {
                Assert.IsNotNull(randomCard);
            }

            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        #endregion

        #region Find Tests

        [TestMethod]
        public void Should_GetNull_When_NoWordsInDb()
        {
            Assert.IsNull(mongoHandler.FindCardAtIndex(5));
            Assert.IsNull(mongoHandler.FindCardAtIndexAsync(5).Result);
        }

        [TestMethod]
        public void Should_GetNull_When_IndexOutOfBounds()
        {
            string word = "Test";
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            Assert.IsNull(mongoHandler.FindCardAtIndex(5));
            Assert.IsNull(mongoHandler.FindCardAtIndexAsync(5).Result);
        }

        [TestMethod]
        public void Should_GetCard_When_IndexValid()
        {
            string word = "Test";
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            Assert.IsNotNull(mongoHandler.FindCardAtIndex(1));
            Assert.IsNotNull(mongoHandler.FindCardAtIndexAsync(1).Result);
        }

        [TestMethod]
        public void Should_GetLastDocument_When_WordsInDb()
        {
            FillDatabase();

            string word = "GamesIsOn";
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            card = mongoHandler.FindLastDocument();
            Assert.IsNotNull(card);
            CardDocument card2 = mongoHandler.FindLastDocumentAsync().Result;
            Assert.IsNotNull(card2);
            Assert.AreEqual(card.Word, word);
            Assert.AreEqual(card2.Word, word);
        }

        [TestMethod]
        public void Should_GetNull_When_DbIsEmpty()
        {
            CardDocument card = mongoHandler.FindLastDocument();
            Assert.IsNull(card);
            CardDocument card2 = mongoHandler.FindLastDocumentAsync().Result;
            Assert.IsNull(card2);
        }

        #endregion

        [TestMethod]
        public void Should_GenerateCorrectId_When_DbIsEmptyOrFilled()
        {
            Assert.IsTrue(mongoHandler.GenerateNewId().Result == 1);

            string word = "Test";
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            Assert.IsTrue(mongoHandler.GenerateNewId().Result == 2);
        }

        private void FillDatabase()
        {
            string word = "Test";
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Wall";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Random";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Fake";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Word";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Filler";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Nini";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Tamir";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Code";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);

            word = "Name";
            card = CardDocument.CreateBasedOnWordAsync(mongoHandler, word).Result;
            mongoHandler.InsertCard(card);
        }

        [TestCleanup]
        public void MongoHandlerDestroyer()
        {
            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }
    }
}
