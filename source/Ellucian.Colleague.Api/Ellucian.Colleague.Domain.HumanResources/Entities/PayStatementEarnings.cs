/*Copyright 2017-2021 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// A PayStatementEarnings entity that represents the earnings data on a Pay Statement report
    /// </summary>
    [Serializable]
    public class PayStatementEarnings
    {

        /// <summary>
        /// The Id of the EarningsType
        /// </summary>
        public string EarningsTypeId { get; private set; }

        /// <summary>
        /// A description of the EarningsType
        /// </summary>
        public string EarningsTypeDescription { get; private set; }

        /// <summary>
        /// Units worked for the Earnings. For Salaried employees, this will be null
        /// </summary>
        public decimal? UnitsWorked { get; private set; }

        /// <summary>
        /// The rate at which the employee was paid for this earnings
        /// </summary>
        public decimal? Rate { get; private set; }

        /// <summary>
        /// The amount the employee was paid for this earnings for the period.
        /// </summary>
        public decimal? PeriodPaymentAmount { get; private set; }

        /// <summary>
        /// The amount the employee has been paid for this earnings, year to date.
        /// </summary>
        public decimal YearToDatePaymentAmount { get; private set; }
        
  
        /// <summary>
        /// Create a line item for standard period earnings.
        /// </summary>
        /// <param name="earningsType"></param>
        /// <param name="unitsWorked"></param>
        /// <param name="rate"></param>
        /// <param name="periodPaymentAmount"></param>
        /// <param name="yearToDateEarningsEntries"></param>
        public PayStatementEarnings(EarningsType earningsType, decimal? unitsWorked, decimal? rate, decimal periodPaymentAmount, 
            IEnumerable<PayrollRegisterEarningsEntry> yearToDateEarningsEntries)
        {
            if (earningsType == null)
            {
                throw new ArgumentNullException("earningsType");
            }
            if (yearToDateEarningsEntries == null)
            {
                throw new ArgumentNullException("yearToDateEarningsEntries");
            }

            EarningsTypeId = earningsType.Id;
            EarningsTypeDescription = earningsType.Description;
            UnitsWorked = unitsWorked;
            Rate = rate;
            if (earningsType.Category == EarningsCategory.Overtime && Rate.HasValue)
            {
                Rate = Rate * earningsType.Factor;

                //make sure the math works out
                var x = UnitsWorked.HasValue ? UnitsWorked.Value : 0;
                var y = Rate.Value;
                var mathCheck = Math.Round(x * y, 2);
                if (mathCheck != periodPaymentAmount)
                {
                    Rate = null;
                }
            }

            PeriodPaymentAmount = periodPaymentAmount;
            YearToDatePaymentAmount = yearToDateEarningsEntries.Sum(ytd => ytd.BasePeriodEarningsAmount + ytd.EarningsFactorPeriodAmount 
                + ytd.EarningsAdjustmentAmount);

        }

        /// <summary>
        /// Create a line item for standard year-to-date only earnings
        /// </summary>
        /// <param name="earningsType"></param>
        /// <param name="yearToDateEarningsEntries"></param>
        public PayStatementEarnings(EarningsType earningsType, IEnumerable<PayrollRegisterEarningsEntry> yearToDateEarningsEntries)
        {
            if (earningsType == null)
            {
                throw new ArgumentNullException("earningsType");
            }
            if (yearToDateEarningsEntries == null)
            {
                throw new ArgumentNullException("yearToDateEarningsEntries");
            }

            EarningsTypeId = earningsType.Id;
            EarningsTypeDescription = earningsType.Description;
            YearToDatePaymentAmount = yearToDateEarningsEntries.Sum(ytd => ytd.BasePeriodEarningsAmount + ytd.EarningsFactorPeriodAmount + ytd.EarningsAdjustmentAmount);        
        }

        /// <summary>
        /// Create a line item for differential period earnings.
        /// </summary>
        /// <param name="earningsDifferential"></param>
        /// <param name="unitsWorked"></param>
        /// <param name="rate"></param>
        /// <param name="periodPaymentAmount"></param>
        /// <param name="yearToDateEarningsEntries"></param>
        public PayStatementEarnings(EarningsDifferential earningsDifferential, decimal? unitsWorked, decimal? rate, decimal periodPaymentAmount, IEnumerable<PayrollRegisterEarningsEntry> yearToDateEarningsEntries)
        {
            if (earningsDifferential == null)
            {
                throw new ArgumentNullException("earningsDifferential");
            }
            if (yearToDateEarningsEntries == null)
            {
                throw new ArgumentNullException("yearToDateEarningsEntries");
            }

            EarningsTypeId = earningsDifferential.Code;
            EarningsTypeDescription = earningsDifferential.Description;
            UnitsWorked = unitsWorked;
            Rate = rate;

            PeriodPaymentAmount = periodPaymentAmount;
            YearToDatePaymentAmount = yearToDateEarningsEntries.Sum(ytd => ytd.DifferentialPeriodEarningsAmount);
        }

        /// <summary>
        /// Create a line item for differential year-to-date only earnings.
        /// </summary>
        /// <param name="earningsDifferential"></param>
        /// <param name="yearToDateEarningsEntries"></param>
        public PayStatementEarnings(EarningsDifferential earningsDifferential, IEnumerable<PayrollRegisterEarningsEntry> yearToDateEarningsEntries)
        {
            if (earningsDifferential == null)
            {
                throw new ArgumentNullException("earningsDifferential");
            }
            if (yearToDateEarningsEntries == null)
            {
                throw new ArgumentNullException("yearToDateEarningsEntries");
            }
            EarningsTypeId = earningsDifferential.Code;
            EarningsTypeDescription = earningsDifferential.Description;
            YearToDatePaymentAmount = yearToDateEarningsEntries.Sum(ytd => ytd.DifferentialPeriodEarningsAmount);

        }
    }
}
