using WordsDatabaseAPI.DatabaseModels;

namespace WordsDatabaseAPIUnitTests
{
    public static class TestingConsts
    {
        public const string LOCAL_PORT = "27017";
        public const string DATABASE_URL = "mongodb://localhost:" + LOCAL_PORT;
        public const string DATABASE_NAME = "codeNameTestingDb";
        public const string COLLECTION_NAME = "testCollection";

        public static DatabaseInfo DB_INFO = new DatabaseInfo(LOCAL_PORT, DATABASE_URL, DATABASE_NAME, COLLECTION_NAME);
    }
}
