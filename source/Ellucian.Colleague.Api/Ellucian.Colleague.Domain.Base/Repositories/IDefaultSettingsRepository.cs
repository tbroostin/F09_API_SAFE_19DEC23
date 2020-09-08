/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IDefaultSettingsRepository : IEthosExtended
    {
        /// <summary>
        /// Get a collection of IntgDefaultSettings
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgDefaultSettings</returns>
        Task<IEnumerable<DefaultSettings>> GetDefaultSettingsAsync(bool bypassCache);

        Task<DefaultSettings> GetDefaultSettingsByGuidAsync(string guid, bool bypassCache);

        Task<string> GetDefaultSettingsIdFromGuidAsync(string guid);

        Task<DefaultSettings> UpdateDefaultSettingsAsync(DefaultSettings defaultSettings);

        Task<Dictionary<string, string>> GetAllArCodesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllArTypesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllApTypesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllApprovalAgenciesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllStaffApprovalsAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllCreditTypesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllAcademicLevelsAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllAssignmentContractTypesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllPositionsAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllLoadPeriodsAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllBendedCodesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllApplicationStaffAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllApplicationStatusesAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllReceiptTenderGlDistrsAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllPaymentMethodsAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllSponsorsAsync(bool bypassCache);

        Task<Dictionary<string, string>> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache, string specialProcessing = "*");

        Task<DefaultSettingsAdvancedSearch> GetDefaultSettingsAdvancedSearchOptionsAsync(string guid, string keyword, bool bypassCache);
    }
}