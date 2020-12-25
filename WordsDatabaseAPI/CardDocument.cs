using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Threading.Tasks;

namespace WordsDatabaseAPI
{
    public class CardDocument
    {
        [BsonId]
        public ulong Id { get; set; }

        [BsonElement]
        public string Word { get; set; }


        public CardDocument(ulong id, string word)
        {
            Id = id;
            Word = word;
        }

        public static async Task<CardDocument> CreateBasedOnWordAsync(string word)
        {
            long id = await MongoHandler.Instance.GenerateNewId();
            return new CardDocument((ulong)id, word);
        }
    }
}
