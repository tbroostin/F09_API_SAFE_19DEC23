/*Copyright 2021 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    /// <summary>
    ///  Custom Adapter for EmployeeLeavePlan Entity to EmployeeLeavePlan Dto
    /// </summary>
    public class EmployeeLeavePlanEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeLeavePlan, Dtos.HumanResources.EmployeeLeavePlan>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency for EmployeeLeaveTransaction
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public EmployeeLeavePlanEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.EmployeeLeaveTransaction, Dtos.HumanResources.EmployeeLeaveTransaction>();
        }
    }
}
