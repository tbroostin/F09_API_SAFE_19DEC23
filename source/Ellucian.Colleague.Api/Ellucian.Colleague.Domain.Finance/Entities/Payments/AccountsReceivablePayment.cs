// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class AccountsReceivablePayment
    {
        
        public string PersonId { get; set; }

        
        public string PersonName { get; set; }

        
        public string Type { get; set; }

        
        public string Description { get; set; }

        
        public string Term { get; set; }

        
        public string TermDescription { get; set; }

        
        public string Location { get; set; }

        
        public string LocationDescription { get; set; }

        
        public string PaymentDescription { get; set; }

        
        public Nullable<Decimal> NetAmount { get; set; }

        public string PaymentControlId { get; set; }
    }
}
