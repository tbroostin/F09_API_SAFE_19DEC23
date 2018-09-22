//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Student NSLDS Information service class
    /// </summary>
    [RegisterType]
    public class StudentNsldsInformationService : FinancialAidCoordinationService, IStudentNsldsInformationService
    {
        private readonly IStudentNsldsInformationRepository studentNsldsInformationRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="studentNsldsInformationRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public StudentNsldsInformationService(IAdapterRegistry adapterRegistry,
            IStudentNsldsInformationRepository studentNsldsInformationRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.studentNsldsInformationRepository = studentNsldsInformationRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Gets student NSLDS related information
        /// </summary>
        /// <param name="studentId">student id to retrieve information for</param>
        /// <returns>StudentNsldsInformation DTO</returns>
        public async Task<StudentNsldsInformation> GetStudentNsldsInformationAsync(string studentId)
        {
            if(string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId is required");
            }
            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access nslds information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            var studentNsldsInformationEntity = await studentNsldsInformationRepository.GetStudentNsldsInformationAsync(studentId);

            var nsldsInformationEntityToDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentNsldsInformation, StudentNsldsInformation>(_adapterRegistry, logger);

            return nsldsInformationEntityToDtoAdapter.MapToType(studentNsldsInformationEntity);
        }
    }
}
