// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Enumeration of possible statuses of Enrollment Status
    /// </summary>
    [Serializable]
    public enum EnrollmentStatusType
    {
        /// <summary>
        /// Active
        /// </summary>
        active,
        /// <summary>
        /// Inactive
        /// </summary>
        inactive,
        /// <summary>
        /// Complete
        /// </summary>
        complete,
    }
}