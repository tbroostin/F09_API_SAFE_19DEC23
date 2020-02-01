/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    /// <summary>
    ///  Custom Adapter for EmployeeCompensation Entity to EmployeeCompensation Dto
    /// </summary>
    public class EmployeeCompensationEntityToDtoAdapter: AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeCompensation,Dtos.HumanResources.EmployeeCompensation>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency for EmployeeCompensation
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public EmployeeCompensationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            :base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.EmployeeBended, Dtos.HumanResources.EmployeeBended>();
            AddMappingDependency<Domain.HumanResources.Entities.EmployeeTax, Dtos.HumanResources.EmployeeTax>();
            AddMappingDependency<Domain.HumanResources.Entities.EmployeeStipend, Dtos.HumanResources.EmployeeStipend>();
            AddMappingDependency<Domain.HumanResources.Entities.EmployeeCompensationError, Dtos.HumanResources.EmployeeCompensationError>();
        }
    }
}
