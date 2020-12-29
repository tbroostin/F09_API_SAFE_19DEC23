// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is the project budget period.
    /// (this class is moved to Colleague Finance module from Projects Accounting module)
    /// </summary>
    public class ProjectBudgetPeriod
    {
        /// <summary>
        /// This is the unique budget period identifier for the project
        /// </summary>
        public string SequenceNumber { get; set; }

        /// <summary>
        /// This is the project budget period start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// This is the project budget period end date
        /// </summary>
        public DateTime EndDate { get; set; }

    }
}
