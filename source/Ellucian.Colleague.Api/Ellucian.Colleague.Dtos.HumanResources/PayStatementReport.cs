/* Copyright 2017-2022 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO Representing all the data on a Pay Statement
    /// </summary>
    public class PayStatementReport
    {
        /// <summary>
        /// The Report Id. Used to request this report object
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The EmployeeId
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// The name of the employee to be printed on the report
        /// </summary>
        public string EmployeeName { get; set; }

        /// <summary>
        /// The Employee's mailing label.
        /// </summary>
        public List<PayStatementAddress> EmployeeMailingLabel { get; set; }

        /// <summary>
        /// The Employee's SSN or SIN, potentially masked by any privacy 
        /// restrictions set up by the Payroll office
        /// </summary>
        public string EmployeeSSN { get; set; }

        /// <summary>
        /// The title of the employee's primary position during the time period of this pay statement
        /// </summary>
        public string PrimaryPosition { get; set; }

        /// <summary>
        /// The Statement's Reference Id. Used by HR and Payroll offices.
        /// </summary>
        public string StatementReferenceId { get; set; }

        /// <summary>
        /// The Reference Id for the Paycheck. Used by HR and Payroll offices.
        /// </summary>
        public string PaycheckReferenceId { get; set; }

        /// <summary>
        /// The start date of the pay period for which this report applies
        /// </summary>
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// The end date of the pay period for which this report applies.
        /// </summary>
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// The date the paycheck that this statement describes was issued
        /// </summary>
        public DateTime PayDate { get; set; }

        /// <summary>
        /// The Total Gross Pay for the Pay Period
        /// </summary>
        public decimal PeriodGrossPay { get; set; }

        /// <summary>
        /// The Total Net Pay for the Pay Period
        /// </summary>
        public decimal PeriodNetPay { get; set; }

        /// <summary>
        /// The Total Gross Pay (Year to date, as of this statement date)
        /// </summary>
        public decimal YearToDateGrossPay { get; set; }

        /// <summary>
        /// The Total Net Pay (Year to date, as of this statement date)
        /// </summary>
        public decimal YearToDateNetPay { get; set; }

        /// <summary>
        /// The name of the institution to be printed along side the mailing label
        /// </summary>
        public string InstitutionName { get; set; }

        /// <summary>
        /// The institution's mailing label.
        /// </summary>
        public List<PayStatementAddress> InstitutionMailingLabel { get; set; }

        /// <summary>
        /// The employee's federal withholding status based on the filing status of their federal withholding type tax entry.
        /// </summary>
        public string FederalWithholdingStatus { get; set; }

        /// <summary>
        /// The employee's state withholding status based on the filing status of their first state withholding type tax entry.
        /// </summary>
        public string StateWithholdingStatus { get; set; }

        /// <summary>
        /// The number of exemptions taken on the federal withholding type tax entry
        /// </summary>
        public int? FederalExemptions { get; set; }

        /// <summary>
        /// The number of exemptions taken on the first state withholding type tax entry.
        /// </summary>
        public int? StateExemptions { get; set; }

        /// <summary>
        /// The amount of additional tax withheld for federal taxes
        /// </summary>
        public decimal AdditionalFederalWithholding { get; set; }

        /// <summary>
        /// The amount of additonal tax withheld for state taxes
        /// </summary>
        public decimal AdditionalStateWithholding { get; set; }

        /// <summary>
        /// A list of the Earnings Details for this Statement
        /// </summary>
        public List<PayStatementEarnings> Earnings { get; set; }

        /// <summary>
        /// A list of the Deduction Details for this Statement
        /// </summary>
        public List<PayStatementDeduction> Deductions { get; set; }

        /// <summary>
        /// A list of the Deposit Details for this Statement
        /// </summary>
        public List<PayStatementBankDeposit> Deposits { get; set; }

        /// <summary>
        /// A list of Leave Deatils for this Statement
        /// </summary>
        public List<PayStatementLeave> Leave { get; set; }

        /// <summary>
        /// A list of Taxable Benefit Details for this Statement
        /// </summary>
        public List<PayStatementTaxableBenefit> TaxableBenefits { get; set; }

        /// <summary>
        /// Comments specified by the payroll office
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Boolean to indicate if this record is using the 2020 W4 calculation rules.
        /// </summary>
        public bool Apply2020W4Rules { get; set; }
    }
}
