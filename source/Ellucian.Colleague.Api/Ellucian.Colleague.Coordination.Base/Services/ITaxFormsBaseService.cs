//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for TaxForms services
    /// </summary>
    public interface ITaxFormsBaseService : IBaseService
    {
          
        /// <summary>
        /// Gets all tax-forms
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="TaxForms">taxForms</see> objects</returns>
         Task<IEnumerable<Dtos.TaxForms>> GetTaxFormsAsync(bool bypassCache = false);
               
        /// <summary>
        /// Get a taxForms by guid.
        /// </summary>
        /// <param name="guid">Guid of the taxForms in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="TaxForms">taxForms</see></returns>
        Task<Dtos.TaxForms> GetTaxFormsByGuidAsync(string guid, bool bypassCache = true);
    }
}
