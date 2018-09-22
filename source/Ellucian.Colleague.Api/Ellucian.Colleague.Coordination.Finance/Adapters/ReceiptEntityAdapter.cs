// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class ReceiptEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Receipt, Ellucian.Colleague.Dtos.Finance.Receipt>
    {
        public ReceiptEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.NonCashPayment, Ellucian.Colleague.Dtos.Finance.NonCashPayment>();
        }
    }
}
