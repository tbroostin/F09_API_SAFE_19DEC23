// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This class represents a generic financial document. This class is intended to be extended
    /// by LedgerEntryDocument and AccountsPayablePurchasingDocument.
    /// </summary>
    [Serializable]
    public abstract class BaseFinanceDocument
    {
        /// <summary>
        /// Private system-generated ID.
        /// </summary>
        private readonly string id;

        /// <summary>
        /// This is the public getter for the private ID.
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Private system-generated GUID.
        /// </summary>
        private readonly string guid;

        /// <summary>
        /// This is the public getter for the private GUID.
        /// </summary>
        public string Guid { get { return guid; } }

        /// <summary>
        /// Private variable for the document date.
        /// </summary>
        private readonly DateTime date;

        /// <summary>
        /// Public getter for the private document date.
        /// </summary>
        public DateTime Date { get { return date; } }

        /// <summary>
        /// Document comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// This is the private list of approval information for the document.
        /// </summary>
        private readonly List<Approver> approvers = new List<Approver>();

        /// <summary>
        /// This is the public getter for the private list of approval information for the document.
        /// </summary>
        public ReadOnlyCollection<Approver> Approvers { get; private set; }

        /// <summary>
        /// Create a new base finance document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="date">Document date</param>
        public BaseFinanceDocument(string documentId, DateTime date)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId", "Document ID is a required field.");
            }

            this.id = documentId;
            this.date = date;
            Approvers = this.approvers.AsReadOnly();
        }


        /// <summary>
        /// Create a new base finance document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="guid">GUID</param>
        /// <param name="date">Document date</param>
        public BaseFinanceDocument(string documentId, string guid, DateTime date)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new ArgumentNullException("documentId", "Document ID is a required field.");
            }

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Guid is a required field.");
            }

            this.id = documentId;
            this.guid = guid;
            this.date = date;
            Approvers = this.approvers.AsReadOnly();
        }

        /// <summary>
        /// This method adds an approver to the list of approvers for the document.
        /// </summary>
        /// <param name="approver">Approver object.</param>
        public void AddApprover(Approver approver)
        {
            if (approver == null)
            {
                throw new ArgumentNullException("approver", "Approver cannot be null");
            }

            if (approvers != null && approvers.Where(x => x.ApproverId == approver.ApproverId).Count() == 0)
            {
                approvers.Add(approver);
            }
        }
    }
}
