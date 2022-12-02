// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    /// <summary>
    /// Student Planning configuration data
    /// </summary>
    [Serializable]
    public class PlanningConfiguration
    {
        /// <summary>
        /// Default curriculum track
        /// </summary>
        public string DefaultCurriculumTrack { get; set; }

        /// <summary>
        /// Default <see cref="CatalogPolicy">catalog policy</see>
        /// </summary>
        public CatalogPolicy DefaultCatalogPolicy { get; set; }

        /// <summary>
        /// Flag indicating whether or not to show the Advisement Complete workflow
        /// </summary>
        public bool ShowAdvisementCompleteWorkflow { get; set; }

        /// <summary>
        /// Flag to identify the Advising by office configuration.
        /// </summary>
        public bool AdviseByOfficeFlag { get; set; }

        /// <summary>
        /// Flag to identify the Advising by Assignment configuration.
        /// </summary>
        public bool AdviseByAssignmentFlag { get; set; }

        /// <summary>
		/// List of open office advisors
		/// </summary>
        public List<string> OpenOfficeAdvisors { get; set; }
    }
}
