//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
            CheckViewPermission();

            var sectionInstructorsCollection = new List<SectionInstructors>();

            string newSection = string.Empty;
            string newInstructor = string.Empty;
            List<string> newInstructionalEvents = new List<string>();
            try
            {
                newSection = (section == string.Empty? string.Empty : await _sectionRepository.GetSectionIdFromGuidAsync(section));
                if (!string.IsNullOrEmpty(section) && string.IsNullOrEmpty(newSection))
                {
                    // throw new ArgumentException("Invalid section Id argument");
                    return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
                }
            }
            catch
            {
                // throw new ArgumentException("Invalid section Id argument");
                return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
            }
            try
            {
                newInstructor = (instructor == string.Empty ? string.Empty : await _personRepository.GetPersonIdFromGuidAsync(instructor));
                if (!string.IsNullOrEmpty(instructor) && string.IsNullOrEmpty(newInstructor))
                {
                    // throw new ArgumentException("Invalid instructor Id argument");
                    return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
                }
            }
            catch
            {
                // throw new ArgumentException("Invalid instructor Id argument");
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
                    // throw new ArgumentException("Invalid instructional event Id argument");
                    return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
                }
            }
            catch
            {
                // throw new ArgumentException("Invalid instructional event Id argument");
                return new Tuple<IEnumerable<SectionInstructors>, int>(new List<SectionInstructors>(), 0);
            }

            var pageOfItems = await _sectionRepository.GetSectionFacultyAsync(offset, limit, newSection, newInstructor, newInstructionalEvents);
            var entities = pageOfItems.Item1;

            foreach (var entity in entities)
            {
                var sectionDto = await ConvertSectionInstructorsEntityToDtoAsync(entity, bypassCache);
                sectionInstructorsCollection.Add(sectionDto);
            }
            return new Tuple<IEnumerable<SectionInstructors>, int>(sectionInstructorsCollection,pageOfItems.Item2);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a SectionInstructors from its GUID
        /// </summary>
        /// <returns>SectionInstructors DTO object</returns>
        public async Task<SectionInstructors> GetSectionInstructorsByGuidAsync(string guid)
        {
            CheckViewPermission();

            try
            {
                var sectionFaculty = await _sectionRepository.GetSectionFacultyByGuidAsync(guid);
                return await ConvertSectionInstructorsEntityToDtoAsync(sectionFaculty);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("section-instructors not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("section-instructors not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Create a section instructors
        /// </summary>
        /// <param name="sectionInstructors">The event to create</param>
        /// <returns>SectionInstructors DTO</returns>
        public async Task<SectionInstructors> CreateSectionInstructorsAsync(SectionInstructors sectionInstructors)
        {
            CheckCreatePermission();
            ValidateSectionInstructorsDto(sectionInstructors);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var sectionFaculty = await ConvertSectionInstructorsDtoToEntityAsync(sectionInstructors);
            var updatedEntity = await _sectionRepository.PostSectionFacultyAsync(sectionFaculty, sectionInstructors.Id);
            var dto = await ConvertSectionInstructorsEntityToDtoAsync(updatedEntity);

            return dto;
        }

        /// <summary>
        /// Update a section instructors
        /// </summary>
        /// <param name="sectionInstructors">The event to update</param>
        /// <returns>SectionInstructors DTO</returns>
        public async Task<SectionInstructors> UpdateSectionInstructorsAsync(string guid, SectionInstructors sectionInstructors)
        {
            CheckCreatePermission();
            ValidateSectionInstructorsDto(sectionInstructors);

            _sectionRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            var sectionFaculty = await ConvertSectionInstructorsDtoToEntityAsync(sectionInstructors);
            var updatedEntity = await _sectionRepository.PutSectionFacultyAsync(sectionFaculty, sectionInstructors.Id);
            var dto = await ConvertSectionInstructorsEntityToDtoAsync(updatedEntity);

            return dto;
        }

        /// <summary>
        /// Delete a section faculty
        /// </summary>
        /// <param name="guid">The GUID of the section faculty</param>
        public async Task DeleteSectionInstructorsAsync(string guid)
        {
            CheckDeletePermission();
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            SectionFaculty sectionInstructors = null;

            try
            {
                sectionInstructors = await _sectionRepository.GetSectionFacultyByGuidAsync(guid);
            }
            catch (Exception e)
            {
                throw new KeyNotFoundException("No section-instructors found for guid '" + guid + "'.");
            }
            if (sectionInstructors == null)
            {
                throw new KeyNotFoundException("Invalid Guid for section-instructors");
            }

            await _sectionRepository.DeleteSectionFacultyAsync(sectionInstructors, sectionInstructors.Id);
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
                throw new ArgumentNullException("source", "SectionFaculty Entity is a required parameter. ");
            }
            sectionInstructors.Id = source.Guid;
            sectionInstructors.Section = new GuidObject2(await _sectionRepository.GetSectionGuidFromIdAsync(source.SectionId));
            sectionInstructors.Instructor = new GuidObject2(await _personRepository.GetPersonGuidFromIdAsync(source.FacultyId));
            if (source.SecMeetingIds != null && source.SecMeetingIds.Any())
            {
                sectionInstructors.InstructionalEvents = new List<GuidObject2>();
                foreach (var meeting in source.SecMeetingIds)
                {
                    var secMeetingGuid = await _sectionRepository.GetSectionMeetingGuidFromIdAsync(meeting);
                    if (!string.IsNullOrEmpty(secMeetingGuid))
                    {
                        sectionInstructors.InstructionalEvents.Add(new GuidObject2(secMeetingGuid));
                    }
                }
            }
            if (source.PrimaryIndicator)
            {
                sectionInstructors.InstructorRole = SectionInstructorsInstructorRole.Primary;
            }
            if (!string.IsNullOrEmpty(source.InstructionalMethodCode))
            {
                var instructionalMethodEntity = (await _referenceDataRepository.GetInstructionalMethodsAsync(bypassCache)).FirstOrDefault(im => im.Code == source.InstructionalMethodCode);
                if (instructionalMethodEntity != null)
                {
                    sectionInstructors.InstructionalMethod = new GuidObject2(instructionalMethodEntity.Guid);
                }
            }
            sectionInstructors.ResponsibilityPercentage = source.ResponsibilityPercentage;
            sectionInstructors.WorkLoad = source.LoadFactor.Value;
            sectionInstructors.WorkStartOn = source.StartDate;
            sectionInstructors.WorkEndOn = source.EndDate;
                                                                                                                                               
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
                throw new ArgumentNullException("source", "The section instructors is a required parameter for updates/creates. ");
            }
            var allInstructionalMethods = await _referenceDataRepository.GetInstructionalMethodsAsync(false);
            if (allInstructionalMethods == null)
            {
                var repoError = new RepositoryException("sectionInstructors");
                repoError.AddError(new Domain.Entities.RepositoryError("Instructional Methods call returns no values.Required for updates / creates. "));
                throw repoError;
            }
            if (source.InstructionalMethod == null || string.IsNullOrEmpty(source.InstructionalMethod.Id))
            {
                if (source.InstructionalEvents != null && source.InstructionalEvents.Any())
                {
                    foreach (var meetingGuid in source.InstructionalEvents)
                    {
                        var meeting = await _sectionRepository.GetSectionMeetingByGuidAsync(meetingGuid.Id);
                        var instrMethod = allInstructionalMethods.FirstOrDefault(im => im.Code == meeting.InstructionalMethodCode);
                        source.InstructionalMethod = new GuidObject2(instrMethod.Guid);
                    }
                }
                else
                {
                    throw new ArgumentNullException("source.instructionalMethod.id", "The instructional Method is required for updates/creates. ");
                }
            }

            var instructionalMethodEntity = allInstructionalMethods.FirstOrDefault(im => im.Guid == source.InstructionalMethod.Id);
            var sectionId = await _sectionRepository.GetSectionIdFromGuidAsync(source.Section.Id);
            var instructorId = await _personRepository.GetPersonIdFromGuidAsync(source.Instructor.Id);
            if (string.IsNullOrEmpty(instructorId))
            {
                throw new ArgumentException("Instructor not found for guid :'" + source.Instructor.Id + "'.");
            }
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentException("Section not found for guid :'" + source.Section.Id + "'.");
            }
            var instructionalMethodCode = instructionalMethodEntity.Code;
            var sectionFacultyId = await _sectionRepository.GetSectionFacultyIdFromGuidAsync(source.Id);
            if (string.IsNullOrEmpty(sectionFacultyId)) sectionFacultyId = "$NEW";

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

            return sectionFacultyEntity;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view section-instructors
        /// </summary>
        /// <permission cref="StudentPermissionCodes.ViewSectionInstructors"></permission>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewPermission()
        {
            bool hasViewPermission = HasPermission(StudentPermissionCodes.ViewSectionInstructors);

            // User is not allowed to view section-instructors without the appropriate permissions
            if (!hasViewPermission)
            {
                var message = "User " + CurrentUser.UserId + " does not have permission to create or view section-instructors.";
                logger.Error(message);
                throw new PermissionsException(message);
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
                throw new PermissionsException(message);
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
                throw new ArgumentNullException("sectionInstructors", "SectionInstructors body is required in PUT or POST request. ");
            }
            if (sectionInstructors.InstructionalMethod == null && sectionInstructors.InstructionalEvents == null)
            {
                throw new ArgumentNullException("Either an instructional event or an instructional method must be specified. ");
            }
            if (sectionInstructors.Section == null || string.IsNullOrEmpty(sectionInstructors.Section.Id))
            {
                throw new ArgumentNullException("sectionInstructors.section.id", "The section id is a required parameter for updates/creates. ");
            }
            if (sectionInstructors.Instructor == null || string.IsNullOrEmpty(sectionInstructors.Instructor.Id))
            {
                throw new ArgumentNullException("sectionInstructors.instructor.id", "The instructor id is a required parameter for updates/creates. ");
            }
        }
    }
}
