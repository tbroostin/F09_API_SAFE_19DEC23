// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class Payment
    {
        public Payment()
        {
            PaymentItems = new List<PaymentItem>();
        }

        public string PersonId { get; set; }

        public string PayerId { get; set; }

        public string ReturnUrl { get; set; }

        public string Distribution { get; set; }

        public string PayMethod { get; set; }

        public decimal AmountToPay { get; set; }

        public string ProviderAccount { get; set; }

        public string ConvenienceFee { get; set; }

        public decimal? ConvenienceFeeAmount { get; set; }

        public string ConvenienceFeeGeneralLedgerNumber { get; set; }

        public CheckPayment CheckDetails { get; set; }

        public List<PaymentItem> PaymentItems { get; set; }

        public string ReturnToOriginUrl { get; set; }
    }
}
