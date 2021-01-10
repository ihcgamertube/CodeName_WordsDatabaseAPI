using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;

namespace WordsDatabaseAPI.DatabaseModels
{
    public interface IDatabaseHandler
    {
        bool InsertCard(CardDocument card);
        Task<bool> InsertCardAsync(CardDocument card);
        bool RemoveWord(string word);
        Task<bool> RemoveWordAsync(string word);
        public bool UpdateWord(string existingWord, string newWord);
        Task<bool> UpdateWordAsync(string existingWord, string newWord);
        Task<CardDocument> FindRandomCardAsync();
        Task<CardDocument[]> FindMultipleRandomCardsAsync(uint numberOfRandomCards);
        long GetDocumentsCount();
        Task<long> GetDocumentsCountAsync();
        public CardDocument FindCard(string word);
        Task<CardDocument> FindCardAsync(string word);
        void DeleteDatabase(string databaseName);
        Task DeleteDatabaseAsync(string databaseName);
    }
}
