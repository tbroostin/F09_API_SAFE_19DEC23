// Copyright 2014-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Services;
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
        const string AllAccountingStringComponentValuesCache = "AllAccountingStringComponentValues";

        const int ColleagueFinanceReferenceDataCacheTimeout = 20; // Clear from cache every 20 minutes

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
                throw new KeyNotFoundException(string.Format("No accounting string component value was found for guid '{0}'. ", Guid));
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
                throw new KeyNotFoundException(string.Format("No accounting string component value was found for guid '{0}'. ", Guid));
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
                    ASCV = ConvertGLtoASCV2Async(glAccount, glClassDef, glAcctCC, fiscalYearDataContract);
                    break;
                default:
                    throw new KeyNotFoundException(string.Format("No accounting string component value was found for guid '{0}'. ", Guid));
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
                accountsPayableSource.Source = accountsPayableSourcesRecord.AptSource;
                accountsPayableSourcesEntities.Add(accountsPayableSource);
            }


            return accountsPayableSourcesEntities;
        }

        /// <summary>
        /// Get guid for AccountsPayableSource code
        /// </summary>
        /// <param name="code">AccountsPayableSource code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAccountsPayableSourceGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            IEnumerable<AccountsPayableSources> allCodesCache = null;
            try
            {
                allCodesCache = await GetAccountsPayableSourcesAsync(false);
            }
            catch
            {
                throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TYPES', Record ID:'", code, "'"));
            }
            AccountsPayableSources codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAccountsPayableSourcesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'AP.TYPES', Record ID:'", code, "'"));
            }
            return guid;

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
                itemCode => new AccountsPayableTax(itemCode.Recordkey, itemCode.ApTaxDesc) {
                    AllowAccountsPayablePurchaseEntry = !string.IsNullOrEmpty(itemCode.ApTaxAppurEntryFlag) ? (itemCode.ApTaxAppurEntryFlag.ToLowerInvariant() == "y" ? true : false) :false,
                    IsUseTaxCategory = !string.IsNullOrEmpty(itemCode.ApUseTaxFlag) ? (itemCode.ApUseTaxFlag.ToLowerInvariant() == "y" ? true : false) : false,
                    TaxCategory = itemCode.ApTaxCategory
                });
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
                itemCode => new AccountsPayableType(itemCode.Recordkey, itemCode.ApTypesDesc) { BankCode = itemCode.AptBankCode, Source =itemCode.AptSource });
            });
        }

        /// <summary>
        /// Get guid for AssetCategories code
        /// </summary>
        /// <param name="code">AssetCategories code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAssetCategoriesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            IEnumerable<Domain.ColleagueFinance.Entities.AssetCategories> allCodesCache = null;
            try
            {
                allCodesCache = await GetAssetCategoriesAsync(false);
            }
            catch
            {
                throw new RepositoryException(string.Concat("No Guid found, Entity:'ASSET.CATEGORIES', Record ID:'", code, "'"));
            }
            Domain.ColleagueFinance.Entities.AssetCategories codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAssetCategoriesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ASSET.CATEGORIES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ASSET.CATEGORIES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ASSET.CATEGORIES', Record ID:'", code, "'"));
            }
            return guid;

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
        /// Get guid for AssetTypes code
        /// </summary>
        /// <param name="code">AssetTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAssetTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            IEnumerable<Domain.ColleagueFinance.Entities.AssetTypes> allCodesCache = null;
            try
            {
                allCodesCache = await GetAssetTypesAsync(false);
            }
            catch
            {
                throw new RepositoryException(string.Concat("No Guid found, Entity:'ASSET.TYPES', Record ID:'", code, "'"));
            }
            Domain.ColleagueFinance.Entities.AssetTypes codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAssetTypesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ASSET.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ASSET.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ASSET.TYPES', Record ID:'", code, "'"));
            }
            return guid;

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
                logger.Info("Unable to read records from ASSET.TYPES. Might be empty.");
                return assetTypeEntities;
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
        /// Get guid for CommodityCode code
        /// </summary>
        /// <param name="code">CommodityCode code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCommodityCodeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCommodityCodesAsync(false);
            CommodityCode codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCommodityCodesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'COMMODITY.CODES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'COMMODITY.CODES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'COMMODITY.CODES', Record ID:'", code, "'"));
            }
            return guid;
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
        /// Get guid for CommodityUnitType code
        /// </summary>
        /// <param name="code">CommodityUnitType code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetCommodityUnitTypeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetCommodityUnitTypesAsync(false);
            CommodityUnitType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetCommodityUnitTypesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'UNIT.ISSUES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'UNIT.ISSUES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'UNIT.ISSUES', Record ID:'", code, "'"));
            }
            return guid;
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
        /// Get a collection of FxaTransferFlags
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of FxaTransferFlags</returns>
        public async Task<IEnumerable<FxaTransferFlags>> GetFxaTransferFlagsAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<FxaTransferFlags>("CF", "FXA.TRANSFER.FLAGS",
                (cl, g) => new FxaTransferFlags(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }


        /// <summary>
        /// Get guid for FxaTransferFlag
        /// </summary>
        /// <param name="code">FxaTransferFlag code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetFxaTransferFlagGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetFxaTransferFlagsAsync(false);
            FxaTransferFlags codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetFxaTransferFlagsAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CF.VALCODES - FXA.TRANSFER.FLAGS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CF.VALCODES - FXA.TRANSFER.FLAGS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CF.VALCODES - FXA.TRANSFER.FLAGS', Record ID:'", code, "'"));
            }
            return guid;
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
        /// Get guid for FreeOnBoardType code
        /// </summary>
        /// <param name="code">FreeOnBoardType code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetFreeOnBoardTypeGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetFreeOnBoardTypesAsync(false);
            FreeOnBoardType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetFreeOnBoardTypesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'FOBS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'FOBS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'FOBS', Record ID:'", code, "'"));
            }
            return guid;

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
        /// Get guid for ShippingMethod code
        /// </summary>
        /// <param name="code">ShippingMethod code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetShippingMethodGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetShippingMethodsAsync(false);
            ShippingMethod codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetShippingMethodsAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SHIP.VIAS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SHIP.VIAS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SHIP.VIAS', Record ID:'", code, "'"));
            }
            return guid;

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
        /// Get guid for ShipToDestination code
        /// </summary>
        /// <param name="code">ShipToDestination code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetShipToDestinationGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetShipToDestinationsAsync(false);
            ShipToDestination codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetShipToDestinationsAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SHIP.TO.CODES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SHIP.TO.CODES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'SHIP.TO.CODES', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of IntgVendorAddressUsages
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgVendorAddressUsages</returns>
        public async Task<IEnumerable<IntgVendorAddressUsages>> GetIntgVendorAddressUsagesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<IntgVendorAddressUsages>("CF", "INTG.VENDOR.ADDRESS.USAGES",
                (cl, g) => new IntgVendorAddressUsages(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember)), bypassCache: ignoreCache);
        }

        /// <summary>
        /// Get guid for IntgVendorAddressUsages code
        /// </summary>
        /// <param name="code">IntgVendorAddressUsages code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetIntgVendorAddressUsagesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetIntgVendorAddressUsagesAsync(false);
            IntgVendorAddressUsages codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetIntgVendorAddressUsagesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CF.VALCODES - INTG.VENDOR.ADDRESS.USAGES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CF.VALCODES - INTG.VENDOR.ADDRESS.USAGES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CF.VALCODES - INTG.VENDOR.ADDRESS.USAGES', Record ID:'", code, "'"));
            }
            return guid;

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
        /// Get guid for VendorHoldReasons code
        /// </summary>
        /// <param name="code">VendorHoldReasons code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetVendorHoldReasonsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetVendorHoldReasonsAsync(false);
            VendorHoldReasons codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetVendorHoldReasonsAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CF.VALCODES - INTG.VENDOR.HOLD.REASONS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CF.VALCODES - INTG.VENDOR.HOLD.REASONS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'CF.VALCODES - INTG.VENDOR.HOLD.REASONS', Record ID:'", code, "'"));
            }
            return guid;

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
        /// Get guid for VendorTerm code
        /// </summary>
        /// <param name="code">VendorTerm code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetVendorTermGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetVendorTermsAsync(false);
            VendorTerm codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetVendorTermsAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'VENDOR.TERMS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'VENDOR.TERMS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'VENDOR.TERMS', Record ID:'", code, "'"));
            }
            return guid;

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
        /// Get guid for VendorTypes code
        /// </summary>
        /// <param name="code">VendorTypes code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetVendorTypesGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetVendorTypesAsync(false);
            VendorType codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetVendorTypesAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'VENDOR.TYPES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'VENDOR.TYPES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'VENDOR.TYPES', Record ID:'", code, "'"));
            }
            return guid;

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


        /// <summary>
        /// Get a collection of ShipToCodes
        /// </summary>
        /// <returns>Collection of ShipToCodes</returns>
        public async Task<IEnumerable<ShipToCode>> GetShipToCodesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<ShipToCode>>("ShipToCodes", async () =>
            {
                return await GetCodeItemAsync<ShipToCodes, ShipToCode>("AllShipToCodes", "SHIP.TO.CODES",
                itemCode => new ShipToCode(itemCode.Recordkey, itemCode.ShptName));
            });
        }

        /// <summary>
        /// Get a collection of ShipViaCodes
        /// </summary>
        /// <returns>Collection of ShipViaCodes</returns>
        public async Task<IEnumerable<ShipViaCode>> GetShipViaCodesAsync()
        {
            return await GetOrAddToCacheAsync<IEnumerable<ShipViaCode>>("ShipViaCodes", async () =>
            {
                return await GetCodeItemAsync<ShipVias, ShipViaCode>("AllShipViaCodes", "SHIP.VIAS",
                itemCode => new ShipViaCode(itemCode.Recordkey, itemCode.ShipViasDesc));
            });
        }

        /// <summary>
        /// Get a collection of Commodity Codes
        /// </summary>
        /// <returns>Collection of Commodity Codes</returns>
        public async Task<IEnumerable<Domain.ColleagueFinance.Entities.ProcurementCommodityCode>> GetAllCommodityCodesAsync()
        {
            return await GetOrAddToCacheAsync("ProcurementCommodityCodes", async () =>
            {
                return await GetCodeItemAsync("GetAllCommodityCodes", "COMMODITY.CODES",
               (DataContracts.CommodityCodes cc) => new Domain.ColleagueFinance.Entities.ProcurementCommodityCode(cc.Recordkey, cc.CmdtyDesc));

            });
        }

        /// <summary>
        /// Get a collection of fixed asset transfer flags
        /// </summary>
        /// <returns>Collection of FixedAssetsFlag</returns>
        public async Task<IEnumerable<FixedAssetsFlag>> GetFixedAssetTransferFlagsAsync()
        {
            return await GetOrAddToCacheAsync("FxaTransferFlags", async () =>
            {
                return  await GetValcodeAsync<FixedAssetsFlag>("CF", "FXA.TRANSFER.FLAGS",
                   fxaFlag => new FixedAssetsFlag(fxaFlag.ValInternalCodeAssocMember, fxaFlag.ValExternalRepresentationAssocMember));
            });
        }
        
        /// <summary>
        /// Gets CommodityUnitTypes
        /// </summary>
        /// <returns>List of unit types objects.</returns>
        public async Task<IEnumerable<CommodityUnitType>> GetAllCommodityUnitTypesAsync()
        {
            return await GetOrAddToCacheAsync("CommodityCodeUnitTypes", async () =>
            {
                return await GetCodeItemAsync("AllCommodityUnitTypes", "UNIT.ISSUES",
               (DataContracts.UnitIssues cc) => new Domain.ColleagueFinance.Entities.CommodityUnitType(cc.RecordGuid, cc.Recordkey, cc.UiDesc));

            });
        }

        /// <summary>
        /// Returns a Commodity Code
        /// </summary>
        /// <returns>Procurement Commodity Code entity</returns>
        public async Task<Domain.ColleagueFinance.Entities.ProcurementCommodityCode> GetCommodityCodeByCodeAsync(string recordKey)
        {
            var cc = await DataReader.ReadRecordAsync<CommodityCodes>(recordKey);
            if (cc != null)
            {
                var procurementCommodityCode = new Domain.ColleagueFinance.Entities.ProcurementCommodityCode(cc.Recordkey, cc.CmdtyDesc);
                procurementCommodityCode.DefaultDescFlag = !string.IsNullOrEmpty(cc.CmdtyDefaultDescFlag) ? cc.CmdtyDefaultDescFlag.ToUpper().Equals("Y") : false;
                procurementCommodityCode.FixedAssetsFlag = cc.CmdtyFixedAssetsFlag;
                procurementCommodityCode.Price = cc.CmdtyPrice;
                procurementCommodityCode.TaxCodes = cc.CmdtyTaxCodes;
                return procurementCommodityCode;
            }
            return null;
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
            catch(RepositoryException e)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw new RepositoryException("Failed to retrieve accounting string components.");
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
                RepositoryException exception = new RepositoryException();
                exception.AddError(new RepositoryError("avail.funds.controller.NotFoundInFiscalYear", string.Format("The record associated to the accounting string component value contains an invalid element. guid: '{0}'", glAccount.RecordGuid)));
                throw exception;
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
            

            if (dataContract != null && !dataContract.FiscalStartMonth.HasValue)
            {
                throw new ConfigurationException("Fiscal year start month must have a value.");
            }

            if (dataContract != null && (dataContract.FiscalStartMonth < 1 || dataContract.FiscalStartMonth > 12))
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
                
            var dataContract = await DataReader.ReadRecordAsync<Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", true);

            if (dataContract != null && !dataContract.FiscalStartMonth.HasValue)
            {
                throw new ConfigurationException("Fiscal year start month must have a value.");
            }

            if (dataContract != null && (dataContract.FiscalStartMonth < 1 || dataContract.FiscalStartMonth > 12))
            {
                throw new ConfigurationException("Fiscal year start month must be in between 1 and 12.");
            }
            if (genLdgrRecords != null && genLdgrRecords.Any())
            {
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
           string transactionStatus, string typeAccount)
        {
            List<AccountingStringComponentValues> glAccounts = new List<AccountingStringComponentValues>();
            List<AccountingStringComponentValues> projects = new List<AccountingStringComponentValues>();
            string[] glLimitingKeys = null;
            string[] prLimitingKeys = null;
            List<string> combinedIds = new List<string>();
            string[] glIds = null;
            string[] prIds = null;
            int totalCount = 0;

            string glcriteria = string.Empty;
            string prCriteria = string.Empty;

            #region component, either GL.ACCT or PROJECT or BOTH
            if (!string.IsNullOrEmpty(component))
            {
                if (component.Equals("GL.ACCT", StringComparison.OrdinalIgnoreCase))
                {
                    glLimitingKeys = await DataReader.SelectAsync("GL.ACCTS", string.Empty);
                    if (glLimitingKeys == null || !glLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                    }
                }
                else if (component.Equals("PROJECT", StringComparison.OrdinalIgnoreCase))
                {
                    prLimitingKeys = await DataReader.SelectAsync("PROJECTS", string.Empty);
                    if (prLimitingKeys == null || !prLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                    }
                }
            }
            else
            {
                glLimitingKeys = await DataReader.SelectAsync("GL.ACCTS", string.Empty);
                prLimitingKeys = await DataReader.SelectAsync("PROJECTS", string.Empty);
                //If both collections are empty then return empty set.
                if ((glLimitingKeys == null || !glLimitingKeys.Any()) && (prLimitingKeys == null || !prLimitingKeys.Any()))
                {
                    return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                }
            }
            #endregion

            #region transactionStatus, status
            if (!string.IsNullOrEmpty(transactionStatus))
            {
                glcriteria = "WITH GL.INACTIVE EQ 'A'";
                prCriteria = "WITH PRJ.CURRENT.STATUS EQ 'A'";
                glLimitingKeys = await DataReader.SelectAsync("GL.ACCTS", glLimitingKeys, glcriteria);
                prLimitingKeys = await DataReader.SelectAsync("PROJECTS", prLimitingKeys, prCriteria);
                //If both collections are empty then return empty set.
                if ((glLimitingKeys == null || !glLimitingKeys.Any()) && (prLimitingKeys == null || !prLimitingKeys.Any()))
                {
                    return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                }
            }
            #endregion

            #region type.account
            //TO DO: come back to it later
            if (!string.IsNullOrEmpty(typeAccount))
            {
                var glClassDef = await DataReader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
                int startPos = glClassDef.GlClassLocation.ElementAt(0).GetValueOrDefault() - 1;
                int length = glClassDef.GlClassLocation.ElementAt(1).GetValueOrDefault();

                switch (typeAccount)
                {
                    case "asset":
                        if (glClassDef.GlClassAssetValues != null && glClassDef.GlClassAssetValues.Any())
                        {
                            prLimitingKeys = null;
                            glLimitingKeys = glLimitingKeys
                                .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassAssetValues.Contains(k.Substring(startPos, length)))
                                .Select(i => i).ToArray();
                        }
                        else
                        {
                            return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                        }
                        break;
                    case "liability":
                        if (glClassDef.GlClassLiabilityValues != null && glClassDef.GlClassLiabilityValues.Any())
                        {
                            prLimitingKeys = null;
                            glLimitingKeys = glLimitingKeys
                                .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassLiabilityValues.Contains(k.Substring(startPos, length)))
                                .Select(i => i).ToArray();
                        }
                        else
                        {
                            return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                        }
                        break;
                    case "fundBalance":
                        if (glClassDef.GlClassFundBalValues != null && glClassDef.GlClassFundBalValues.Any())
                        {
                            prLimitingKeys = null;
                            glLimitingKeys = glLimitingKeys
                                .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassFundBalValues.Contains(k.Substring(startPos, length)))
                                .Select(i => i).ToArray();
                        }
                        else
                        {
                            return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                        }
                        break;
                    case "revenue":
                        if (glClassDef.GlClassRevenueValues != null && glClassDef.GlClassRevenueValues.Any())
                        {
                            prLimitingKeys = null;
                            glLimitingKeys = glLimitingKeys
                                .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassRevenueValues.Contains(k.Substring(startPos, length)))
                                .Select(i => i).ToArray();
                        }
                        else
                        {
                            return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                        }
                        break;
                    case "expense":
                        if (glClassDef.GlClassExpenseValues != null && glClassDef.GlClassExpenseValues.Any())
                        {
                            glLimitingKeys = glLimitingKeys
                                .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassExpenseValues.Contains(k.Substring(startPos, length)))
                                .Select(i => i).ToArray();
                        }
                        else
                        {
                            return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                        }
                        break;
                    default:
                        return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                }
            }
            #endregion

            //here combine the keys to get the total count.
            if (glLimitingKeys != null && glLimitingKeys.Any())
            {
                foreach (var glKey in glLimitingKeys)
                {
                    combinedIds.Add(string.Concat("GL*", glKey));
                }
            }

            if (prLimitingKeys != null && prLimitingKeys.Any())
            {
                foreach (var prKey in prLimitingKeys)
                {
                    combinedIds.Add(string.Concat("PR*", prKey));
                }
            }
            if(!combinedIds.Any())
            {
                return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
            }
            totalCount = combinedIds.Count();

            //sort & page
            combinedIds.Sort();
            var sublist = combinedIds.Skip(Offset).Take(Limit);

            //Get the ids used in bulk read.
            glIds = sublist.Where(i => !string.IsNullOrEmpty(i) && i.Split('*')[0].Equals("GL", StringComparison.OrdinalIgnoreCase)).Select(k => k.Split('*')[1]).ToArray();
            prIds = sublist.Where(i => !string.IsNullOrEmpty(i) && i.Split('*')[0].Equals("PR", StringComparison.OrdinalIgnoreCase)).Select(k => k.Split('*')[1]).ToArray();

            //component, either GL.ACCT or PROJECT
            switch (component)
            {
                case "GL.ACCT":
                    if (glLimitingKeys == null || !glLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                    }
                    glAccounts = await BuildAllGLAccounts2(glIds);
                    break;
                case "PROJECT":
                    if (prLimitingKeys == null || !prLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                    }
                    projects = await BuildAllProjects2(prIds);
                    break;
                default:
                    glAccounts = await BuildAllGLAccounts2(glIds);
                    projects = await BuildAllProjects2(prIds);
                    break;
            }

            List<AccountingStringComponentValues> allASCV = new List<AccountingStringComponentValues>();
            allASCV.AddRange(glAccounts);
            allASCV.AddRange(projects);

            return allASCV.Any() ? new Tuple<IEnumerable<AccountingStringComponentValues>, int>(allASCV, totalCount) :
                   new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
        }

        private async Task<List<AccountingStringComponentValues>> BuildAllGLAccounts2(IEnumerable<string> ids)
        {
            List<AccountingStringComponentValues> AllGlAccounts = new List<AccountingStringComponentValues>();
            try
            {
                var glAccounts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", ids.ToArray());
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
            catch(RepositoryException e)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw new RepositoryException("Failed to retrieve accounting string components.");
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
                throw new KeyNotFoundException(string.Format("No accounting string component value was found for guid '{0}'. ", Guid));
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
                    ASCV = ConvertProjectsToASCV2(project);
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
                    ASCV = ConvertGLtoASCV3Async(glAccount, glClassDef, glAcctCC, fiscalYearDataContract);
                    break;
                default:
                    throw new KeyNotFoundException(string.Format("No accounting string component value was found for guid '{0}'. ", Guid));
            }
            return ASCV;
        }

        /// <summary>
        /// Gets accounting string component values V15.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="component"></param>
        /// <param name="transactionStatus"></param>
        /// <param name="typeAccount"></param>
        /// <param name="typeFund"></param>
        /// <param name="status"></param>
        /// <param name="grants"></param>
        /// <param name="effectiveOn"></param>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<AccountingStringComponentValues>, int>> GetAccountingStringComponentValues3Async(int offset, int limit, string component,
           string typeAccount, string status, IEnumerable<string> grants, DateTime? effectiveOn)
        {
            List<AccountingStringComponentValues> glAccounts = new List<AccountingStringComponentValues>();
            List<AccountingStringComponentValues> projects = new List<AccountingStringComponentValues>();
            string[] glLimitingKeys = null;
            string[] prLimitingKeys = null;
            List<string> combinedIds = new List<string>();
            string[] glIds = null;
            string[] prIds = null;
            int totalCount = 0;

            string glcriteria = string.Empty;
            string prCriteria = string.Empty;
            string criteria = string.Empty;
            string[] subList = null;

            string accountingStringComponentValuesCache = CacheSupport.BuildCacheKey(AllAccountingStringComponentValuesCache, component, typeAccount, status, grants, 
                    effectiveOn != null && effectiveOn.HasValue ? effectiveOn : null);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                accountingStringComponentValuesCache,
                "",
                offset,
                limit,
                ColleagueFinanceReferenceDataCacheTimeout,
                async () => {

            /*
                Notes: 
                    If filters are not requested then we can cache the data?
                Filters: 
                    component: where to get data from, GL.ACCT or PROJECT or both
                    transactionStatus & status: If any value of GL.FREEZE.FLAGS is "O" or "A", then return "available". In case of PROJECT PrjCurrentStatus = "A"
                    account: Based on the glClassDef figure out the record keys
                    fund: This concept does not apply to Colleague so filtering on it cannot match any records. Therefore, return an empty set if it's used.
                    grants: Translate the GUID to a PROJECTS.CF key. Select GL.ACCTS with GL.PROJECTS.ID = the project.
                    effectiveOn: Look at glAccount.GlAcctsAddDate, glAccount.GlAcctsChgdate, project.PrjStartDate, project.PrjEndDate
            */
            #region component, either GL.ACCT or PROJECT or BOTH
            if (!string.IsNullOrEmpty(component))
            {
                if(component.Equals("GL.ACCT", StringComparison.OrdinalIgnoreCase))
                {
                    glLimitingKeys = await DataReader.SelectAsync("GL.ACCTS", string.Empty);
                    if(glLimitingKeys == null || !glLimitingKeys.Any())
                    {
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                            }
                }
                else if (component.Equals("PROJECT", StringComparison.OrdinalIgnoreCase))
                {
                    prLimitingKeys = await DataReader.SelectAsync("PROJECTS", string.Empty);
                    if (prLimitingKeys == null || !prLimitingKeys.Any())
                    {
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                            }
                }
            }
            else
            {
                glLimitingKeys = await DataReader.SelectAsync("GL.ACCTS", string.Empty);
                prLimitingKeys = await DataReader.SelectAsync("PROJECTS", string.Empty);
                //If both collections are empty then return empty set.
                if ((glLimitingKeys == null || !glLimitingKeys.Any()) && (prLimitingKeys == null || !prLimitingKeys.Any()))
                {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
            }
            #endregion

            #region transactionStatus, status
            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    glcriteria = "WITH GL.CURRENT.STATUS EQ 'A'";
                    prCriteria = "WITH PRJ.CURRENT.STATUS EQ 'A'";
                }
                else
                {
                    glcriteria = "WITH GL.CURRENT.STATUS EQ 'I'";
                    prCriteria = "WITH PRJ.CURRENT.STATUS NE 'A'";
                }
                glLimitingKeys = await DataReader.SelectAsync("GL.ACCTS", glLimitingKeys, glcriteria);
                prLimitingKeys = await DataReader.SelectAsync("PROJECTS", prLimitingKeys, prCriteria);
                //If both collections are empty then return empty set.
                if ((glLimitingKeys == null || !glLimitingKeys.Any()) && (prLimitingKeys == null || !prLimitingKeys.Any()))
                {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }

            }
                    #endregion

                    #region type.account
                    //TO DO: come back to it later
                    if (!string.IsNullOrEmpty(typeAccount))
                    {
                        var glClassDef = await DataReader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", true);
                        int startPos = glClassDef.GlClassLocation.ElementAt(0).GetValueOrDefault() - 1;
                        int length = glClassDef.GlClassLocation.ElementAt(1).GetValueOrDefault();

                        switch (typeAccount)
                        {
                            case "asset":
                                if (glClassDef.GlClassAssetValues != null && glClassDef.GlClassAssetValues.Any())
                                {
                                    prLimitingKeys = null;
                                    glLimitingKeys = glLimitingKeys
                                        .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassAssetValues.Contains(k.Substring(startPos, length)))
                                        .Select(i => i).ToArray();
                                }
                                else
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                                break;
                            case "liability":
                                if (glClassDef.GlClassLiabilityValues != null && glClassDef.GlClassLiabilityValues.Any())
                                {
                                    prLimitingKeys = null;
                                    glLimitingKeys = glLimitingKeys
                                        .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassLiabilityValues.Contains(k.Substring(startPos, length)))
                                        .Select(i => i).ToArray();
                                }
                                else
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                                break;
                            case "fundBalance":
                                if (glClassDef.GlClassFundBalValues != null && glClassDef.GlClassFundBalValues.Any())
                                {
                                    prLimitingKeys = null;
                                    glLimitingKeys = glLimitingKeys
                                        .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassFundBalValues.Contains(k.Substring(startPos, length)))
                                        .Select(i => i).ToArray();
                                }
                                else
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                                break;
                            case "revenue":
                                if (glClassDef.GlClassRevenueValues != null && glClassDef.GlClassRevenueValues.Any())
                                {
                                    prLimitingKeys = null;
                                    glLimitingKeys = glLimitingKeys
                                        .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassRevenueValues.Contains(k.Substring(startPos, length)))
                                        .Select(i => i).ToArray();
                                }
                                else
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                                break;
                            case "expense":
                                if (glClassDef.GlClassExpenseValues != null && glClassDef.GlClassExpenseValues.Any())
                                {
                                    glLimitingKeys = glLimitingKeys
                                        .Where(k => !string.IsNullOrEmpty(k) && glClassDef.GlClassExpenseValues.Contains(k.Substring(startPos, length)))
                                        .Select(i => i).ToArray();
                                }
                                else
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                                break;
                            default:
                                return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                    }
                    #endregion

                    #region grants
                    if (grants != null && grants.Any())
                    {                     
                        glcriteria = "WITH GL.PROJECTS.ID EQ '" + (string.Join(" ", grants.ToArray())).Replace(" ", "' '") + "'";
                        prCriteria = "WITH PROJECTS.ID EQ '" + (string.Join(" ", grants.ToArray())).Replace(" ", "' '") + "'";

                        glLimitingKeys = await DataReader.SelectAsync("GL.ACCTS", glLimitingKeys, glcriteria);
                        prLimitingKeys = await DataReader.SelectAsync("PROJECTS", prLimitingKeys, prCriteria);

                        //If both collections are empty then return empty set.
                        if ((glLimitingKeys == null || !glLimitingKeys.Any()) && (prLimitingKeys == null || !prLimitingKeys.Any()))
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                    }
                    #endregion

                    #region effectiveOn
                    if (effectiveOn.HasValue)
                    {
                        var date = await GetUnidataFormattedDate(effectiveOn.Value.ToShortDateString());
                        //GL.ACCT
                        glcriteria = string.Format("WITH GL.ACCTS.ADD.DATE <= '{0}' AND ((GL.INACTIVE EQ 'A' OR GL.INACTIVE EQ '') OR GL.INACTIVE EQ 'I' OR GL.ACCTS.CHGDATE >= '{0}')", date);
                        //PROJECT
                        prCriteria = string.Format("WITH PRJ.START.DATE <= '{0}' AND (PRJ.END.DATE EQ '' OR PRJ.END.DATE >= '{0}')", date);
                        glLimitingKeys = await DataReader.SelectAsync("GL.ACCTS", glLimitingKeys, glcriteria);
                        prLimitingKeys = await DataReader.SelectAsync("PROJECTS", prLimitingKeys, prCriteria);
                        //If both collections are empty then return empty set.
                        if ((glLimitingKeys == null || !glLimitingKeys.Any()) && (prLimitingKeys == null || !prLimitingKeys.Any()))
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }
                    }
                    #endregion

                    //here combine the keys to get the total count.
                    if (glLimitingKeys != null && glLimitingKeys.Any())
                    {
                        foreach (var glKey in glLimitingKeys)
                        {
                            combinedIds.Add(string.Concat("GL*", glKey));
                        }
                    }
                    if (prLimitingKeys != null && prLimitingKeys.Any())
                    {
                        foreach (var prKey in prLimitingKeys)
                        {
                            combinedIds.Add(string.Concat("PR*", prKey));
                        }
                    }

                    if (!combinedIds.Any())
                    {
                        return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                    }

                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = combinedIds.Distinct().ToList(),
                    };

                    return requirements;

                });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
            }
            
            subList = keyCache.Sublist.ToArray();

            totalCount = keyCache.TotalCount.Value;
            //totalCount = combinedIds.Count();

            //sort & page
            // combinedIds.Sort();
            //var sublist = combinedIds.Skip(Offset).Take(Limit);

            //Get the ids used in bulk read.
            glIds = subList.Where(i => !string.IsNullOrEmpty(i) && i.Split('*')[0].Equals("GL", StringComparison.OrdinalIgnoreCase)).Select(k => k.Split('*')[1]).ToArray();
            prIds = subList.Where(i => !string.IsNullOrEmpty(i) && i.Split('*')[0].Equals("PR", StringComparison.OrdinalIgnoreCase)).Select(k => k.Split('*')[1]).ToArray();

            //component, either GL.ACCT or PROJECT
            switch (component)
            {
                case "GL.ACCT":
                    if (glLimitingKeys == null || !glLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                    }
                    glAccounts = await BuildAllGLAccounts3(glIds);
                    break;
                case "PROJECT":
                    if (prLimitingKeys == null || !prLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
                    }
                    projects = await BuildAllProjects2(prIds);
                    break;
                default:
                    glAccounts = await BuildAllGLAccounts3(glIds);
                    projects = await BuildAllProjects2(prIds);
                    break;
            }

            List<AccountingStringComponentValues> allASCV = new List<AccountingStringComponentValues>();
            allASCV.AddRange(glAccounts);
            allASCV.AddRange(projects);

            return allASCV.Any() ? new Tuple<IEnumerable<AccountingStringComponentValues>, int>(allASCV, totalCount) :
                   new Tuple<IEnumerable<AccountingStringComponentValues>, int>(new List<AccountingStringComponentValues>(), 0);
        }

        /// <summary>
        /// Return a Unidata Formatted Date string from an input argument of string type
        /// </summary>
        /// <param name="date">String representing a Date</param>
        /// <returns>Unidata formatted Date string for use in Colleague Selection.</returns>
        private async Task<string> GetUnidataFormattedDate(string date)
        {
            var internationalParameters = await InternationalParametersAsync();
            var newDate = DateTime.Parse(date).Date;
            return UniDataFormatter.UnidataFormatDate(newDate, internationalParameters.HostShortDateFormat, internationalParameters.HostDateDelimiter);
        }

        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;
        /// <summary>
        /// Gets international parameters.
        /// </summary>
        /// <returns></returns>
        private async Task<Ellucian.Data.Colleague.DataContracts.IntlParams> InternationalParametersAsync()
        {

            if (_internationalParameters == null)
            {
                _internationalParameters = await GetInternationalParametersAsync();
            }
            return _internationalParameters;
        }

        /// <summary>
        /// Builds GL Accounts.
        /// </summary>
        /// <returns></returns>
        private async Task<List<AccountingStringComponentValues>> BuildAllGLAccounts3(IEnumerable<string> glIds)
        {
            List<AccountingStringComponentValues> AllGlAccounts = new List<AccountingStringComponentValues>();

            try
            {
                var glAccounts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", glIds.ToArray());
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
            catch(RepositoryException)
            {
                throw;
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

            try
            {
                newASCV = new AccountingStringComponentValues()
                {
                    Guid = glAccount.RecordGuid,
                    AccountDef = "GL",
                    AccountNumber = glAccount.Recordkey,
                    Id = glAccount.Recordkey,
                    PooleeAccounts = BuildPooleeAccountsAsync(glAccount.MemosEntityAssociation)
                };
            }
            catch
            {
                RepositoryException exception = new RepositoryException();
                exception.AddError(new RepositoryError("avail.funds.controller.NotFound", string.Format("The record associated to the accounting string component value contains an invalid element. guid: '{0}'", glAccount.RecordGuid)));
                throw exception;
            }

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
                    newASCV.Status = "unavailable";
                }
                else
                {
                    string glFFA = fiscalYearStatus.GlFreezeFlagsAssocMember;

                    /*
                        Prior to v15, this logic was in the transactionStatus property. Move the logic to 'status' and check that it included GL.INACTIVE = blank (which is valid but wasn't 
                        mentioned in the transactionStatus logic). GL accounts, 
                        IF GL.INACTIVE = "A" or "", then
                        If any value of GL.FREEZE.FLAGS is "O" or "A", then return "available", else return "unavailable".
                        Else (GL.INACTIVE = "I"), return "unavailable".
                        The accounting strings must exist in an open or authorized fiscal year to be considered "available" as long as the account is neither inactive or frozen.
                    */
                    if (string.IsNullOrEmpty(glAccount.GlInactive) || glAccount.GlInactive.Equals("A"))
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
            }
            else
            {
                newASCV.Status = "unavailable";
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
        private async Task<List<AccountingStringComponentValues>> BuildAllProjects2(IEnumerable<string> prjIds)
        {
            List<AccountingStringComponentValues> allProjects = new List<AccountingStringComponentValues>();
            var projects = await DataReader.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", prjIds.ToArray());

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
            catch
            {
                RepositoryException exception = new RepositoryException();
                exception.AddError(new RepositoryError("avail.funds.controller.NotFound", string.Format("The record associated to the accounting string component value contains an invalid element. guid: '{0}'", glAccount.RecordGuid)));
                throw exception;
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
                    var fiscalYearStatus = glAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYearDataContract.CfCurrentFiscalYear);
                    if (fiscalYearStatus == null)
                    {
                        newASCV.Status = "unavailable";
                    }
                    else
                    {
                        string glFFA = fiscalYearStatus.GlFreezeFlagsAssocMember;

                        if ((glFFA == "O" || glFFA == "I") && glAccount.GlInactive == "A")
                        {
                            newASCV.Status = "available";
                        }
                        else if (glAccount.GlInactive == "I" || glFFA == "F")
                        {
                            newASCV.Status = "unavailable";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                RepositoryException exception = new RepositoryException();
                exception.AddError(new RepositoryError("avail.funds.controller.NotFoundInFiscalYear", string.Format("The record associated to the accounting string component value contains an invalid element. guid: '{0}'", glAccount.RecordGuid)));
                throw exception;
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
                    if (!string.IsNullOrEmpty(item.AvailFundsControllerAssocMember) &&
                        !dict.ContainsKey(item.AvailFundsControllerAssocMember) &&
                        !string.IsNullOrEmpty(item.GlPooledTypeAssocMember)  && item.GlPooledTypeAssocMember.Equals("P"))
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

        /// <summary>
        /// Get a collection of tax forms
        /// </summary>
        /// <returns>Collection of TaxForm</returns>
        public async Task<IEnumerable<TaxForm>> GetTaxFormsAsync()
        {
            return await GetOrAddToCacheAsync("TaxFormCodes", async () =>
            {
                return await GetValcodeAsync<TaxForm>("CORE", "TAX.FORMS",
                   taxForm => new TaxForm(taxForm.ValInternalCodeAssocMember, taxForm.ValExternalRepresentationAssocMember));
            });
        }
        #endregion

    }
}

