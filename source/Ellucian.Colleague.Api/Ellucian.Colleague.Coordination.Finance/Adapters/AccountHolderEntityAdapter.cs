// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class AccountHolderEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountHolder, Ellucian.Colleague.Dtos.Finance.AccountHolder>
    {
        public AccountHolderEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.DepositDue, Ellucian.Colleague.Dtos.Finance.DepositDue>();
        }
    }
}
