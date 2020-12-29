// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert RetentionAlertClosedCase Entity to RetentionAlertClosedCase DTO
    /// </summary>
    public class RetentionAlertClosedCasesByReasonEntityToDtoAdapter : AutoMapperAdapter<Domain.Base.Entities.RetentionAlertClosedCasesByReason, Dtos.Student.RetentionAlertClosedCasesByReason>
    {
        public RetentionAlertClosedCasesByReasonEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Base.Entities.RetentionAlertClosedCase, Dtos.Student.RetentionAlertClosedCase>();           
        }        
    }
}