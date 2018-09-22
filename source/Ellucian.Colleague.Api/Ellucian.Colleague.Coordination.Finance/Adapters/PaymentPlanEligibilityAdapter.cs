// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class PaymentPlanEligibilityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanEligibility, Ellucian.Colleague.Dtos.Finance.PaymentPlanEligibility>
    {
        public PaymentPlanEligibilityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.BillingTermPaymentPlanInformation, Ellucian.Colleague.Dtos.Finance.BillingTermPaymentPlanInformation>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanIneligibilityReason, Ellucian.Colleague.Dtos.Finance.PaymentPlanIneligibilityReason>();
        }
    }
}
