// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// An approved document.
    /// </summary>
    [Serializable]
    public class ApprovedDocument
    {
        /// <summary>
        /// The approved document ID.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

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
        /// The net amount of the document
        /// </summary>
        public decimal NetAmount { get; set; }


        /// <summary>
        /// List of approvers and next approvers for this document.
        /// </summary>
        public List<Approver> DocumentApprovers { get; set; }


        public ApprovedDocument(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("Id", "Document ID is a required field.");
            }
            this.id = id;
        }
    }
}
