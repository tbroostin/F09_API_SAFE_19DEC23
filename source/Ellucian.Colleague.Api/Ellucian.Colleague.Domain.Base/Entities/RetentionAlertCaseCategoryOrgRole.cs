// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Retention Alert Case Category Org Role and settings.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Create a RetentionAlertCaseCategoryOrgRole object.
        /// </summary>
        public RetentionAlertCaseCategoryOrgRole()
        {

        }
    }
}
