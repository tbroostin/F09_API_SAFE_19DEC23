/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    /// <summary>
    /// Custom Adapter for EmployeeBenefitsEnrollmentInfo Dto to EmployeeBenefitsEnrollmentInfo Entity
    /// </summary>
    public class EmployeeBenefitsEnrollmentInfoDtoToEntityAdapter : AutoMapperAdapter<Dtos.HumanResources.EmployeeBenefitsEnrollmentInfo, Domain.HumanResources.Entities.EmployeeBenefitsEnrollmentInfo>
    {
        public EmployeeBenefitsEnrollmentInfoDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.HumanResources.EmployeeBenefitsEnrollmentDetail, Domain.HumanResources.Entities.EmployeeBenefitsEnrollmentDetail>();
        }
    }
}
