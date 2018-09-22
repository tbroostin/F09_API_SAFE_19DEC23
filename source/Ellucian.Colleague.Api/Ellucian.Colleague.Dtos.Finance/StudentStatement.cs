// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using Ellucian.Colleague.Dtos.Finance.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Statement of a student's accounts receivable information for a particular timeframe
    /// </summary>
    public class StudentStatement
    {
        /// <summary>
        /// ID of the student for whom the statement was generated
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Name of the student for whom the statement was generated
        /// </summary>
        public string StudentName { get; set; }

        /// <summary>
        /// Mailing address of the student for whom the statement was generated
        /// </summary>
        public string StudentAddress { get; set; }

        /// <summary>
        /// ID of the timeframe for which the statement was generated
        /// </summary>
        public string TimeframeId { get; set; }

        /// <summary>
        /// Title of the statement
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Name of the institution issuing the statement
        /// </summary>
        public string InstitutionName { get; set; }

        /// <summary>
        /// Mailing address associated with the issuing institution
        /// </summary>
        public string RemittanceAddress { get; set; }

        /// <summary>
        /// A message to print on the statement
        /// </summary>
        public string StatementMessage { get; set; }

        /// <summary>
        /// Current accounts receivable balance for the student
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Total amount that the student must pay on or before the Due Date
        /// </summary>
        public decimal TotalAmountDue { get; set; }

        /// <summary>
        /// Total outstanding accounts receivable balance for the student
        /// </summary>
        public decimal TotalBalance { get; set; }

        /// <summary>
        /// Date on which the statement was generated
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Date on which the total amount due must be paid
        /// </summary>
        public string DueDate { get; set; }

        /// <summary>
        /// Summary accounts receivable information for the student for the statement term or period
        /// </summary>
        public StudentStatementSummary AccountSummary { get; set; }

        /// <summary>
        /// Detailed accounts receivable information for the student for the statement term or period
        /// </summary>
        public DetailedAccountPeriod AccountDetails { get; set; }

        /// <summary>
        /// Course section information to be shown on the statement
        /// </summary>
        public IEnumerable<StudentStatementScheduleItem> CourseSchedule { get; set; }

        /// <summary>
        /// Flag indicating whether or not the student's payment for the statement term or period is past due
        /// </summary>
        public bool Overdue { get; set; }

        /// <summary>
        /// Enumeration value indicating whether account details are displayed by term or by period
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ActivityDisplay ActivityDisplay { get; set; }

        /// <summary>
        /// Accounts receivable balance for the student prior to the statement term or period
        /// </summary>
        public decimal PreviousBalance { get; set; }

        /// <summary>
        /// Accounts receivable balance for the student beyond the statement term or period
        /// </summary>
        public decimal FutureBalance { get; set; }

        /// <summary>
        /// Non-Term Accounts receivable balance for the student (for Term statements),
        /// or Term accounts receivable balance for the student (for Non-Term statements)
        /// Does not apply to Past/Current/Future statements
        /// </summary>
        public decimal OtherBalance { get; set; }

        /// <summary>
        /// Deposits Due to be shown on the statement
        /// </summary>
        public IEnumerable<StudentStatementDepositDue> DepositsDue { get; set; }

        /// <summary>
        /// Description of the previous balance
        /// </summary>
        public string PreviousBalanceDescription { get; set; }

        /// <summary>
        /// Description of the future balance
        /// </summary>
        public string FutureBalanceDescription { get; set; }

        /// <summary>
        /// Flag indicating whether or not to include the student's schedule on the statement
        /// </summary>
        public bool IncludeSchedule { get; set; }

        /// <summary>
        /// Flag indicating whether or not to include the student's account details on the statement
        /// </summary>
        public bool IncludeDetail { get; set; }

        /// <summary>
        /// Flag indicating whether or not to include the student's account history on the statement
        /// </summary>
        public bool IncludeHistory { get; set; }

        /// <summary>
        /// Institution-defined text to be shown on the last page of the statement
        /// </summary>
        public string DisclosureStatement { get; set; }

        /// <summary>
        /// Amount that is overdue 
        /// </summary>
        public decimal OverdueAmount { get; set; }
        
        /// <summary>
        /// Total amount currently due, equal to the total amount due minus any overdue amounts 
        /// </summary>
        public decimal CurrentAmountDue { get; set; }
    }
}
