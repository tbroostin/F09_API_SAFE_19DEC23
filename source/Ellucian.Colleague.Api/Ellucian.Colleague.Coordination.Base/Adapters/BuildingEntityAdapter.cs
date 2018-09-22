// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters {

    /// <summary>
    /// Adapter for BUILDING entity
    /// </summary>
    public class BuildingEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.Building, Dtos.Base.Building> 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingEntityAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public BuildingEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) 
        {
            AddMappingDependency<Domain.Base.Entities.Coordinate, Dtos.Base.Coordinate>();
        }
    }
}
