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
            CardDocument card = new CardDocument("Test");
            mongoHandler.InsertCard(card);
            Assert.IsTrue(mongoHandler.GetDocumentsCount() == 1);

            card = new CardDocument("Wall");
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
            CardDocument card = new CardDocument("Test");
            mongoHandler.InsertCard(card);

            string word = "Shniztel";
            Assert.IsFalse(mongoHandler.RemoveWord(word));
            Assert.IsFalse(mongoHandler.RemoveWordAsync(word).Result);

            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        [TestMethod]
        public void Should_RemoveWord_When_WordExistsInDocument()
        {
            CardDocument card = new CardDocument("Test");
            mongoHandler.InsertCard(card);

            card = new CardDocument("Screen");
            mongoHandler.InsertCard(card);

            Assert.IsTrue(mongoHandler.RemoveWord("Test") && mongoHandler.GetDocumentsCount() == 1);

            card = new CardDocument("Test");
            mongoHandler.InsertCard(card);

            bool removed = mongoHandler.RemoveWordAsync("Test").Result;
            Assert.IsTrue(removed && mongoHandler.GetDocumentsCount() == 1);
            
            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }

        #endregion

        #region Count Tests

        [TestMethod]
        public void Should_Count2Documents_When_Inserted2Documents()
        {
            CardDocument card = new CardDocument("Test");
            mongoHandler.InsertCard(card);

            CardDocument card2 = new CardDocument("Wall");
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
        public void Should_GetRandomCard_When_WordsInDb()
        {
            string word = "Test";
            CardDocument card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            CardDocument randomCard = mongoHandler.FindRandomCardAsync().Result;
            Assert.IsTrue(randomCard.Word == word);

            word = "Wall";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Random";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Fake";
            card = new CardDocument(word);
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
        public void Should_ReturnLessWords_When_NotEnoughWordsInDatabase()
        {
            uint numberOfRandomCards = 15;
            var cards = mongoHandler.FindMultipleRandomCardsAsync(numberOfRandomCards).Result;
            Assert.IsTrue(cards.Length < numberOfRandomCards);
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
            Assert.IsNull(mongoHandler.FindCard("Word"));
            Assert.IsNull(mongoHandler.FindCardAsync("Word").Result);
        }

        [TestMethod]
        public void Should_GetNull_When_WordNotInDb()
        {
            string word = "Test";
            CardDocument card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            Assert.IsNull(mongoHandler.FindCard("Word"));
            Assert.IsNull(mongoHandler.FindCardAsync("Word").Result);
        }

        [TestMethod]
        public void Should_GetCard_When_WordInDb()
        {
            string word = "Test";
            CardDocument card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            Assert.IsNotNull(mongoHandler.FindCard(card.Word));
            Assert.IsNotNull(mongoHandler.FindCardAsync(card.Word).Result);
        }

        #endregion

        private void FillDatabase()
        {
            string word = "Test";
            CardDocument card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Wall";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Random";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Fake";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Word";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Filler";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Nini";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Tamir";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Code";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);

            word = "Name";
            card = new CardDocument(word);
            mongoHandler.InsertCard(card);
        }

        [TestCleanup]
        public void MongoHandlerDestroyer()
        {
            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }
    }
}
