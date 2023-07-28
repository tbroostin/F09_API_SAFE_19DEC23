/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Represents a node for an employee in the organizational chart
    /// </summary>
    [Serializable]
    public class OrgChartNode
    {
        /// <summary>
        /// The perpos Id of employee for the org chart node
        /// </summary>
        public string PersonPositionId { get; private set; }

        /// <summary>
        /// The employee Id for the org chart node
        /// </summary>
        public string EmployeeId { get; private set; }

        /// <summary>
        /// The position code of employee for the org chart node
        /// </summary>
        public string PositionCode { get; private set; }

        /// <summary>
        /// The position code of employee for the org chart node
        /// </summary>
        public int? DirectReportCount { get; set; }

        /// <summary>
        /// The perpos Id of parent employee for the org chart node
        /// </summary>
        public string ParentPerposId { get; private set; }

        /// <summary>
        /// The person Id of parent employee for the org chart node
        /// </summary>
        public string ParentPersonId { get; private set; }

        /// <summary>
        /// The position code of parent employee for the org chart node
        /// </summary>
        public string ParentPositionCode { get; private set; }

        public OrgChartNode(string personPositionId, 
            string employeeId,
            string employeePositionCode, 
            string parentPerposId, 
            string parentPersonId, 
            string parentPositionCode,
            int? directReportCount = null)
        {
            PersonPositionId = personPositionId ?? throw new ArgumentNullException(nameof(personPositionId));
            EmployeeId = employeeId ?? throw new ArgumentNullException(nameof(employeeId));
            PositionCode= employeePositionCode ?? throw new ArgumentNullException(nameof(employeePositionCode));
            ParentPerposId = parentPerposId;
            ParentPersonId = parentPersonId;
            ParentPositionCode = parentPositionCode;
            DirectReportCount= directReportCount;
        }

        public OrgChartNode() { }
    }
}
