// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Converters;
using System;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The Resources API provides a mechanism by which callers can view a list of Ethos resources that are exposed through the server API.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiResources
    {
        /// <summary>
        /// The full name of a room.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// A description of a room.
        /// </summary>
        [JsonProperty("representations")]
        public List<Representation> Representations { get; set; }
    }

    /// <summary>
    /// Representation
    /// </summary>
    public class Representation
    {
        /// <summary>
        /// X-Media-Type
        /// </summary>
        [JsonProperty("X-Media-Type", NullValueHandling = NullValueHandling.Ignore)]
        public string XMediaType { get; set; }

        /// <summary>
        /// Methods GET, PUT, POST and DELETE
        /// </summary>
        [JsonProperty("methods")]
        public List<string> Methods { get; set; }

        /// <summary>
        /// Array of any supported filters by the API in json dot notation. This is not included if no filters are supported
        /// </summary>
        [JsonProperty("filters", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Filters { get; set; }

        /// <summary>
        /// Array of any supported named queries by the API in json dot notation. This is not included if no named queries are supported.
        /// </summary>
        [JsonProperty("namedQueries", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<NamedQuery> NamedQueries { get; set; }

        /// <summary>
        /// Deprecation notice
        /// </summary>
        [JsonProperty("deprecationNotice", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DeprecationNotice DeprecationNotice { get; set; }
    }

    /// <summary>
    /// NamedQuery
    /// </summary>
    public class NamedQuery
    {
        /// <summary>
        /// Name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        /// <summary>
        /// Array of any supported filters by the API in json dot notation. This is not included if no filters are supported
        /// </summary>
        [JsonProperty("filters", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Filters { get; set; }
    }

    /// <summary>
    /// DeprecationNotice
    /// </summary>
    public class DeprecationNotice
    {
        /// <summary>
        /// Deprecated On
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("deprecatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DeprecatedOn { get; set; }

        /// <summary>
        /// Sunset On
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [JsonProperty("sunsetOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? SunsetOn { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }
}