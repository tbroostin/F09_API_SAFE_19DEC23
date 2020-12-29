/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
