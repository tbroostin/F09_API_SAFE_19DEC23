/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// A summary of information related to a specific employee
    /// </summary>
    [Serializable]
    public class EmployeeSummary
    {
        /// <summary>
        /// ID of employee related to this summary
        /// </summary>
        public string EmployeeId { get; private set; }

        /// <summary>
        /// PersonPosition objects related to this employee
        /// </summary>
        public IEnumerable<PersonPosition> PersonPositions { get; private set; }

        /// <summary>
        /// PersonPositionWage objects related to this employee
        /// </summary>
        public IEnumerable<PersonPositionWage> PersonPositionWages { get; private set; }

        /// <summary>
        /// PersonEmploymentStatus objects related to this employee
        /// </summary>
        public IEnumerable<PersonEmploymentStatus> PersonEmploymentStatuses { get; private set; }

        /// <summary>
        /// PersonBase object related to this employee
        /// </summary>
        public PersonBase PersonDemographics { get; private set; }

        /// <summary>
        /// PersonBase objects containing supervisors demographics data
        /// </summary>
        public IEnumerable<PersonBase> PersonSupervisorsDemographics { get; private set; }

        /// <summary>
        /// EmployeeSummary constructor
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="personPositions"></param>
        /// <param name="personPositionWages"></param>
        /// <param name="personEmploymentStatuses"></param>
        /// <param name="personDemographics"></param>
        public EmployeeSummary(string employeeId, IEnumerable<PersonPosition> personPositions, IEnumerable<PersonPositionWage> personPositionWages,
            IEnumerable<PersonEmploymentStatus> personEmploymentStatuses, PersonBase personDemographics, IEnumerable<PersonBase> supervisorsDemographics)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }

            EmployeeId = employeeId;
            PersonPositions = personPositions;
            PersonPositionWages = personPositionWages;
            PersonEmploymentStatuses = personEmploymentStatuses;
            PersonDemographics = personDemographics;
            PersonSupervisorsDemographics = supervisorsDemographics;
        }
    }
}
