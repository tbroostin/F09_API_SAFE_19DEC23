// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Query request for retention alert cases
    /// </summary>
    public class RetentionAlertQueryCriteria
    {
        /// <summary>
        /// Student Name or Id to retrieve cases
        /// </summary>
        public string StudentSearchKeyword { get; set; }

        /// <summary>
        /// Case Ids to retrieve cases
        /// </summary>
        public IEnumerable<string> CaseIds { get; set; }

        /// <summary>
        /// Is Include closed cases
        /// </summary>
        public bool IsIncludeClosedCases { get; set; }
    }
}
