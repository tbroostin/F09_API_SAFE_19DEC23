using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// A PayStatementLeave entity that represents the leave data on a Pay Statement report
    /// </summary>
    [Serializable]
    public class PayStatementLeave
    {

        /// <summary>
        /// The id of the leave type
        /// </summary>
        public string LeaveTypeId { get; set; }

        /// <summary>
        /// The type of leave
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Amount of leave taken this period
        /// </summary>
        public decimal? LeaveTaken { get; set; }

        /// <summary>
        /// Amount of leave remaining of this type
        /// </summary>
        public decimal? LeaveRemaining { get; set; }

        /// <summary>
        /// Create a leave line item
        /// </summary>
        /// <param name="description"></param>
        /// <param name="leaveTaken"></param>
        /// <param name="leaveRemaining"></param>
        public PayStatementLeave(string id, string description, decimal? leaveTaken, decimal? leaveRemaining)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            LeaveTypeId = id;
            Description = description;
            LeaveTaken = leaveTaken;
            LeaveRemaining = leaveRemaining;
        }
    }
}
