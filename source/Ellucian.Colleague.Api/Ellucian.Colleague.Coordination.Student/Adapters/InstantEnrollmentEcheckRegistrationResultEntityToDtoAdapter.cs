﻿// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an Instant Enrollment proposed registration result entity to the corresponding dto
    /// </summary>
    public class InstantEnrollmentEcheckRegistrationResultEntityToDtoAdapter : 
        AutoMapperAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistrationResult>
    {
        public InstantEnrollmentEcheckRegistrationResultEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection, Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection>();
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage, Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseMessage>();
        }
    }
}
