// Copyright 2018 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Implement the IStudentFinancialAidAcademicProgressStatusesRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentFinancialAidAcademicProgressStatusesRepository : BaseColleagueRepository, IStudentFinancialAidAcademicProgressStatusesRepository
    {
        public StudentFinancialAidAcademicProgressStatusesRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Gets sap result by id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<SapResult> GetSapResultByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || string.IsNullOrEmpty(recordInfo.Entity) || !recordInfo.Entity.Equals("SAP.RESULTS", StringComparison.OrdinalIgnoreCase))
            {
                throw new KeyNotFoundException(string.Format("No student financial aid academic progress status was found for guid {0}'. ", id));
            }
            var sapResultsDataContract = await DataReader.ReadRecordAsync<SapResults>("SAP.RESULTS", recordInfo.PrimaryKey);
            if (sapResultsDataContract == null)
            {
                throw new KeyNotFoundException(string.Format("No student financial aid academic progress status was found for guid {0}'. ", id));
            }

            SapResult sapResults = BuildSapResult(sapResultsDataContract);
            return sapResults;
        }

        /// <summary>
        /// Gets sap results.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="personId"></param>
        /// <param name="statusId"></param>
        /// <param name="typeId"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<SapResult>, int>> GetSapResultsAsync(int offset, int limit, string personId = "", string statusId = "", string typeId = "", bool bypassCache = false)
        {
            var sapResults = new List<SapResult>();
            StringBuilder criteria = new StringBuilder();
            int totalCount = 0;
            string[] limitingKeys = null;

            if (!string.IsNullOrEmpty(personId))
            {
                //Here try get student id from FIN.AID to perf reasons
                var record = await DataReader.ReadRecordAsync<FinAid>("FIN.AID", personId);
                if (record == null || !record.FaSapResultsId.Any())
                {
                    return new Tuple<IEnumerable<SapResult>, int>(new List<SapResult>(), 0);
                }
                limitingKeys = record.FaSapResultsId.ToArray();
            }

            if (!string.IsNullOrEmpty(statusId))
            {
                if(criteria.Length > 0)
                {
                    criteria.Append(string.Format(" AND WITH SAPR.OVR.SAP.STATUS EQ '{0}' OR WITH SAPR.CALC.SAP.STATUS EQ '{0}' AND SAPR.OVR.SAP.STATUS EQ ''", statusId));
                }
                else
                {
                    criteria.Append(string.Format("WITH SAPR.OVR.SAP.STATUS EQ '{0}' OR WITH SAPR.CALC.SAP.STATUS EQ '{0}' AND SAPR.OVR.SAP.STATUS EQ ''", statusId));
                }
            }

            if(!string.IsNullOrEmpty(typeId))
            {
                if(criteria.Length > 0)
                {
                    criteria.Append(string.Format(" AND WITH SAPR.SAP.TYPE.ID EQ '{0}'", typeId));
                }
                else
                {
                    criteria.Append(string.Format("WITH SAPR.SAP.TYPE.ID EQ '{0}'", typeId));
                }
            }

            string[] sapResultsIds = await DataReader.SelectAsync("SAP.RESULTS", limitingKeys, criteria.ToString());
            if(sapResultsIds == null || !sapResultsIds.Any())
            {
                return new Tuple<IEnumerable<SapResult>, int>(new List<SapResult>(), 0);
            }

            totalCount = sapResultsIds.Count();
            Array.Sort(sapResultsIds);
            string[] filteredIds = sapResultsIds.Skip(offset).Take(limit).ToArray();

            var sapResultsDCs = await DataReader.BulkReadRecordAsync<SapResults>("SAP.RESULTS", filteredIds);

            foreach (var sapResultsDC in sapResultsDCs)
            {
                sapResults.Add(BuildSapResult(sapResultsDC));
            }

            return sapResults.Any() ? new Tuple<IEnumerable<SapResult>, int>(sapResults, totalCount) :
                new Tuple<IEnumerable<SapResult>, int>(new List<SapResult>(), 0);
        }

        /// <summary>
        /// Builds sap result.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="terms"></param>
        /// <returns></returns>
        private SapResult BuildSapResult(SapResults source)
        {
            string guid = string.Empty, id = string.Empty, personId = string.Empty, status = string.Empty;
            DateTime? ovrSapDate = null;

            personId = source.SaprStudentId;

            //If SAPR.OVR.SAP.STATUS is populated, use this status, else obtain status from SAPR.CALC.SAP.STATUS.  Expose the guid of the retrieved status.
            if (!string.IsNullOrEmpty(source.SaprOvrSapStatus))
            {
                status = source.SaprOvrSapStatus;
            }
            else
            {
                status = source.SaprCalcSapStatus;
            }

            //if SAPR.OVR.SAP.DATE is populated, use this date,  else SAP.RESULTS.ADDDATE
            if (source.SaprOvrSapDate.HasValue)
            {
                ovrSapDate = source.SaprOvrSapDate.HasValue ? source.SaprOvrSapDate.Value : default(DateTime?);
            }
            else
            {
                ovrSapDate = source.SapResultsAdddate.HasValue ? source.SapResultsAdddate.Value : default(DateTime?);
            }

            var defaultNullDateTime = default(DateTime?);
            SapResult sapResult = new SapResult(source.RecordGuid, source.Recordkey, status);
            sapResult.PersonId = !string.IsNullOrEmpty(personId)? personId : string.Empty;
            sapResult.OvrResultsAddDate = ovrSapDate.HasValue? ovrSapDate.Value : defaultNullDateTime;
            sapResult.SapTypeId = !string.IsNullOrEmpty(source.SaprSapTypeId)? source.SaprSapTypeId : string.Empty;
            sapResult.SaprEvalPdEndTerm = source.SaprEvalPdEndTerm;
            sapResult.SaprEvalPdEndDate = source.SaprEvalPdEndDate.HasValue ? source.SaprEvalPdEndDate.Value : defaultNullDateTime;
            sapResult.SaprCalcThruTerm = source.SaprCalcThruTerm;
            sapResult.SaprCalcThruDate = source.SaprCalcThruDate.HasValue ? source.SaprCalcThruDate.Value : defaultNullDateTime;

            return sapResult;
        }
    }
}