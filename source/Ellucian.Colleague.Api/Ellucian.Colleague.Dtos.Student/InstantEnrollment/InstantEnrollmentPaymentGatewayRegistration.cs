// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the information needed to start a payment gateway transaction to pay for an instant enrollment registration.
    /// The process of starting the payment gateway transaction includes registering the student, and created person
    /// and student records as needed. This contains the section, demographic, and financial information necessary to 
    /// perform those functions.
    /// </summary>
    public class InstantEnrollmentPaymentGatewayRegistration
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
        /// The Colleague self-service URL to which the user should be directed after interacting with the external payment provider.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// The GL distribution associated with the provider account and convenience fee
        /// </summary>
        public string GlDistribution { get; set; }

        /// <summary>
        /// The ecommerce provider account.
        /// </summary>
        public string ProviderAccount { get; set; }

        /// <summary>
        /// The description of any convenience fee.
        /// </summary>
        public string ConvenienceFeeDesc { get; set; }

        /// <summary>
        /// The amount of any convenience fee.
        /// </summary>
        public decimal? ConvenienceFeeAmount { get; set; }

        /// <summary>
        /// The GL account of any convenience fee.  Must be supplied if the convenience fee is charged.
        /// </summary>
        public string ConvenienceFeeGlAccount { get; set; }

    }
}
