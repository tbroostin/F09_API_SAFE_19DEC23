// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Representation of staff
    /// </summary>
    public class Staff
    {
        /// <summary>
        /// The set of privacy codes this staff member is allowed to access
        /// </summary>
        public List<string> PrivacyCodes { get; set; }

        /// <summary>
        /// The staff member's ID
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The staff member's last name
        /// </summary>
        public string LastName { get; private set; }
        /// <summary>
        /// The staff member's initials
        /// </summary>
        public string StaffInitials { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Staff"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="lastName">The last name.</param>
        public Staff(string id, string lastName)
        {
            Id = id;
            LastName = lastName;
        }
    }
}
