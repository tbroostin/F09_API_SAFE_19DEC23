/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    /// <summary>
    /// Custom Adapter for LeaveRequest Entity to LeaveRequest DTO
    /// </summary>
    public class LeaveRequestEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.LeaveRequest, Dtos.HumanResources.LeaveRequest>
    {
        public LeaveRequestEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.LeaveRequestDetail, Dtos.HumanResources.LeaveRequestDetail>();

            AddMappingDependency<Domain.HumanResources.Entities.LeaveRequestComment, Dtos.HumanResources.LeaveRequestComment>();
        }
    }
}
