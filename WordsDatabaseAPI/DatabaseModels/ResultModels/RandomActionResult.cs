using System;
using System.Collections.Generic;
using System.Text;
using WordsDatabaseAPI.DatabaseModels.CollectionModels;

namespace WordsDatabaseAPI.DatabaseModels.ResultModels
{
    [Flags]
    public enum RandomActionResultReason
    {
        FAILED = 0,
        OK = 1,
        NO_WORDS_IN_DB = 2,
        NOT_ENOUGH_WORDS_IN_DB = 4,
        NO_CARDS_REQUESTS = 8
    }

    public class RandomActionResult
    {
        public CardDocument[] Result { get; set; }
        public RandomActionResultReason Reason { get; set; }
    }
}
