// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This enumeration contains all of the status values that are available for projects.
    /// (this enum is moved to Colleague Finance module from Projects Accounting module)
    /// </summary>
    public enum ProjectStatus
    {
        /// <summary>
        /// Active status means the project can be changed and transactions can be posted.
        /// </summary>
        Active,

        /// <summary>
        /// Inactive status means the project can be changed, but some restrictions apply
        /// </summary>
        Inactive,

        /// <summary>
        /// Closed status means the project cannot be changed and transactions cannot be posted.
        /// However, the project can be re-activated in case changes need to be made.
        /// </summary>
        Closed
    }
}
