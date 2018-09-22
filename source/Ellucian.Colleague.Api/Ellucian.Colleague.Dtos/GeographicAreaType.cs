// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a geographic area type
    /// </summary>
    [DataContract]
    public class GeographicAreaType : CodeItem2
    {
        /// <summary>
        /// The <see cref="GeographicAreaTypeCategory">entity type</see> for the geographic area type categories
        /// </summary>
        [DataMember(Name = "category")]
        public GeographicAreaTypeCategory geographicAreaTypeCategory { get; set; }
    }
}