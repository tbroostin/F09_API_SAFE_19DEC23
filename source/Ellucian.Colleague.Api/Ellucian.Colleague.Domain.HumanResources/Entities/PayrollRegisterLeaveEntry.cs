// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Describes the leave information for a pay period. 
    /// A LeaveEntry must be assigned to a PayrollRegisterEntry.
    /// </summary>
    [Serializable]
    public class PayrollRegisterLeaveEntry
    {
        /// <summary>
        /// The leave code
        /// </summary>
        public string LeaveCode { get; private set; }

        /// <summary>
        /// The type of leave
        /// </summary>
        public string LeaveType { get; set; }

        /// <summary>
        /// Amount of leave taken this period
        /// </summary>
        public decimal LeaveTaken { get; set; }

        /// <summary>
        /// Amount of leave remaining of this type
        /// </summary>
        public decimal LeaveRemaining { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveCode"></param>
        /// <param name="leaveType"></param>
        /// <param name="leaveTaken"></param>
        /// <param name="leaveRemaining"></param>
        public PayrollRegisterLeaveEntry(string leaveCode, string leaveType, decimal? leaveTaken, decimal? leaveRemaining)
        {
            if (leaveCode == null)
            {
                throw new ArgumentNullException("leaveCode");
            }

            if (leaveType == null)
            {
                throw new ArgumentNullException("leaveType");
            }

            LeaveCode = leaveCode;
            LeaveType = leaveType;
            LeaveTaken = leaveTaken.HasValue ? leaveTaken.Value : 0;
            LeaveRemaining = leaveRemaining.HasValue ? leaveRemaining.Value : 0;
        }
    }
}
