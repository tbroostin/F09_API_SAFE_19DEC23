// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class CourseCatalogConfigurationEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.CourseCatalogConfiguration, Dtos.Student.CourseCatalogConfiguration>
    {
        public CourseCatalogConfigurationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.CatalogFilterOption, Dtos.Student.CatalogFilterOption>();
        }
    }
}
