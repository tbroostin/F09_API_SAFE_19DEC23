// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Required methods.
    /// </summary>
    public interface IColleagueFinanceTaxFormPdfDataRepository
    {
        /// <summary>
        /// Returns a FormT4aPdfData.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the T4A.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a T4A tax form</param>
        /// <returns>A FormT4aPdfData object.</returns>
        Task<FormT4aPdfData> GetFormT4aPdfDataAsync(string personId, string recordId);

        /// <summary>
        /// Get the pdf data for tax form 1099-MISC.
        /// </summary>
        /// <param name="personId">ID of the person assigned to and requesting the 1099-MISC.</param>
        /// <param name="recordId">ID of the record containing the pdf data for a 1099-MISC tax form</param>
        /// <returns>The pdf data for tax form 1099-MISC</returns>
        Task<Form1099MIPdfData> GetForm1099MiPdfDataAsync(string personId, string recordId);
    }
}