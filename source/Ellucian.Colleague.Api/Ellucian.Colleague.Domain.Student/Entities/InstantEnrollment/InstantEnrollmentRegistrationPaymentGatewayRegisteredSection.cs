// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Contains the section and related information for an Instant Enrollment section in which an 
    /// individual has successfully enrolled and paid with using a payment gateway
    /// </summary>
    [Serializable]
    public class InstantEnrollmentRegistrationPaymentGatewayRegisteredSection : InstantEnrollmentRegistrationBaseRegisteredSection
    {
        /// <summary>
        /// The number of academic credits for which the student registered (if not CEUS)
        /// </summary>
        public decimal? AcademicCredits { get; private set; }

        /// <summary>
        /// The number of continuing education credits for which the student registered (if not academic credits)
        /// </summary>
        public decimal? Ceus { get; private set; }

        /// <summary>
        /// Creates a new <see cref="InstantEnrollmentRegistrationPaymentGatewayRegisteredSection"/>
        /// </summary>
        /// <param name="sectionId">The unique identifier of the section that was successfully registered</param>
        /// <param name="sectionCost">The cost of the section that was successfully registered</param>
        /// <param name="academicCredits">The number of academic credits for which the student registered</param>
        /// <param name="ceus">The number of continuing education credits for which the student registered</param>
        public InstantEnrollmentRegistrationPaymentGatewayRegisteredSection(string sectionId, decimal? sectionCost, decimal? academicCredits, decimal? ceus) :
               base(sectionId, sectionCost)
        {
            AcademicCredits = academicCredits;
            Ceus = ceus;
        }
    }
}
