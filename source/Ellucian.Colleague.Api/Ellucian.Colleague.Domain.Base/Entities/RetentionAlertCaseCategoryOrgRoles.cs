// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Retention Alert Case Category Org Roles List
    /// </summary>
    [Serializable]
    public class RetentionAlertCaseCategoryOrgRoles
    {
        /// <summary>
        /// Case Category Id
        /// </summary>
        public string CaseCategoryId { get; set; }

        /// <summary>
        /// List of Org Roles and associated settings within this Case Category
        /// </summary>
        public List<RetentionAlertCaseCategoryOrgRole> CaseCategoryOrgRoles { get; set; }

        /// <summary>
        /// Create a RetentionAlertCaseCategoryOrgRoles object.
        /// </summary>
        public RetentionAlertCaseCategoryOrgRoles()
        {
            CaseCategoryOrgRoles = new List<RetentionAlertCaseCategoryOrgRole>();
        }
    }
}
