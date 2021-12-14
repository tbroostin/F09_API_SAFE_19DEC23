//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class SectionInstructorsService : BaseCoordinationService, ISectionInstructorsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public SectionInstructorsService(

            IStudentReferenceDataRepository referenceDataRepository,
            ISectionRepository sectionRepository,
            IPersonRepository personRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
            _sectionRepository = sectionRepository;
            _personRepository = personRepository;
            _configurationRepository = configurationRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all section-instructors
        /// </summary>
        /// <returns>Collection of SectionInstructors DTO objects</returns>
        public async Task<Tuple<IEnumerable<SectionInstructors>, int>> GetSectionInstructorsAsync(int offset, int limit, string section, string instructor, List<string> instructionalEvents, bool bypassCache = false)
        {
            //CheckViewPermission();

            var sectionInstructorsCollection = new List<SectionInstructors>();

            string newSection = string.Empty;
            string newInstructor = string.Empty;
            List<string> newInstructionalEvents = new List<string>();
            try
            {
                newSection = (section == string.Empty? string.Empty : await _sectionRepository.GetSectionIdFromGuidAsync(section));
                if (!string.IsNullOrEmpty(section) && string.IsNullOrEmpty(newSection))
                {
                    return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
                }
            }
            catch
            {
                return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
            }
            try
            {
                newInstructor = (instructor == string.Empty ? string.Empty : await _personRepository.GetPersonIdFromGuidAsync(instructor));
                if (!string.IsNullOrEmpty(instructor) && string.IsNullOrEmpty(newInstructor))
                {
                    return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
                }
            }
            catch
            {
                return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
            }
            try
            {
                foreach (var instructionalEvent in instructionalEvents)
                {
                    var instrEvent = (instructionalEvent == string.Empty ? string.Empty : await _sectionRepository.GetSectionMeetingIdFromGuidAsync(instructionalEvent));
                    if (!string.IsNullOrEmpty(instrEvent))
                        newInstructionalEvents.Add(instrEvent);
                }
                if (instructionalEvents.Count() != newInstructionalEvents.Count())
                {
                    return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
                }
            }
            catch
            {
                return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
            }

            Tuple<IEnumerable<SectionFaculty>, int> sectionInstructors = null;
            var sectionInstructorsDto = new List<Dtos.SectionInstructors>();
            try
            {
                sectionInstructors = await _sectionRepository.GetSectionFacultyAsync(offset, limit, newSection, newInstructor, newInstructionalEvents);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            var entities = sectionInstructors.Item1;

            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    var sectionDto = await ConvertSectionInstructorsEntityToDtoAsync(entity, bypassCache);
                    sectionInstructorsCollection.Add(sectionDto);
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return new Tuple<IEnumerable<SectionInstructors>, int>(sectionInstructorsCollection, sectionInstructors.Item2);
            }
            else
            {
                return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a SectionInstructors from its GUID
        /// </summary>
        /// <returns>SectionInstructors DTO object</returns>
        public async Task<SectionInstructors> GetSectionInstructorsByGuidAsync(string guid)
        {
            //CheckViewPermission();

            try
            {
                var sectionFaculty = await _sectionRepository.GetSectionFacultyByGuidAsync(guid);
                var sectionDto = await ConvertSectionInstructorsEntityToDtoAsync(sectionFaculty);
                if (IntegrationApiException == null)
                {
                    return sectionDto;
                }
                {
                    throw IntegrationApiException;
                }
            }
            catch(RepositoryException e)
            {
                IntegrationApiExceptionAddError(e.Message, guid: guid);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException ex)
            {
                IntegrationApiExceptionAddError(ex.Message, guid: guid);
                throw IntegrationApiException;
            }
            catch (InvalidOperationException ex)
            {
                IntegrationApiExceptionAddError(ex.Message, guid: guid);
                throw IntegrationApiException;
            }
            catch (ArgumentNullException e)
            {
                IntegrationApiExceptionAddError(e.Message, guid: guid);
                throw IntegrationApiException;
            }
            catch (ArgumentOutOfRangeException e)
            {
                IntegrationApiExceptionAddError(e.Message, guid: guid);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Create a section instructors
        /// </summary>
        /// <param name="sectionInstructors">The event to create</param>
        /// <returns>SectionInstructors DTO</returns>
        public async Task<SectionInstructors> CreateSectionInstructorsAsync(SectionInstructors sectionInstructors)
        {
            //CheckCreatePermission();
            ValidateSectionInstructorsDto(sectionInstructors);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var sectionFaculty = await ConvertSectionInstructorsDtoToEntityAsync(sectionInstructors);
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            var updatedEntity = await _sectionRepository.PostSectionFacultyAsync(sectionFaculty, sectionInstructors.Id);
            var dto = await ConvertSectionInstructorsEntityToDtoAsync(updatedEntity);
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return dto;

        }

        /// <summary>
        /// Update a section instructors
        /// </summary>
        /// <param name="sectionInstructors">The event to update</param>
        /// <returns>SectionInstructors DTO</returns>
        public async Task<SectionInstructors> UpdateSectionInstructorsAsync(string guid, SectionInstructors sectionInstructors)
        {
            //CheckCreatePermission();
            ValidateSectionInstructorsDto(sectionInstructors);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var sectionFaculty = await ConvertSectionInstructorsDtoToEntityAsync(sectionInstructors);
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            else
            {
                var updatedEntity = await _sectionRepository.PutSectionFacultyAsync(sectionFaculty, sectionInstructors.Id);
                var dto = await ConvertSectionInstructorsEntityToDtoAsync(updatedEntity);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return dto;
            }
        }

        /// <summary>
        /// Delete a section instructor
        /// </summary>
        /// <param name="guid">The GUID of the section instructor</param>
        public async Task DeleteSectionInstructorsAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to delete a section-instructors.");
            }
            try
            {
                //CheckDeletePermission();

                var sectionInstructors = await _sectionRepository.GetSectionFacultyByGuidAsync(guid);
                if (sectionInstructors == null)
                {
                    throw new KeyNotFoundException();
                }

                await _sectionRepository.DeleteSectionFacultyAsync(sectionInstructors, guid);
            }
            catch (RepositoryException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new KeyNotFoundException(string.Format("Section-instructors not found for guid: '{0}'.", guid));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a SectionInstructors domain entity to its corresponding SectionInstructors DTO
        /// </summary>
        /// <param name="source">SectionInstructors domain entity</param>
        /// <returns>SectionInstructors DTO</returns>
        private async Task<SectionInstructors> ConvertSectionInstructorsEntityToDtoAsync(SectionFaculty source, bool bypassCache = false)
        {
            var sectionInstructors = new Ellucian.Colleague.Dtos.SectionInstructors();

            if (source == null || string.IsNullOrEmpty(source.Guid))
            {
                IntegrationApiExceptionAddError(string.Concat("SectionFaculty Entity is a required parameter."));
            }
            else
            {
                sectionInstructors.Id = source.Guid;

                try
                {
                    sectionInstructors.Section = new GuidObject2(await _sectionRepository.GetSectionGuidFromIdAsync(source.SectionId));
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Concat("GUID not found for section '" + source.SectionId + "'."), guid: source.Guid, id: source.Id);
                }

                try
                {
                    sectionInstructors.Instructor = new GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(source.FacultyId));
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Concat("GUID not found for faculty '" + source.FacultyId + "'."), guid: source.Guid, id: source.Id);
                }

                if (source.SecMeetingIds != null && source.SecMeetingIds.Any())
                {
                    sectionInstructors.InstructionalEvents = new List<GuidObject2>();
                    foreach (var meeting in source.SecMeetingIds)
                    {
                        try
                        {
                            var secMeetingGuid = await _sectionRepository.GetSectionMeetingGuidFromIdAsync(meeting);
                            if (!string.IsNullOrEmpty(secMeetingGuid))
                            {
                                sectionInstructors.InstructionalEvents.Add(new GuidObject2(secMeetingGuid));
                            }
                        }
                        catch
                        {
                            IntegrationApiExceptionAddError(string.Concat("GUID not found for instructional event '" + meeting + "'."), guid: source.Guid, id: source.Id);
                        }
                    }
                }
                if (source.PrimaryIndicator)
                {
                    sectionInstructors.InstructorRole = SectionInstructorsInstructorRole.Primary;
                }
                if (!string.IsNullOrEmpty(source.InstructionalMethodCode))
                {
                    Domain.Student.Entities.InstructionalMethod instructionalMethodEntity = null;
                    try
                    {
                        instructionalMethodEntity = (await _referenceDataRepository.GetInstructionalMethodsAsync(bypassCache)).FirstOrDefault(im => im.Code == source.InstructionalMethodCode);
                        if (instructionalMethodEntity != null && instructionalMethodEntity.Guid != null)
                        {
                            sectionInstructors.InstructionalMethod = new GuidObject2(instructionalMethodEntity.Guid);
                        }
                    }
                    catch (ArgumentNullException e)
                    {
                        IntegrationApiExceptionAddError(e.Message, guid: source.Guid, id: source.Id);
                    }
                }
                sectionInstructors.ResponsibilityPercentage = source.ResponsibilityPercentage;
                if (source.LoadFactor != null && source.LoadFactor.HasValue)
                {
                    sectionInstructors.WorkLoad = source.LoadFactor.Value;
                }
                sectionInstructors.WorkStartOn = source.StartDate;
                sectionInstructors.WorkEndOn = source.EndDate;
            }

            return sectionInstructors;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a SectionInstructors domain entity to its corresponding SectionInstructors DTO
        /// </summary>
        /// <param name="source">SectionInstructors domain entity</param>
        /// <returns>SectionInstructors DTO</returns>
        private async Task<SectionFaculty> ConvertSectionInstructorsDtoToEntityAsync(SectionInstructors source)
        {
            if (source == null)
            {
                IntegrationApiExceptionAddError("The section instructors is a required parameter for updates/creates.");
                return null;
            }

            string sectionId = null;
            string instructorId = null;
            try
            {
                if (source.Section != null && source.Section.Id != null)
                {
                    sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(source.Section.Id);
                }
                else
                {
                    IntegrationApiExceptionAddError("Section is a required property.", guid: source.Id);
                }
            }
            catch
            {
                IntegrationApiExceptionAddError("Section not found for guid :'" + source.Section.Id + "'.", guid: source.Id);
            }

            try
            {
                if (source.Instructor != null && source.Instructor.Id != null)
                {
                    instructorId = await _personRepository.GetPersonIdFromGuidAsync(source.Instructor.Id);
                }
                else
                {
                    IntegrationApiExceptionAddError("Instructor is a required property.", guid: source.Id);
                }
            }
            catch
            {
                IntegrationApiExceptionAddError("Instructor not found for guid :'" + source.Instructor.Id + "'.", guid: source.Id);
            }

            IEnumerable<Domain.Student.Entities.InstructionalMethod> allInstructionalMethods = null;
            string instructionalMethodCode = null;
            try
            {
                allInstructionalMethods = await _referenceDataRepository.GetInstructionalMethodsAsync(false);
            }
            catch (Exception e)
            {
                IntegrationApiExceptionAddError(e.Message, guid: source.Id);
            }

            if (allInstructionalMethods == null)
            {
                IntegrationApiExceptionAddError("Instructional Methods call returns no values.  Required for updates/creates.", guid: source.Id);
            }
            if (source.InstructionalMethod == null || string.IsNullOrEmpty(source.InstructionalMethod.Id))
            {
                if (source.InstructionalEvents != null && source.InstructionalEvents.Any())
                {
                    foreach (var meetingGuid in source.InstructionalEvents)
                    {
                        SectionMeeting meeting = null;
                        try
                        {
                            meeting = await _sectionRepository.GetSectionMeetingByGuidAsync(meetingGuid.Id);
                        }
                        catch (Exception e)
                        {
                            IntegrationApiExceptionAddError(e.Message, guid: source.Id);
                        }
                        if (meeting != null && meeting.InstructionalMethodCode != null)
                        {
                            var instrMethod = allInstructionalMethods.FirstOrDefault(im => im.Code == meeting.InstructionalMethodCode);
                            source.InstructionalMethod = new GuidObject2(instrMethod.Guid);
                        }
                    }
                }
                else
                {
                    IntegrationApiExceptionAddError("The instructional Method is required for updates/creates.", guid: source.Id);
                }
            }
            if (source.InstructionalMethod != null && allInstructionalMethods != null)
            {
                var instructionalMethodEntity = allInstructionalMethods.FirstOrDefault(im => im.Guid == source.InstructionalMethod.Id);
                if (instructionalMethodEntity != null)
                {
                    instructionalMethodCode = instructionalMethodEntity.Code;
                }
                else
                {
                    IntegrationApiExceptionAddError("Instructional method not found for GUID '" + source.InstructionalMethod.Id + "'.", guid: source.Id);
                }
            }


            var sectionFacultyId = await _sectionRepository.GetSectionFacultyIdFromGuidAsync(source.Id);
            if (string.IsNullOrEmpty(sectionFacultyId)) sectionFacultyId = "$NEW";

            try
            {
                var sectionFacultyEntity = new SectionFaculty(source.Id, sectionFacultyId, sectionId, instructorId, instructionalMethodCode,
                    source.WorkStartOn, source.WorkEndOn, source.ResponsibilityPercentage);

                if (source.InstructionalEvents != null && source.InstructionalEvents.Any())
                {
                    sectionFacultyEntity.SecMeetingIds = new List<string>();
                    foreach (var meeting in source.InstructionalEvents)
                    {
                        var meetingId = await _sectionRepository.GetSectionMeetingIdFromGuidAsync(meeting.Id);
                        if (!string.IsNullOrEmpty(meetingId))
                        {
                            sectionFacultyEntity.SecMeetingIds.Add(meetingId);
                        }
                    }
                }
                sectionFacultyEntity.PrimaryIndicator = source.InstructorRole == SectionInstructorsInstructorRole.Primary ? true : false;
                sectionFacultyEntity.LoadFactor = source.WorkLoad;

                if (sectionFacultyEntity != null)
                {
                    return sectionFacultyEntity;
                }
                else
                {
                    return null;
                }
            }
            catch (ArgumentNullException e)
            {
                IntegrationApiExceptionAddError(e.Message, guid: source.Id);
                throw IntegrationApiException;                
            }
            catch (ArgumentOutOfRangeException e)
            {
                IntegrationApiExceptionAddError(e.Message, guid: source.Id);
                throw IntegrationApiException;
            }
        } 

        /// <summary>
        /// Helper method to determine if the user has permission to view section-instructors
        /// </summary>
        /// <permission cref="StudentPermissionCodes.ViewSectionInstructors"></permission>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewPermission()
        {
            bool hasViewPermission = HasPermission(StudentPermissionCodes.ViewSectionInstructors);
            bool hasCreatePermission = HasPermission(StudentPermissionCodes.CreateSectionInstructors);

            // User is not allowed to view section-instructors without the appropriate permissions to either view or create section instructors
            if (!hasViewPermission && !hasCreatePermission)
            {
                var message = "User '" + CurrentUser.UserId + "' is not authorized to create or view section-instructors.";
                logger.Error(message);
                IntegrationApiExceptionAddError(message, "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create or update section-instructors
        /// </summary>
        /// <permission cref="StudentPermissionCodes.CreateSectionInstructors"></permission>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreatePermission()
        {
            bool hasCreatePermission = HasPermission(StudentPermissionCodes.CreateSectionInstructors);

            // User is not allowed to create or update section-instructors without the appropriate permissions
            if (!hasCreatePermission)
            {
                var message = "User " + CurrentUser.UserId + " does not have permission to create or update section-instructors.";
                logger.Error(message);
                IntegrationApiExceptionAddError(message, "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to delete section-instructors
        /// </summary>
        /// <permission cref="StudentPermissionCodes.DeleteSectionInstructors"></permission>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckDeletePermission()
        {
            bool hasCreatePermission = HasPermission(StudentPermissionCodes.DeleteSectionInstructors);

            // User is not allowed to delete section-instructors without the appropriate permissions
            if (!hasCreatePermission)
            {
                var message = "User " + CurrentUser.UserId + " does not have permission to delete section-instructors.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Helper method to validate a section-instructors DTO
        /// </summary>
        /// <param name="sectionInstructors"><see cref="SectionInstructors"/>  for validation.</param>
        private void ValidateSectionInstructorsDto(SectionInstructors sectionInstructors)
        {
            if (sectionInstructors == null)
            {
                IntegrationApiExceptionAddError("SectionInstructors body is required in PUT or POST request. ");
            }
            else
            {
                if (sectionInstructors.InstructionalMethod == null && sectionInstructors.InstructionalEvents == null)
                {
                    IntegrationApiExceptionAddError("Either an instructional event or an instructional method must be specified.", guid: sectionInstructors.Id);
                }
                if (sectionInstructors.Section == null || string.IsNullOrEmpty(sectionInstructors.Section.Id))
                {
                    IntegrationApiExceptionAddError("The section id is a required parameter for updates/creates.", guid: sectionInstructors.Id);
                }
                if (sectionInstructors.Instructor == null || string.IsNullOrEmpty(sectionInstructors.Instructor.Id))
                {
                    IntegrationApiExceptionAddError("The instructor id is a required parameter for updates/creates.", guid: sectionInstructors.Id);
                }
            }
        }
    }
}
