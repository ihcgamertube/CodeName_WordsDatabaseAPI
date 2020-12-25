using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordsDatabaseAPI;

namespace WordsDatabaseAPIUnitTests
{
    [TestClass]
    public class MongoHandlerTests
    {
        [TestMethod]
        public void Should_Succeed_When_AddWordToDb()
        {
            CardDocument card = CardDocument.CreateBasedOnWordAsync("Test").Result;
            MongoHandler.Instance.InsertCardAsync(card);
        }
    }
}
