// Copyright 2020 Ellucian Company L.P. and its affiliates.using System;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    public class DocumentApprovalRequestDtoToEntityAdapter : AutoMapperAdapter<Dtos.ColleagueFinance.DocumentApprovalRequest, DocumentApprovalRequest>
    {
        public DocumentApprovalRequestDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.ColleagueFinance.ApprovalDocumentRequest, ApprovalDocumentRequest>();
            AddMappingDependency<Dtos.ColleagueFinance.ApprovalItem, ApprovalItem>();
        }
    }
}
