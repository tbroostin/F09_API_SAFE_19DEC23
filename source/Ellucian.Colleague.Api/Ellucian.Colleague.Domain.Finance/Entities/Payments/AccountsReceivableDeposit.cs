// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class AccountsReceivableDeposit
    {
        
        public string PersonId { get; set; }

        
        public string PersonName { get; set; }

        
        public string Type { get; set; }

        
        public string Description { get; set; }

        
        public string Term { get; set; }

        
        public string TermDescription { get; set; }

        
        public string Location { get; set; }

        
        public string LocationDescription { get; set; }

        
        public Nullable<Decimal> NetAmount { get; set; }
    }
}
