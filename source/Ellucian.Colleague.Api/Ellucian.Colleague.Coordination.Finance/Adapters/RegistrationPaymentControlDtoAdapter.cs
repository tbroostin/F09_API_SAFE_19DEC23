// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Finance;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class RegistrationPaymentControlDtoAdapter : AutoMapperAdapter<RegistrationPaymentControl, Domain.Finance.Entities.RegistrationPaymentControl>
    {
        public RegistrationPaymentControlDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Domain.Finance.Entities.RegistrationPaymentControl MapToType(RegistrationPaymentControl Source)
        {
            var entity = new Domain.Finance.Entities.RegistrationPaymentControl(Source.Id, Source.StudentId, Source.TermId,
                (Domain.Finance.Entities.RegistrationPaymentStatus)Source.PaymentStatus);
            if (Source.InvoiceIds != null && Source.InvoiceIds.Count() > 0)
            {
                foreach (var invoiceId in Source.InvoiceIds)
                {
                    entity.AddInvoice(invoiceId);
                }
            }
            if (Source.RegisteredSectionIds != null && Source.RegisteredSectionIds.Count() > 0)
            {
                foreach (var sectionId in Source.RegisteredSectionIds)
                {
                    entity.AddRegisteredSection(sectionId);
                }
            }
            if (Source.Payments != null && Source.Payments.Count() > 0)
            {
                foreach (var paymentId in Source.Payments)
                {
                    entity.AddPayment(paymentId);
                }
            }

            return base.MapToType(Source);
        }
    }
}
