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
    /// Status for a payment plan
    /// </summary>
    public class PlanStatus
    {
        /// <summary>
        /// Plan status
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PlanStatusType Status { get; set; }

        /// <summary>
        /// Date as of which the plan had the status
        /// </summary>
        public DateTime Date { get; set; }
    }
}
