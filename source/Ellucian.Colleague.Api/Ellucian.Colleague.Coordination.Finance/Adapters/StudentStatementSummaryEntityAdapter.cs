// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class StudentStatementSummaryEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.StudentStatementSummary, Ellucian.Colleague.Dtos.Finance.StudentStatementSummary>
    {
        public StudentStatementSummaryEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.ActivityTermItem, Ellucian.Colleague.Dtos.Finance.AccountActivity.ActivityTermItem>();
        }
    }
}
