// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an Person
    /// </summary>
    [Serializable]
    public enum PersonRoleType
    {
        /// <summary>
        /// Student
        /// </summary>
        Student,
        /// <summary>
        /// Instructor
        /// </summary>
        Instructor,
        /// <summary>
        /// Employee
        /// </summary>
        Employee,
        /// <summary>
        /// Vendor
        /// </summary>
        Vendor,
        /// <summary>
        /// Alumni
        /// </summary>
        Alumni,
        /// <summary>
        /// Prospective Student
        /// </summary>
        ProspectiveStudent,
        /// <summary>
        /// Advisor
        /// </summary>
        Advisor,
        /// <summary>
        /// Partner
        /// </summary>
        Partner,
        /// <summary>
        /// Affiliate
        /// </summary>
        Affiliate,
        /// <summary>
        /// Constituent
        /// </summary>
        Constituent

    }
}
