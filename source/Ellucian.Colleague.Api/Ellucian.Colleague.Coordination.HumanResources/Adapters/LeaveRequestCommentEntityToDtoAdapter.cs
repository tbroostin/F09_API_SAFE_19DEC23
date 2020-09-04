/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
 
   [RegisterType]
    public class LeaveRequestCommentEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.LeaveRequestComment, Dtos.HumanResources.LeaveRequestComment>
    {
        public LeaveRequestCommentEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.Timestamp, Dtos.Base.Timestamp>();
        }
    }
}
