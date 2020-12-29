// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a next approver for a document. 
    /// </summary>
    [Serializable]
    public class NextApprover
    {
        /// <summary>
        /// The next approver ID.
        /// </summary>
        public string NextApproverId { get { return nextApproverId; } }
        private readonly string nextApproverId;

        /// <summary>
        /// The next approver name.
        /// </summary>
        public string NextApproverName { get { return nextApproverName; } }
        private string nextApproverName;

        public string NextApproverPersonId { get; set; }

        /// <summary>
        /// This constructor initializes a next approver domain entity.
        /// </summary>
        /// <param name="nextApproverId">The next approver ID.</param>
        public NextApprover(string nextApproverId)
        {
            if (string.IsNullOrEmpty(nextApproverId))
            {
                throw new ArgumentNullException("nextApproverId", "Next approver ID is a required field.");
            }

            this.nextApproverId = nextApproverId;
        }

        /// <summary>
        /// Set the next approver name. If the name is null/blank, then use the next approver ID.
        /// </summary>
        /// <param name="name">Next approver name.</param>
        public void SetNextApproverName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                nextApproverName = nextApproverId;
            }
            else
            {
                nextApproverName = name;
            }
        }
    }
}
