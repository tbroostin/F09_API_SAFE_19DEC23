// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class PlannedCourseToPlannedCourse2DtoAdapter : AutoMapperAdapter<PlannedCourse, Dtos.Student.DegreePlans.PlannedCourse2>
    {
        public PlannedCourseToPlannedCourse2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Dtos.Student.DegreePlans.PlannedCourse2 MapToType(PlannedCourse Source)
        {
            var pcDto = base.MapToType(Source);
            pcDto.AddedOn = Source.AddedOn.HasValue ? Source.AddedOn.Value.DateTime : (DateTime?)null;
            return pcDto;
        }
    }
}
