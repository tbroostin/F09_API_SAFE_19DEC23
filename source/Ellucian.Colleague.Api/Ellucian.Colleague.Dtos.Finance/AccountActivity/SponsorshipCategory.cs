// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Sponsorship payments on an account
    /// </summary>
    public partial class SponsorshipCategory
    {
        /// <summary>
        /// List of <see cref="ActivitySponsorPaymentItem">sponsor payments</see>
        /// </summary>
        public List<ActivitySponsorPaymentItem> SponsorItems { get; set; }
    }
}
