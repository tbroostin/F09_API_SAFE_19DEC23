/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    /// <summary>
    ///  Custom Adapter for EmployeeBenefitsEnrollmentInfo Entity to EmployeeBenefitsEnrollmentInfo Dto
    /// </summary>
    public class EmployeeBenefitsEnrollmentInfoEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.EmployeeBenefitsEnrollmentInfo, Dtos.HumanResources.EmployeeBenefitsEnrollmentInfo>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency for EmployeeBenefitsEnrollmentDetail
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public EmployeeBenefitsEnrollmentInfoEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.EmployeeBenefitsEnrollmentDetail, Dtos.HumanResources.EmployeeBenefitsEnrollmentDetail>();
        }
    }
}
