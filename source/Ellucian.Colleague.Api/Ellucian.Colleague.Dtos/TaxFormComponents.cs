//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The specific elements included on a tax form (i.e. non-exempt income, prizes and awards, etc). 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TaxFormComponents : CodeItem2
    {
        /// <summary>
        /// The tax form with which the components are associated.
        /// </summary>

        [JsonProperty("taxForm", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 TaxForm { get; set; }

    }
}