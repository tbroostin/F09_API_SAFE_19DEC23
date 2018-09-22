// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The valid frequency types supported by Colleague
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [Serializable]
    public enum AdapterDebugLevel
    {
        /// <summary>
        /// Daily
        /// </summary>
        Fatal,
        /// <summary>
        /// Weekly
        /// </summary>
        Error,
        /// <summary>
        /// Monthly
        /// </summary>
        Warning,
        /// <summary>
        /// Yearly
        /// </summary>
        Information,
        /// <summary>
        /// Debug
        /// </summary>
        Debug,
        /// <summary>
        /// Trace
        /// </summary>
        Trace
    }
}
