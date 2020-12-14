// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// Contains the section and related information for an Instant Enrollment section in which an 
    /// individual has successfully enrolled and paid with using a payment gateway
    /// </summary>
    public class InstantEnrollmentRegistrationPaymentGatewayRegisteredSection : InstantEnrollmentRegistrationBaseRegisteredSection
    {
        /// <summary>
        /// The number of academic credits for which the student registered
        /// </summary>
        public decimal? AcademicCredits { get; set; }

        /// <summary>
        /// The number of continuing education credits for which the student registered
        /// </summary>
        public decimal? Ceus { get; set; }

    }

}
