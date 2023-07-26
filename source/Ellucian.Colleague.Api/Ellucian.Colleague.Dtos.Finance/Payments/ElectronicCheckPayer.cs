// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// Information about an e-check payer (coming from Colleague)
    /// </summary>
    public class ElectronicCheckPayer
    {
        /// <summary>
        /// Payer's first name
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string FirstName { get; set; }

        /// <summary>
        /// Payer's middle name
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string MiddleName { get; set; }

        /// <summary>
        /// Payer's last name
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string LastName { get; set; }

        /// <summary>
        /// Payer's street address
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string Street { get; set; }

        /// <summary>
        /// Payer's city
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string City { get; set; }

        /// <summary>
        /// Payer's state code
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string State { get; set; }

        /// <summary>
        /// Payer's postal code
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string PostalCode { get; set; }

        /// <summary>
        /// Payer's country code
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string Country { get; set; }

        /// <summary>
        /// Payer's email address
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string Email { get; set; }

        /// <summary>
        /// Payer's telephone number
        /// </summary>
        [Metadata(DataIsInquiryOnly = true)]
        public string Telephone { get; set; }
    }
}
