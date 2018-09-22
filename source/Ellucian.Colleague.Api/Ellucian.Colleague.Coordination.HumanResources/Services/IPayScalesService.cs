//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for PayScales services
    /// </summary>
    public interface IPayScalesService : IBaseService
    {
          
        /// <summary>
        /// Gets all pay-scales
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PayScales">payScales</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.PayScales>> GetPayScalesAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a payScales by guid.
        /// </summary>
        /// <param name="guid">Guid of the payScales in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PayScales">payScales</see></returns>
        Task<Ellucian.Colleague.Dtos.PayScales> GetPayScalesByGuidAsync(string guid, bool bypassCache = false);

            
    }
}
