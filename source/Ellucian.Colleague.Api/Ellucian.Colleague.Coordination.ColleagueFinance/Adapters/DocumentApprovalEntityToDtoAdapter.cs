// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{

    /// <summary>
    /// Adapter for a Document Approval entity to Dto mapping.
    /// </summary>
    public class DocumentApprovalEntityToDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.DocumentApproval, Dtos.ColleagueFinance.DocumentApproval>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentApprovalEntityToDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public DocumentApprovalEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.ApprovalDocument, Dtos.ColleagueFinance.ApprovalDocument>();
            AddMappingDependency<Domain.ColleagueFinance.Entities.ApprovalItem, Dtos.ColleagueFinance.ApprovalItem>();
        }
    }
}
