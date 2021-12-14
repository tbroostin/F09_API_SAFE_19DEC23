/*Copyright 2014-2021 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class Address
    {
        // Required fields
        
        /// <summary>
        /// Unique ID of the address
        /// </summary>
        public string AddressId { get { return _addressId; } }
        private readonly string _addressId;
        
        /// <summary>
        /// Person related to this address
        /// </summary>
        public string PersonId { get { return _personId; } }
        private readonly string _personId;

        // Non-required fields

        private string _Guid;
        /// <summary>
        /// GUID for the address; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _Guid; }
            set
            {
                if (string.IsNullOrEmpty(_Guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _Guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }
        /// <summary>
        /// The address type description. May be multiple values separate by commas.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The address type code. Code from AddressType which also indicates the nature of the
        /// address, such as home, business, etc. For address updates submitted from the web, may
        /// be a web address type. May have multiple values separated by commas.
        /// </summary>
        public string TypeCode { get; set; }
        
        /// <summary>
        /// The street address lines
        /// </summary>
        public List<string> AddressLines { get; set; }
        
        /// <summary>
        /// A line to appear just before the street address lines in the address label.
        /// Sometimes used to designate Care Of, or similar
        /// </summary>
        public string AddressModifier { get; set; }

        /// <summary>
        /// The city
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// State or province
        /// </summary>
        public string State { get; set; }
        
        /// <summary>
        /// Postal code, or zip code for US addresses
        /// </summary>
        public string PostalCode { get; set; }
        
        /// <summary>
        /// County code
        /// </summary>
        public string County { get; set; }
        
        /// <summary>
        /// Country description translation of CountryCode
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Country code. Likely null for US addresses
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Route Code
        /// </summary>
        public string RouteCode { get; set; }

        private readonly List<Phone> _phoneNumbers = new List<Phone>();
        
        /// <summary>
        /// List of phones associated with the address.
        /// </summary>
        public ReadOnlyCollection<Phone> PhoneNumbers { get; private set; }
        
        /// <summary>
        /// Address label built from address components
        /// </summary>
        public List<string> AddressLabel { get; set; }
        
        /// <summary>
        /// The data this address goes into effect
        /// </summary>
        public DateTime? EffectiveStartDate { get; set; }

        /// <summary>
        /// The date this address is obsolete
        /// </summary>
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>
        /// Seasonal Start and End dates
        /// </summary>
        public List<AddressSeasonalDates> SeasonalDates { get; set; }

        /// <summary>
        /// Indicates if this is the person's preferred mailing address
        /// </summary>
        public bool IsPreferredAddress { get; set; }

        /// <summary>
        /// Indicates if this is the person's residence address
        /// </summary>
        public bool IsPreferredResidence { get; set; }

        /// <summary>
        /// Store the city for addresses outside the US/Canada
        /// </summary>
        public string IntlLocality { get; set; }

        /// <summary>
        /// Store regions (e.g. state/province) for addresses outside the US/Canada
        /// </summary>
        public string IntlRegion { get; set; }

        /// <summary>
        /// Store sub-regions (e.g. counties) for addresses outside the US/Canada
        /// </summary>
        public string IntlSubRegion { get; set; }

        /// <summary>
        /// Store postal codes (e.g. zip code) for addresses outside the US/Canada
        /// </summary>
        public string IntlPostalCode { get; set; }

        /// <summary>
        /// Store the delivery point digits (for US addresses only).
        /// </summary>
        public string DeliveryPoint { get; set; }

        /// <summary>
        /// Store the correction digit (for US addresses only).
        /// </summary>
        public string CorrectionDigit { get; set; }

        /// <summary>
        /// A subdivision of a US zipcode.
        /// </summary>
        public string CarrierRoute { get; set; }

        /// <summary>
        /// Chapter
        /// </summary>
        public List<string> AddressChapter { get; set; }

        /// <summary>
        /// Status of Current or Former
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Store the address latitude for any address regardless of the country
        /// </summary>
        public Decimal? Latitude { get; set; }

        /// <summary>
        /// Store the address longitude for any address regardless of the country
        /// </summary>
        public Decimal? Longitude { get; set; }

        /// <summary>
        /// The list of different address types - this address has
        /// </summary>
        public List<string> AddressTypeCodes { get; set; }

        /// <summary>
        /// Constructor used by HEDM
        /// </summary>
        public Address()
        {
            AddressLines = new List<string>();
            AddressLabel = new List<string>();
            PhoneNumbers = _phoneNumbers.AsReadOnly();
        }

        /// <summary>
        /// Constructor used by HEDM
        /// </summary>
        public Address(string addressId, bool isPreferredAddress)
        {
            if (string.IsNullOrEmpty(addressId))
            {
                throw new ArgumentNullException("addressId");
            }

            AddressLines = new List<string>();
            AddressLabel = new List<string>();
            PhoneNumbers = _phoneNumbers.AsReadOnly();
            _addressId = addressId;
            IsPreferredAddress = isPreferredAddress;
        }

        /// <summary>
        /// Constructor used for new addresses
        /// </summary>
        public Address(string personId)
        {
            _addressId = null;
            _personId = personId;
            AddressLines = new List<string>();
            AddressLabel = new List<string>();
            PhoneNumbers = _phoneNumbers.AsReadOnly();
        }

        /// <summary>
        /// Address Constructor. Only AddressID and PersonID required.
        /// </summary>
        /// <param name="addressId"></param>
        /// <param name="personId"></param>
        public Address(string addressId, string personId)
            : this(personId)
        {
            if (string.IsNullOrEmpty(addressId))
            {
                throw new ArgumentNullException("addressId");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            _addressId = addressId;
            AddressTypeCodes = new List<string>();
        }
        
        /// <summary>
        /// Add a phone to the address
        /// </summary>
        /// <param name="phone"></param>
        public void AddPhone(Phone phone)
        {
            if (phone == null)
            {
                throw new ArgumentNullException("phone", "Phone must be specified");
            }
            if (_phoneNumbers.Where(f => f.Equals(phone)).Count() > 0)
            {
                throw new ArgumentException("Phone number already exists in this list");
            }
            _phoneNumbers.Add(phone);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Address">Address</see> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="Address">Address</see> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="Address">Address</see> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Address other = obj as Address;
            if (other == null)
            {
                return false;
            }
            return
                (string.IsNullOrEmpty(AddressId) ? string.IsNullOrEmpty(other.AddressId) : AddressId.Equals(other.AddressId)) &&
                (string.IsNullOrEmpty(TypeCode) ? string.IsNullOrEmpty(other.TypeCode) : TypeCode.Equals(other.TypeCode)) &&
                (AddressLines == null || AddressLines.Count() == 0 ? (other.AddressLines == null || other.AddressLines.Count() == 0) : (other.AddressLines == null ? false : AddressLines.SequenceEqual(other.AddressLines))) &&
                (string.IsNullOrEmpty(AddressModifier) ? string.IsNullOrEmpty(other.AddressModifier) : AddressModifier.Equals(other.AddressModifier)) &&
                (string.IsNullOrEmpty(City) ? string.IsNullOrEmpty(other.City) : City.Equals(other.City)) &&
                (string.IsNullOrEmpty(State) ? string.IsNullOrEmpty(other.State) : State.Equals(other.State)) &&
                (string.IsNullOrEmpty(PostalCode) ? string.IsNullOrEmpty(other.PostalCode) : PostalCode.Equals(other.PostalCode)) &&
                (string.IsNullOrEmpty(CountryCode) ? string.IsNullOrEmpty(other.CountryCode) : CountryCode.Equals(other.CountryCode)) &&
                (string.IsNullOrEmpty(County) ? string.IsNullOrEmpty(other.County) : County.Equals(other.County)) &&
                (string.IsNullOrEmpty(RouteCode) ? string.IsNullOrEmpty(other.RouteCode) : RouteCode.Equals(other.RouteCode)) &&
                (PhoneNumbers == null || PhoneNumbers.Count() == 0 ? (other.PhoneNumbers == null || other.PhoneNumbers.Count() == 0) : (other.PhoneNumbers == null ? false : PhoneNumbers.SequenceEqual(other.PhoneNumbers))) &&
                (string.IsNullOrEmpty(IntlLocality) ? string.IsNullOrEmpty(other.IntlLocality) : City.Equals(other.IntlLocality)) &&
                (string.IsNullOrEmpty(IntlRegion) ? string.IsNullOrEmpty(other.IntlRegion) : City.Equals(other.IntlRegion)) &&
                (string.IsNullOrEmpty(IntlSubRegion) ? string.IsNullOrEmpty(other.IntlSubRegion) : City.Equals(other.IntlSubRegion)) &&
                (string.IsNullOrEmpty(IntlPostalCode) ? string.IsNullOrEmpty(other.IntlPostalCode) : City.Equals(other.IntlPostalCode));
//                (EffectiveStartDate == null ? other.EffectiveStartDate == null : (other.EffectiveStartDate == null ? false : EffectiveStartDate.Equals(other.EffectiveStartDate))) &&
//                (EffectiveEndDate == null ? other.EffectiveEndDate == null : (other.EffectiveEndDate == null ? false : EffectiveEndDate.Equals(other.EffectiveEndDate)));
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return AddressId.GetHashCode();
        }
    }
}
