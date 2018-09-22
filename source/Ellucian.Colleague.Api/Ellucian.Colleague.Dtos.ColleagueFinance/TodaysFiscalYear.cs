// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// DTO for the today's fiscal Year based on the General Ledger Configuration.
    /// </summary>
    public class TodaysFiscalYear
    {
        /// <summary>
        /// The fiscal year for today's date.
        /// </summary>
        public string FiscalYearForToday { get; set; }
    }
}
