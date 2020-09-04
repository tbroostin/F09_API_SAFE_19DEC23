// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert RetentionAlertCaseDetail entity to RetentionAlertCaseDetail DTO
    /// </summary>
    public class RetentionAlertCaseDetailEntityToDtoAdapter : AutoMapperAdapter<Domain.Base.Entities.RetentionAlertCaseDetail, Dtos.Student.RetentionAlertCaseDetail>
    {
        public RetentionAlertCaseDetailEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.RetentionAlertCaseHistory, Dtos.Student.RetentionAlertCaseHistory>();         
            AddMappingDependency<Domain.Base.Entities.RetentionAlertCaseRecipEmail, Dtos.Student.RetentionAlertCaseRecipEmail>();            
            AddMappingDependency<Domain.Base.Entities.RetentionAlertCaseReassignmentDetail, Dtos.Student.RetentionAlertCaseReassignmentDetail>();
        }
    }
}
