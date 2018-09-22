// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class PaymentTermsAcceptance2DtoAdapter : BaseAdapter<PaymentTermsAcceptance2, Domain.Finance.Entities.PaymentTermsAcceptance>
    {
        public PaymentTermsAcceptance2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) { }

        public override Domain.Finance.Entities.PaymentTermsAcceptance MapToType(PaymentTermsAcceptance2 Source)
        {
            var entity = new Domain.Finance.Entities.PaymentTermsAcceptance(Source.StudentId, Source.PaymentControlId, Source.AcknowledgementDateTime,
                Source.InvoiceIds, Source.SectionIds, Source.TermsText, Source.ApprovalUserId, Source.ApprovalReceived) 
                { AcknowledgementText = Source.AcknowledgementText.ToList() };

            return entity;
        }
    }
}
