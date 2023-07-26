/*Copyright 2017-2023 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Pay Statement Source Data
    /// </summary>
    [Serializable]
    public class PayStatementSourceData
    {
        /// <summary>
        /// The database ID of the PayStatement
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The Id represented as an int, which eases sorting and comparison.
        /// </summary>
        public int IdNumber { get { return int.Parse(Id); } }

        /// <summary>
        /// The Employee Id of the person who owns this pay statement.
        /// </summary>
        public string EmployeeId { get; private set; }

        /// <summary>
        /// The name of the employee to be printed on the report
        /// </summary>
        public string EmployeeName { get; private set; }

        /// <summary>
        /// The Employee's mailing label.
        /// </summary>
        public IEnumerable<PayStatementAddress> EmployeeMailingLabel { get; private set; }

        /// <summary>
        /// The Employee's SSN or SIN, potentially masked by any privacy 
        /// restrictions set up by the Payroll office.
        /// Marked as virtual so that inheriting classes can apply said privacy settings
        /// </summary>
        public virtual string EmployeeSSN { get; private set; }

        /// <summary>
        /// The Statement Reference ID, also known as the Pay Advice Number.
        /// </summary>
        public string StatementReferenceId { get; private set; }

        /// <summary>
        /// The Reference Id for the Paycheck, also known as the Check Number
        /// </summary>
        public string PaycheckReferenceId { get; private set; }      
            
        /// <summary>
        /// The Date the employee was paid
        /// </summary>                         
        public DateTime PayDate { get; private set; }

        /// <summary>
        /// The end date of the pay period for which this pay statement applies
        /// </summary>
        public DateTime PeriodEndDate { get; private set; }

        /// <summary>
        /// The Gross Pay for the period
        /// </summary>
        public decimal PeriodGrossPay { get; private set; }

        /// <summary>
        /// The Net Pay for the period
        /// </summary>
        public decimal PeriodNetPay { get; private set; }

        /// <summary>
        /// The Gross Pay, year to date.
        /// </summary>
        public decimal YearToDateGrossPay { get; private set; }

        /// <summary>
        /// The Net Pay, year to date.
        /// </summary>
        public decimal YearToDateNetPay { get; private set; }

        /// <summary>
        /// The list of direct deposits made to the employee's various bank accounts.
        /// </summary>
        public List<PayStatementSourceBankDeposit> SourceBankDeposits { get; private set; }

        /// <summary>
        /// The reference key is a combination of the StatementReferenceId and the PaycheckReferenceId.
        /// It can be used to join a PayrollRegisterEntry to a Pay Statement.
        /// </summary>
        public string ReferenceKey
        {
            get
            {
                var stmtId = StatementReferenceId ?? string.Empty;
                var checkId = PaycheckReferenceId ?? string.Empty;
                return string.Format("{0}*{1}", stmtId, checkId);
            }
        }

        /// <summary>
        /// The comments 
        /// </summary>
        public string Comments { get; private set; }

        /// <summary>
        /// Total taxes for the period
        /// </summary>
        public decimal PeriodTotalTaxes { get; private set; }

        /// <summary>
        /// Total benefits and deductions for the period
        /// </summary>
        public decimal PeriodTotalBenDeds { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employeeId"></param>
        /// <param name="employeeName"></param>
        /// <param name="employeeSsn"></param>
        /// <param name="employeeMailingLabel"></param>
        /// <param name="paycheckReferenceId"></param>
        /// <param name="statementReferenceId"></param>
        /// <param name="payDate"></param>
        /// <param name="periodEndDate"></param>
        /// <param name="periodGrossPay"></param>
        /// <param name="periodNetPay"></param>
        /// <param name="yearToDateGrossPay"></param>
        /// <param name="yearToDateNetPay"></param>
        /// <param name="comments"></param>
        /// <param name="periodTotalTaxes"></param>
        /// <param name="periodTotalBenDeds"></param>
        public PayStatementSourceData(
            string id, 
            string employeeId, 
            string employeeName,
            string employeeSsn,
            IEnumerable<PayStatementAddress> employeeMailingLabel,
            string paycheckReferenceId,
            string statementReferenceId, 
            DateTime payDate, 
            DateTime periodEndDate,
            decimal periodGrossPay,           
            decimal periodNetPay,
            decimal yearToDateGrossPay,
            decimal yearToDateNetPay,
            string comments,
            decimal periodTotalTaxes,
            decimal periodTotalBenDeds
        )
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }            
            if (string.IsNullOrEmpty(paycheckReferenceId) && string.IsNullOrEmpty(statementReferenceId))
            {
                throw new ArgumentException("paycheckReferenceId or payStatementReferenceId is required");
            }

            Id = id;
            EmployeeId = employeeId;
            EmployeeName = employeeName; //not checking for null or empty on name on purpose
            EmployeeSSN = employeeSsn; //not checking for null or empty on ssn on purpose
            EmployeeMailingLabel = employeeMailingLabel != null ? employeeMailingLabel : new List<PayStatementAddress>();
            PayDate = payDate;
            PeriodEndDate = periodEndDate;
            PaycheckReferenceId = paycheckReferenceId;
            StatementReferenceId = statementReferenceId;
            PeriodGrossPay = periodGrossPay;
            PeriodNetPay = periodNetPay;
            YearToDateGrossPay = yearToDateGrossPay;
            YearToDateNetPay = yearToDateNetPay;
            SourceBankDeposits = new List<PayStatementSourceBankDeposit>();
            Comments = comments;
            PeriodTotalTaxes = periodTotalTaxes;
            PeriodTotalBenDeds = periodTotalBenDeds;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            return Id == ((PayStatementSourceData)obj).Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
