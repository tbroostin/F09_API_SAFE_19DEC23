/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// describes external-employments as information about an applicant's or employee's previous employments.
    /// </summary>
    [Serializable]
    public class ExternalEmployments
    {
        /// <summary>
        /// The global identifier for the employmt
        /// </summary>
        public string Guid { get; private set; }


        /// <summary>
        /// The database ID of the employmt

        /// </summary>
        public string Id
        {
            get { return id; }
        }

        private readonly string id;

        /// <summary>
        /// The PERSON Id
        /// </summary>
        public string PersonId
        {
            get { return personId; }
        }

        private readonly string personId;

        /// <summary>
        /// The organization Id
        /// </summary>
        public string OrganizationId;
       

        /// <summary>
        /// The PositionId. <see cref="Position"/>
        /// </summary>
        public string PositionId;
       

        /// <summary>
        ///The start of employment at the organization.
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
        /// 	The end of employment at the organization.
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
        /// The job title for the external employment.
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// The name of the organization
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// TAn indication whether the position was considered as primary for the employee.
        /// </summary>
        public string PrincipalEmployment { get; set; }


        /// <summary>
        ///The status of the external employment.
        /// </summary>
        public string Status { get; set; }


        /// <summary>
        /// The number of hours worked
        /// </summary>
        public Decimal? HoursWorked { get; set; }

        /// <summary>
        /// The vocation associated with the external employment.
        /// </summary>
        public List<string> Vocations { get; set; }

        /// <summary>
        /// The list of supervisors of the person 
        /// </summary>
        public List<ExternalEmploymentSupervisors> Supervisors { get; set; }
        
        /// <summary>
        /// Any comment associated with the external employment.
        /// </summary>
        public string comments { get; set; }

        /// <summary>
        /// set if self employed.
        /// </summary>
        public string selfEmployed { get; set; }

        /// <summary>
        /// set employer is unknown.
        /// </summary>
        public string unknownEmployer { get; set; }

        /// <summary>
        /// Create a ExternalEmployments object
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="id">The Id of the employmt</param>
        /// <param name="personId">The person associated with the external employment.</param>
        /// <param name="title">The job title for the external employment.</param>
        /// <param name="status">The status of the external employment</param>
        public ExternalEmployments(string guid, string id, string personId, string title, string status)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentException("guid cannot be null, Entity:'EMPLOYMT', Record ID: '" + id + "'");
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id cannot be null, Entity:'EMPLOYMT', Record ID: '" + id + "'");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentException("personId cannot be null, Entity:'EMPLOYMT', Record ID: '" + id + "'");
            }
            this.id = id;
            this.personId = personId;
            this.JobTitle = title;
            this.Status = status;
            this.Guid = guid;
        }
    }

       
    
}
