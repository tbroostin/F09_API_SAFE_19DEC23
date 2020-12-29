// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class CourseCatalogConfigurationEntityToDto2Adapter : AutoMapperAdapter<Domain.Student.Entities.CourseCatalogConfiguration, Dtos.Student.CourseCatalogConfiguration2>
    {
        public CourseCatalogConfigurationEntityToDto2Adapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.CatalogFilterOption, Dtos.Student.CatalogFilterOption2>();
        }
    }
}
