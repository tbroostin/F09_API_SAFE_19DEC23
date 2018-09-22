// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates a <see cref="Dtos.Base.PersonProxyUser"/> from a <see cref="Domain.Base.Entities.PersonProxyUser"/>
    /// </summary>
    public class PersonProxyUserEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.PersonProxyUser, Dtos.Base.PersonProxyUser>
    {
        /// <summary>
        /// Instantiates a new <see cref="PersonProxyUserEntityAdapter"/>
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public PersonProxyUserEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) :
            base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.PersonName, Dtos.Base.PersonName>();
            AddMappingDependency<Domain.Base.Entities.EmailAddress, Dtos.Base.EmailAddress>();
            AddMappingDependency<Domain.Base.Entities.Phone, Dtos.Base.Phone>();
        }

    }
}
