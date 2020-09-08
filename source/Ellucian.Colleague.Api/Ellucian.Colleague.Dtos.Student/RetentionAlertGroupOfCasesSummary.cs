// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Summary of Groups of Retention Alert Cases
    /// </summary>
    public class RetentionAlertGroupOfCasesSummary
    {
        /// <summary>
        /// Summary description of the Groups of Cases
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// List of Retention Alert Cases owned by a Role
        /// </summary>
        public List<RetentionAlertGroupOfCases> RoleCases { get; set; }

        /// <summary>
        /// List of Retention Alert Cases owned by individual Entitiles
        /// </summary>
        public List<RetentionAlertGroupOfCases> EntityCases { get; set; }

        /// <summary>
        /// Total number of Retention Alert Cases from the RoleCases and the EntityCases lists.
        /// </summary>
        public int TotalCases { get; set; }
    }
}


