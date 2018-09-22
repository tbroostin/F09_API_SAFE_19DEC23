// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class PaymentPlanEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan, Ellucian.Colleague.Dtos.Finance.PaymentPlan>
    {
        public PaymentPlanEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PlanStatus, Ellucian.Colleague.Dtos.Finance.PlanStatus>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.ScheduledPayment, Ellucian.Colleague.Dtos.Finance.ScheduledPayment>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PlanCharge, Ellucian.Colleague.Dtos.Finance.PlanCharge>();
        }
    }
}
