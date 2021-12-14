// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class CourseCatalogConfigurationEntityToDto3Adapter : AutoMapperAdapter<Domain.Student.Entities.CourseCatalogConfiguration, Dtos.Student.CourseCatalogConfiguration3>
    {
        public CourseCatalogConfigurationEntityToDto3Adapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.CatalogFilterOption, Dtos.Student.CatalogFilterOption3>();
            AddMappingDependency<Domain.Student.Entities.CatalogSearchResultHeaderOption, Dtos.Student.CatalogSearchResultHeaderOption>();
            AddMappingDependency<Domain.Student.Entities.SelfServiceCourseCatalogSearchView, Dtos.Student.SelfServiceCourseCatalogSearchView>();
            AddMappingDependency<Domain.Student.Entities.SelfServiceCourseCatalogSearchResultView, Dtos.Student.SelfServiceCourseCatalogSearchResultView>();
        }
    }
}
