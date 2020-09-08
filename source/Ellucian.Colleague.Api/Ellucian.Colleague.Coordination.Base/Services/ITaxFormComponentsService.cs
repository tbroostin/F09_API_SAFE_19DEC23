//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for TaxFormComponents services
    /// </summary>
    public interface ITaxFormComponentsService : IBaseService
    {
          
        /// <summary>
        /// Gets all tax-form-components
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="TaxFormComponents">taxFormComponents</see> objects</returns>
         Task<IEnumerable<Ellucian.Colleague.Dtos.TaxFormComponents>> GetTaxFormComponentsAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a taxFormComponents by guid.
        /// </summary>
        /// <param name="guid">Guid of the taxFormComponents in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="TaxFormComponents">taxFormComponents</see></returns>
        Task<Ellucian.Colleague.Dtos.TaxFormComponents> GetTaxFormComponentsByGuidAsync(string guid, bool bypassCache = true);
    }
}
