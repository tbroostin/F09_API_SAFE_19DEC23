// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// Result of e-check processing
    /// </summary>
    public class ElectronicCheckProcessingResult
    {
        /// <summary>
        /// Cash receipt ID (if payment successful)
        /// </summary>
        public string CashReceiptsId { get; set; }

        /// <summary>
        /// Error message (if payment failed)
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
