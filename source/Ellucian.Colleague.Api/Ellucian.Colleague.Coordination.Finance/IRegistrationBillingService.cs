// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Payments;

namespace Ellucian.Colleague.Coordination.Finance
{
    /// <summary>
    /// Registration billing service
    /// </summary>
    public interface IRegistrationBillingService
    {
        /// <summary>
        /// Get a registration payment control by ID
        /// </summary>
        /// <param name="id">Payment control ID</param>
        /// <returns>RegistrationPaymentControl DTO</returns>
        RegistrationPaymentControl GetPaymentControl(string id);

        /// <summary>
        /// Get the incomplete payment controls for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>List of RegistrationPaymentControl DTOs</returns>
        IEnumerable<RegistrationPaymentControl> GetStudentPaymentControls(string studentId);

        /// <summary>
        /// Get a document for a registration payment control
        /// </summary>
        /// <param name="id">Payment control ID</param>
        /// <param name="documentId">Document ID</param>
        /// <returns>TextDocument DTO</returns>
        TextDocument GetPaymentControlDocument(string id, string documentId);

        /// <summary>
        /// Approve the payment terms for a registration
        /// </summary>
        /// <param name="acceptance">The PaymentTermsAcceptance DTO</param>
        /// <returns>The updated RegistrationTermsApproval DTO</returns>
        RegistrationTermsApproval ApproveRegistrationTerms(PaymentTermsAcceptance acceptance);

        /// <summary>
        /// Approve the payment terms for a registration
        /// </summary>
        /// <param name="acceptance">The PaymentTermsAcceptance2 DTO</param>
        /// <returns>The updated RegistrationTermsApproval2 DTO</returns>
        RegistrationTermsApproval2 ApproveRegistrationTerms2(PaymentTermsAcceptance2 acceptance);

        /// <summary>
        /// Gets the payment options for a student
        /// </summary>
        /// <param name="paymentControlId">ID of the payment control record for the student/term</param>
        /// <returns>ImmediatePaymentOptions DTO outlining the payment options available to the student</returns>
        ImmediatePaymentOptions GetPaymentOptions(string paymentControlId);

        /// <summary>
        /// Updates a payment control record
        /// </summary>
        /// <param name="rpcDto">Registration Payment Control DTO to update</param>
        /// <returns>Updated Registration Payment Control DTO</returns>
        RegistrationPaymentControl UpdatePaymentControl(RegistrationPaymentControl rpcDto);

        /// <summary>
        /// Get the payment summary for a payment control, pay method, and payment amount
        /// </summary>
        /// <param name="id">Registration payment control ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="amount">Total payment amount</param>
        /// <returns>List of payments to be made</returns>
        IEnumerable<Payment> GetPaymentSummary(string id, string payMethod, decimal amount);

        /// <summary>
        /// Start a registration payment
        /// </summary>
        /// <param name="paymentDto">Registration Payment DTO</param>
        /// <returns>Payment Provider DTO</returns>
        PaymentProvider StartRegistrationPayment(Payment paymentDto);

        /// <summary>
        /// Get a registration terms approval
        /// </summary>
        /// <param name="id">Approval ID</param>
        /// <returns>RegistrationTermsApproval DTO</returns>
        RegistrationTermsApproval GetTermsApproval(string id);

        /// <summary>
        /// Get a registration terms approval
        /// </summary>
        /// <param name="id">Approval ID</param>
        /// <returns>RegistrationTermsApproval2 DTO</returns>
        RegistrationTermsApproval2 GetTermsApproval2(string id);

        /// <summary>
        /// Get a proposed payment plan
        /// </summary>
        /// <param name="payControlId">ID of a payment control record</param>
        /// <param name="receivableType">Receivable Type for proposed payment plan</param>
        /// <returns>PaymentPlan DTO</returns>
        PaymentPlan GetProposedPaymentPlan(string payControlId, string receivableType);
    }
}
