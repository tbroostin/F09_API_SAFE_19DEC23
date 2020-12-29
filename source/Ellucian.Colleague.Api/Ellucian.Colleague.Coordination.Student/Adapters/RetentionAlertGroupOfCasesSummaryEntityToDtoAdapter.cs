// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert RetentionAlertGroupOfCasesSummary entity to RetentionAlertGroupOfCasesSummary DTO
    /// </summary>
    public class RetentionAlertGroupOfCasesSummaryEntityToDtoAdapter : AutoMapperAdapter<Domain.Base.Entities.RetentionAlertGroupOfCasesSummary, Dtos.Student.RetentionAlertGroupOfCasesSummary>
    {
        public RetentionAlertGroupOfCasesSummaryEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.RetentionAlertGroupOfCases, Dtos.Student.RetentionAlertGroupOfCases>();
        }
    }
}
