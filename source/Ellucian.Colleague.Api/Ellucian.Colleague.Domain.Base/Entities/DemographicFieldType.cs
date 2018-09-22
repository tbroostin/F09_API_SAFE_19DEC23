// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Defines the various types of demographic fields
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DemographicFieldType
    {
        /// <summary>
        /// Prefix for a person's name
        /// </summary>
        Prefix,
        /// <summary>
        /// Person's first name
        /// </summary>
        FirstName,
        /// <summary>
        /// Person's middle name
        /// </summary>
        MiddleName,
        /// <summary>
        /// Person's last name
        /// </summary>
        LastName,
        /// <summary>
        /// Suffix for a person's name
        /// </summary>
        Suffix,
        /// <summary>
        /// Person's former first name
        /// </summary>
        FormerFirstName,
        /// <summary>
        /// Person's former middle name
        /// </summary>
        FormerMiddleName,
        /// <summary>
        /// Person's former last name
        /// </summary>
        FormerLastName,
        /// <summary>
        /// Person's email address
        /// </summary>
        EmailAddress,
        /// <summary>
        /// Person's email address type
        /// </summary>
        EmailType,
        ///// <summary>
        ///// Street address lines
        ///// </summary>
        //AddressLines,
        ///// <summary>
        ///// Street address type
        ///// </summary>
        //AddressType,
        ///// <summary>
        ///// Person's city of residence
        ///// </summary>
        //City,
        ///// <summary>
        ///// Person's state or province of residence
        ///// </summary>
        //StateProvince,
        ///// <summary>
        ///// Person's postal code of residence
        ///// </summary>
        //PostalCode,
        ///// <summary>
        ///// Person's country of residence
        ///// </summary>
        //Country,
        /// <summary>
        /// Person's telephone number
        /// </summary>
        Phone,
        /// <summary>
        /// Person's telephone number extension
        /// </summary>
        PhoneExtension,
        /// <summary>
        /// Person's telephone number type
        /// </summary>
        PhoneType,
        /// <summary>
        /// Person's date of birth
        /// </summary>
        BirthDate,
        /// <summary>
        /// Person's gender
        /// </summary>
        Gender,
        /// <summary>
        /// Person's government ID
        /// </summary>
        GovernmentId,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }
}