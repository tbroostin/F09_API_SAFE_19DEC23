/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.HumanResources;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IPayStatementConfigurationService
    {
        /// <summary>
        /// Gets pay statement configuration
        /// </summary>
        /// <returns></returns>
        Task<PayStatementConfiguration> GetPayStatementConfigurationAsync();
    }
}
