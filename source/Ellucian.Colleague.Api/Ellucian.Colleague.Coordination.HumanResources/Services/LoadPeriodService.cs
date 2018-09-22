/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class LoadPeriodService : BaseCoordinationService, ILoadPeriodService
    {
        private ILoadPeriodRepository _loadPeriodRepository;
        public LoadPeriodService(ILoadPeriodRepository loadPeriodRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ILogger logger, IStaffRepository staffRepository = null, IConfigurationRepository configurationRepository = null) :
            base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            
            _loadPeriodRepository = loadPeriodRepository;
        }

        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.LoadPeriod>> GetLoadPeriodsByIdsAsync(IEnumerable<string> ids)
        {
            if(ids == null)
            {
                throw new ArgumentNullException("ids", "ids cannot be null or empty");
            }

            var loadPeriodDtos = new List<Dtos.Base.LoadPeriod>();

            if (ids.Any())
            {
                var loadPeriodsEntities = await _loadPeriodRepository.GetLoadPeriodsByIdsAsync(ids);

                var loadPeriodAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.LoadPeriod, Dtos.Base.LoadPeriod>();

                foreach (var loadPeriodsEntity in loadPeriodsEntities)
                {
                    loadPeriodDtos.Add(loadPeriodAdapter.MapToType(loadPeriodsEntity));
                }
            }
            return loadPeriodDtos;
        }

    }
}
