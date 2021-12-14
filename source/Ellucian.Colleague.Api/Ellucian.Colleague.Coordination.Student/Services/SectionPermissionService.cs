// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class SectionPermissionService : BaseCoordinationService, ISectionPermissionService
    {
        private readonly ISectionPermissionRepository _sectionPermissionRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        /// <summary>
        /// Initialize the service for accessing section permissions
        /// </summary>
        /// <param name="adapterRegistry">Dto adapter registry</param>
        /// <param name="sectionPermissionRepository">Section permissions for faculty consent and student petitions</param>
        /// <param name="logger">error logging</param>
        public SectionPermissionService(IAdapterRegistry adapterRegistry, ISectionPermissionRepository sectionPermissionRepository, IStudentRepository studentRepository, ISectionRepository sectionRepository, IStudentReferenceDataRepository referenceDataRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this._sectionPermissionRepository = sectionPermissionRepository;
            this._studentRepository = studentRepository;
            this._sectionRepository = sectionRepository;
            this._referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Asynchronous method to get a section permission object for a section.
        /// </summary>
        /// <param name="sectionId">Section Id requested</param>
        /// <returns>A <see cref="Dtos.Student.StudentPetition"></see> object.</returns>
        public async Task<Dtos.Student.SectionPermission> GetAsync(string sectionId)
        {
            Dtos.Student.SectionPermission sectionPermissionDto = new Dtos.Student.SectionPermission();
            sectionPermissionDto.SectionId = sectionId;
            // Get the specified section from the repository
            Domain.Student.Entities.Section section = await GetSectionAsync(sectionId);

            // Ensure that the current user is a faculty of the given section. 
            if (!IsSectionFaculty(section))
            {
                var message = "Current user is not a faculty of requested section " + sectionId + " and therefore cannot access waivers";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.SectionPermission sectionPermEntity = await _sectionPermissionRepository.GetSectionPermissionAsync(sectionId);

                if (sectionPermEntity != null && ((sectionPermEntity.StudentPetitions != null && sectionPermEntity.StudentPetitions.Count() > 0) || (sectionPermEntity.FacultyConsents != null && sectionPermEntity.FacultyConsents.Count() > 0)))
                {
                    //mapping
                    var sectionPermissionDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.SectionPermission, Ellucian.Colleague.Dtos.Student.SectionPermission>();
                    sectionPermissionDto = sectionPermissionDtoAdapter.MapToType(sectionPermEntity);
                }

            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to read student petitions from repository using section id " + sectionId + "Exception message: " + ex.Message;
                logger.Error(message);
                throw new Exception(message);
            }
            return sectionPermissionDto;
        }

        /// <summary>
        /// Validates incoming Petition and calls repository to add Petition to the database, returning the newly created Petition.
        /// </summary>
        /// <param name="studentPetition">Student Petition to add to the database</param>
        /// <returns>The newly created <see cref="Dtos.Student.StudentPetition">Student Petition</see></returns>
        public async Task<Dtos.Student.StudentPetition> AddStudentPetitionAsync(Dtos.Student.StudentPetition studentPetitionToAdd)
        {

            // Throw exception if incoming Student Petition is null
            if (studentPetitionToAdd == null)
            {
                throw new ArgumentNullException("studentPetition", "StudentPetition object must be provided.");
            }

            // Throw exception if user does not have correct permissions for the item being added
            // NOTE: It was decided to only check a person's permissions and not also verify that this is a section the person is teaching.
            if (studentPetitionToAdd.Type == Dtos.Student.StudentPetitionType.StudentPetition && !(await GetUserPermissionCodesAsync()).Contains(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentPetition))
            {
                var message = "User does not have permissions required to create a Student Petition.";
                logger.Error(message);
                throw new PermissionsException(message);
            }

            if (studentPetitionToAdd.Type == Dtos.Student.StudentPetitionType.FacultyConsent && !(await GetUserPermissionCodesAsync()).Contains(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateFacultyConsent))
            {
                var message = "User does not have permissions required to create a Faculty Consent.";
                logger.Error(message);
                throw new PermissionsException(message);
            }

            if (string.IsNullOrEmpty(studentPetitionToAdd.SectionId))
            {
                logger.Error("Unable to add new student petition or faculty conset because no section ID was provided.");
                throw new ArgumentException("Section ID is required when adding a new student permission or faculty consent.");
            }

            if (string.IsNullOrEmpty(studentPetitionToAdd.StudentId))
            {
                logger.Error("Unable to add new student petition or faculty conset because no section ID was provided.");
                throw new ArgumentException("Student ID is required when adding a new student permission or faculty consent.");
            }

            // Validate incoming Petition status code - This is required for an add
            if (!string.IsNullOrEmpty(studentPetitionToAdd.StatusCode))
            {
                try
                {
                    if (!((await _referenceDataRepository.GetPetitionStatusesAsync()).Select(wr => wr.Code).Contains(studentPetitionToAdd.StatusCode)))
                    {
                        throw new Exception("Petition Status Code is not valid.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred during validation of Petition Status Code (" + studentPetitionToAdd.StatusCode + ") specified in Petition. Message: " + ex.Message);
                    throw;
                }
            }
            else
            {
                logger.Error("error occurred during validation of Petition Status Code (" + studentPetitionToAdd.StatusCode + ") specified in Petition.");
                throw new ArgumentException("A status code is required when adding a new student permission or faculty consent.");
            }

            // Validate incoming Petition reason code and make sure there is either a reason or a comment
            if (!string.IsNullOrEmpty(studentPetitionToAdd.ReasonCode))
            {
                try
                {
                    if (!((await _referenceDataRepository.GetStudentPetitionReasonsAsync()).Select(wr => wr.Code).Contains(studentPetitionToAdd.ReasonCode)))
                    {
                        throw new Exception("Petition Reason is not valid.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred during validation of Petition Reason Code (" + studentPetitionToAdd.ReasonCode + ") specified in Petition. Message: " + ex.Message);
                    throw;
                }
            }
            else
            {
                //If there is no reason code then there should be a comment or the request to add is not valid
                if (string.IsNullOrEmpty(studentPetitionToAdd.Comment))
                {
                    throw new Exception("Student Petition must have either a Reason or a comment to be valid.");
                }
            }

            // The incoming entity has been validated and the permission has been checked.
            // Convert the DTO to an entity, call the repository method, and convert it into the DTO.
            Domain.Student.Entities.StudentPetition petitionToAdd = null;
            try
            {
                var studentPetitionDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentPetition, Domain.Student.Entities.StudentPetition>();
                petitionToAdd = studentPetitionDtoToEntityAdapter.MapToType(studentPetitionToAdd);
            }
            catch (Exception ex)
            {
                logger.Error("Error converting incoming StudentPetitionToAdd Dto to StudentPetition Entity: " + ex.Message);
                throw new ArgumentException("Student Petition to add is invalide", ex);
            }
            var studentPetitionAdded = await _sectionPermissionRepository.AddStudentPetitionAsync(petitionToAdd);
            var studentPetitionEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentPetition, Dtos.Student.StudentPetition>();
            var studentPetitionDto = studentPetitionEntityToDtoAdapter.MapToType(studentPetitionAdded);
            return studentPetitionDto;
        }

        /// <summary>
        /// Validates student petition and updates it in the database, returning the student petition id updated
        /// </summary>
        /// <param name="studentPetition">Student petition object to update</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentPetition">StudentPetition</see> object that was updated</returns>
        public async Task<Dtos.Student.StudentPetition> UpdateStudentPetitionAsync(Dtos.Student.StudentPetition studentPetitionToUpdate)
        {
            // Throw exception if incoming Student Petition is null
            if (studentPetitionToUpdate == null)
            {
                throw new ArgumentNullException("studentPetition", "StudentPetition object must be provided.");
            }

            // Throw exception if user does not have correct permissions for the item being updated
            // NOTE: It was decided to only check a person's permissions and not also verify that this is a section the person is teaching.
            if (studentPetitionToUpdate.Type == Dtos.Student.StudentPetitionType.StudentPetition && !(await GetUserPermissionCodesAsync()).Contains(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentPetition))
            {
                var message = "User does not have permissions required to update a Student Petition.";
                logger.Error(message);
                throw new PermissionsException(message);
            }

            if (studentPetitionToUpdate.Type == Dtos.Student.StudentPetitionType.FacultyConsent && !(await GetUserPermissionCodesAsync()).Contains(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateFacultyConsent))
            {
                var message = "User does not have permissions required to update a Faculty Consent.";
                logger.Error(message);
                throw new PermissionsException(message);
            }

            if (string.IsNullOrEmpty(studentPetitionToUpdate.SectionId))
            {
                logger.Error("Unable to update student petition or faculty consent because no section ID was provided.");
                throw new ArgumentException("Section ID is required when updating a student petition or faculty consent.");
            }

            if (string.IsNullOrEmpty(studentPetitionToUpdate.StudentId))
            {
                logger.Error("Unable to update student petition or faculty consent because no StudentId ID was provided.");
                throw new ArgumentException("Student ID is required when updating a student petition or faculty consent.");
            }

            // Validate incoming Petition status code - This is required for an update
            if (!string.IsNullOrEmpty(studentPetitionToUpdate.StatusCode))
            {
                try
                {
                    if (!((await _referenceDataRepository.GetPetitionStatusesAsync()).Select(wr => wr.Code).Contains(studentPetitionToUpdate.StatusCode)))
                    {
                        throw new Exception("Petition Status Code is not valid.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "error occurred during validation of Petition Status Code (" + studentPetitionToUpdate.StatusCode + ") specified in Petition. Message: " + ex.Message);
                    throw ex;
                }
            }
            else
            {
                logger.Error("error occurred during validation of Petition Status Code (" + studentPetitionToUpdate.StatusCode + ") specified in Petition.");
                throw new ArgumentException("A status code is required when updating a student petition or faculty consent.");
            }

            // Validate incoming Petition reason code and make sure there is either a reason or a comment
            if (!string.IsNullOrEmpty(studentPetitionToUpdate.ReasonCode))
            {
                try
                {
                    if (!((await _referenceDataRepository.GetStudentPetitionReasonsAsync()).Select(wr => wr.Code).Contains(studentPetitionToUpdate.ReasonCode)))
                    {
                        throw new Exception("Petition Reason is not valid.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "error occurred during validation of Petition Reason Code (" + studentPetitionToUpdate.ReasonCode + ") specified in Petition. Message: " + ex.Message);
                    throw ex;
                }
            }
            else
            {
                //If there is no reason code then there should be a comment or the request to update is not valid
                if (string.IsNullOrEmpty(studentPetitionToUpdate.Comment))
                {
                    throw new Exception("Student Petition must have either a Reason or a comment to be valid.");
                }
            }

            // The incoming entity has been validated and the permission has been checked.
            // Convert the DTO to an entity, call the repository method, and convert it into the DTO.
            Domain.Student.Entities.StudentPetition petitionToUpdate = null;
            try
            {
                var studentPetitionDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.StudentPetition, Domain.Student.Entities.StudentPetition>();
                petitionToUpdate = studentPetitionDtoToEntityAdapter.MapToType(studentPetitionToUpdate);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error converting incoming StudentPetitionToAdd Dto to StudentPetition Entity: " + ex.Message);
                throw new ArgumentException("Student Petition to update is invalide", ex);
            }
            var studentPetitionUpated = await _sectionPermissionRepository.UpdateStudentPetitionAsync(petitionToUpdate);
            var studentPetitionEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentPetition, Dtos.Student.StudentPetition>();
            var studentPetitionDto = studentPetitionEntityToDtoAdapter.MapToType(studentPetitionUpated);
            return studentPetitionDto;
        }

        /// <summary>
        /// Gets a new student petition 
        /// </summary>
        /// <param name="type">Type of student petition to return</param>
        /// <param name="studentPetitionId">Id of requested StudentPetition</param>
        /// <returns><see cref="Ellucian.Colleague.Dtos.Student.StudentPetition">StudentPetition</see> object that was created</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.StudentPetition> GetStudentPetitionAsync(string studentPetitionId, string sectionId, StudentPetitionType type)
        {
            // Throw exception if any of the parameters are null
            if (string.IsNullOrEmpty(studentPetitionId))
            {
                throw new ArgumentNullException("studentPetitionId", "StudentPetitionId must be provided.");
            }

            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "sectionId must be provided.");
            }
            Domain.Student.Entities.StudentPetitionType entityType = Domain.Student.Entities.StudentPetitionType.StudentPetition;
            switch (type)
            {
                case StudentPetitionType.StudentPetition:
                    entityType = Domain.Student.Entities.StudentPetitionType.StudentPetition;
                    break;
                case StudentPetitionType.FacultyConsent:
                    entityType = Domain.Student.Entities.StudentPetitionType.FacultyConsent;
                    break;
            }
            // Log and rethrow any general exception that occurs in the repository or during dto conversion.
            Domain.Student.Entities.StudentPetition studentPetition = null;
            try
            {
                studentPetition =await  _sectionPermissionRepository.GetAsync(studentPetitionId, sectionId, entityType);
            }
            catch (KeyNotFoundException)
            {
                logger.Error("StudentPetition not found in repository for petition Id " + studentPetitionId + " and type " + type.ToString() + " and section Id " + sectionId);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred getting student petition from repository using student petition id " + studentPetitionId + " and type of " + type.ToString() + " Exception message: " + ex.Message;
                logger.Error(message);
                throw new Exception(message);
            }


            // Now that we have retrieved it, see if it is a petition viewable by person requesting.
            // Faculty can see only petitions for their own sections
            Domain.Student.Entities.Section section;
            try
            {
                section =  await GetSectionAsync(studentPetition.SectionId);
            }
            catch
            {
                var message = "Section Id " + studentPetition.SectionId + " specified in student petition could not be verified.";
                logger.Error(message);
                throw new Exception(message);
            }
            if (!IsSectionFaculty(section))
            {
                var message = "Current user is not a faculty of requested section " + studentPetition.SectionId + " and therefore cannot access student petition requested.";
                logger.Info(message);
                throw new PermissionsException(message);
            }

            var studentPetitionEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentPetition, Dtos.Student.StudentPetition>();
            var studentPetitionDto = studentPetitionEntityToDtoAdapter.MapToType(studentPetition);
           
            return studentPetitionDto;
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
                section = await _sectionRepository.GetSectionAsync(sectionId);
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
                throw new Exception(ex.Message);
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
    }
}
