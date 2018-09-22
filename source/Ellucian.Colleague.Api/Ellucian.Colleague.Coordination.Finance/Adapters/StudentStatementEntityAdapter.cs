// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class StudentStatementEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.StudentStatement, Ellucian.Colleague.Dtos.Finance.StudentStatement>
    {
        public StudentStatementEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.StudentStatementSummary, Ellucian.Colleague.Dtos.Finance.StudentStatementSummary>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Ellucian.Colleague.Dtos.Finance.AccountActivity.DetailedAccountPeriod>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.StudentStatementScheduleItem, Ellucian.Colleague.Dtos.Finance.StudentStatementScheduleItem>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Configuration.ActivityDisplay, Ellucian.Colleague.Dtos.Finance.Configuration.ActivityDisplay>();
        }
    }
}
