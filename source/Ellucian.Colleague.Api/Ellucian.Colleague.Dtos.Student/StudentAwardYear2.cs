/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates*/

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// A StudentAwardYear2 DTO indicates that a student has financial aid data for the indicated year.
    /// The attributes are specific to the student and award year.
    /// </summary>
    public class StudentAwardYear2
    {
        /// <summary>
        /// The StudentAwardYear Code that identifies the year
        /// Required in PUT
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The student award year description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Colleague PERSON id of this object
        /// Required in PUT
        /// </summary>
        public string StudentId { get; set; }
       
        /// <summary>
        /// Indicates whether the student's application for financial aid for this award year has been reviewed by
        /// a financial aid counselor.
        /// </summary>
        public bool IsApplicationReviewed { get; set; }

        /// <summary>
        /// The Id of the student's assigned financial aid office for this award year
        /// </summary>
        public string FinancialAidOfficeId { get; set; }

        /// <summary>
        /// This is the unique identifier of a pending LoanRequest object that the student has submitted for this award year.
        /// </summary>
        public string PendingLoanRequestId { get; set; }
        
        /// <summary>
        /// This flag indicates whether a student has opted to receive paper copies of financial aid correspondence.
        /// Can be updated via PUT
        /// </summary>
        public bool IsPaperCopyOptionSelected { get; set; }

        /// <summary>
        /// This is the student's estimated cost of attendance for this award year
        /// </summary>
        public decimal? EstimatedCostOfAttendance { get; set; }

        /// <summary>
        /// This is the total amount that the student has been awarded minus denied and rejected awards.
        /// </summary>
        public decimal TotalAwardedAmount { get; set; }

        /// <summary>
        /// Collection of AwardLetterHistoryItem DTOs for the year
        /// </summary>
        public IEnumerable<AwardLetterHistoryItem> AwardLetterHistoryItemsForYear { get; set; }

        /// <summary>
        /// Flag indicating whether to suppress disbursement info display for the year
        /// </summary>
        public bool SuppressDisbursementInfoDisplay { get; set; }
    }
}