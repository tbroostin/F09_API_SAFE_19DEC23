// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.AccountActivity
{
    /// <summary>
    /// A term-based transaction
    /// </summary>
    public class ActivityTermItem
    {
        /// <summary>
        /// Charge amount
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Charge description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Charge ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Charge term code
        /// </summary>
        public string TermId { get; set; }
    }
}
