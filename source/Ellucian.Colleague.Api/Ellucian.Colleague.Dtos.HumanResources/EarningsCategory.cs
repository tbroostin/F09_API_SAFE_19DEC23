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
    /// Categorize an earnings type into Regular, Overtime, Leave, Colleage Work Study, or Miscellaneous
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EarningsCategory
    {
        /// <summary>
        /// Regular earnings category
        /// </summary>
        Regular,
        /// <summary>
        /// Overtime earnings category
        /// </summary>
        Overtime,
        /// <summary>
        /// Leave earnings category
        /// </summary>
        Leave,
        /// <summary>
        /// College Work Study earnings category
        /// </summary>
        CollegeWorkStudy,
        /// <summary>
        /// Miscellaneous earnings category
        /// </summary>
        Miscellaneous
    }
}
