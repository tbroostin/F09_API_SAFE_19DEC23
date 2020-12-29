/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// A PersonStipend object describes the parameters used to determine how stipend 
    /// is paid to a person for their particular position.
    /// PersonStipend is a subset of the PersonPOsitionWage record
    /// </summary>
    [Serializable]
    public class PersonStipend
    {
        /// <summary>
        /// The DB Id of the object
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The Colleague PersonId of the object
        /// </summary>
        public string PersonId { get { return personId; } }
        private readonly string personId;

        /// <summary>
        /// The PositionId of the object
        /// </summary>
        public string PositionId { get { return positionId; } }
        private readonly string positionId;

        /// <summary>
        /// The start date of when these wage parameters take effect
        /// </summary>
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                if (EndDate.HasValue && EndDate.Value < value)
                {
                    throw new ArgumentOutOfRangeException("value", "Start Date must be before End Date");
                }
                startDate = value;
            }
        }
        private DateTime startDate;

        /// <summary>
        /// The end date of when these wage parameters stop taking effect
        /// This is a nullable field
        /// </summary>
        public DateTime? EndDate
        {
            get { return endDate; }
            set
            {
                if (value.HasValue && value.Value < StartDate)
                {
                    throw new ArgumentOutOfRangeException("value", "End Date must be after Start Date");
                }
                endDate = value;
            }
        }
        private DateTime? endDate;

        /// <summary>
        /// The description of this PersonPositionWage object
        /// </summary>
        public string Description { get { return description; } }
        private readonly string description;

        /// <summary>
        /// The base amount of this PersonPositionWage object
        /// </summary>
        public string BaseAmount { get { return baseAmount; } }
        private readonly string baseAmount;

        /// <summary>
        /// The payroll designation of this PersonPositionWage object
        /// </summary>
        public string PayrollDesignation { get { return payrollDesignation; } }
        private readonly string payrollDesignation;

        /// <summary>
        /// The number of paychecks for this PersonPositionWage object
        /// </summary>
        public int? NumberOfPayments { get { return numberOfPayments; } }
        private readonly int? numberOfPayments;

        /// <summary>
        /// The number of paychecks taken for this PersonPositionWage object
        /// </summary>
        public int? NumberOfPaymentsTaken { get { return numberOfPaymentsTaken; } }
        private readonly int? numberOfPaymentsTaken;

        /// <summary>
        /// Course Section Assignments associated with this  PersonPositionWage object
        /// </summary>
        public List<string> CourseSectionAssignments { get; set; }

        /// <summary>
        /// Org Advisor Assignments associated with this  PersonPositionWage object
        /// </summary>
        public List<string> AdvisorAssignments { get; set; }

        /// <summary>
        /// Org Membership Assignments associated with this  PersonPositionWage object
        /// </summary>
        public List<string> MembershipAssignments { get; set; }

        /// <summary>
        /// Build a PersonStipend object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personId"></param>
        /// <param name="positionId"></param>        
        /// <param name="startDate"></param>     
        /// <param name="endDate"></param>  
        /// <param name="description"></param>
        /// <param name="baseAmount"></param>
        /// <param name="payrollDesignation"></param>
        /// <param name="numberOfPayments"></param>
        /// <param name="numberOfPaymentsTaken"></param>
        /// <param name="courseSectionAssignments"></param>
        /// <param name="advisorAssignments"></param>
        /// <param name="membershipAssignments"></param>       
        public PersonStipend(
            string id,
            string personId,
            string positionId,
            DateTime startDate,
            DateTime? endDate,
            string description,
            string baseAmount,
            string payrollDesignation,
            int? numberOfPayments,
            int? numberOfPaymentsTaken,
            List<string> courseSectionAssignments,
            List<string> advisorAssignments,
            List<string> membershipAssignments)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(baseAmount))
            {
                throw new ArgumentNullException("baseAmount");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            if (string.IsNullOrEmpty(payrollDesignation))
            {
                throw new ArgumentNullException("payrollDesignation");
            }

            this.id = id;
            this.personId = personId;
            this.positionId = positionId;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.description = description;
            this.baseAmount = baseAmount;
            this.payrollDesignation = payrollDesignation;
            this.numberOfPayments = numberOfPayments;
            this.numberOfPaymentsTaken = numberOfPaymentsTaken;
            CourseSectionAssignments = courseSectionAssignments;
            AdvisorAssignments = advisorAssignments;
            MembershipAssignments = membershipAssignments;
        }
    }
}
