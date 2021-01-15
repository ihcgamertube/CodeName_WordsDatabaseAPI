using System;
using System.Collections.Generic;
using System.Text;

namespace WordsDatabaseAPI.DatabaseModels.ResultModels
{
    public enum UpdateActionResult
    {
        EXISTING_WORD_NOT_IN_DATABASE = 0,
        OK = 1,
    }
}
