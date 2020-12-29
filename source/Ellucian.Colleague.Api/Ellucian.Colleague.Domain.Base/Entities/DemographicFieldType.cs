// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
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
        Unknown,
        /// <summary>
        /// Street address line 1
        /// </summary>
        AddressLine1,
        /// <summary>
        /// Street address line 2
        /// </summary>
        AddressLine2,
        /// <summary>
        /// Person's city of residence
        /// </summary>
        City,
        /// <summary>
        /// Person's state or province of residence
        /// </summary>
        StateProvince,
        /// <summary>
        /// Person's postal code of residence
        /// </summary>
        PostalCode,
        /// <summary>
        /// Person's county of residence
        /// </summary>
        County,
        /// <summary>
        /// Person's country of residence
        /// </summary>
        AddressCountry,
        /// <summary>
        /// Person's country of citizenship
        /// </summary>
        CitizenshipCountry,
        /// <summary>
        /// Person's ethnicity
        /// </summary>
        Ethnicity,
        /// <summary>
        /// Person's race
        /// </summary>
        Race,
        /// <summary>
        /// Street address line 3
        /// </summary>
        AddressLine3,
        /// <summary>
        /// Street address line 4
        /// </summary>
        AddressLine4
    }
}