/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    public class BenefitsEnrollmentPackageEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeBenefitsEnrollmentPackage, Dtos.HumanResources.EmployeeBenefitsEnrollmentPackage>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency for EmployeeBenefitsEnrollmentPackageEntity
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public BenefitsEnrollmentPackageEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.EmployeeBenefitType, Dtos.HumanResources.EmployeeBenefitType>();
            AddMappingDependency<Domain.HumanResources.Entities.EnrollmentPeriodBenefit, Dtos.HumanResources.EnrollmentPeriodBenefit>();
        }
    }
}
