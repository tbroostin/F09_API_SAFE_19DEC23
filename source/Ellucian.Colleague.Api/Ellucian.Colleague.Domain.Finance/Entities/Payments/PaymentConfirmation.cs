// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class PaymentConfirmation
    {
        public PaymentConfirmation()
        {
            ConfirmationText = new List<string>();
        }

        public string ProviderAccount { get; set; }
        public string ConvenienceFeeCode { get; set; }
        public string ConvenienceFeeDescription { get; set; }
        public Nullable<Decimal> ConvenienceFeeAmount { get; set; }
        public string ConvenienceFeeGeneralLedgerNumber { get; set; }
        public List<string> ConfirmationText { get; set; }
        public string ErrorMessage { get; set; }
    }
}
