/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class BenefitsEnrollmentConfigurationService : BaseCoordinationService, IBenefitsEnrollmentConfigurationService
    {
        private readonly IBenefitsEnrollmentConfigurationRepository benefitsEnrollmentConfigurationRepository;

        public BenefitsEnrollmentConfigurationService(
            IBenefitsEnrollmentConfigurationRepository benefitsEnrollmentConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IStaffRepository staffRepository = null,
            IConfigurationRepository configurationRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            this.benefitsEnrollmentConfigurationRepository = benefitsEnrollmentConfigurationRepository;
        }

        /// <summary>
        /// Gets the configurations for benefits enrollment
        /// </summary>
        /// <returns>BenefitsEnrollmentConfiguration</returns>
        public async Task<BenefitsEnrollmentConfiguration> GetBenefitsEnrollmentConfigurationAsync()
        {
            try
            {
                var benefitsEnrollmentConfigurationEntity = await benefitsEnrollmentConfigurationRepository.GetBenefitsEnrollmentConfigurationAsync();
                var entityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.BenefitsEnrollmentConfiguration, Dtos.HumanResources.BenefitsEnrollmentConfiguration>();
                var benefitsEnrollmentConfigurationDto = entityToDtoAdapter.MapToType(benefitsEnrollmentConfigurationEntity);

                return benefitsEnrollmentConfigurationDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
