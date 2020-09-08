using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    /// <summary>
    /// Custom Adapter for LeaveRequest DTO to LeaveRequest Entity
    /// </summary>
    public class LeaveRequestDtoToEntityAdapter : AutoMapperAdapter<Dtos.HumanResources.LeaveRequest, Domain.HumanResources.Entities.LeaveRequest>
    {
        public LeaveRequestDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.HumanResources.LeaveRequestDetail, Domain.HumanResources.Entities.LeaveRequestDetail>();
            AddMappingDependency<Dtos.HumanResources.LeaveRequestComment, Domain.HumanResources.Entities.LeaveRequestComment>();
        }
    }
}
