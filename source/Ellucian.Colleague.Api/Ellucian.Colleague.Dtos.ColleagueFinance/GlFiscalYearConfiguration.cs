// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Contains fiscaly year configuration information for the a finance query.
    /// </summary>
    public class GlFiscalYearConfiguration
    {
        /// <summary>
        /// Month which determines the start of the fiscal year.
        /// </summary>
        public int StartMonth { get; set; }

        /// <summary>
        /// Current month in the current fiscal year.
        /// If start month is 6 and current calendar month is 6 (June), this property will be 1.
        /// If start month is 6 and current calendar month is 9 (September), this property will be 4.
        /// If start month is 6 and current calendar month is 2 (February), this property will be 9.
        /// </summary>
        public int CurrentFiscalMonth { get; set; }

        /// <summary>
        /// Current fiscal year.
        /// </summary>
        public string CurrentFiscalYear { get; set; }

        /// <summary>
        /// Date which represents the first day of the fiscal year.
        /// </summary>
        public DateTime StartOfFiscalYear { get; set; }

        /// <summary>
        /// Date which represents the last day of the fiscal year.
        /// </summary>
        public DateTime EndOfFiscalYear { get; set; }

        /// <summary>
        /// End of the fiscal year extended by the number of fiscal periods for a future date warning condition.
        /// </summary>
        public DateTime ExtendedEndOfFiscalYear { get; set; }

        /// <summary>
        /// Number of fiscal periods that determine a future fiscal year warning.
        /// </summary>
        public int NumberOfFuturePeriods { get; set; }

        /// <summary>
        /// Open fiscal years.
        /// </summary>
        public IEnumerable<string> OpenFiscalYears { get; set; }
    }
}

