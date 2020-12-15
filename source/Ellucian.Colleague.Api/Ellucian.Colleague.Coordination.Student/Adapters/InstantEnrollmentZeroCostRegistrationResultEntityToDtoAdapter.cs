// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment zero cost registration result entitiy to the zero cost registration result dto
    /// </summary>
    public class InstantEnrollmentZeroCostRegistrationResultEntityToDtoAdapter : 
        AutoMapperAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult>
    {
        public InstantEnrollmentZeroCostRegistrationResultEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection, Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection>();
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage, Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage>();
        }
    }



}
