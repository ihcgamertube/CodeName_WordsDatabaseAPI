using MongoDB.Bson;
using MongoDB.Driver;
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

        public void InsertCard(CardDocument card)
        {
            if (card == null)
                return;

            lock (_lock)
            {
                CardDocument possibleExistingId = FindCardAtIndex((uint)card.Id);
                if (possibleExistingId != null)
                    return;

                wordsCollection.InsertOne(card);
            }
        }

        public async Task InsertCardAsync(CardDocument card)
        {
            if (card == null)
                return;

            CardDocument possibleExistingId = await FindCardAtIndexAsync((uint)card.Id);
            if (possibleExistingId != null)
                return;

            await wordsCollection.InsertOneAsync(card);
        }

        #endregion

        #region Remove/Delete

        public bool RemoveWord(string word)
        {
            var wordFilter = Builders<CardDocument>.Filter.Eq("Word", word);

            lock (_lock)
            {
                CardDocument lastCardInDb = FindLastDocument();
                if (lastCardInDb == null)
                    return false;

                if (lastCardInDb.Word == word)
                    return DeleteCardDocument(lastCardInDb);

                CardDocument cardToRemove = wordsCollection.Find(wordFilter).FirstOrDefault();
                if (cardToRemove == null)
                    return false;

                var findOptions = new FindOneAndUpdateOptions<CardDocument>() { ReturnDocument = ReturnDocument.After };
                var updateDefinition = Builders<CardDocument>.Update.Set("Word", lastCardInDb.Word);
                CardDocument updatedDocument = wordsCollection.FindOneAndUpdate(wordFilter, updateDefinition, findOptions);
                if (updatedDocument.Word == lastCardInDb.Word)
                    return DeleteCardDocument(lastCardInDb);

                return false;
            }
        }

        public async Task<bool> RemoveWordAsync(string word)
        {
            var wordFilter = Builders<CardDocument>.Filter.Eq("Word", word);

            CardDocument lastCardInDb = await FindLastDocumentAsync();
            if (lastCardInDb == null)
                return false;

            IAsyncCursor<CardDocument> cardsFound = await wordsCollection.FindAsync(wordFilter);
            CardDocument cardToRemove = cardsFound.FirstOrDefault();

            if (cardToRemove == null)
                return false;

            var findOptions = new FindOneAndUpdateOptions<CardDocument>() { ReturnDocument = ReturnDocument.After };
            var updateDefinition = Builders<CardDocument>.Update.Set("Word", lastCardInDb.Word);
            CardDocument updatedDocument = await wordsCollection.FindOneAndUpdateAsync(wordFilter, updateDefinition, findOptions);
            if (updatedDocument.Word == lastCardInDb.Word)
                return await DeleteCardDocumentAsync(lastCardInDb);

            return false;
        }

        private bool DeleteCardDocument(CardDocument cardDocument)
        {
            if (cardDocument == null)
                return false;

            var wordFilter = Builders<CardDocument>.Filter.Eq("_id", cardDocument.Id);

            lock (_lock)
            {
                CardDocument returnedDocument = wordsCollection.FindOneAndDelete<CardDocument>(wordFilter);
                return returnedDocument.Equals(cardDocument);
            }
        }

        private async Task<bool> DeleteCardDocumentAsync(CardDocument cardDocument)
        {
            if (cardDocument == null)
                return false;

            var wordFilter = Builders<CardDocument>.Filter.Eq("_id", cardDocument.Id);
            CardDocument returnedDocument = await wordsCollection.FindOneAndDeleteAsync<CardDocument>(wordFilter);
            return returnedDocument.Equals(cardDocument);
        }

        #endregion

        #region Randomizer

        public async Task<CardDocument> FindRandomCardAsync()
        {
            long documentsCount = await GetDocumentsCountAsync();

            if (documentsCount == 0)
                return null;

            uint randomWordIndex = RandomNumberGenerator.GenerateRandomNumber((uint)documentsCount);
            return await FindCardAtIndexAsync(randomWordIndex);
        }

        public async Task<CardDocument[]> FindMultipleRandomCardsAsync(uint numberOfRandomCards)
        {
            if (numberOfRandomCards < 1)
                return null;

            long documentsCount = await GetDocumentsCountAsync();
            if (documentsCount < numberOfRandomCards)
                throw new ArgumentException("There Aren't Enough Words in Database.");

            uint[] randomCardIndexes = RandomNumberGenerator.GenerateRandomNumbers((uint)documentsCount, numberOfRandomCards);
            var randomCards = new BlockingCollection<CardDocument>((int)numberOfRandomCards);

            foreach(uint index in randomCardIndexes)
            {
                CardDocument card = await FindCardAtIndexAsync(index);
                if (card != null)
                    randomCards.Add(card);
            }

            // TODO check if better performance possible using parallel
            //Parallel.ForEach(randomCardIndexes, async (index) =>
            //{
            //    CardDocument card = await FindCardAtIndexAsync(index);
            //    if(card != null)
            //        randomCards.Add(card);
            //});

            return randomCards.ToArray();
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
            return await wordsCollection.CountDocumentsAsync(new BsonDocument());
        }

        #endregion

        #region Find

        public CardDocument FindCardAtIndex(uint cardIndex)
        {
            if (cardIndex == 0)
                cardIndex = 1;

            var WordFilter = Builders<CardDocument>.Filter.Eq("_id", cardIndex);

            var card = wordsCollection.FindSync<CardDocument>(WordFilter);
            return card.FirstOrDefault();
        }

        public async Task<CardDocument> FindCardAtIndexAsync(uint cardIndex)
        {
            var WordFilter = Builders<CardDocument>.Filter.Eq("_id", cardIndex);

            var card = await wordsCollection.FindAsync<CardDocument>(WordFilter);
            return card.FirstOrDefault();
        }

        public CardDocument FindLastDocument()
        {
            var sort = Builders<CardDocument>.Sort.Descending("_id");
            lock (_lock)
            {
                CardDocument last = wordsCollection.Find(new BsonDocument()).Sort(sort).Limit(1).FirstOrDefault();
                return last;
            }
        }

        public async Task<CardDocument> FindLastDocumentAsync()
        {
            var sort = Builders<CardDocument>.Sort.Descending("_id");
            var options = new FindOptions<CardDocument>
            {
                Sort = sort
            };

            IAsyncCursor<CardDocument> last = await wordsCollection.FindAsync(new BsonDocument(), options);
            return last.FirstOrDefault();
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
            await mongoClient.DropDatabaseAsync(databaseName);
        }

        #endregion

        public async Task<long> GenerateNewId()
        {
            long documentsCount = await GetDocumentsCountAsync();
            return ++documentsCount;
        }
    }
}
