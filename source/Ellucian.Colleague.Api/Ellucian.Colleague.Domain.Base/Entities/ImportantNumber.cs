// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities 
{
    /// <summary>
    /// Representation of an important number
    /// </summary>
    [Serializable]
    public class ImportantNumber 
    {

        private string _Id;
        private readonly string _Name;
        private readonly string _City;
        private readonly string _State;
        private readonly string _PostalCode;
        private readonly string _Country;
        private readonly string _Phone;
        private readonly string _Extension;
        private readonly string _Category;
        private readonly string _Email;
        private readonly List<string> _AddressLines;
        private readonly string _BuildingCode;
        private readonly bool _OfficeUseOnly;
        private readonly decimal? _Latitude;
        private readonly decimal? _Longitude;
        private readonly string _LocationCode;

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get { return _Id; } }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get { return _Name; } }

        // check on validation in colleague for city, state, country
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
        /// Gets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        public string Phone { get { return _Phone; } }

        /// <summary>
        /// Gets the extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public string Extension { get { return _Extension; } }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get { return _Category; } }

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get { return _Email; } }

        /// <summary>
        /// Gets the address lines.
        /// </summary>
        /// <value>
        /// The address lines.
        /// </value>
        public List<string> AddressLines { get { return _AddressLines; } }

        /// <summary>
        /// Gets the building code.
        /// </summary>
        /// <value>
        /// The building code.
        /// </value>
        public string BuildingCode { get { return _BuildingCode; } }

        /// <summary>
        /// Gets a value indicating whether [office use only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [office use only]; otherwise, <c>false</c>.
        /// </value>
        public bool OfficeUseOnly { get { return _OfficeUseOnly; } }

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public decimal? Latitude { get { return _Latitude; } }

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public decimal? Longitude { get { return _Longitude; } }

        /// <summary>
        /// Gets the location code.
        /// </summary>
        /// <value>
        /// The location code.
        /// </value>
        public string LocationCode { get { return _LocationCode; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportantNumber"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="phone">The phone.</param>
        /// <param name="extension">The extension.</param>
        /// <param name="category">The category.</param>
        /// <param name="email">The email.</param>
        /// <param name="buildingCode">The building code.</param>
        /// <param name="visibleToMobile">The visible to mobile.</param>
        /// <exception cref="System.ArgumentNullException">
        /// id;Id is required
        /// or
        /// name;Name is required
        /// or
        /// phone;Phone is required
        /// or
        /// category;Category is required
        /// or
        /// visibleToMobile;Visible To Mobile flag is required
        /// </exception>
        public ImportantNumber(string id,
                               string name, 
                               string phone, 
                               string extension,
                               string category,
                               string email, 
                               string buildingCode,
                               string visibleToMobile) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException("id", "Id is required");
            }
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name", "Name is required");
            }
            if (string.IsNullOrEmpty(phone)) {
                throw new ArgumentNullException("phone", "Phone is required");
            }
            if (string.IsNullOrEmpty(category)) {
                throw new ArgumentNullException("category", "Category is required");
            }
            if (string.IsNullOrEmpty(visibleToMobile)) {
                throw new ArgumentNullException("visibleToMobile", "Visible To Mobile flag is required");
            }
            // required fields
            _Id = id;
            _Name = name;
            _Category = category;
            _Phone = phone;
            _OfficeUseOnly = (visibleToMobile == "Y") ? false : true;
            _BuildingCode = buildingCode;
            // optional fields
            _Extension = extension;
            _Email = email;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportantNumber"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The postal code.</param>
        /// <param name="country">The country.</param>
        /// <param name="phone">The phone.</param>
        /// <param name="extension">The extension.</param>
        /// <param name="category">The category.</param>
        /// <param name="email">The email.</param>
        /// <param name="addressLines">The address lines.</param>
        /// <param name="visibleToMobile">The visible to mobile.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="locationCode">The location code.</param>
        /// <exception cref="System.ArgumentNullException">
        /// id;Id is required
        /// or
        /// name;Name is required
        /// or
        /// phone;Phone is required
        /// or
        /// category;Category is required
        /// or
        /// visibleToMobile;Visible To Mobile flag is required
        /// </exception>
        public ImportantNumber(string id, 
                               string name, 
                               string city, 
                               string state, 
                               string postalCode, 
                               string country, 
                               string phone, 
                               string extension, 
                               string category, 
                               string email, 
                               List<string> addressLines, 
                               string visibleToMobile,
                               decimal? latitude, 
                               decimal? longitude,
                               string locationCode) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException("id", "Id is required");
            }
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name", "Name is required");
            }
            if (string.IsNullOrEmpty(phone)) {
                throw new ArgumentNullException("phone", "Phone is required");
            }
            if (string.IsNullOrEmpty(category)) {
                throw new ArgumentNullException("category", "Category is required");
            }
            if (string.IsNullOrEmpty(visibleToMobile)) {
                throw new ArgumentNullException("visibleToMobile", "Visible To Mobile flag is required");
            }
            // required fields
            _Id = id;
            _Name = name;
            _Category = category;
            _Phone = phone;
            _OfficeUseOnly = (visibleToMobile == "Y") ? false : true;
            // optional fields
            _Extension = extension;
            _Email = email;
            if (addressLines != null) {
                _AddressLines = new List<String>();
                _AddressLines.AddRange(addressLines);
            }
            if (!string.IsNullOrEmpty(city)) { _City = city; }
            if (!string.IsNullOrEmpty(state)) { _State = state; }
            if (!string.IsNullOrEmpty(postalCode)) { _PostalCode = postalCode; }
            if (!string.IsNullOrEmpty(country)) { _Country = country; }
            _Latitude = latitude;
            _Longitude = longitude;
            // locationCode is optional for this case
            if (!string.IsNullOrEmpty(locationCode)) { _LocationCode = locationCode; }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" }, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            ImportantNumber imp = obj as ImportantNumber;
            if (imp == null) {
                return false;
            }
            return imp.Id.Equals(Id);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() {
            return Id.GetHashCode();
        }

    }
}
