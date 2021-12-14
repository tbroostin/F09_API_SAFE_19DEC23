// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// Contains the course sections that are applicable for update from portal.
    /// </summary>
    [Serializable]
    public class PortalUpdatedSectionsResult
    {
        /// <summary>
        /// Format for dates
        /// </summary>
        public string ShortDateFormat { get; private set; }

        /// <summary>
        /// Total number of course sections applicable for update from Portal.
        /// </summary>
        public Nullable<int> TotalSections { get; private set; }

        /// <summary>
        /// List of course sections applicable for update from Portal.
        /// </summary>
        public List<PortalSection> Sections { get; private set; }

        public PortalUpdatedSectionsResult(string shortDateFormat, Nullable<int> totalSections, List<PortalSection> sections)
        {
            ShortDateFormat = shortDateFormat;
            TotalSections = totalSections;
            Sections = sections;
        }
    }
}
