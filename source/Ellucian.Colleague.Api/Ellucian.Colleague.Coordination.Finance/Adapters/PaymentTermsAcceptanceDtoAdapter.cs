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
    public class PaymentTermsAcceptanceDtoAdapter : BaseAdapter<PaymentTermsAcceptance, Domain.Finance.Entities.PaymentTermsAcceptance>
    {
        public PaymentTermsAcceptanceDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) { }

        public override Domain.Finance.Entities.PaymentTermsAcceptance MapToType(PaymentTermsAcceptance Source)
        {
            TimeSpan localOffset = new TimeSpan((DateTime.Now - DateTime.UtcNow).Hours, (DateTime.Now - DateTime.UtcNow).Minutes, 0);
            var entity = new Domain.Finance.Entities.PaymentTermsAcceptance(Source.StudentId, Source.PaymentControlId, 
                new DateTimeOffset(Source.AcknowledgementDateTime, localOffset), Source.InvoiceIds, Source.SectionIds, Source.TermsText,
                Source.ApprovalUserId, new DateTimeOffset(Source.ApprovalReceived, localOffset)) 
                { AcknowledgementText = Source.AcknowledgementText };

            return entity;
        }
    }
}
