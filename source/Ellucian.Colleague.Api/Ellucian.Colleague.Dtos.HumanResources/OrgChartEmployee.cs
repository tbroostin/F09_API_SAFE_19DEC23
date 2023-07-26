// Copyright 2023 Ellucian Company L.P. an?d its affiliates.
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Information about Organizational Chart.
    /// </summary>
    [DataContract]
    public class OrgChartEmployee
    {
        /// <summary>
        /// The person position id of employee for the org chart node
        /// </summary>
        [JsonProperty("personPositionId")]
        [Metadata("PERPOS.ID", DataDescription = "Person Position ID")]
        public string PersonPositionId { get; set; }

        /// <summary>
        /// The person position id of parent for the org chart node
        /// </summary>
        [JsonProperty("parentPersonPositionId")]
        [Metadata("PERPOS.ID", DataDescription = "Parent Person Position ID")]
        public string ParentPersonPositionId { get; set; }

        /// <summary>
        /// The id of the employee in org chart
        /// </summary>
        [JsonProperty("employeeId")]
        [Metadata("PERPOS.HRP.ID", DataDescription = "Employee Id")]
        public string EmployeeId { get; set; }

        /// <summary>
        /// The employee id of parent for the org chart node
        /// </summary>
        [JsonProperty("parentEmployeeId")]
        [Metadata("PERPOS.SUPERVISOR.HRP.ID", DataDescription = "Parent Employee ID")]
        public string ParentEmployeeId { get; set; }

        /// <summary>
        /// The name of the employee in org chart
        /// </summary>
        [JsonProperty("employeeName")]
        [Metadata(DataDescription = "Employee Name")]
        public OrgChartEmployeeName EmployeeName { get; set; }

        /// <summary>
        /// Number of direct reports for this employee
        /// </summary>
        [JsonProperty("directReportCount")]
        [Metadata(DataDescription = "Direct Report Count")]
        public string DirectReportCount { get; set; }

        /// <summary>
        /// The code for the position related to the employee in org chart
        /// </summary>
        [JsonProperty("positionCode")]
        [Metadata("PERPOS.POSITION.ID", DataDescription = "Person Position ID")]
        public string PositionCode { get; set; }

        /// <summary>
        /// The description for the position related to the employee in org chart
        /// </summary>
        [JsonProperty("positionDescription")]
        [Metadata("POS.SHORT.TITLE", DataDescription = "Person Position Description")]
        public string PositionDescription { get; set; }

        /// <summary>
        /// The location of the employee in org chart
        /// </summary>
        [JsonProperty("locationCode")]
        [Metadata("POS.LOCATION", DataDescription = "Location Code")]
        public string LocationCode { get; set; }

        /// <summary>
        /// The location description of the employee in org chart
        /// </summary>
        [JsonProperty("locationDescription")]
        [Metadata("LOC.DESC", DataDescription = "Location Description")]
        public string LocationDescription { get; set; }

        /// <summary>
        /// The URL to the image of the employee in org chart 
        /// </summary>
        [JsonProperty("imageUrl")]
        [Metadata(DataDescription = "Image Url")]
        public string ImageUrl { get; set; }
    }
}