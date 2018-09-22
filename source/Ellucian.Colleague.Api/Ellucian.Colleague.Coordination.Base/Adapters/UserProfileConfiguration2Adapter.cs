// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for USERPROFILECONFIGURATION2 entity
    /// </summary>
    public class UserProfileConfiguration2Adapter : AutoMapperAdapter<UserProfileConfiguration2, Ellucian.Colleague.Dtos.Base.UserProfileConfiguration2>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileConfiguration2Adapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public UserProfileConfiguration2Adapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<UserProfileViewUpdateOption, Ellucian.Colleague.Dtos.Base.UserProfileViewUpdateOption>();
        }
    }
}
