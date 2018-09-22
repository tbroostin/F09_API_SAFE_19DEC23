// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class PaymentReceiptEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentReceipt, Ellucian.Colleague.Dtos.Finance.Payments.PaymentReceipt>
    {
        public PaymentReceiptEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Payments.AccountsReceivablePayment, Ellucian.Colleague.Dtos.Finance.Payments.AccountsReceivablePayment>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Payments.AccountsReceivableDeposit, Ellucian.Colleague.Dtos.Finance.Payments.AccountsReceivableDeposit>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Payments.GeneralPayment, Ellucian.Colleague.Dtos.Finance.Payments.GeneralPayment>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Payments.ConvenienceFee, Ellucian.Colleague.Dtos.Finance.Payments.ConvenienceFee>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentMethod, Ellucian.Colleague.Dtos.Finance.Payments.PaymentMethod>();
        }
    }
}
