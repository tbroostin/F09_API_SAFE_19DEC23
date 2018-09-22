//Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of award letter groupings group types
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupType
    {
        /// <summary>
        /// AwardCategories group type
        /// </summary>
        AwardCategories,

        /// <summary>
        /// Award period column group type
        /// </summary>
        AwardPeriodColumn
    }
}
