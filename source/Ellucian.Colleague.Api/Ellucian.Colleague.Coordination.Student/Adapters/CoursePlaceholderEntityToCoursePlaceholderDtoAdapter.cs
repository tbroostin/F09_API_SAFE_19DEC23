// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for converting a <see cref="CoursePlaceholder"/> to a <see cref="Dtos.Student.DegreePlans.CoursePlaceholder"/>
    /// </summary>
    public class CoursePlaceholderEntityToCoursePlaceholderDtoAdapter : AutoMapperAdapter<CoursePlaceholder, Ellucian.Colleague.Dtos.Student.DegreePlans.CoursePlaceholder>
    {
        /// <summary>
        /// Maps a <see cref="CoursePlaceholder"/> to a <see cref="Dtos.Student.DegreePlans.CoursePlaceholder"/>
        /// </summary>
        public CoursePlaceholderEntityToCoursePlaceholderDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) 
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requirements.AcademicRequirementGroup, Ellucian.Colleague.Dtos.Student.Requirements.AcademicRequirementGroup>();
        }

    }
}
