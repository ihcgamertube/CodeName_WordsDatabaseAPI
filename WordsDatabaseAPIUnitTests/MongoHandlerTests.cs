using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordsDatabaseAPI.DatabaseModels;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;

namespace WordsDatabaseAPIUnitTests
{
    [TestClass]
    public class MongoHandlerTests
    {
        private const string LOCAL_PORT = "27017";
        private const string DATABASE_URL = "mongodb://localhost:" + LOCAL_PORT;
        private const string DATABASE_NAME = "codeNameTestingDb";
        private const string COLLECTION_NAME = "testCollection";

        private MongoHandler mongoHandler;

        [TestInitialize]
        public void MongoHandlerInitiallizer()
        {
            DatabaseInfo info = TestingConsts.DB_INFO;
            mongoHandler = new MongoHandler(info);
        }

        //[TestMethod]
        public void Should_Succeed_When_AddWordToDb()
        {
            CardDocument card = CardDocument.CreateBasedOnWordAsync(mongoHandler, "Test").Result;
            mongoHandler.InsertCardAsync(card);
        }

        [TestCleanup]
        public void MongoHandlerDestroyer()
        {
            mongoHandler.DeleteDatabase(mongoHandler.DbInfo.DatabaseName);
        }
    }
}
