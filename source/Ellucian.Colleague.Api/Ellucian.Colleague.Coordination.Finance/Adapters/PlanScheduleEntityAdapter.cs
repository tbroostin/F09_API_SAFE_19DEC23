// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class PlanScheduleEntityAdapter : AutoMapperAdapter<Domain.Finance.Entities.PlanSchedule, Dtos.Finance.PlanSchedule>
    {
        public PlanScheduleEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Finance.Entities.PlanSchedule, Dtos.Finance.PlanSchedule>();
        }
    }
}
