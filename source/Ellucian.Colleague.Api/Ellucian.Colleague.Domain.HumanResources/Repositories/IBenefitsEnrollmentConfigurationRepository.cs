/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    /// <summary>
    /// Interface for IBenefitsEnrollmentConfiguration repository methods
    /// </summary>
    public interface IBenefitsEnrollmentConfigurationRepository
    {
        /// <summary>
        /// Returns BenefitsEnrollmentConfiguration object
        /// </summary>
        /// <returns></returns>
        Task<BenefitsEnrollmentConfiguration> GetBenefitsEnrollmentConfigurationAsync();
    }
}
