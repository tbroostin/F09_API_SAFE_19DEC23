//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for BeneficiaryPreferenceTypes services
    /// </summary>
    public interface IBeneficiaryPreferenceTypesService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes>> GetBeneficiaryPreferenceTypesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes> GetBeneficiaryPreferenceTypesByGuidAsync(string id);
    }
}
