/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Represents leave for a Pay Statement
    /// </summary>
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
    }
}
