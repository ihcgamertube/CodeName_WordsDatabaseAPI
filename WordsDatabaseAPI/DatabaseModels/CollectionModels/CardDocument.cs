using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WordsDatabaseAPI.DatabaseModels.CollectionModels
{
    public class CardDocument
    {
        [BsonId]
        public ulong Id { get; set; }

        [BsonElement]
        public string Word { get; set; }


        public CardDocument(ulong id, string word)
        {
            if(string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("The Word is empty");

            bool isWordValid = word.All((letter) =>
            {
                bool isLetter = char.IsLetter(letter);
                bool isWhitSpace = char.IsWhiteSpace(letter);
                bool isHyphen = letter == '-';
                return (isLetter && !isWhitSpace) || isHyphen;
            });
            
            if(!isWordValid)
                throw new ArgumentException("The Word must contain only letters");

            Id = id;
            Word = word;
        }

        public static async Task<CardDocument> CreateBasedOnWordAsync(MongoHandler mongoHandler, string word)
        {
            if (mongoHandler == null)
                throw new ArgumentNullException("Mongo Handler is Null");

            long id = await mongoHandler.GenerateNewId();
            return new CardDocument((ulong)id, word);
        }

        public override bool Equals(object obj)
        {
            CardDocument cardDocument = (CardDocument)obj;
            return (Id == cardDocument.Id && Word == cardDocument.Word);
        }
    }
}
