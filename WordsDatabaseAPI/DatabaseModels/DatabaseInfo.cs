using System;
using System.Collections.Generic;
using System.Text;

namespace WordsDatabaseAPI.DatabaseModels
{
    public struct DatabaseInfo
    {
        private const string PORT = "27017";
        private const string DATABASE_URL = "mongodb://localhost:" + PORT;
        private const string DATABASE_NAME = "codeNameDb";
        private const string COLLECTION_NAME = "englishCards";

        public readonly string Port { get; }
        public readonly string DatabaseUrl { get; }
        public readonly string DatabaseName { get;  }
        public readonly string CollectionName { get; }

        public DatabaseInfo(string port = PORT, string databaseUrl = DATABASE_URL,
            string databaseName = DATABASE_NAME, string collectionName = COLLECTION_NAME)
        {
            Port = port;
            DatabaseUrl = databaseUrl;
            DatabaseName = databaseName;
            CollectionName = collectionName;
        }
    }
}
