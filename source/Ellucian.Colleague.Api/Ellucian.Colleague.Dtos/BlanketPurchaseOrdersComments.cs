//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The comments associated with the blanket purchase order. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class BlanketPurchaseOrdersComments
    {
        /// <summary>
        /// A comment associated with the blanket purchase order.
        /// </summary>

        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Comment { get; set; }

        /// <summary>
        /// An indication of whether the comment is printed or not printed.
        /// </summary>

        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CommentTypes? Type { get; set; }


    }
}