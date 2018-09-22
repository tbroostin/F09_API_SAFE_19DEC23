// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class PlanStatusEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanStatus, Ellucian.Colleague.Dtos.Finance.PlanStatus>
    {
        public PlanStatusEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PlanStatusType, Ellucian.Colleague.Dtos.Finance.PlanStatusType>();
        }
    }
}
