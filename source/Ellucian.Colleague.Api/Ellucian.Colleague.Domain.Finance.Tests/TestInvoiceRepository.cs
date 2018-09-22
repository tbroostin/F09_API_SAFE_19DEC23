// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Data.Finance.Tests;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestInvoiceRepository
    {
        private static List<Invoice> _invoices = new List<Invoice>();
        public static List<Invoice> Invoices 
        { 
            get 
            {
                if (_invoices.Count == 0)
                {
                    GenerateEntities();
                }
                return _invoices; 
            }
        }

        private static void GenerateEntities()
        {
            foreach (var arInvoice in TestArInvoicesRepository.ArInvoices)
            {
                var invoiceItems = TestChargeRepository.Charges.Where(x => x.InvoiceId == arInvoice.Recordkey);
                var invoice = new Invoice(arInvoice.Recordkey,
                    arInvoice.InvPersonId,
                    arInvoice.InvArType,
                    arInvoice.InvTerm,
                    arInvoice.InvNo,
                    arInvoice.InvDate.Value,
                    arInvoice.InvDueDate.Value,
                    arInvoice.InvBillingStartDate.Value,
                    arInvoice.InvBillingEndDate.Value,
                    arInvoice.InvDesc, invoiceItems)
                {
                    Archived = !String.IsNullOrEmpty(arInvoice.InvArchive),
                    AdjustmentToInvoice = arInvoice.InvAdjToInvoice
                };
                if (arInvoice.InvAdjByInvoices != null && arInvoice.InvAdjByInvoices.Count > 0)
                {
                    foreach (var adj in arInvoice.InvAdjByInvoices)
                    {
                        invoice.AddAdjustingInvoice(adj);
                    }
                }
                _invoices.Add(invoice);
                
            }
        }
    }
}
