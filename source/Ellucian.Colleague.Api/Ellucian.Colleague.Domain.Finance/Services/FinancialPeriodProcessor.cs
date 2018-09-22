// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Services;

namespace Ellucian.Colleague.Domain.Finance.Services
{
    /// <summary>
    /// Class to perform financial period logic
    /// </summary>
    public static class FinancialPeriodProcessor
    {
        /// <summary>
        /// Get the FinancialPeriod for the specified period code
        /// </summary>
        /// <param name="type">Period code</param>
        /// <param name="periods">Collection of financial periods</param>
        /// <returns>Financial period</returns>
        public static Domain.Finance.Entities.FinancialPeriod GetPeriodByCode(string code, IEnumerable<Domain.Finance.Entities.FinancialPeriod> periods)
        {
            if (String.IsNullOrEmpty(code))
            {
                return null;
            }
            var type = GetPeriodType(code);
            return GetPeriodByType(type, periods);
        }

        /// <summary>
        /// Get the FinancialPeriod for the specified PeriodType
        /// </summary>
        /// <param name="type">Period type</param>
        /// <param name="periods">Collection of financial periods</param>
        /// <returns>Financial period</returns>
        public static Domain.Finance.Entities.FinancialPeriod GetPeriodByType(PeriodType? type, IEnumerable<Domain.Finance.Entities.FinancialPeriod> periods)
        {
            if (periods == null || periods.Count() == 0)
            {
                return null;
            }
            if (type == null)
            {
                return null;
            }
            return periods.Where(x => x.Type == type.Value).FirstOrDefault();
        }

        /// <summary>
        /// Get the PeriodType for a period code
        /// </summary>
        /// <param name="code">Period code</param>
        /// <returns>Period type</returns>
        public static Domain.Base.Entities.PeriodType? GetPeriodType(string code)
        {
            PeriodType? type = null;
            switch (code)
            {
                case FinanceTimeframeCodes.PastPeriod:
                    type = PeriodType.Past;
                    break;
                case FinanceTimeframeCodes.CurrentPeriod:
                    type = PeriodType.Current;
                    break;
                case FinanceTimeframeCodes.FuturePeriod:
                    type = PeriodType.Future;
                    break;
                default:
                    return type;
            }
            return type;
        }

        /// <summary>
        /// Get the period for a date range
        /// </summary>
        /// <param name="startDate">Range starting date</param>
        /// <param name="endDate">Range ending date</param>
        /// <param name="periods">Collection of financial periods</param>
        /// <returns>Period type in which date range falls</returns>
        /// <remarks>Even if the date range falls into multiple periods, this method will only return
        /// a single period.  If any part of the date range overlaps the Current period, then the Current
        /// period is returned.  Otherwise, the Future period is checked for an overlap.  If the date range
        /// isn't in either of those periods, then it is assumed to be in the Past period.</remarks>
        public static PeriodType? GetDateRangePeriod(DateTime? startDate, DateTime? endDate, IEnumerable<Domain.Finance.Entities.FinancialPeriod> periods)
        {
            PeriodType? period = null;
            // If there are no periods, don't do anything
            if (periods == null || periods.Count() == 0)
            {
                return period;
            }
            // If any part of the date range falls in the current period, then it's in that period
            if (IsInPeriod(startDate, endDate, PeriodType.Current, periods))
            {
                period = PeriodType.Current;
            }
            else if (IsInPeriod(startDate, endDate, PeriodType.Future, periods))
            {
                period = PeriodType.Future;
            }
            else
            {
                period = PeriodType.Past;
            }
            return period;
        }

        /// <summary>
        /// Determine if a date range falls in a specified period
        /// </summary>
        /// <param name="startDate">Range starting date</param>
        /// <param name="endDate">Range ending date</param>
        /// <param name="type">Period type for comparison</param>
        /// <param name="periods">Collection of financial periods</param>
        /// <returns>True/false indicator</returns>
        public static bool IsInPeriod(DateTime? startDate, DateTime? endDate, PeriodType? type, IEnumerable<Domain.Finance.Entities.FinancialPeriod> periods)
        {
            TermPeriodProcessor.DateRangeDefaults(ref startDate, ref endDate);
            var period = GetPeriodByType(type, periods);
            return (period == null) ? false : TermPeriodProcessor.IsRangeOverlap(startDate.Value, endDate.Value, period.Start, period.End);
        }
    }
}
