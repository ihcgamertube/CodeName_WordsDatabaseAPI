using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;
using WordsDatabaseAPI.DatabaseModels.ResultModels;
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

        public InsertActionResult InsertCard(CardDocument card)
        {
            if (card == null)
                return InsertActionResult.BAD_VALUE;

            lock (_lock)
            {
                CardDocument possibleExistingWord = FindCard(card.Word);
                if (possibleExistingWord != null)
                    return InsertActionResult.EXISTING_WORD;

                wordsCollection.InsertOne(card);
                return InsertActionResult.OK;
            }
        }

        public async Task<InsertActionResult> InsertCardAsync(CardDocument card)
        {
            if (card == null)
                return InsertActionResult.BAD_VALUE;

            CardDocument possibleExistingWord = await FindCardAsync(card.Word);
            if (possibleExistingWord != null)
                return InsertActionResult.EXISTING_WORD;

            await wordsCollection.InsertOneAsync(card).ConfigureAwait(false);
            return InsertActionResult.OK;
        }

        #endregion

        #region Remove/Delete

        public RemoveActionResult RemoveWord(string word)
        {
            var removeFilter = Builders<CardDocument>.Filter.Eq((cardDocument) => cardDocument.Word, word);

            lock (_lock)
            {
                CardDocument removedCard = wordsCollection.FindOneAndDelete(removeFilter);

                if (removedCard != null && removedCard.Word == word)
                    return RemoveActionResult.OK;
                return RemoveActionResult.WORD_NOT_IN_DATABASE;
            }
        }

        public async Task<RemoveActionResult> RemoveWordAsync(string word)
        {
            var removeFilter = Builders<CardDocument>.Filter.Eq((cardDocument) => cardDocument.Word, word);

            CardDocument removedCard = await wordsCollection.FindOneAndDeleteAsync(removeFilter);
            if (removedCard != null && removedCard.Word == word)
                return RemoveActionResult.OK;
            return RemoveActionResult.WORD_NOT_IN_DATABASE;
        }

        #endregion

        #region Update

        public UpdateActionResult UpdateWord(string existingWord, string newWord)
        {
            var filter = Builders<CardDocument>.Filter.Eq((card) => card.Word, existingWord);
            var updateDefinition = Builders<CardDocument>.Update.Set((card) => card.Word, newWord);
            lock(_lock)
            {
                var result = wordsCollection.UpdateOne(filter, updateDefinition);
                return (result.MatchedCount != 0)? UpdateActionResult.OK: UpdateActionResult.EXISTING_WORD_NOT_IN_DATABASE;
            }
        }

        public async Task<UpdateActionResult> UpdateWordAsync(string existingWord, string newWord)
        {
            var filter = Builders<CardDocument>.Filter.Eq((card) => card.Word, existingWord);
            var updateDefinition = Builders<CardDocument>.Update.Set((card) => card.Word, newWord);
            var result = await wordsCollection.UpdateOneAsync(filter, updateDefinition);
            return (result.MatchedCount != 0) ? UpdateActionResult.OK : UpdateActionResult.EXISTING_WORD_NOT_IN_DATABASE;
        }

        #endregion

        #region Randomizer

        public async Task<RandomActionResult> FindRandomCardAsync()
        {
            RandomActionResultReason validation = RandomActionResultReason.FAILED;
            CardDocument randomCard = await wordsCollection.AsQueryable().Sample(1)
                                        .FirstOrDefaultAsync().ConfigureAwait(false);
            if (randomCard == null)
                validation |= RandomActionResultReason.NO_WORDS_IN_DB;
            else
                validation = RandomActionResultReason.OK;

            CardDocument[] cardDocuments = { randomCard };
            return new RandomActionResult() { Reason = validation, Result = cardDocuments };
        }

        public async Task<RandomActionResult> FindMultipleRandomCardsAsync(uint numberOfRandomCards)
        {
            RandomActionResultReason validation = RandomActionResultReason.FAILED;
            if (numberOfRandomCards < 1)
            {
                validation |= RandomActionResultReason.NO_CARDS_REQUESTS;
                return new RandomActionResult() { Reason = validation, Result = null };
            }

            var randomCards = wordsCollection.AsQueryable().Sample(numberOfRandomCards).ToArray();

            if (randomCards.Length == 0)
                validation |= RandomActionResultReason.NO_WORDS_IN_DB;
            else if (randomCards.Length < numberOfRandomCards)
                validation |= RandomActionResultReason.NOT_ENOUGH_WORDS_IN_DB;
            else
                validation = RandomActionResultReason.OK;

            return new RandomActionResult() { Reason = validation, Result = randomCards };
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
