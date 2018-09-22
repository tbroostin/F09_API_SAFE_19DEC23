using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestInvoiceTypeRepository
    {
        private static List<InvoiceType> _invoiceTypes = new List<InvoiceType>();
        public static List<InvoiceType> InvoiceTypes
        {
            get
            {
                if (_invoiceTypes.Count == 0)
                {
                    GenerateEntities();
                }
                return _invoiceTypes;
            }
        }

        private static void GenerateEntities()
        {
            _invoiceTypes.Add(new InvoiceType("PROMO", "Promotional item sale"));
            _invoiceTypes.Add(new InvoiceType("SGVT", "Student govt mtg expense"));
            _invoiceTypes.Add(new InvoiceType("BK", "Bookstore"));
            _invoiceTypes.Add(new InvoiceType("EXTRL", "External Res. Life System"));
        }
    }
}
