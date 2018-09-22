// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    /// <summary>
    /// Adapt a NonCashPayment entity to a DTO
    /// </summary>
    public class NonCashPaymentEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.NonCashPayment, Ellucian.Colleague.Dtos.Finance.NonCashPayment>
    {
        public NonCashPaymentEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.NonCashPayment, Ellucian.Colleague.Dtos.Finance.NonCashPayment>();
        }
    }
}
