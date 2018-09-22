// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class ReceiptDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Dtos.Finance.Receipt, Ellucian.Colleague.Domain.Finance.Entities.Receipt>
    {
        public ReceiptDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Domain.Finance.Entities.Receipt MapToType(Dtos.Finance.Receipt source)
        {
            var nonCashPayments = new List<Domain.Finance.Entities.NonCashPayment>();
            if (source.NonCashPayments != null && source.NonCashPayments.Any())
            {
                foreach (var pmt in source.NonCashPayments)
                {
                    nonCashPayments.Add(new Domain.Finance.Entities.NonCashPayment(pmt.PaymentMethodCode, pmt.Amount));
                }
            }

            var entity = new Domain.Finance.Entities.Receipt(source.Id,
                source.ReferenceNumber,
                source.Date,
                source.PayerId,
                source.DistributionCode,
                source.DepositIds,
                nonCashPayments)
                {
                    CashierId = source.CashierId,
                    PayerName = source.PayerName
                };
            if (!string.IsNullOrEmpty(source.ExternalIdentifier) || !string.IsNullOrEmpty(source.ExternalSystem))
            {
                entity.AddExternalSystemAndId(source.ExternalSystem, source.ExternalIdentifier);
            }
            return entity;
        }

    }
}
