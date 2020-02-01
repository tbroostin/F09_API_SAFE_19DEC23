// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class PlannedCourseWarningEntityAdaptor : AutoMapperAdapter<PlannedCourseWarning, Dtos.Student.DegreePlans.PlannedCourseWarning>
    {
        public PlannedCourseWarningEntityAdaptor(IAdapterRegistry adapterRegistry, ILogger logger) 
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Requisite, Dtos.Student.Requisite>();
            AddMappingDependency<SectionRequisite, Dtos.Student.SectionRequisite>();
        }
    }
}
