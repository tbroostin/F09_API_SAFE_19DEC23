// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment Zero Cost Registration sections and person details from the Dto to the Entity
    /// </summary>
    public class InstantEnrollmentZeroCostRegistrationDtoToEntityAdapter :
        BaseAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistration>
    {

        public InstantEnrollmentZeroCostRegistrationDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Converts a InstantEnrollmentZeroCostRegistration DTO to a InstantEnrollmentZeroCostRegistration entity
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration"/> to convert</param>
        /// <returns>The corrresponding <see cref="Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistration "/></returns>
        public override Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistration MapToType(Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("InstantEnrollmentZeroCostRegistration source is required.");
            }

            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic demoEnt = null;
            if (source.PersonDemographic != null && string.IsNullOrEmpty(source.PersonId))
            {
                var demoAdapter = adapterRegistry.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic,
                    Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic>();
                demoEnt = demoAdapter.MapToType(source.PersonDemographic);
            }
            // If we know the person ID already, we still want to use the email address (if provided) in the request
            if (source.PersonDemographic != null && !string.IsNullOrEmpty(source.PersonDemographic.EmailAddress) && !string.IsNullOrEmpty(source.PersonId))
            {
                demoEnt = new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic(source.PersonDemographic.FirstName, source.PersonDemographic.LastName)
                {
                    EmailAddress = source.PersonDemographic.EmailAddress
                };
            }

            var sectList = new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>();
            if (source.ProposedSections != null)
            {
                var sectAdapter = adapterRegistry.GetAdapter<
                        Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister,
                        Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>();
                var sections = source.ProposedSections
                    .Select(x => sectAdapter.MapToType(x))
                    .ToList();
                if (sections != null)
                {
                    sectList.AddRange(sections);
                }
            }

            return new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistration(
                source.PersonId,
                demoEnt,
                source.AcademicProgram,
                source.Catalog,
                sectList
                )
            {
                EducationalGoal = source.EducationalGoal,
            };
        }

    }
}
