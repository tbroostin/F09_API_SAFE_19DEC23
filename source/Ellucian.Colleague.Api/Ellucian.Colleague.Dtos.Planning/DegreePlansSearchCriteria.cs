// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Contains INCOMING degree plans search/filter request
    /// </summary>
    public class DegreePlansSearchCriteria
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public DegreePlansSearchCriteria()
        {

        }

        /// <summary>
        /// Used when requesting a search of degree plans for open office
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DegreePlanRetrievalType DegreePlanRetrievalType { get; set; }
        
        /// <summary>
        /// searching advisee Keyword
        /// </summary>
        public string AdviseeKeyword { get; set; }
    }
}
