// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class DegreePlanApprovalEntityAdapter : AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlanApproval, Dtos.Student.DegreePlans.DegreePlanApproval>
    {
        public DegreePlanApprovalEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override Dtos.Student.DegreePlans.DegreePlanApproval MapToType(Domain.Student.Entities.DegreePlans.DegreePlanApproval Source)
        {
            var dpaDto = base.MapToType(Source);
            // for some unknown reason the DateTimeOffsetToDateTimeAdapter assigns a default DateTime
            // to the target object's Date property. So we still need to manually handle this mapping.
            dpaDto.Date = Source.Date.DateTime;
            return dpaDto;
        }
    }
}
