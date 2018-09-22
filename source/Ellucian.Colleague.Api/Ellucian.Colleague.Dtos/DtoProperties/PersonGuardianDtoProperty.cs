// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Information about a persons guardian.
    /// </summary>
    [DataContract]
    public class PersonGuardianDtoProperty
    {
        /// <summary>
        /// Guid of the guardian
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}