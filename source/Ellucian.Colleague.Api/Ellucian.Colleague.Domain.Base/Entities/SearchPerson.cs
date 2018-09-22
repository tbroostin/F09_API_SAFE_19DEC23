// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// SearchPerson domain representation
    /// </summary>
    [Serializable]
    public class SearchPerson
    {
        // Required fields

        private readonly string _Id;
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get { return _Id; } }

        private readonly string _LastName;
        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get { return _LastName; } }

        // Non-required fields

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName { get; set; }
        /// <summary>
        /// Gets or sets the middle name.
        /// </summary>
        /// <value>
        /// The middle name.
        /// </value>
        public string MiddleName { get; set; }

        #region Constructor

        /// <summary>
        /// Create a SearchPerson domain object
        /// </summary>
        /// <param name="personId">ID</param>
        /// <param name="lastName">Last Name</param>
        /// <param name="firstName">First Name</param>
        public SearchPerson(string personId, string lastName)
        {
            if (String.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (String.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }
            _Id = personId;
            _LastName = lastName;
        }

        #endregion

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
            SearchPerson other = obj as SearchPerson;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(Id);
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