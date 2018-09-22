//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Define the Student Award Summary Domain Entity
    /// </summary>
    [Serializable]
    public class StudentAwardSummary
    {
        private readonly string _StudentId;
        private readonly string _AidYear;
        private readonly string _FundType;

        /// <summary>
        /// Student Id for Fafsa Data
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        /// <summary>
        /// Financial Aid year
        /// </summary>
        public string AidYear { get { return _AidYear; } }
        /// <summary>
        /// AwardType which comes from AwardCategories in Colleague
        /// Used to describe Loan, Grant, Scholarship, or Work.
        /// </summary>
        public string AwardType { get; set; }
        /// <summary>
        /// FundSource which is the same as AwardTypes in Colleague
        /// </summary>
        public string FundSource { get; set; }
        /// <summary>
        /// FundType which is the same as Awards in Colleague
        /// </summary>
        public string FundType { get { return _FundType; } }
        /// <summary>
        /// Amount of award which has been offered to the student across all award periods
        /// </summary>
        public decimal? AmountOffered { get; set; }
        /// <summary>
        /// Amount of award which has been accepted by the student across all award periods
        /// </summary>
        public decimal? AmountAccepted { get; set; }
        /// <summary>
        /// Initialization Method
        /// </summary>
        /// <param name="studentId">Student Id for FAFSA data</param>
        /// <param name="awardYear">Award Year to be use in FAFSA data</param>
        public StudentAwardSummary(string studentId, string awardYear, string fundType)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }

            _StudentId = studentId;
            _AidYear = awardYear;
            _FundType = fundType;
        }
        /// <summary>
        /// Override Equals
        /// </summary>
        /// <param name="obj">Fafsa Object to be compared</param>
        /// <returns>True or False if objects are considered equal</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var summary = obj as StudentAwardSummary;

            if (summary.StudentId != this.StudentId || summary.AidYear != this.AidYear || summary.FundType != this.FundType)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Override Get HashCode
        /// </summary>
        /// <returns>HashCode integer</returns>
        public override int GetHashCode()
        {
            return StudentId.GetHashCode() ^ AidYear.GetHashCode() ^ FundType.GetHashCode();
        }
    }
}
