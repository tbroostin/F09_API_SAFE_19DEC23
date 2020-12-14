// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using slf4net;
using Ellucian.Web.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment Proposed Registration  (sections and person details) Dto to Entity
    /// </summary>
    public class InstantEnrollmentProposedRegistrationDtoToEntityAdapter : 
        BaseAdapter< Ellucian.Colleague.Dtos.Student.InstantEnrollment.InstantEnrollmentProposedRegistration, Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistration>
    {
        public InstantEnrollmentProposedRegistrationDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Converts a InstantEnrollmentEcheckRegistration DTO to a InstantEnrollmentEcheckRegistration entity
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistration"/> to convert</param>
        /// <returns>The corrresponding <see cref="Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistration "/></returns>
        public override Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistration MapToType(Dtos.Student.InstantEnrollment.InstantEnrollmentProposedRegistration source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic demoEnt = null;
            // Only convert demographics information when a person ID is not provided
            if (source.PersonDemographic != null && string.IsNullOrEmpty(source.PersonId))
            {
                var demoAdapter = adapterRegistry.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPersonDemographic,
                    Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic>();
                demoEnt = demoAdapter.MapToType(source.PersonDemographic);
            }

            List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister> sectList =
                new List<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>();
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

            return new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistration(
                source.PersonId,
                demoEnt,
                source.AcademicProgram,
                source.Catalog,
                sectList
                )
            { EducationalGoal = source.EducationalGoal };
        }
    }

}