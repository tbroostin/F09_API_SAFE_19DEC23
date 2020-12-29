// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{

    /// <summary>
    /// Adapter for a Document Approval entity to Dto mapping.
    /// </summary>
    public class DocumentApprovalResponseEntityToDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.DocumentApprovalResponse, Dtos.ColleagueFinance.DocumentApprovalResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentApprovalResponseEntityToDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public DocumentApprovalResponseEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.ApprovalDocumentResponse, Dtos.ColleagueFinance.ApprovalDocumentResponse>();
        }
    }
}