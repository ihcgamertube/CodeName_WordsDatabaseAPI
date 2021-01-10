using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;

namespace WordsDatabaseAPI.DatabaseModels
{
    public interface IDatabaseHandler
    {
        Task<bool> InsertCardAsync(CardDocument card);
        Task<bool> RemoveWordAsync(string word);
        Task<bool> UpdateWordAsync(string existingWord, string newWord);
        Task<CardDocument> FindRandomCardAsync();
        Task<CardDocument[]> FindMultipleRandomCardsAsync(uint numberOfRandomCards);
        Task<long> GetDocumentsCountAsync();
        Task<CardDocument> FindCardAsync(string word);
    }
}
