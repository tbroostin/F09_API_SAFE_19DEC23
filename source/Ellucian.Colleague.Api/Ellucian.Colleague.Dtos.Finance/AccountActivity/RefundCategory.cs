// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Refund transactions
    /// </summary>
    public partial class RefundCategory
    {
        /// <summary>
        /// A list of <see cref="ActivityPaymentMethodItem">refunds</see>
        /// </summary>
        public List<ActivityPaymentMethodItem> Refunds { get; set; }
    }
}