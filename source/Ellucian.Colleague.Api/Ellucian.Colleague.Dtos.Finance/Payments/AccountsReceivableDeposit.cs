// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// An AR deposit payment
    /// </summary>
    public class AccountsReceivableDeposit
    {
        /// <summary>
        /// Deposit holder ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Deposit holder name
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Deposit type code
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Deposit type description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Term code of deposit
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// Term description of deposit
        /// </summary>
        public string TermDescription { get; set; }

        /// <summary>
        /// Location code of deposit
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Location description of deposit
        /// </summary>
        public string LocationDescription { get; set; }

        /// <summary>
        /// Amount of deposit
        /// </summary>
        public Nullable<Decimal> NetAmount { get; set; }
    }
}
