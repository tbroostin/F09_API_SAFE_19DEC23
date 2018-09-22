// Copyright 2015 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Details for a specific requisite (spec-based) waiver
    /// </summary>
    public class RequisiteWaiver
    {
        /// <summary>
        /// ID of the academic requirement that describes the requisite being waived
        /// </summary>
        public string RequisiteId { get; set; }

        /// <summary>
        /// Indicates whether waiver has had no action, has been approved, or has been denied.
        /// </summary>
        public WaiverStatus Status { get; set; }
    }
}
