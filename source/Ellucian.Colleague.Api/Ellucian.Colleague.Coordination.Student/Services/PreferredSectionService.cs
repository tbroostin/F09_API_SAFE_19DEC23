// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// PreferredSectionService is an application that responds to a request for Preferred Section Management
    /// </summary>
    [RegisterType]
    public class PreferredSectionService : StudentCoordinationService, IPreferredSectionService 
    {
        private readonly IPreferredSectionRepository _preferredSectionRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public PreferredSectionService(IAdapterRegistry adapterRegistry, IPreferredSectionRepository preferredSectionRepository, 
            IStudentRepository studentRepository, ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository, ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _preferredSectionRepository = preferredSectionRepository;
            _studentRepository = studentRepository;
            _configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get the list of a student's preferred sections, and optional messages.
        /// </summary>
        /// <param name="studentId">A student ID</param>
        /// <returns>The student's updated preferred sections list, and any messaging generated</returns>
        public async Task<Dtos.Student.PreferredSectionsResponse> GetAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Student ID is required.");
            }

            var studentEntity =await _studentRepository.GetAsync(studentId);
            if (studentEntity == null)
            {
                throw new KeyNotFoundException("Student not found in repository.");
            }

            if (!UserIsSelf(studentId))
            {
                logger.Info(CurrentUser + " does not have permission to view these preferred sections.");
                throw new PermissionsException();
            }

            Domain.Student.Entities.PreferredSectionsResponse responseEntity =await _preferredSectionRepository.GetAsync(studentId);
            return MapPreferredSectionsResponse(responseEntity);
        }

        /// <summary>
        /// Updates the student's preferred section list with the received changes. 
        /// </summary>
        /// <param name="studentId">A student ID</param>
        /// <param name="preferredSections">A list of preferred sections to create/update</param>
        /// <returns>Error messages related to the update operation</returns>
        public async Task<IEnumerable<Dtos.Student.PreferredSectionMessage>> UpdateAsync(string studentId, IEnumerable<Dtos.Student.PreferredSection> preferredSections)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Student ID is required.");
            }

            var studentEntity = await _studentRepository.GetAsync(studentId);
            if (studentEntity == null)
            {
                throw new KeyNotFoundException("Student not found in repository.");
            }

            if (!UserIsSelf(studentId))
            {
                logger.Info(CurrentUser + " does not have permission to update these preferred sections.");
                throw new PermissionsException();
            }

            if (preferredSections == null || preferredSections.Count() <= 0)
            {
                throw new ArgumentException("One or more preferred sections are required");
            }

            List<Domain.Student.Entities.PreferredSectionMessage> updateMessages = new List<Domain.Student.Entities.PreferredSectionMessage>();
            List<Domain.Student.Entities.PreferredSection> updateSections = new List<Domain.Student.Entities.PreferredSection>();
            foreach (var preferredSection in preferredSections)
            {
                var prefSec = new Domain.Student.Entities.PreferredSection(studentId, preferredSection.SectionId, preferredSection.Credits);
                updateSections.Add(prefSec);
            }

            List<Domain.Student.Entities.PreferredSectionMessage> repoMessages = new List<Domain.Student.Entities.PreferredSectionMessage>();
            repoMessages =(await _preferredSectionRepository.UpdateAsync(studentId, updateSections)).ToList();
            if (repoMessages.Count() > 0)
            {
                updateMessages.AddRange(repoMessages);
            }
            return MapPreferredSectionsMessages(updateMessages);
        }


        /// <summary>
        /// Delete the indicated section from the student's preferred sections list.
        /// </summary>
        /// <param name="studentId">A student ID</param>
        /// <param name="sectionId">The section ID to delete</param>
        /// <returns>Error messages related to the delete operation</returns>
        public async Task<IEnumerable<Dtos.Student.PreferredSectionMessage>> DeleteAsync(string studentId, string sectionId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId, "Student ID is required.");
            }

            var studentEntity = await _studentRepository.GetAsync(studentId);
            if (studentEntity == null)
            {
                throw new KeyNotFoundException("Student not found in repository.");
            }

            if (!UserIsSelf(studentId))
            {
                logger.Info(CurrentUser + " does not have permission to delete these preferred sections.");
                throw new PermissionsException();
            }

            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentException("Section Id is required.");
            }

            List<Domain.Student.Entities.PreferredSectionMessage> responseMessages = new List<Domain.Student.Entities.PreferredSectionMessage>();

            var deleteMessages = await _preferredSectionRepository.DeleteAsync(studentId, sectionId);
            if (deleteMessages.Count() > 0)
            {
                responseMessages.AddRange(deleteMessages);
            }
            return MapPreferredSectionsMessages(responseMessages);
        }


        private Dtos.Student.PreferredSectionsResponse MapPreferredSectionsResponse(Domain.Student.Entities.PreferredSectionsResponse responseEntity)
        {
            var dtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionsResponse, Ellucian.Colleague.Dtos.Student.PreferredSectionsResponse>();
            Dtos.Student.PreferredSectionsResponse responseDto = dtoAdapter.MapToType(responseEntity);
            return responseDto;
        }

        private IEnumerable<Dtos.Student.PreferredSectionMessage> MapPreferredSectionsMessages(IEnumerable<Domain.Student.Entities.PreferredSectionMessage> messages)
        {
            List<Dtos.Student.PreferredSectionMessage> dtoList = new List<Dtos.Student.PreferredSectionMessage>();
            var dtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionMessage, Ellucian.Colleague.Dtos.Student.PreferredSectionMessage>();
            foreach (var message in messages)
            {
                Dtos.Student.PreferredSectionMessage msgDto = dtoAdapter.MapToType(message);
                dtoList.Add(msgDto);
            }
            return dtoList;
        }

    }
}
