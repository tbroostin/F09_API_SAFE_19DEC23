//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for ContractTypes services
    /// </summary>
    public interface IContractTypesService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.ContractTypes>> GetContractTypesAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.ContractTypes> GetContractTypesByGuidAsync(string id);
    }
}