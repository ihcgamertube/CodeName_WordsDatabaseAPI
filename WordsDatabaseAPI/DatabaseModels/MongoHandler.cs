using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Linq;
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

        public void InsertCard(CardDocument card)
        {
            if (card.Word == "" || card.Word.Contains(" "))
                return;

            lock(_lock)
            {
                wordsCollection.InsertOne(card);
            }
        }

        public async void InsertCardAsync(CardDocument card)
        {
            if (card.Word == "" || card.Word.Contains(" "))
                return;

            await wordsCollection.InsertOneAsync(card);
        }

        public bool RemoveWord(string word)
        {
            var wordFilter = Builders<CardDocument>.Filter.Eq("Word", word);
            lock(_lock)
            {
                CardDocument removedCard = wordsCollection.FindOneAndDeleteAsync(wordFilter).Result;
                return (removedCard != null);
            }
        }

        public async Task<bool> RemoveWordAsync(string word)
        {
            var wordFilter = Builders<CardDocument>.Filter.Eq("Word", word);
            CardDocument removedCard = await wordsCollection.FindOneAndDeleteAsync(wordFilter);
            return (removedCard != null);
        }

        public async Task<CardDocument> FindRandomCardAsync()
        {
            long documentsCount = await GetDocumentsCountAsync();

            uint randomWordIndex = RandomNumberGenerator.GenerateRandomNumber((uint)documentsCount);
            return await FindCardAtIndexAsync(randomWordIndex);
        }

        public async Task<CardDocument[]> FindMultipleRandomCardsAsync(uint numberOfRandomCards)
        {
            if (numberOfRandomCards < 1)
                throw new ArgumentException("No Words Requested.");

            long documentsCount = await GetDocumentsCountAsync();
            if (documentsCount < numberOfRandomCards)
                throw new ArgumentException("There Aren't Enough Words in Database.");

            uint[] randomCardIndexes = RandomNumberGenerator.GetRandomNumbers((uint)documentsCount, numberOfRandomCards);
            var randomCards = new BlockingCollection<CardDocument>((int)numberOfRandomCards);
            Parallel.ForEach(randomCardIndexes, async (index) =>
            {
                CardDocument card = await FindCardAtIndexAsync(index);
                if(card != null)
                    randomCards.Add(card);
            });

            return randomCards.ToArray();
        }

        public long GetDocumentsCount()
        {
            lock(_lock)
            {
                return wordsCollection.CountDocumentsAsync(new BsonDocument()).Result;
            }
        }

        public async Task<long> GetDocumentsCountAsync()
        {
            return await wordsCollection.CountDocumentsAsync(new BsonDocument());
        }

        public async Task<CardDocument> FindCardAtIndexAsync(uint cardIndex)
        {
            long documentsCount = await wordsCollection.CountDocumentsAsync(new BsonDocument());
            if (documentsCount > 0)
            {
                var WordFilter = Builders<CardDocument>.Filter.Eq("_id", cardIndex);

                var card = await wordsCollection.FindAsync<CardDocument>(WordFilter);
                if (card != null)
                    return card.First();
            }

            return null;
        }

        public async Task<long> GenerateNewId()
        {
            long documentsCount = await wordsCollection.CountDocumentsAsync(new BsonDocument());
            return ++documentsCount;
        }

        public void DeleteDatabase(string databaseName)
        {
            lock(_lock)
            {
                mongoClient.DropDatabase(databaseName);
            }
        }

        public async void DeleteDatabaseAsync(string databaseName)
        {
            await mongoClient.DropDatabaseAsync(databaseName);
        }
    }
}
