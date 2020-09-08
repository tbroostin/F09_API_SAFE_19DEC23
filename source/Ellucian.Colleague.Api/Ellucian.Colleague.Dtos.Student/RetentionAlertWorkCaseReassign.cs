// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention alert work case for Reassign Owners action
    /// </summary>
    public class RetentionAlertWorkCaseReassign
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// List of Retention Case Reassign Owners
        /// </summary>
        public IEnumerable<RetentionAlertCaseReassignmentDetail> ReassignOwners { get; set; }

        /// <summary>
        /// The Detailed notes given during reassignment
        /// </summary>
        public List<string> Notes { get; set; }
    }
}