// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.DtoProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Accounting strings that are valid for use with the finance system.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountingString
    {
        /// <summary>
        /// An object containing metadata.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MetaDataDtoProperty MetadataObject { get; set; }

        /// <summary>
        /// The full representation of the accounting string in the format specified by the authoritative system.
        /// </summary>
        [JsonProperty("accountingString")]
        public string AccountingStringValue { get; set; }

        /// <summary>
        /// The description of the accounting-string.
        /// </summary>
        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }
    }
}
