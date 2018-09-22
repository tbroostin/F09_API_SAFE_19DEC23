using Ellucian.Web.Adapters;
// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class PaymentRequirementEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentRequirement, Ellucian.Colleague.Dtos.Finance.PaymentRequirement>
    {
        public PaymentRequirementEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PaymentDeferralOption, Ellucian.Colleague.Dtos.Finance.PaymentDeferralOption>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanOption, Ellucian.Colleague.Dtos.Finance.PaymentPlanOption>();
        }
    }
}
