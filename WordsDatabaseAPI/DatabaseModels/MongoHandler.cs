using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

            wordsCollection.InsertOne(card);
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
            CardDocument removedCard = wordsCollection.FindOneAndDeleteAsync(wordFilter).Result;
            return (removedCard != null);
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

            uint[] randomCardIndexes = GetRandomCardIndexes((uint)documentsCount, numberOfRandomCards);
            var randomCards = new BlockingCollection<CardDocument>((int)numberOfRandomCards);
            Parallel.ForEach(randomCardIndexes, async (index) =>
            {
                CardDocument card = await FindCardAtIndexAsync(index);
                if(card != null)
                    randomCards.Add(card);
            });

            return randomCards.ToArray();
        }


        private uint[] GetRandomCardIndexes(uint maxRandomNumber, uint numberOfRandomCards)
        {
            HashSet<uint> indexes = new HashSet<uint>();
            for (int i = 0; i < numberOfRandomCards; i++)
            {
                uint randomCardIndex = 0;

                do
                {
                    randomCardIndex = RandomNumberGenerator.GenerateRandomNumber(maxRandomNumber);
                } while (indexes.Contains(randomCardIndex));

                indexes.Add(randomCardIndex);
            }
            return indexes.ToArray();
        }

        public long GetDocumentsCount()
        {
            return wordsCollection.CountDocumentsAsync(new BsonDocument()).Result;
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
            mongoClient.DropDatabase(databaseName);
        }

        public async void DeleteDatabaseAsync(string databaseName)
        {
            await mongoClient.DropDatabaseAsync(databaseName);
        }
    }
}
