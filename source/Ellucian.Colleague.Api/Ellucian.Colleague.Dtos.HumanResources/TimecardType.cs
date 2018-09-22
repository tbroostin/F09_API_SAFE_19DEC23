/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimecardType
    {
        /// <summary>
        /// No timecard type will be applied to the associated position
        /// </summary>
        None,
        /// <summary>
        /// Summary hours
        /// </summary>
        Summary,
        /// <summary>
        /// Detailed time
        /// </summary>
        Detailed
    }
}
