// Copyright 2018 Ellucian Company L.P. and its affiliates.
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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentSectionAttendancesService : StudentCoordinationService, IStudentSectionAttendancesService
    {
        private readonly IStudentAttendanceRepository _studentAttendanceRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IConfigurationRepository _configurationRepository;

        /// <summary>
        /// Initialize the service for accessing student attendance info
        /// </summary>
        /// <param name="adapterRegistry">Dto adapter registry</param>
        /// <param name="studentAttendanceRepository">Repository for student attendance</param>
        /// <param name="logger">error logging</param>
        public StudentSectionAttendancesService(IAdapterRegistry adapterRegistry, IStudentAttendanceRepository studentAttendanceRepository, IStudentRepository studentRepository, ISectionRepository sectionRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            this._studentAttendanceRepository = studentAttendanceRepository;
            this._studentRepository = studentRepository;
            this._sectionRepository = sectionRepository;
        }
        

        /// <summary>
        /// Retrieves student section attendances Dto based on a set of criteria.  StudentId is required in criteria.
        /// This returns student attendances for the given studentId and the given sectionIds.
        /// If no sectionId is provided then attendances from all the student's sections are returned. 
        /// </summary>
        /// <param name="criteria">Object that contains the sections and studentId for which attendances are requested</param>
        /// <returns>A dto that contains list of section wise student attendances</returns>
        public async Task<StudentSectionsAttendances> QueryStudentSectionAttendancesAsync(StudentSectionAttendancesQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Query criteria must be included");
            }
            if (string.IsNullOrEmpty(criteria.StudentId))
            {
                throw new ArgumentException("Criteria must include a Student Id");
            }
            if (!UserIsSelf(criteria.StudentId))
            {
                throw new PermissionsException("Only Authenticated user can retrieve its own attendances");
            }
            try
            {
                var studentSectionAttendancesEntity = await _studentAttendanceRepository.GetStudentSectionAttendancesAsync( criteria.StudentId, criteria.SectionIds);
                var attendanceDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentSectionsAttendances, Dtos.Student.StudentSectionsAttendances>();
                 Dtos.Student.StudentSectionsAttendances attendanceDto = attendanceDtoAdapter.MapToType(studentSectionAttendancesEntity);
                return attendanceDto;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to get student attendances for student " + criteria.StudentId;
                logger.Info(ex, message);
                throw;
            }
        }
    }
}
