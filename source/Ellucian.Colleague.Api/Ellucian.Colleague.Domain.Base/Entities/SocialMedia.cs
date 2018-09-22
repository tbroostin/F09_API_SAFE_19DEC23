// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of social media information
    /// </summary>
    [Serializable]
    public class SocialMedia
    {
        /// <summary>
        /// Social Media Handle
        /// </summary>
        public string Handle { get; private set; }

        /// <summary>
        /// Social Media Type code
        /// </summary>
        public string TypeCode { get; private set; }

        /// <summary>
        /// Status of Social Media of this type and handle
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Set to true when this is the preferred social media
        /// </summary>
        public bool IsPreferred { get; set; }

        // using an optional extension parameter in the constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMedia"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="typeCode">The type code.</param>
        /// <param name="extension">The extension.</param>
        /// <exception cref="System.ArgumentNullException">number;Phones number must be specified</exception>
        public SocialMedia(string typeCode, string handle, bool isPreferred = false)
        {
            if (string.IsNullOrEmpty(typeCode))
            {
                throw new ArgumentNullException("type", "Social Media type must be specified");
            }
            if (string.IsNullOrEmpty(handle))
            {
                throw new ArgumentNullException("handle", "Social Media handle must be specified");
            }
            TypeCode = typeCode;
            Handle = handle;
            IsPreferred = isPreferred;
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
            SocialMedia other = obj as SocialMedia;
            if (other == null)
            {
                return false;
            }
            return TypeCode.Equals(other.TypeCode) &&
                (Handle != null ? Handle.Equals(other.Handle) : other.Handle == null) &&
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
            return TypeCode.GetHashCode();
        }
    }
}
