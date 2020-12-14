// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]

    /// <summary>
    /// The result of the repository method to start an Instant Enrollment payment gateway transaction
    /// </summary>
    public class InstantEnrollmentStartPaymentGatewayRegistrationResult
    {

        /// <summary>
        /// List of error messages resulting from starting the payment gateway transaction.
        /// The operation was successful if this list is empty.
        /// The operation failed if this list contains any messages.
        /// This list may contain warning messages mixed with the error messages, and there is no way 
        /// to know which message are warnings due to the design of the underlying code.
        /// </summary>
        public List<string> ErrorMessages{ get; private set; }

        /// <summary>
        /// The url to which the user should be redirected to make a payment through an external 
        /// payment provider. 
        /// This will only be set when the operation was successful.
        /// </summary>
        public string PaymentProviderRedirectUrl{ get; private set; }

        public InstantEnrollmentStartPaymentGatewayRegistrationResult(IEnumerable<string> errorMessages, string paymentProviderRedirectUrl)
        {
            ErrorMessages = new List<string>();
            if (errorMessages != null) ErrorMessages.AddRange(errorMessages);
            PaymentProviderRedirectUrl = paymentProviderRedirectUrl;

            // One of the two arguments must have a value, but both cannot have a value at the same time.
            if (ErrorMessages.Count(e => !(string.IsNullOrEmpty(e))) > 0)
            {
                // Messages. There cannot be a url
                if (!string.IsNullOrEmpty(PaymentProviderRedirectUrl))
                {
                    throw new ArgumentException("There cannot be both error messages and a payment provider redirect url");
                }
                // Else we have error messages
            } else
            {
                // No messages. There must be a url
                if (string.IsNullOrEmpty(PaymentProviderRedirectUrl))
                {
                    throw new ArgumentNullException("At least one error message or a payment provider redirect url must be specified");
                }
                // Else we have a url
            }
        }
    }
}
