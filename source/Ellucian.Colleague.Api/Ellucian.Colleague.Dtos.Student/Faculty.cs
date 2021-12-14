// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Student {
    /// <summary>
    /// Information specific to a person as a faculty member.
    /// This does not inherit from PERSON to prevent exposing the Faculty address
    /// </summary>
    public class Faculty  
    {
        /// <summary>
        /// Unique person Id of this faculty
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Faculty member last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Faculty member first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Faculty member middle name or initial
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Gender of the Faculty Member
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Name of faculty as "formatted" to print on external sources such as section details.
        /// </summary>
        public string ProfessionalName { get; set; }

        /// <summary>
        /// Only those <see cref="Phone">phone</see> numbers for this person that are designated "faculty" phones.
        /// </summary>
        public IEnumerable<Phone> Phones { get; set; }

        /// <summary>
        /// Only those emails for this person that are designated "faculty" emails.
        /// </summary>
        public IEnumerable<string> EmailAddresses { get; set; }

        /// <summary>
        /// Only those addresses for this person that are designated "faculty" addresses.
        /// </summary>
        public IEnumerable<Address> Addresses { get; set; }

        /// <summary>
        /// Information that should be used when displaying a Faculty's name.  
        /// The hierarchy that is used in calculating this name is defined in the Faculty Display Name Hierarchy on the SPWP form in Colleague.  
        /// If no hierarchy is provide on SPWP, PersonDisplayName will be null.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }
    }
}
