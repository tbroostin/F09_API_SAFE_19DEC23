/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Represents a node for an employee in the organizational chart
    /// </summary>
    [Serializable]
    public class OrgChartEmployee
    {
        /// <summary>
        /// The person position id of employee for the org chart node
        /// </summary>
        public string PersonPositionId { get; set; }

        /// <summary>
        /// The person position id of parent for the org chart node
        /// </summary>
        public string ParentPersonPositionId { get; set; }

        /// <summary>
        /// The employee id of parent for the org chart node
        /// </summary>
        public string ParentEmployeeId { get; set; }

        /// <summary>
        /// The code for the position related to the employee in org chart
        /// </summary>
        public string PositionCode { get; set; }

        /// <summary>
        /// The description for the position related to the employee in org chart
        /// </summary>
        public string PositionDescription { get; set; }

        /// <summary>
        /// The id of employee for the org chart node
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// The location of the employee in org chart
        /// </summary>
        public string LocationCode { get; set; }

        /// <summary>
        /// The location description of the employee in org chart
        /// </summary>
        public string LocationDescription { get; set; }

        /// <summary>
        /// The name of the employee in org chart
        /// </summary>
        public OrgChartEmployeeName EmployeeName { get; set; }

        /// <summary>
        /// The position code of employee for the org chart node
        /// </summary>
        public int? DirectReportCount { get; set; }

        /// <summary>
        /// The URL to the image of the employee in org chart 
        /// </summary>
        public string ImageUrl { get; set; }

        public OrgChartEmployee(string personPositionId,
            string parentPersonPositionId,
            string parentEmployeeId,
            string positionCode,
            string positionDescription,
            string employeeId,
            string locationCode,
            string locationDescription,
            OrgChartEmployeeName employeeName,
            int? directReportCount = null)
        {
            
            if (string.IsNullOrWhiteSpace(personPositionId))
            {
                throw new ArgumentNullException("personPositionId");
            }
            if (string.IsNullOrWhiteSpace(positionCode))
            {
                throw new ArgumentNullException("positionCode");
            }

            PersonPositionId = personPositionId;
            ParentPersonPositionId = parentPersonPositionId;
            ParentEmployeeId = parentEmployeeId;
            PositionCode = positionCode;
            PositionDescription = positionDescription;
            EmployeeId = employeeId;
            EmployeeName = employeeName;
            DirectReportCount = directReportCount;
            LocationCode = locationCode;
            LocationDescription = locationDescription;
            ImageUrl = "";
        }

        public OrgChartEmployee() { }

    }
}
