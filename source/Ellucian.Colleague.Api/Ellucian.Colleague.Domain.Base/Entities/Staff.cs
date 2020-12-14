// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of staff
    /// </summary>
    [Serializable]
    public class Staff : Person
    {
        /// <summary>
        /// Indicates whether this is a currently active staff member.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is active]; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// The set of privacy codes this staff member is allowed to access
        /// </summary>
        public List<string> PrivacyCodes { get; set; }

        /// <summary>
        /// The staff member's initials
        /// </summary>
        public string StaffInitials { get; set; }

        /// <summary>
        /// The staff member's loginid/operatorid
        /// </summary>
        public string StaffLoginId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Staff"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="privacyCodes">The set of privacy codes for this staff member</param>
        public Staff(string id, string lastName)
            :base(id, lastName)
        {
            PrivacyCodes = new List<string>();
        }
    }
}
