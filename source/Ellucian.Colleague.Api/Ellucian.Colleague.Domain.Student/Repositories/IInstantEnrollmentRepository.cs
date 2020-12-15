// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for an InstantEnrollmentRepository.
    /// </summary>
    public interface IInstantEnrollmentRepository
    {
        /// <summary>
        /// Accepts demographic information and a list of course sections, performs a mock registration, and return list of sections with the associated cost
        /// </summary>
        /// <param name="proposedRegistration">A <see cref="InstantEnrollmentProposedRegistration"/> containing the information necessary to estimate costs.</param>
        /// <returns>A <see cref="InstantEnrollmentProposedRegistrationResult"/> containing the estimated costs of the sections.</returns>
        Task<InstantEnrollmentProposedRegistrationResult> GetProposedRegistrationResultAync(InstantEnrollmentProposedRegistration criteria);

        /// <summary>
        /// Registers a student for classes, creating the student if necessary for a zero cost registration.
        /// </summary>
        /// <param name="zeroCostRegistration">A <see cref="InstantEnrollmentZeroCostRegistration"/> containing the information necessary to register for classes.</param>
        /// <returns>A <see cref=InstantEnrollmentZeroCostRegistrationResult"/> containing the results of the registration.</returns>
        Task<InstantEnrollmentZeroCostRegistrationResult> GetZeroCostRegistrationResultAsync(InstantEnrollmentZeroCostRegistration zeroCostRegistration);

        /// <summary>
        /// Registers a student for classes, creating the student if necessary, and pays the cost of the classes with an electronic transfer.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentEcheckRegistration"/> containing the information necessary to register and pay for classes.</param>
        /// <returns>A <see cref=InstantEnrollmentEcheckRegistrationResult"/> containing the results of the registration.</returns>
        Task<InstantEnrollmentEcheckRegistrationResult> GetEcheckRegistrationResultAsync(InstantEnrollmentEcheckRegistration criteria);

        /// <summary>
        /// Starts a payment gateway transaction for instant enrollment. 
        /// Starting a payment gateway transaction involves creating an EC.PAY.TRANS record and returning a url to redirect the user to the external
        /// payment provider.
        /// In the case of instant enrollment the CTX also registers the student and creates the person and student records if needed.
        /// Call backs to other CTXs from the Payment Gateway web server will complete the interaction after the user either completes or cancels the payment
        /// at the external provider. Garbage collection WAGC will cancel the interaction if a timeout elapses before the user completes the interaction at the 
        /// external payment provider. Either through garbage collection or a callback from the payment gateway web server, the person and student creation as 
        /// well as the registration will be reversed if the interaction is canceled.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentPaymentGatewayRegistration"/> containing the information necessary to register and pay for classes.</param>
        /// 
        /// <returns>A <see cref="InstantEnrollmentStartPaymentGatewayRegistrationResult"/> containing any error messages or the payment provider URL to which to redirect the user.</returns>
        Task<InstantEnrollmentStartPaymentGatewayRegistrationResult> StartInstantEnrollmentPaymentGatewayTransactionAsync(InstantEnrollmentPaymentGatewayRegistration proposedRegistration);

        /// <summary>
        /// Retrieves instant enrollment payment acknowledgement paragraph text for a given <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/></param>
        /// <returns>Instant enrollment payment acknowledgement paragraph text</returns>
        Task<IEnumerable<string>> GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(InstantEnrollmentPaymentAcknowledgementParagraphRequest request);

        /// <summary>
        /// Query persons matching the criteria using the ELF duplicate checking criteria configured for Instant Enrollment.
        /// </summary>
        /// <param name="criteria">The <see cref="PersonMatchCriteriaInstantEnrollment">criteria</see> to use when searching for people</param>
        /// <returns>Result of a person biographic/demographic matching inquiry for Instant Enrollment</returns>
        Task<InstantEnrollmentPersonMatchResult> GetMatchingPersonResultsInstantEnrollmentAsync(PersonMatchCriteriaInstantEnrollment criteria);

        /// <summary>
        /// Retrieves instant enrollment payment cash receipt acknowledgement for a given <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/></param>
        /// <returns>the cash receipt and sections for an instant enrollment payment</returns>
        Task<InstantEnrollmentCashReceiptAcknowledgement> GetInstantEnrollmentCashReceiptAcknowledgementAsync(InstantEnrollmentCashReceiptAcknowledgementRequest request);
    }
}
