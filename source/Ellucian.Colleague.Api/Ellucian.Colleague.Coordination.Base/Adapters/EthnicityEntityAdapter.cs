// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates an <see cref="Ethnicity"/> from an <see cref="Domain.Base.Entities.Ethnicity"/>
    /// </summary>
    public class EthnicityEntityAdapter : AutoMapperAdapter<Domain.Base.Entities.Ethnicity, Dtos.Base.Ethnicity>
    {
        /// <summary>
        /// Instantiates a new <see cref="EthnicityEntityAdapter"/>
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public EthnicityEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.EthnicityType, Dtos.Base.EthnicityType>();
        }
    }
}