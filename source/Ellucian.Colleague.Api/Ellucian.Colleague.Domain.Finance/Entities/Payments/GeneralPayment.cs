// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class GeneralPayment
    {
        
        public string Code { get; set; }

        
        public string Description { get; set; }

        
        public string Location { get; set; }

        
        public string LocationDescription { get; set; }

        
        public Nullable<Decimal> NetAmount { get; set; }
    }
}
