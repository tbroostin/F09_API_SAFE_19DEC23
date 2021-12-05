// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;
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
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IRequisitionRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FixedAssetsRepository : BaseColleagueRepository, IFixedAssetsRepository
    {
        const string AllFixedAssetsCache = "AllFixedAssets";
        const int AllFixedAssetsCacheTimeout = 20; // Clear from cache every 20 minutes
        RepositoryException exception = null;

        /// <summary>
        /// The constructor to instantiate a fixed asset object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public FixedAssetsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        #region GET Methods

        /// <summary>
        /// Gets fixed assets by Guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Domain.ColleagueFinance.Entities.FixedAssets> GetFixedAssetByIdAsync(string guid)
        {
            DataContracts.FixedAssets fixedAssetData = null;
            Collection<DataContracts.Insurance> insuranceDC = null;
            Collection<DataContracts.CalcMethods> calcMethods = null;
            Collection<DataContracts.GlAccts> glAccounts = null;

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required.");
            }

            var key = await GetRecordKeyFromGuidAsync(guid);
            if (string.IsNullOrEmpty(key))
            {
                throw new KeyNotFoundException(string.Format("fixed-assets not found for GUID '{0}'", guid));
            }

            fixedAssetData = await DataReader.ReadRecordAsync<DataContracts.FixedAssets>(key);
            if (fixedAssetData == null)
            {
                throw new KeyNotFoundException(string.Format("fixed-assets not found for key: {0}", key));
            }
            try
            {
                var insuranceId = !string.IsNullOrEmpty(fixedAssetData.FixInsuranceId) ? fixedAssetData.FixInsuranceId : null;
                insuranceDC = await DataReader.BulkReadRecordAsync<DataContracts.Insurance>("INSURANCE", new string[] { insuranceId });
                calcMethods = await DataReader.BulkReadRecordAsync<DataContracts.CalcMethods>("CALC.METHODS", string.Empty);
                //GL.ACCTS
                var glId = !string.IsNullOrEmpty(fixedAssetData.FixCalcAcct) ? fixedAssetData.FixCalcAcct : null;
                glAccounts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", new string[] { glId });
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                throw;

            }
            Domain.ColleagueFinance.Entities.FixedAssets fixedAssets = null;
            try
            {
                fixedAssets = BuildFixedAsset(fixedAssetData, insuranceDC, calcMethods, glAccounts);
            }

            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {

                    Id = guid
                }); ;
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return fixedAssets;
        }

        /// <summary>
        /// Gets fixed assets paged.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Domain.ColleagueFinance.Entities.FixedAssets>, int>> GetFixedAssetsAsync(int offset, int limit, bool bypassCache)
        {
            var fixedAssetsEntities = new List<Domain.ColleagueFinance.Entities.FixedAssets>();
            int totalCount = 0;
            string[] subList = null;
            Collection<DataContracts.FixedAssets> fixedAssetData = null;
            Collection<DataContracts.Insurance> insuranceDC = null;
            Collection<DataContracts.CalcMethods> calcMethods = null;
            Collection<DataContracts.GlAccts> glAccounts = null;
            try
            {
                var fixedAssetsPeriodsCacheKey = CacheSupport.BuildCacheKey(AllFixedAssetsCache);

                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    fixedAssetsPeriodsCacheKey,
                    "FIXED.ASSETS",
                    offset,
                    limit,
                    AllFixedAssetsCacheTimeout,
                    async () =>
                    {
                        return new CacheSupport.KeyCacheRequirements() { };
                    });

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {
                    return new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.FixedAssets>, int>(new List<Domain.ColleagueFinance.Entities.FixedAssets>(), 0);
                }
                subList = keyCache.Sublist.ToArray();

                totalCount = keyCache.TotalCount.Value;

                fixedAssetData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.FixedAssets>("FIXED.ASSETS", subList);

                var insuranceIds = fixedAssetData.Where(i => !string.IsNullOrEmpty(i.FixInsuranceId)).Select(id => id.FixInsuranceId);
                insuranceDC = await DataReader.BulkReadRecordAsync<DataContracts.Insurance>("INSURANCE", insuranceIds.Distinct().ToArray());
                calcMethods = await DataReader.BulkReadRecordAsync<DataContracts.CalcMethods>("CALC.METHODS", string.Empty);
                //GL.ACCTS
                var glIds = fixedAssetData.Where(a => !string.IsNullOrEmpty(a.FixCalcAcct)).Select(i => i.FixCalcAcct);
                //null check glIds
                glAccounts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", glIds.ToArray());

            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                throw;

            }

            foreach (var fixedAssetDC in fixedAssetData)
            {
                try
                {
                    fixedAssetsEntities.Add(BuildFixedAsset(fixedAssetDC, insuranceDC, calcMethods, glAccounts));
                }

                catch (Exception ex)
                {
                    if (exception == null)
                        exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                    {
                        SourceId = fixedAssetDC.Recordkey,
                        Id = fixedAssetDC.RecordGuid
                    });
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return fixedAssetsEntities != null && fixedAssetsEntities.Any() ? new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.FixedAssets>, int>(fixedAssetsEntities, totalCount) :
                new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.FixedAssets>, int>(new List<Domain.ColleagueFinance.Entities.FixedAssets>(), 0);

        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Builds the FixedAssets entity.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="insSource"></param>
        /// <param name="calcMethods"></param>
        /// <param name="glAccounts"></param>
        /// <returns></returns>
        private Domain.ColleagueFinance.Entities.FixedAssets BuildFixedAsset(DataContracts.FixedAssets source, IEnumerable<DataContracts.Insurance> insSource,
                                                            IEnumerable<DataContracts.CalcMethods> calcMethods, IEnumerable<DataContracts.GlAccts> glAccounts)
        {
            if (source == null)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "DataContract FixedAssets source is required."));
                return null;
            }

            if (string.IsNullOrEmpty(source.RecordGuid))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Guid is required.")
                {
                    SourceId = source.Recordkey,
                    Id = source.RecordGuid
                });
            }
            if (string.IsNullOrEmpty(source.Recordkey))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "RecordKey is required.")
                {
                    SourceId = source.Recordkey,
                    Id = source.RecordGuid
                });
            }
            if (string.IsNullOrEmpty(source.FixCapitalize))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Capitalization status is required.")
                {
                    SourceId = source.Recordkey,
                    Id = source.RecordGuid
                });
            }
            if (string.IsNullOrEmpty(source.FixAcquisMethod))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Acquisition method is required.")
                {
                    SourceId = source.Recordkey,
                    Id = source.RecordGuid
                });
            }
            if (string.IsNullOrEmpty(source.FixPropertyTag))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "Fix property tag is required.")
                {
                    SourceId = source.Recordkey,
                    Id = source.RecordGuid
                });
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                return null;
            }

            Domain.ColleagueFinance.Entities.FixedAssets fixedAsset = null;
            try
            {

                fixedAsset = new Domain.ColleagueFinance.Entities.FixedAssets(source.RecordGuid, source.Recordkey, source.FixDesc, source.FixCapitalize, source.FixAcquisMethod, source.FixPropertyTag)
                {
                    FixAssetType = source.FixAssetType,
                    FixAssetCategory = source.FixAssetCategory,
                    //GET/GET ALL - If the field FIX.DISPOSAL.DATE is populated with a date that is equal to or greater than 'today" publish "disposed".
                    FixDisposalDate = source.FixDisposalDate,
                    //GET/GET ALL - Publish the description associated to the Valcode (ITEM.CONDITION) stored within FIX.INV.COND
                    FixInvoiceCondition = source.FixInvCond,
                    //GET/GET ALL - Publish the stored string. Within Colleague this property is not a lookup to the LOCATION (LOCN) file.
                    FixLocation = source.FixLocation,
                    FixBuilding = source.FixBldg,
                    FixRoom = source.FixRoom,
                    //GET/GET ALL - Publish the value if stored.
                    InsuranceAmountCoverage = BuildInsuranceCoverage(source.FixInsuranceId, insSource),
                    FixValueAmount = BuildFixValueAmount(source.ValuationEntityAssociation),
                    FixAcqisitionCost = source.FixAcquisCost,
                    FixAllowAmount = BuildAllowAmount(source.AllowEntityAssociation),
                    FixCalculationMethod = BuildCalculationMethod(source.FixCalcMethod, calcMethods),
                    FixSalvageValue = source.FixSalvageValue,
                    FixUsefulLife = source.FixUsefulLife,
                    FixCalcAccount = BuildAcctComponentValue(source.FixCalcAcct, glAccounts),
                    FixRenewalAmount = BuildRenewalAmount(source.FixRenewalsEntityAssociation),
                    FixStewerdId = source.FixStewardId
                };
            }
            catch (Exception ex)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    SourceId = source.Recordkey,
                    Id = source.RecordGuid
                });
            }
            return fixedAsset;
        }

        /// <summary>
        /// Gets insurence amount.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="insSource"></param>
        /// <returns></returns>
        private decimal? BuildInsuranceCoverage(string source, IEnumerable<Insurance> insSource)
        {
            decimal? amt = null;

            if (string.IsNullOrEmpty(source)) return amt;

            if (insSource != null && insSource.Any())
            {
                var ins = insSource.FirstOrDefault(i => i.Recordkey.Equals(source, StringComparison.OrdinalIgnoreCase));
                if (ins == null)
                {
                    throw new RepositoryException(string.Format("Insurance record not found for key: {0}", source));
                }
                return ins.InsAmtCoverage;
            }
            return amt;
        }

        /// <summary>
        /// Gets fix value.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private decimal? BuildFixValueAmount(List<FixedAssetsValuation> source)
        {
            if (source != null && source.Any())
            {
                //use the value of FIX.VALUE.AMT that is associated with the highest FIX.VALUE.DATE
                var item = source.OrderByDescending(d => d.FixValueDateAssocMember).FirstOrDefault();
                if (item != null && item.FixValueAmtAssocMember.HasValue)
                {
                    return item.FixValueAmtAssocMember;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets sum of all allow amount.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private decimal? BuildAllowAmount(List<FixedAssetsAllow> source)
        {
            if (source != null && source.Any())
            {
                var item = source.Where(r => r.FixAllowAmtAssocMember.HasValue).Sum(i => i.FixAllowAmtAssocMember);
                if (item != null && item.HasValue)
                {
                    return item.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets calculate method.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="calcMethods"></param>
        /// <returns></returns>
        private string BuildCalculationMethod(string source, IEnumerable<CalcMethods> calcMethods)
        {
            if (string.IsNullOrEmpty(source)) return null;

            if (calcMethods != null && calcMethods.Any())
            {
                var method = calcMethods.FirstOrDefault(m => m.Recordkey.Equals(source, StringComparison.OrdinalIgnoreCase));
                if (method == null)
                {
                    throw new RepositoryException(string.Format("Calculation method not found for id: {0}", source));
                }
                return method.CalcDesc;
            }
            return null;
        }

        /// <summary>
        /// Gets gl account guid.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="glAccounts"></param>
        /// <returns></returns>
        private string BuildAcctComponentValue(string source, IEnumerable<DataContracts.GlAccts> glAccounts)
        {
            if (glAccounts != null && glAccounts.Any())
            {
                if (!string.IsNullOrEmpty(source))
                {
                    var glAcct = glAccounts.FirstOrDefault(a => a.Recordkey.Equals(source, StringComparison.OrdinalIgnoreCase));
                    if (glAcct == null)
                    {
                        throw new RepositoryException(string.Format("Gl account not found for id: {0}", source));
                    }
                    return glAcct.RecordGuid;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets sum of all renewal amounts.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private decimal? BuildRenewalAmount(List<FixedAssetsFixRenewals> source)
        {
            if (source != null && source.Any())
            {
                var result = source.Where(r => r.FixRenewalAmtAssocMember.HasValue).Sum(i => i.FixRenewalAmtAssocMember);
                if (result != null && result.HasValue)
                {
                    return result.Value;
                }
            }
            return null;
        }
        #endregion
    }
}