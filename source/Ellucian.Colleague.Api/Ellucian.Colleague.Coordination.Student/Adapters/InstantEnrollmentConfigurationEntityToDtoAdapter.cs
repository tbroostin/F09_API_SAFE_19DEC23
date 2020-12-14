// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class InstantEnrollmentConfigurationEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration, Dtos.Student.InstantEnrollment.InstantEnrollmentConfiguration>
    {
        public InstantEnrollmentConfigurationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.AcademicProgramOption, Dtos.Student.InstantEnrollment.AcademicProgramOption>();
            AddMappingDependency<Domain.Student.Entities.InstantEnrollment.AddNewStudentProgramBehavior, Dtos.Student.InstantEnrollment.AddNewStudentProgramBehavior>();
            AddMappingDependency<Domain.Student.Entities.CatalogFilterOption, Dtos.Student.CatalogFilterOption3>();
            AddMappingDependency<Domain.Base.Entities.DemographicField, Dtos.Base.DemographicField>();
        }
    }
}
