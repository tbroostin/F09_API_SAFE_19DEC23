/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Specify an earnings type as leave accrued, leave taken, or time not paid
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EarningsMethod
    {
        /// <summary>
        /// Default earnings method
        /// </summary>
        None,
        /// <summary>
        /// Earnings method for leave accrued
        /// </summary>
        Accrued,
        /// <summary>
        /// Earnings method for leave taken
        /// </summary>
        Taken,
        /// <summary>
        /// Earnings method for time not paid
        /// </summary>
        NoPay
    }
}
