// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a geographic area
    /// </summary>
    [DataContract]
    public class GeographicArea : CodeItem2
    {
        /// <summary>
        /// Array of geographic-areas guids within an area.
        /// </summary>
        [DataMember(Name = "includedAreas", EmitDefaultValue = false)]
        public List<GuidObject2> IncludedAreas { get; set; }

        /// <summary>
        /// Geographic area type of an area.
        /// </summary>
        [DataMember(Name = "type")]
        public GeographicAreaTypeProperty Type { get; set; }
    }
}