// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Colleague.Coordination.Base.Services;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Service to fetch the CF Web default values
    /// </summary>
    [RegisterType]
    public class ColleagueFinanceWebConfigurationsService : BaseCoordinationService, IColleagueFinanceWebConfigurationsService
    {
        private IColleagueFinanceWebConfigurationsRepository cfWebConfigutationsRepository;

        // This constructor initializes the private attributes
        public ColleagueFinanceWebConfigurationsService(IColleagueFinanceWebConfigurationsRepository cfWebDefaultsRepository,
            IConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.cfWebConfigutationsRepository = cfWebDefaultsRepository;
        }


        public async Task<ColleagueFinanceWebConfiguration> GetColleagueFinanceWebConfigurationsAsync()
        {
            Dtos.ColleagueFinance.ColleagueFinanceWebConfiguration cfWebDefaultsDto = new ColleagueFinanceWebConfiguration();
            var cfWebDefaults = await cfWebConfigutationsRepository.GetColleagueFinanceWebConfigurations();
            if (cfWebDefaults != null)
            {
                // Convert the colleague finance web configuration and all its child objects into DTOs                
                var cfWebDefaultsEntityDtoAdapter = new ColleagueFinanceWebConfigurationsEntityDtoAdapter(_adapterRegistry, logger);

                cfWebDefaultsDto = cfWebDefaultsEntityDtoAdapter.MapToType(cfWebDefaults);
            }
            return cfWebDefaultsDto;
        }
    }
}
