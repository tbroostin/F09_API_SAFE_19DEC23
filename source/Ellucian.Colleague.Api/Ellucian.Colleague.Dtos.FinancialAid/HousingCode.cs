//Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Housing codes that are specified on a FAFSA
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HousingCode
    {
        /// <summary>
        /// On campus housing
        /// </summary>
        OnCampus,
        /// <summary>
        /// With parent
        /// </summary>
        WithParent,
        /// <summary>
        /// Off campus housing
        /// </summary>
        OffCampus
    }
}
