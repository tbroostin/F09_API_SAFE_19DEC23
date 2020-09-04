﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention Alert Case Category Org Role and settings.
    /// </summary>
    public class RetentionAlertCaseCategoryOrgRole
    {
        /// <summary>
        /// Org Role Id
        /// </summary>
        public string OrgRoleId { get; set; }

        /// <summary>
        /// Org Role Name
        /// </summary>
        public string OrgRoleName { get; set; }

        /// <summary>
        /// Is this Case Category assigned when the case is created?
        /// </summary>
        public string IsAssignedInitially { get; set; }

        /// <summary>
        /// Is this Case Category available for reassignment when the case is being worked?
        /// </summary>
        public string IsAvailableForReassignment { get; set; }

        /// <summary>
        /// Is this Case Category available for reporting, or reporting with administrative privileges?
        /// </summary>
        public string IsReportingAndAdministrative { get; set; }

    }
}
