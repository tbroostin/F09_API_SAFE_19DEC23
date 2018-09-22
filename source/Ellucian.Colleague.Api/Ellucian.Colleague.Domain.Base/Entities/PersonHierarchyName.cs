// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The name components for a person based on a specfic Name Address Hierarchy (NAHM)
    /// </summary>
    [Serializable]
    public class PersonHierarchyName
    {
        /// <summary>
        /// The person's given name (First Name).
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The person's middle name
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// The person's family name.  Required.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Calculated hierarchy Name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Hierarchy used in the calculation
        /// </summary>
        private readonly string _hierarchyCode;
        public string HierarchyCode { get { return _hierarchyCode; } }


        /// <summary>
        /// Person name information calculated based on an Name Hierarchy
        /// </summary>
        /// <param name="hierarchyCode">Hierarchy codes used to calculate this Name object</param>
        /// <param name="hierarchyName">Hierarchy name calculated based on the hierarchy code</param>
        public PersonHierarchyName(string hierarchyCode)
        {
            if (string.IsNullOrEmpty(hierarchyCode))
            {
                throw new ArgumentNullException("hierarchyCode");
            }
            _hierarchyCode = hierarchyCode;
        }

        /// <summary>
        /// Compares two PersonHierarchyName objects for equality
        /// </summary>
        /// <param name="obj">The other object to test</param>
        /// <returns>A boolean indicating the equality</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            PersonHierarchyName other = obj as PersonHierarchyName;
            if (other == null)
            {
                return false;
            }
            return FullName.Equals(other.FullName) &&
                 HierarchyCode.Equals(other.HierarchyCode);
        }

        /// <summary>
        /// Return a hashcode for the PersonHierarchyName
        /// </summary>
        /// <returns>The generated hashcode</returns>
        public override int GetHashCode()
        {
            return FullName.GetHashCode() ^ HierarchyCode.GetHashCode();
        }
    }
}
