/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.HumanResources;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IBenefitsEnrollmentConfigurationService
    {
        Task<BenefitsEnrollmentConfiguration> GetBenefitsEnrollmentConfigurationAsync();
    }
}
