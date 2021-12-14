//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// This class contains lifetime summary data about a student's loans.
    /// </summary>
    [Serializable]
    public class StudentLoanSummary
    {

        /// <summary>
        /// The Colleague PERSON id of the student that this object belongs to.
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        private readonly string _StudentId;

        /// <summary>
        /// Entrance Interview Date for a Subsidized or Unsubsidized Loan. 
        /// If null, student has no subsidized loan entrance interview, or doesn't need one
        /// </summary>
        public DateTime? DirectLoanEntranceInterviewDate { get; set; }

        /// <summary>
        /// Interview date for a GradPLUS loan.
        /// If null, student has no grad plus loan interview, or doesn't need one.
        /// </summary>
        public DateTime? GraduatePlusLoanEntranceInterviewDate { get; set; }

        /// <summary>
        /// Direct Loan (Subsidized or Unsubsidized) Master Promissory Note (MPN) expiration date.
        /// If null, student has no Direct Loan MPN
        /// </summary>
        public DateTime? DirectLoanMpnExpirationDate { get; set; }

        /// <summary>
        /// Direct PLUS or Graduate PLUS Loan Master Promissory Note (MPN) expiration date
        /// If null, student has no Direct PLUS or GradPLUS loan MPN
        /// </summary>
        public DateTime? PlusLoanMpnExpirationDate { get; set; }

        /// <summary>
        /// Placeholder
        /// </summary>
        public DateTime? GraduatePlusLoanMpnExpirationDate { get; set; }

        /// <summary>
        /// Flag that denotes whether the Informed Borrower checklist item is completed.
        /// </summary>
        public List<InformedBorrowerItem> InformedBorrowerItem { get; set; }

        /// <summary>
        /// A list of Plus ASLA Items
        /// </summary>
        public List<PlusLoanItem> PlusLoanItems { get; set; }

        /// <summary>
        /// A List of Plus Applicaiton Items
        /// </summary>
        public List<PlusApplicationItem> PlusApplicationItems { get; set; }

        /// <summary>
        /// A list of Plus MPN Items
        /// </summary>
        public List<PlusMpnItem> PlusMpnItems { get; set; }

        /// <summary>
        /// Aggregate amount of all student loan debt - this amount can be higher than the sum of all 
        /// StudentLoanHistory amounts
        /// </summary>
        public int StudentLoanCombinedTotalAmount { get; set; }
        
        /// <summary>
        /// A ReadOnlyCollection of StudentLoanHistory objects. 
        /// </summary>
        public ReadOnlyCollection<StudentLoanHistory> StudentLoanHistory { get; private set; }
        private readonly List<StudentLoanHistory> studentLoanHistory;

        /// <summary>
        /// StudentLoanSummary constructor requires a studentId
        /// </summary>
        /// <param name="studentId">Student's Person Id this object applies to</param>
        public StudentLoanSummary(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            _StudentId = studentId;

            InformedBorrowerItem = new List<InformedBorrowerItem>();
            PlusLoanItems = new List<PlusLoanItem>();
            PlusApplicationItems = new List<PlusApplicationItem>();
            studentLoanHistory = new List<StudentLoanHistory>();
            StudentLoanHistory = studentLoanHistory.AsReadOnly();
        }

        /// <summary>
        /// Add to or update the student's loan history.
        /// </summary>
        /// <param name="opeId">The school's OPE (Office of Postsecondary Education) Id, where the student borrowed money</param>
        /// <param name="loanAmount">The amount of money borrowed. This amount will be added to any existing amount the student has
        /// already borrowed from this school</param>
        public void AddOrUpdateLoanHistory(string opeId, int loanAmount)
        {
            if (string.IsNullOrEmpty(opeId))
            {
                throw new ArgumentNullException("opeId");
            }
            if (studentLoanHistory.Select(h => h.OpeId).Contains(opeId))
            {
                var history = studentLoanHistory.First(h => h.OpeId == opeId);
                history.AddToTotalLoanAmount(loanAmount);
            }
            else
            {
                var history = new StudentLoanHistory(opeId);
                history.AddToTotalLoanAmount(loanAmount);
                studentLoanHistory.Add(history);
            }
        }

        /// <summary>
        /// Two StudentLoanSummary objects are equal when their studentId attributes are equal
        /// </summary>
        /// <param name="obj">The StudentLoanSummary object to compare to this object</param>
        /// <returns>True if the two object's StudentId attributes are equal. False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var studentLoanSummary = obj as StudentLoanSummary;

            if (studentLoanSummary.StudentId != this.StudentId)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the HashCode of this object based on the StudentId
        /// </summary>
        /// <returns>Unique HashCode identifier</returns>
        public override int GetHashCode()
        {
            return StudentId.GetHashCode();
        }
    }
}
