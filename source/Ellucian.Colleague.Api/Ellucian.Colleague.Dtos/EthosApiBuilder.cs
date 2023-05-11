// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// An Extended Data representation.
    /// </summary>
    [DataContract]
    public class EthosApiBuilder
    {
        /// <summary>
        /// A Globally Unique ID (GUID)
        /// </summary>
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string _Id { get; set; }

        /// <summary>
        /// Code item constructor
        /// </summary>
        public EthosApiBuilder() : base() { }
    }
}
