//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for EmploymentFrequencies services
    /// </summary>
    public interface IEmploymentFrequenciesService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentFrequencies>> GetEmploymentFrequenciesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.EmploymentFrequencies> GetEmploymentFrequenciesByGuidAsync(string id);
    }
}
