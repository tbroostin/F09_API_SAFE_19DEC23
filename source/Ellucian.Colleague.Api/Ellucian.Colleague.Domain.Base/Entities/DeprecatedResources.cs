// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The Resources API provides a mechanism by which callers can view a list of Ethos resources that are exposed through the server API.
    /// </summary>
    [Serializable]
    public class DeprecatedResources
    {
        /// <summary>
        /// The full name of a room.
        /// </summary>
        //[JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// A description of a room.
        /// </summary>
       // [JsonProperty("representations")]
        public List<Representation> Representations { get; set; }
    }

    /// <summary>
    /// Representation
    /// </summary>
    [Serializable]
    public class Representation
    {
        /// <summary>
        /// X-Media-Type
        /// </summary>
        [JsonProperty("X-Media-Type")]
        public string XMediaType { get; set; }

        /// <summary>
        /// Methods GET, PUT, POST and DELETE
        /// </summary>
        //[JsonProperty("methods")]
        public List<string> Methods { get; set; }

        /// <summary>
        /// Deprecation notice
        /// </summary>
        //[JsonProperty("deprecationNotice", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DeprecationNotice DeprecationNotice { get; set; }

    }

    /// <summary>
    /// DeprecationNotice
    /// </summary>
    [Serializable]
    public class DeprecationNotice
    {
        /// <summary>
        /// Deprecated On
        /// </summary>
        //[JsonProperty("deprecatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DeprecatedOn { get; set; }

        /// <summary>
        /// Sunset On
        /// </summary>
        //[JsonProperty("sunsetOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? SunsetOn { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        //[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }
}