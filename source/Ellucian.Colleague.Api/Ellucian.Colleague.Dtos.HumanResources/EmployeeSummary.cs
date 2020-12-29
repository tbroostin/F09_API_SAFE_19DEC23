/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// A summary of information related to a specific employee
    /// </summary>
    public class EmployeeSummary
    {
        /// <summary>
        /// ID of employee related to this summary
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// PersonPosition objects related to this employee
        /// </summary>
        public List<PersonPosition> PersonPositions { get; set; }

        /// <summary>
        /// PersonPositionWage objects related to this employee
        /// </summary>
        public List<PersonPositionWage> PersonPositionWages { get; set; }

        /// <summary>
        /// PersonEmploymentStatus objects related to this employee
        /// </summary>
        public List<PersonEmploymentStatus> PersonEmploymentStatuses { get; set; }

        /// <summary>
        /// HumanResourceDemographics object related to this employee
        /// </summary>
        public HumanResourceDemographics PersonDemographics { get; set; }

        /// <summary>
        /// HumanResourceDemographics objects containing supervisors demographics data
        /// </summary>
        public List<HumanResourceDemographics> PersonSupervisorsDemographics { get; set; }
    }
}
