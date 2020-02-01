// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.QuickRegistration;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service for student Colleague Self-Service Quick Registration operations
    /// </summary>
    [RegisterType]
    public class StudentQuickRegistrationService : StudentCoordinationService, IStudentQuickRegistrationService
    {
        private readonly IStudentQuickRegistrationRepository _studentQuickRegistrationRepository;
        public StudentQuickRegistrationService(IStudentQuickRegistrationRepository studentQuickRegistrationRepository, IAdapterRegistry adapterRegistry, 
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, 
            IStudentRepository studentRepository, IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _studentQuickRegistrationRepository = studentQuickRegistrationRepository;
        }

        /// <summary>
        /// Retrieves a given student's Colleague Self-Service Quick Registration information for the provided academic term codes
        /// </summary>
        /// <param name="studentId">ID of the student for whom Colleague Self-Service Quick Registration data will be retrieved</param>
        /// <returns>A <see cref="StudentQuickRegistration"/> object</returns>
        public async Task<StudentQuickRegistration> GetStudentQuickRegistrationAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "A student ID is required when retrieving a student's quick registration data.");
            }
            Stopwatch watch = null;
            if (logger.IsDebugEnabled)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            // Users may only retrieve their own quick registration data
            if (!UserIsSelf(studentId))
            {
                throw new PermissionsException(string.Format("Current User {0} cannot retrieve Colleague Self-Service Quick Registration data for student {1}. Users may only retrieve their own Colleague Self-Service Quick Registration data.", CurrentUser.PersonId, studentId));
            }

            // Retrieve the student's quick registration data
            var studentQuickRegistrationEntity = await _studentQuickRegistrationRepository.GetStudentQuickRegistrationAsync(studentId);
            if (logger.IsDebugEnabled)
            {
                watch.Stop();
                logger.Debug("StudentQuickRegistrationRepository.GetStudentQuickRegistrationAsync for student " + studentId + " completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
                watch.Restart();
            }
            // Entity to DTO conversion
            var studentQuickRegistrationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.QuickRegistration.StudentQuickRegistration, Dtos.Student.QuickRegistration.StudentQuickRegistration>();
            var studentQuickRegistrationDto = studentQuickRegistrationDtoAdapter.MapToType(studentQuickRegistrationEntity);
            return studentQuickRegistrationDto;
        }
    }
}
