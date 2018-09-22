// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is an approver for a document. If there is an approval date, then
    /// the approver has approved the document.
    /// </summary>
    [Serializable]
    public class Approver
    {
        /// <summary>
        /// Private variable for the approval ID.
        /// </summary>
        private readonly string approverId;

        /// <summary>
        /// Public getter for the private approval ID.
        /// </summary>
        public string ApproverId { get { return approverId; } }

        /// <summary>
        /// Private variable for the approval name.
        /// </summary>
        private string approverName;

        /// <summary>
        /// Public getter for the private approval name.
        /// </summary>
        public string ApprovalName { get { return approverName; } }

        /// <summary>
        /// This is the date of the approval.
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// This constructor initializes a approver domain entity.
        /// </summary>
        /// <param name="approvalName">This is the approver name</param>
        public Approver(string approverId)
        {
            if (string.IsNullOrEmpty(approverId))
            {
                throw new ArgumentNullException("approverId", "Approver ID is a required field.");
            }

            this.approverId = approverId;
        }

        /// <summary>
        /// Set the approval name. If the name is null/blank, then use the approver ID.
        /// </summary>
        /// <param name="name">Approver name</param>
        public void SetApprovalName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                approverName = approverId;
            }
            else
            {
                approverName = name;
            }
        }
    }
}
