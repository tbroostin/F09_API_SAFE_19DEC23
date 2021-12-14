// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class GraduationConfigurationEntityToDto2Adapter : AutoMapperAdapter<Domain.Student.Entities.GraduationConfiguration, Dtos.Student.GraduationConfiguration2>
    {
        public GraduationConfigurationEntityToDto2Adapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.GraduationQuestion, Dtos.Student.GraduationQuestion>();
        }
    }
}
