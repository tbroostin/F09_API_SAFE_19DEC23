/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// StudentAwardYear class defines a Student Award Year.
    /// </summary>
    [Serializable]
    public class StudentAwardYear
    {
        private readonly string _code;
        private readonly string _studentId;

        /// <summary>
        /// The StudentId is one of the unique identifiers of this object.
        /// </summary>
        public string StudentId { get { return _studentId; } }

        /// <summary>
        /// The AwardYear Code is one of the unique identifiers of this object.
        /// </summary>
        public string Code { get { return _code; } }

        /// <summary>
        /// The award year description
        /// </summary>
        public string Description
        {
            get
            {
                return CurrentConfiguration != null ? CurrentConfiguration.AwardYearDescription : null;
            }
        }

        /// <summary>
        /// This is the Office object of the student's current office for this award year.
        /// </summary>
        public FinancialAidOffice CurrentOffice { get { return currentOffice; } }
        private FinancialAidOffice currentOffice;

        /// <summary>
        /// This is the Id of the CurrentOffice
        /// </summary>
        public string FinancialAidOfficeId
        {
            get
            {
                return CurrentOffice != null ? CurrentOffice.Id : null;
            }
        }

        /// <summary>
        /// This is the Configuration object of the Current Office that applies to this award year
        /// </summary>
        public FinancialAidConfiguration CurrentConfiguration
        {
            get
            {
                return (CurrentOffice != null && CurrentOffice.Configurations != null) ?
                    CurrentOffice.Configurations.FirstOrDefault(cc => cc.AwardYear == this.Code) :
                    null;
            }
        }

        /// <summary>
        /// This returns true if the Student's current FA office has activated this award year. False, otherwise;
        /// </summary>
        public bool IsActive
        {
            get
            {
                return (CurrentConfiguration != null) ?
                    CurrentConfiguration.IsSelfServiceActive : false;
            }
        }


        public bool IsAwardingActive
        {
            get
            {
                return (CurrentConfiguration != null) ?
                    CurrentConfiguration.IsAwardingActive : false;
            }
        }

        /// <summary>
        /// Indicates whether students are allowed to make awarding changes for a given year.
        /// </summary>
        public bool AreAwardingChangesAllowed
        {
            get
            {
                return (CurrentConfiguration != null) ?
                    CurrentConfiguration.AreAwardChangesAllowed : false;
            }
        }

        /// <summary>
        /// Indicates whether the student needs to complete a profile application for this award year.
        /// </summary>
        public bool NeedsProfileApplication
        {
            get
            {
                return (CurrentConfiguration != null) ?
                    CurrentConfiguration.IsProfileActive :
                    false;
            }
        }

        /// <summary>
        /// Indicates whether or not to display document instances for the year
        /// to the student
        /// </summary>
        public bool SuppressInstanceData
        {
            get
            {
                return (CurrentConfiguration != null) ?
                    CurrentConfiguration.SuppressInstanceData :
                    false;
            }
        }

        /// <summary>
        /// Indicates whether the student's application for financial aid for this award year has been reviewed by
        /// a financial aid counselor.
        /// </summary>
        public bool IsApplicationReviewed { get; set; }

        /// <summary>
        /// Indicates whether a student can request a loan for a given year.
        /// </summary>
        public bool CanRequestLoan
        {
            get
            {
                return (CurrentConfiguration != null) ?
                    CurrentConfiguration.AreLoanRequestsAllowed :
                    false;
            }
        }
        /// <summary>
        /// This flag indicates whether a student has opted to receive paper copies of financial aid correspondence
        /// </summary>
        public bool IsPaperCopyOptionSelected { get; set; }

        /// <summary>
        /// This is the unique identifier of a pending LoanRequest object that the student has submitted for this award year.
        /// </summary>
        public string PendingLoanRequestId { get; set; }

        /// <summary>
        /// This amount is the total of the estimated expenses for this award year. This amount is calculated by assigning 
        /// individual estimated expense items to the student and totaling up the amounts.        
        /// </summary>
        public decimal? TotalEstimatedExpenses { get; set; }

        /// <summary>
        /// This is an adjustment to the estimated expenses for the award year. This amount is usually assigned on a student
        /// by student basis
        /// </summary>
        public decimal? EstimatedExpensesAdjustment { get; set; }

        /// <summary>
        /// This is the student's estimated cost of attendance for this award year, calculated by adding the TotalEstimatedExpenses
        /// and the EstimatedExpensesAdjustment.
        /// </summary>
        public decimal? EstimatedCostOfAttendance
        {
            get
            {
                if (TotalEstimatedExpenses.HasValue && !EstimatedExpensesAdjustment.HasValue)
                {
                    return TotalEstimatedExpenses;
                }
                else if (TotalEstimatedExpenses.HasValue && EstimatedExpensesAdjustment.HasValue)
                {
                    return TotalEstimatedExpenses + EstimatedExpensesAdjustment;
                }
                return null;
            }
        }

        /// <summary>
        /// This is the total amount that the student has been awarded minus denied and rejected awards.
        /// Must be a non-negative number. If a negative value is attempted to be set, zero is set instead
        /// </summary>
        public decimal TotalAwardedAmount
        {
            get { return totalAwardedAmount; }
            set
            {
                totalAwardedAmount = (value < 0) ? 0 : value;
            }
        }
        private decimal totalAwardedAmount;

        /// <summary>
        /// Federally flagged ISIR id
        /// </summary>
        public string FederallyFlaggedIsirId { get; set; }

        /// <summary>
        /// Collection of AwardLetterHistoryItems for the year
        /// </summary>
        public List<AwardLetterHistoryItem> AwardLetterHistoryItemsForYear { get; set; }

        /// <summary>
        /// Flag indicating whether to suppress disbursement info display for the year
        /// </summary>
        public bool SuppressDisbursementInfoDisplay
        {
            get
            {
                if (CurrentConfiguration == null) return true;
                else return CurrentConfiguration.SuppressDisbursementInfoDisplay;
            }

        }

        /// <summary>
        /// Constructor that accepts studentId and awardYearCode
        /// </summary>
        public StudentAwardYear(string studentId, string awardYearCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode");
            }
            _code = awardYearCode;
            _studentId = studentId;
            AwardLetterHistoryItemsForYear = new List<AwardLetterHistoryItem>();
        }

        /// <summary>
        /// Constructor for StudentAwardYear object requires a code 
        /// </summary>
        /// <param name="code">The unique award year code.</param>
        public StudentAwardYear(string studentId, string awardYearCode, FinancialAidOffice currentOffice)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode");
            }
            if (currentOffice == null)
            {
                throw new ArgumentNullException("currentOffice");
            }
            _code = awardYearCode;
            _studentId = studentId;
            this.currentOffice = currentOffice;
            AwardLetterHistoryItemsForYear = new List<AwardLetterHistoryItem>();
        }

        public override string ToString()
        {
            return _code;
        }

        /// <summary>
        /// Two StudentAwardYear objects are equal when their studentIds and awardYear codes are equal
        /// </summary>
        /// <param name="obj">StudentAwardYear object to compare to this StudentAwardYear</param>
        /// <returns>True if the two object's codes are equal. False otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var studentAwardYear = obj as StudentAwardYear;

            if (studentAwardYear.Code != this.Code || studentAwardYear.StudentId != this.StudentId)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the HashCode of this object, computed by getting the HashCode of 
        /// the Code property.
        /// </summary>
        /// <returns>Returns the HashCode of this StudentAwardYear object</returns>
        public override int GetHashCode()
        {
            return this.Code.GetHashCode() ^ this.StudentId.GetHashCode();
        }
    }
}