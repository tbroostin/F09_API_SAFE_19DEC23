﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class BillingTermPaymentPlanInformationEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.BillingTermPaymentPlanInformation, Ellucian.Colleague.Dtos.Finance.BillingTermPaymentPlanInformation>
    {
        public BillingTermPaymentPlanInformationEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanIneligibilityReason, Ellucian.Colleague.Dtos.Finance.PaymentPlanIneligibilityReason>();
        }
    }
}
