// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public abstract class StudentCoordinationService : BaseCoordinationService
    {
        private IStudentRepository _studentRepository;
        private readonly IConfigurationRepository _configurationRepository;

        protected StudentCoordinationService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStudentRepository studentRepository, IConfigurationRepository configurationRepository, IStaffRepository staffRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            _studentRepository = studentRepository;
            _configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Confirms that the user is the student being accessed
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        protected bool UserIsSelf(string studentId)
        {
            // Access is Ok if the current user is this student
            if (CurrentUser.IsPerson(studentId))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Confirms this user is advisor to this person. If user is an advisor
        /// with "ViewAnyAdvisee" permission, then return true. If user is an advisor with
        /// "ViewAssignedAdvisees" permission but the advisor is not in the list of advisors,
        /// return false. 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="advisorIds"></param>
        /// <returns>boolean</returns>
        public async Task<bool> UserIsAdvisorAsync(string studentId, Domain.Student.Entities.StudentAccess student = null)
        {
            // Get user permissions
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();
            // Allow user to pass as advisor if they have permission to view any student
            if (userPermissions.Contains(PlanningPermissionCodes.ViewAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.ReviewAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.UpdateAnyAdvisee) || userPermissions.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee))
            {
                return true;
            }
            // Allow user to pass as advisor only if they are in the list of student's advisors
            if (userPermissions.Contains(PlanningPermissionCodes.ViewAssignedAdvisees) || userPermissions.Contains(PlanningPermissionCodes.ReviewAssignedAdvisees) || userPermissions.Contains(PlanningPermissionCodes.UpdateAssignedAdvisees) || userPermissions.Contains(PlanningPermissionCodes.AllAccessAssignedAdvisees))
            {
                if (student == null)
                {
                    student = (await _studentRepository.GetStudentAccessAsync(new List<string>() { studentId })).FirstOrDefault();
                }
                if (student != null && student.AdvisorIds != null)
                {
                    foreach (var advisorId in student.AdvisorIds)
                    {
                        if (CurrentUser.IsPerson(advisorId))
                        {
                            return true;
                        }
                    }
                }
            }
            // If not otherwise returned as true, return as false
            return false;
        }

        /// <summary>
        /// If the current user does not have permissions to access the given student, throws an error.
        /// </summary>
        /// <param name="student"></param>
        public async Task CheckUserAccessAsync(string studentId, Domain.Student.Entities.StudentAccess student = null)
        {
            var proxySubject = CurrentUser.ProxySubjects.FirstOrDefault();

            // They're allowed to see another's data if they are a proxy for that user or have the admin permission
            if (UserIsSelf(studentId) || (await UserIsAdvisorAsync(studentId, student)) || HasPermission(StudentPermissionCodes.ViewStudentInformation) || HasProxyAccessForPerson(studentId))
            {
                return;
            }
            // If not one of the above conditions is true, the user does not have permissions to access this student and we throw this exception
            throw new PermissionsException("User does not have permissions to access to this student");
        }

        /// <summary>
        /// If the current user does not have permissions to access the given student (by virtue of either being the student or having correct advisor permissions), then throw a permission exception.
        /// Proxy access is not allowed for methods using this access check.
        /// </summary>
        /// <param name="student"></param>
        public async Task CheckStudentAdvisorUserAccessAsync(string studentId, Domain.Student.Entities.StudentAccess student = null)
        {

            // They're allowed to see another's transcript data if they are that user or have the correct advisor permission
            if (UserIsSelf(studentId) || (await UserIsAdvisorAsync(studentId, student)))
            {
                return;
            }
            // If not one of the above conditions is true, the user does not have permissions to access this student and we throw this exception
            throw new PermissionsException("User does not have permissions to access to this student");
        }

        /// <summary>
        /// If the current user does not have permissions to access person restrictions, throws an error.
        /// </summary>
        /// <param name="studentId">Person ID for which to retrieve restrictions</param>
        public void CheckStudentRestrictionsAccess(string studentId)
        {
            var proxySubject = CurrentUser.ProxySubjects.FirstOrDefault();

            // Allowed access to restrictions if the user is a proxy for the student or has the view person restriction permission
            if (CurrentUser.IsPerson(studentId) || HasPermission(Domain.Base.BasePermissionCodes.ViewPersonRestrictions) || HasProxyAccessForPerson(studentId))
            {
                return;
            }
            // The user does not have permissions to access this student
            throw new PermissionsException("User does not have permissions to access student restrictions");
        }

        /// <summary>
        /// Determines if the user has permission to perform registration types of actions on the student's degree plan - will throw a PermissionException if not permitted.
        /// </summary>
        /// <param name="student">Student for whom the action is being taken.</param>
        public async Task CheckRegisterPermissionsAsync(string studentId, Ellucian.Colleague.Domain.Student.Entities.StudentAccess studentAccess = null)
        {
            // Access is Ok if the current user is this student
            if (UserIsSelf(studentId)) { return; }

            // Get user permissions
            IEnumerable<string> userPermissions = await GetUserPermissionCodesAsync();

            //Access is Ok if this is an advisor with full access to any student or has full access to their assigned advisees and this an an assigned advisee.
            if (userPermissions.Contains(PlanningPermissionCodes.AllAccessAnyAdvisee) || (userPermissions.Contains(PlanningPermissionCodes.AllAccessAssignedAdvisees) && (await UserIsAssignedAdvisorAsync(studentId, studentAccess))))
            {
                return;
            }

            // User does not have permissions and error needs to be thrown and logged
            logger.Info(CurrentUser + " does not have permissions to register for this student");
            throw new PermissionsException();
        }

        /// <summary>
        /// Confirms this user is advisor to this person. If user is an advisor
        /// with "ViewAnyAdvisee" permission, then return true. If user is an advisor with
        /// "ViewAssignedAdvisees" permission but the advisor is not in the list of advisors,
        /// return false. 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="advisorIds"></param>
        /// <returns>boolean</returns>
        public async Task<bool> UserIsAssignedAdvisorAsync(string studentId, Domain.Student.Entities.StudentAccess student = null)
        {
            // Determine if the user is an assigned advisor for this student.
            // If student not passed, get it now
            if (student == null)
            {
                student = (await _studentRepository.GetStudentAccessAsync(new List<string>() { studentId })).FirstOrDefault();
            }
            // Return true if advisor is in the list of student's advisors
            if (student != null && student.AdvisorIds != null)
            {
                foreach (var advisorId in student.AdvisorIds)
                {
                    if (CurrentUser.IsPerson(advisorId))
                    {
                        return true;
                    }
                }
            }
            // If not otherwise returned as true, return as false
            return false;
        }

        /// <summary>
        /// Helper method to determine if the user is an assigned faculty on a provided section
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        public bool UserIsSectionFaculty(Domain.Student.Entities.Section section)
        {
            if (section == null)
            {
                throw new ArgumentNullException("Must provide a section to check faculty permission.");
            }
            if (section.FacultyIds != null && section.FacultyIds.Contains(CurrentUser.PersonId))
            {
                return true;
            }
            string error = "Current user is not authorized to modify information for section : " + section.Id;
            logger.Error(error);
            throw new PermissionsException(error);

        }
    }
}
