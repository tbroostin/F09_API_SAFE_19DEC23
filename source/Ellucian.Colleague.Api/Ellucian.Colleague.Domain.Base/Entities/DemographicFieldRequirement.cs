// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The valid demographic field requirement types supported by Colleague
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [Serializable]
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
