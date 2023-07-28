// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Attributes;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.AccountDue
{
    /// <summary>
    /// Account due information broken down by term
    /// </summary>
    public class AccountDue
    {
        /// <summary>
        /// AccountDue constructor
        /// </summary>
        public AccountDue()
        {
            AccountTerms = new List<AccountTerm>();
        }

        /// <summary>
        /// A list of <see cref="AccountTerm">AccountTerm</see> items
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public List<AccountTerm> AccountTerms { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public DateTime? EndDate { get; set; }

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
