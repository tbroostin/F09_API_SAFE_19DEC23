// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// Payments on an account
    /// </summary>
    public partial class StudentPaymentCategory
    {
        /// <summary>
        /// A list of <see cref="ActivityPaymentPaidItem">payments</see>
        /// </summary>
        public List<ActivityPaymentPaidItem> StudentPayments { get; set; }
    }
}
