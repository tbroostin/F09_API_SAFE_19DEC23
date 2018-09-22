// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    /// <summary>
    /// Adapts a ReceivableInvoice DTO to a ReceivableInvoice entity
    /// </summary>
    public class ReceivableInvoiceDtoAdapter : BaseAdapter<Ellucian.Colleague.Dtos.Finance.ReceivableInvoice, Ellucian.Colleague.Domain.Finance.Entities.ReceivableInvoice>
    {
        /// <summary>
        /// Constructor for ReceivableInvoiceDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry">Base interface for adapter registries</param>
        /// <param name="logger">Interface for logging mechanisms</param>
        public ReceivableInvoiceDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Maps a ReceivableInvoice DTO to its corresponding domain entity
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        public override Domain.Finance.Entities.ReceivableInvoice MapToType(ReceivableInvoice invoice)
        {
            var charges = new List<Domain.Finance.Entities.ReceivableCharge>();
            if (invoice.Charges != null && invoice.Charges.Any())
            {
                charges.AddRange(invoice.Charges.Select(item => ReceivableChargeDtoAdapterMapToType(item)));
            }

            var entity = new Domain.Finance.Entities.ReceivableInvoice(invoice.Id,
                invoice.ReferenceNumber,
                invoice.PersonId,
                invoice.ReceivableType,
                invoice.TermId,
                invoice.Date,
                invoice.DueDate,
                invoice.BillingStart,
                invoice.BillingEnd,
                invoice.Description,
                charges)
                {
                    InvoiceType = invoice.InvoiceType,
                    AdjustmentToInvoice = invoice.AdjustmentToInvoice,
                    Location = invoice.Location
                };
            entity.AddExternalSystemAndId(invoice.ExternalSystem, invoice.ExternalIdentifier);

            if (invoice.AdjustedByInvoices != null && invoice.AdjustedByInvoices.Count > 0) {
                foreach (var inv in invoice.AdjustedByInvoices)
                {
                    entity.AddAdjustingInvoice(inv);
                }
            }

            return entity;
        }

        private Domain.Finance.Entities.ReceivableCharge ReceivableChargeDtoAdapterMapToType(ReceivableCharge charge)
        {
            var entity = new Domain.Finance.Entities.ReceivableCharge(charge.Id, charge.InvoiceId, charge.Description,
                charge.Code, charge.BaseAmount);
            entity.TaxAmount = charge.TaxAmount;

            if (charge.AllocationIds != null && charge.AllocationIds.Count > 0)
            {
                foreach (var alloc in charge.AllocationIds)
                {
                    entity.AddAllocation(alloc);
                }
            }

            if (charge.PaymentPlanIds != null && charge.PaymentPlanIds.Count > 0)
            {
                foreach (var plan in charge.PaymentPlanIds)
                {
                    entity.AddPaymentPlan(plan);
                }
            }

            return entity;
        }
    }
}
