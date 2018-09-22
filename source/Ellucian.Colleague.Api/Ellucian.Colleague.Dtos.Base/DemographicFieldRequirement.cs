// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Categorize a bank account into Checking and Savings Types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DemographicFieldRequirement
    {
        /// <summary>
        /// The field is not displayed or required
        /// </summary>
        Hidden,
        /// <summary>
        /// The field is displayed but not required
        /// </summary>
        Optional,
        /// <summary>
        /// The field is displayed and required
        /// </summary>
        Required
    }
}