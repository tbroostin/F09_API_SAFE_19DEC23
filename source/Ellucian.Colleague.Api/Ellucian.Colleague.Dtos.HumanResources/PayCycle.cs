using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Describes  a pay cycle and its pay period date ranges
    /// </summary>
    public class PayCycle
    {
        /// <summary>
        /// The database Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The number of pay cycles in a year
        /// </summary>
        public int AnnualPayFrequency { get; set; }
        /// <summary>
        /// The day of the week on which the pay cycle starts
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DayOfWeek WorkWeekStartDay { get; set; }
        /// <summary>
        /// The pay cycle description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// List of pay classes associated with the pay cycle
        /// </summary>
        public List<string> PayClassIds { get; set; }
        /// <summary>
        /// List of pay period date ranges associated with the pay cycle
        /// </summary>
        public List<PayPeriod> PayPeriods { get; set; }
    }
}
