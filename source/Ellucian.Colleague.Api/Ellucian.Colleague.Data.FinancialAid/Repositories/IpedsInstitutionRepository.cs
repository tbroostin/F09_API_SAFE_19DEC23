// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using slf4net;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using System.IO;
using Newtonsoft.Json;
using Ellucian.Web.Http.Configuration;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// IPEDS Institution Repository gets data from Colleague database
    /// As of 8/12/2014, the IPEDS API documentation lives here
    /// https://inventory.data.gov/dataset/post-secondary-universe-survey-2010-directory-information/resource/38625c3d-5388-4c16-a30f-d105432553a4
    /// as of 2017, we no longer use the above mentioned url
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class IpedsInstitutionRepository : BaseColleagueRepository, IIpedsInstitutionRepository
    {
        private readonly int bulkRecordReadSize;

        /// <summary>
        /// Create the IpedsInstitutionRepository object
        /// </summary>
        /// <param name="cacheProvider">CacheProvider</param>
        /// <param name="transactionFactory">TransactionFactory</param>
        /// <param name="logger">Logger</param>
        public IpedsInstitutionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkRecordReadSize = (apiSettings != null && apiSettings.BulkReadSize > 0) ? apiSettings.BulkReadSize : 5000;
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get IpedsInstitution objects based on a list of OPE (Office of Post Secondary Education) ids
        /// </summary>
        /// <param name="opeIdList">List of OPE ids of IpedsInstitution objects to be returned</param>
        /// <returns>A list of IpedsInstitution objects limited to the ones with an OPE Id listed in the opeIdList argument. If an
        /// OPE id from the input list is invalid, no IpedsInstitution object is returned for that OPE Id. </returns>
        public async Task<IEnumerable<IpedsInstitution>> GetIpedsInstitutionsAsync(IEnumerable<string> opeIdList)
        {
            if (opeIdList == null || !opeIdList.Any())
            {
                throw new ArgumentNullException("opeIdList");
            }

            var distinctOpeIdList = opeIdList.Distinct().Select(o => o.TrimStart('0')).ToArray();

            var ipedsInstitutionEntities = new List<IpedsInstitution>();

            var allIpedsInstitutions = await GetOrAddToCacheAsync<List<IpedsInstitution>>("AllIpedsInstitutions",
                async () =>
                {
                    var ipedsInstitutionList = new List<IpedsInstitution>();

                    //page record reads. get the ids
                    var ipedsRecordIds = await DataReader.SelectAsync("IPEDS.INSTITUTIONS", string.Empty);
                    var ipedsRecordData = new List<IpedsInstitutions>();

                    if (ipedsRecordIds != null && ipedsRecordIds.Any())
                    {
                        for (int i = 0; i < ipedsRecordIds.Count(); i += bulkRecordReadSize)
                        {
                            var subList = ipedsRecordIds.Skip(i).Take(bulkRecordReadSize).ToArray();
                            var bulkData = await DataReader.BulkReadRecordAsync<IpedsInstitutions>(subList);
                            if (bulkData != null)
                            {
                                ipedsRecordData.AddRange(bulkData);
                            }
                        }
                    }

                    if (ipedsRecordData != null && ipedsRecordData.Any())
                    {
                        foreach (var ipedsRecord in ipedsRecordData)
                        {
                            var lastModified = ipedsRecord.IiIpedsLastModified.HasValue ? ipedsRecord.IiIpedsLastModified.Value : DateTime.Now;
                            ipedsInstitutionList.Add(
                                new IpedsInstitution(ipedsRecord.Recordkey, ipedsRecord.IiUnitId, ipedsRecord.IiInstitutionName, ipedsRecord.IiOpeId.TrimStart('0'), lastModified)
                            );
                        }
                    }
                    return ipedsInstitutionList;
                });

            return allIpedsInstitutions.Where(i => distinctOpeIdList.Contains(i.OpeId.TrimStart('0'))).ToList();
        }
    }
}
