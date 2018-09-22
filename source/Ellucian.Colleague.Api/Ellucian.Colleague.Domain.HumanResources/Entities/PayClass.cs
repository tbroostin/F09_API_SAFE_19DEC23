//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Payclass
    /// </summary>
    [Serializable]
    public class PayClass : GuidCodeItem
    {
        /// <summary>
        /// The number of pays per year employees are scheduled to receive.
        /// </summary>
        public decimal? PaysPerYear { get; set; }

        /// <summary>
        /// The cycle at which employees are paid when they are working.
        /// </summary>
        public string PayCycle { get; set; }

        /// <summary>
        /// The frequency at which employees are paid when they are working.
        /// </summary>
        public string PayFrequency { get; set; }

        /// <summary>
        /// The default work hours in the specified time period.
        /// </summary>
        public decimal YearHoursPerPeriodHours { get; set; }

        /// <summary>
        /// The default time period (e.g. day, week, month, etc.).
        /// </summary>
        public string YearHoursPerPeriodPeriod { get; set; }

        /// <summary>
        /// The default work hours in the specified time period.
        /// </summary>
        public decimal CycleHoursPerPeriodHours { get; set; }

        /// <summary>
        /// The default time period (e.g. day, week, month, etc.).
        /// </summary>
        public string CycleHoursPerPeriodPeriod { get; set; }

        /// <summary>
        /// The status of the pay class (e.g. active or inactive).
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The compensation type associated with the pay class (e.g. salary or wages).
        /// </summary>
        public string CompensationType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Payclass"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public PayClass(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}