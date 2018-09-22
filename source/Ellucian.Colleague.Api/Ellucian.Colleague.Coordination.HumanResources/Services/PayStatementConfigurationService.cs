/*Copyright 2017-2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PayStatementConfigurationService : BaseCoordinationService, IPayStatementConfigurationService
    {
        private readonly IHumanResourcesReferenceDataRepository humanResourceReferenceDataRepository;

        public PayStatementConfigurationService(
            IHumanResourcesReferenceDataRepository humanResourceReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.humanResourceReferenceDataRepository = humanResourceReferenceDataRepository;
        }

        public async Task<PayStatementConfiguration> GetPayStatementConfigurationAsync()
        {
            var domainConfig = await humanResourceReferenceDataRepository.GetPayStatementConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.PayStatementConfiguration, PayStatementConfiguration>();
            return adapter.MapToType(domainConfig);
        }
    }
}
