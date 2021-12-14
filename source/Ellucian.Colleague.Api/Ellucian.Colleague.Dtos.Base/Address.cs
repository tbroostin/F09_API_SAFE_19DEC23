/*Copyright 2014-2021 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Address 
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Key to the Address Record
        /// </summary>
        public string AddressId { get; set; }
        /// <summary>
        /// Type of Address when taken from a student/person perspective
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Address Lines for street, apartment, etc.
        /// </summary>
        public List<string> AddressLines { get; set; }
        /// <summary>
        /// Modifier Line sometimes used to designate Care Of, or similar
        /// </summary>
        public string AddressModifier { get; set; }
        /// <summary>
        /// City for the Address
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// State or Province for the Address
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Zip code or other Postal Code for the Address
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// County for the Address
        /// </summary>
        public string County { get; set; }
        /// <summary>
        /// The translated Country 
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Route Code associated to this Address
        /// </summary>
        public string RouteCode { get; set; }
        /// <summary>
        /// Address Related Phone numbers and Personal Phone numbers
        /// </summary>
        public ICollection<Phone> PhoneNumbers { get; set; }
        /// <summary>
        /// Full Address Label from the Address Data
        /// </summary>
        public List<string> AddressLabel { get; set; }
        /// <summary>
        /// Person residing at this address
        /// </summary>
        public string PersonId { get; set; }
        /// <summary>
        /// Date that this address is effective
        /// </summary>
        public DateTime? EffectiveStartDate { get; set; }
        /// <summary>
        /// Date that this address is no longer active
        /// </summary>
        /// todo: Change to properties instead of members.
        public DateTime? EffectiveEndDate { get; set; }
        /// <summary>
        /// If this is the preferred address, then Yes, otherwise No.
        /// </summary>
        public bool IsPreferredAddress { get; set; }
        /// <summary>
        /// If this is the preferred residence, then Yes, otherwise No.
        /// </summary>
        public bool IsPreferredResidence { get; set; }
        /// <summary>
        /// The untranslated code indicating the type of address.
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// The untranslate country code. Country contains the translated value that the end-user sees.
        /// </summary>
        public string CountryCode { get; set; }
        /// <summary>
        /// The list of different address types - this address has
        /// </summary>
        public List<string> AddressTypeCodes { get; set; }
    }
}