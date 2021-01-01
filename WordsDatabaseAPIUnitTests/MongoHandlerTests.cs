using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        }

        [TestCleanup]
        public void MongoHandlerDestroyer()
        {
            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }
    }
}
