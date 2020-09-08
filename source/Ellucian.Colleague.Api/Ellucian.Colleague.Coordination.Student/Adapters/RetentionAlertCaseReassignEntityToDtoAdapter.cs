// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Convert RetentionAlertCaseReassign DTO to RetentionAlertCaseReassign entity
    /// </summary>
    public class RetentionAlertCaseReassignDtoToEntityAdapter : AutoMapperAdapter<Dtos.Student.RetentionAlertWorkCaseReassign,Domain.Base.Entities.RetentionAlertWorkCaseReassign>
    {
        public RetentionAlertCaseReassignDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.Student.RetentionAlertCaseReassignmentDetail, Domain.Base.Entities.RetentionAlertCaseReassignmentDetail>();
           
        }        
    }
}