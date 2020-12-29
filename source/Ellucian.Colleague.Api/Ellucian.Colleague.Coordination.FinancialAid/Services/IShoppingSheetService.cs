/*Copyright 2015-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Defines methods to interact with ShoppingSheet resources
    /// </summary>
    public interface IShoppingSheetService
    {
        /// <summary>
        /// Get a collection of Student specific Shopping Sheets
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get shopping sheets</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of ShoppingSheets</returns>
        Task<IEnumerable<ShoppingSheet>> GetShoppingSheetsAsync(string studentId, bool getActiveYearsOnly = false);

        /// <summary>
        /// Get a collection of Student specific Shopping Sheets
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="getActiveYearsOnly"></param>
        /// <returns>A list of ShoppingSheets</returns>
        Task<IEnumerable<ShoppingSheet2>> GetShoppingSheets2Async(string studentId, bool getActiveYearsOnly = false);
    }
}
