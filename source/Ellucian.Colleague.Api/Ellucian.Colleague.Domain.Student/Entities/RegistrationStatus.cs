// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Valid statuses for section registration (required).
    /// </summary>
    [Serializable]
    public enum RegistrationStatus
    {
        /// <summary>
        /// A person is registered in the section.  The Registration status code is automatically assigned 
        /// in Colleague.  The Status Reason must be set to "Registered". 
        /// </summary>
        Registered,
        /// <summary>
        /// A person is not registered in the section.  A valid Status Reason must be assigned.
        /// </summary>
        NotRegistered 
    }
}
