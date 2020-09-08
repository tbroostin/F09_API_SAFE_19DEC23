// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Approval Item DTO.
    /// </summary>
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
