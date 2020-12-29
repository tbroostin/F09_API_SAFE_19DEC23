// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    [Serializable]
    /// <summary>
    /// Contains the information necessary to calculate the cost of the
    /// proposed section registration for a person having the given demographic
    /// characteristics
    /// </summary>
    public class InstantEnrollmentProposedRegistration : InstantEnrollmentRegistrationBaseRegistration
    {
        public InstantEnrollmentProposedRegistration(
            string personId,
            InstantEnrollmentPersonDemographic personDemographics,
            string acadProgram,
            string catalog,
            List<InstantEnrollmentRegistrationBaseSectionToRegister> sections) :
               base(personId, personDemographics, acadProgram, catalog, sections)
        {

        }
    }
}
