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
    }
}
