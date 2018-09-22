// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of phone numbers
    /// </summary>
    [Serializable]
    public class Phone
    {
        private string _Number;
        /// <summary>
        /// Gets the number.
        /// </summary>
        /// <value>
        /// The number.
        /// </value>
        public string Number { get { return _Number; } }

        private string _Extension;
        /// <summary>
        /// Gets the extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public string Extension { get { return _Extension; } }

        private string _TypeCode;
        /// <summary>
        /// Gets the type code.
        /// </summary>
        /// <value>
        /// The type code.
        /// </value>
        public string TypeCode { get { return _TypeCode; } }

        /// <summary>
        /// Set to true if this is preferred.  Used in Data Model.
        /// </summary>
        public bool IsPreferred { get; set; }

        /// <summary>
        /// Country calling code used for Data Model.
        /// </summary>
        public string CountryCallingCode { get; set; }

        // using an optional extension parameter in the constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Phone"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="typeCode">The type code.</param>
        /// <param name="extension">The extension.</param>
        /// <exception cref="System.ArgumentNullException">number;Phones number must be specified</exception>
        public Phone(string number, string typeCode = null, string extension = null)
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "Phones number must be specified");
            }
            _Number = number;
            _TypeCode = typeCode;
            _Extension = extension;
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
            Phone other = obj as Phone;
            if (other == null)
            {
                return false;
            }
            return Number.Equals(other.Number) &&
                (Extension != null ? Extension.Equals(other.Extension) : other.Extension == null) &&
                (TypeCode != null ? TypeCode.Equals(other.TypeCode) : other.TypeCode == null);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }
    }
}
