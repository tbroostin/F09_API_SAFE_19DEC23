// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for mapping SectionGradingComplete entity to SectionGradingComplete dto
    /// </summary>    
    public class SectionMidtermGradingCompleteEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMidtermGradingComplete, Ellucian.Colleague.Dtos.Student.SectionMidtermGradingComplete>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SectionMidtermGradingCompleteEntityToDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public SectionMidtermGradingCompleteEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
                : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.GradingCompleteIndication, Ellucian.Colleague.Dtos.Student.GradingCompleteIndication>();
        }

    }
}
