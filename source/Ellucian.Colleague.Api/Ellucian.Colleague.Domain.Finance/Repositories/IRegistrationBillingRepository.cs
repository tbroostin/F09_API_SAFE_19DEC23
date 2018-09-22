// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    public interface IRegistrationBillingRepository
    {
        /// <summary>
        /// Get a registration payment control by ID
        /// </summary>
        /// <param name="id">Payment control ID</param>
        /// <returns>Registration payment control</returns>
        RegistrationPaymentControl GetPaymentControl(string id);

        /// <summary>
        /// Get all payment controls for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>List of registration payment controls</returns>
        IEnumerable<RegistrationPaymentControl> GetStudentPaymentControls(string studentId);

        /// <summary>
        /// Accept the terms and conditions for a registration
        /// </summary>
        /// <param name="acceptance">The registration acceptance information</param>
        /// <returns>The updated registration approval</returns>
        RegistrationTermsApproval ApproveRegistrationTerms(PaymentTermsAcceptance acceptance);

        /// <summary>
        /// Get a registration billing by ID
        /// </summary>
        /// <param name="id">Registration billing ID</param>
        /// <returns>The registration billing</returns>
        RegistrationBilling GetRegistrationBilling(string id);

        /// <summary>
        /// Get registration billing items by ID
        /// </summary>
        /// <param name="ids">List of item IDs</param>
        /// <returns>List of registration billing items</returns>
        IEnumerable<RegistrationBillingItem> GetRegistrationBillingItems(IEnumerable<string> ids);

        /// <summary>
        /// Update a payment control record
        /// </summary>
        /// <param name="regPmtControl">The payment control to be updated</param>
        /// <returns>The updated payment control</returns>
        RegistrationPaymentControl UpdatePaymentControl(RegistrationPaymentControl regPmtControl);

        /// <summary>
        /// Get payment requirements for a term, sorted by processing order
        /// </summary>
        /// <param name="termId">ID of the term for which to retrieve payment requirements</param>
        /// <returns>List of payment requirements</returns>
        IEnumerable<PaymentRequirement> GetPaymentRequirements(string termId);

        /// <summary>
        /// Starts a registration payment
        /// </summary>
        /// <param name="payment">Payment being posted</param>
        /// <returns>Payment provider information</returns>
        PaymentProvider StartRegistrationPayment(Payment payment);

        /// <summary>
        /// Get a registration approval
        /// </summary>
        /// <param name="id">ID of approval</param>
        /// <returns>The registration approval information</returns>
        RegistrationTermsApproval GetTermsApproval(string id);
    }
}
