// Copyright 2013-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// A person who advises students
    /// </summary>
    public class Advisor 
    {
        /// <summary>
        /// Advisor's unique system ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Advisor's last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Advisor's first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Advisor's middle name
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Only those emails for this person that are designated as "faculty/advisor" type of emails.
        /// </summary>
        public IEnumerable<string> EmailAddresses { get; set; }

        /// <summary>
        /// Information that should be used when displaying a Advisor's name.  
        /// The hierarchy that is used in calculating this name is defined in the Advisor Display Name Hierarchy on the SPWP form in Colleague.  
        /// If no hierarchy is provide on SPWP, PersonDisplayName will be null.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }
    }
}
