using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class PortalEventsAndRemindersQueryCriteriaDtoToEntityAdapter : BaseAdapter<Dtos.Student.Portal.PortalEventsAndRemindersQueryCriteria, Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria>
    {
        public PortalEventsAndRemindersQueryCriteriaDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {

        }

        public override Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria MapToType(Dtos.Student.Portal.PortalEventsAndRemindersQueryCriteria source)
        {
            Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria entity = null;
            if (source != null)
            {
                entity = new Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria(source.StartDate,
                    source.EndDate,
                    source.EventTypeCodes);
            } else
            {
                entity = new Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria(null,
                    null,
                    null);
            }
            return entity;
        }

    }
}
