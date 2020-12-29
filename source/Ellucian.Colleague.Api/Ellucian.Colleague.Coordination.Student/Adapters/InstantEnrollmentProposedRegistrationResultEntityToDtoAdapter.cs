// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// This is to map Instant Enrollment proposed registration result entitiy to dto
    /// </summary>
    public class InstantEnrollmentProposedRegistrationResultEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentProposedRegistrationResult>
    {
        public InstantEnrollmentProposedRegistrationResultEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection, Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection>();
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage, Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage>();
        }
    }
}
