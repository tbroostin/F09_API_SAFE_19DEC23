/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    /// <summary>
    ///  Custom Adapter for EmployeeSummary Entity to EmployeeSummary Dto
    /// </summary>
    public class EmployeeSummaryEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeSummary, Dtos.HumanResources.EmployeeSummary>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency for EmployeeSummary
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public EmployeeSummaryEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.PersonPosition, Dtos.HumanResources.PersonPosition>();
            AddMappingDependency<Domain.HumanResources.Entities.PersonPositionWage, Dtos.HumanResources.PersonPositionWage>();
            AddMappingDependency<Domain.HumanResources.Entities.PersonEmploymentStatus, Dtos.HumanResources.PersonEmploymentStatus>();
            AddMappingDependency<Domain.Base.Entities.PersonBase, Dtos.HumanResources.HumanResourceDemographics>();
        }
    }
}
