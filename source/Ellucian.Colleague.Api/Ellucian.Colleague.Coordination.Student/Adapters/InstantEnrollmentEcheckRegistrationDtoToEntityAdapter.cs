// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment echeck registration criteria dto to the corresponding entity
    /// </summary>
    public class InstantEnrollmentEcheckRegistrationDtoToEntityAdapter : 
        BaseAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistration,
            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistration>
    {
        public InstantEnrollmentEcheckRegistrationDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Converts a InstantEnrollmentEcheckRegistration DTO to a InstantEnrollmentEcheckRegistration entity
        /// </summary>
        /// <param name="source">The <see cref="Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistration"/> to convert</param>
        /// <returns>The corrresponding <see cref="Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistration "/></returns>
        public override Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistration MapToType(Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistration source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic demoEnt = null;
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
                var sections = source.ProposedSections.Select(x => sectAdapter.MapToType(x)).ToList();
                sectList.AddRange(sections);
            }

            return new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistration(
                source.PersonId,
                demoEnt,
                source.AcademicProgram,
                source.Catalog,
                sectList,
                source.PaymentAmount,
                source.PaymentMethod,
                source.ProviderAccount,
                source.BankAccountOwner,
                source.BankAccountRoutingNumber,
                source.BankAccountNumber,
                source.BankAccountCheckNumber,
                source.BankAccountType,
                source.ConvenienceFeeDesc,
                source.ConvenienceFeeAmount,
                source.ConvenienceFeeGlAccount,
                source.PayerEmailAddress,
                source.PayerAddress,
                source.PayerCity,
                source.PayerState,
                source.PayerPostalCode
                )
            {
                EducationalGoal = source.EducationalGoal,
                GovernmentId = source.GovernmentId,
                GovernmentIdState = source.GovernmentIdState,
            };
        }
    }
}
