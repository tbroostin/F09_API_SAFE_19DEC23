//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Payroll deduction arrangement change reason.
    /// </summary>
    [Serializable]
    public class PayrollDeductionArrangements
    {
        /// <summary>
        /// The global identifier of a payroll deduction request.
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// The employee for whom the payroll deduction is requested.
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// Key to PERBEN record in Colleague
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The contribution (pledge/recurring donation) for which the payroll deduction is requested.
        /// </summary>
        public string CommitmentContributionId { get; set; }

        /// <summary>
        /// The reason for a payroll deduction request.
        /// </summary>
        public string CommitmentType { get; set; }

        /// <summary>
        /// Payroll deduction for HSA, dining, parking, wage garnishments, etc
        /// </summary>
        public string DeductionTypeCode { get; set; }

        /// <summary>
        /// The status of a payroll deduction request.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The amount to be deducted per instance.
        /// </summary>
        public decimal? AmountPerPayment { get; set; }

        /// <summary>
        /// The total amount to be deducted. 
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// The date when the payroll deductions should begin.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The date when the payroll deductions should end.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The number of pay periods between deductions, e.g 1 = every pay period, 2 = every other pay period
        /// </summary>
        public int? Interval { get; set; }

        /// <summary>
        /// A list of pay periods during a month when deductions should occur.
        /// </summary>
        public List<int?> MonthlyPayPeriods { get; set; }

        /// <summary>
        /// The reason why a property was changed (example: status change).
        /// </summary>
        public string ChangeReason { get; set; }

        /// <summary>
        /// Create an Payroll Deduction Arrangement Object
        /// </summary>
        /// <param name="guid">The global identifier for the person/benefit (PERBEN) record</param>
        /// <param name="personId">The Colleague PERSON id of the person</param>
        public PayrollDeductionArrangements(string guid, string personId)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("id", string.Format("The record key for PERBEN was not provided in PayrollDeductionArrangement. personId: '{0}' ", personId));
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", string.Format("The record key for PERBEN was not provided in PayrollDeductionArrangement. guid: '{0}' ", guid));
            }

            Guid = guid;
            PersonId = personId;
            MonthlyPayPeriods = new List<int?>();
        }

        /// <summary>
        /// Two PayrollDeductionArrangement are equal when their Ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var payrollDeductionArrangement = obj as PayrollDeductionArrangements;

            return payrollDeductionArrangement.Guid == Guid;
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
