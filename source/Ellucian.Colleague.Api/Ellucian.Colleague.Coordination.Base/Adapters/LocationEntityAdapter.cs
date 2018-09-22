// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for LOCATION entity
    /// </summary>
    public class LocationEntityAdapter : AutoMapperAdapter<Location, Ellucian.Colleague.Dtos.Base.Location> {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationEntityAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public LocationEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) {
                AddMappingDependency<Coordinate, Ellucian.Colleague.Dtos.Base.Coordinate>();
        }
    }
}
