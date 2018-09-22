using Ellucian.Web.Adapters;
// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class RegistrationBillingItemEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.RegistrationBillingItem, Ellucian.Colleague.Dtos.Finance.RegistrationBillingItem>
    {
        public RegistrationBillingItemEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section>();
        }
    }
}