// Copyright 2014-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Class for person alternate IDs
    /// </summary>
    [Serializable]
    public class PersonAlt
    {
        private static string elevatePersonAltType = "ELEV";
        public static string ElevatePersonAltType
        {
            get { return elevatePersonAltType; }
            set { elevatePersonAltType = value; }
        }


        /// <summary>
        /// Gets or sets the person's alternate ID
        /// </summary>
        /// <value>
        /// The person's alternate ID.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the person's alternate ID type
        /// </summary>
        /// <value>
        /// The person's alternate ID type.
        /// </value>
        public string Type { get; set; }
      
        /// <summary>
        /// Create a person alt domain object
        /// </summary>
        /// <param name="personAltId">Person Alternate ID</param>
        /// <param name="personAltIdType">Person Alternate ID Type</param>
        public PersonAlt(string personAltId, string personAltIdType)
        {
            if (string.IsNullOrEmpty(personAltId))
            {
                throw new ArgumentNullException("personAltId");
            }
            Id = personAltId;
            Type = personAltIdType;
        }

        #region Override methods

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
            PersonAlt other = obj as PersonAlt;
            if (other == null)
            {
                return false;
            }
            return (Id.Equals(other.Id) && Type.Equals(other.Type));
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Id;
        }

        #endregion
    }
}