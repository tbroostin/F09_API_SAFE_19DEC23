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
	/// Parent's income details
	/// </summary>
	public class ParentsIncome
	{

		/// <summary>
		/// Parent medicaid or supplemental security income
		/// </summary>
		[JsonConverter(typeof(NullableBooleanConverter))]
		[JsonProperty("ssiBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.SSI.BEN", false, DataDescription = "Parents' Medicaid or Supplemental Security Income.", DataMaxLength = 5)]
		public bool? SsiBenefits { get; set; }

		/// <summary>
		/// Parent receives food stamps
		/// </summary>
		[JsonConverter(typeof(NullableBooleanConverter))]
		[JsonProperty("foodStamps", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.FOOD.STAMPS", false, DataDescription = "Parents' food stamps.", DataMaxLength = 5)]
		public bool? FoodStamps { get; set; }

		/// <summary>
		/// Parent free or reduced price school lunch benefits
		/// </summary>
		[JsonConverter(typeof(NullableBooleanConverter))]
		[JsonProperty("lunchBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.LUNCH.BEN", false, DataDescription = "Parents' free or reduced price school lunch benefits.", DataMaxLength = 5)]
		public bool? LunchBenefits { get; set; }

		/// <summary>
		/// Parent temporary assistance for needy families
		/// </summary>
		[JsonConverter(typeof(NullableBooleanConverter))]
		[JsonProperty("tanfBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.TANF", false, DataDescription = "Parents' temporary assistance for needy families.", DataMaxLength = 5)]
		public bool? TanfBenefits { get; set; }

		/// <summary>
		/// Parent women, infants, and children benefits
		/// </summary>
		[JsonConverter(typeof(NullableBooleanConverter))]
		[JsonProperty("wicBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.WIC", false, DataDescription = "Parents' women, infants, and children benefits.", DataMaxLength = 5)]
		public bool? WicBenefits { get; set; }

		/// <summary>
		/// <see cref="AidApplicationsTaxReturnFiledDto"/> Did the parent's complete a tax return
		/// </summary>
		[JsonProperty("taxReturnFiled", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.TAX.RETURN.FILED", false, DataDescription = "Did the parents complete a tax return?", DataMaxLength = 20)]
		public AidApplicationsTaxReturnFiledDto? TaxReturnFiled { get; set; }

		/// <summary>
		/// <see cref="AidApplicationsTaxFormTypeDto"/> Parent's type of tax form used
		/// </summary>
		[JsonProperty("taxFormType", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.TAX.FORM.TYPE", false, DataDescription = "Parents' type of tax form used.", DataMaxLength = 65)]
		public AidApplicationsTaxFormTypeDto? TaxFormType { get; set; }

		/// <summary>
		/// <see cref="AidApplicationsTaxFilingStatusDto"/> Parent's tax return filing status
		/// </summary>
		[JsonProperty("taxFilingStatus", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.TAX.FILING.STATUS", false, DataDescription = "Parents' tax return filing status.", DataMaxLength = 30)]
		public AidApplicationsTaxFilingStatusDto? TaxFilingStatus { get; set; }

		/// <summary>
		/// Parent's filed a schedule 1
		/// </summary>
		[JsonProperty("schedule1Filed", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.SCHED1", false, DataDescription = "Parents filed a Schedule 1.", DataMaxLength = 10)]
		public AidApplicationsYesOrNoDto? Schedule1Filed { get; set; }

		/// <summary>
		/// Parent is a dislocated worker
		/// </summary>
		[JsonProperty("dislocatedWorker", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.DIS.WORKER", false, DataDescription = "Parents are dislocated workers.", DataMaxLength = 10)]
		public AidApplicationsYesOrNoDto? DislocatedWorker { get; set; }

		/// <summary>
		/// Parent's adjusted gross income from IRS tax form
		/// </summary>
		[JsonProperty("adjustedGrossIncome", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.AGI", false, DataDescription = "Parents' adjusted gross income from IRS tax form.")]
		public int? AdjustedGrossIncome { get; set; }

		/// <summary>
		/// Parent's U.S. taxes paid
		/// </summary>
		[JsonProperty("usTaxPaid", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.US.TAX.PAID", false, DataDescription = "Parents' U.S. taxes paid.")]
		public int? UsTaxPaid { get; set; }

		/// <summary>
		/// Parent 1 income
		/// </summary>
		[JsonProperty("firstParentWorkEarnings", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P1.INCOME", false, DataDescription = "Parent 1 income.")]
		public int? FirstParentWorkEarnings { get; set; }

		/// <summary>
		/// Parent 2 income
		/// </summary>
		[JsonProperty("secondParentWorkEarnings", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P2.INCOME", false, DataDescription = "Parent 2 income.")]
		public int? SecondParentworkEarnings { get; set; }

		/// <summary>
		/// Parent's cash, savings, and checking
		/// </summary>
		[JsonProperty("cashSavingsChecking", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.CASH", false, DataDescription = "Parents' cash, savings, and checking.")]
		public int? CashSavingsChecking { get; set; }

		/// <summary>
		/// Parent's investment net worth
		/// </summary>
		[JsonProperty("investmentNetWorth", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.INV.NET.WORTH", false, DataDescription = "Parents' investment net worth.")]
		public int? InvestmentNetWorth { get; set; }

		/// <summary>
		/// Parent's business and farm net worth
		/// </summary>
		[JsonProperty("businessOrFarmNetWorth", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.BUS.NET.WORTH", false, DataDescription = "Parents' business and farm net worth.")]
		public int? BusinessOrFarmNetWorth { get; set; }

		/// <summary>
		/// Parent's education credits
		/// </summary>
		[JsonProperty("educationalCredits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.EDU.CREDIT", false, DataDescription = "Parents' education credits.")]
		public int? EducationalCredits { get; set; }

		/// <summary>
		/// Parent's child support paid
		/// </summary>
		[JsonProperty("childSupportPaid", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.CHILD.SUPPORT.PD", false, DataDescription = "Parents' child support paid.")]
		public int? ChildSupportPaid { get; set; }

		/// <summary>
		/// Parent's need-based employment
		/// </summary>
		[JsonProperty("needBasedEmployment", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.NEED.BASED.EMP", false, DataDescription = "Parents' need-based employment.")]
		public int? NeedBasedEmployment { get; set; }

		/// <summary>
		/// Parent's grant or scholarship aid
		/// </summary>
		[JsonProperty("grantOrScholarshipAid", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.GRANT.SCHOL.AID", false, DataDescription = "Parents' grant/scholarship aid.")]
		public int? GrantOrScholarshipAid { get; set; }

		/// <summary>
		/// Parent's combat pay
		/// </summary>
		[JsonProperty("combatPay", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.COMBAT.PAY", false, DataDescription = "Parents' combat pay.")]
		public int? CombatPay { get; set; }

		/// <summary>
		/// Parent's co-op earnings
		/// </summary>
		[JsonProperty("coopEarnings", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.CO.OP.EARNINGS", false, DataDescription = "Parents' co-op earnings.")]
		public int? CoopEarnings { get; set; }

		/// <summary>
		/// Parent's pension payments
		/// </summary>
		[JsonProperty("pensionPayments", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.PENSION.PYMTS", false, DataDescription = "Parents' pension payments.")]
		public int? PensionPayments { get; set; }

		/// <summary>
		/// Parent's IRA payments
		/// </summary>
		[JsonProperty("iraPayments", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.IRA.PYMTS", false, DataDescription = "Parents' IRA payments.")]
		public int? IraPayments { get; set; }

		/// <summary>
		/// Parent's child support received
		/// </summary>
		[JsonProperty("childSupportReceived", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.CHILD.SUP.RCVD", false, DataDescription = "Parents' child support received.")]
		public int? ChildSupportReceived { get; set; }

		/// <summary>
		/// Parent's tax exempt interest income
		/// </summary>
		[JsonProperty("taxExemptInterestIncome", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.UNTX.INT.INC", false, DataDescription = "Parents' tax exempt interest income.")]
		public int? TaxExemptInterstIncome { get; set; }

		/// <summary>
		/// Parent's untaxed portions of IRA distributions and pensions
		/// </summary>
		[JsonProperty("untaxedIraAndPensions", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.UNTX.IRA.PEN", false, DataDescription = "Parents' untaxed portions of IRA distributions and pensions.")]
		public int? UntaxedIraAndPensions { get; set; }

		/// <summary>
		/// Parent's military or clergy allowances
		/// </summary>
		[JsonProperty("militaryOrClergyAllowances", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.MIL.CLER.ALLOW", false, DataDescription = "Parents' military/clergy allowances.")]
		public int? MilitaryOrClergyAllowances { get; set; }

		/// <summary>
		/// Parent's veterans noneducation benefits
		/// </summary>
		[JsonProperty("veteranNonEdBenefits", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.VET.NON.ED.BEN", false, DataDescription = "Parents' veterans noneducation benefits.")]
		public int? VeteranNonEdBenefits { get; set; }

		/// <summary>
		/// Parents’ other untaxed income
		/// </summary>
		[JsonProperty("otherUntaxedIncome", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
		[Metadata("FAAA.P.OTHER.UNTX.INC", false, DataDescription = "Parents' other untaxed income.")]
		public int? OtherUntaxedIncome { get; set; }


	}
}