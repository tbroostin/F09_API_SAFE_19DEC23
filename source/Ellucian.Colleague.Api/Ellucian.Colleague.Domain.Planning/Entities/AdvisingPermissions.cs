// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    /// <summary>
    /// Permissions that an advisor has with respect to advisees
    /// </summary>
    [Serializable]
    public class AdvisingPermissions
    {
        private readonly bool canViewAssignedAdvisees;
        private readonly bool canReviewAssignedAdvisees;
        private readonly bool canUpdateAssignedAdvisees;
        private readonly bool hasFullAccessForAssignedAdvisees;
        private readonly bool canViewAnyAdvisee;
        private readonly bool canReviewAnyAdvisee;
        private readonly bool canUpdateAnyAdvisee;
        private readonly bool hasFullAccessForAnyAdvisee;
        private readonly bool canCreateStudentAcademicProgram;

        /// <summary>
        /// Advisor can view their assigned advisees' degree plans
        /// </summary>
        public bool CanViewAssignedAdvisees { get { return canViewAssignedAdvisees; } }

        /// <summary>
        /// Advisor can review their assigned advisees degree plans
        /// </summary>
        public bool CanReviewAssignedAdvisees { get { return canReviewAssignedAdvisees; } }

        /// <summary>
        /// Advisor can update their assigned advisees' data
        /// </summary>
        public bool CanUpdateAssignedAdvisees { get { return canUpdateAssignedAdvisees; } }

        /// <summary>
        /// Advisor can view, review, and update their assigned advisees' data, and can register assigned advisees for classes
        /// </summary>
        public bool HasFullAccessForAssignedAdvisees { get { return hasFullAccessForAssignedAdvisees; } }

        /// <summary>
        /// Advisor can view any advisee's degree plan
        /// </summary>
        public bool CanViewAnyAdvisee { get { return canViewAnyAdvisee; } }

        /// <summary>
        /// Advisor can review any advisee's degree plan
        /// </summary>
        public bool CanReviewAnyAdvisee { get { return canReviewAnyAdvisee; } }

        /// <summary>
        /// Advisor can update any advisee's degree plan
        /// </summary>
        public bool CanUpdateAnyAdvisee { get { return canUpdateAnyAdvisee; } }

        /// <summary>
        /// Advisor can view, review, and update any advisee's degree plan, and can register any advisee for classes
        /// </summary>
        public bool HasFullAccessForAnyAdvisee { get { return hasFullAccessForAnyAdvisee; } }

        /// <summary>
        /// Advisor can create or update student academic programs
        /// </summary>
        public bool CanCreateStudentAcademicProgram { get { return canCreateStudentAcademicProgram; } }

        /// <summary>
        /// Creates a new <see cref="AdvisingPermissions"/> object.
        /// </summary>
        public AdvisingPermissions()
        {
            canViewAssignedAdvisees = false;
            canReviewAssignedAdvisees = false;
            canUpdateAssignedAdvisees = false;
            hasFullAccessForAssignedAdvisees = false;
            canViewAnyAdvisee = false;
            canReviewAnyAdvisee = false;
            canUpdateAnyAdvisee = false;
            hasFullAccessForAnyAdvisee = false;
            canCreateStudentAcademicProgram = false;
        }

        /// <summary>
        /// Creates a new <see cref="AdvisingPermissions"/> object and sets permissions based on the supplied permission codes
        /// </summary>
        /// <param name="permissionCodes">Permission codes from which advising permissions will be built</param>
        /// <param name="logger">Logging interface</param>
        public AdvisingPermissions(IEnumerable<string> permissionCodes, ILogger logger = null) : base()
        {
            if (permissionCodes == null)
            {
                throw new ArgumentNullException("Collection of permission codes cannot be null when building advisor permissions.");
            }
            if (logger == null)
            {
                logger = slf4net.LoggerFactory.GetLogger(typeof(AdvisingPermissions));
            }
            if (permissionCodes.Contains(StudentPermissionCodes.CreateStudentAcademicProgramConsent))
            {
                canCreateStudentAcademicProgram = true;
                if (logger != null)
                {
                    logger.Debug(string.Format("User has permission {0}; CanCreateStudentAcademicProgram = true.", StudentPermissionCodes.CreateStudentAcademicProgramConsent));
                }
            }
            if (permissionCodes.Contains(PlanningPermissionCodes.ViewAssignedAdvisees))
            {
                canViewAssignedAdvisees = true;
                if (logger != null)
                {
                    logger.Debug(string.Format("User has permission {0}; CanViewAssignedAdvisees = true.", PlanningPermissionCodes.ViewAssignedAdvisees));
                }
            }
            if (permissionCodes.Contains(PlanningPermissionCodes.ReviewAssignedAdvisees))
            {
                canViewAssignedAdvisees = true;
                canReviewAssignedAdvisees = true;
                if (logger != null)
                {
                    logger.Debug(string.Format("User has permission {0}; CanViewAssignedAdvisees = true. CanReviewAssignedAdvisees = true.", PlanningPermissionCodes.ReviewAssignedAdvisees));
                }
            }
            if (permissionCodes.Contains(PlanningPermissionCodes.UpdateAssignedAdvisees))
            {
                canViewAssignedAdvisees = true;
                canReviewAssignedAdvisees = true;
                canUpdateAssignedAdvisees = true;
                if (logger != null)
                {
                    logger.Debug(string.Format("User has permission {0}; CanViewAssignedAdvisees = true. CanReviewAssignedAdvisees = true. CanUpdateAssignedAdvisees = true.", PlanningPermissionCodes.UpdateAssignedAdvisees));
                }
            }
            if (permissionCodes.Contains(PlanningPermissionCodes.AllAccessAssignedAdvisees))
            {
                canViewAssignedAdvisees = true;
                canReviewAssignedAdvisees = true;
                canUpdateAssignedAdvisees = true;
                hasFullAccessForAssignedAdvisees = true;
                if (logger != null)
                {
                    logger.Debug(string.Format("User has permission {0}; CanViewAssignedAdvisees = true. CanReviewAssignedAdvisees = true. CanUpdateAssignedAdvisees = true. HasFullAccessForAssignedAdvisees = true.", PlanningPermissionCodes.AllAccessAssignedAdvisees));
                }
            }
            if (permissionCodes.Contains(PlanningPermissionCodes.ViewAnyAdvisee))
            {
                canViewAnyAdvisee = true;
                canViewAssignedAdvisees = true;
                if (logger != null)
                {
                    logger.Debug(string.Format("User has permission {0}; CanViewAnyAdvisee = true. CanViewAssignedAdvisees = true.", PlanningPermissionCodes.ViewAnyAdvisee));
                }
            }
            if (permissionCodes.Contains(PlanningPermissionCodes.ReviewAnyAdvisee))
            {
                canViewAnyAdvisee = true;
                canReviewAnyAdvisee = true;
                canViewAssignedAdvisees = true;
                canReviewAssignedAdvisees = true;
                if (logger != null)
                {
                    logger.Debug(string.Format("User has permission {0}; CanViewAnyAdvisee = true. CanReviewAnyAdvisee = true. CanViewAssignedAdvisees = true. CanReviewAssignedAdvisees = true.", PlanningPermissionCodes.ReviewAnyAdvisee));
                }
            }
            if (permissionCodes.Contains(PlanningPermissionCodes.UpdateAnyAdvisee))
            {
                canViewAnyAdvisee = true;
                canReviewAnyAdvisee = true;
                canUpdateAnyAdvisee = true;
                canViewAssignedAdvisees = true;
                canReviewAssignedAdvisees = true;
                canUpdateAssignedAdvisees = true;
                if (logger != null)
                {
                    logger.Debug(string.Format("User has permission {0}; CanViewAnyAdvisee = true. CanReviewAnyAdvisee = true. CanUpdateAnyAdvisee = true. CanViewAssignedAdvisees = true. CanReviewAssignedAdvisees = true. CanUpdateAssignedAdvisees = true.", PlanningPermissionCodes.UpdateAnyAdvisee));
                }
            }
            if (permissionCodes.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee))
            {
                canViewAnyAdvisee = true;
                canReviewAnyAdvisee = true;
                canUpdateAnyAdvisee = true;
                hasFullAccessForAnyAdvisee = true;
                canViewAssignedAdvisees = true;
                canReviewAssignedAdvisees = true;
                canUpdateAssignedAdvisees = true;
                hasFullAccessForAssignedAdvisees = true;
                if (logger != null)
                {
                    logger.Debug(string.Format("User has permission {0}; CanViewAnyAdvisee = true. CanReviewAnyAdvisee = true. CanUpdateAnyAdvisee = true. HasFullAccessForAnyAdvisee = true. CanViewAssignedAdvisees = true. CanReviewAssignedAdvisees = true. CanUpdateAssignedAdvisees = true. HasFullAccessForAssignedAdvisees = true.", PlanningPermissionCodes.AllAccessAnyAdvisee));
                }
            }
        }
    }
}
