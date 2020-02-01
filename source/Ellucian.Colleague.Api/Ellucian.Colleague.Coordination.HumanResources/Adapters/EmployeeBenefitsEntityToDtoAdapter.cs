/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
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
    ///  Custom Adapter for EmployeeBenefits Entity to EmployeeBenefits Dto
    /// </summary>
    public class EmployeeBenefitsEntityToDtoAdapter: AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeBenefits, Dtos.HumanResources.EmployeeBenefits>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency for EmployeeBenefits
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public EmployeeBenefitsEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            :base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.CurrentBenefit, Dtos.HumanResources.CurrentBenefit>();
        }
    }
}
