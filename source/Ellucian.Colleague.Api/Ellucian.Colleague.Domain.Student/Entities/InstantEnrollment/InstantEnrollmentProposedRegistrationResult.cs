// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]

    /// <summary>
    /// Contains the results of a trial registration of given classes for a given individual.
    /// </summary>
    public class InstantEnrollmentProposedRegistrationResult : InstantEnrollmentRegistrationBaseResult
    {

        public InstantEnrollmentProposedRegistrationResult(
            bool errorOccurred,
            IEnumerable<InstantEnrollmentRegistrationBaseRegisteredSection> sections,
            IEnumerable<InstantEnrollmentRegistrationBaseMessage> messages) : 
               base(errorOccurred, sections, messages)
        {

        }
    }
}
