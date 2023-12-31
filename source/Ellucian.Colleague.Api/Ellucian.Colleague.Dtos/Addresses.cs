﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A designation of a student's progress, based on the number of completed courses
    /// </summary>
    [DataContract]
    public class Addresses : BaseModel2
    {

        /// <summary>
        /// The address lines of the address, such as a street address, post office box number, or city, region, and postal code
        /// </summary>
        [DataMember(Name = "addressLines")]
        public List<string> AddressLines { get; set; }

       /// <summary>
        /// A country specific postal region.  Postal regions are expressed as a hierarchy of country, region, and sub-region, and as a locality with postal automation codes
        /// </summary> 
        [DataMember(Name = "place", EmitDefaultValue = false)]
        public AddressPlace Place { get; set; }

        /// <summary>
        /// The latitude of the location.
        /// </summary>
        [DataMember(Name = "latitude", EmitDefaultValue=false)]
        public Decimal? Latitude { get; set; }

        /// <summary>
        /// The longitude of the location.
        /// </summary>
        [DataMember(Name = "longitude", EmitDefaultValue = false)]
        public Decimal? Longitude { get; set; }

        /// <summary>
        /// The geographic areas assigned to this location.
        /// </summary>
        [DataMember(Name = "geographicAreas", EmitDefaultValue=false)]
        public List<GuidObject2> GeographicAreas { get; set; }

    }
}