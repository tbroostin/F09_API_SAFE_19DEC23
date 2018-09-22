// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class GraduationConfigurationEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.GraduationConfiguration, Dtos.Student.GraduationConfiguration>
    {
        public GraduationConfigurationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.GraduationQuestion, Dtos.Student.GraduationQuestion>();
        }
    }
}
