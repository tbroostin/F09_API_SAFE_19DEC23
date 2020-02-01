//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for tax form box codes service
    /// </summary>
    public interface ITaxFormBoxCodesService : IBaseService
    {
        /// <summary>
        /// Gets all box codes configured for tax forms
        /// </summary>        
        /// <returns>Collection of <see cref="Ellucian.Colleague.Dtos.Base.TaxFormBoxCodes">boxcodes</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.TaxFormBoxCodes>> GetAllTaxFormBoxCodesAsync();


    }
}
