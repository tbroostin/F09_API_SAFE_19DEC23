//Copyright 2017 Ellucian Company L.P.and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class SectionMeetingInstanceEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>
    {
        public SectionMeetingInstanceEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }
    }
}
