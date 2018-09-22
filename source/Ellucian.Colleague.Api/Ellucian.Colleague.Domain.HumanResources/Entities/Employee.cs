/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// PersonPosition object describes how a person holds a position.
    /// </summary>
    [Serializable]
    public class Employee
    {
        /// <summary>
        /// The global identifier for the employee
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// The PERSON Id
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// The physical location assigned to an employee.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The primary position of the employee.
        /// </summary>
        public string PrimaryPosition { get; set; }

        /// <summary>
        /// The type of employment (Eg. Full-time or Part-time).
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// The classification assigned to the employee, or their primary job, 
        /// that may impact payroll processing such as time entry methods, earning 
        /// types and pay periods.
        /// </summary>
        public string PayClass { get; set; }

        /// <summary>
        /// The pay status of the employee (with pay, without pay or partial pay).
        /// </summary>
        public PayStatus? PayStatus { get; set; }

        /// <summary>
        /// The benefits status of the employee (with or without benefits).
        /// </summary>
        public BenefitsStatus? BenefitsStatus { get; set; }

        /// <summary>
        /// This property represents the number of hours worked per period.
        /// </summary>
        public IEnumerable<decimal?> PayPeriodHours { get; set; }

        /// <summary>
        /// This property represents the number of hours worked per period.
        /// </summary>
        public decimal? PpwgCycleWorkTimeAmt { get; set; }

        /// <summary>
        /// This property represents the number of hours worked per year.
        /// </summary>
        public decimal? PpwgYearWorkTimeAmt { get; set; }

        /// <summary>
        /// The employment status (active, terminated, on leave).
        /// </summary>
        public EmployeeStatus? EmploymentStatus { get; set; }

        /// <summary>
        /// The first date of employment.
        /// </summary>
        public DateTime? StartDate 
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
        private DateTime? startDate;

        /// <summary>
        /// The last date of employment.
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
        /// The reason for employment termination.
        /// </summary>
        public string StatusEndReasonCode { get; set; }

        /// <summary>
        /// The eligibility status for being rehired. 
        /// </summary>
        public string RehireEligibilityCode { get; set; }

        /// <summary>
        /// Whether the employee has consented to view their earnings statement online
        /// </summary>
        public bool HasOnlineEarningsStatementConsent { get; set; }
      
        /// <summary>
        /// Create an Employee object
        /// </summary>
        /// <param name="guid">The global identifier for the employee record</param>
        /// <param name="personId">The Colleague PERSON id of the person</param>
        public Employee(string guid, string personId)
        { 
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            Guid = guid;
            PersonId = personId;
        }

        /// <summary>
        /// Two employees are equal when their Ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var employee = obj as Employee;

            return employee.Guid == Guid;
        }

        /// <summary>
        /// Hashcode representation of object (Id)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        /// <summary>
        /// String representation of object (Id)
        /// </summary>
        /// <returns>Global Identifier</returns>
        public override string ToString()
        {
            return Guid;
        }
    }
}
