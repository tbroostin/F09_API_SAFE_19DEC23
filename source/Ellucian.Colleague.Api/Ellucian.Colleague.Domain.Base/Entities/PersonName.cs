// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The name components of a person
    /// </summary>
    [Serializable]
    public class PersonName
    {
        /// <summary>
        /// The person's given name.  Required.
        /// </summary>
        public string GivenName { get; private set; }

        /// <summary>
        /// The person's middle name
        /// </summary>
        public string MiddleName { get; private set; }

        /// <summary>
        /// The person's family name.  Required.
        /// </summary>
        public string FamilyName { get; private set; }

        /// <summary>
        /// Creates a new PersonName
        /// </summary>
        /// <param name="given">The person's given name.  Required.</param>
        /// <param name="middle">The person's middle name</param>
        /// <param name="family">The person's family name.  Required.</param>
        public PersonName(string given, string middle, string family)
        {
            if (string.IsNullOrEmpty(family))
            {
                throw new ArgumentNullException("family");
            }
            GivenName = given;
            MiddleName = string.IsNullOrEmpty(middle) ? string.Empty : middle;
            FamilyName = family;
        }

        /// <summary>
        /// Compares two PersonName objects for equality
        /// </summary>
        /// <param name="obj">The other object to test</param>
        /// <returns>A boolean indicating the equality</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            PersonName other = obj as PersonName;
            if (other == null)
            {
                return false;
            }
            return GivenName.Equals(other.GivenName) &&
                 MiddleName.Equals(other.MiddleName) &&
                 FamilyName.Equals(other.FamilyName);
        }

        /// <summary>
        /// Return a hashcode for the PersonName
        /// </summary>
        /// <returns>The generated hashcode</returns>
        public override int GetHashCode()
        {
            return GivenName.GetHashCode() ^ MiddleName.GetHashCode() ^ FamilyName.GetHashCode();
        }
    }
}
