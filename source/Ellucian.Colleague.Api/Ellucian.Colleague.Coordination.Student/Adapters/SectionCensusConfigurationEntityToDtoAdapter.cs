// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class SectionCensusConfigurationEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.SectionCensusConfiguration, Dtos.Student.SectionCensusConfiguration>
    {
        public SectionCensusConfigurationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.CensusDatePositionSubmission, Dtos.Student.CensusDatePositionSubmission>();
        }
    }
}
