﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Portal
{
    /// <summary>
    /// Contains the course sections that are applicable for deletion from portal.
    /// </summary>
    public class PortalDeletedSectionsResult
    {
        /// <summary>
        /// Total number of course sections applicable for deletion from Portal.
        /// </summary>
        public Nullable<int> TotalSections { get; set; }

        /// <summary>
        /// List of Course Section Ids that are applicable for deletion from Portal.
        /// </summary>
        public List<string> SectionIds { get; set; }
    }
}
