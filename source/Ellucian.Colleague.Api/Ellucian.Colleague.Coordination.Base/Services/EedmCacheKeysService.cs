// Copyright 2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class EedmCacheKeysService : BaseCoordinationService, IEedmCacheKeysService
    {
        private readonly IEedmCacheKeysRepository _eedmCacheKeysRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ILogger _logger;
        private const string _dataOrigin = "Colleague";

        public EedmCacheKeysService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            IEedmCacheKeysRepository eedmCacheKeysRepository, IConfigurationRepository configurationRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _logger = logger;
            _configurationRepository = configurationRepository;
            _eedmCacheKeysRepository = eedmCacheKeysRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Clear EEDM cache keys
        /// </summary>
        public void ClearEedmCacheKeys()
        {
            _eedmCacheKeysRepository.ClearEedmCacheKeys();  
        }
    }
}