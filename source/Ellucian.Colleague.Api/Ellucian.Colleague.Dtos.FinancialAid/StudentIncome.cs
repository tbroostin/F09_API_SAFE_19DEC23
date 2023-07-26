using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
      {
      /// <summary>
      /// Student's Income details
      /// </summary>
      public class StudentIncome
            {
            /// <summary>
            /// Student Medical or Supplemental Security Income (SSI) Benefits
            /// </summary>
            [JsonConverter(typeof(NullableBooleanConverter))]
            [JsonProperty("medicaidOrSsiBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.SSI.BEN", DataDescription = "Student Medical or Supplemental Security Income (SSI) benefits.", DataMaxLength = 5)]
            public bool? MedicaidOrSSIBenefits { get; set; }

            /// <summary>
            /// Food Stamps
            /// </summary>
            [JsonConverter(typeof(NullableBooleanConverter))]
            [JsonProperty("foodStamps", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.FOOD.STAMPS", DataDescription = "Student food stamps.", DataMaxLength = 5)]
            public bool? FoodStamps { get; set; }

            /// <summary>
            /// Student Free or Reduced Price School Lunch Benefits
            /// </summary>
            [JsonConverter(typeof(NullableBooleanConverter))]
            [JsonProperty("lunchBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.LUNCH.BEN", DataDescription = "Student free or reduced price school lunch benefits.", DataMaxLength = 5)]
            public bool? LunchBenefits { get; set; }

            /// <summary>
            /// Student TANF Benefits
            /// </summary>
            [JsonConverter(typeof(NullableBooleanConverter))]
            [JsonProperty("tanfBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.TANF", DataDescription = "Student TANF benefits.", DataMaxLength = 5)]
            public bool? TanfBenefits { get; set; }

            /// <summary>
            /// Student WIC Benefits
            /// </summary>
            [JsonConverter(typeof(NullableBooleanConverter))]
            [JsonProperty("wicBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.WIC", DataDescription = "Student WIC benefits.", DataMaxLength = 5)]
            public bool? WicBenefits { get; set; }

            /// <summary>
            /// <see cref="AidApplicationsTaxReturnFiledDto"/> Student's Tax Return Completed?
            /// </summary>
            [JsonProperty("taxReturnFiled", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.TAX.RETURN.FILED", DataDescription = "Student's tax return completed?", DataMaxLength = 20)]
            public AidApplicationsTaxReturnFiledDto? TaxReturnFiled { get; set; }

            /// <summary>
            /// <see cref="AidApplicationsTaxFormTypeDto"/>Student's Type of 2021 Tax Form Used?
            /// </summary>
            [JsonProperty("taxFormType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.TAX.FORM.TYPE", DataDescription = "Student's type of 2021 tax form used?", DataMaxLength = 65)]
            public AidApplicationsTaxFormTypeDto? TaxFormType { get; set; }

            /// <summary>
            /// <see cref="AidApplicationsTaxFilingStatusDto"/>Student's Tax Return Filing Status
            /// </summary>
            [JsonProperty("taxFilingStatus", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.TAX.FILING.STATUS", DataDescription = "Student's tax return filing status.", DataMaxLength = 30)]
            public AidApplicationsTaxFilingStatusDto? TaxFilingStatus { get; set; }

            /// <summary>
            /// Student filed a schedule 1
            /// </summary>
            [JsonProperty("schedule1Filed", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.SCHED1", DataDescription = "Student filed a schedule 1?", DataMaxLength = 10)]
            public AidApplicationsYesOrNoDto? Schedule1Filed { get; set; }

            /// <summary>
            /// Student/Spouse Dislocated Worker
            /// </summary>
            [JsonProperty("dislocatedWorker", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.DISL.WORKER", DataDescription = "Student/spouse dislocated worker.", DataMaxLength = 10)]
            public AidApplicationsYesOrNoDto? DislocatedWorker { get; set; }

            /// <summary>
            /// Student's Adjusted Gross Income from IRS form
            /// </summary>
            [JsonProperty("adjustedGrossIncome", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.AGI", DataDescription = "Student's adjusted gross income from IRS form.")]
            public int? AdjustedGrossIncome { get; set; }

            /// <summary>
            /// Student's U.S. Income tax paid
            /// </summary>
            [JsonProperty("usTaxPaid", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.US.TAX.PD", DataDescription = "Student's U.S. income tax paid.")]
            public int? UsTaxPaid { get; set; }

            /// <summary>
            /// Student' income earned from work
            /// </summary>
            [JsonProperty("workEarnings", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.STUDENT.INC", DataDescription = "Student's income earned from work.")]
            public int? WorkEarnings { get; set; }

            /// <summary>
            /// Spouse's Income Earned from Work
            /// </summary>
            [JsonProperty("spouseWorkEarnings", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.SPOUSE.INC", DataDescription = "Spouse's income earned from work.")]
            public int? SpouseWorkEarnings { get; set; }

            /// <summary>
            /// Student's Cash, Savings, and Checking
            /// </summary>
            [JsonProperty("cashSavingsChecking", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.CASH", DataDescription = "Student's cash, savings, and checking.")]
            public int? CashSavingsChecking { get; set; }

            /// <summary>
            /// Student's Investment Net Worth
            /// </summary>
            [JsonProperty("investmentNetWorth", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.INV.NET.WORTH", DataDescription = "Student's investment net worth.")]
            public int? InvestmentNetWorth { get; set; }

            /// <summary>
            /// Student's Business and/or Investment Farm
            /// </summary>
            [JsonProperty("businessNetWorth", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.BUS.NET.WORTH", DataDescription = "Student's business and/or investment farm")]
            public int? BusinessNetWorth { get; set; }

            /// <summary>
            /// Student's Educational Credits
            /// </summary>
            [JsonProperty("educationalCredit", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.EDU.CREDIT", DataDescription = "Student's educational credits.")]
            public int? EducationalCredit { get; set; }

            /// <summary>
            /// Student's Child Support Paid
            /// </summary>
            [JsonProperty("childSupportPaid", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.CHILD.SUP.PAID", DataDescription = "Student's child support paid.")]
            public int? ChildSupportPaid { get; set; }

            /// <summary>
            /// Student's Need-Based Employment
            /// </summary>
            [JsonProperty("needBasedEmployment", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.NEED.BASED.EMP", DataDescription = "Student's need-based employment.")]
            public int? NeedBasedEmployment { get; set; }

            /// <summary>
            /// Student's Grant/Scholarship Aid
            /// </summary>
            [JsonProperty("grantAndScholarshipAid", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.GRANT.SCHOL.AID", DataDescription = "Student's grant/scholarship aid.")]
            public int? GrantAndScholarshipAid { get; set; }

            /// <summary>
            /// Student's Combat Pay
            /// </summary>
            [JsonProperty("combatPay", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.COMBAT.PAY", DataDescription = "Student's combat pay.")]
            public int? CombatPay { get; set; }

            /// <summary>
            /// Student's Co-op Earnings
            /// </summary>
            [JsonProperty("coopEarnings", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.CO.OP.EARNINGS", DataDescription = "Student's co-op earnings.")]
            public int? CoopEarnings { get; set; }

            /// <summary>
            /// Student's Pension Payments
            /// </summary>
            [JsonProperty("pensionPayments", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.PENSION.PAYMENTS", DataDescription = "Student's pension payments.")]
            public int? PensionPayments { get; set; }

            /// <summary>
            /// Student's IRA Payments
            /// </summary>
            [JsonProperty("iraPayments", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.IRA.PAYMENTS", DataDescription = "Student's IRA payments.")]
            public int? IraPayments { get; set; }

            /// <summary>
            /// Student's Child Support Received
            /// </summary>
            [JsonProperty("childSupportReceived", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.CHILD.SUP.RECV", DataDescription = "Student's child support received.")]
            public int? ChildSupportReceived { get; set; }

            /// <summary>
            /// Student's Tax Exempt Interest Income
            /// </summary>
            [JsonProperty("interestIncome", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.INTEREST.INCOME", DataDescription = "Student's tax exempt interest income.")]
            public int? InterestIncome { get; set; }

            /// <summary>
            /// Student's Untaxed Portions of IRA Distributions and Pensions
            /// </summary>
            [JsonProperty("untaxedIraPension", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.UNTX.IRA.PEN", DataDescription = "Student's untaxed portions of IRA distributions and pensions.")]
            public int? UntaxedIraPension { get; set; }

            /// <summary>
            /// Student's Military/Clergy Allowances
            /// </summary>
            [JsonProperty("militaryClergyAllowance", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.MILITARY.CLERGY.ALLOW", DataDescription = "Student's military/clergy allowances.")]
            public int? MilitaryClergyAllowance { get; set; }

            /// <summary>
            /// Student's Veteran Noneducation Benefits
            /// </summary>
            [JsonProperty("veteranNonEdBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.VET.NON.ED.BEN", DataDescription = "Student's veteran noneducation benefits.")]
            public int? VeteranNonEdBenefits { get; set; }

            /// <summary>
            /// Student's Other Untaxed Income
            /// </summary>
            [JsonProperty("otherUntaxedIncome", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.OTHER.UNTAXED.INC", DataDescription = "Student's other untaxed income.")]
            public int? OtherUntaxedIncome { get; set; }

            /// <summary>
            /// Student's Other Non-Reported Money
            /// </summary>
            [JsonProperty("otherNonReportedMoney", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
            [Metadata("FAAA.S.OTHER.NON.REP.MONEY", DataDescription = "Student's other non-reported money.")]
            public int? OtherNonReportedMoney { get; set; }

            }
      }
