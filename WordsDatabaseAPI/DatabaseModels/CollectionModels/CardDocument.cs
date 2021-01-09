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
        public ObjectId Id { get; set; }

        [BsonElement]
        public string Word { get; set; }


        public CardDocument(string word)
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

            Id = ObjectId.GenerateNewId();
            Word = word;
        }

        public override bool Equals(object obj)
        {
            CardDocument cardDocument = (CardDocument)obj;
            return (Id == cardDocument.Id && Word == cardDocument.Word);
        }
    }
}
