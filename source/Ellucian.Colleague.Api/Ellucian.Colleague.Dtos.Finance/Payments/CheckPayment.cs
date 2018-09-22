// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// Payment information for an e-check (going to payment provider)
    /// </summary>
    public class CheckPayment
    {
        /// <summary>
        /// ABA routing number
        /// </summary>
        public string AbaRoutingNumber { get; set; }

        /// <summary>
        /// Bank account number
        /// </summary>
        public string BankAccountNumber { get; set; }

        /// <summary>
        /// Check number
        /// </summary>
        public string CheckNumber { get; set; }

        /// <summary>
        /// Driver's license number
        /// </summary>
        public string DriversLicenseNumber { get; set; }

        /// <summary>
        /// Driver's license state
        /// </summary>
        public string DriversLicenseState { get; set; }

        /// <summary>
        /// Payer's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Payer's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Payer's billing address, line 1
        /// </summary>
        public string BillingAddress1 { get; set; }

        /// <summary>
        /// Payer's billing address, line 2
        /// </summary>
        public string BillingAddress2 { get; set; }

        /// <summary>
        /// Payer's city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Payer's state code
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Payer's ZIP/postal code
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Payer's email address
        /// </summary>
        public string EmailAddress { get; set; }
    }
}
