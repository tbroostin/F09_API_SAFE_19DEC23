// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class AllocationEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentAllocation, Ellucian.Colleague.Dtos.Finance.PaymentAllocation>
    {
        public AllocationEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PaymentAllocationSource, Ellucian.Colleague.Dtos.Finance.PaymentAllocationSource>();
        }
    }
}
