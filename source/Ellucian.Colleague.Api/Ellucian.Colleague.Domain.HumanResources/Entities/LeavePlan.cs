/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// LeavePlan
    /// </summary>
    [Serializable]
    public class LeavePlan
    {
        /// <summary>
        /// The global identifier of the employee leave plan
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// id for the leaveplan
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        private readonly string id;

        /// <summary>
        /// The full name of the leave plan.
        /// </summary>
        public string Title
        {
            get { return title; }
        }

        private readonly string title;

        /// <summary>
        /// The type of leave (e.g. sick, vacation, etc.)
        /// </summary>
        public string Type
        {
            get { return type; }
        }

        private readonly string type;

        /// <summary>
        /// The method by which leave can be accumulated.
        /// </summary>
        public string AccrualMethod
        {
            get { return accrualMethod; }
        }

        private readonly string accrualMethod;

        /// <summary>
        /// An indication whether the leave plan can be used before accrual or after accrual
        /// </summary>
        public string AllowNegative { get; set; }

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
        /// The default start of the plan in a year.
        /// </summary>
        public DateTime? YearlyStartDate { get; set; }
        
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
        /// The global identifier for the Alternate Rollover Leave Type
        /// </summary>
        public string RollOverLeaveType { get; set; }


        /// <summary>
        /// The frequency at which leave can be accrue
        /// </summary>
        public string AccuralFrequency { get; set; }

        /// <summary>
        /// The amount of time an employee must wait until he/she can take a leave.
        /// </summary>
        public decimal? WaitDays { get; set; }

        /// <summary>
        /// A list of earnings types associated to this leave plan.
        /// </summary>
        public IEnumerable<string> EarningsTypes { get; set; }

        /// <summary>
        /// Create a Leave Plan
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="id">A code that may be used to identify the leave plan.</param>
        /// <param name="startDate">The start date of the leave plan.</param>
        /// <param name="title">The full name of the leave plan.</param>
        /// <param name="type">The type of leave (e.g. sick, vacation, etc.).</param>
        /// <param name="accuralMethod">The method by which leave can be accumulated.</param>
        public LeavePlan(string guid, string id, DateTime? startDate, string title, string type, string accuralMethod, IEnumerable<string> earningsType = null)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException(string.Concat("LeavePlans guid can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(string.Concat("LeavePlans id can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(string.Concat("LeavePlans title can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException(string.Concat("LeavePlans type can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }
            if (string.IsNullOrEmpty(accuralMethod))
            {
                throw new ArgumentNullException(string.Concat("LeavePlans accuralMethod can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }
            if ((startDate == null) || (!startDate.HasValue))
            {
                throw new ArgumentNullException(string.Concat("LeavePlans startDate can not be null or empty. Entity: ‘LEAVPLAN’, Record ID: '", id, "'"));
            }

            this.id = id;
            this.title= title;
            this.type = type;
            this.startDate = startDate;
            this.accrualMethod = accuralMethod;
            this.Guid = guid;
            this.EarningsTypes = earningsType;
        }

      
    }
}