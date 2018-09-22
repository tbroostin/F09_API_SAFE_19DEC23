// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// These are the available types for a Journal Entry
    /// </summary>
    [Serializable]
    public enum JournalEntryType
    {
        /// <summary>
        /// A general journal entry
        /// </summary>
        General,

        /// <summary>
        /// An opening Balance journal entry
        /// </summary>
        OpeningBalance
    }
}
