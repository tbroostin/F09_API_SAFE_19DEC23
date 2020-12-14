// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IInstantEnrollmentService : IBaseService
    {
        /// <summary>
        /// Accepts a list of proposed course sections, along with demographic information, and returns the anticipated cost of the sections.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentProposedRegistration"/> containing the information to evaluate.</param>
        /// <returns>A <see cref="InstantEnrollmentProposedRegistrationResult"/> containing the cost of the proposed sections.</returns>
        Task<InstantEnrollmentProposedRegistrationResult> ProposedRegistrationForClassesAsync(InstantEnrollmentProposedRegistration criteria);

        /// <summary>
        /// Accepts a list of course sections and demographic information and registers person for classes when the total cost for registration is zero.
        /// </summary>
        /// <param name="zeroCostRegistration">A <see cref="InstantEnrollmentZeroCostRegistration"/> containing the information for registration.</param>
        /// <returns>A <see cref="InstantEnrollmentZeroCostRegistrationResult"/> containing the results of the registration attempt.</returns>
        Task<InstantEnrollmentZeroCostRegistrationResult> ZeroCostRegistrationForClassesAsync(InstantEnrollmentZeroCostRegistration zeroCostRegistration);

        /// <summary>
        /// Accepts a list of course sections and demographic information, then registers the person for the sections and pays
        /// for them using electronic transfer.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentEcheckRegistration"/> containing the information to evaluate.</param>
        /// <returns>A <see cref="InstantEnrollmentEcheckRegistrationResult"/> containing the results of the registration attempt.</returns>
        Task<InstantEnrollmentEcheckRegistrationResult> EcheckRegistrationForClassesAsync(InstantEnrollmentEcheckRegistration criteria);

        /// <summary>
        /// Start an instant enrollment payment gateway transaction, which includes registering the student and creating the student if needed.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentPaymentGatewayRegistration"/> containing the information needed to start the payment gateway transaction.</param>
        /// <returns></returns>
        Task<InstantEnrollmentStartPaymentGatewayRegistrationResult> StartInstantEnrollmentPaymentGatewayTransaction(InstantEnrollmentPaymentGatewayRegistration criteria);

        /// <summary>
        /// Retrieves instant enrollment payment acknowledgement paragraph text for a given <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/></param>
        /// <returns>Instant enrollment payment acknowledgement paragraph text</returns>
        Task<IEnumerable<string>> GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(InstantEnrollmentPaymentAcknowledgementParagraphRequest request);

        /// <summary>
        /// Query persons matching the criteria using the ELF duplicate checking criteria configured for Instant Enrollment.
        /// </summary>
        /// <param name="person">The <see cref="PersonMatchCriteriaInstantEnrollment">criteria</see> to query by.</param>
        /// <returns>Result of a person biographic/demographic matching inquiry for Instant Enrollment</returns>
        Task<InstantEnrollmentPersonMatchResult> QueryPersonMatchResultsInstantEnrollmentByPostAsync(PersonMatchCriteriaInstantEnrollment criteria);

        /// <summary>
        /// Retrieves instant enrollment payment cash receipt acknowledgement for a given <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/></param>
        /// <returns>the cash receipt and sections for an instant enrollment payment</returns>
        Task<InstantEnrollmentCashReceiptAcknowledgement> GetInstantEnrollmentCashReceiptAcknowledgementAsync(InstantEnrollmentCashReceiptAcknowledgementRequest request);

        /// <summary>
        /// Retrieves the programs in which the specified student is enrolled.
        /// </summary>
        /// <param name="studentId">Student's ID</param>
        /// <param name="currentOnly">Boolean to indicate whether this request is for active student programs, or ended/past programs as well</param>
        /// <returns>All <see cref="Dtos.Student.StudentProgram2">Programs</see> in which the specified student is enrolled.</returns>
        Task<IEnumerable<Dtos.Student.StudentProgram2>> GetInstantEnrollmentStudentPrograms2Async(string studentId, bool currentOnly = true);
    }
}
