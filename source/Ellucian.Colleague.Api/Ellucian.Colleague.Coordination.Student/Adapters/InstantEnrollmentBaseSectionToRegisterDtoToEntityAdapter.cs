// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment section to register dto to the corresponding entity
    /// </summary>

    public class InstantEnrollmentBaseSectionToRegisterDtoToEntityAdapter :
        BaseAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister,
            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>
    {
        public InstantEnrollmentBaseSectionToRegisterDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Converts a InstantEnrollmentRegistrationBaseSectionToRegister DTO to a InstantEnrollmentRegistrationBaseSectionToRegister entity
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister"/> to convert</param>
        /// <returns>the corresponding <see cref="Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister"/></returns>
        public override Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister MapToType(Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister(
                source.SectionId,
                source.AcademicCredits)
            {
                MarketingSource = source.MarketingSource,
                RegistrationReason = source.RegistrationReason
            };
        }
    }
}
