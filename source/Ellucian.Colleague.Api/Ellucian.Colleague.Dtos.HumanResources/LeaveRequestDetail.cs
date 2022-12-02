/* Copyright 2021 Ellucian Company L.P. and its affiliates. */
using System;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO for leave request detail.
    /// Each leave request record may contain many leave request detail records.
    /// A leave request detail record contains the leave related details for one day.
    /// </summary>
    public class LeaveRequestDetail
    {
        /// <summary>
        /// DB id of this leave request detail object
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the leave request object
        /// </summary>
        public string LeaveRequestId { get; set; }

        /// <summary>
        /// Date for which leave is requested
        /// </summary>
        public DateTime LeaveDate { get; set; }

        /// <summary>
        /// Hours of leave requested
        /// </summary>
        public decimal? LeaveHours { get; set; }

        /// <summary>
        /// Indicates if this detail record has been processed in a pay period by payroll
        /// </summary>
        public bool ProcessedInPayPeriod { get; set; }

        /// <summary>
        /// Leave request detail change operator
        /// </summary>
        public string LeaveRequestDetailChgopr { get; set; }
    }
}
