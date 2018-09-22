// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class ActivityFinancialAidTerm
    {
        public string AwardTerm { get; set; }

        public decimal? DisbursedAmount { get; set; }

        public decimal? AnticipatedAmount { get; set; }
    }
}
