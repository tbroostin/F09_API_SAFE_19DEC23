// Copyright 2013-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of locations
    /// </summary>
    [Serializable]
    public class Location : GuidCodeItem
    {
        private readonly List<string> _BuildingCodes = new List<string>();
        private readonly Coordinate _NorthWestCoordinate;
        private readonly Coordinate _SouthEastCoordinate;
        private readonly bool _hideInSelfServiceCourseSearch;
        private int? _sortOrder;

        /// <summary>
        /// Gets the building codes.
        /// </summary>
        /// <value>
        /// The building codes.
        /// </value>
        public ReadOnlyCollection<string> BuildingCodes { get; private set; }

        /// <summary>
        /// Gets the northwest coordinate.
        /// </summary>
        /// <value>
        /// The north west coordinate.
        /// </value>
        public Coordinate NorthWestCoordinate { get { return _NorthWestCoordinate; } }

        /// <summary>
        /// Gets the southeast coordinate.
        /// </summary>
        /// <value>
        /// The southeast coordinate.
        /// </value>
        public Coordinate SouthEastCoordinate { get { return _SouthEastCoordinate; } }
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
        public bool HideInSelfServiceCourseSearch { get { return _hideInSelfServiceCourseSearch; } }

        /// <summary>
        /// Order in which this location should be sorted relative to other <see cref="Location">locations</see> 
        /// </summary>
        /// <remarks>A sort order of 1 is the highest priority. A null value is treated as lowest priority.
        /// </remarks>
        public int? SortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                if (value.HasValue && (value.Value <= 0 || value.Value > 9999))
                {
                    throw new InvalidOperationException("If specified, location sort order must be greater than zero and less than 9999.");
                }
                _sortOrder = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="hideInSelfServiceCourseSearch">Flag indicating whether or not this location should be hidden from Colleague Self-Service course searches</param>
        public Location(string guid, string code, string description, bool hideInSelfServiceCourseSearch = false)
            : base(guid, code, description)
        {
            bool officeUseOnly = true;
            _NorthWestCoordinate = new Coordinate(null, null, officeUseOnly);
            _SouthEastCoordinate = new Coordinate(null, null, officeUseOnly);

            BuildingCodes = _BuildingCodes.AsReadOnly();

            _hideInSelfServiceCourseSearch = hideInSelfServiceCourseSearch;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class.
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="nwlat">The NW latitude.</param>
        /// <param name="nwlong">The NW longitude.</param>
        /// <param name="selat">The SE latitude.</param>
        /// <param name="selong">The SE longitude.</param>
        /// <param name="visibleForMobile">Visibility for mobile.</param>
        /// <param name="buildings">The buildings.</param>
        /// <param name="hideInSelfServiceCourseSearch">Flag indicating whether or not this location should be hidden from Colleague Self-Service course searches</param>
        /// <exception cref="System.ArgumentNullException">
        /// NW latitude
        /// or
        /// NW longitude
        /// or
        /// SE latitude
        /// or
        /// SE longitude
        /// </exception>
        public Location(string guid, string code, string description, decimal? nwlat, decimal? nwlong, decimal? selat, decimal? selong, string visibleForMobile, List<string> buildings, bool hideInSelfServiceCourseSearch = false) 
            : this(guid, code, description, hideInSelfServiceCourseSearch)
        {
            if (!string.IsNullOrEmpty(visibleForMobile) && visibleForMobile.Equals("Y")) {
                if (nwlat == null) throw new ArgumentNullException("nwlat");
                if (nwlong == null) throw new ArgumentNullException("nwlong");
                if (selat == null) throw new ArgumentNullException("selat");
                if (selong == null) throw new ArgumentNullException("selong");
            }

            if (buildings != null && buildings.Count > 0)
            {
                foreach (string buildingCode in buildings)
                {
                    AddBuilding(buildingCode);
                }
            }

            bool officeUseOnly = true;
            if (nwlat.HasValue && nwlong.HasValue && selat.HasValue && selong.HasValue) {
                if (!string.IsNullOrEmpty(visibleForMobile) && visibleForMobile.Equals("Y")) {
                    officeUseOnly = false;
                }
            }
            _NorthWestCoordinate = new Coordinate(nwlat, nwlong, officeUseOnly);
            _SouthEastCoordinate = new Coordinate(selat, selong, officeUseOnly);

            BuildingCodes = _BuildingCodes.AsReadOnly();
        }

        /// <summary>
        /// Add a building code to this location
        /// </summary>
        /// <param name="buildingCode">Building code</param>
        public void AddBuilding(string buildingCode)
        {
            if (string.IsNullOrEmpty(buildingCode))
            {
                throw new ArgumentNullException("buildingCode");
            }
            if (!_BuildingCodes.Contains(buildingCode))
            {
                _BuildingCodes.Add(buildingCode);
            }
        }
    }
}
