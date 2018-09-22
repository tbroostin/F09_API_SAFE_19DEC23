// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for ProxyWorkflowGroup entity->Dto mapping
    /// </summary>
    public class ProxyWorkflowGroupEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.ProxyWorkflowGroup, Dtos.Base.ProxyWorkflowGroup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyWorkflowGroupEntityAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public ProxyWorkflowGroupEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.ProxyWorkflow, Dtos.Base.ProxyWorkflow>();
        }
    }
}
