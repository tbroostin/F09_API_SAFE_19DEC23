// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment proposed registration result entity to the corresponding dto
    /// </summary>
    public class InstantEnrollmentStartPaymentGatewayRegistrationResultEntityToDtoAdapter : 
        AutoMapperAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult>
    {
        public InstantEnrollmentStartPaymentGatewayRegistrationResultEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }
    }
}
