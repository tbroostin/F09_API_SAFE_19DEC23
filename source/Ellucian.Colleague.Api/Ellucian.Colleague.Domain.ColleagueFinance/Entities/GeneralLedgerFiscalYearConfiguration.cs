// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Exceptions;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Contains information regarding the fiscal year of the institution.
    /// </summary>
    [Serializable]
    public class GeneralLedgerFiscalYearConfiguration
    {
        /// <summary>
        /// Month which determines the start of the fiscal year.
        /// </summary>
        public int StartMonth { get { return startMonth; } }
        private readonly int startMonth;

        /// <summary>
        /// Current fiscal year.
        /// </summary>
        public string CurrentFiscalYear { get { return currentFiscalYear; } }
        private readonly string currentFiscalYear;

        /// <summary>
        /// Flag to indicate if a future date warning overrides a future fiscal year warning.
        /// </summary>
        public string FutureDateOverrideFutureYear { get { return futureDateOverrideFutureYear; } }
        private readonly string futureDateOverrideFutureYear;

        /// <summary>
        /// Number of fiscal periods to extend the fiscal year for a future date warning.
        /// </summary>
        public int NumberOfFuturePeriods { get { return numberOfFuturePeriods; } }
        private readonly int numberOfFuturePeriods;

        /// <summary>
        /// Calculate the fiscal year based on the current date.
        /// </summary>
        public int FiscalYearForToday
        {
            get
            {
                // Initialize the current year using the currentDate variable, but increase it to the next year
                // if the current month falls AFTER the start month.
                int currentYear = DateTime.Now.Year;
                if (DateTime.Now.Month >= this.startMonth)
                    currentYear += 1;

                return currentYear;
            }
        }

        /// <summary>
        /// Current month in the current fiscal year.
        /// </summary>
        public int CurrentFiscalMonth { get { return currentFiscalMonth; } }
        private readonly int currentFiscalMonth;

        /// <summary>
        /// Date which represents the first day of the fiscal year.
        /// </summary>
        public DateTime StartOfFiscalYear
        {
            get
            {
                var startYear = Int32.Parse(currentFiscalYear);
                if (startMonth > 1)
                {
                    startYear--;
                }
                return new DateTime(startYear, startMonth, 1);
            }
        }

        /// <summary>
        /// Date which represents the last day of the fiscal year.
        /// </summary>
        public DateTime EndOfFiscalYear
        {
            get
            {
                if (startMonth == 1)
                {
                    return new DateTime(Int32.Parse(currentFiscalYear), 12, 31);
                }

                return new DateTime(Int32.Parse(currentFiscalYear), startMonth, 1).AddDays(-1);
            }
        }

        /// <summary>
        /// End of the fiscal year extended by the number of fiscal periods for a future date warning condition.
        /// </summary>
        public DateTime ExtendedEndOfFiscalYear
        {
            get
            {
                if (futureDateOverrideFutureYear == "Y")
                {
                    int finalMonth = EndOfFiscalYear.Month;
                    int extendedFiscalYear = EndOfFiscalYear.Year;
                    finalMonth += (1 + numberOfFuturePeriods);
                    if (finalMonth > 12)
                    {
                        finalMonth -= 12;
                        extendedFiscalYear += 1;
                    }
                    return new DateTime(extendedFiscalYear, finalMonth, 1).AddDays(-1);
                }
                else
                {
                    return EndOfFiscalYear;
                }
            }
        }

        /// <summary>
        /// Initializes the GL fiscal year information.
        /// </summary>
        /// <param name="startMonth">Month on which a fiscal year starts.</param>
        /// <param name="currentYear">Current fiscal year for the institution.</param>
        /// <param name="currentFiscalMonth">Current month in the fiscal year.</param>
        /// <param name="numberOfFuturePeriods">Number of future fiscal periods for date warning.</param>
        /// <param name="futureDateOverrideFutureYear">Flag to indicate if future date overrides future fiscal year.</param>
        public GeneralLedgerFiscalYearConfiguration(int startMonth, string currentYear, int currentFiscalMonth, int numberOfFuturePeriods, string futureDateOverrideFutureYear)
        {
            if (string.IsNullOrEmpty(currentYear))
            {
                throw new ArgumentNullException("currentYear", "currentYear must have a value.");
            }

            if (startMonth < 1 || startMonth > 12)
            {
                throw new ConfigurationException("Fiscal year start month must be in between 1 and 12.");
            }

            if (currentFiscalMonth < 1 || currentFiscalMonth > 12)
            {
                throw new ConfigurationException("Current fiscal month must be in between 1 and 12.");
            }

            if (numberOfFuturePeriods < 0 || numberOfFuturePeriods > 12)
            {
                numberOfFuturePeriods = 0;
            }

            if ((futureDateOverrideFutureYear == null) || (futureDateOverrideFutureYear.ToUpperInvariant() != "Y"))
            {
                futureDateOverrideFutureYear = "N";
            }

            this.startMonth = startMonth;
            this.currentFiscalYear = currentYear;
            this.currentFiscalMonth = currentFiscalMonth;
            this.numberOfFuturePeriods = numberOfFuturePeriods;
            this.futureDateOverrideFutureYear = futureDateOverrideFutureYear;
        }
    }
}