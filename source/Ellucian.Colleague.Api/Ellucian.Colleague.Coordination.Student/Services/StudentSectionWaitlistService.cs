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
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentSectionWaitlistsService : BaseCoordinationService, IStudentSectionWaitlistsService
    {

        private readonly ISectionRepository _sectionRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public StudentSectionWaitlistsService(
            ISectionRepository sectionRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _sectionRepository = sectionRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all student-section-waitlists
        /// </summary>
        /// <returns>Collection of StudentSectionWaitlists DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.StudentSectionWaitlist>, int>> GetStudentSectionWaitlistsAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckStudentSectionWaitlistPermission();
            var studentSectionWaitlistsCollection = new List<Ellucian.Colleague.Dtos.StudentSectionWaitlist>();
            var returnedEntitiesTuple = await _sectionRepository.GetWaitlistsAsync(offset, limit);

            if (returnedEntitiesTuple.Item1 != null)
            {

                foreach (var wl in returnedEntitiesTuple.Item1)
                {
                    studentSectionWaitlistsCollection.Add(ConvertStudentSectionWaitlistsEntityToDto(wl));
                }

            }
            return new Tuple<IEnumerable<Dtos.StudentSectionWaitlist>, int>(studentSectionWaitlistsCollection, returnedEntitiesTuple.Item2);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// 
        /// <summary>
        /// Get a StudentSectionWaitlist from its GUID
        /// </summary>
        /// <returns>StudentSectionWaitlist DTO object</returns>
        public async Task<Dtos.StudentSectionWaitlist> GetStudentSectionWaitlistsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentException("Guid argument is required");
            }
            CheckStudentSectionWaitlistPermission();
            try
            {
                var waitEntity = await _sectionRepository.GetWaitlistFromGuidAsync(guid);
                if (waitEntity == null)
                {
                    throw new Exception();

                }
                return ConvertStudentSectionWaitlistsEntityToDto(waitEntity);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("student-section-waitlists not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("student-section-waitlists not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Helper method to convert waitlist entities to dto
        /// </summary>
        private Ellucian.Colleague.Dtos.StudentSectionWaitlist ConvertStudentSectionWaitlistsEntityToDto(Ellucian.Colleague.Domain.Student.Entities.StudentSectionWaitlist source)
        {
            var waitListDto = new Ellucian.Colleague.Dtos.StudentSectionWaitlist();

            waitListDto.Person = new Dtos.DtoProperties.StudentSectionWaitlistsPersonDtoProperty() { personId = source.PersonId };
            waitListDto.Section = new Dtos.DtoProperties.StudentSectionWaitlistsSectionDtoProperty() { sectionId = source.SectionId };
            waitListDto.Priority = source.Priority;
            waitListDto.Id = source.Guid;
            return waitListDto;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view student-section-waitlists
        /// </summary>
        /// <permission cref="StudentPermissionCodes.ViewStudentSectionWaitlist"></permission>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckStudentSectionWaitlistPermission()
        {
            bool hasSectionPermission = HasPermission(StudentPermissionCodes.ViewStudentSectionWaitlist);

            // User is not allowed to create or update sections without the appropriate permissions
            if (!hasSectionPermission)
            {
                var message = "User " + CurrentUser.UserId + " does not have permission to view waitlists.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }
    }
}