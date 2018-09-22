// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class PlanChargeEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanCharge, Ellucian.Colleague.Dtos.Finance.PlanCharge>
    {
        public PlanChargeEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Charge, Ellucian.Colleague.Dtos.Finance.Charge>();
        }
    }
}
