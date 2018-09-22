// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Indicates the frequency by which a payment plan's scheduled payments are calculated
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlanFrequency
    {
        /// <summary>
        /// Weekly - scheduled payment due every 7 days / 1 week
        /// </summary>
        Weekly,

        /// <summary>
        /// Biweekly - scheduled payment due every 14 days / 2 weeks
        /// </summary>
        Biweekly,

        /// <summary>
        /// Monthly - scheduled payment due once per month
        /// </summary>
        Monthly,

        /// <summary>
        /// Yearly - scheduled payment due once per year
        /// </summary>
        Yearly,

        /// <summary>
        /// Custom - payment plan schedule determined by a custom frequency subroutine
        /// </summary>
        Custom
    }
}
