using System;
using System.Collections.Generic;
using System.Text;

namespace WordsDatabaseAPI.DatabaseModels.ResultModels
{
    public enum RemoveActionResult
    {
        OK = 1,
        WORD_NOT_IN_DATABASE = 2,
        NO_WORDS_IN_DATABASE = 3
    }
}
