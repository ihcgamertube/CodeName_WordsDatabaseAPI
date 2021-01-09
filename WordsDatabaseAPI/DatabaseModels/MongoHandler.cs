using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;
using WordsDatabaseAPI.Utillities;

namespace WordsDatabaseAPI.DatabaseModels
{
    public class MongoHandler : IDatabaseHandler
    {
        public readonly DatabaseInfo DbInfo;

        private MongoClient mongoClient;
        private IMongoDatabase mongoDatabase;
        private IMongoCollection<CardDocument> wordsCollection;

        private static readonly object _lock = new Object();

        public MongoHandler(DatabaseInfo dbInfo)
        {
            DbInfo = dbInfo;
            mongoClient = new MongoClient(DbInfo.DatabaseUrl);
            mongoDatabase = mongoClient.GetDatabase(DbInfo.DatabaseName);
            wordsCollection = mongoDatabase.GetCollection<CardDocument>(DbInfo.CollectionName);
        }

        #region Insert

        public bool InsertCard(CardDocument card)
        {
            if (card == null)
                return false;

            lock (_lock)
            {
                CardDocument possibleExistingWord = FindCard(card.Word);
                if (possibleExistingWord != null)
                    return false;

                wordsCollection.InsertOne(card);
                return true;
            }
        }

        public async Task<bool> InsertCardAsync(CardDocument card)
        {
            if (card == null)
                return false;

            CardDocument possibleExistingWord = await FindCardAsync(card.Word);
            if (possibleExistingWord != null)
                return false;

            await wordsCollection.InsertOneAsync(card).ConfigureAwait(false);
            return true;
        }

        #endregion

        #region Remove/Delete

        public bool RemoveWord(string word)
        {
            var removeFilter = Builders<CardDocument>.Filter.Eq((cardDocument) => cardDocument.Word, word);

            lock (_lock)
            {
                CardDocument removedCard = wordsCollection.FindOneAndDelete(removeFilter);

                if (removedCard != null && removedCard.Word == word)
                    return true;
                return false;
            }
        }

        public async Task<bool> RemoveWordAsync(string word)
        {
            var removeFilter = Builders<CardDocument>.Filter.Eq((cardDocument) => cardDocument.Word, word);

            CardDocument removedCard = await wordsCollection.FindOneAndDeleteAsync(removeFilter);
            if (removedCard != null && removedCard.Word == word)
                return true;
            return false;
        }

        #endregion

        #region Randomizer

        public async Task<CardDocument> FindRandomCardAsync()
        {
            CardDocument randomCard = await wordsCollection.AsQueryable().Sample(1)
                                        .FirstOrDefaultAsync().ConfigureAwait(false);
            return randomCard;
        }

        public async Task<CardDocument[]> FindMultipleRandomCardsAsync(uint numberOfRandomCards)
        {
            if (numberOfRandomCards < 1)
                return null;

            var randomCards = wordsCollection.AsQueryable().Sample(numberOfRandomCards).ToArray();
            return randomCards;
        }

        #endregion

        #region Count

        public long GetDocumentsCount()
        {
            lock(_lock)
            {
                return wordsCollection.CountDocuments(new BsonDocument());
            }
        }

        public async Task<long> GetDocumentsCountAsync()
        {
            return await wordsCollection.CountDocumentsAsync(new BsonDocument()).ConfigureAwait(false);
        }

        #endregion

        #region Find

        public CardDocument FindCard(string word)
        {
            var WordFilter = Builders<CardDocument>.Filter.Eq("Word", word);
            var card = wordsCollection.FindSync<CardDocument>(WordFilter);
            return card.FirstOrDefault();
        }

        public async Task<CardDocument> FindCardAsync(string word)
        {
            var WordFilter = Builders<CardDocument>.Filter.Eq("Word", word);
            var card = await wordsCollection.FindAsync<CardDocument>(WordFilter).ConfigureAwait(false);
            return card.FirstOrDefault();
        }

        #endregion

        #region Delete
        public void DeleteDatabase(string databaseName)
        {
            lock (_lock)
            {
                mongoClient.DropDatabase(databaseName);
            }
        }

        public async Task DeleteDatabaseAsync(string databaseName)
        {
            await mongoClient.DropDatabaseAsync(databaseName).ConfigureAwait(false);
        }

        #endregion
    }
}
