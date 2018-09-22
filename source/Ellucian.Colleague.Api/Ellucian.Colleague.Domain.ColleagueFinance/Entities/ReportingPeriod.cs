// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Reporting period.
    /// </summary>
    [Serializable]
    public class ReportingPeriod
    {
        public DateTime? EndDate { get; set; }
        public DateTime? StartDate { get; set; }
    }
}