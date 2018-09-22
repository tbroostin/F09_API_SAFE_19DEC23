// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Denotes the severity styling
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AlertStyle
    {
        /// <summary>
        /// The alert is serious
        /// </summary>
        Critical,

        /// <summary>
        /// The alert is moderately important
        /// </summary>
        Warning,

        /// <summary>
        /// The alert is not serious
        /// </summary>
        Information
    }
}
