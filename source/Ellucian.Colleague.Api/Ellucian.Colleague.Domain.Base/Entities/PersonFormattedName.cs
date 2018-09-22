// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A formatted name for a person (and its associated user defined format type)
    /// </summary>
    [Serializable]
    public class PersonFormattedName
    {
        /// <summary>
        /// Formatted Name
        /// </summary>
        private readonly string _name;
        public string Name { get { return _name; }  }

        /// <summary>
        /// Format type - user defined
        /// </summary>
        private readonly string _type;
        public string Type { get { return _type; } }


        /// <summary>
        /// Person name information calculated based on an Name Hierarchy
        /// </summary>
        /// <param name="hierarchyCode">Hierarchy codes used to calculate this Name object</param>
        /// <param name="hierarchyName">Hierarchy name calculated based on the hierarchy code</param>
        public PersonFormattedName(string type, string name)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type", "Name type is required for a formatted name");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "Name is required for a formatted name");
            }
            _type = type;
            _name = name;
        }

        /// <summary>
        /// Compares two formatted name objects for equality
        /// </summary>
        /// <param name="obj">The other object to test</param>
        /// <returns>A boolean indicating the equality</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            PersonFormattedName other = obj as PersonFormattedName;
            if (other == null)
            {
                return false;
            }
            return Name.Equals(other.Name) &&
                 Type.Equals(other.Type);
        }

        /// <summary>
        /// Return a hashcode for the PersonHierarchyName
        /// </summary>
        /// <returns>The generated hashcode</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode();
        }
    }
}
