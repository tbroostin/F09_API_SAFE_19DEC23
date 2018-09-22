// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using slf4net;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    // Complex objects require additional dependency mappings
    public class CourseEntityToCourse2DtoAdapter : AutoMapperAdapter<Course, Ellucian.Colleague.Dtos.Student.Course2>
    {
        public CourseEntityToCourse2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requisite, Ellucian.Colleague.Dtos.Student.Requisite>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.LocationCycleRestriction, Ellucian.Colleague.Dtos.Student.LocationCycleRestriction>();
        }
    }
}
