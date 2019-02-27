//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    /// <summary>
    /// Interface for PositionClassifications services
    /// </summary>
    public interface IPositionClassificationsService : IBaseService
    {
          
        /// <summary>
        /// Gets all position-classifications
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="PositionClassifications">positionClassifications</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.PositionClassification>> GetPositionClassificationsAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a positionClassifications by guid.
        /// </summary>
        /// <param name="guid">Guid of the positionClassifications in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="PositionClassifications">positionClassifications</see></returns>
        Task<Ellucian.Colleague.Dtos.PositionClassification> GetPositionClassificationsByGuidAsync(string guid, bool bypassCache = false);

            
    }
}
