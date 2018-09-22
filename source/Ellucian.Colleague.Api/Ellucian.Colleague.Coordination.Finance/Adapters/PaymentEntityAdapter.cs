// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class PaymentEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.Payment, Ellucian.Colleague.Dtos.Finance.Payments.Payment>
    {
        public PaymentEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Map dependencies
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Payments.CheckPayment, Ellucian.Colleague.Dtos.Finance.Payments.CheckPayment>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentItem, Ellucian.Colleague.Dtos.Finance.Payments.PaymentItem>();
        }
    }
}
