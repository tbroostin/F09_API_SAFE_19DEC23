// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A sponsored payment transaction
    /// </summary>
    public class ActivitySponsorPaymentItem : ActivityDateTermItem
    {
        /// <summary>
        /// Sponsorship ID
        /// </summary>
        public string Sponsorship { get; set; }
    }
}
