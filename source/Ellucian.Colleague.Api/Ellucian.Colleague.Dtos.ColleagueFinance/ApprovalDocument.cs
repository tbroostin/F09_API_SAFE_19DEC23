// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Approval Document DTO.
    /// </summary>
    public class ApprovalDocument
    {
        /// <summary>
        /// The ID of the approval document.
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
        /// The over budget amount of the document.
        /// </summary>
        public decimal OverBudgetAmount { get; set; }

        /// <summary>
        /// The change date of the document.
        /// </summary>
        public string ChangeDate { get; set; }

        /// <summary>
        /// The change time of the document.
        /// </summary>
        public string ChangeTime { get; set; }

        /// <summary>
        /// The list of line items associated with the document.
        /// </summary>
        public List<ApprovalItem> DocumentItems { get; set; }
    }
}
