/* Copyright 2016-2017 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// LeavePlan
    /// </summary>
    [Serializable]
    public class Perleave
    {
        /// <summary>
        /// The global identifier of the employee leave plan
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// id for the employee leaveplan
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        private readonly string id;

        /// <summary>
        /// The person associated with the leave plan.
        /// </summary>
        public string PersonId
        {
            get { return personId; }
        }

        private readonly string personId;

        /// <summary>
        /// The leave plan assigned to the employee
        /// </summary>
        public string LeavePlan
        {
            get { return plan; }
        }

        private readonly string plan;

        /// <summary>
        /// The start date of the leave plan.
        /// </summary>
        public DateTime? StartDate
        {
            get { return startDate; }
            set
            {
                if (EndDate.HasValue && EndDate.Value < value)
                {
                    throw new ArgumentOutOfRangeException("Start Date cannot be after the EndDate");
                }
                startDate = value;
            }
        }

        private DateTime? startDate;

        /// <summary>
        /// The end date of the leave plan
        /// </summary>
        public DateTime? EndDate
        {
            get { return endDate; }
            set
            {
                if (value.HasValue && value.Value < StartDate)
                {
                    throw new ArgumentOutOfRangeException("End Date cannot be before Start Date");
                }
                endDate = value;
            }
        }

        private DateTime? endDate;

        /// <summary>
        /// Create a employee leave plan.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="id">A code that may be used to identify the employee leave plan.</param>
        /// <param name="personId">The employee of employee leave plan.</param>
        /// <param name="startDate">The start date of employee leave plan</param>
        /// <param name="plan">The leave of the employee leave plan</param>
        public Perleave(string guid, string id, DateTime? startDate, string personId, string plan)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Concat("EmployeeLeavePlans guid can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(string.Concat("EmployeeLeavePlans id can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException(string.Concat("EmployeeLeavePlans PersonId can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }
            if (string.IsNullOrEmpty(plan))
            {
                throw new ArgumentNullException(string.Concat("EmployeeLeavePlans plan can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }
            if ((startDate == null) || (startDate == new DateTime(1968,1,1)))
            {
                throw new ArgumentNullException(string.Concat("EmployeeLeavePlans startDate can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }

            this.id = id;
            this.personId= personId;
            this.plan = plan;
            this.startDate = startDate;
            this.Guid = guid;
        }

      
    }
}