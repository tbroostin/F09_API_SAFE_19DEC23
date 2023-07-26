/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    /// <summary>
    ///  Custom Adapter for OrgChartEmployee Entity to OrgChartEmployee Dto
    /// </summary>
    public class OrgChartEmployeeEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.OrgChartEmployee, Dtos.HumanResources.OrgChartEmployee>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency for OrgChartEmployee
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public OrgChartEmployeeEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            :base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.OrgChartEmployeeName, Dtos.HumanResources.OrgChartEmployeeName>();
        }
    }
}
