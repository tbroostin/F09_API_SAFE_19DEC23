// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Valid statuses for term registration
    /// </summary>
    [Serializable]
    public enum TermRegistrationStatus
    {
        /// <summary>
        /// Open for preregistration, registration, add, or drop
        /// </summary>
        Open,
        /// <summary>
        /// Closed for preregistration, registration, add, or drop
        /// </summary>
        Closed
    }
}
