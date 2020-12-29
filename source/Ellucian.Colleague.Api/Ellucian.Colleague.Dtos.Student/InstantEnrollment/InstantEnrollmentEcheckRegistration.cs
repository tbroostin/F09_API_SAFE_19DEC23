// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the section, demographic, and financial information necessary to create a student,
    /// register for classes, and pay for them with an electronic check.
    /// </summary>
    public class InstantEnrollmentEcheckRegistration
    {
        /// <summary>
        /// The identifier of the person proposing to register.  The person
        /// may not yet be known to the system.  Not required.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The demographic attributes necessary to find or create a new person.
        /// </summary>
        public InstantEnrollmentPersonDemographic PersonDemographic { get; set; }

        /// <summary>
        /// The academic program of the proposed student. 
        /// </summary>
        public string AcademicProgram { get; set; }

        /// <summary>
        /// The list of sections for which the student would like to register. 
        /// </summary>
        public List<InstantEnrollmentRegistrationBaseSectionToRegister> ProposedSections { get; set; }
        /// <summary>
        /// The catalog to use for the the academic program.
        /// </summary>
        public string Catalog { get; set; }

        /// <summary>
        /// The purpose for taking these classes.
        /// </summary>
        public string EducationalGoal { get; set; }

        /// <summary>
        /// The expected cost of the registered sections.  Includes the convenience fee amount.
        /// </summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// Specifies how the payment will be made.
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// The ecommerce provider account.
        /// </summary>
        public string ProviderAccount { get; set; }

        /// <summary>
        /// The name on the bank account.
        /// </summary>
        public string BankAccountOwner { get; set; }

        /// <summary>
        /// The routing number of the bank account.
        /// </summary>
        public string BankAccountRoutingNumber { get; set; }

        /// <summary>
        /// The bank account number.
        /// </summary>
        public string BankAccountNumber { get; set; }

        /// <summary>
        /// The number of the check written against the account.
        /// </summary>
        public string BankAccountCheckNumber { get; set; }

        /// <summary>
        /// The type of the bank account (e.g., checking or savings).
        /// </summary>
        public string BankAccountType { get; set; }

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
        public string ConvenienceFeeDesc { get; set; }

        /// <summary>
        /// The amount of any convenience fee.
        /// </summary>
        public decimal? ConvenienceFeeAmount { get; set; }

        /// <summary>
        /// The GL account of any convenience fee.
        /// </summary>
        public string ConvenienceFeeGlAccount { get; set; }

        /// <summary>
        /// The email address of the payer.
        /// </summary>
        public string PayerEmailAddress { get; set; }

        /// <summary>
        /// The mailing address of the payer.
        /// </summary>
        public string PayerAddress { get; set; }

        /// <summary>
        /// The mailing city of the payer.
        /// </summary>
        public string PayerCity { get; set; }

        /// <summary>
        /// The mailing state or province of the payer.
        /// </summary>
        public string PayerState { get; set; }

        /// <summary>
        /// The mailing postal code of the payer.
        /// </summary>
        public string PayerPostalCode { get; set; }
    }
}
