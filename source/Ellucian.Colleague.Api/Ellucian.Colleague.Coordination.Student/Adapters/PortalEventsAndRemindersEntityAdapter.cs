// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Converts a <see cref="Domain.Student.Entities.Portal.PortalEventsAndReminders"/> entity to a <see cref="Dtos.Student.Portal.PortalEventsAndReminders"/> DTO
    /// </summary>
    public class PortalEventsAndRemindersEntityAdapter : AutoMapperAdapter<Domain.Student.Entities.Portal.PortalEventsAndReminders, Dtos.Student.Portal.PortalEventsAndReminders>
    {
        public PortalEventsAndRemindersEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Student.Entities.Portal.PortalEvent, Dtos.Student.Portal.PortalEvent>();
            AddMappingDependency<Domain.Student.Entities.Portal.PortalReminder, Dtos.Student.Portal.PortalReminder>();
        }
    }
}
