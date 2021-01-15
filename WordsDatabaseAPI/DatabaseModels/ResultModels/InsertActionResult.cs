using System;
using System.Collections.Generic;
using System.Text;

namespace WordsDatabaseAPI.DatabaseModels.ResultModels
{
    public enum InsertActionResult
    {
        OK = 0,
        BAD_VALUE = 1,
        EXISTING_WORD = 2,
        UNKNOWN_FAILURE = 4
    }
}
