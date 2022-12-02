// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class SectionCensusConfiguration2EntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.SectionCensusConfiguration2, Dtos.Student.SectionCensusConfiguration2>
    {
        public SectionCensusConfiguration2EntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.CensusDatePositionSubmission, Dtos.Student.CensusDatePositionSubmission>();
        }
    }
}
