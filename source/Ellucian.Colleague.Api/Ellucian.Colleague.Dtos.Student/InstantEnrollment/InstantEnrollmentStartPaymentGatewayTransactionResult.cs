// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{

    /// <summary>
    /// The result of starting an Instant Enrollment payment gateway transaction
    /// </summary>
    public class InstantEnrollmentStartPaymentGatewayRegistrationResult
    {

        /// <summary>
        /// List of error messages resulting from starting the payment gateway transaction.
        /// The operation was successful if this list is empty.
        /// The operation failed if this list contains any messages.
        /// This list may contain warning messages mixed with the error messages.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// The url to which the user should be redirected to make a payment through an external 
        /// payment provider. 
        /// This will only be set when the operation was successful.
        /// </summary>
        public string PaymentProviderRedirectUrl { get; set; }

    }
}
