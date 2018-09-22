//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for FinancialDocumentTypes services
    /// </summary>
    public interface IFinancialDocumentTypesService : IBaseService
    {

        /// <summary>
        /// Gets all financial-document-types
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="FinancialDocumentTypes">financialDocumentTypes</see> objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialDocumentTypes>> GetFinancialDocumentTypesAsync(bool bypassCache = false);

        /// <summary>
        /// Get a financialDocumentTypes by guid.
        /// </summary>
        /// <param name="guid">Guid of the financialDocumentTypes in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="FinancialDocumentTypes">financialDocumentTypes</see></returns>
        Task<Ellucian.Colleague.Dtos.FinancialDocumentTypes> GetFinancialDocumentTypesByGuidAsync(string guid, bool bypassCache = false);


    }
}
