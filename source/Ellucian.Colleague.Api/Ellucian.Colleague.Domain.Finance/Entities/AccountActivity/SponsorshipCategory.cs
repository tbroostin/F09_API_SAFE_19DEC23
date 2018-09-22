// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    /// <summary>
    /// Use this class to store the list of SponsorPaymentItems when
    /// populating the account activity period.
    /// </summary>
    [Serializable]
    public partial class SponsorshipCategory
    {
        public List<ActivitySponsorPaymentItem> SponsorItems { get; set; }
    }
}
