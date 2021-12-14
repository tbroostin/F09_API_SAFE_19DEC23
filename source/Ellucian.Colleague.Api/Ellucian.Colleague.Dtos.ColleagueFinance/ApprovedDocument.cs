// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Approved Document DTO.
    /// </summary>
    public class ApprovedDocument
    {
        /// <summary>
        /// The ID of the approved document.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The number that the document is known by.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// The document current status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The date of the document.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The name of the vendor associated with the document.
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// The net amount of the document.
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// List of approvers for this document.
        /// </summary>
        public List<Approver> DocumentApprovers { get; set; }
    }
}
