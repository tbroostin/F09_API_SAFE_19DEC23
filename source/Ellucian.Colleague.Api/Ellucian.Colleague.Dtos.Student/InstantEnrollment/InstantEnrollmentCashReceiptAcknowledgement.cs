// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// a cash receipt for an instant enrollment registration payment
    /// </summary>
    public class InstantEnrollmentCashReceiptAcknowledgement
    {
        /// <summary>
        /// ID of the cash receipt
        /// </summary>
        public string CashReceiptsId { get; set; }

        /// <summary>
        /// Receipt reference number
        /// </summary>
        public string ReceiptNo { get; set; }

        /// <summary>
        /// Date the receipt was created
        /// </summary>
        public Nullable<DateTime> ReceiptDate { get; set; }

        /// <summary>
        /// Time the receipt was created
        /// </summary>
        public Nullable<DateTime> ReceiptTime { get; set; }

        /// <summary>
        /// List containing the merchant's name and address
        /// </summary>
        public List<string> MerchantNameAddress { get; set; }

        /// <summary>
        /// Merchant's contact telephone number
        /// </summary>
        public string MerchantPhone { get; set; }

        /// <summary>
        /// Merchant's contact email address
        /// </summary>
        public string MerchantEmail { get; set; }

        /// <summary>
        /// Person ID of the receipt payer
        /// </summary>
        public string ReceiptPayerId { get; set; }

        /// <summary>
        /// Name of the receipt payer
        /// </summary>
        public string ReceiptPayerName { get; set; }

        /// <summary>
        /// The value of EC.PROC.STATUS if an EC.PAY.TRANS record was read. 
        /// Will be S for success, F for failure or C for canceled.
        /// As explained in comments in the routine if EC.PROC.STATUS is not S yet there is a CASH.RCPTS record with data, the CTX will return an "S" in this field.
        /// Only returned when a e-commerce transaction id is supplied.
        /// </summary>
        public EcommerceProcessStatus Status { get; set; }

        /// <summary>
        /// DMI Registry Username of the person that registered for the class(es) when a successful registration occurs for a new person.
        /// Only returned when the person registering is a new user (what about e-commerece transactin id check (will e-check return??).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Any error messages generated while creating the new user login. 
        /// The login is created after registration and payment, so registration and payment will still be in effect even if an error occurred generating the login.
        /// Only returned when a e-commerce transaction id is supplied.
        /// </summary>
        public List<string> UsernameCreationErrors { get; set; }

        /// <summary>
        /// List of <see cref="ConvenienceFee">convenience fees</see> paid on this receipt
        /// </summary>
        public List<ConvenienceFee> ConvenienceFees { get; set; }

        /// <summary>
        /// List of <see cref="PaymentMethod">PaymentMethod</see> on this receipt
        /// </summary>
        public List<PaymentMethod> PaymentMethods { get; set; }

        /// <summary>
        /// List of <see cref="InstantEnrollmentRegistrationPaymentGatewayRegisteredSection">RegisteredSections</see> for this registration receipt
        /// </summary>
        public List<InstantEnrollmentRegistrationPaymentGatewayRegisteredSection> RegisteredSections { get; set; }

        /// <summary>
        /// List of <see cref="InstantEnrollmentRegistrationPaymentGatewayFailedSection">Failed Section Registrations</see> for this registration receipt
        /// </summary>
        public List<InstantEnrollmentRegistrationPaymentGatewayFailedSection> FailedSections { get; set; }

        /// <summary>
        /// InstantEnrollmentCashReceiptAcknowledgement constructor
        /// </summary>
        public InstantEnrollmentCashReceiptAcknowledgement()
        {
            MerchantNameAddress = new List<string>();
            ConvenienceFees = new List<ConvenienceFee>();
            PaymentMethods = new List<PaymentMethod>();
            RegisteredSections = new List<InstantEnrollmentRegistrationPaymentGatewayRegisteredSection>();
            FailedSections = new List<InstantEnrollmentRegistrationPaymentGatewayFailedSection>();
        }
    }
}
