// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

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
        public string FirstName { get; set; }

        /// <summary>
        /// Payer's middle name
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Payer's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Payer's street address
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Payer's city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Payer's state code
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Payer's postal code
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Payer's country code
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Payer's email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Payer's telephone number
        /// </summary>
        public string Telephone { get; set; }
    }
}
