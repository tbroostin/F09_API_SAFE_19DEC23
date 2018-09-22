/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class PayPeriod
    {
        /// <summary>
        /// The database Id
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The database Description
        /// </summary>
        public string Description { get { return description; } }
        private readonly string description;

        /// <summary>
        /// The database PayDate
        /// </summary>
        public DateTime PayDate { get { return payDate; } }
        private readonly DateTime payDate;

        /// <summary>
        /// The database PayCycle
        /// </summary>
        public string PayCycle { get { return payCycle; } }
        private readonly string payCycle;

        /// <summary>
        /// The database TimeEntryEndOn
        /// </summary>
        public DateTime? TimeEntryEndOn { get; set; }

        /// <summary>
        /// The pay period start date
        /// </summary>
        public DateTime StartDate2 { get { return startDate2; } }
        private DateTime startDate2;

        /// <summary>
        /// The pay period end date
        /// </summary>
        public DateTime EndDate2 { get { return endDate2; } }
        private DateTime endDate2;

        /// <summary>
        /// The pay period start date
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                if (value > endDate)
                {
                    throw new ArgumentException("StartDate cannot be later than EndDate");
                }
                startDate = value;
            }
        }
        private DateTime startDate;

        /// <summary>
        /// The pay period end date
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return endDate;
            }
            set
            {
                if (startDate > value)
                {
                    throw new ArgumentException("StartDate cannot be later than EndDate");
                }
                endDate = value;
            }
        }
        private DateTime endDate;


        public DateTimeOffset? EmployeeTimecardCutoffDateTime { get; set; }
        public DateTimeOffset? SupervisorTimecardCutoffDateTime { get; set; }

        public PayPeriodStatus Status { get { return status; } }
        private PayPeriodStatus status;
        
        /// <summary>
        /// Constructor for a PayPeriod object
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public PayPeriod(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("StartDate cannot be later than EndDate");
            }

            this.EndDate = endDate; 
            this.StartDate = startDate;
        }

        /// <summary>
        /// Constructor for a PayPeriod object
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="status"></param>
        public PayPeriod(DateTime startDate, DateTime endDate, DateTimeOffset? employeeTimecardCutoffDateTime, DateTimeOffset? supervisorTimecardCutoffDateTime, PayPeriodStatus status, string payCycleId)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("StartDate cannot be later than EndDate");
            }
            if (string.IsNullOrWhiteSpace(payCycleId))
            {
                throw new ArgumentNullException("payCycleId");
            }

            this.EndDate = endDate;
            this.StartDate = startDate;
            this.EmployeeTimecardCutoffDateTime = employeeTimecardCutoffDateTime;
            this.SupervisorTimecardCutoffDateTime = supervisorTimecardCutoffDateTime;
            this.status = status;
            payCycle = payCycleId;
        }

        /// <summary>
        /// Constructor for a PayPeriod object
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="status"></param>
        public PayPeriod(string id, string description, DateTime startDate, DateTime endDate, DateTime payDate, string payCycle)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id is required for PayPeriod entity.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("description is required for PayPeriod entity.");
            }

            if (startDate == null)
            {
                throw new ArgumentNullException("startDate is required for PayPeriod entity.");
            }

            if (endDate == null)
            {
                throw new ArgumentNullException("endDate is required for PayPeriod entity.");
            }

            if (string.IsNullOrWhiteSpace(payCycle))
            {
                throw new ArgumentNullException("payCycle is required for PayPeriod entity.");
            }

            if (startDate > endDate)
            {
                throw new ArgumentException("StartDate cannot be later than EndDate");
            }

            this.id = id;
            this.description = description;
            this.startDate2 = startDate;
            this.endDate2 = endDate;
            this.payDate = payDate;
            this.payCycle = payCycle;
        }

        /// <summary>
        /// Two pay period date ranges are equal when their start and end dates are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var payPeriod = obj as PayPeriod;
            return payPeriod.StartDate == this.StartDate && payPeriod.EndDate == this.EndDate;
        }

        /// <summary>
        /// Hashcode representation of PayPeriod (StartDate & EndDate)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return StartDate.GetHashCode() ^ EndDate.GetHashCode();
        }
    }
}
