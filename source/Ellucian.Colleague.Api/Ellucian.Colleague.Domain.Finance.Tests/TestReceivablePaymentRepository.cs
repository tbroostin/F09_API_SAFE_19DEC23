// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestReceivablePaymentRepository
    {
        private static List<ReceivablePayment> _payments = new List<ReceivablePayment>();
        public static List<ReceivablePayment> Payments
        {
            get
            {
                if (_payments.Count == 0)
                {
                    GenerateEntities();
                }
                return _payments;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var arPayment in TestArPaymentsRepository.ArPayments)
            {
                var payment = new ReceivablePayment(arPayment.Recordkey, null, arPayment.ArpPersonId, arPayment.ArpArType, arPayment.ArpTerm, 
                    arPayment.ArpDate.GetValueOrDefault(), arPayment.ArpAmt.GetValueOrDefault())
                {
                    IsArchived = arPayment.ArpArchive == "Y",
                    Location = arPayment.ArpLocation
                };
                _payments.Add(payment);
            }
        }
    }
}
