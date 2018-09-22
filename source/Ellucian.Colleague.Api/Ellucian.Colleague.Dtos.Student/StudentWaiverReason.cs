// Copyright 2015 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Locally defined reasons for a waiver
    /// </summary>
    public class StudentWaiverReason
    {
        /// <summary>
        /// The reason code (unique identifier)
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// The description for this code.
        /// </summary>
        public string Description { get; set; }
    }
}
