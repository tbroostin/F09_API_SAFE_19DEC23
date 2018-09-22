using Ellucian.Web.Adapters;
// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class RegistrationBillingEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.RegistrationBilling, Ellucian.Colleague.Dtos.Finance.RegistrationBilling>
    {
        public RegistrationBillingEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.RegistrationBillingItem, Ellucian.Colleague.Dtos.Finance.RegistrationBillingItem>();
        }
    }
}