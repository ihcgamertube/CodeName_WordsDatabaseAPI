using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;

namespace WordsDatabaseAPI.DatabaseModels
{
    public interface IDatabaseHandler
    {
        const string REMOVED_WORD_TEMP = "HOLO-EMPTY";
        void InsertCard(CardDocument card);
        Task InsertCardAsync(CardDocument card);
        bool RemoveWord(string word);
        Task<bool> RemoveWordAsync(string word);
        Task<CardDocument> FindRandomCardAsync();
        Task<CardDocument[]> FindMultipleRandomCardsAsync(uint numberOfRandomCards);
        long GetDocumentsCount();
        Task<long> GetDocumentsCountAsync();
        Task<CardDocument> FindCardAtIndexAsync(uint cardIndex);
        void DeleteDatabase(string databaseName);
        Task DeleteDatabaseAsync(string databaseName);
    }
}
