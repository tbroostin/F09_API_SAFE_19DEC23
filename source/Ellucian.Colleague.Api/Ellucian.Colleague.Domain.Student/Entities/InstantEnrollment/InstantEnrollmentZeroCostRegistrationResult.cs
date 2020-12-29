// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// Contains the results of a zero cost registration of given classes for a given individual.
    /// </summary>
    [Serializable]
    public class InstantEnrollmentZeroCostRegistrationResult : InstantEnrollmentRegistrationBaseResult
    {

        /// <summary>
        /// The unique identifier of the person that registered for the class(es) when a successful registration occurs.
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// DMI Registry Username of the person that registered for the class(es) when a successful registration occurs. 
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Creates a new <see cref="InstantEnrollmentZeroCostRegistrationResult"/>
        /// </summary>
        /// <param name="errorOccurred">Flag indicating whether or not an error occurred.</param>
        /// <param name="sections">Collection of <see cref="InstantEnrollmentRegistrationBaseRegisteredSection"/></param>
        /// <param name="messages">Collection of <see cref="InstantEnrollmentRegistrationBaseMessage"/></param>
        /// <param name="personId">The unique identifier of the person that registered for the class(es) when a successful registration occurs.</param>
        /// <param name="userName">DMI Registry Username of the person that registered for the class(es) when a successful registration occurs.</param>
        public InstantEnrollmentZeroCostRegistrationResult(
            bool errorOccurred,
            IEnumerable<InstantEnrollmentRegistrationBaseRegisteredSection> sections,
            IEnumerable<InstantEnrollmentRegistrationBaseMessage> messages,
            string personId,
            string userName) :
               base(errorOccurred, sections, messages)
        {
            PersonId = personId;
            UserName = userName;
        }
    }
}
