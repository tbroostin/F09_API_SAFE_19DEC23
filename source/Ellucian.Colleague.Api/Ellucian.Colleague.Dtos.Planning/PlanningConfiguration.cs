// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// Student Planning configuration data
    /// </summary>
    public class PlanningConfiguration
    {
        /// <summary>
        /// Default curriculum track
        /// </summary>
        public string DefaultCurriculumTrack { get; set; }

        /// <summary>
        /// Default <see cref="CatalogPolicy">catalog policy</see>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
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
        /// List of Open office advisors
        /// </summary>
        public List<string> OpenOfficeAdvisors { get; set; }
    }
}
