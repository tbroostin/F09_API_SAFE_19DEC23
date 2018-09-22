// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// These are the available statuses for a Journal Entry
    /// </summary>
    [Serializable]
    public enum JournalEntryStatus
    {
        /// <summary>
        /// The journal entry is finished and posted
        /// </summary>
        Complete,

        /// <summary>
        /// The journal entry is completed but it is awaiting
        /// approvals before it can be posted
        /// </summary>
        NotApproved,

        /// <summary>
        /// The journal entry is not yet completed and has not been posted
        /// </summary>
        Unfinished
    }
}
