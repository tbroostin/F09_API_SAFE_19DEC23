// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for Emergency Information entity->Dto mapping
    /// </summary>
    public class ProxyConfigurationEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.ProxyConfiguration, Ellucian.Colleague.Dtos.Base.ProxyConfiguration>
    {
        private IAdapterRegistry AdapterRegistry;
        private ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyConfigurationEntityAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public ProxyConfigurationEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) 
        {
            AddMappingDependency<Domain.Base.Entities.ProxyWorkflowGroup, Dtos.Base.ProxyWorkflowGroup>();
            AddMappingDependency<Domain.Base.Entities.DemographicField, Dtos.Base.DemographicField>();
            AddMappingDependency<Domain.Base.Entities.ProxyAndUserPermissionsMap, Dtos.Base.ProxyAndUserPermissionsMap>();
        }
    }
}
