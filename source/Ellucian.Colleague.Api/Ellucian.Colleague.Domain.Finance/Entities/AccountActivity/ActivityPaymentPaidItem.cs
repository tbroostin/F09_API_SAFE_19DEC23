// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    /// <summary>
    /// A paid transaction
    /// </summary>
    [Serializable]
    public class ActivityPaymentPaidItem : ActivityPaymentMethodItem
    {
        /// <summary>
        /// Transactional reference number
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Cash receipt number
        /// </summary>
        public string ReceiptNumber { get; set; }
    }
}
