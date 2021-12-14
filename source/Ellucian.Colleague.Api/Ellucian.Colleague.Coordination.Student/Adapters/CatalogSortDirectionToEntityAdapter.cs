// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class CatalogSortDirectionToEntityAdapter : AutoMapperAdapter<Dtos.Student.CatalogSortDirection, Domain.Student.Entities.CatalogSortDirection>
    {
        public CatalogSortDirectionToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Dtos.Student.CatalogSortDirection, Domain.Student.Entities.CatalogSortDirection>();
        }
    }
}
