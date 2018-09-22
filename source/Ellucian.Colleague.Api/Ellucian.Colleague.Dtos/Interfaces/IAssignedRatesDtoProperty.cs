// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Interfaces
{ 
    /// <summary>
    /// Information about rate for student meal plans
    /// </summary>
    public interface IAssignedRatesDtoProperty
    {
        /// <summary>
        /// The number of periods for the rate.
        /// </summary>
        [JsonProperty("numberOfPeriods", DefaultValueHandling = DefaultValueHandling.Ignore)]
        int? NumberOfPeriods { get; set; }

    }
}
