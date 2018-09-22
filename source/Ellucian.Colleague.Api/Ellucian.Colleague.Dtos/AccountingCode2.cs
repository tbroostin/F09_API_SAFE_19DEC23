// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The type of contacts where comments can be made
    /// </summary>
    [DataContract]
    public class AccountingCode2 : CodeItem2
    {
        /// <summary>
        /// The top level category of the accounting code.
        /// </summary>
        [JsonProperty("category", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public AccountingCodeCategoryDtoProperty AccountingCodeCategory { get; set; }
    }
}