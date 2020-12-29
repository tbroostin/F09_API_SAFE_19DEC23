// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class DegreePlanNoteEntityAdapter : AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlanNote, Dtos.Student.DegreePlans.DegreePlanNote>
    {
        public DegreePlanNoteEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Dtos.Student.DegreePlans.DegreePlanNote MapToType(Domain.Student.Entities.DegreePlans.DegreePlanNote Source)
        {
            var dpnDto = base.MapToType(Source);
            // for some unknown reason the DateTimeOffsetToDateTimeAdapter assigns a default DateTime
            // to the target object's Date property. So we still need to manually handle this mapping.
            dpnDto.Date = Source.Date.HasValue ? Source.Date.Value.DateTime : (DateTime?)null;
            return dpnDto;
        }
    }
}
