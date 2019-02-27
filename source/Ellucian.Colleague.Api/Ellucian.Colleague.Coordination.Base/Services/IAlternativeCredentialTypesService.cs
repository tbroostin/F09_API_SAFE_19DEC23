//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for AlternativeCredentialTypes services
    /// </summary>
    public interface IAlternativeCredentialTypesService : IBaseService
    {
          
        /// <summary>
        /// Gets all alternative-credential-types
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="AlternativeCredentialTypes">alternativeCredentialTypes</see> objects</returns>
         Task<IEnumerable<Dtos.AlternativeCredentialTypes>> GetAlternativeCredentialTypesAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a alternativeCredentialTypes by guid.
        /// </summary>
        /// <param name="guid">Guid of the alternativeCredentialTypes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="AlternativeCredentialTypes">alternativeCredentialTypes</see></returns>
        Task<Dtos.AlternativeCredentialTypes> GetAlternativeCredentialTypesByGuidAsync(string guid, bool bypassCache = true);
            
    }
}