// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for Roles
    /// </summary>
    public class RoleAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Entities.Role, Dtos.Base.Role>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public RoleAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Entities.Permission, Dtos.Base.Permission>();
        }
    }
}
