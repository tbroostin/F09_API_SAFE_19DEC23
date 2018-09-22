// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class PaymentDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Dtos.Finance.Payments.Payment, Ellucian.Colleague.Domain.Finance.Entities.Payments.Payment>
    {
        public PaymentDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            // Map dependencies
            AddMappingDependency<Ellucian.Colleague.Dtos.Finance.Payments.CheckPayment, Ellucian.Colleague.Domain.Finance.Entities.Payments.CheckPayment>();
            AddMappingDependency<Ellucian.Colleague.Dtos.Finance.Payments.PaymentItem, Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentItem>();
        }
    }
}
