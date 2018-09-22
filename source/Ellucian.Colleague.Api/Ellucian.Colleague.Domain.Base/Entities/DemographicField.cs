// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A demographic field potentially collected when adding persons to Colleague via Proxy interfaces
    /// </summary>
    [Serializable]
    public class DemographicField : CodeItem
    {
        private DemographicFieldRequirement _requirement;

        /// <summary>
        /// Field requirement type
        /// </summary>
        public DemographicFieldRequirement Requirement { get { return _requirement; } }

        /// <summary>
        /// The type of field
        /// </summary
        public DemographicFieldType Type
        {
            get
            {
                var upperCode = Code.ToUpperInvariant();
                switch (upperCode)
                {
                    case "PREFIX":
                        return DemographicFieldType.Prefix;
                    case "FIRST_NAME":
                        return DemographicFieldType.FirstName;
                    case "MIDDLE_NAME":
                        return DemographicFieldType.MiddleName;
                    case "LAST_NAME":
                        return DemographicFieldType.LastName;
                    case "SUFFIX":
                        return DemographicFieldType.Suffix;
                    case "FORMER_FIRST_NAME":
                        return DemographicFieldType.FormerFirstName;
                    case "FORMER_MIDDLE_NAME":
                        return DemographicFieldType.FormerMiddleName;
                    case "FORMER_LAST_NAME":
                        return DemographicFieldType.FormerLastName;
                    case "EMAIL_ADDRESS":
                        return DemographicFieldType.EmailAddress;
                    case "EMAIL_TYPE":
                        return DemographicFieldType.EmailType;
                    //case "ADDRESS_LINES":
                    //    return DemographicFieldType.AddressLines;
                    //case "ADDRESS_TYPE":
                    //    return DemographicFieldType.AddressType;
                    //case "CITY":
                    //    return DemographicFieldType.City;
                    //case "STATE":
                    //    return DemographicFieldType.StateProvince;
                    //case "POSTAL_CODE":
                    //    return DemographicFieldType.PostalCode;
                    //case "COUNTRY":
                    //    return DemographicFieldType.Country;
                    case "PHONE":
                        return DemographicFieldType.Phone;
                    case "PHONE_EXTENSION":
                        return DemographicFieldType.PhoneExtension;
                    case "PHONE_TYPE":
                        return DemographicFieldType.PhoneType;
                    case "BIRTH_DATE":
                        return DemographicFieldType.BirthDate;
                    case "GENDER":
                        return DemographicFieldType.Gender;
                    case "SSN":
                        return DemographicFieldType.GovernmentId;
                    default:
                        return DemographicFieldType.Unknown;
                }
            }
        }

        /// <summary>
        /// Constructor for a demographic field
        /// </summary>
        /// <param name="code">Code for the field</param>
        /// <param name="desc">Description of the field</param>
        /// <param name="requirement">Field requirement type</param>
        public DemographicField(string code, string desc, DemographicFieldRequirement requirement)
            : base(code, desc)
        {
            _requirement = requirement;
        }

        /// <summary>
        /// Determines whether the specified <see cref="DemographicField">DemographicField</see> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="DemographicField">DemographicField</see> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="DemographicField">DemographicField</see> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var dfr = obj as DemographicField;

            return dfr.Code == this.Code;
        }

        /// <summary>
        /// Computes the HashCode of this object based on the Code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }
}
