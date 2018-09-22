// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Status for a payment plan
    /// </summary>
    [Serializable]
    public class PlanStatus
    {
        // Private fields
        private readonly PlanStatusType _status;
        private readonly DateTime _date;

        /// <summary>
        /// Payment plan status
        /// </summary>
        public PlanStatusType Status { get { return _status; } }
        
        /// <summary>
        /// Date as of which the plan had the status
        /// </summary>
        public DateTime Date { get { return _date; } }

        /// <summary>
        /// Constructor for plan status
        /// </summary>
        /// <param name="status">Plan status</param>
        /// <param name="date">Status date</param>
        public PlanStatus(PlanStatusType status, DateTime date)
        {
            _status = status;
            _date = date;
        }
    }
}
