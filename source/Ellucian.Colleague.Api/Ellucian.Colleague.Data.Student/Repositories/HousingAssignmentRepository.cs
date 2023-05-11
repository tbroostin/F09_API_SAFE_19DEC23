﻿// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class HousingAssignmentRepository : BaseColleagueRepository, IHousingAssignmentRepository
    {
        /// <summary>
        /// ...ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public HousingAssignmentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get all paged housing assignments
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<HousingAssignment>, int>> GetHousingAssignmentsAsync(int offset, int limit, string person = "", string term = "", string status = "", string startDate = "", string endDate = "", bool bypassCache= false)
        {
            List<HousingAssignment> housingAssignmentEntities = new List<HousingAssignment>();
            var criteria = new StringBuilder();
            criteria.AppendFormat("WITH RMAS.ROOM NE '' AND RMAS.BLDG NE ''");
            String select = string.Empty;
            var repositoryException = new RepositoryException();
            if (!string.IsNullOrEmpty(person))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH RMAS.PERSON.ID EQ '{0}'", person);
            }
            if (!string.IsNullOrEmpty(term))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH RMAS.TERM EQ '{0}'", term);
            }
            if (!string.IsNullOrEmpty(status))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH RMAS.CURRENT.STATUS EQ '{0}'", status);
            }
            if (!string.IsNullOrEmpty(startDate))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH RMAS.START.DATE GE '{0}'", startDate);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                if (criteria.Length > 0)
                    criteria.Append(" AND ");
                criteria.AppendFormat("WITH RMAS.END.DATE NE '' AND RMAS.END.DATE LE '{0}'", endDate);
            }

            if (criteria.Length > 0)
                select = criteria.ToString();
            var totalCount = 0;

            var housingAssignmentIds = await DataReader.SelectAsync("ROOM.ASSIGNMENT", select);

            totalCount = housingAssignmentIds.Count();

            if (totalCount == 0 || totalCount <= offset)
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.HousingAssignment>, int>(housingAssignmentEntities, 0);
            }

            var sublist = housingAssignmentIds.OrderBy(id => id).Skip(offset).Take(limit).ToArray();
                        
            var results = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.RoomAssignment>("ROOM.ASSIGNMENT", sublist);
            if (results.Equals(default(BulkReadOutput<DataContracts.RoomAssignment>)))
                return new Tuple<IEnumerable<Domain.Student.Entities.HousingAssignment>, int>(new List<Domain.Student.Entities.HousingAssignment>(), 0);
            if ((results.InvalidKeys != null && results.InvalidKeys.Any()) || (results.InvalidRecords!=null && results.InvalidRecords.Any()))
            {
                if (results.InvalidKeys.Any())
                {
                    repositoryException.AddErrors(results.InvalidKeys
                        .Select(key => new RepositoryError("invalid.key",
                        string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                }
                if (results.InvalidRecords.Any())
                {
                    repositoryException.AddErrors(results.InvalidRecords
                       .Select(r => new RepositoryError("invalid.record",
                       string.Format("Error: '{0}'. Entity: 'ROOM.ASSIGNMENT', Record ID: '{1}' ", r.Value, r.Key))
                       { }));
                }
                throw repositoryException;
            }
            var housingAssignmentDataContracts = results.BulkRecordsRead.ToList();
            if (housingAssignmentDataContracts != null && housingAssignmentDataContracts.Any())
            {
                //Get records from AR.ADDNL.AMTS
                string[] rmAssignmentIds = housingAssignmentDataContracts.Select(i => i.Recordkey).Distinct().ToArray();
                var arAddnlAmtsIds = await DataReader.SelectAsync("AR.ADDNL.AMTS", "WITH ARAA.ROOM.ASSIGNMENT EQ '?'", rmAssignmentIds);

                var housingRequestDict = await GetHousingGuidDictionary(rmAssignmentIds, "ROOM.ASSIGNMENT");

                var arAddnlAmtsDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.ArAddnlAmts>("AR.ADDNL.AMTS", arAddnlAmtsIds);
                
                foreach (var housingAssignmentDataContract in housingAssignmentDataContracts)
                {
                    //to catch entity  exceptions
                    try
                    {
                        Domain.Student.Entities.HousingAssignment housingAssignmentEntity = BuildHousingAssignmentEntity(housingAssignmentDataContract, arAddnlAmtsDataContracts, housingRequestDict);
                        housingAssignmentEntities.Add(housingAssignmentEntity);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                        var message = string.Concat(ex.Message, ". Entity: 'ROOM.ASSIGNMENT', Record ID: '", housingAssignmentDataContract.Recordkey, "'");

                        repositoryException.AddError(new RepositoryError("invalid housing assignment", message)
                        {
                            //SourceId = stncData.Recordkey,
                            //Id = stncData.RecordGuid
                        });
                    }
                }
            }
            if (repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return housingAssignmentEntities.Any()? new Tuple<IEnumerable<HousingAssignment>, int>(housingAssignmentEntities, totalCount) : 
                new Tuple<IEnumerable<Domain.Student.Entities.HousingAssignment>, int>(null, 0);
        }

        /// <summary>
        /// Using a collection of ROOM.ASSIGNMENT ids, get a dictionary collection of associated secondary guids on RMAS.INTG.KEY.IDX
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetHousingGuidDictionary(IEnumerable<string> ids, string filename)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();

            try
            {
                var guidLookup = ids
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .Distinct().ToList()
                   .ConvertAll(p => new RecordKeyLookup(filename, p, "RMAS.INTG.KEY.IDX", p, false)).ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Format("Error occured while getting guids for {0}.", filename), ex); ;
            }

            return guidCollection;
        }
        

        /// <summary>
        /// Gets housing assignment by id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<HousingAssignment> GetHousingAssignmentByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required.");
            }
            var housingAssignmentId = await this.GetHousingAssignmentKeyAsync(guid);//await GetRecordKeyFromGuidAsync(guid);
            if (string.IsNullOrEmpty(housingAssignmentId))
            {
                throw new KeyNotFoundException(string.Format("No housing assignment was found for guid {0}.", guid));
            }
            var housingAssignmentDC = await DataReader.ReadRecordAsync<RoomAssignment>("ROOM.ASSIGNMENT", housingAssignmentId);
            if (housingAssignmentDC == null || string.IsNullOrEmpty(housingAssignmentDC.RmasRoom))
            {
                throw new KeyNotFoundException(string.Format("Room id is required for housing assignment {0}.", guid));
            }

            //Get records from AR.ADDNL.AMTS
            string[] rmAssignmentIds = new[] { housingAssignmentDC.Recordkey };
            var arAddnlAmtsIds = await DataReader.SelectAsync("AR.ADDNL.AMTS", "WITH ARAA.ROOM.ASSIGNMENT EQ '?'", rmAssignmentIds);

            var arAddnlAmtsDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.ArAddnlAmts>("AR.ADDNL.AMTS", arAddnlAmtsIds);

            var hrDict = await GetHousingGuidDictionary(new List<string>() { housingAssignmentId }, "ROOM.ASSIGNMENT");

            if (hrDict.Values.Contains(guid))
            {
                throw new KeyNotFoundException(string.Format("No housing assignment was found for guid {0}.  Found as a housing request.    ", guid));
            }

            Domain.Student.Entities.HousingAssignment housingAssignmentEntity = BuildHousingAssignmentEntity(housingAssignmentDC, arAddnlAmtsDataContracts, hrDict);
            return housingAssignmentEntity;
        }

        /// <summary>
        /// Returns key for the housing assignment.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<string> GetHousingAssignmentKeyAsync(string guid)
        {
            return await GetRecordKeyFromGuidAsync(guid);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all the guids for the person keys
        /// </summary>
        /// <param name="personRecordKeys"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> personRecordKeys)
        {
            if (personRecordKeys == null || !personRecordKeys.Any())
            {
                return null;
            }

            var personGuids = new Dictionary<string, string>();

            if (personRecordKeys != null && personRecordKeys.Any())
            {
                // convert the person keys to person guids
                var personGuidLookup = personRecordKeys.ToList().ConvertAll(p => new RecordKeyLookup("PERSON", p, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    string[] splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!personGuids.ContainsKey(splitKeys[1]))
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            personGuids.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                        }
                    }
                }
            }
            return (personGuids != null && personGuids.Any()) ? personGuids : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="housingAssignmentEntity"></param>
        /// <returns></returns>
        public async Task<HousingAssignment> UpdateHousingAssignmentAsync(HousingAssignment source)
        {
            try
            {
                if (source == null)
                {
                    throw new ArgumentNullException("housingAssignment", "Housing assignment must be provided.");
                }

                CreateUpdateHousingAssignRequest request = new CreateUpdateHousingAssignRequest() 
                {
                    AGuid = source.Guid,
                    AId = source.RecordKey,
                    AHousingRequestGuid = source.HousingRequest,
                    APersonId = source.StudentId,
                    ARoomId = source.RoomId,
                    ATerm = source.Term,
                    AStartDate = source.StartOn.HasValue ? source.StartOn.Value.Date : default(DateTime?),
                    AEndDate = source.EndDate.HasValue? source.EndDate.Value.Date : default(DateTime?),
                    ACurrentStatus = source.Status,
                    ACurrentStatusDate = source.StatusDate.Value.Date,
                    AContractNo = string.IsNullOrEmpty(source.ContractNumber) ? string.Empty : source.ContractNumber,
                    AComments = string.IsNullOrEmpty(source.Comments) ? string.Empty : source.Comments,
                    ARoomRateTable = source.RoomRateTable,
                    ARoomRatePeriod = source.RatePeriod,
                    AOverrideRate = source.RateOverride.HasValue ? source.RateOverride.Value : default(decimal?),
                    AOverrideRateReason = string.IsNullOrEmpty(source.RateOverrideReason) ? string.Empty : source.RateOverrideReason,
                    CreateUpdareHousingAssignmentAddnlCharges = BuildAdditionalCharges(source.ArAdditionalAmounts),
                    AResidentStaffIndicator = source.ResidentStaffIndicator                    
                };

                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    request.ExtendedNames = extendedDataTuple.Item1;
                    request.ExtendedValues = extendedDataTuple.Item2;
                }

                var updateResponse = await transactionInvoker.ExecuteAsync<CreateUpdateHousingAssignRequest, CreateUpdateHousingAssignResponse>(request);
                
                if (updateResponse.CreateUpdareHousingAssignmentErrors != null && updateResponse.CreateUpdareHousingAssignmentErrors.Any())
                {
                    var errorMessage = new StringBuilder();
                    errorMessage.Append(string.Format("Error(s) occurred updating housing assignment for guid: '{0}': ", request.AGuid));
                    updateResponse.CreateUpdareHousingAssignmentErrors.ForEach(err =>
                    {
                        errorMessage.Append(string.Format(" {0}", err.AlErrorMsg));
                        logger.Error(errorMessage.ToString());
                    });

                    throw new InvalidOperationException(errorMessage.ToString());
                }

                return await this.GetHousingAssignmentByGuidAsync(updateResponse.AGuid);
            }
            catch (Exception)
            {                
                throw;
            }
        }

        /// <summary>
        /// Build additional charged
        /// 
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private List<CreateUpdareHousingAssignmentAddnlCharges> BuildAdditionalCharges(IEnumerable<ArAdditionalAmount> sources)
        {
            List<CreateUpdareHousingAssignmentAddnlCharges> addlCharges = new List<CreateUpdareHousingAssignmentAddnlCharges>();

            if (sources != null && sources.Any())
            {
                foreach (var source in sources)
                {
                    CreateUpdareHousingAssignmentAddnlCharges addlCharge = new CreateUpdareHousingAssignmentAddnlCharges()
                    {
                        AlAddnlChargeAmounts = BuildChargeAmount(source.AraaChargeAmt, source.AraaCrAmt),
                        AlAddnlChargeCodes = source.AraaArCode
                    };
                    addlCharges.Add(addlCharge);
                }
            }
            return addlCharges.Any() ? addlCharges : null;
        }

        /// <summary>
        /// Build charge amount.
        /// </summary>
        /// <param name="charge"></param>
        /// <param name="credit"></param>
        /// <returns></returns>
        private decimal? BuildChargeAmount(decimal? charge, decimal? credit)
        {
            if (charge.HasValue)
            {
                return charge.Value;
            }

            if (credit.HasValue)
            {
                return credit.Value;
            }

            return null;
        }

        /// <summary>
        /// Builds housing assignment
        /// </summary>
        /// <param name="source"></param>
        /// <param name="arAddnlAmtsDataContracts"></param>
        /// <returns></returns>
        private Domain.Student.Entities.HousingAssignment BuildHousingAssignmentEntity(DataContracts.RoomAssignment source, IEnumerable<ArAddnlAmts> arAddnlAmtsDataContracts, IDictionary<string, string> housingRequestDict)
        {
            Domain.Student.Entities.HousingAssignment housingAssignmentEntity = new HousingAssignment(source.RecordGuid, source.Recordkey,
                source.RmasPersonId, source.RmasRoom, source.RmasStartDate, source.RmasEndDate);

            housingAssignmentEntity.Building = source.RmasBldg;
            housingAssignmentEntity.Term = source.RmasTerm;
            housingAssignmentEntity.Statuses = BuildStatuses(source.RmasStatusesEntityAssociation);
            housingAssignmentEntity.ContractNumber = source.RmasContract;
            housingAssignmentEntity.Comments = source.RmasComments;
            housingAssignmentEntity.RoomRateTable = source.RmasRoomRateTable;
            housingAssignmentEntity.RatePeriod = source.RmasRatePeriod;
            housingAssignmentEntity.RateOverride = source.RmasOverrideRate;
            housingAssignmentEntity.RateOverrideReason = source.RmasRateOverrideReason;
            housingAssignmentEntity.ResidentStaffIndicator = source.RmasResidentStaffIndic;
            housingAssignmentEntity.ArAdditionalAmounts = BuildArAdditionalAmounts(source.Recordkey, arAddnlAmtsDataContracts);
            housingAssignmentEntity.HousingRequest = GetGuid(source.Recordkey, housingRequestDict);

            return housingAssignmentEntity;
        }

        private string GetGuid(string key, IDictionary<string, string> dict)
        {
            string guid = null;
            if (!string.IsNullOrEmpty(key))
            {
                if (dict.TryGetValue(key, out guid))
                {
                    return guid;
                }
            }
            return guid;
        }

        /// <summary>
        /// Builds additional amounts
        /// </summary>
        /// <param name="housingAssignmentId"></param>
        /// <param name="arAddnlAmtsDataContracts"></param>
        /// <returns></returns>
        private IEnumerable<ArAdditionalAmount> BuildArAdditionalAmounts(string housingAssignmentId, IEnumerable<ArAddnlAmts> arAddnlAmtsDataContracts)
        {
            List<ArAdditionalAmount> arAddlAmountList = new List<ArAdditionalAmount>();
            if (arAddnlAmtsDataContracts == null || !arAddnlAmtsDataContracts.Any())
            {
                return null;
            }
            var arAddlAmountDCs = arAddnlAmtsDataContracts.Where(i => i.AraaRoomAssignment.Equals(housingAssignmentId, StringComparison.OrdinalIgnoreCase));
            if (arAddlAmountDCs != null && arAddlAmountDCs.Any())
            {
                foreach (var arAddlAmountsDC in arAddlAmountDCs)
                {
                    ArAdditionalAmount arAddlAmountEntity = new ArAdditionalAmount()
                    {
                        AraaArCode = arAddlAmountsDC.AraaArCode,
                        AraaChargeAmt = arAddlAmountsDC.AraaChargeAmt,
                        AraaCrAmt = arAddlAmountsDC.AraaCrAmt,
                        AraaRoomAssignmentId = arAddlAmountsDC.AraaRoomAssignment,
                        Recordkey = arAddlAmountsDC.Recordkey
                    };
                    arAddlAmountList.Add(arAddlAmountEntity);
                }
            }
            return arAddlAmountList.Any() ? arAddlAmountList : null;
        }

        /// <summary>
        /// Builds statuses
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private IEnumerable<HousingAssignmentStatus> BuildStatuses(List<RoomAssignmentRmasStatuses> source)
        {
            List<HousingAssignmentStatus> statuses = new List<HousingAssignmentStatus>();

            if (source != null && source.Any())
            {
                foreach (var item in source)
                {
                    HousingAssignmentStatus status = new HousingAssignmentStatus() 
                    {
                        Status = item.RmasStatusAssocMember,
                        StatusDate = item.RmasStatusDateAssocMember.HasValue? item.RmasStatusDateAssocMember.Value : default(DateTime?)
                    };
                    statuses.Add(status);
                }
            }

            return statuses.Any() ? statuses : null;
        }
    }
}
