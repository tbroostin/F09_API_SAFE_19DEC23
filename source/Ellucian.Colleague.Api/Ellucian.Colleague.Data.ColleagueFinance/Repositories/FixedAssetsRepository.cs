// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
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
            if(string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required.");
            }

            var key = await GetRecordKeyFromGuidAsync(guid);
            if(string.IsNullOrEmpty(key))
            {
                throw new KeyNotFoundException(string.Format("Fixed asset not found for guid: {0}", guid));
            }

            var fixedAssetData = await DataReader.ReadRecordAsync<DataContracts.FixedAssets>(key);
            if(fixedAssetData == null)
            {
                throw new KeyNotFoundException(string.Format("Fixed asset not found for key: {0}", key));
            }

            var insuranceId = !string.IsNullOrEmpty(fixedAssetData.FixInsuranceId) ? fixedAssetData.FixInsuranceId : null;
            var insuranceDC = await DataReader.BulkReadRecordAsync<DataContracts.Insurance>("INSURANCE", new string[] { insuranceId });
            var calcMethods = await DataReader.BulkReadRecordAsync<DataContracts.CalcMethods>("CALC.METHODS", string.Empty);
            //GL.ACCTS
            var glId = !string.IsNullOrEmpty(fixedAssetData.FixCalcAcct) ? fixedAssetData.FixCalcAcct : null;
            var glAccounts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", new string[] { glId });

            return BuildFixedAsset(fixedAssetData, insuranceDC, calcMethods, glAccounts);
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
            List<Domain.ColleagueFinance.Entities.FixedAssets> fixedAssetsEntities = new List<Domain.ColleagueFinance.Entities.FixedAssets>();
            int totalCount = 0;
            try
            {
                var fixedAssetsIds = await DataReader.SelectAsync("FIXED.ASSETS", string.Empty);
                totalCount = fixedAssetsIds.Count();
                Array.Sort(fixedAssetsIds);
                var subList = fixedAssetsIds.Skip(offset).Take(limit).ToArray();

                var fixedAssetData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.FixedAssets>("FIXED.ASSETS", subList);

                if(fixedAssetData != null && fixedAssetData.Any())
                {
                    var insuranceIds = fixedAssetData.Where(i => !string.IsNullOrEmpty(i.FixInsuranceId)).Select(id => id.FixInsuranceId);
                    var insuranceDC = await DataReader.BulkReadRecordAsync<DataContracts.Insurance>("INSURANCE", insuranceIds.Distinct().ToArray());
                    var calcMethods = await DataReader.BulkReadRecordAsync<DataContracts.CalcMethods>("CALC.METHODS", string.Empty);
                    //GL.ACCTS
                    var glIds = fixedAssetData.Where(a => !string.IsNullOrEmpty(a.FixCalcAcct)).Select(i => i.FixCalcAcct);
                    //null check glIds
                    var glAccounts = await DataReader.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", glIds.ToArray());


                    foreach (var fixedAssetDC in fixedAssetData)
                    {
                        fixedAssetsEntities.Add(BuildFixedAsset(fixedAssetDC, insuranceDC, calcMethods, glAccounts));
                    }
                }

                return fixedAssetsEntities != null && fixedAssetsEntities.Any() ? new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.FixedAssets>, int>(fixedAssetsEntities, totalCount) :
                    new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.FixedAssets>, int>(new List<Domain.ColleagueFinance.Entities.FixedAssets>(), 0);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw;                
            }
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
            Domain.ColleagueFinance.Entities.FixedAssets fixedAssett = new Domain.ColleagueFinance.Entities.FixedAssets(source.RecordGuid, source.Recordkey, source.FixDesc, source.FixCapitalize, source.FixAcquisMethod, source.FixPropertyTag)
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
            return fixedAssett;
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
                    throw new KeyNotFoundException(string.Format("Insurance record not found for key: {0}", source));
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
                if(method == null)
                {
                    throw new KeyNotFoundException(string.Format("Calculation method not found for id: {0}", source));
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
                        throw new KeyNotFoundException(string.Format("Gl account not found for id: {0}", source));
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
                if(result != null && result.HasValue)
                {
                    return result.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets host country.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetHostCountryAsync()
        {
            var intlParams = await GetInternationalParametersAsync();
            return intlParams.HostCountry;
        }

        #endregion
    }
}