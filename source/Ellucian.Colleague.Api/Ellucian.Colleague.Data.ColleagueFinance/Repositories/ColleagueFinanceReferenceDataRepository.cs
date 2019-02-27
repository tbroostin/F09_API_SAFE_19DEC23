// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Repository for Colleague Finance reference data
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ColleagueFinanceReferenceDataRepository : BaseColleagueRepository, IColleagueFinanceReferenceDataRepository
    {
        public ColleagueFinanceReferenceDataRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region Public Methods

        /// <summary>
        /// Get a collection of AccountComponents
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AccountComponents</returns>
        public async Task<IEnumerable<AccountComponents>> GetAccountComponentsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<AccountComponents>("CF", "ACCOUNT.COMPONENTS",
                (e, g) => new AccountComponents(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Gets a collection of GlSourceCodes
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GlSourceCodes>> GetGlSourceCodesValcodeAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<GlSourceCodes>("CF", "GL.SOURCE.CODES",
                (e, g) => new GlSourceCodes(g, e.ValInternalCodeAssocMember, (string.IsNullOrEmpty(e.ValExternalRepresentationAssocMember)
                    ? e.ValInternalCodeAssocMember : e.ValExternalRepresentationAssocMember), e.ValActionCode3AssocMember), bypassCache: ignoreCache);
        }

        public async Task<AccountingStringComponentValues> GetAccountingStringComponentValueByGuid(string Guid)
        {
            var recordInfo = await GetRecordInfoFromGuidAsync(Guid);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey))
            {
                throw new KeyNotFoundException(string.Format("No accounting string componenent value was found for guid '{0}'. ", Guid));
            }
            AccountingStringComponentValues ASCV = new AccountingStringComponentValues();
            switch (recordInfo.Entity)
            {
                case "PROJECTS":
                    var project = await DataReader.ReadRecordAsync<Projects>(recordInfo.PrimaryKey);
                    if (project == null)
                    {
                        throw new KeyNotFoundException(string.Format("No accounting string component value was found for guid '{0}'. ", Guid));
                    }
                    ASCV = convertProjectsToASCV(project);
                    break;
                case "GL.ACCTS":
                    var glAccount = await DataReader.ReadRecordAsync<DataContracts.GlAccts>(recordInfo.PrimaryKey);
                    if (glAccount == null)
                    {
                        throw new KeyNotFoundException(string.Format("No accounting string component value was found for guid '{0}'. ", Guid));
                    }
                    var glClassDef = await DataReader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
                    var glAcctCC = await DataReader.ReadRecordAsync<DataContracts.GlAcctsCc>(recordInfo.PrimaryKey);
                    var fiscalYearDataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);
                    ASCV = ConvertGLtoASCV(glAccount, glClassDef, glAcctCC, fiscalYearDataContract);
                    break;
                default:
                    throw new KeyNotFoundException(string.Format("No accounting string component value was found for guid '{0}'. ", Guid));
            }

            return ASCV;

        }

        public async Task<AccountingStringComponentValues> GetAccountingStringComponentValue2ByGuid(string Guid)
        {
            var recordInfo = await GetRecordInfoFromGuidAsync(Guid);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey))
            {
                throw new KeyNotFoundException(string.Format("No accounting string componenent value was found for guid '{0}'. ", Guid));
            }
            AccountingStringComponentValues ASCV = new AccountingStringComponentValues();
            switch (recordInfo.Entity)
            {
                case "PROJECTS":
                    var project = await DataReader.ReadRecordAsync<Projects>(recordInfo.PrimaryKey);
                    if (project == null)
                    {
                        throw new KeyNotFoundException(string.Format("No accounting string componenent value was found for guid '{0}'. ", Guid));
                    }
                    ASCV = convertProjectsToASCV(project);
                    break;
                case "GL.ACCTS":
                    var glAccount = await DataReader.ReadRecordAsync<DataContracts.GlAccts>(recordInfo.PrimaryKey);
                    if (glAccount == null)
                    {
                        throw new KeyNotFoundException(string.Format("No accounting string componenent value was found for guid '{0}'. ", Guid));
                    }
                    var glClassDef = await DataReader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
                    var glAcctCC = await DataReader.ReadRecordAsync<DataContracts.GlAcctsCc>(recordInfo.PrimaryKey);
                    var fiscalYearDataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);
                    ASCV = ConvertGLtoASCV2Async(glAccount, glClassDef, glAcctCC, fiscalYearDataContract);
                    break;
                default:
                    throw new KeyNotFoundException(string.Format("No accounting string componenent value was found for guid '{0}'. ", Guid));
            }

            return ASCV;

        }

        public async Task<Tuple<IEnumerable<AccountingStringComponentValues>, int>> GetAccountingStringComponentValuesAsync(int Offset, int Limit, string component,
            string transactionStatus, string typeAccount, string typeFund, bool ignoreCache)
        {
            List<AccountingStringComponentValues> glAccounts = new List<AccountingStringComponentValues>();
            List<AccountingStringComponentValues> projects = new List<AccountingStringComponentValues>();
            if (ignoreCache)
            {
                switch (component)
                {
                    case "GL.ACCT":
                        glAccounts = await BuildAllGLAccounts();
                        break;
                    case "PROJECT":
                        projects = await BuildAllProjects();
                        break;
                    default:
                        glAccounts = await BuildAllGLAccounts();
                        projects = await BuildAllProjects();
                        break;
                }
            }
            else
            {
                string GlcacheId = "AllAccountStringCompValuesGLA";
                string prjtCacheId = "AllAccountStringCompValuesPRJTS";
                switch (component)
                {
                    case "GL.ACCT":
                        glAccounts = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(GlcacheId, async () => await this.BuildAllGLAccounts(), Level1CacheTimeoutValue);
                        break;
                    case "PROJECT":
                        projects = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(prjtCacheId, async () => await this.BuildAllProjects(), Level1CacheTimeoutValue);
                        break;
                    default:
                        glAccounts = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(GlcacheId, async () => await this.BuildAllGLAccounts(), Level1CacheTimeoutValue);
                        projects = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(prjtCacheId, async () => await this.BuildAllProjects(), Level1CacheTimeoutValue);
                        break;
                }
            }

            List<AccountingStringComponentValues> allASCV = new List<AccountingStringComponentValues>();
            allASCV.AddRange(glAccounts);
            allASCV.AddRange(projects);
            if (!string.IsNullOrEmpty(transactionStatus) && allASCV.Count > 0)
            {
                var temp = allASCV.Where(x => x.Status == transactionStatus);
                allASCV = new List<AccountingStringComponentValues>();
                allASCV.AddRange(temp);
            }
            if (!string.IsNullOrEmpty(typeAccount) && allASCV.Count > 0)
            {
                var temp = allASCV.Where(x => x.Type == typeAccount);
                allASCV = new List<AccountingStringComponentValues>();
                allASCV.AddRange(temp);
            }

            allASCV.OrderBy(o => o.AccountNumber);
            int totalCount = allASCV.Count();
            var pageList = allASCV.Skip(Offset).Take(Limit).ToArray();

            return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(pageList, totalCount);
        }


        /// <summary>
        /// Returns a single general ledger activity record.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<string> GetAccountingStringComponentValuesGuidFromIdAsync(string id)
        {              
            try
            {
                return await GetGuidFromRecordInfoAsync("GL.ACCTS", id); 
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("accounting-string-component-values", "GUID not found for accounting-string-component-values " + id));
                throw ex;
            }
        }
    

        /// <summary>
        /// Get a collection of AccountComponents
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AccountComponents</returns>
        public async Task<IEnumerable<AccountingFormat>> GetAccountFormatsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<AccountingFormat>("CF", "INTG.ACCOUNTING.STRING.FORMATS",
                (e, g) => new AccountingFormat(g, e.ValInternalCodeAssocMember, e.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of AccountsPayableSources
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AccountsPayableSources</returns>
        public async Task<IEnumerable<AccountsPayableSources>> GetAccountsPayableSourcesAsync(bool ignoreCache)
        {

            if (ignoreCache)
            {
                return await BuildAllAccountsPayableSources();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<AccountsPayableSources>>("AllAccountsPayableSources", async () => await this.BuildAllAccountsPayableSources(), Level1CacheTimeoutValue);
            }
        }

        private async Task<IEnumerable<AccountsPayableSources>> BuildAllAccountsPayableSources()
        {
            var accountsPayableSourcesEntities = new List<AccountsPayableSources>();
            var accountsPayableSourcesRecords = await DataReader.BulkReadRecordAsync<DataContracts.ApTypes>("AP.TYPES", "");
            var bankCodesRecords = (await DataReader.BulkReadRecordAsync<Base.DataContracts.BankCodes>("BANK.CODES", ""));


            foreach (var accountsPayableSourcesRecord in accountsPayableSourcesRecords)
            {

                var accountsPayableSource = new AccountsPayableSources(accountsPayableSourcesRecord.RecordGuid, accountsPayableSourcesRecord.Recordkey, accountsPayableSourcesRecord.ApTypesDesc);
                var bankCode = bankCodesRecords.FirstOrDefault(b => b.Recordkey == accountsPayableSourcesRecord.AptBankCode);
                if (bankCode != null)
                    accountsPayableSource.directDeposit = bankCode.BankEftActiveFlag;
                accountsPayableSourcesEntities.Add(accountsPayableSource);
            }


            return accountsPayableSourcesEntities;
        }

        /// <summary>
        /// Return a list of Accounts Payable Tax codes.
        /// Cache them for the maximum time. It is very stable information.
        /// </summary>
        public async Task<IEnumerable<AccountsPayableTax>> GetAccountsPayableTaxCodesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<AccountsPayableTax>>("AccountsPayableTaxes", async () =>
            {
                return await GetCodeItemAsync<ApTaxes, AccountsPayableTax>("AllApTaxes", "AP.TAXES",
                itemCode => new AccountsPayableTax(itemCode.Recordkey, itemCode.ApTaxDesc));
            });
        }

        /// <summary>
        /// Return a list of AP type codes.
        /// Cache them for the maximum time. It is very stable information.
        /// </summary>
        public async Task<IEnumerable<AccountsPayableType>> GetAccountsPayableTypeCodesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<AccountsPayableType>>("AccountsPayableTypes", async () =>
            {
                return await GetCodeItemAsync<ApTypes, AccountsPayableType>("AllApTypes", "AP.TYPES",
                itemCode => new AccountsPayableType(itemCode.Recordkey, itemCode.ApTypesDesc) { BankCode = itemCode.AptBankCode });
            });
        }

        /// <summary>
        /// Gets all assett categories.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AssetCategories>> GetAssetCategoriesAsync(bool bypassCache)
        {
            return await GetGuidCodeItemAsync<DataContracts.AssetCategories, Domain.ColleagueFinance.Entities.AssetCategories>("AllAssetCategories", "ASSET.CATEGORIES",
                (cl, g) => new Domain.ColleagueFinance.Entities.AssetCategories(g, cl.Recordkey, (string.IsNullOrEmpty(cl.AsctDesc)
                    ? cl.Recordkey : cl.AsctDesc)), bypassCache: bypassCache);
        }


        /// <summary>
        /// Gets all asset types.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Domain.ColleagueFinance.Entities.AssetTypes>> GetAssetTypesAsync(bool bypassCache)
        {

            if(bypassCache)
            {
                return await BuildAssetTypes();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<Domain.ColleagueFinance.Entities.AssetTypes>>("AllAssetTypes", async () => await BuildAssetTypes());
            }
        }
        
        /// <summary>
        /// Builds asset types.
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.AssetTypes>> BuildAssetTypes()
        {
            var calcMethods = await DataReader.BulkReadRecordAsync<DataContracts.CalcMethods>("CALC.METHODS", string.Empty);
            List<Domain.ColleagueFinance.Entities.AssetTypes> assetTypeEntities = new List<Domain.ColleagueFinance.Entities.AssetTypes>();

            var assetTypeDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.AssetTypes>("ASSET.TYPES", string.Empty);

            if(assetTypeDataContracts == null || !assetTypeDataContracts.Any())
            {
                return null;
            }

            foreach (var assetTypeDataContract in assetTypeDataContracts)
            {
                Domain.ColleagueFinance.Entities.AssetTypes assetTypeEntity = new Domain.ColleagueFinance.Entities.AssetTypes(assetTypeDataContract.RecordGuid, 
                    assetTypeDataContract.Recordkey, (string.IsNullOrEmpty(assetTypeDataContract.AstpDesc)? assetTypeDataContract.Recordkey : assetTypeDataContract.AstpDesc))
                {
                    AstpSalvagePoint = assetTypeDataContract.AstpSalvagePct,
                    AstpUsefulLife = assetTypeDataContract.AstpUsefulLife,
                    AstpCalcMethod = BuildCalcMethod(calcMethods, assetTypeDataContract.AstpCalcMethod)
                };

                assetTypeEntities.Add(assetTypeEntity);
            }

            return assetTypeEntities;
        }

        /// <summary>
        /// Build calc method.
        /// </summary>
        /// <param name="calcMethods"></param>
        /// <param name="astpCalcMethod"></param>
        /// <returns></returns>
        private string BuildCalcMethod(Collection<DataContracts.CalcMethods> calcMethods, string astpCalcMethod)
        {
            if(calcMethods == null || !calcMethods.Any() || string.IsNullOrWhiteSpace(astpCalcMethod))
            {
                return null;
            }

            var method = calcMethods.FirstOrDefault(m => m.Recordkey.Equals(astpCalcMethod, StringComparison.OrdinalIgnoreCase));
            if(method == null)
            {
                throw new KeyNotFoundException(string.Format("Calc method not found for id {0}", astpCalcMethod));
            }

            return method.CalcDesc;
        }

        /// <summary>
        /// Gets CommodityCodes
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CommodityCode>> GetCommodityCodesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<CommodityCodes, CommodityCode>("AllCommodityCodes", "COMMODITY.CODES",
            (cc, g) => new CommodityCode(g, cc.Recordkey, cc.CmdtyDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Gets CommodityUnitTypes
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CommodityUnitType>> GetCommodityUnitTypesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<UnitIssues, CommodityUnitType>("AllCommodityUnitTypes", "UNIT.ISSUES",
            (cu, g) => new CommodityUnitType(g, cu.Recordkey, cu.UiDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Return a list of CurrencyCodes.
        /// Cache them for the maximum time. It is very stable information.
        /// </summary>
        public async Task<IEnumerable<CurrencyConversion>> GetCurrencyConversionAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<CurrencyConversion>>("CurrencyCodes", async () =>
            {
                return await GetCodeItemAsync<CurrencyConv, CurrencyConversion>("AllCurrencyCodes", "CURRENCY.CONV",
                    itemCode => new CurrencyConversion(itemCode.Recordkey, itemCode.CurrencyConvDesc) { CurrencyCode = ConvertCurrencyConvIsoCodeToCurrencyCode(itemCode.CurrencyConvIsoCode) });
            });
        }

        /// <summary>
        /// Get a collection of FiscalPeriodsIntg
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FiscalPeriodsIntg</returns>
        public async Task<IEnumerable<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg>> GetFiscalPeriodsIntgAsync(bool ignoreCache)
        {

            if (ignoreCache)
            {
                return await BuildAllFiscalPeriodsIntgs();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg>>("AllFiscalPeriodIntgs", async () => await this.BuildAllFiscalPeriodsIntgs(), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Get a collection of FiscalYears
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FiscalYears</returns>
        public async Task<IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear>> GetFiscalYearsAsync(bool ignoreCache)
        {

            if (ignoreCache)
            {
                return await BuildAllFiscalYears();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear>>("AllFiscalYearsGenLdgr", async () => await this.BuildAllFiscalYears(), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Gets FreeOnBoardType
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<FreeOnBoardType>> GetFreeOnBoardTypesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<Fobs, FreeOnBoardType>("AllFreeOnBoardTypes", "FOBS",
            (fob, g) => new FreeOnBoardType(g, fob.Recordkey, fob.FobsDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Gets ShippingMethod
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ShippingMethod>> GetShippingMethodsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<ShipVias, ShippingMethod>("AllShippingMethods", "SHIP.VIAS",
            (sm, g) => new ShippingMethod(g, sm.Recordkey, sm.ShipViasDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of ShipToDestinations
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of ShipToDestinations</returns>
        public async Task<IEnumerable<ShipToDestination>> GetShipToDestinationsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<ColleagueFinance.DataContracts.ShipToCodes, ShipToDestination>("AllShipToDestinations", "SHIP.TO.CODES",
            (stc, g) => new ShipToDestination(g, stc.Recordkey, stc.ShptName)
            {
                addressLines = stc.ShptAddress,
                placeCountryRegionCode = stc.ShptState,
                placeCountryLocality = stc.ShptCity,
                placeCountryPostalCode = stc.ShptZip,
                contactName = stc.ShptName,
                phoneNumber = stc.ShptPhone,
                phoneExtension = stc.ShptExt
            }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get a collection of Collection of VendorHoldReasons domain objects
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of VendorHoldReasons domain objects></returns>
        public async Task<IEnumerable<VendorHoldReasons>> GetVendorHoldReasonsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<VendorHoldReasons>("CF", "INTG.VENDOR.HOLD.REASONS",
                (cl, g) => new VendorHoldReasons(g, cl.ValInternalCodeAssocMember, cl.ValExternalRepresentationAssocMember), bypassCache: ignoreCache);

        }

        /// <summary>
        /// Get a collection of Collection of VendorTerm domain objects
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of VendorTerm domain objects</returns>
        public async Task<IEnumerable<VendorTerm>> GetVendorTermsAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<VendorTerms, VendorTerm>("AllVendorTerms", "VENDOR.TERMS",
                (e, g) => new VendorTerm(g, e.Recordkey, e.VendorTermsDesc), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Gets Vendor types
        /// </summary>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<VendorType>> GetVendorTypesAsync(bool ignoreCache)
        {
            return await GetGuidCodeItemAsync<VendorTypes, VendorType>("AllVendorTypes", "VENDOR.TYPES",
            (cu, g) => new VendorType(g, cu.Recordkey, cu.VendorTypesDesc), bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get a collection of AcctStructureIntg
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AcctStructureIntg</returns>
        public async Task<IEnumerable<Domain.ColleagueFinance.Entities.AcctStructureIntg>> GetAcctStructureIntgAsync(bool ignoreCache)
        {
            if (ignoreCache)
            {
                return await BuildAllAcctStructureIntg();
            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<Domain.ColleagueFinance.Entities.AcctStructureIntg>>("AllAcctStructureIntg", async () => await this.BuildAllAcctStructureIntg(), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Gets all FD.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetFdDescs(bool bypassCache)
        {

            return await GetGuidCodeItemAsync<DataContracts.FdDescs, Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>("AllFdDescs", "FD.DESCS",
                (cl, g) => new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(g, cl.Recordkey, (string.IsNullOrEmpty(cl.FdDescription)
                    ? cl.Recordkey : cl.FdDescription), "FD")
                {
                    Explanation = cl.FdExplanation
                }, bypassCache: bypassCache);
        }

        /// <summary>
        /// Gets all UN.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetUnDescs(bool bypassCache)
        {

            return await GetGuidCodeItemAsync<DataContracts.UnDescs, Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>("AllUnDescs", "UN.DESCS",
                (cl, g) => new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(g, cl.Recordkey, (string.IsNullOrEmpty(cl.UnDescription)
                    ? cl.Recordkey : cl.UnDescription), "UN")
                { Explanation = cl.UnExplanation }, bypassCache: bypassCache);
        }

        /// <summary>
        /// Gets all OB.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetObDescs(bool bypassCache)
        {

            return await GetGuidCodeItemAsync<DataContracts.ObDescs, Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>("AllObDescs", "OB.DESCS",
                (cl, g) => new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(g, cl.Recordkey, (string.IsNullOrEmpty(cl.ObDescription)
                    ? cl.Recordkey : cl.ObDescription), "OB")
                { Explanation = cl.ObExplanation }, bypassCache: bypassCache);
        }

        /// <summary>
        /// Gets all FC.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetFcDescs(bool bypassCache)
        {

            return await GetGuidCodeItemAsync<DataContracts.FcDescs, Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>("AllFcDescs", "FC.DESCS",
                (cl, g) => new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(g, cl.Recordkey, (string.IsNullOrEmpty(cl.FcDescription)
                    ? cl.Recordkey : cl.FcDescription), "FC")
                { Explanation = cl.FcExplanation }, bypassCache: bypassCache);
        }

        /// <summary>
        /// Gets all SO.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetSoDescs(bool bypassCache)
        {

            return await GetGuidCodeItemAsync<DataContracts.SoDescs, Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>("AllSoDescs", "SO.DESCS",
                 (cl, g) => new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(g, cl.Recordkey, (string.IsNullOrEmpty(cl.SoDescription)
                     ? cl.Recordkey : cl.SoDescription), "SO")
                 { Explanation = cl.SoExplanation }, bypassCache: bypassCache);
        }

        /// <summary>
        /// Gets all LO.DESCS
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetLoDescs(bool bypassCache)
        {

            return await GetGuidCodeItemAsync<DataContracts.LoDescs, Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>("AllLoDescs", "LO.DESCS",
                (cl, g) => new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(g, cl.Recordkey, (string.IsNullOrEmpty(cl.LoDescription)
                    ? cl.Recordkey : cl.LoDescription), "LO")
                { Explanation = cl.LoExplanation }, bypassCache: bypassCache);
        }

        /// <summary>
        /// Gets all accounting string sub component values
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>> GetAllAccountingStringSubcomponentValuesAsync(bool bypassCache)
        {
            var acctStringValuesCollection = new List<AccountingStringSubcomponentValues>();
            var fdCollection = await GetFdDescs(bypassCache);
            if (fdCollection != null && fdCollection.Any())
                acctStringValuesCollection.AddRange(fdCollection);
            var unCollection = await GetUnDescs(bypassCache);
            if (unCollection != null && unCollection.Any())
                acctStringValuesCollection.AddRange(unCollection);
            var obCollection = await GetObDescs(bypassCache);
            if (obCollection != null && obCollection.Any())
                acctStringValuesCollection.AddRange(obCollection);
            var fcCollection = await GetFcDescs(bypassCache);
            if (fcCollection != null && fcCollection.Any())
                acctStringValuesCollection.AddRange(fcCollection);
            var soCollection = await GetSoDescs(bypassCache);
            if (soCollection != null && soCollection.Any())
                acctStringValuesCollection.AddRange(soCollection);
            var loCollection = await GetLoDescs(bypassCache);
            if (loCollection != null && loCollection.Any())
                acctStringValuesCollection.AddRange(loCollection);
            return acctStringValuesCollection;
        }


        /// <summary>
        /// Gets all accounting string sub component values
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>, int>> GetAccountingStringSubcomponentValuesAsync(int offset, int limit, bool bypassCache)
        {
            var acctStringValuesCollection = await GetAllAccountingStringSubcomponentValuesAsync(bypassCache);
            var acctStringValuesEntities = new List<AccountingStringSubcomponentValues>();
            var totalCount = acctStringValuesCollection.Count();
            if (acctStringValuesCollection != null && acctStringValuesCollection.Any())
            {
                acctStringValuesEntities = acctStringValuesCollection.Skip(offset).Take(limit).ToList();
            }

            return new Tuple<IEnumerable<AccountingStringSubcomponentValues>, int>(acctStringValuesEntities, totalCount);
        }

        /// <summary>
        /// Gets particular accounting string sub component values
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues> GetAccountingStringSubcomponentValuesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Accounting String Subcomponent Values Id must be provided.");
            }
            Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues acctStringValues = null;
            var recordInfo = await this.GetRecordInfoFromGuidAsync(guid);
            if (recordInfo != null)
            {
                switch (recordInfo.Entity)
                {
                    case ("FD.DESCS"):
                        var fdDescs = await DataReader.ReadRecordAsync<FdDescs>(recordInfo.PrimaryKey);
                        if (fdDescs != null)
                            acctStringValues = new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(fdDescs.RecordGuid, fdDescs.Recordkey, fdDescs.FdDescription, "FD") { Explanation = fdDescs.FdExplanation };
                        break;

                    case ("UN.DESCS"):
                        var UnDescs = await DataReader.ReadRecordAsync<UnDescs>(recordInfo.PrimaryKey);
                        if (UnDescs != null)
                            acctStringValues = new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(UnDescs.RecordGuid, UnDescs.Recordkey, UnDescs.UnDescription, "UN") { Explanation = UnDescs.UnExplanation };
                        break;

                    case ("OB.DESCS"):
                        var ObDescs = await DataReader.ReadRecordAsync<ObDescs>(recordInfo.PrimaryKey);
                        if (ObDescs != null)
                            acctStringValues = new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(ObDescs.RecordGuid, ObDescs.Recordkey, ObDescs.ObDescription, "OB") { Explanation = ObDescs.ObExplanation };
                        break;

                    case ("FC.DESCS"):
                        var FcDescs = await DataReader.ReadRecordAsync<FcDescs>(recordInfo.PrimaryKey);
                        if (FcDescs != null)
                            acctStringValues = new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(FcDescs.RecordGuid, FcDescs.Recordkey, FcDescs.FcDescription, "FC") { Explanation = FcDescs.FcExplanation };
                        break;

                    case ("SO.DESCS"):
                        var SoDescs = await DataReader.ReadRecordAsync<SoDescs>(recordInfo.PrimaryKey);
                        if (SoDescs != null)
                            acctStringValues = new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(SoDescs.RecordGuid, SoDescs.Recordkey, SoDescs.SoDescription, "SO") { Explanation = SoDescs.SoExplanation };
                        break;

                    case ("LO.DESCS"):
                        var LoDescs = await DataReader.ReadRecordAsync<LoDescs>(recordInfo.PrimaryKey);
                        if (LoDescs != null)
                            acctStringValues = new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues(LoDescs.RecordGuid, LoDescs.Recordkey, LoDescs.LoDescription, "LO") { Explanation = LoDescs.LoExplanation };
                        break;

                    default:
                        throw new KeyNotFoundException(string.Concat("No accounting string subcomponent value was found for guid ", guid));

                }
                return acctStringValues;
            }

            else
            {
                throw new KeyNotFoundException(string.Concat("No accounting string subcomponent value was found for guid ", guid));
            }
        }

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

        public async Task<string> GetGuidFromEntityInfoAsync(string entity, string primaryKey, string secondaryField = "", string secondaryKey = "")
        {
            return await GetGuidFromRecordInfoAsync(entity, primaryKey, secondaryField, secondaryKey);
        }

       

        #endregion

        #region Private methods

        private CurrencyCodes? ConvertCurrencyConvIsoCodeToCurrencyCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            switch (code)
            {
                case "CAD":
                    return CurrencyCodes.CAD;
                case "EUR":
                    return CurrencyCodes.EUR;
                case "USD":
                    return CurrencyCodes.USD;
                case "AED":
                    return CurrencyCodes.AED;
                case "AFN":
                    return CurrencyCodes.AFN;
                case "ALL":
                    return CurrencyCodes.ALL;
                case "AMD":
                    return CurrencyCodes.AMD;
                case "ANG":
                    return CurrencyCodes.ANG;
                case "AOA":
                    return CurrencyCodes.AOA;
                case "ARS":
                    return CurrencyCodes.ARS;
                case "AUD":
                    return CurrencyCodes.AUD;
                case "AWG":
                    return CurrencyCodes.AWG;
                case "AZN":
                    return CurrencyCodes.AZN;
                case "BAM":
                    return CurrencyCodes.BAM;
                case "BBD":
                    return CurrencyCodes.BBD;
                case "BDT":
                    return CurrencyCodes.BDT;
                case "BGN":
                    return CurrencyCodes.BGN;
                case "BHD":
                    return CurrencyCodes.BHD;
                case "BIF":
                    return CurrencyCodes.BIF;
                case "BMD":
                    return CurrencyCodes.BMD;
                case "BND":
                    return CurrencyCodes.BND;
                case "BOB":
                    return CurrencyCodes.BOB;
                case "BRL":
                    return CurrencyCodes.BRL;
                case "BSD":
                    return CurrencyCodes.BSD;
                case "BTN":
                    return CurrencyCodes.BTN;
                case "BWP":
                    return CurrencyCodes.BWP;
                case "BYR":
                    return CurrencyCodes.BYR;
                case "BZD":
                    return CurrencyCodes.BZD;
                case "CDF":
                    return CurrencyCodes.CDF;
                case "CHF":
                    return CurrencyCodes.CHF;
                case "CLP":
                    return CurrencyCodes.CLP;
                case "CNY":
                    return CurrencyCodes.CNY;
                case "COP":
                    return CurrencyCodes.COP;
                case "CRC":
                    return CurrencyCodes.CRC;
                case "CUC":
                    return CurrencyCodes.CUC;
                case "CUP":
                    return CurrencyCodes.CUP;
                case "CVE":
                    return CurrencyCodes.CVE;
                case "CZK":
                    return CurrencyCodes.CZK;
                case "DJF":
                    return CurrencyCodes.DJF;
                case "DKK":
                    return CurrencyCodes.DKK;
                case "DOP":
                    return CurrencyCodes.DOP;
                case "DZD":
                    return CurrencyCodes.DZD;
                case "EGP":
                    return CurrencyCodes.EGP;
                case "ERN":
                    return CurrencyCodes.ERN;
                case "ETB":
                    return CurrencyCodes.ETB;
                case "FJD":
                    return CurrencyCodes.FJD;
                case "FKP":
                    return CurrencyCodes.FKP;
                case "GBP":
                    return CurrencyCodes.GBP;
                case "GEL":
                    return CurrencyCodes.GEL;
                case "GHS":
                    return CurrencyCodes.GHS;
                case "GIP":
                    return CurrencyCodes.GIP;
                case "GMD":
                    return CurrencyCodes.GMD;
                case "GNF":
                    return CurrencyCodes.GNF;
                case "GTQ":
                    return CurrencyCodes.GTQ;
                case "GYD":
                    return CurrencyCodes.GYD;
                case "HKD":
                    return CurrencyCodes.HKD;
                case "HNL":
                    return CurrencyCodes.HNL;
                case "HRK":
                    return CurrencyCodes.HRK;
                case "HTG":
                    return CurrencyCodes.HTG;
                case "HUF":
                    return CurrencyCodes.HUF;
                case "IDR":
                    return CurrencyCodes.IDR;
                case "ILS":
                    return CurrencyCodes.ILS;
                case "INR":
                    return CurrencyCodes.INR;
                case "IQD":
                    return CurrencyCodes.IQD;
                case "IRR":
                    return CurrencyCodes.IRR;
                case "ISK":
                    return CurrencyCodes.ISK;
                case "JMD":
                    return CurrencyCodes.JMD;
                case "JOD":
                    return CurrencyCodes.JOD;
                case "JPY":
                    return CurrencyCodes.JPY;
                case "KES":
                    return CurrencyCodes.KES;
                case "KGS":
                    return CurrencyCodes.KGS;
                case "KHR":
                    return CurrencyCodes.KHR;
                case "KMF":
                    return CurrencyCodes.KMF;
                case "KPW":
                    return CurrencyCodes.KPW;
                case "KRW":
                    return CurrencyCodes.KRW;
                case "KWD":
                    return CurrencyCodes.KWD;
                case "KYD":
                    return CurrencyCodes.KYD;
                case "KZT":
                    return CurrencyCodes.KZT;
                case "LAK":
                    return CurrencyCodes.LAK;
                case "LBP":
                    return CurrencyCodes.LBP;
                case "LKR":
                    return CurrencyCodes.LKR;
                case "LRD":
                    return CurrencyCodes.LRD;
                case "LSL":
                    return CurrencyCodes.LSL;
                case "LYD":
                    return CurrencyCodes.LYD;
                case "MAD":
                    return CurrencyCodes.MAD;
                case "MDL":
                    return CurrencyCodes.MDL;
                case "MGA":
                    return CurrencyCodes.MGA;
                case "MKD":
                    return CurrencyCodes.MKD;
                case "MMK":
                    return CurrencyCodes.MMK;
                case "MNT":
                    return CurrencyCodes.MNT;
                case "MOP":
                    return CurrencyCodes.MOP;
                case "MRO":
                    return CurrencyCodes.MRO;
                case "MUR":
                    return CurrencyCodes.MUR;
                case "MVR":
                    return CurrencyCodes.MVR;
                case "MWK":
                    return CurrencyCodes.MWK;
                case "MXN":
                    return CurrencyCodes.MXN;
                case "MYR":
                    return CurrencyCodes.MYR;
                case "MZN":
                    return CurrencyCodes.MZN;
                case "NAD":
                    return CurrencyCodes.NAD;
                case "NGN":
                    return CurrencyCodes.NGN;
                case "NIO":
                    return CurrencyCodes.NIO;
                case "NOK":
                    return CurrencyCodes.NOK;
                case "NPR":
                    return CurrencyCodes.NPR;
                case "NZD":
                    return CurrencyCodes.NZD;
                case "OMR":
                    return CurrencyCodes.OMR;
                case "PAB":
                    return CurrencyCodes.PAB;
                case "PEN":
                    return CurrencyCodes.PEN;
                case "PGK":
                    return CurrencyCodes.PGK;
                case "PHP":
                    return CurrencyCodes.PHP;
                case "PKR":
                    return CurrencyCodes.PKR;
                case "PLN":
                    return CurrencyCodes.PLN;
                case "PYG":
                    return CurrencyCodes.PYG;
                case "QAR":
                    return CurrencyCodes.QAR;
                case "RON":
                    return CurrencyCodes.RON;
                case "RSD":
                    return CurrencyCodes.RSD;
                case "RUB":
                    return CurrencyCodes.RUB;
                case "RWF":
                    return CurrencyCodes.RWF;
                case "SAR":
                    return CurrencyCodes.SAR;
                case "SBD":
                    return CurrencyCodes.SBD;
                case "SCR":
                    return CurrencyCodes.SCR;
                case "SDG":
                    return CurrencyCodes.SDG;
                case "SEK":
                    return CurrencyCodes.SEK;
                case "SGD":
                    return CurrencyCodes.SGD;
                case "SHP":
                    return CurrencyCodes.SHP;
                case "SLL":
                    return CurrencyCodes.SLL;
                case "SOS":
                    return CurrencyCodes.SOS;
                case "SRD":
                    return CurrencyCodes.SRD;
                case "SSP":
                    return CurrencyCodes.SSP;
                case "STD":
                    return CurrencyCodes.STD;
                case "SVC":
                    return CurrencyCodes.SVC;
                case "SYP":
                    return CurrencyCodes.SYP;
                case "SZL":
                    return CurrencyCodes.SZL;
                case "THB":
                    return CurrencyCodes.THB;
                case "TJS":
                    return CurrencyCodes.TJS;
                case "TMT":
                    return CurrencyCodes.TMT;
                case "TND":
                    return CurrencyCodes.TND;
                case "TOP":
                    return CurrencyCodes.TOP;
                case "TRY":
                    return CurrencyCodes.TRY;
                case "TTD":
                    return CurrencyCodes.TTD;
                case "TWD":
                    return CurrencyCodes.TWD;
                case "TZS":
                    return CurrencyCodes.TZS;
                case "UAH":
                    return CurrencyCodes.UAH;
                case "UGX":
                    return CurrencyCodes.UGX;
                case "UYU":
                    return CurrencyCodes.UYU;
                case "UZS":
                    return CurrencyCodes.UZS;
                case "VEF":
                    return CurrencyCodes.VEF;
                case "VND":
                    return CurrencyCodes.VND;
                case "VUV":
                    return CurrencyCodes.VUV;
                case "WST":
                    return CurrencyCodes.WST;
                case "XAF":
                    return CurrencyCodes.XAF;
                case "XCD":
                    return CurrencyCodes.XCD;
                case "XOF":
                    return CurrencyCodes.XOF;
                case "XPF":
                    return CurrencyCodes.XPF;
                case "YER":
                    return CurrencyCodes.YER;
                case "ZAR":
                    return CurrencyCodes.ZAR;
                case "ZMW":
                    return CurrencyCodes.ZMW;
                case "ZWL":
                    return CurrencyCodes.ZWL;

                default:
                    return null;
            }
        }

        private async Task<List<AccountingStringComponentValues>> BuildAllProjects()
        {
            List<AccountingStringComponentValues> allProjects = new List<AccountingStringComponentValues>();
            var projects = await DataReader.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", "");

            foreach (var project in projects)
            {
                var newASCV = convertProjectsToASCV(project);
                allProjects.Add(newASCV);
            }

            return allProjects;
        }

        private async Task<List<AccountingStringComponentValues>> BuildAllGLAccounts()
        {
            List<AccountingStringComponentValues> AllGlAccounts = new List<AccountingStringComponentValues>();
            try
            {
                var glAccounts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", "");
                var glClassDefs = await DataReader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
                var glAcctCCs = await DataReader.BulkReadRecordAsync<DataContracts.GlAcctsCc>("GL.ACCTS.CC", "");
                var fiscalYearDataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);

                foreach (var glAccount in glAccounts)
                {
                    var glAcctCc = glAcctCCs.FirstOrDefault(x => x.Recordkey == glAccount.Recordkey);
                    var newASCV = ConvertGLtoASCV(glAccount, glClassDefs, glAcctCc, fiscalYearDataContract);

                    AllGlAccounts.Add(newASCV);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed becuase " + e.Message);
            }

            return AllGlAccounts;
        }
        

        private AccountingStringComponentValues convertProjectsToASCV(Projects project)
        {
            AccountingStringComponentValues newASCV = new AccountingStringComponentValues()
            {
                Guid = project.RecordGuid,
                AccountDef = "Project",
                AccountNumber = project.PrjRefNo,
                Description = project.PrjTitle,
                Type = "expense"
            };

            switch (project.PrjCurrentStatus)
            {
                case "A":
                    newASCV.Status = "available";
                    break;
                case "X":
                    newASCV.Status = "unavailable";
                    break;
                case "I":
                    newASCV.Status = "unavailable";
                    break;
            }
            return newASCV;
        }        

        private AccountingStringComponentValues ConvertGLtoASCV(GlAccts glAccount, Glclsdef glClassDef, GlAcctsCc glAcctCC, Fiscalyr fiscalYearDataContract)
        {
            AccountingStringComponentValues newASCV = new AccountingStringComponentValues();
            try
            {
                newASCV = new AccountingStringComponentValues()
                {
                    Guid = glAccount.RecordGuid,
                    AccountDef = "GL",
                    AccountNumber = glAccount.Recordkey
                };
            }
            catch (Exception e)
            {
                throw new Exception("failed to load accountingstring");
            }

            try
            {
                // get the Description for this GL
                if (glAcctCC != null)
                {
                    newASCV.Description = glAcctCC.GlccAcctDesc;
                }
            }
            catch (Exception e)
            {
                throw new Exception("failed to load description");
            }

            try
            {
                //get the Status
                if (fiscalYearDataContract != null && glAccount.MemosEntityAssociation != null && glAccount.MemosEntityAssociation.Count > 0)
                {
                    string fiscalYearStatus = glAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYearDataContract.CfCurrentFiscalYear).GlFreezeFlagsAssocMember;
                    if ((fiscalYearStatus == "O" || fiscalYearStatus == "I") && glAccount.GlInactive == "A")
                    {
                        newASCV.Status = "available";
                    }
                    else if (glAccount.GlInactive == "I" || fiscalYearStatus == "F")
                    {
                        newASCV.Status = "unavailable";
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(" failed in status retrieval; GL number " + glAccount.Recordkey);
            }


            try
            {
                if (glClassDef.GlClassLocation != null)
                {
                    int startPos = glClassDef.GlClassLocation[0].GetValueOrDefault() - 1;
                    int length = glClassDef.GlClassLocation[1].GetValueOrDefault();

                    string component = glAccount.Recordkey.Substring(startPos, length);

                    var test = glClassDef.GlClassAssetValues.FirstOrDefault(x => x == component);

                    if (test != null)
                    {
                        newASCV.Type = "asset";
                    }
                    else
                    {
                        test = glClassDef.GlClassLiabilityValues.FirstOrDefault(x => x == component);
                        if (test != null)
                        {
                            newASCV.Type = "liability";
                        }
                        else
                        {
                            test = glClassDef.GlClassFundBalValues.FirstOrDefault(x => x == component);
                            if (test != null)
                            {
                                newASCV.Type = "fundBalance";
                            }
                            else
                            {
                                test = glClassDef.GlClassRevenueValues.FirstOrDefault(x => x == component);
                                if (test != null)
                                {
                                    newASCV.Type = "revenue";
                                }
                                else
                                {
                                    test = glClassDef.GlClassExpenseValues.FirstOrDefault(x => x == component);
                                    if (test != null)
                                    {
                                        newASCV.Type = "expense";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get type");
            }
            //Get the Account type

            return newASCV;
        }

        private Base.DataContracts.Defaults GetDefaults()
        {
            return GetOrAddToCache<Data.Base.DataContracts.Defaults>("CoreDefaults",
                () =>
                {
                    var coreDefaults = DataReader.ReadRecord<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Base.DataContracts.Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg>> BuildAllFiscalPeriodsIntgs()
        {
            var fiscalPeriodIntgEntities = new List<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg>();
            var fiscalPeriodIntgRecords = await DataReader.BulkReadRecordAsync<DataContracts.FiscalPeriodsIntg>("FISCAL.PERIODS.INTG", "BY FPI.FISCAL.YEAR BY FPI.FISCAL.PERIOD");

            var dataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);
            if (dataContract == null)
            {
                throw new ConfigurationException("Fiscal year data is not set up.");
            }

            if (!dataContract.FiscalStartMonth.HasValue)
            {
                throw new ConfigurationException("Fiscal year start month must have a value.");
            }

            if (dataContract.FiscalStartMonth < 1 || dataContract.FiscalStartMonth > 12)
            {
                throw new ConfigurationException("Fiscal year start month must be in between 1 and 12.");
            }
            foreach (var fiscalPeriodIntgRecord in fiscalPeriodIntgRecords)
            {
                var monthName = string.Empty;
                var yearName = string.Empty;
                
                var fiscalPeriodIntg = new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg(fiscalPeriodIntgRecord.RecordGuid, fiscalPeriodIntgRecord.Recordkey);

                if ((fiscalPeriodIntgRecord.FpiFiscalPeriod.HasValue) && (fiscalPeriodIntgRecord.FpiFiscalYear.HasValue))
                {
                    Dictionary<int, string> mnthDict = GetMonthsTranslationDictionary(dataContract.FiscalStartMonth.Value, fiscalPeriodIntgRecord.FpiFiscalYear.Value);
                    string[] monthYear = mnthDict[fiscalPeriodIntgRecord.FpiFiscalPeriod.Value].Split("*".ToArray());

                    if (mnthDict != null && mnthDict.Any())
                    {
                        monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt16(monthYear[0]));
                        fiscalPeriodIntg.Month = Convert.ToInt32(monthYear[0]);
                        fiscalPeriodIntg.Year = Convert.ToInt32(monthYear[1]);

                        yearName = Convert.ToString(monthYear[1]);
                        fiscalPeriodIntg.FiscalYear = fiscalPeriodIntgRecord.FpiFiscalYear;
                    }
                }
                fiscalPeriodIntg.Title = (string.Concat(monthName, " ", yearName)).Trim();
                fiscalPeriodIntg.Status = fiscalPeriodIntgRecord.FpiCurrentStatus;

                fiscalPeriodIntgEntities.Add(fiscalPeriodIntg);
            }
            return fiscalPeriodIntgEntities;
        }

        public async Task<string> GetCorpNameAsync()
        {
            var defaults = this.GetDefaults();

            if (defaults == null)
            {
                throw new ApplicationException("Unable to retrieve defaults.");
            }
            var defaultCorpId = defaults.DefaultHostCorpId;
            if (string.IsNullOrEmpty(defaultCorpId))
            {
                throw new ApplicationException("Unable to retrieve default host corp id.");
            }
            var corpContract = await DataReader.ReadRecordAsync<Base.DataContracts.Corp>("PERSON", defaultCorpId);
            if (corpContract.CorpName == null || !corpContract.CorpName.Any())
            {
                throw new ApplicationException("Institution must have a name.");
            }
            return String.Join(" ", corpContract.CorpName.Where(x => !string.IsNullOrEmpty(x)));

        }

        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear>> BuildAllFiscalYears()
        {
            var institutionName = string.Empty;
            var defaultCorpId = string.Empty;

            var fiscalYearsEntities = new List<Domain.ColleagueFinance.Entities.FiscalYear>();

            var defaults = this.GetDefaults();
            if (defaults != null)
            {
                defaultCorpId = defaults.DefaultHostCorpId;
                var corpContract = await DataReader.ReadRecordAsync<Base.DataContracts.Corp>("PERSON", defaultCorpId);
                if (corpContract.CorpName == null || !corpContract.CorpName.Any())
                {
                    throw new ApplicationException("Institution must have a name.");
                }
                institutionName = String.Join(" ", corpContract.CorpName.Where(x => !string.IsNullOrEmpty(x)));
            }

            var genLdgrRecords = await DataReader.BulkReadRecordAsync<DataContracts.GenLdgr>("GEN.LDGR", " WITH GEN.LDGR.TYPE NE 'CONTROL'");

            if (genLdgrRecords == null || genLdgrRecords.Count <= 0)
                throw new ConfigurationException("No fiscal years have been set up.");
            var dataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);

            if (dataContract == null)
            {
                throw new ConfigurationException("Fiscal year data is not set up.");
            }

            if (!dataContract.FiscalStartMonth.HasValue)
            {
                throw new ConfigurationException("Fiscal year start month must have a value.");
            }

            if (dataContract.FiscalStartMonth < 1 || dataContract.FiscalStartMonth > 12)
            {
                throw new ConfigurationException("Fiscal year start month must be in between 1 and 12.");
            }

            foreach (var genLdgrRecord in genLdgrRecords)
            {
                var fiscalYear = new Domain.ColleagueFinance.Entities.FiscalYear(genLdgrRecord.RecordGuid, genLdgrRecord.Recordkey);
                fiscalYear.Title = genLdgrRecord.GenLdgrDescription;
                fiscalYear.Status = genLdgrRecord.GenLdgrStatus;
                fiscalYear.HostCorpId = defaultCorpId;
                fiscalYear.InstitutionName = institutionName;
                fiscalYear.FiscalStartMonth = dataContract.FiscalStartMonth;
                fiscalYear.CurrentFiscalYear = dataContract.CurrentFiscalYear;
                fiscalYearsEntities.Add(fiscalYear);
            }
            return fiscalYearsEntities;
        }

        /// <summary>
        /// Gets the dictionary of month*year dictionary
        /// </summary>
        /// <param name="fiscalStartMonth"></param>
        /// <param name="fpiFiscalYear"></param>
        /// <returns></returns>
        private Dictionary<int, string> GetMonthsTranslationDictionary(int fiscalStartMonth, int fpiFiscalYear)
        {
            if (fiscalStartMonth < 1 || fiscalStartMonth > 12)
            {
                throw new InvalidOperationException(string.Format("Fiscal start month value {0} is invalid.", fiscalStartMonth));
            }

            int startMnth = fiscalStartMonth;
            int year = fpiFiscalYear;
            int tempYear = 0;
            Dictionary<int, string> mnthYearDict = new Dictionary<int, string>();

            if (fiscalStartMonth != 1)
            {
                for (int i = 1; i <= 12; i++)
                {
                    if (startMnth > 12)
                    {
                        startMnth = 1;
                        tempYear = year;
                        mnthYearDict.Add(i, string.Concat(startMnth.ToString(), "*", (year).ToString()));
                        startMnth++;
                    }
                    else
                    {
                        if (tempYear == 0)
                        {
                            mnthYearDict.Add(i, string.Concat(startMnth.ToString(), "*", (year - 1).ToString()));
                            startMnth++;
                        }
                        else
                        {
                            mnthYearDict.Add(i, string.Concat(startMnth.ToString(), "*", year.ToString()));
                            startMnth++;
                        }
                    }
                }
            }
            else
            {
                for (int j = 1; j <= 12; j++)
                {
                    mnthYearDict.Add(j, string.Concat(j, "*", year));
                }
            }
            return mnthYearDict;
        }

        /// <summary>
        /// Gets accounting string component values V12.
        /// </summary>
        /// <param name="Offset"></param>
        /// <param name="Limit"></param>
        /// <param name="component"></param>
        /// <param name="transactionStatus"></param>
        /// <param name="typeAccount"></param>
        /// <param name="typeFund"></param>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<AccountingStringComponentValues>, int>> GetAccountingStringComponentValues2Async(int Offset, int Limit, string component,
           string transactionStatus, string typeAccount, string typeFund, bool ignoreCache)
        {
            List<AccountingStringComponentValues> glAccounts = new List<AccountingStringComponentValues>();
            List<AccountingStringComponentValues> projects = new List<AccountingStringComponentValues>();
            if (ignoreCache)
            {
                switch (component)
                {
                    case "GL.ACCT":
                        glAccounts = await BuildAllGLAccounts2();
                        break;
                    case "PROJECT":
                        projects = await BuildAllProjects();
                        break;
                    default:
                        glAccounts = await BuildAllGLAccounts2();
                        projects = await BuildAllProjects();
                        break;
                }
            }
            else
            {
                string GlcacheId = "AllAccountStringCompValuesGLA";
                string prjtCacheId = "AllAccountStringCompValuesPRJTS";
                switch (component)
                {
                    case "GL.ACCT":
                        glAccounts = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(GlcacheId, async () => await this.BuildAllGLAccounts2(), Level1CacheTimeoutValue);
                        break;
                    case "PROJECT":
                        projects = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(prjtCacheId, async () => await this.BuildAllProjects(), Level1CacheTimeoutValue);
                        break;
                    default:
                        glAccounts = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(GlcacheId, async () => await this.BuildAllGLAccounts2(), Level1CacheTimeoutValue);
                        projects = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(prjtCacheId, async () => await this.BuildAllProjects(), Level1CacheTimeoutValue);
                        break;
                }
            }

            List<AccountingStringComponentValues> allASCV = new List<AccountingStringComponentValues>();
            allASCV.AddRange(glAccounts);
            allASCV.AddRange(projects);
            if (!string.IsNullOrEmpty(transactionStatus) && allASCV.Count > 0)
            {
                var temp = allASCV.Where(x => x.Status == transactionStatus);
                allASCV = new List<AccountingStringComponentValues>();
                allASCV.AddRange(temp);
            }
            if (!string.IsNullOrEmpty(typeAccount) && allASCV.Count > 0)
            {
                var temp = allASCV.Where(x => x.Type == typeAccount);
                allASCV = new List<AccountingStringComponentValues>();
                allASCV.AddRange(temp);
            }

            allASCV.OrderBy(o => o.AccountNumber);
            int totalCount = allASCV.Count();
            var pageList = allASCV.Skip(Offset).Take(Limit).ToArray();

            return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(pageList, totalCount);
        }        
               
        private async Task<List<AccountingStringComponentValues>> BuildAllGLAccounts2()
        {
            List<AccountingStringComponentValues> AllGlAccounts = new List<AccountingStringComponentValues>();
            try
            {
                var glAccounts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", "");
                var glClassDefs = await DataReader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
                var glAcctCCs = await DataReader.BulkReadRecordAsync<DataContracts.GlAcctsCc>("GL.ACCTS.CC", "");
                var fiscalYearDataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);

                foreach (var glAccount in glAccounts)
                {
                    var glAcctCc = glAcctCCs.FirstOrDefault(x => x.Recordkey == glAccount.Recordkey);
                    var newASCV = ConvertGLtoASCV2Async(glAccount, glClassDefs, glAcctCc, fiscalYearDataContract);

                    AllGlAccounts.Add(newASCV);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw new RepositoryException("Failed to retrieve data.");
            }

            return AllGlAccounts;
        }

        /// <summary>
        /// Gets accounting string component value by guid.
        /// </summary>
        /// <param name="Guid"></param>
        /// <returns></returns>
        public async Task<AccountingStringComponentValues> GetAccountingStringComponentValue3ByGuid(string Guid)
        {
            var recordInfo = await GetRecordInfoFromGuidAsync(Guid);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey))
            {
                throw new KeyNotFoundException(string.Format("No accounting string componenent value was found for guid '{0}'. ", Guid));
            }
            AccountingStringComponentValues ASCV = new AccountingStringComponentValues();
            switch (recordInfo.Entity)
            {
                case "PROJECTS":
                    var project = await DataReader.ReadRecordAsync<Projects>(recordInfo.PrimaryKey);
                    if (project == null)
                    {
                        throw new KeyNotFoundException(string.Format("No accounting string componenent value was found for guid '{0}'. ", Guid));
                    }
                    ASCV = ConvertProjectsToASCV2(project);
                    break;
                case "GL.ACCTS":
                    var glAccount = await DataReader.ReadRecordAsync<DataContracts.GlAccts>(recordInfo.PrimaryKey);
                    if (glAccount == null)
                    {
                        throw new KeyNotFoundException(string.Format("No accounting string componenent value was found for guid '{0}'. ", Guid));
                    }
                    var glClassDef = await DataReader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
                    var glAcctCC = await DataReader.ReadRecordAsync<DataContracts.GlAcctsCc>(recordInfo.PrimaryKey);
                    var fiscalYearDataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);
                    ASCV = ConvertGLtoASCV3Async(glAccount, glClassDef, glAcctCC, fiscalYearDataContract);
                    break;
                default:
                    throw new KeyNotFoundException(string.Format("No accounting string componenent value was found for guid '{0}'. ", Guid));
            }

            return ASCV;

        }

        /// <summary>
        /// Gets accounting string component values V15.
        /// </summary>
        /// <param name="Offset"></param>
        /// <param name="Limit"></param>
        /// <param name="component"></param>
        /// <param name="transactionStatus"></param>
        /// <param name="typeAccount"></param>
        /// <param name="typeFund"></param>
        /// <param name="status"></param>
        /// <param name="grants"></param>
        /// <param name="effectiveOn"></param>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<AccountingStringComponentValues>, int>> GetAccountingStringComponentValues3Async(int Offset, int Limit, string component,
           string typeAccount, string status, IEnumerable<string> grants, DateTime? effectiveOn, bool ignoreCache)
        {
            List<AccountingStringComponentValues> glAccounts = new List<AccountingStringComponentValues>();
            List<AccountingStringComponentValues> projects = new List<AccountingStringComponentValues>();

            if (ignoreCache)
            {
                switch (component)
                {
                    case "GL.ACCT":
                        glAccounts = await BuildAllGLAccounts3();
                        break;
                    case "PROJECT":
                        projects = await BuildAllProjects2();
                        break;
                    default:
                        glAccounts = await BuildAllGLAccounts3();
                        projects = await BuildAllProjects2();
                        break;
                }
            }
            else
            {
                string GlcacheId = "AllAccountStringCompValuesGLA";
                string prjtCacheId = "AllAccountStringCompValuesPRJTS";
                switch (component)
                {
                    case "GL.ACCT":
                        glAccounts = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(GlcacheId, async () => await this.BuildAllGLAccounts3(), Level1CacheTimeoutValue);
                        break;
                    case "PROJECT":
                        projects = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(prjtCacheId, async () => await this.BuildAllProjects2(), Level1CacheTimeoutValue);
                        break;
                    default:
                        glAccounts = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(GlcacheId, async () => await this.BuildAllGLAccounts3(), Level1CacheTimeoutValue);
                        projects = await GetOrAddToCacheAsync<List<AccountingStringComponentValues>>(prjtCacheId, async () => await this.BuildAllProjects2(), Level1CacheTimeoutValue);
                        break;
                }
            }

            List<AccountingStringComponentValues> allASCV = new List<AccountingStringComponentValues>();
            allASCV.AddRange(glAccounts);
            allASCV.AddRange(projects);
            if (!string.IsNullOrEmpty(status) && allASCV.Any())
            {
                var temp = allASCV.Where(x => !string.IsNullOrWhiteSpace(x.Status) && x.Status.ToUpperInvariant() == status.ToUpperInvariant()).ToList();
                if(temp == null || !temp.Any())
                {
                    return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                }
                allASCV = new List<AccountingStringComponentValues>();
                allASCV.AddRange(temp);
            }
            if (!string.IsNullOrEmpty(typeAccount) && allASCV.Any())
            {
                var temp = allASCV.Where(x => !string.IsNullOrWhiteSpace(x.Type) && x.Type.ToUpperInvariant() == typeAccount.ToUpperInvariant());
                if (temp == null || !temp.Any())
                {
                    return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                }
                allASCV = new List<AccountingStringComponentValues>();
                allASCV.AddRange(temp);
            }

            //grants
            if(grants != null && grants.Any() && allASCV.Any())
            {
                var grIds = allASCV.Where(gr => gr.GrantIds != null && gr.GrantIds.Any()).SelectMany(id => id.GrantIds);
                var intersectIds = grants.ToList().Intersect(grIds.ToList());
                if(intersectIds == null || !intersectIds.Any())
                {
                    return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                }
                var ascvWithGrants = new List<AccountingStringComponentValues>();
                intersectIds.ToList().ForEach(id => 
                {
                    var temp = allASCV.Where(ascv => ascv.GrantIds.Contains(id));
                    ascvWithGrants.AddRange(temp);
                });
                allASCV = new List<AccountingStringComponentValues>();
                allASCV.AddRange(ascvWithGrants);
            }

            //effectiveOn
            if(effectiveOn.HasValue && allASCV.Any())
            {
                try
                {
                    var temp = allASCV
                    .Where(ascv => ((ascv.StartDate.HasValue && ascv.StartDate.Value.Date <= effectiveOn.Value.Date && (!string.IsNullOrEmpty(ascv.Status) && ascv.Status.Equals("available", StringComparison.OrdinalIgnoreCase)) || 
                    string.IsNullOrEmpty(ascv.Status))) || 
                    (ascv.Status.Equals("unavailable", StringComparison.OrdinalIgnoreCase) && ascv.EndDate.HasValue && ascv.EndDate.Value.Date >= effectiveOn.Value.Date)).ToList();

                    if (temp == null ||!temp.Any())
                    {
                        return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                    }
                    allASCV = new List<AccountingStringComponentValues>();
                    allASCV.AddRange(temp);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                }
            }

            allASCV.OrderBy(o => o.AccountNumber);
            int totalCount = allASCV.Count();
            var pageList = allASCV.Skip(Offset).Take(Limit).ToArray();

            return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(pageList, totalCount);
        }

        /// <summary>
        /// Builds GL Accounts.
        /// </summary>
        /// <returns></returns>
        private async Task<List<AccountingStringComponentValues>> BuildAllGLAccounts3()
        {
            List<AccountingStringComponentValues> AllGlAccounts = new List<AccountingStringComponentValues>();
            try
            {
                var glAccounts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", "");
                var glClassDefs = await DataReader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
                var glAcctCCs = await DataReader.BulkReadRecordAsync<DataContracts.GlAcctsCc>("GL.ACCTS.CC", "");
                var fiscalYearDataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);

                foreach (var glAccount in glAccounts)
                {
                    var glAcctCc = glAcctCCs.FirstOrDefault(x => x.Recordkey == glAccount.Recordkey);
                    var newASCV = ConvertGLtoASCV3Async(glAccount, glClassDefs, glAcctCc, fiscalYearDataContract);

                    AllGlAccounts.Add(newASCV);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw new RepositoryException("Failed to retrieve accounting string components.");
            }

            return AllGlAccounts;
        }

        /// <summary>
        /// Converts GL to accounting string component values.
        /// </summary>
        /// <param name="glAccount"></param>
        /// <param name="glClassDef"></param>
        /// <param name="glAcctCC"></param>
        /// <param name="fiscalYearDataContract"></param>
        /// <returns></returns>
        private AccountingStringComponentValues ConvertGLtoASCV3Async(GlAccts glAccount, Glclsdef glClassDef, GlAcctsCc glAcctCC, Fiscalyr fiscalYearDataContract)
        {
            AccountingStringComponentValues newASCV = new AccountingStringComponentValues();

            newASCV = new AccountingStringComponentValues()
            {
                Guid = glAccount.RecordGuid,
                AccountDef = "GL",
                AccountNumber = glAccount.Recordkey,
                Id = glAccount.Recordkey,
                PooleeAccounts = BuildPooleeAccountsAsync(glAccount.MemosEntityAssociation)
            };

            //effectiveStartOn
            newASCV.StartDate = glAccount.GlAcctsAddDate;
            //effectiveEndOn
            newASCV.EndDate = glAccount.GlAcctsChgdate;

            //Grant ids
            if (glAccount.GlProjectsId != null && glAccount.GlProjectsId.Any())
            {
                var glPrjIds = glAccount.GlProjectsId;
                newASCV.GrantIds.AddRange(glPrjIds);
            }

            // get the Description for this GL
            if (glAcctCC != null)
            {
                newASCV.Description = !string.IsNullOrEmpty(glAcctCC.GlccAcctDesc) ? glAcctCC.GlccAcctDesc : string.Empty;
            }

            //get the Status
            if (fiscalYearDataContract != null && glAccount.MemosEntityAssociation != null && glAccount.MemosEntityAssociation.Any())
            {
                var fiscalYearStatus = glAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYearDataContract.CfCurrentFiscalYear);
                if (fiscalYearStatus == null)
                {
                    throw new RepositoryException(string.Format("Current fiscal year not found for {0}: fiscalYearDataContract.CfCurrentFiscalYear."));
                }
                string glFFA = fiscalYearStatus.GlFreezeFlagsAssocMember;

                /*
                    Prior to v15, this logic was in the transactionStatus property. Move the logic to 'status' and check that it included GL.INACTIVE = blank (which is valid but wasn't 
                    mentioned in the transactionStatus logic). GL accounts, 
                    IF GL.INACTIVE = "A" or "", then
                    If any value of GL.FREEZE.FLAGS is "O" or "A", then return "available", else return "unavailable".
                    Else (GL.INACTIVE = "I"), return "unavailable".
                    The accounting strings must exist in an open or authorized fiscal year to be considered "available" as long as the account is neither inactive or frozen.
                */
                if (string.IsNullOrEmpty(glAccount.GlInactive)  || glAccount.GlInactive.Equals("A"))
                {
                    if (glFFA.Equals("A", StringComparison.OrdinalIgnoreCase) || glFFA.Equals("O", StringComparison.OrdinalIgnoreCase))
                    {
                        newASCV.Status = "available";
                    }
                    else
                    {
                        newASCV.Status = "unavailable";
                    }
                }
                else if (glAccount.GlInactive.Equals("I", StringComparison.OrdinalIgnoreCase))
                {
                    newASCV.Status = "unavailable";
                }
                else
                {
                    newASCV.Status = "unavailable";
                }
            }

            if (glClassDef.GlClassLocation != null)
            {
                int startPos = glClassDef.GlClassLocation.ElementAt(0).GetValueOrDefault() - 1;
                int length = glClassDef.GlClassLocation.ElementAt(1).GetValueOrDefault();

                string component = glAccount.Recordkey.Substring(startPos, length);

                var test = glClassDef.GlClassAssetValues.FirstOrDefault(x => x == component);

                if (test != null)
                {
                    newASCV.Type = "asset";
                }
                else
                {
                    test = glClassDef.GlClassLiabilityValues.FirstOrDefault(x => x == component);
                    if (test != null)
                    {
                        newASCV.Type = "liability";
                    }
                    else
                    {
                        test = glClassDef.GlClassFundBalValues.FirstOrDefault(x => x == component);
                        if (test != null)
                        {
                            newASCV.Type = "fundBalance";
                        }
                        else
                        {
                            test = glClassDef.GlClassRevenueValues.FirstOrDefault(x => x == component);
                            if (test != null)
                            {
                                newASCV.Type = "revenue";
                            }
                            else
                            {
                                test = glClassDef.GlClassExpenseValues.FirstOrDefault(x => x == component);
                                if (test != null)
                                {
                                    newASCV.Type = "expense";
                                }
                            }
                        }
                    }
                }
            }
            return newASCV;
        }

        /// <summary>
        /// Builds projects.
        /// </summary>
        /// <returns></returns>
        private async Task<List<AccountingStringComponentValues>> BuildAllProjects2()
        {
            List<AccountingStringComponentValues> allProjects = new List<AccountingStringComponentValues>();
            var projects = await DataReader.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", "");

            foreach (var project in projects)
            {
                var newASCV = ConvertProjectsToASCV2(project);
                allProjects.Add(newASCV);
            }

            return allProjects;
        }

        /// <summary>
        /// Converts to project entities.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private AccountingStringComponentValues ConvertProjectsToASCV2(Projects project)
        {
            AccountingStringComponentValues newASCV = new AccountingStringComponentValues()
            {
                Guid = project.RecordGuid,
                Id = project.Recordkey,
                AccountDef = "Project",
                AccountNumber = project.PrjRefNo,
                Description = project.PrjTitle,
                Type = "expense",
                StartDate = project.PrjStartDate,
                EndDate = project.PrjEndDate,
            };

            switch (project.PrjCurrentStatus)
            {
                case "A":
                    newASCV.Status = "available";
                    break;
                case "X":
                    newASCV.Status = "unavailable";
                    break;
                case "I":
                    newASCV.Status = "unavailable";
                    break;
                default:
                    newASCV.Status = "unavailable";
                    break;
            }
            //Grant ids
            newASCV.GrantIds.Add(project.Recordkey);
            return newASCV;
        }

        /// <summary>
        /// Converts GL to accounting string component values.
        /// </summary>
        /// <param name="glAccount"></param>
        /// <param name="glClassDef"></param>
        /// <param name="glAcctCC"></param>
        /// <param name="fiscalYearDataContract"></param>
        /// <returns></returns>
        private AccountingStringComponentValues ConvertGLtoASCV2Async(GlAccts glAccount, Glclsdef glClassDef, GlAcctsCc glAcctCC, Fiscalyr fiscalYearDataContract)
        {
            AccountingStringComponentValues newASCV = new AccountingStringComponentValues();
            try
            {
                newASCV = new AccountingStringComponentValues()
                {
                    Guid = glAccount.RecordGuid,
                    AccountDef = "GL",
                    AccountNumber = glAccount.Recordkey,
                    PooleeAccounts = BuildPooleeAccountsAsync(glAccount.MemosEntityAssociation)
                };
            }
            catch (Exception e)
            {
                throw new Exception("failed to load accountingstring");
            }

            try
            {
                // get the Description for this GL
                if (glAcctCC != null)
                {
                    newASCV.Description = glAcctCC.GlccAcctDesc;
                }
            }
            catch (Exception e)
            {
                throw new Exception("failed to load description");
            }

            try
            {
                //get the Status
                if (fiscalYearDataContract != null && glAccount.MemosEntityAssociation != null && glAccount.MemosEntityAssociation.Any())
                {
                    string fiscalYearStatus = glAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYearDataContract.CfCurrentFiscalYear).GlFreezeFlagsAssocMember;
                    if ((fiscalYearStatus == "O" || fiscalYearStatus == "I") && glAccount.GlInactive == "A")
                    {
                        newASCV.Status = "available";
                    }
                    else if (glAccount.GlInactive == "I" || fiscalYearStatus == "F")
                    {
                        newASCV.Status = "unavailable";
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(" failed in status retrieval; GL number " + glAccount.Recordkey);
            }


            try
            {
                if (glClassDef.GlClassLocation != null)
                {
                    int startPos = glClassDef.GlClassLocation[0].GetValueOrDefault() - 1;
                    int length = glClassDef.GlClassLocation[1].GetValueOrDefault();

                    string component = glAccount.Recordkey.Substring(startPos, length);

                    var test = glClassDef.GlClassAssetValues.FirstOrDefault(x => x == component);

                    if (test != null)
                    {
                        newASCV.Type = "asset";
                    }
                    else
                    {
                        test = glClassDef.GlClassLiabilityValues.FirstOrDefault(x => x == component);
                        if (test != null)
                        {
                            newASCV.Type = "liability";
                        }
                        else
                        {
                            test = glClassDef.GlClassFundBalValues.FirstOrDefault(x => x == component);
                            if (test != null)
                            {
                                newASCV.Type = "fundBalance";
                            }
                            else
                            {
                                test = glClassDef.GlClassRevenueValues.FirstOrDefault(x => x == component);
                                if (test != null)
                                {
                                    newASCV.Type = "revenue";
                                }
                                else
                                {
                                    test = glClassDef.GlClassExpenseValues.FirstOrDefault(x => x == component);
                                    if (test != null)
                                    {
                                        newASCV.Type = "expense";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get type");
            }
            //Get the Account type

            return newASCV;
        }
             
        /// <summary>
        /// Builds poolee accounts
        /// </summary>
        /// <param name="memosEntityAssociation"></param>
        /// <returns></returns>
        private Dictionary<string, string> BuildPooleeAccountsAsync(List<GlAcctsMemos> memosEntityAssociation)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (memosEntityAssociation != null && memosEntityAssociation.Any())
            {
                foreach (var item in memosEntityAssociation)
                {
                    if (!dict.ContainsKey(item.AvailFundsControllerAssocMember) &&
                        !string.IsNullOrEmpty(item.AvailFundsControllerAssocMember) &&
                        item.GlPooledTypeAssocMember != null && item.GlPooledTypeAssocMember.Equals("P"))
                    {
                        dict.Add(item.AvailFundsControllerAssocMember, item.GlBudgetLinkageAssocMember);
                    }
                }
            }
            return dict.Any() ? dict : null;
        }

        /// <summary>
        /// Gets a dictionary of guids for Poolee accounts.
        /// </summary>
        /// <param name="glAccts"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, string>> GetGuidsForPooleeGLAcctsInFiscalYearsAsync(IEnumerable<string> glAccts)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if(glAccts != null && glAccts.Any())
            {
                var personGuidLookup = glAccts
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .Distinct().ToList()
                   .ConvertAll(gl => new RecordKeyLookup("GL.ACCTS", gl, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!dict.ContainsKey(splitKeys[1]))
                    {
                        dict.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
            }

            return dict;
        }


        private async Task<IEnumerable<Domain.ColleagueFinance.Entities.AcctStructureIntg>> BuildAllAcctStructureIntg()
        {
            var acctStructureIntgEntities = new List<Domain.ColleagueFinance.Entities.AcctStructureIntg>();
            var acctStructureIntgRecords = await DataReader.BulkReadRecordAsync<DataContracts.AcctStructureIntg>("ACCT.STRUCTURE.INTG", "");
            foreach (var intgRecord in acctStructureIntgRecords)
            {
                var acctStructureIntgEntity = new Domain.ColleagueFinance.Entities.AcctStructureIntg(intgRecord.RecordGuid,intgRecord.Recordkey, intgRecord.AsiDesc);
                acctStructureIntgEntity.Type = intgRecord.AsiComponentType;
                acctStructureIntgEntity.ParentSubComponent = intgRecord.AsiParent;
                acctStructureIntgEntity.FileName = intgRecord.AsiFileName;
                acctStructureIntgEntity.StartPosition = intgRecord.AsiStartPos;
                acctStructureIntgEntity.Length = intgRecord.AsiLength;
                acctStructureIntgEntities.Add(acctStructureIntgEntity);
            }
            return acctStructureIntgEntities;
        }
        #endregion

    }
}

