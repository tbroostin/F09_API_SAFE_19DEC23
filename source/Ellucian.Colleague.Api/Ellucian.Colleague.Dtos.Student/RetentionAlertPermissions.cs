// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention alert permissions dto 
    /// </summary>
    public class RetentionAlertPermissions
    {
        /// <summary>
        /// Indicates whether the user can work on retention alert cases.
        /// </summary>
        public bool CanWorkCases { get; set; }

        /// <summary>
        /// Indicates whether the user can work on any retention alert case.
        /// </summary>
        public bool CanWorkAnyCase { get; set; }

        /// <summary>
        /// Indicates whether the user can contribute to retention alert cases.
        /// </summary>
        public bool CanContributeToCases { get; set; }
    }
}
