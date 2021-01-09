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
                CardDocument possibleExistingId = FindCardAtIndex((uint)card.Id);
                if (possibleExistingId != null)
                    return false;

                var WordFilter = Builders<CardDocument>.Filter.Eq("Word", card.Word);
                CardDocument possibleExistingWord = wordsCollection.Find(WordFilter).FirstOrDefault();
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

            CardDocument possibleExistingId = await FindCardAtIndexAsync((uint)card.Id).ConfigureAwait(false);
            if (possibleExistingId != null)
                return false;

            var WordFilter = Builders<CardDocument>.Filter.Eq("Word", card.Word);
            var findQueryResult = await wordsCollection.FindAsync(WordFilter).ConfigureAwait(false);
            CardDocument possibleExistingWord = findQueryResult.FirstOrDefault();
            if (possibleExistingWord != null)
                return false;

            await wordsCollection.InsertOneAsync(card).ConfigureAwait(false);
            return true;
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

            CardDocument lastCardInDb = await FindLastDocumentAsync().ConfigureAwait(false);
            if (lastCardInDb == null)
                return false;

            IAsyncCursor<CardDocument> cardsFound = await wordsCollection.FindAsync(wordFilter).ConfigureAwait(false);
            CardDocument cardToRemove = cardsFound.FirstOrDefault();

            if (cardToRemove == null)
                return false;

            var findOptions = new FindOneAndUpdateOptions<CardDocument>() { ReturnDocument = ReturnDocument.After };
            var updateDefinition = Builders<CardDocument>.Update.Set("Word", lastCardInDb.Word);
            CardDocument updatedDocument = await wordsCollection.FindOneAndUpdateAsync(wordFilter, updateDefinition, findOptions).ConfigureAwait(false);
            if (updatedDocument.Word == lastCardInDb.Word)
                return await DeleteCardDocumentAsync(lastCardInDb).ConfigureAwait(false);

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
            CardDocument returnedDocument = await wordsCollection.FindOneAndDeleteAsync<CardDocument>(wordFilter).ConfigureAwait(false);
            return returnedDocument.Equals(cardDocument);
        }

        #endregion

        #region Randomizer

        public async Task<CardDocument> FindRandomCardAsync()
        {
            long documentsCount = await GetDocumentsCountAsync().ConfigureAwait(false);

            if (documentsCount == 0)
                return null;

            CardDocument randomCard = await wordsCollection.AsQueryable().Sample(1)
                                        .FirstOrDefaultAsync().ConfigureAwait(false);
            return randomCard;
        }

        public async Task<CardDocument[]> FindMultipleRandomCardsAsync(uint numberOfRandomCards)
        {
            if (numberOfRandomCards < 1)
                return null;

            long documentsCount = await GetDocumentsCountAsync().ConfigureAwait(false);
            if (documentsCount < numberOfRandomCards)
                numberOfRandomCards = (uint)documentsCount;

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

            var card = await wordsCollection.FindAsync<CardDocument>(WordFilter).ConfigureAwait(false);
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

            IAsyncCursor<CardDocument> last = await wordsCollection.FindAsync(new BsonDocument(), options).ConfigureAwait(false);
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
            await mongoClient.DropDatabaseAsync(databaseName).ConfigureAwait(false);
        }

        #endregion

        public async Task<long> GenerateNewId()
        {
            long documentsCount = await GetDocumentsCountAsync().ConfigureAwait(false);
            return ++documentsCount;
        }
    }
}
