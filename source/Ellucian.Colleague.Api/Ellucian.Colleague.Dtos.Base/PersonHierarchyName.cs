// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// The name components of a person
    /// </summary>
    public class PersonHierarchyName
    {
        /// <summary>
        /// Code of the hierarchy that was used to calculate this name object
        /// </summary>
        public string HierarchyCode { get; set; }

        /// <summary>
        /// Full name calculated for this person based on the associated hierarchy code
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// The person's first name associated most closely with the hierarchy name.  
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The person's middle name associated most closely with the hierarchy name.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// The person's last name associated most closely with the hierarchy name. 
        /// </summary>
        public string LastName { get; set; }
    }
}
