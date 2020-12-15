// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]
    /// <summary>
    /// Contains the section, demographic, and financial information necessary to create a student,
    /// register for classes, and pay for them with an electronic transfer.
    /// </summary>
    public class InstantEnrollmentEcheckRegistration : InstantEnrollmentRegistrationBaseRegistration
    {
        /// <summary>
        /// The expected cost of the registered sections.  Includes the convenience fee amount.
        /// </summary>
        public decimal PaymentAmount { get; private set; }

        /// <summary>
        /// Specifies how the payment will be made.
        /// </summary>
        public string PaymentMethod { get; private set; }

        /// <summary>
        /// The ecommerce provider account.
        /// </summary>
        public string ProviderAccount { get; private set; }

        /// <summary>
        /// The name on the bank account.
        /// </summary>
        public string BankAccountOwner { get; private set; }

        /// <summary>
        /// The routing number of the bank account.
        /// </summary>
        public string BankAccountRoutingNumber { get; private set; }

        /// <summary>
        /// The bank account number.
        /// </summary>
        public string BankAccountNumber { get; private set; }

        /// <summary>
        /// The number of the check written against the account.
        /// </summary>
        public string BankAccountCheckNumber { get; private set; }

        /// <summary>
        /// The type of the bank account (e.g., checking or savings).
        /// </summary>
        public string BankAccountType { get; private set; }

        /// <summary>
        /// The number on the government-issued identifcation (e.g., drivers license).
        /// </summary>
        public string GovernmentId { get; set; }

        /// <summary>
        /// The government entity issuing the identification.
        /// </summary>
        public string GovernmentIdState { get; set; }

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

        /// <summary>
        /// The email address of the payer.
        /// </summary>
        public string PayerEmailAddress { get; private set; }

        /// <summary>
        /// The mailing address of the payer.
        /// </summary>
        public string PayerAddress { get; private set; }

        /// <summary>
        /// The mailing city of the payer.
        /// </summary>
        public string PayerCity { get; private set; }

        /// <summary>
        /// The mailing state or province of the payer.
        /// </summary>
        public string PayerState { get; private set; }

        /// <summary>
        /// The mailing postal code of the payer.
        /// </summary>
        public string PayerPostalCode { get; private set; }

        public InstantEnrollmentEcheckRegistration(
            string personId,
            InstantEnrollmentPersonDemographic personDemographics,
            string acadProgram,
            string catalog,
            List<InstantEnrollmentRegistrationBaseSectionToRegister> sections,
            decimal payAmount,
            string payMethod,
            string providerAccount,
            string bankAccountOwner,
            string routingNumber,
            string bankAccountNumber,
            string bankAccountCheckNumber,
            string bankAccountType,
            string convFeeDesc,
            decimal? convFeeAmount,
            string convFeeGlAccount,
            string payerEmailAddress,
            string payerAddress,
            string payerCity,
            string payerState,
            string payerPostalCode
            ) : base(personId, personDemographics, acadProgram, catalog, sections)
        {
            // EcheckRegistration has additional requirements
            if (personDemographics != null)
            {
                if (String.IsNullOrEmpty(personDemographics.City))
                {
                    throw new ArgumentException("City cannot be null in personDemographics");
                }
                if (String.IsNullOrEmpty(personDemographics.State))
                {
                    throw new ArgumentException("State cannot be null in personDemographics");
                }
                if (String.IsNullOrEmpty(personDemographics.ZipCode))
                {
                    throw new ArgumentException("ZipCode cannot be null in personDemographics");
                }
            }

            // PaymentAmount
            if (payAmount == 0)
            {
                throw new ArgumentOutOfRangeException("payAmount", "Payment amount cannot be zero.");
            }
            PaymentAmount = payAmount;

            // PaymentMethod
            if (String.IsNullOrEmpty(payMethod))
            {
                throw new ArgumentNullException("payMethod");
            }
            PaymentMethod = payMethod;

            // ProviderAccount
            if (String.IsNullOrEmpty(providerAccount))
            {
                throw new ArgumentNullException("providerAccount");
            }
            ProviderAccount = providerAccount;

            // BankAccountOwner
            if (String.IsNullOrEmpty(bankAccountOwner))
            {
                throw new ArgumentNullException("bankAccountOwner");
            }
            BankAccountOwner = bankAccountOwner;

            // BankAccountRoutingNumber
            if (String.IsNullOrEmpty(routingNumber))
            {
                throw new ArgumentNullException("routingNumber");
            }
            BankAccountRoutingNumber = routingNumber;

            // BankAccountNumber
            if (String.IsNullOrEmpty(bankAccountNumber))
            {
                throw new ArgumentNullException("bankAccountNumber");
            }
            BankAccountNumber = bankAccountNumber;

            // BankAccountCheckNumber
            if (String.IsNullOrEmpty(bankAccountCheckNumber))
            {
                throw new ArgumentNullException("bankAccountCheckNumber");
            }
            BankAccountCheckNumber = bankAccountCheckNumber;

            // BankAccountType
            if (String.IsNullOrEmpty(bankAccountType))
            {
                throw new ArgumentNullException("bankAccountType");
            }
            BankAccountType = bankAccountType;

            // PayerEmailAddress
            if (String.IsNullOrEmpty(payerEmailAddress))
            {
                throw new ArgumentNullException("payerEmailAddress");
            }
            PayerEmailAddress = payerEmailAddress;

            // PayerAddress
            if (String.IsNullOrEmpty(payerAddress))
            {
                throw new ArgumentNullException("payerAddress");
            }
            PayerAddress = payerAddress;

            // PayerCity
            if (String.IsNullOrEmpty(payerCity))
            {
                throw new ArgumentNullException("payerCity");
            }
            PayerCity = payerCity;

            // PayerState
            if (String.IsNullOrEmpty(payerState))
            {
                throw new ArgumentNullException("payerState");
            }
            PayerState = payerState;

            // PayerPostalCode
            if (String.IsNullOrEmpty(payerPostalCode))
            {
                throw new ArgumentNullException("payerPostalCode");
            }
            PayerPostalCode = payerPostalCode;

            // If there is a convenience fee, there must be a GL number
            if (convFeeAmount != null && String.IsNullOrEmpty(convFeeGlAccount))
            {
                throw new ArgumentException("A convenience fee requires a corresponding GL account.");
            }

            // ConvenienceFeeAmount
            ConvenienceFeeAmount = convFeeAmount;

            // ConvenienceFeeDesc
            ConvenienceFeeDesc = convFeeDesc;

            // ConvenienceFeeGlAccount
            ConvenienceFeeGlAccount = convFeeGlAccount;
        }
    }
}
