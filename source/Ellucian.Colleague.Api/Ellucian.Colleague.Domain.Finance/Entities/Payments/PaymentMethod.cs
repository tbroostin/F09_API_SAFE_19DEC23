// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class PaymentMethod
    {
        public string PayMethodCode { get; set; }

        
        public string PayMethodDescription { get; set; }

        
        public string ControlNumber { get; set; }

        
        public string ConfirmationNumber { get; set; }

        
        public string TransactionNumber { get; set; }

        
        public string TransactionDescription { get; set; }

        
        public Nullable<Decimal> TransactionAmount { get; set; }
    }
}
