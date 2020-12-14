// Copyrght 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]
    /// <summary>
    /// Contains the information needed to start a payment gateway transaction to pay for an instant enrollment registration.
    /// The process of starting the payment gateway transaction includes registering the student, and created person
    /// and student records as needed. This contains the section, demographic, and financial information necessary to 
    /// perform those functions.
    /// </summary>
    public class InstantEnrollmentPaymentGatewayRegistration : InstantEnrollmentRegistrationBaseRegistration
    {
        /// <summary>
        /// The expected cost of the registered sections.  Includes the convenience fee amount.
        /// </summary>
        public decimal PaymentAmount { get; private set; }

        /// <summary>
        /// The payment method. A code from the WEB.PAY.METHODS valcode in Colleague.
        /// </summary>
        public string PaymentMethod { get; private set; }
        
        /// <summary>
        /// The Colleague self-service URL to which the user should be directed after interacting with the external payment provider.
        /// </summary>
        public string ReturnUrl { get; private set; }

        /// <summary>
        /// The GL distribution associated with the provider account and convenience fee
        /// </summary>
        public string GlDistribution { get; private set; }

        /// <summary>
        /// The ecommerce provider account.
        /// </summary>
        public string ProviderAccount { get; private set; }

        /// <summary>
        /// The description of any convenience fee.
        /// </summary>
        public string ConvenienceFeeDesc { get; private set; }

        /// <summary>
        /// The amount of any convenience fee.
        /// </summary>
        public decimal? ConvenienceFeeAmount { get; private set; }

        /// <summary>
        /// The GL account of any convenience fee.  Must be supplied if the convenience fee is charged.
        /// </summary>
        public string ConvenienceFeeGlAccount { get; private set; }

        public InstantEnrollmentPaymentGatewayRegistration(
            string personId,
            InstantEnrollmentPersonDemographic personDemographics,
            string acadProgram,
            string catalog,
            List<InstantEnrollmentRegistrationBaseSectionToRegister> sections,
            decimal payAmount,
            string payMethod,
            string returnUrl,
            string glDistribution,
            string providerAccount,
            string convFeeDesc,
            decimal? convFeeAmount,
            string convFeeGlAccount
            ) : base(personId, personDemographics, acadProgram, catalog, sections)
        {
            if (payAmount <= 0)
            {
                throw new ArgumentOutOfRangeException("payAmount", "Payment amount cannot be zero or negative.");
            }
            if (String.IsNullOrEmpty(payMethod))
            {
                throw new ArgumentNullException("payMethod");
            }
            if (String.IsNullOrEmpty(returnUrl))
            {
                throw new ArgumentNullException("returnUrl");
            }

            PaymentAmount = payAmount;
            PaymentMethod = payMethod;
            ReturnUrl = returnUrl;
            GlDistribution = glDistribution;
            ProviderAccount = providerAccount;
            ConvenienceFeeDesc = convFeeDesc;
            ConvenienceFeeAmount = convFeeAmount;
            ConvenienceFeeGlAccount = convFeeGlAccount;
        }
    }
}
