// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class RetentionAlertWorkCaseReassign
    {
        /// <summary>
        /// The ID of the person who should be recorded as updating the records in Colleague
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// The list of reassign case owners
        /// </summary>
        public List<RetentionAlertCaseReassignmentDetail> ReassignOwners { get; set; }

        /// <summary>
        /// The Detailed notes given during reassignment
        /// </summary>
        public List<string> Notes { get; set; }
    }
}
