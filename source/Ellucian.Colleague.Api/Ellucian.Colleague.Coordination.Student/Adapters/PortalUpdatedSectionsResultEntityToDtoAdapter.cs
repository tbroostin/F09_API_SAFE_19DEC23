// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert PortalUpdatedSectionResult entity to PortalUpdatedSectionResult DTO
    /// </summary>
    public class PortalUpdatedSectionsResultEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.Portal.PortalUpdatedSectionsResult, Dtos.Student.Portal.PortalUpdatedSectionsResult>
    {
        public PortalUpdatedSectionsResultEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Student.Entities.Portal.PortalSectionMeeting, Dtos.Student.Portal.PortalSectionMeeting>();
            AddMappingDependency<Domain.Student.Entities.Portal.PortalBookInformation, Dtos.Student.Portal.PortalBookInformation>();
            AddMappingDependency<Domain.Student.Entities.Portal.PortalSection, Dtos.Student.Portal.PortalSection>();
        }
    }
}
