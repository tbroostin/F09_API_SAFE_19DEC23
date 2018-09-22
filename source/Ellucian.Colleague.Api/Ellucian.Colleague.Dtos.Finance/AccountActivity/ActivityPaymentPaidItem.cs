// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A paid transaction
    /// </summary>
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
