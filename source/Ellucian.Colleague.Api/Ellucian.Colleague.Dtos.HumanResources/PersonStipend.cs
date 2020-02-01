/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// A PersonStipend object describes the parameters used to determine how stipend 
    /// is paid to a person for their particular position.
    /// PersonStipend is a subset of the PersonPositionWage record
    /// </summary>
    public class PersonStipend
    {
        /// <summary>
        /// The DB Id of the object
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Colleague PersonId of the object
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The PositionId of the object
        /// </summary>
        public string PositionId { get; set; }

        /// <summary>
        /// The start date of when these wage parameters take effect
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of when these wage parameters stop taking effect
        /// This is a nullable field
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The description of this PersonPositionWage object
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The base amount of this PersonPositionWage object
        /// </summary>
        public string BaseAmount { get; set; }

        /// <summary>
        /// The payroll designation of this PersonPositionWage object
        /// </summary>
        public string PayrollDesignation { get; set; }

        /// <summary>
        /// The number of paychecks for this PersonPositionWage object
        /// </summary>
        public int? NumberOfPayments { get; set; }

        /// <summary>
        /// The number of paychecks taken for this PersonPositionWage object
        /// </summary>
        public int? NumberOfPaymentsTaken { get; set; }

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
    }
}
