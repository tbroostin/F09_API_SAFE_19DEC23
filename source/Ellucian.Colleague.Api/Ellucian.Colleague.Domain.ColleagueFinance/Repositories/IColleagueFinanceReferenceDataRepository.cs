// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Provides read-only access to basic data required for Colleague Finance Self Service
    /// </summary>
    public interface IColleagueFinanceReferenceDataRepository 
    {
        /// <summary>
        /// Get a collection of AccountComponents
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AccountComponents</returns>
        Task<IEnumerable<AccountComponents>> GetAccountComponentsAsync(bool ignoreCache);

        /// <summary>
        /// Get a collection of GL source codes
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        Task<IEnumerable<GlSourceCodes>> GetGlSourceCodesValcodeAsync(bool ignoreCache);

        /// <summary>
        /// Get a collectoin of accounting string component values
        /// </summary>
        Task<Tuple<IEnumerable<AccountingStringComponentValues>,int>> GetAccountingStringComponentValuesAsync(int Offset, int Limit, string component, 
            string transactionStatus, string typeAccount, string typeFund, bool bypassCache);
        Task<Tuple<IEnumerable<AccountingStringComponentValues>, int>> GetAccountingStringComponentValues2Async(int Offset, int Limit, string component, 
            string transactionStatus, string typeAccount);
        Task<Tuple<IEnumerable<AccountingStringComponentValues>, int>> GetAccountingStringComponentValues3Async(int Offset, int Limit, string component,
           string typeAccount, string status, IEnumerable<string> grants, DateTime? effectiveOn);

        /// <summary>
        /// Get a accounting string component value from a Guid
        /// </summary>
        /// <param name="Guid"></param>
        /// <returns></returns>
        Task<AccountingStringComponentValues> GetAccountingStringComponentValueByGuid(string Guid);
        Task<AccountingStringComponentValues> GetAccountingStringComponentValue2ByGuid(string Guid);
        Task<AccountingStringComponentValues> GetAccountingStringComponentValue3ByGuid(string Guid);
        Task<string> GetAccountingStringComponentValuesGuidFromIdAsync(string id);

        /// <summary>
        /// Gets all asset ctegories.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<AssetCategories>> GetAssetCategoriesAsync(bool bypassCache);

        /// <summary>
        /// Gets all asset types.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<AssetTypes>> GetAssetTypesAsync(bool bypassCache);


        /// <summary>
        /// Get a collection of AccountFormats
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AccountFormats</returns>
        Task<IEnumerable<AccountingFormat>> GetAccountFormatsAsync(bool ignoreCache);

        /// <summary>
        /// Returns domain entities for AccountsPayableSources.
        /// </summary>
        Task<IEnumerable<AccountsPayableSources>> GetAccountsPayableSourcesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for AccountsPayableSource code
        /// </summary>
        /// <param name="code">AccountsPayableSource code</param>
        /// <returns>Guid</returns>
        Task<string> GetAccountsPayableSourceGuidAsync(string code);

        /// <summary>
        /// Returns domain entities for Accounts Payable Tax codes.
        /// </summary>
        Task<IEnumerable<AccountsPayableTax>> GetAccountsPayableTaxCodesAsync();

        /// <summary>
        /// Returns domain entities for accounts payable type codes.
        /// </summary>
        Task<IEnumerable<AccountsPayableType>> GetAccountsPayableTypeCodesAsync();

        /// <summary>
        /// Returns domain entities for commodity codes.
        /// </summary>
        Task<IEnumerable<CommodityCode>> GetCommodityCodesAsync(bool ignoreCache);

        /// <summary>
        /// Returns domain entities for commodity codes.
        /// </summary>
        Task<IEnumerable<ProcurementCommodityCode>> GetAllCommodityCodesAsync();

        /// <summary>
        /// Get guid for CommodityCode code
        /// </summary>
        /// <param name="code">CommodityCode code</param>
        /// <returns>Guid</returns>
        Task<string> GetCommodityCodeGuidAsync(string code);

        /// <summary>
        /// Returns domain entities for commodity unit types.
        /// </summary>
        Task<IEnumerable<CommodityUnitType>> GetCommodityUnitTypesAsync(bool ignoreCache);

        Task<IEnumerable<CommodityUnitType>> GetAllCommodityUnitTypesAsync();

        /// <summary>
        /// Get guid for CommodityUnitType code
        /// </summary>
        /// <param name="code">FreeOnBoardType code</param>
        /// <returns>Guid</returns>
        Task<string> GetCommodityUnitTypeGuidAsync(string code);

        /// <summary>
        /// Return the name (CORP.NAME) of the Host Organization ID on PID2 (DEFAULTS.HOST.CORP.ID).
        /// </summary>
        /// <returns></returns>
        Task<string> GetCorpNameAsync();

        /// <summary>
        /// Returns domain entities for CurrencyConversion
        /// </summary>
        Task<IEnumerable<CurrencyConversion>> GetCurrencyConversionAsync();

        /// <summary>
        /// Returns domain entities for FiscalPeriodsIntg
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FiscalPeriodsIntg</returns>
        Task<IEnumerable<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg>> GetFiscalPeriodsIntgAsync(bool ignoreCache);

        /// <summary>
        /// Returns domain entities for FiscalYears
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FiscalYears</returns>
        Task<IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear>> GetFiscalYearsAsync(bool ignoreCache);


        /// <summary>
        /// Returns domain entities for free on board types.
        /// </summary>
        Task<IEnumerable<FreeOnBoardType>> GetFreeOnBoardTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for FreeOnBoardType code
        /// </summary>
        /// <param name="code">FreeOnBoardType code</param>
        /// <returns>Guid</returns>
        Task<string> GetFreeOnBoardTypeGuidAsync(string code);

        /// <summary>
        /// Returns domain entities for shipping methods.
        /// </summary>
        Task<IEnumerable<ShippingMethod>> GetShippingMethodsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for ShippingMethods code
        /// </summary>
        /// <param name="code">ShippingMethods code</param>
        /// <returns>Guid</returns>
        Task<string> GetShippingMethodGuidAsync(string code);

        /// <summary>
        /// Get a collection of ShipToDestinations
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ShipToDestinations</returns>
        Task<IEnumerable<ShipToDestination>> GetShipToDestinationsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for ShipToDestination code
        /// </summary>
        /// <param name="code">ShipToDestination code</param>
        /// <returns>Guid</returns>
        Task<string> GetShipToDestinationGuidAsync(string code);

        /// <summary>
        /// Returns domain entities for VendorTerms.
        /// </summary>
        Task<IEnumerable<VendorTerm>> GetVendorTermsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for VendorTerm code
        /// </summary>
        /// <param name="code">VendorTerm code</param>
        /// <returns>Guid</returns>
        Task<string> GetVendorTermGuidAsync(string code);

        /// <summary>
        /// Returns domain entities for commodity unit types.
        /// </summary>
        Task<IEnumerable<VendorType>> GetVendorTypesAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for VendorTypes code
        /// </summary>
        /// <param name="code">VendorTypes code</param>
        /// <returns>Guid</returns>
        Task<string> GetVendorTypesGuidAsync(string code);

        /// <summary>
        /// Get a collection of IntgVendorAddressUsages
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgVendorAddressUsages</returns>
        Task<IEnumerable<IntgVendorAddressUsages>> GetIntgVendorAddressUsagesAsync(bool ignoreCache);

        /// <summary>
        /// Get a guid of IntgVendorAddressUsages
        /// </summary>
        /// <param name="code">IntgVendorAddressUsages code</param>
        /// <returns>Guid</returns>
        Task<string> GetIntgVendorAddressUsagesGuidAsync(string code);

        // <summary>
        /// Returns domain entities for VendorHoldReasons.
        /// </summary>
        Task<IEnumerable<VendorHoldReasons>> GetVendorHoldReasonsAsync(bool ignoreCache);

        /// <summary>
        /// Get guid for VendorHoldReasons code
        /// </summary>
        /// <param name="code">VendorHoldReasons code</param>
        /// <returns>Guid</returns>
        Task<string> GetVendorHoldReasonsGuidAsync(string code);

        Task<IDictionary<string, string>> GetGuidsForPooleeGLAcctsInFiscalYearsAsync(IEnumerable<string> glAccts);

        /// <summary>
        /// Get a collection of AcctStructureIntg
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AcctStructureIntg</returns>
       Task<IEnumerable<AcctStructureIntg>> GetAcctStructureIntgAsync(bool ignoreCache);


        /// <summary>
        /// Gets all accounting string sub component values
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>, int>> GetAccountingStringSubcomponentValuesAsync(int offset, int limit, bool bypassCache);

        /// <summary>
        /// Gets all accounting string sub component values
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetAllAccountingStringSubcomponentValuesAsync(bool bypassCache);

        /// <summary>
        /// Gets all FD.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetFdDescs(bool bypassCache);

        /// <summary>
        /// Gets all UN.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetUnDescs(bool bypassCache);

        /// <summary>
        /// Gets all OB.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetObDescs(bool bypassCache);

        /// <summary>
        /// Gets all FC.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetFcDescs(bool bypassCache);

        /// <summary>
        /// Gets all SO.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetSoDescs(bool bypassCache);

        /// <summary>
        /// Gets all LO.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetLoDescs(bool bypassCache);

        /// <summary>
        /// Gets particular accounting string sub component values
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues> GetAccountingStringSubcomponentValuesByGuidAsync(string guid);

        // Summary:
        //     Get the GUID for a specified entity, primary key, and optional secondary field
        //     and key
        //
        // Parameters:
        //   entity:
        //     The name of the entity
        //
        //   primaryKey:
        //     The value of the primary key
        //
        //   secondaryField:
        //     The CDD name of the secondary field
        //
        //   secondaryKey:
        //     The value of the secondary key
        //
        // Returns:
        //     The corresponding GUID
        Task<string> GetGuidFromEntityInfoAsync(string entity, string primaryKey, string secondaryField = "", string secondaryKey = "");

        ///// <summary>
        ///// Gets particular accounting string sub component values using entity and Key
        ///// </summary>
        ///// <param name="entity"></param>
        ///// <param name="Key"></param>
        ///// <returns></returns>
        //Task<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues> GetAccountingStringSubcomponentValuesByEntityKey(string entity, string key);

        /// <summary>
        /// Get a collection of ShipToCodes
        /// </summary>
        /// <returns>Collection of ShipToCodes</returns>
        Task<IEnumerable<ShipToCode>> GetShipToCodesAsync();

        /// <summary>
        /// Get a collection of ShipViaCodes
        /// </summary>
        /// <returns>Collection of ShipViaCodes</returns>
        Task<IEnumerable<ShipViaCode>> GetShipViaCodesAsync();

        /// <summary>
        /// Get fixed asset transfer flags
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FixedAssetsFlag>> GetFixedAssetTransferFlagsAsync();

        /// <summary>
        /// Get a commodity code
        /// </summary>
        /// <param name="recordKey">commodity code key</param>
        /// <returns>ProcurementCommodityCode</returns>
        Task<Domain.ColleagueFinance.Entities.ProcurementCommodityCode> GetCommodityCodeByCodeAsync(string recordKey);

        /// <summary>
        /// Get tax forms
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TaxForm>> GetTaxFormsAsync();


    }
}