// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    /// <summary>
    /// Adapt a NonCashPayment DTO to an entity
    /// </summary>
    public class NonCashPaymentDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Dtos.Finance.NonCashPayment, Ellucian.Colleague.Domain.Finance.Entities.NonCashPayment>
    {
        public NonCashPaymentDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Dtos.Finance.NonCashPayment, Ellucian.Colleague.Domain.Finance.Entities.NonCashPayment>();
        }
    }
}
