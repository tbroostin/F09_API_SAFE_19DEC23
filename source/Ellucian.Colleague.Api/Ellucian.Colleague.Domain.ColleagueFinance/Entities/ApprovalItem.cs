// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// An approval document line item.
    /// </summary>
    [Serializable]
    public class ApprovalItem
    {
        /// <summary>
        /// Contains the type of document.
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// Contains the document ID.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// Contains the item ID.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Contains the date of the last change.
        /// </summary>
        public string ChangeDate { get; set; }

        /// <summary>
        /// Contains the time of the last change.
        /// </summary>
        public string ChangeTime { get; set; }
    }
}