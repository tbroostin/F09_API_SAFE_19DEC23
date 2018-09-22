// Copyright 2014 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This enumeration contains the available types for a project transaction.
    /// </summary>
    [Serializable]
    public enum GlTransactionType
    {
        /// <summary>
        /// The transaction represents an actual amount
        /// </summary>
        Actual,

        /// <summary>
        /// The transaction represents an budget amount
        /// </summary>
        Budget,

        /// <summary>
        /// The transaction represents an encumbrance amount
        /// </summary>
        Encumbrance,

        /// <summary>
        /// The transaction represents a requisition amount
        /// </summary>
        Requisition
    }
}
