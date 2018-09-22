// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    public class RegistrationOptionsService : StudentCoordinationService, IRegistrationOptionsService
    {
        private IRegistrationOptionsRepository registrationOptionsRepository;

        public RegistrationOptionsService(IAdapterRegistry adapterRegistry, IRegistrationOptionsRepository registrationOptionsRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, IStudentRepository studentRepository,  ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository,null)
        {
            this.registrationOptionsRepository = registrationOptionsRepository;
        }

        /// <summary>
        /// Given a student ID, get the registration options for that student
        /// </summary>
        /// <param name="studentId">A student ID</param>
        /// <returns>The registration options corresponding to the supplied student IDs</returns>
        public async Task<Dtos.Student.RegistrationOptions> GetRegistrationOptionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (!UserIsSelf(studentId) && !(await UserIsAdvisorAsync(studentId)))
            {
                var message = "Current user is not the student to request registration options or current user is advisor or faculty but doesn't have appropriate permissions and therefore cannot access it.";
                logger.Info(message);
                throw new PermissionsException(message);
            }
            var registrationOptionsEntities = await registrationOptionsRepository.GetAsync(new List<string> { studentId });

            var registrationOptions = new Dtos.Student.RegistrationOptions();

            var adapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.RegistrationOptions, Dtos.Student.RegistrationOptions>();

            registrationOptions = adapter.MapToType(registrationOptionsEntities.First());

            return registrationOptions;
        }
    }
}
