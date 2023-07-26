// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.Finance.AccountDue
{
    /// <summary>
    /// Account due information broken down by period
    /// </summary>
    public class AccountDuePeriod
    {
        /// <summary>
        /// Past period due items
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public AccountDue Past { get; set; }

        /// <summary>
        /// Current period due items
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public AccountDue Current { get; set; }

        /// <summary>
        /// Future period due items
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public AccountDue Future { get; set; }

        /// <summary>
        /// Account holder name
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string PersonName { get; set; }

        /// <summary>
        /// Balance due
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public decimal Balance { get; set; }
    }
}
