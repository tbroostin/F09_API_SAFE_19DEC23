// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// OfficeCodes are used to identify which office owns certain CommunicationCodes, and which
    /// Staff member belong to which office.
    /// </summary>
    public class OfficeCode
    {
        /// <summary>
        /// Unique system code for this Office Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of this office code
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of office this code represents, i.e. FinancialAid, Admissions, etc.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public OfficeCodeType Type { get; set; }
    }
}
