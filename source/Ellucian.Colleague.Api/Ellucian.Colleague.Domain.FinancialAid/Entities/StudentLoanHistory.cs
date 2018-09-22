//Copyright 2014 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// List of total loans per school code
    /// </summary>
    [Serializable]
    public class StudentLoanHistory
    {
        private readonly string opeId;
        /// <summary>
        /// School's OPE (Office of Postsecondary Education) Id, where the student borrowed money
        /// </summary>
        public string OpeId { get { return opeId; } }

        /// <summary>
        /// Total loan amount the student borrowed from this school
        /// </summary>
        public int TotalLoanAmount { get { return totalLoanAmount; } }
        private int totalLoanAmount;

        /// <summary>
        /// Constructor for StudentLoanHistory
        /// </summary>
        /// <param name="opeId">School's OPE Id</param>
        /// <param name="schoolName">Name of the school</param>
        public StudentLoanHistory(string opeId)
        {
            if (string.IsNullOrEmpty(opeId))
            {
                throw new ArgumentNullException("schoolCode");
            }

            this.opeId = opeId;
            totalLoanAmount = 0;
        }

        public void AddToTotalLoanAmount(int loanAmount)
        {
            totalLoanAmount += loanAmount;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var studentLoanHistory = obj as StudentLoanHistory;

            if (studentLoanHistory.OpeId == this.OpeId)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return OpeId.GetHashCode();
        }
    }
}
