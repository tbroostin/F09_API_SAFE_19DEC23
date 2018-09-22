//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Financial Aid AwardStatus DTO
    /// </summary>
    public class AwardStatus
    {
        /// <summary>
        /// The Code of the AwardStatus
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// A Short Description of the AwardStatus
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The category of the AwardStatus
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AwardStatusCategory Category { get; set; }

    }
}
