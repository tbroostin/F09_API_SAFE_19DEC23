// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Retention alert permissions entity 
    /// </summary>
    [Serializable]
    public class RetentionAlertPermissions
    {
        /// <summary>
        /// Indicates whether the user can work on retention alert cases.
        /// </summary>
        public bool CanWorkCases { get { return canWorkCases; } }
        private readonly bool canWorkCases;

        /// <summary>
        /// Indicates whether the user can work on any retention alert case.
        /// </summary>
        public bool CanWorkAnyCase { get { return canWorkAnyCase; } }
        private readonly bool canWorkAnyCase;

        /// <summary>
        /// Indicates whether the user can contribute to retention alert cases.
        /// </summary>
        public bool CanContributeToCases { get { return canContributeToCases; } }
        private readonly bool canContributeToCases;

        public RetentionAlertPermissions()
        {
            canWorkCases = false;
            canWorkAnyCase = false;
            canContributeToCases = false;
        }

        /// <summary>
        /// Creates a new <see cref="RetentionAlertPermissions"/> object and sets permissions based on the supplied permission codes
        /// </summary>
        /// <param name="permissionCodes">Permission codes</param>
        public RetentionAlertPermissions(IEnumerable<string> permissionCodes)
        {
            if (permissionCodes == null)
            {
                throw new ArgumentNullException("permissionCodes", "Collection of permission codes cannot be null when building retention alert permissions.");
            }

            if (permissionCodes.Contains(RetentionAlertPermissionCodes.WorkCases))
            {
                canWorkCases = true;
            }

            if (permissionCodes.Contains(RetentionAlertPermissionCodes.WorkAnyCase))
            {
                canWorkAnyCase = true;
            }

            if (permissionCodes.Contains(RetentionAlertPermissionCodes.ContributeToCases))
            {
                canContributeToCases = true;
            }
        }
    }
}
