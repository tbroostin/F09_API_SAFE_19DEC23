/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// LeavePlan
    /// </summary>
    [Serializable]
    public class PerleaveDetails
    {
        /// <summary>
        /// The global identifier of the employee leave transactions
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// id for the employee leave transactions
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        private readonly string id;

        /// <summary>
        /// The leave plan for which the transaction occurred.
        /// </summary>
        public string EmployeeLeaveId
        {
            get { return employeeLeaveId; }
        }

        private readonly string employeeLeaveId;

        /// <summary>
        /// The date of the transaction.
        /// </summary>
        public DateTime? TransactionDate
        {
            get { return tranDate; }
            
        }
        private DateTime? tranDate { get; set; }

        /// <summary>
        /// The number of hours accrued/taken for leave.
        /// </summary>
        /// 
        public decimal? LeaveHours { get; set; }

        /// <summary>
        /// The number of hours available for leave.
        /// </summary>
        /// 
        public decimal? AvailableHours { get; set; }


        /// <summary>
        /// Create a Leave Plan
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="id"> employee leave transactions Id.</param>
        /// <param name="leaveId">The global identifier for the Employee Leave.</param>
        /// <param name="tranDate">The type of leave (e.g. sick, vacation, etc.).</param>
        public PerleaveDetails(string guid, string id, DateTime? tranDate, string leaveId)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentException(string.Concat("EmployeeLeaveTransactions guid can not be null or empty. Entity: ‘PERLVDTL’, Record ID: '", id, "'"));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(string.Concat("EmployeeLeaveTransactions id can not be null or empty. Entity: ‘PERLVDTL’, Record ID: '", id, "'"));
            }
            if (string.IsNullOrEmpty(leaveId))
            {
                throw new ArgumentNullException(string.Concat("EmployeeLeaveTransactions Leave Id can not be null or empty. Entity: ‘PERLVDTL’, Record ID: '", id, "'"));
            }
            if ((tranDate == null) || (!tranDate.HasValue))
            {
                throw new ArgumentNullException(string.Concat("EmployeeLeaveTransactions Transaction Date can not be null or empty. Entity: ‘PERLVDTL’, Record ID: '", id, "'"));
            }

            this.id = id;
            this.employeeLeaveId= leaveId;
            this.tranDate = tranDate;
            this.Guid = guid;
        }

      
    }
}