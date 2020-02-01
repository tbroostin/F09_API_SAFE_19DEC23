// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Base.Repositories;
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
    public class StudentPetitionService : StudentCoordinationService, IStudentPetitionService
    {
        private readonly IStudentPetitionRepository _studentPetitionRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        /// <summary>
        /// Initialize the service for accessing section permissions
        /// </summary>
        /// <param name="adapterRegistry">Dto adapter registry</param>
        /// <param name="sectionPermissionRepository">Section permissions for faculty consent and student petitions</param>
        /// <param name="logger">error logging</param>
        public StudentPetitionService(IAdapterRegistry adapterRegistry, IStudentPetitionRepository studentPetitionRepository, IStudentRepository studentRepository, ISectionRepository sectionRepository, IStudentReferenceDataRepository referenceDataRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            this._studentPetitionRepository = studentPetitionRepository;
            this._studentRepository = studentRepository;
            this._sectionRepository = sectionRepository;
        }

        /// <summary>
        /// retrieves student petitions & consent asynchronously
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>A collection of <see cref="Dtos.Student.StudentPetition"></see> object.</returns>
        public async Task<IEnumerable<Dtos.Student.StudentPetition>> GetAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                var message = "Student Id must be provided";
                logger.Info(message);
                throw new ArgumentNullException(message);
            }
            if (!UserIsSelf(studentId) && !(await UserIsAdvisorAsync(studentId)))
            {
                var message = "Current user is not the student of requested petitions and therefore cannot access it.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            List<Dtos.Student.StudentPetition> studentPetitionsDto = new List<Dtos.Student.StudentPetition>();
            try
            {
                IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentPetition> studentPetitionsEntity = await _studentPetitionRepository.GetStudentPetitionsAsync(studentId);
                if (studentPetitionsEntity != null && studentPetitionsEntity.Any())
                {
                    var studentPetitionDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>();
                    studentPetitionsDto.AddRange(studentPetitionsEntity.Select(s => studentPetitionDtoAdapter.MapToType(s)));
                }
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to read student petitions from repository using student id " + studentId;
                logger.Error(ex, message);
                throw;
            }
            return studentPetitionsDto;
        }
    }
}
