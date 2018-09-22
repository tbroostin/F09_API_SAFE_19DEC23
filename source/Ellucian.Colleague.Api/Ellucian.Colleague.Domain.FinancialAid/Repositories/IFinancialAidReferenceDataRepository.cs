//Copyright 2013-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.FinancialAid.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// Provides read-only access to fundamental Financial Aid data
    /// </summary>
    public interface IFinancialAidReferenceDataRepository
    {
        /// <summary>
        /// Get a list of all Awards in Colleague
        /// </summary>
        IEnumerable<Award> Awards { get; }

        /// <summary>
        /// Get a list of all Award Year definitions in Colleague.
        /// </summary>
        IEnumerable<AwardYear> AwardYears { get; }

        /// <summary>
        /// Get a list of all Award Status definitions in Colleague
        /// </summary>
        IEnumerable<AwardStatus> AwardStatuses { get; }

        /// <summary>
        /// Get a list of all Award Types definitions in Colleague
        /// </summary>
        IEnumerable<AwardType> AwardTypes { get; }

        /// <summary>
        /// Get a list of all Award Periods 
        /// </summary>
        IEnumerable<AwardPeriod> AwardPeriods { get; }

        /// <summary>
        /// Get a list of all Award Categories in Colleague
        /// </summary>
        Task<IEnumerable<AwardCategory>> GetAwardCategoriesAsync();

        /// <summary>
        /// Get a list of all the Award Categories. This is the obsolete version. Use
        /// the Async method.
        /// </summary>
        [Obsolete("Obsolete as of Api version 1.8, use GetAwardCategoriesAsync")]
        IEnumerable<AwardCategory> AwardCategories { get; }

        /// <summary>
        /// Get all Helpful Links
        /// </summary>
        IEnumerable<Link> Links { get; }

        /// <summary>
        /// Get all BudgetComponents for all AwardYears
        /// </summary>
        IEnumerable<BudgetComponent> BudgetComponents { get; }

        /// <summary>
        /// Get all Checklist Items
        /// </summary>
        IEnumerable<ChecklistItem> ChecklistItems { get; }

        /// <summary>
        /// Get all AcademicProgressStatuses
        /// </summary>
        Task<IEnumerable<AcademicProgressStatus>> GetAcademicProgressStatusesAsync();

        /// <summary>
        /// Get all Academic Progress Appeal Codes
        /// </summary>
        Task<IEnumerable<AcademicProgressAppealCode>> GetAcademicProgressAppealCodesAsync();

        /// <summary>
        /// Gets all Award Letter configurations
        /// </summary>
        Task<IEnumerable<AwardLetterConfiguration>> GetAwardLetterConfigurationsAsync();

        /// <summary>
        /// Get a collection of financial aid award periods
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid award periods</returns>
        Task<IEnumerable<FinancialAidAwardPeriod>> GetFinancialAidAwardPeriodsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of financial aid fund categories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund categories</returns>
        Task<IEnumerable<FinancialAidFundCategory>> GetFinancialAidFundCategoriesAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of financial aid fund classifications
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid fund classifications</returns>
        Task<IEnumerable<FinancialAidFundClassification>> GetFinancialAidFundClassificationsAsync(bool ignoreCache);
        
        /// <summary>
        /// Get a collection of financial aid years
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid years</returns>
        Task<IEnumerable<FinancialAidYear>> GetFinancialAidYearsAsync(bool ignoreCache);

        /// <summary>
        /// Gets host country
        /// </summary>
        /// <returns></returns>
        Task<string> GetHostCountryAsync();

        
        /// <summary>
        /// Gets all financial aid explanations
        /// </summary>
        /// <returns>a set of FinanciaAidExplanation entities</returns>
        Task<IEnumerable<FinancialAidExplanation>> GetFinancialAidExplanationsAsync();
    }
}
