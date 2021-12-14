// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Filter criteria for approved documents.
    /// </summary>
    public class ApprovedDocumentFilterCriteria
    {
        /// <summary>
        /// The type of document.
        /// </summary>
        public List<string> DocumentType { get; set; }

        /// <summary>
        /// List of vendor IDs.
        /// </summary>
        public List<string> VendorIds { get; set; }

        /// <summary>
        /// Document date range from.
        /// </summary>
        public DateTime? DocumentDateFrom { get; set; }

        /// <summary>
        /// Document date range to.
        /// </summary>
     
        public DateTime? DocumentDateTo { get; set; }

        /// <summary>
        /// Approval date range from.
        /// </summary>
        public DateTime? ApprovalDateFrom { get; set; }

        /// <summary>
        /// Approval date range to.
        /// </summary>
        public DateTime? ApprovalDateTo { get; set; }
    }
}