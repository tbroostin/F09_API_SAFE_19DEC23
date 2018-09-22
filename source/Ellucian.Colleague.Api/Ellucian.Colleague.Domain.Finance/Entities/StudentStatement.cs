// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;


namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Statement of a student's accounts receivable information for a particular timeframe
    /// </summary>
    [Serializable]
    public class StudentStatement
    {
        private readonly AccountHolder _accountHolder;
        private readonly string _timeframeId;
        private readonly FinanceConfiguration _financeConfiguration;
        private readonly decimal _currentBalance;
        private readonly decimal _totalAmountDue;
        private readonly decimal _totalBalance;
        private readonly decimal _overdueAmount;
        private readonly DateTime _date;
        private readonly DateTime? _dueDate;
        private readonly StudentStatementSummary _accountSummary;
        private readonly DetailedAccountPeriod _accountDetails;
        private readonly List<StudentStatementScheduleItem> _courseSchedule = new List<StudentStatementScheduleItem>();
        private readonly List<DepositDue> _depositsDue = new List<DepositDue>();

        /// <summary>
        /// ID of the student for whom the statement was generated
        /// </summary>
        public string StudentId { get { return _accountHolder.Id; } }

        /// <summary>
        /// Name of the student for whom the statement was generated
        /// </summary>
        public string StudentName { get { return _accountHolder.PreferredName; } }

        /// <summary>
        /// Mailing address of the student for whom the statement was generated
        /// </summary>
        public string StudentAddress { get { return string.Join(Environment.NewLine, _accountHolder.PreferredAddress); } }

        /// <summary>
        /// ID of the timeframe for which the statement was generated
        /// </summary>
        public string TimeframeId { get { return _timeframeId; } }

        /// <summary>
        /// Title of the statement
        /// </summary>
        public string Title { get { return _financeConfiguration.StatementTitle; } }

        /// <summary>
        /// Name of the institution issuing the statement
        /// </summary>
        public string InstitutionName { get { return _financeConfiguration.InstitutionName; } }

        /// <summary>
        /// Mailing address associated with the issuing institution
        /// </summary>
        public string RemittanceAddress { get { return string.Join(Environment.NewLine, _financeConfiguration.RemittanceAddress); } }

        /// <summary>
        /// Message to display on all statements
        /// </summary>
        public string StatementMessage { get { return string.Join(string.Empty, _financeConfiguration.StatementMessage); } }

        /// <summary>
        /// Current accounts receivable balance for the student
        /// </summary>
        public decimal CurrentBalance { get { return _currentBalance; } }

        /// <summary>
        /// Total amount that the student must pay on or before the Due Date
        /// </summary>
        public decimal TotalAmountDue { get { return Math.Max(0, _totalAmountDue); } }

        /// <summary>
        /// Total outstanding accounts receivable balance for the student
        /// </summary>
        public decimal TotalBalance { get { return _totalBalance; } }

        /// <summary>
        /// Date on which the statement was generated
        /// </summary>
        public DateTime Date { get { return _date; } }

        /// <summary>
        /// Date on which the total amount due must be paid
        /// </summary>
        public string DueDate 
        { 
            get 
            {
                if (_totalAmountDue <= 0 || !_dueDate.HasValue)
                {
                    return null;
                }
                //if (_dueDate.Value < DateTime.Today)
                //{
                //    return "Overdue";
                //}
                return _dueDate.Value.ToShortDateString(); 
            } 
        }

        /// <summary>
        /// Summary accounts receivable information for the student for the statement term or period
        /// </summary>
        public StudentStatementSummary AccountSummary { get { return _accountSummary; } }

        /// <summary>
        /// Detailed accounts receivable information for the student for the statement term or period
        /// </summary>
        public DetailedAccountPeriod AccountDetails { get { return _accountDetails; } }

        /// <summary>
        /// Course section information to be shown on the statement
        /// </summary>
        public ReadOnlyCollection<StudentStatementScheduleItem> CourseSchedule { get; private set; }

        /// <summary>
        /// Flag indicating whether or not the student's payment for the statement term or period is past due
        /// </summary>
        public bool Overdue { get { return _dueDate.HasValue && _dueDate.Value < DateTime.Today; } }

        /// <summary>
        /// Enumeration value indicating whether account details are displayed by term or by period
        /// </summary>
        public ActivityDisplay ActivityDisplay { get { return _financeConfiguration.ActivityDisplay; } }

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
        public bool IncludeSchedule { get { return _financeConfiguration.IncludeSchedule; } }

        /// <summary>
        /// Flag indicating whether or not to include the student's account details on the statement
        /// </summary>
        public bool IncludeDetail { get { return _financeConfiguration.IncludeDetail; } }

        /// <summary>
        /// Flag indicating whether or not to include the student's account history on the statement
        /// </summary>
        public bool IncludeHistory { get { return _financeConfiguration.IncludeHistory; } }

        /// <summary>
        /// Institution-defined text to be shown on the last page of the statement
        /// </summary>
        public string DisclosureStatement { get; set; }

        /// <summary>
        /// Amount that is overdue 
        /// </summary>
        public decimal OverdueAmount { get { return _overdueAmount; } }

        /// <summary>
        /// Total amount currently due, equal to the total amount due minus any overdue amounts 
        /// </summary>
        public decimal CurrentAmountDue { get { return Math.Max(0, _totalAmountDue - _overdueAmount); } }

        /// <summary>
        /// Constructor for StudentStatement
        /// </summary>
        /// <param name="accountHolder">The accounts receivable accountholder for whom the statement was generated</param>
        /// <param name="timeframeId">ID of the timeframe for which the statement was generated</param>
        /// <param name="date">Date on which the statement was generated</param>
        /// <param name="dueDate">Date on which the total amount due must be paid</param>
        /// <param name="financeConfiguration">Finance configuration information for the institution</param>
        /// <param name="currentBalance">Current accounts receivable balance for the student</param>
        /// <param name="totalAmountDue">Total amount that the student must pay on or before the Due Date</param>
        /// <param name="totalBalance">Total outstanding accounts receivable balance for the student</param>
        /// <param name="accountSummary">Summary accounts receivable information for the student for the statement term or period</param>
        /// <param name="accountDetails">Detailed accounts receivable information for the student for the statement term or period</param>
        /// <param name="courseSchedule">Course section information to be shown on the statement</param>
        /// <param name="overdueAmount">Amount of the total balance that is overdue</param>
        /// </summary>
        public StudentStatement(AccountHolder accountHolder, string timeframeId, DateTime date, DateTime? dueDate, 
            FinanceConfiguration financeConfiguration, decimal currentBalance, decimal totalAmountDue, decimal totalBalance,
            StudentStatementSummary statementSummary, DetailedAccountPeriod accountDetails, IEnumerable<StudentStatementScheduleItem> courseSchedule, decimal overdueAmount)
        {
            if (accountHolder == null)
            {
                throw new ArgumentNullException("accountHolder", "Accountholder cannot be null.");
            }
            if (string.IsNullOrEmpty(timeframeId))
            {
                throw new ArgumentNullException("timeframeId", "Term/Period ID cannot be null or empty.");
            }
            if (financeConfiguration == null)
            {
                throw new ArgumentNullException("financeConfiguration", "Finance configuration cannot be null.");
            }
            if (statementSummary == null)
            {
                throw new ArgumentNullException("statementSummary", "Statement Summary cannot be null.");
            }
            if (accountDetails == null)
            {
                throw new ArgumentNullException("accountDetails", "Account Details cannot be null.");
            }

            _accountHolder = accountHolder;
            _timeframeId = timeframeId;
            _date = date;
            _dueDate = dueDate;
            _financeConfiguration = financeConfiguration;
            _currentBalance = currentBalance;
            _totalAmountDue = totalAmountDue;
            _totalBalance = totalBalance;
            _accountSummary = statementSummary;
            _accountDetails = accountDetails;
            _overdueAmount = overdueAmount;

            if (courseSchedule != null)
            {
                _courseSchedule.AddRange(courseSchedule);
            }
            CourseSchedule = _courseSchedule.AsReadOnly();
            PreviousBalanceDescription = string.Empty;
            FutureBalanceDescription = string.Empty;
        }
    }
}
