// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of buildings.
    /// </summary>
    [Serializable]
    public class Building : GuidCodeItem
    {
        private readonly string _LocationId;
        private readonly string _BuildingType;
        private readonly string _LongDescription;
        private readonly List<string> _AddressLines;
        private readonly string _City;
        private readonly string _State;
        private readonly string _PostalCode;
        private readonly string _Country;
        private readonly string _ImageUrl;
        private readonly string _AdditionalServices;
        private readonly string _Comments;
        private readonly Coordinate _BuildingCoordinate;
        private readonly decimal? _Latitude;
        private readonly decimal? _Longitude;
        private readonly string _ExportToMobile;

        /// <summary>
        /// Gets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public string LocationId { get { return _LocationId; } }

        /// <summary>
        /// Gets the type of the building.
        /// </summary>
        /// <value>
        /// The type of the building.
        /// </value>
        public string BuildingType { get { return _BuildingType; } }

        /// <summary>
        /// Gets the long description.
        /// </summary>
        /// <value>
        /// The long description.
        /// </value>
        public string LongDescription { get { return _LongDescription; } }

        /// <summary>
        /// Gets the address lines.
        /// </summary>
        /// <value>
        /// The address lines.
        /// </value>
        public List<string> AddressLines { get { return _AddressLines; } }

        /// <summary>
        /// Gets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        public string City { get { return _City; } }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get { return _State; } }

        /// <summary>
        /// Gets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        public string PostalCode { get { return _PostalCode; } }

        /// <summary>
        /// Gets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public string Country { get { return _Country; } }

        /// <summary>
        /// Gets the image URL.
        /// </summary>
        /// <value>
        /// The image URL.
        /// </value>
        public string ImageUrl { get { return _ImageUrl; } }

        /// <summary>
        /// Gets the additional services.
        /// </summary>
        /// <value>
        /// The additional services.
        /// </value>
        public string AdditionalServices { get { return _AdditionalServices; } }

        /// <summary>
        /// Gets the additional services.
        /// </summary>
        /// <value>
        /// The building comments.
        /// </value>
        public string Comments { get { return _Comments; } }

        /// <summary>
        /// Gets the building coordinate.
        /// </summary>
        /// <value>
        /// The building coordinate.
        /// </value>
        public Coordinate BuildingCoordinate { get { return _BuildingCoordinate; } }

        /// <summary>
        /// Gets the building latitude.
        /// </summary>
        /// <value>
        /// The building latitude.
        /// </value>
        public decimal? Latitude { get { return _Latitude; } }

        /// <summary>
        /// Gets the building longitude.
        /// </summary>
        /// <value>
        /// The building longitude.
        /// </value>
        public decimal? Longitude { get { return _Longitude; } }

        /// <summary>
        /// Gets the flag to export to mobile.
        /// </summary>
        /// <value>
        /// The building flag to export to mobile
        /// </value>
        public string ExportToMobile { get { return _ExportToMobile; } }


        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public Building(string guid, string code, string description)
            : base(guid, code, description)
        {
            bool officeUseOnly = true;
            _BuildingCoordinate = new Coordinate(null, null, officeUseOnly);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class.
        /// </summary>
        /// <param name="guid">The GUID for the building</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="bldgType">Type of the building.</param>
        /// <param name="longDesc">The long description.</param>
        /// <param name="addressLines">The address lines.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="imageUrl">The image URL.</param>
        /// <param name="addnlServices">The additional services.</param>
        /// <param name="visibleForMobile">The visible for mobile.</param>
        public Building(string guid, string code, string description, string locationId, string bldgType, string longDesc, List<string> addressLines, string city, string state, string postalCode, string country, decimal? latitude, decimal? longitude, string imageUrl, string addnlServices, string visibleForMobile)
            : base(guid, code, description)
        {
            if (!string.IsNullOrEmpty(locationId)) { _LocationId = locationId; }
            if (!string.IsNullOrEmpty(bldgType)) { _BuildingType = bldgType; }
            if (!string.IsNullOrEmpty(longDesc)) { _LongDescription = longDesc; }
            if (addressLines != null) {
                _AddressLines = new List<string>();
                _AddressLines.AddRange(addressLines);
            }
            if (!string.IsNullOrEmpty(city)) { _City = city; }
            if (!string.IsNullOrEmpty(state)) { _State = state; }
            if (!string.IsNullOrEmpty(postalCode)) { _PostalCode = postalCode; }
            if (!string.IsNullOrEmpty(country)) { _Country = country; }
            if (!string.IsNullOrEmpty(imageUrl)) { _ImageUrl = imageUrl; }
            if (!string.IsNullOrEmpty(addnlServices)) { _AdditionalServices = addnlServices; }
            _BuildingCoordinate = null;
            if (latitude.HasValue && longitude.HasValue) {
                bool officeUseOnly = true;
                if (!string.IsNullOrEmpty(visibleForMobile) && visibleForMobile.Equals("Y")) {
                    officeUseOnly = false;
                }
                _BuildingCoordinate = new Coordinate(latitude, longitude, officeUseOnly);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class.
        /// </summary>
        /// <param name="guid">The GUID for the building</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="longDesc">The long description.</param>
        /// <param name="addressLines">The address lines.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="imageUrl">The image URL.</param>
        /// <param name="addnlServices">The additional services.</param>
        /// <param name="visibleForMobile">The comments.</param>
        public Building(string guid, string code, string description, string locationId, string longDesc, List<string> addressLines, string city, string state, string postalCode, string country, decimal? latitude, decimal? longitude, string imageUrl, string addnlServices, string comments, string exportToMobile)
            : base(guid, code, description)
        {
            _LocationId = locationId;
            _LongDescription = longDesc;
            _AddressLines = new List<string>();
            if (addressLines.Count >= 0)
            {
                _AddressLines.AddRange(addressLines);
            }
            _City = city;
            _State = state;
            _PostalCode = postalCode;
            _Country = country;
            _ImageUrl = imageUrl;
            _AdditionalServices = addnlServices;
            _Comments = comments;
            _Latitude = latitude;
            _Longitude = longitude;
            _ExportToMobile = exportToMobile;
        }
    }
}
