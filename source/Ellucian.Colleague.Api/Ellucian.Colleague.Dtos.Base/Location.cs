// Copyright 2013-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Institutionally defined area
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Unique code of this location
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Public description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Buildings relevant to this location
        /// </summary>
        public List<string> BuildingCodes { get; set; }
        /// <summary>
        /// Geographical <see cref="Coordinate">coordinates</see> of northwest corner of conceptual location "box"
        /// </summary>
        public Coordinate NorthWestCoordinate { get; set; }
        /// <summary>
        /// Geographical <see cref="Coordinate">coordinates</see> of the southeast corner of conceptual location "box"
        /// </summary>
        public Coordinate SouthEastCoordinate { get; set; }
        /// <summary>
        /// Campus Location
        /// </summary>
        public string CampusLocation { get; set; }
        /// <summary>
        /// Address Lines for the Location if applicable
        /// </summary>
        public List<string> AddressLines { get; set; }
        /// <summary>
        /// City of this location (optional)
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// State or Province for this location (optional)
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Postal Code or Zip Code for this location (optional)
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// Country of Location (optional)
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Flag indicating whether or not this location should be hidden from Colleague Self-Service course searches
        /// </summary>
        public bool HideInSelfServiceCourseSearch { get; set; }
        /// <summary>
        /// Order in which this location should be sorted relative to other <see cref="Location">locations</see> 
        /// </summary>
        public int? SortOrder { get; set; }
    }
}
