// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Retention Alert Case Category Org Roles List
    /// </summary>
    public class RetentionAlertCaseCategoryOrgRoles
    {
        /// <summary>
        /// Case Category Ids
        /// </summary>
        public string CaseCategoryId { get; set; }

        /// <summary>
        /// List of Org Roles and associated settings within this Case Category
        /// </summary>
        public List<RetentionAlertCaseCategoryOrgRole> CaseCategoryOrgRoles { get; set; }

    }
}
