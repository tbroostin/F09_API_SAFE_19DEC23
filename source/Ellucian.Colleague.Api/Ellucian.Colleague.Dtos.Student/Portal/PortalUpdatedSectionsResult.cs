// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Contains a course section that is applicable for update from Portal.
    /// </summary>
    public class PortalUpdatedSectionsResult
    {
        /// <summary>
        /// Format for dates
        /// </summary>
        public string ShortDateFormat { get; set; }

        /// <summary>
        /// Total number of course sections applicable for update from Portal.
        /// </summary>
        public Nullable<int> TotalSections { get; set; }

        /// <summary>
        /// List of course sections applicable for update from Portal.
        /// </summary>
        public List<PortalSection> Sections { get; set; }

    }
}
