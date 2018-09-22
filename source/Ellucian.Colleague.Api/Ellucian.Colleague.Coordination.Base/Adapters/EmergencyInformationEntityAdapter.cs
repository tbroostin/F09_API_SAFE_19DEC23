// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters {

    /// <summary>
    /// Adapter for Emergency Information entity->Dto mapping
    /// </summary>
    public class EmergencyInformationEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.EmergencyInformation, Dtos.Base.EmergencyInformation> 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingEntityAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public EmergencyInformationEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) 
        {
            AddMappingDependency<Domain.Base.Entities.EmergencyContact, Dtos.Base.EmergencyContact>();
        }
    }
}
