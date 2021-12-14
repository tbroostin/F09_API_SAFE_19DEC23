// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class PortalStudentPreferredCourseSectionUpdateResultDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.Portal.PortalStudentPreferredCourseSectionUpdateResult, Dtos.Student.Portal.PortalStudentPreferredCourseSectionUpdateResult>
    {
        public PortalStudentPreferredCourseSectionUpdateResultDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Domain.Student.Entities.Portal.PortalStudentPreferredCourseSectionUpdateStatus, Dtos.Student.Portal.PortalStudentPreferredCourseSectionUpdateStatus>();
        }
    }
}
