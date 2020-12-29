// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]
    /// <summary>
    /// Contains the information common to all Instant Enrollment registration endpoint results, 
    /// as well as the cash receipt that was created.
    /// </summary>
    public class InstantEnrollmentEcheckRegistrationResult : InstantEnrollmentRegistrationBaseResult
    {
        /// <summary>
        /// The unique identifier of the person that registered for the class(es) when a successful registration occurs.
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// The cash receipt that was created after paying for registration.
        /// </summary>
        public string CashReceipt { get; private set; }

        /// <summary>
        /// DMI Registry Username of the person that registered for the class(es) when a successful registration occurs. 
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Creates a new <see cref="InstantEnrollmentEcheckRegistrationResult"/>
        /// </summary>
        /// <param name="errorOccurred">Flag indicating whether or not an error occurred.</param>
        /// <param name="sections">Collection of <see cref="InstantEnrollmentRegistrationBaseRegisteredSection"/></param>
        /// <param name="messages">Collection of <see cref="InstantEnrollmentRegistrationBaseMessage"/></param>
        /// <param name="cashReceipt">The cash receipt that was created after paying for registration.</param>
        /// <param name="userName">DMI Registry Username of the person that registered for the class(es) when a successful registration occurs.</param>
        public InstantEnrollmentEcheckRegistrationResult(
           bool errorOccurred,
           IEnumerable<InstantEnrollmentRegistrationBaseRegisteredSection> sections,
           IEnumerable<InstantEnrollmentRegistrationBaseMessage> messages,
           string personId,
           string cashReceipt,
           string userName) : 
            base(errorOccurred, sections, messages)
        {
            PersonId = personId;
            CashReceipt = cashReceipt;
            UserName = userName;
        }
    }
}
