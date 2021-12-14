// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert PortalUpdatedCoursesResult entity to PortalUpdatedCoursesResult DTO
    /// </summary>
    public class PortalUpdatedCoursesResultEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.Portal.PortalUpdatedCoursesResult, Dtos.Student.Portal.PortalUpdatedCoursesResult>
    {
        public PortalUpdatedCoursesResultEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Student.Entities.Portal.PortalCourse, Dtos.Student.Portal.PortalCourse>();
        }
    }


}
