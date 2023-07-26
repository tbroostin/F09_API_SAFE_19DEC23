// Copyright 2023 Ellucian Company L.P. an?d its affiliates.
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Employee name for org chart
    /// </summary>
    [DataContract]
    public class OrgChartEmployeeName
    {
        /// <summary>
        /// The person position id of employee for the org chart node
        /// </summary>
        [JsonProperty("firstName")]
        [Metadata("FIRST.NAME", DataDescription = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// The person position id of parent for the org chart node
        /// </summary>
        [JsonProperty("MiddleName")]
        [Metadata("MIDDLE.NAME", DataDescription = "Middle Name")]
        public string MiddleName { get; set; }

        /// <summary>
        /// The id of the employee in org chart
        /// </summary>
        [JsonProperty("lastName")]
        [Metadata("LAST.NAME", DataDescription = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// The employee id of parent for the org chart node
        /// </summary>
        [JsonProperty("fullName")]
        [Metadata(DataDescription = "Full Name")]
        public string FullName { get; set; }
    }
}