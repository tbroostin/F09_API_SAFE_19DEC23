// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for ProxyUser entity->Dto mapping
    /// </summary>
    public class ProxyUserEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.ProxyUser, Dtos.Base.ProxyUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyUserEntityAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public ProxyUserEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.ProxyAccessPermission, Dtos.Base.ProxyAccessPermission>();
        }
    }
}
