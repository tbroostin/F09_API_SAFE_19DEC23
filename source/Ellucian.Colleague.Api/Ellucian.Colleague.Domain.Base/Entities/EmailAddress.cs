// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of email addresses
    /// </summary>
    [Serializable]
    public class EmailAddress
    {
        
        private string _Value;
        /// <summary>
        /// Email address string (e.g. smith@yahoo.com)
        /// </summary>
        public string Value { get { return _Value; } }

        private string _TypeCode;
        /// <summary>
        /// Type of email address
        /// </summary>
        public string TypeCode { get { return _TypeCode; } }

        /// <summary>
        /// Is the email address identified as a preferred type of email.
        /// </summary>
        public bool IsPreferred { get; set; }

        /// <summary>
        /// Status of email (Active, Inactive) used in Data Model.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddress"/> class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="typeCode">The type code.</param>
        /// <exception cref="System.ArgumentNullException">
        /// emailAddress;Email address must be specified
        /// or
        /// typeCode;Email address type must be specified
        /// </exception>
        public EmailAddress(string emailAddress, string typeCode)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ArgumentNullException("emailAddress", "Email address must be specified");
            }
            if (string.IsNullOrEmpty(typeCode))
            {
                throw new ArgumentNullException("typeCode", "Email address type must be specified");
            }
            // TODO: SSS NEED EMAIL ADDRESS VALIDATION HERE
            _Value = emailAddress;
            _TypeCode = typeCode;
            IsPreferred = false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" }, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            EmailAddress other = obj as EmailAddress;
            if (other == null)
            {
                return false;
            }
            return Value.Equals(other.Value) && TypeCode.Equals(other.TypeCode) && IsPreferred.Equals(other.IsPreferred);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode() + TypeCode.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
