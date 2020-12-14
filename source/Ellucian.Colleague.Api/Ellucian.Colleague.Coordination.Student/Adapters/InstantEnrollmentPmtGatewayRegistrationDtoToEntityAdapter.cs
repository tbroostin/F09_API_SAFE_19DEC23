// Copyright 2019 Ellucian Company L.P. and its affiliates.
using slf4net;
using Ellucian.Web.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment payment gateway registration dto to the corresponding entity
    /// </summary>
    public class InstantEnrollmentPmtGatewayRegistrationDtoToEntityAdapter :
        BaseAdapter<Ellucian.Colleague.Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration, Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration>
    {

        public InstantEnrollmentPmtGatewayRegistrationDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Converts a InstantEnrollmentEcheckRegistration DTO to a InstantEnrollmentEcheckRegistration entity
        /// </summary>
        /// <param name="source">The <see cref="Ellucian.Colleague.Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration"/> to convert</param>
        /// <returns>The corrresponding <see cref="Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration "/></returns>
        public override Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration MapToType(Ellucian.Colleague.Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration source)
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

            // If we know the person ID already, we still want to use the email address (if provided) in the request
            if (source.PersonDemographic != null && !string.IsNullOrEmpty(source.PersonDemographic.EmailAddress) && !string.IsNullOrEmpty(source.PersonId))
            {
                demoEnt = new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonDemographic(source.PersonDemographic.FirstName, source.PersonDemographic.LastName)
                {
                    EmailAddress = source.PersonDemographic.EmailAddress
                };
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

            return new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration(
                source.PersonId,
                demoEnt,
                source.AcademicProgram,
                source.Catalog,
                sectList,
                source.PaymentAmount,
                source.PaymentMethod,
                source.ReturnUrl,
                source.GlDistribution,
                source.ProviderAccount,
                source.ConvenienceFeeDesc,
                source.ConvenienceFeeAmount,
                source.ConvenienceFeeGlAccount
                )
            { EducationalGoal = source.EducationalGoal };
        }

    }
}

