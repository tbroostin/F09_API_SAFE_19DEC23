// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class CatalogSortType2ToEntityAdapter : AutoMapperAdapter<Dtos.Student.CatalogSortType2, Domain.Student.Entities.CatalogSortType>
    {
        public CatalogSortType2ToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Dtos.Student.CatalogSortType2, Domain.Student.Entities.CatalogSortType>();
        }
    }
}
