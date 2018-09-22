// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public partial class FinancialAidCategory
    {
        public List<ActivityFinancialAidItem> AnticipatedAid { get; set; }

         // public List<DateTermItem> FinancialAid { get; set; }
        public List<ActivityDateTermItem> DisbursedAid { get; set; }
    }
}