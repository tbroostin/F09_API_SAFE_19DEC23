// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class BillingTermPaymentPlanInformationDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Dtos.Finance.BillingTermPaymentPlanInformation, Ellucian.Colleague.Domain.Finance.Entities.BillingTermPaymentPlanInformation>
    {
        public BillingTermPaymentPlanInformationDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) { }

        public override Domain.Finance.Entities.BillingTermPaymentPlanInformation MapToType(Dtos.Finance.BillingTermPaymentPlanInformation source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new Domain.Finance.Entities.BillingTermPaymentPlanInformation(source.PersonId, source.TermId, source.ReceivableTypeCode, source.PaymentPlanAmount, source.PaymentPlanTemplateId)
            {
                IneligibilityReason = (Domain.Finance.Entities.PaymentPlanIneligibilityReason?)source.IneligibilityReason
            };
        }
    }
}
