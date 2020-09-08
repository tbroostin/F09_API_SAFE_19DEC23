// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Group of Retention Alert Cases
    /// </summary>
    public class RetentionAlertGroupOfCases 
    {
        /// <summary>
        /// Org Entity ID or Org Role ID of the owner of the group of Cases
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the Group of Retention Alert Cases
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Retention Alert Case Ids
        /// </summary>
        public List<string> CaseIds { get; set; }

        /// <summary>
        /// Number of Retention Alert Cases
        /// </summary>
        public int NumberOfCases { get; set; }
    }
}


