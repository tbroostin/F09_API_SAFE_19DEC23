// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class ActivityTermItem
    {
        // The nullable decimal type is used because the Amount can be empty 
        // and the precision for decimal is optimal for monetary values.
        public decimal? Amount { get; set; }

        public string Description { get; set; }

        public string Id { get; set; }

        public string TermId { get; set; }
    }
}
