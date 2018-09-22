// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A deposit
    /// </summary>
    public class Deposit
    {
        /// <summary>
        /// ID of the deposit
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the deposit holder
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Date on which the deposit payment was made
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Type of deposit
        /// </summary>
        public string DepositType { get; set; }

        /// <summary>
        /// Deposit amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Term of the deposit
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// Receipt used when creating the deposit
        /// </summary>
        public string ReceiptId { get; set; }

        /// <summary>
        /// External system
        /// </summary>
        public string ExternalSystem { get; set; }

        /// <summary>
        /// External identifier
        /// </summary>
        public string ExternalIdentifier { get; set; }
    }
}
