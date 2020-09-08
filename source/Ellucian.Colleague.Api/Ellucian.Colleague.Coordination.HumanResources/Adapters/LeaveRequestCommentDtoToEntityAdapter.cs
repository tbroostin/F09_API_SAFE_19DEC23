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
    public class LeaveRequestCommentDtoToEntityAdapter : BaseAdapter<Dtos.HumanResources.LeaveRequestComment, Domain.HumanResources.Entities.LeaveRequestComment>
    {
        public LeaveRequestCommentDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
           
        }

        public override Domain.HumanResources.Entities.LeaveRequestComment MapToType(Dtos.HumanResources.LeaveRequestComment Source)
        {
            if (Source == null)
            {
                throw new ArgumentNullException("Source");
            }

            var commentEntity = new Domain.HumanResources.Entities.LeaveRequestComment(
                Source.Id,
                Source.LeaveRequestId,
                Source.EmployeeId,
                Source.Comments,
                Source.CommentAuthorName);

            return commentEntity;
        }
    }
}
