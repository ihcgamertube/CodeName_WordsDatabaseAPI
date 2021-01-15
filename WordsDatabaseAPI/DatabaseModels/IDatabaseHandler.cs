using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;
using WordsDatabaseAPI.DatabaseModels.ResultModels;

namespace WordsDatabaseAPI.DatabaseModels
{
    public interface IDatabaseHandler
    {
        InsertActionResult InsertCard(CardDocument card);
        Task<InsertActionResult> InsertCardAsync(CardDocument card);
        RemoveActionResult RemoveWord(string word);
        Task<RemoveActionResult> RemoveWordAsync(string word);
        public UpdateActionResult UpdateWord(string existingWord, string newWord);
        Task<UpdateActionResult> UpdateWordAsync(string existingWord, string newWord);
        Task<RandomActionResult> FindRandomCardAsync();
        Task<RandomActionResult> FindMultipleRandomCardsAsync(uint numberOfRandomCards);
        long GetDocumentsCount();
        Task<long> GetDocumentsCountAsync();
        public CardDocument FindCard(string word);
        Task<CardDocument> FindCardAsync(string word);
        void DeleteDatabase(string databaseName);
        Task DeleteDatabaseAsync(string databaseName);
    }
}
