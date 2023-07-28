// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.Finance.Configuration
{
    /// <summary>
    /// A payment method available for use on Make a Payment
    /// </summary>
    public class AvailablePaymentMethod
    {
        /// <summary>
        /// AvailablePaymentMethod constructor
        /// </summary>
        public AvailablePaymentMethod()
        {
            InternalCode = string.Empty;
            Description = string.Empty;
            Type = string.Empty;
        }

        /// <summary>
        /// Payment method code
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string InternalCode { get; set; }

        /// <summary>
        /// Payment method description
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string Description { get; set; }

        /// <summary>
        /// Type of payment method
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string Type { get; set; }
    }
}
