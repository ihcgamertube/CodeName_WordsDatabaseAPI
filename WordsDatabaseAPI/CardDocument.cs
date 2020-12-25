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
            foreach (char letter in word)
            {
                if (!char.IsLetter(letter) && !(letter != '-'))
                    throw new ArgumentException("The Word must contain only letters");
            }

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
