// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordinates information to enable retrieval and update of Waivers
    /// </summary>
    [RegisterType]
    public class StudentWaiverService : StudentCoordinationService, IStudentWaiverService
    {
        private readonly IStudentWaiverRepository studentWaiverRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISectionRepository sectionRepository;
        private readonly IStudentProgramRepository studentProgramRepository;
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IReferenceDataRepository baseReferenceDataRepository;
        private readonly ICourseRepository courseRepository;
        private readonly IConfigurationRepository _configurationRepository;

        /// <summary>
        /// Initialize the service for accessing waiver functions
        /// </summary>
        /// <param name="adapterRegistry">Dto adapter registry</param>
        /// <param name="studentWaiverRepository">Waiver repository access</param>
        /// <param name="logger">error logging</param>
        public StudentWaiverService(IAdapterRegistry adapterRegistry, IStudentWaiverRepository studentWaiverRepository, ISectionRepository sectionRepository, IStudentProgramRepository studentProgramRepository, IStudentReferenceDataRepository referenceDataRepository, IReferenceDataRepository baseReferenceDataRepository, ICourseRepository courseRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IStudentRepository studentRepository, IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            this.studentWaiverRepository = studentWaiverRepository;
            this.sectionRepository = sectionRepository;
            this.studentProgramRepository = studentProgramRepository;
            this.referenceDataRepository = referenceDataRepository;
            this.baseReferenceDataRepository = baseReferenceDataRepository;
            this.courseRepository = courseRepository;
            this._studentRepository = studentRepository;
        }

        /// <summary>
        /// Get the student waivers granted for this section.
        /// </summary>
        /// <param name="sectionId">Id of the section for which to retrieve waivers</param>
        /// <returns>List of <see cref="Dtos.Student.StudentWaiver">Waiver</see> dto objects</returns>
        public async Task<IEnumerable<Dtos.Student.StudentWaiver>> GetSectionStudentWaiversAsync(string sectionId)
        {
            // Initialize list of waivers to be returned
            List<Dtos.Student.StudentWaiver> waivers = new List<Dtos.Student.StudentWaiver>();

            // Get the specified section from the repository
            Domain.Student.Entities.Section section = await GetSectionAsync(sectionId);
            if (section == null)
            {
                throw new KeyNotFoundException(string.Format("Couldn't retrieve section information for given sectionId {0}", sectionId));
            }

            var allDepartments = await baseReferenceDataRepository.DepartmentsAsync();
            var userPermissions = await GetUserPermissionCodesAsync();

            // The current user must be a faculty member of the section or a departmental oversight member with the required permissions
            if ((section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId)) &&
                    !(CheckDepartmentalOversightAccessForSection(section, allDepartments) && (userPermissions.Contains(DepartmentalOversightPermissionCodes.ViewSectionPrerequisiteWaiver) || userPermissions.Contains(DepartmentalOversightPermissionCodes.CreateSectionPrerequisiteWaiver))))
            {
                throw new PermissionsException(string.Format("Authenticated user is neither a faculty nor a departmental oversight for requested section {0} and therefore cannot access waivers", section.Id));
            }

            // Finally, Get the waivers from the repository for this section and convert to dtos
            // Log and rethrow any general exception that occurs in the repository.
            try
            {
                var waiverEntities = await studentWaiverRepository.GetSectionWaiversAsync(sectionId);
                if (waiverEntities != null)
                {
                    foreach (var waiver in waiverEntities)
                    {
                        // Get the right adapter for the type mapping
                        var waiverDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentWaiver, Dtos.Student.StudentWaiver>();
                        // Map the degree plan entity to the degree plan DTO
                        waivers.Add(waiverDtoAdapter.MapToType(waiver));
                    }
                }
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to read waivers from repository using section id " + sectionId + "Exception message: " + ex.Message;
                logger.Error(message);
                throw new ColleagueWebApiException(message);
            }

            return waivers;
        }

        /// <summary>
        /// Validates incoming waiver and calls repository to add waiver to the database, returning the newly created waiver.
        /// </summary>
        /// <param name="studentWaiver">Waiver to add to the database</param>
        /// <returns>The newly created <see cref="Dtos.Student.StudentWaiver">Waiver</see></returns>
        public async Task<Dtos.Student.StudentWaiver> CreateStudentWaiverAsync(Dtos.Student.StudentWaiver studentWaiver)
        {
            var userPermissions = await GetUserPermissionCodesAsync();
            // Throw exception if user does not have correct permissions
            if (!(userPermissions.Contains(StudentPermissionCodes.CreatePrerequisiteWaiver) || userPermissions.Contains(DepartmentalOversightPermissionCodes.CreateSectionPrerequisiteWaiver)))
            {
                var message = "User does not have permissions required to create a Section Prerequisite Waiver.";
                logger.Error(message);
                throw new PermissionsException(message);
            }

            // Throw exception if incoming waiver is null
            if (studentWaiver == null)
            {
                throw new ArgumentNullException("waiver", "Waiver object must be provided.");
            }

            // Catch any errors thrown by the constructor in the process of mapping to an entity
            Domain.Student.Entities.StudentWaiver waiverToAdd = null;
            try
            {
                var waiverEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentWaiver, Domain.Student.Entities.StudentWaiver>();
                waiverToAdd = waiverEntityAdapter.MapToType(studentWaiver);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting incoming Waiver Dto to Waiver Entity: " + ex.Message);
                throw;
            }

            // Validate incoming waiver reason code
            if (!string.IsNullOrEmpty(waiverToAdd.ReasonCode))
            {
                try
                {
                    if (!((await referenceDataRepository.GetStudentWaiverReasonsAsync()).Select(wr => wr.Code).Contains(waiverToAdd.ReasonCode)))
                    {
                        throw new ColleagueWebApiException("Waiver Reason is not valid.");
                    }
                }
                catch (ColleagueSessionExpiredException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred during validation of Waiver Reason Code (" + waiverToAdd.ReasonCode + ") specified in waiver. Message: " + ex.Message);
                    throw;
                }
            }

            // Validate the requisite IDs
            try
            {
                Domain.Student.Entities.Section waiverSection = await GetSectionAsync(studentWaiver.SectionId);
                Domain.Student.Entities.Course course = await courseRepository.GetAsync(waiverSection.CourseId);
                waiverToAdd.ValidateRequisiteWaivers(waiverSection, course);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred during validation of Requisites for section ID (" + waiverToAdd.SectionId + ") specified in waiver. Message: " + ex.Message);
                throw;
            }

            // Now that we have retrieved it, see if waiver can be added by person requesting.
            // Get the specified section from the repository
            Domain.Student.Entities.Section section = await GetSectionAsync(studentWaiver.SectionId);
            var allDepartments = await baseReferenceDataRepository.DepartmentsAsync();
            // Ensure that the current user is a faculty of the given section or a departmental oversight member with the required permissions. 
            if (!(IsSectionFaculty(section) && userPermissions.Contains(StudentPermissionCodes.CreatePrerequisiteWaiver))
                && !(CheckDepartmentalOversightAccessForSection(section, allDepartments) && userPermissions.Contains(DepartmentalOversightPermissionCodes.CreateSectionPrerequisiteWaiver)))
            {
                var message = "Current user is neither a faculty nor a departmental oversight of requested section " + studentWaiver.SectionId + " with the required permissions and therefore cannot add waiver requested.";
                logger.Info(message);
                throw new PermissionsException(message);
            }
            var newWaiver = await studentWaiverRepository.CreateSectionWaiverAsync(waiverToAdd);

            var waiverDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentWaiver, Dtos.Student.StudentWaiver>();
            return waiverDtoAdapter.MapToType(newWaiver);
        }

        /// <summary>
        /// Retrieve the specified waiver
        /// </summary>
        /// <param name="studentWaiverId">Id of the waiver to retrieve</param>
        /// <returns><see cref="Waiver">Waiver dto object</see></returns>
        public async Task<Dtos.Student.StudentWaiver> GetStudentWaiverAsync(string studentWaiverId)
        {
            // Throw argument null exception if waiver Id not provided
            if (string.IsNullOrEmpty(studentWaiverId))
            {
                var message = "StudentWaiver Id must be provided";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }

            // Get the waiver from the repository. 
            // Log and rethrow any general exception that occurs in the repository or during dto conversion.
            Domain.Student.Entities.StudentWaiver studentWaiver = null;
            try
            {
                studentWaiver = await studentWaiverRepository.GetAsync(studentWaiverId);
            }
            catch (KeyNotFoundException)
            {
                logger.Error("StudentWaiver not found in repository for given waiver Id " + studentWaiverId);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to read waiver from repository using student waiver id " + studentWaiverId + " Exception message: " + ex.Message;
                logger.Error(message);
                throw new ColleagueWebApiException(message);
            }

            // Faculty can see only waivers for their own section
            Domain.Student.Entities.Section section;
            try
            {
                section = await GetSectionAsync(studentWaiver.SectionId);
            }
            catch
            {
                var message = "Section Id " + studentWaiver.SectionId + " specified in student waiver could not be verified.";
                logger.Error(message);
                throw new ColleagueWebApiException(message);
            }
            if (!IsSectionFaculty(section))
            {
                var message = "Current user is not a faculty of requested section " + studentWaiver.SectionId + " and therefore cannot access student waivers";
                logger.Error(message);
                throw new PermissionsException(message);
            }

            // Get the right adapter for the type mapping
            var waiverDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentWaiver, Dtos.Student.StudentWaiver>();
            // Map the degree plan entity to the degree plan DTO
            return waiverDtoAdapter.MapToType(studentWaiver);
        }

        /// <summary>
        /// Attempts to get a section from the repository.
        /// </summary>
        /// <param name="sectionId">Id of section to retrieve</param>
        /// <returns><see cref="Section">Section</see> domain object</returns>
        private async Task<Domain.Student.Entities.Section> GetSectionAsync(string sectionId)
        {
            Domain.Student.Entities.Section section;

            try
            {
                section = await sectionRepository.GetSectionAsync(sectionId, useSeatServiceWhenEnabled: true);
                if (section == null)
                {
                    var message = "Repository returned a null section for Id " + sectionId;
                    logger.Error(message);
                    throw new KeyNotFoundException(message);
                }
                return section;
            }
            catch (ArgumentNullException aex)
            {
                var message = "Section ID must be specified.";
                logger.Error(message);
                throw aex;
            }
            catch (KeyNotFoundException kex)
            {
                var message = "sectionId " + sectionId + " not found in repository. Exception message: " + kex.Message;
                logger.Error(message);
                throw kex;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to read section from repository using id " + sectionId + "Exception message: " + ex.ToString();
                logger.Error(message);
                throw new ColleagueWebApiException(ex.Message);
            }
        }

        /// <summary>
        /// Returns boolean indicating if the current user is a faculty for the given section.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        private bool IsSectionFaculty(Domain.Student.Entities.Section section)
        {
            if (section == null || section.FacultyIds == null || section.FacultyIds.Count() == 0)
            {
                return false;
            }
            return section.FacultyIds.Where(f => f == CurrentUser.PersonId).FirstOrDefault() == null ? false : true;
        }

        /// <summary>
        /// Get the student waivers granted for this student Id.
        /// </summary>
        /// <param name="studentId">Id of the student for which to retrieve waivers</param>
        /// <returns>List of <see cref="Dtos.Student.StudentWaiver">Waiver</see> dto objects</returns>
        public async Task<IEnumerable<Dtos.Student.StudentWaiver>> GetStudentWaiversAsync(string studentId)
        {
            // Throw argument null exception if student Id not provided
            if (string.IsNullOrEmpty(studentId))
            {
                var message = "Student Id must be provided";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }
            if (!CurrentUser.IsPerson(studentId) && !(await UserIsAdvisorAsync(studentId)))
            {
                var message = "Either the current user is not the student of requested waivers or current user is not an advisor with appropriate permissions and therefore cannot access it.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            List<Dtos.Student.StudentWaiver> waivers = new List<Dtos.Student.StudentWaiver>();
            try
            {
                var waiverEntities = await studentWaiverRepository.GetStudentWaiversAsync(studentId);
                if (waiverEntities != null)
                {
                    var waiverDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentWaiver, Dtos.Student.StudentWaiver>();
                    foreach (var waiver in waiverEntities)
                    {
                        waivers.Add(waiverDtoAdapter.MapToType(waiver));
                    }
                }
                return waivers;
            }
            catch (Ellucian.Data.Colleague.Exceptions.ColleagueSessionExpiredException)
            {
                throw;
            }

            catch (Exception ex)
            {
                var message = "Exception occurred while trying to read waivers from repository using student id " + studentId + "Exception message: " + ex.Message;
                logger.Error(message);
                throw;
            }
        }
    }
}
