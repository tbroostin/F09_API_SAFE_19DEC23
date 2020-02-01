// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Used to determine degree plan query filter for open office
    /// </summary>
    public enum DegreePlanRetrievalType
    {
        /// <summary>
        /// Degree plans assigned to for the current advisor
        /// </summary>
        MyAssignedAdvisees,

        /// <summary>
        /// Degree plans which are not assigned to anyone
        /// </summary>
        UnAssignedAdvisees,

        /// <summary>
        /// Degree plans assigned to others
        /// </summary>
        AssignedToOthers,

        /// <summary>
        /// Retrieve all review request without filter
        /// </summary>
        All
    }
}
