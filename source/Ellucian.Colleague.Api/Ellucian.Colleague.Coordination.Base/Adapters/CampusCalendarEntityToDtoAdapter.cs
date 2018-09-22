using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// CampusCalendar Entity to DTO Adapter
    /// </summary>
    [RegisterType]
    public class CampusCalendarEntityToDtoAdapter : AutoMapperAdapter<Domain.Base.Entities.CampusCalendar, Dtos.Base.CampusCalendar>
    {
        /// <summary>
        /// Constructor adds mapping dependencies
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public CampusCalendarEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.SpecialDay, Dtos.Base.SpecialDay>();
        }
    }
}
