// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Adapter for REQUIREDDOCUMENTCONFIGURATION entity
    /// </summary>
    public class RequiredDocumentConfigurationAdapter : AutoMapperAdapter<RequiredDocumentConfiguration, Ellucian.Colleague.Dtos.Base.RequiredDocumentConfiguration>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredDocumentConfigurationAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public RequiredDocumentConfigurationAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<WebSortField, Ellucian.Colleague.Dtos.Base.WebSortField>();
        }
    }
}
