// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Mapping between a business event, resource, version, and path segment
    /// </summary>
    [DataContract]
    public class ResourceBusinessEventMapping
    {
        /// <summary>
        /// Name of the mapped resource
        /// </summary>
        [DataMember(Name = "resource")]
        public string ResourceName { get; set; }

        /// <summary>
        /// Version number of the mapped resource
        /// </summary>
        [DataMember(Name = "version")]
        public string ResourceVersion { get; set; }

        /// <summary>
        /// Segment of the API endpoint for the mapped resource
        /// </summary>
        [DataMember(Name = "path")]
        public string PathSegment { get; set; }

        /// <summary>
        /// Collection of business events to which the resource is mapped
        /// </summary>
        [DataMember(Name = "events")]
        public List<string> BusinessEvents { get; set; }
    }
}