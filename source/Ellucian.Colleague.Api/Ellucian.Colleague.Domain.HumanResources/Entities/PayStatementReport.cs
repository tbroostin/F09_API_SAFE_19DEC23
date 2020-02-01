/*Copyright 2017-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Pay Statement Report 
    /// </summary>
    [Serializable]
    public class PayStatementReport : PayStatementSourceData
    {

        private PayStatementReportDataContext sourceDataContext;
        private IEnumerable<PayStatementReportDataContext> yearToDateDataContext;
        private PayStatementReferenceDataUtility dataUtility;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceDataContext"></param>
        /// <param name="yearToDateDataContext"></param>
        /// <param name="dataUtility"></param>
        public PayStatementReport(PayStatementReportDataContext sourceDataContext, IEnumerable<PayStatementReportDataContext> yearToDateDataContext, PayStatementReferenceDataUtility dataUtility)
            : base(sourceDataContext.sourceData.Id, sourceDataContext.sourceData.EmployeeId, sourceDataContext.sourceData.EmployeeName, sourceDataContext.sourceData.EmployeeSSN, sourceDataContext.sourceData.EmployeeMailingLabel, sourceDataContext.sourceData.PaycheckReferenceId, sourceDataContext.sourceData.StatementReferenceId, sourceDataContext.sourceData.PayDate, sourceDataContext.sourceData.PeriodEndDate, sourceDataContext.sourceData.PeriodGrossPay, sourceDataContext.sourceData.PeriodNetPay, sourceDataContext.sourceData.YearToDateGrossPay, sourceDataContext.sourceData.YearToDateNetPay, sourceDataContext.sourceData.Comments)
        {
            this.sourceDataContext = sourceDataContext;
            this.yearToDateDataContext = yearToDateDataContext;
            this.dataUtility = dataUtility;
        }

        public override string EmployeeSSN
        {
            get
            {
                if (string.IsNullOrEmpty(base.EmployeeSSN))
                {
                    return string.Empty;
                }
                switch (dataUtility.Configuration.SocialSecurityNumberDisplay)
                {
                    case SSNDisplay.Full:
                        return base.EmployeeSSN;
                    case SSNDisplay.LastFour:
                        if (base.EmployeeSSN.Length < 4)
                            return base.EmployeeSSN;

                        var lastFour = base.EmployeeSSN.Substring(base.EmployeeSSN.Length - 4);
                        var firstPart = base.EmployeeSSN.Substring(0, base.EmployeeSSN.Length - 4);
                        return Regex.Replace(firstPart, "[0-9]", "X") + lastFour;
                    case SSNDisplay.Hidden:
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// The title of the employee's primary position during the time period of this pay statement
        /// </summary>
        public string PrimaryPosition
        {
            get
            {
                if (sourceDataContext.personEmploymentStatus != null)
                {
                    var position = dataUtility.GetPosition(sourceDataContext.personEmploymentStatus.PrimaryPositionId);
                    if (position != null)
                    {
                        return position.ShortTitle;
                    }
                }
                return string.Empty;
            }
        }

        public string InstitutionName
        {
            get
            {
                return dataUtility.Configuration.InstitutionName;
            }
        }

        public IEnumerable<PayStatementAddress> InstitutionMailingLabel
        {
            get
            {
                return dataUtility.Configuration.InstitutionMailingLabel;
            }
        }

        public DateTime? PeriodStartDate
        {
            get
            {
                return sourceDataContext.payrollRegisterEntry.PayPeriodStartDate;
            }
        }

        /// <summary>
        /// The employee's federal withholding status based on the filing status of their federal withholding type tax entry.
        /// </summary>
        public string FederalWithholdingStatus
        {
            get
            {
                return GetWithholdingStatusForType(TaxCodeType.FederalWithholding);
            }
        }

        /// <summary>
        /// The employee's state withholding status based on the filing status of their first state withholding type tax entry.
        /// </summary>
        public string StateWithholdingStatus
        {
            get
            {
                return GetWithholdingStatusForType(TaxCodeType.StateWithholding);
            }
        }

        /// <summary>
        /// Helper to get the withholding status based on type (federalwithholding, stateWithholding)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetWithholdingStatusForType(TaxCodeType type)
        {
            //get the tax codes for the given type
            var taxCodes = dataUtility.GetTaxCodesForType(type);

            //find the first tax entry in the payroll register for the given type
            //that aren't inactive and that aren't TaxableExempt
            var taxEntry = sourceDataContext.payrollRegisterEntry.TaxEntries
                .Where(te => te.ProcessingCode != PayrollTaxProcessingCode.Inactive && te.ProcessingCode != PayrollTaxProcessingCode.TaxableExempt)
                .FirstOrDefault(te => taxCodes.Any(t => t.Code == te.TaxCode));

            if (taxEntry != null)
            {
                //pull the filing (withholding) status description from the tax code
                var associatedTaxCode = dataUtility.GetTaxCode(taxEntry.TaxCode);
                if (associatedTaxCode != null && associatedTaxCode.FilingStatus != null)
                {
                    return associatedTaxCode.FilingStatus.Description;
                }
            }
            return string.Empty;
        }


        /// <summary>
        /// The number of exemptions taken on the federal withholding type tax entry
        /// </summary>
        public int FederalExemptions
        {
            get
            {
                var taxCodes = dataUtility.GetTaxCodesForType(TaxCodeType.FederalWithholding);

                var taxEntry = sourceDataContext.payrollRegisterEntry.TaxEntries
                    .Where(te => te.ProcessingCode != PayrollTaxProcessingCode.Inactive && te.ProcessingCode != PayrollTaxProcessingCode.TaxableExempt)
                    .FirstOrDefault(te => taxCodes.Any(t => t.Code == te.TaxCode));

                if (taxEntry != null)
                {
                    return taxEntry.Exemptions;
                }
                return 0;
            }
        }

        /// <summary>
        /// The number of exemptions taken on the first state withholding type tax entry.
        /// </summary>
        public int StateExemptions
        {
            get
            {
                var taxCodes = dataUtility.GetTaxCodesForType(TaxCodeType.StateWithholding);

                var taxEntry = sourceDataContext.payrollRegisterEntry.TaxEntries
                    .Where(te => te.ProcessingCode != PayrollTaxProcessingCode.Inactive && te.ProcessingCode != PayrollTaxProcessingCode.TaxableExempt)
                    .FirstOrDefault(te => taxCodes.Any(t => t.Code == te.TaxCode));

                if (taxEntry != null)
                {
                    return taxEntry.Exemptions;
                }
                return 0;
            }
        }

        /// <summary>
        /// The amount of additional tax withheld for federal taxes
        /// </summary>
        public decimal AdditionalFederalWithholding
        {
            get
            {
                var taxCodes = dataUtility.GetTaxCodesForType(TaxCodeType.FederalWithholding);

                var taxEntry = sourceDataContext.payrollRegisterEntry.TaxEntries
                    .Where(te => te.ProcessingCode == PayrollTaxProcessingCode.AdditionalTaxAmount)
                    .FirstOrDefault(te => taxCodes.Any(t => t.Code == te.TaxCode));

                if (taxEntry != null)
                {
                    return taxEntry.SpecialProcessingAmount ?? 0m;
                }
                return 0;
            }

        }

        /// <summary>
        /// The amount of additonal tax withheld for state taxes
        /// </summary>
        public decimal AdditionalStateWithholding
        {
            get
            {
                var taxCodes = dataUtility.GetTaxCodesForType(TaxCodeType.StateWithholding);

                var taxEntry = sourceDataContext.payrollRegisterEntry.TaxEntries
                    .Where(te => te.ProcessingCode == PayrollTaxProcessingCode.AdditionalTaxAmount)
                    .FirstOrDefault(te => taxCodes.Any(t => t.Code == te.TaxCode));

                if (taxEntry != null)
                {
                    return taxEntry.SpecialProcessingAmount ?? 0m;
                }
                return 0;
            }
        }

        /// <summary>
        /// Leave Line Items displayed on PayStatementReport. Each one represents the leave for a specific type for the statement. Currently, only display leave that is
        /// Sick, Comensetory, or Vacation.
        /// </summary>
        public IEnumerable<PayStatementLeave> Leave
        {
            get
            {
                var leaveItems = new List<PayStatementLeave>();

                var leaveEntryGroups = sourceDataContext.payrollRegisterEntry.LeaveEntries.GroupBy(le => le.LeaveType);
                foreach (var leaveEntryGroup in leaveEntryGroups)
                {
                    var leaveType = dataUtility.GetLeaveType(leaveEntryGroup.Key);
                    if (leaveType != null)
                    {
                        if (leaveType.TimeType == LeaveTypeCategory.Sick ||
                            leaveType.TimeType == LeaveTypeCategory.Compensatory ||
                            leaveType.TimeType == LeaveTypeCategory.Vacation)
                        {
                            var payStatementLeave = new PayStatementLeave(leaveType.Code,
                                leaveType.Description,
                                leaveEntryGroup.Sum(entry => entry.LeaveTaken),
                                leaveEntryGroup.Sum(entry => entry.LeaveRemaining));

                            leaveItems.Add(payStatementLeave);
                        }
                    }
                }
                return leaveItems.OrderByDescending(l => l.Description);
            }
        }

        /// <summary>
        /// Taxable Fringe Benefit line items displayed on PayStatementReport. Each benefit line item represents
        /// a taxable benefit code along with the period and year to date amounts applied to the applicable gross.
        /// </summary>
        public IEnumerable<PayStatementTaxableBenefit> TaxableBenefits
        {
            get
            {
                var taxableBenefits = new List<PayStatementTaxableBenefit>();
                var ytdTaxableBenefits = new List<PayrollRegisterTaxableBenefitEntry>();

                // create a unique list of the year to date taxable benefits, taking the first value from the group. do this, because the taxable benefit is repeated for
                // all tax codes it is applied to, but we only care about one of those values...
                foreach (var context in yearToDateDataContext)
                {

                    ytdTaxableBenefits.AddRange(
                        context.payrollRegisterEntry.TaxableBenefitEntries
                            .GroupBy(t => t.TaxableBenefitId)
                            .Select(g => g.First())
                            .ToList());
                }

                // group the taxable benefits for the year to date...
                var ytdTaxableBenefitGroups = ytdTaxableBenefits.GroupBy(b => b.TaxableBenefitId);

                // group the taxable benefits for this period...
                var periodTaxableBenefitGroups = sourceDataContext
                    .payrollRegisterEntry
                    .TaxableBenefitEntries
                    .GroupBy(tb => tb.TaxableBenefitId);

                // create the PayStatementTaxableBenefit for each year to date benefit group
                foreach (var ytdTaxableBenefitGroup in ytdTaxableBenefitGroups)
                {
                    if (ytdTaxableBenefitGroup.Any())
                    {
                        var benefitDeductionId = ytdTaxableBenefitGroup.First().TaxableBenefitId;
                        var benefitDeductionType = dataUtility.GetBenefitDeductionType(benefitDeductionId);
                        if (benefitDeductionType != null)
                        {
                            var description = !string.IsNullOrEmpty(benefitDeductionType.SelfServiceDescription) ? benefitDeductionType.SelfServiceDescription : benefitDeductionType.Description;
                            var ytdAmount = ytdTaxableBenefitGroup.Sum(t => t.TaxableBenefitAmt);

                            //get the periodBenefitGroup with the matching key
                            var periodBenefitGroup = periodTaxableBenefitGroups.FirstOrDefault(p => p.Key == ytdTaxableBenefitGroup.Key);
                            if (periodBenefitGroup != null && periodBenefitGroup.Any())
                            {
                                var taxableBenefit = new PayStatementTaxableBenefit(benefitDeductionId, description, periodBenefitGroup.First().TaxableBenefitAmt, ytdAmount);
                                taxableBenefits.Add(taxableBenefit);
                            }
                            else
                            {

                                taxableBenefits.Add(
                                    new PayStatementTaxableBenefit(
                                        benefitDeductionId,
                                        description,
                                        0,
                                        ytdAmount));
                            }
                        }
                    }
                }
                return taxableBenefits.OrderBy(t => t.TaxableBenefitDescription);
            }
        }

        /// <summary>
        /// Bank Deposit Line Items displayed on PayStatementReport. Each one represents a direct deposit to the employee's bank account.
        /// </summary>
        public IEnumerable<PayStatementBankDeposit> Deposits
        {
            get
            {
                var bankDeposits = new List<PayStatementBankDeposit>();
                foreach (var deposit in sourceDataContext.sourceData.SourceBankDeposits)
                {
                    bankDeposits.Add(
                        new PayStatementBankDeposit(
                            deposit.BankName,
                            deposit.BankAccountType,
                            deposit.AccountIdLastFour,
                            deposit.DepositAmount));
                }
                return bankDeposits;
            }
        }

        /// <summary>
        /// The Earnings Line Items displayed on the PayStatementReport. Each line item
        /// represents some type of earnings earned during the year, even if the employee didn't earn that item
        /// during the period.
        /// </summary>
        public IEnumerable<PayStatementEarnings> Earnings
        {
            get
            {
                var sortedEarnings = standardEarnings.Concat(stipendEarnings)
                    .OrderBy(e => dataUtility.GetEarningsType(e.EarningsTypeId).Category)
                    .ThenBy(e => e.EarningsTypeDescription);

                return sortedEarnings.Concat(differentialEarnings);
            }
        }

        private IEnumerable<PayStatementEarnings> standardEarnings
        {
            get
            {
                var earnings = new List<PayStatementEarnings>();
                var ytdEarningsGroups = yearToDateDataContext
                    .SelectMany(c => c.payrollRegisterEntry.EarningsEntries
                    .Where(earn => !((earn.IsStipendEarnings) ||
                                    ((dataUtility.GetEarnTypeLeaveTimeType(earn.EarningsTypeId) == LeaveTypeCategory.Compensatory) &&
                                    (dataUtility.GetEarningsType(earn.EarningsTypeId).Method == EarningsMethod.Accrued)))))
                    .GroupBy(ytd => ytd.EarningsTypeId);

                var standardLookup = sourceDataContext.payrollRegisterEntry.EarningsEntries
                    .Where(earn => !((earn.IsStipendEarnings) ||
                                   ((dataUtility.GetEarnTypeLeaveTimeType(earn.EarningsTypeId) == LeaveTypeCategory.Compensatory) &&
                                   (dataUtility.GetEarningsType(earn.EarningsTypeId).Method == EarningsMethod.Accrued))))
                    .ToLookup(earn => earn.EarningsTypeId);

                foreach (var ytdEarningsGroup in ytdEarningsGroups)
                {
                    var periodEarningsEntries = standardLookup.Contains(ytdEarningsGroup.Key) ?
                        standardLookup[ytdEarningsGroup.Key] :
                        new List<PayrollRegisterEarningsEntry>();

                    if (periodEarningsEntries.Any())
                    {
                        //person was paid money for working this earnings type during this period
                        var entryRef = periodEarningsEntries.First();

                        earnings.Add(new PayStatementEarnings(dataUtility.GetEarningsType(entryRef.EarningsTypeId),
                            periodEarningsEntries.Sum(e => e.StandardUnitsWorked),
                            AggregateStandardRate(entryRef.StandardRate, periodEarningsEntries),
                            periodEarningsEntries.Sum(e => e.BasePeriodEarningsAmount + e.EarningsFactorPeriodAmount),
                            ytdEarningsGroup));
                    }
                    else
                    {
                        //person was paid money for working this earnings type at some point during the year, but not this period

                        earnings.Add(new PayStatementEarnings(
                            dataUtility.GetEarningsType(ytdEarningsGroup.First().EarningsTypeId), ytdEarningsGroup));
                    }
                }
                return earnings;
            }
        }

        private decimal? AggregateStandardRate(decimal? initialRate, IEnumerable<PayrollRegisterEarningsEntry> entriesToAggregate)
        {
            if (entriesToAggregate.All(earn => earn.HourlySalaryIndication == HourlySalaryIndicator.Hourly && earn.StandardRate == initialRate))
            {
                return initialRate;
            }
            return null;
        }

        private IEnumerable<PayStatementEarnings> stipendEarnings
        {
            get
            {
                var earnings = new List<PayStatementEarnings>();

                var ytdStipendGroups = yearToDateDataContext
                    .SelectMany(c => c.payrollRegisterEntry.EarningsEntries
                        .Where(earn => earn.IsStipendEarnings))
                    .GroupBy(earn => earn.EarningsTypeId);

                var stipendLookup = sourceDataContext.payrollRegisterEntry.EarningsEntries
                    .Where(e => e.IsStipendEarnings)
                    .ToLookup(e => e.EarningsTypeId);

                foreach (var ytdStipendGroup in ytdStipendGroups)
                {
                    var periodStipendEntries = stipendLookup.Contains(ytdStipendGroup.Key) ?
                        stipendLookup[ytdStipendGroup.Key] :
                        new List<PayrollRegisterEarningsEntry>();

                    if (periodStipendEntries.Any())
                    {
                        var entryRef = periodStipendEntries.First();
                        earnings.Add(new PayStatementEarnings(dataUtility.GetEarningsTypeAsStipend(entryRef.EarningsTypeId),
                            null,
                            null,
                            periodStipendEntries.Sum(p => p.TotalPeriodEarningsAmount),
                            ytdStipendGroup));
                    }
                    else
                    {
                        earnings.Add(new PayStatementEarnings(dataUtility.GetEarningsTypeAsStipend(ytdStipendGroup.First().EarningsTypeId), ytdStipendGroup));
                    }
                }
                return earnings;
            }
        }

        private IEnumerable<PayStatementEarnings> differentialEarnings
        {
            get
            {
                var earnings = new List<PayStatementEarnings>();

                var ytdDifferentialGroups = yearToDateDataContext
                    .SelectMany(c => c.payrollRegisterEntry.EarningsEntries)
                    .Where(earn => earn.HasDifferentialEarnings)
                    .GroupBy(earn => earn.EarningsDifferentialId);

                var differentialLookup = sourceDataContext.payrollRegisterEntry.EarningsEntries
                    .Where(e => e.HasDifferentialEarnings)
                    .ToLookup(e => e.EarningsDifferentialId);

                foreach (var ytdDifferentialGroup in ytdDifferentialGroups)
                {
                    var periodEarningsEntries = differentialLookup.Contains(ytdDifferentialGroup.Key) ?
                        differentialLookup[ytdDifferentialGroup.Key] :
                        new List<PayrollRegisterEarningsEntry>();

                    if (periodEarningsEntries.Any())
                    {
                        //person was paid money for working this earnings differential during this period
                        var entryRef = periodEarningsEntries.First();
                        earnings.Add(new PayStatementEarnings(dataUtility.GetEarningsDifferential(entryRef.EarningsDifferentialId),
                            periodEarningsEntries.Sum(e => e.DifferentialUnitsWorked),
                            AggregateDifferentialRate(entryRef.DifferentialRate, periodEarningsEntries),
                            periodEarningsEntries.Sum(e => e.DifferentialPeriodEarningsAmount),
                            ytdDifferentialGroup));
                    }
                    else
                    {
                        //person was paid money for working this earnings differential at some point during the year, but not this period
                        earnings.Add(new PayStatementEarnings(
                            dataUtility.GetEarningsDifferential(ytdDifferentialGroup.First().EarningsDifferentialId), ytdDifferentialGroup));
                    }
                }
                return earnings.OrderBy(e => e.EarningsTypeDescription);
            }
        }

        private decimal? AggregateDifferentialRate(decimal? initialRate, IEnumerable<PayrollRegisterEarningsEntry> entriesToAggregate)
        {
            var aggregateRateCriteria = new Func<PayrollRegisterEarningsEntry, bool>(earn =>
                earn.HourlySalaryIndication == HourlySalaryIndicator.Hourly &&
                earn.DifferentialRate == initialRate);

            if (entriesToAggregate.All(aggregateRateCriteria))
            {
                return initialRate;
            }
            return null;
        }

        /// <summary>
        /// Boolean to indicate if this record is using the 2020 W4 calculation rules.
        /// </summary>
        public bool Apply2020W4Rules
        {
            get
            {
                return sourceDataContext.payrollRegisterEntry.Apply2020W4Rules;
            }
        }

        /// <summary>
        /// The Deductions are a combination of Tax, Benefit and Other Deduction type deductions 
        /// that were paid by the employee and/or employer during the year, even if those deductions weren't paid during the period
        /// </summary>
        public IEnumerable<PayStatementDeduction> Deductions
        {
            get
            {
                return taxTypeDeductions.Concat(benefitAndOtherTypeDeductions);
            }
        }

        /// <summary>
        /// Helper to calculate the Tax type PayStatementDeductions
        /// </summary>
        private IEnumerable<PayStatementDeduction> taxTypeDeductions
        {
            get
            {
                var taxDeductions = new List<PayStatementDeduction>();

                //loop through the year to date tax codes (we want to display a line item for each tax removed in the year, even if it wasn't removed this pay period)
                var yearToDateTaxes = yearToDateDataContext.SelectMany(c => c.payrollRegisterEntry.TaxEntries).ToList();
                var taxCodes = yearToDateTaxes.Select(tax => tax.TaxCode).Distinct();

                //do data associations once, grouping/lookup     
                var lookup = sourceDataContext.payrollRegisterEntry.TaxEntries.ToLookup(t => t.TaxCode);

                foreach (var code in taxCodes)
                {
                    var periodTaxEntries = lookup.Contains(code) ?
                        lookup[code] :
                        new List<PayrollRegisterTaxEntry>();

                    if (periodTaxEntries.Any())
                    {
                        var aggregateTaxEntry = new PayrollRegisterTaxEntry(code, PayrollTaxProcessingCode.Regular) //processing code doesn't matter for this part
                        {
                            EmployeeTaxAmount = periodTaxEntries.Sum(t => t.EmployeeTaxAmount),
                            EmployeeTaxableAmount = periodTaxEntries.Sum(t => t.EmployeeTaxableAmount),
                            EmployerTaxAmount = periodTaxEntries.Sum(t => t.EmployerTaxAmount),
                            EmployerTaxableAmount = periodTaxEntries.Sum(t => t.EmployerTaxableAmount)
                        };
                        taxDeductions.Add(new PayStatementDeduction(dataUtility.GetTaxCode(code), aggregateTaxEntry, yearToDateTaxes));
                    }
                    else
                    {
                        taxDeductions.Add(new PayStatementDeduction(dataUtility.GetTaxCode(code), yearToDateTaxes));
                    }
                }
                return taxDeductions;
            }
        }

        /// <summary>
        /// Helper to calculate the Benefit and Other Deduction type deductions
        /// </summary>
        private IEnumerable<PayStatementDeduction> benefitAndOtherTypeDeductions
        {
            get
            {
                var deductions = new List<PayStatementDeduction>();

                var yearToDateDeductions = yearToDateDataContext.SelectMany(c => c.payrollRegisterEntry.BenefitDeductionEntries).ToList();

                //deduction codes are the benefits/deductions received/pay yearToDate, 
                //plus (if the PayStatementConfiguration indicates)
                //the benefits and deductions that are active for the person in this pay period, even if they haven't received/paid it in the year to date
                var deductionCodes = yearToDateDeductions.Select(bended => bended.BenefitDeductionId);
                if (dataUtility.Configuration.DisplayZeroAmountBenefitDeductions)
                {
                    var activePeriodBenefitDeductions =
                        sourceDataContext.personBenefitDeductions.Select(pbd => pbd.BenefitDeductionId);
                    deductionCodes = deductionCodes.Concat(activePeriodBenefitDeductions);
                }
                deductionCodes = deductionCodes.Distinct();

                //do data associations once, grouping/lookup              
                var lookup = sourceDataContext.payrollRegisterEntry.BenefitDeductionEntries.ToLookup(bd => bd.BenefitDeductionId);

                foreach (var code in deductionCodes)
                {

                    var periodDeductionEntries = lookup.Contains(code) ?
                        lookup[code] :
                        new List<PayrollRegisterBenefitDeductionEntry>();

                    if (periodDeductionEntries.Any())
                    {
                        var aggregateDeductionEntry = new PayrollRegisterBenefitDeductionEntry(code)
                        {
                            EmployeeAmount = periodDeductionEntries.Sum(t => t.EmployeeAmount),
                            EmployerAmount = periodDeductionEntries.Sum(t => t.EmployerAmount),
                            EmployeeBasisAmount = periodDeductionEntries.Sum(t => t.EmployeeBasisAmount),
                            EmployerBasisAmount = periodDeductionEntries.Sum(t => t.EmployerBasisAmount)
                        };
                        var deduction = new PayStatementDeduction(dataUtility.GetBenefitDeductionType(code), aggregateDeductionEntry, yearToDateDeductions);

                        deductions.Add(deduction);

                    }
                    else
                    {
                        deductions.Add(new PayStatementDeduction(dataUtility.GetBenefitDeductionType(code), yearToDateDeductions));
                    }
                }
                if (!dataUtility.Configuration.DisplayZeroAmountBenefitDeductions)
                {
                    deductions = deductions.Where(bd => !bd.IsZeroYearToDateAmount).ToList();
                }
                return deductions.OrderBy(b => b.Description);
            }
        }
    }
}
