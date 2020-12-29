// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class PlannedCourseWarningEntity2Adaptor : AutoMapperAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning2>
    {
        public PlannedCourseWarningEntity2Adaptor(IAdapterRegistry adapterRegistry, ILogger logger) 
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Requisite, Dtos.Student.Requisite>();
            AddMappingDependency<SectionRequisite, Dtos.Student.SectionRequisite>();
        }
    }
}
