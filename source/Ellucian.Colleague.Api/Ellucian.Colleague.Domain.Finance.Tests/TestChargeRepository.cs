using System.Collections.Generic;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestChargeRepository
    {
        private static List<Charge> _charges = new List<Charge>();
        public static List<Charge> Charges 
        { 
            get 
            {
                if (_charges.Count == 0)
                {
                    GenerateEntities();
                }
                return _charges;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var record in TestArInvoiceItemsRepository.ArInvoiceItems)
            {
                decimal amount = record.InviExtChargeAmt.GetValueOrDefault() - record.InviExtCrAmt.GetValueOrDefault();
                var desc = record.InviDesc.Split(DmiString._VM);
                var entity = new Charge(record.Recordkey, record.InviInvoice, desc, record.InviArCode, amount);
                _charges.Add(entity);
            }
        }

    }
}
