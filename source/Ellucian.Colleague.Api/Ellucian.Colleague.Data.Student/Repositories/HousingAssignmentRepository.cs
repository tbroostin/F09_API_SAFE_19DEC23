// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
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
        public async Task<Tuple<IEnumerable<HousingAssignment>, int>> GetHousingAssignmentsAsync(int offset, int limit, bool bypassCache)
        {
            List<HousingAssignment> housingAssignmentEntities = new List<HousingAssignment>();
            string criteria = "WITH RMAS.ROOM NE '' AND RMAS.BLDG NE ''";
            var totalCount = 0;

            var housingAssignmentIds = await DataReader.SelectAsync("ROOM.ASSIGNMENT", criteria);

            totalCount = housingAssignmentIds.Count();

            if (totalCount == 0 || totalCount <= offset)
            {
                return new Tuple<IEnumerable<Domain.Student.Entities.HousingAssignment>, int>(housingAssignmentEntities, 0);
            }

            var sublist = housingAssignmentIds.OrderBy(id => id).Skip(offset).Take(limit).ToArray();

            var housingAssignmentDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.RoomAssignment>("ROOM.ASSIGNMENT", sublist);

            if (housingAssignmentDataContracts != null && housingAssignmentDataContracts.Any())
            {
                //Get records from AR.ADDNL.AMTS
                string[] rmAssignmentIds = housingAssignmentDataContracts.Select(i => i.Recordkey).Distinct().ToArray();
                var arAddnlAmtsIds = await DataReader.SelectAsync("AR.ADDNL.AMTS", "WITH ARAA.ROOM.ASSIGNMENT EQ '?'", rmAssignmentIds);

                var housingAssignmentDict = await GetHousingGuidDictionary(rmAssignmentIds, housingAssignmentCriteria);

                var housingRequestDict = await GetHousingGuidDictionary(rmAssignmentIds, housingRequestCriteria);

                var arAddnlAmtsDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.ArAddnlAmts>("AR.ADDNL.AMTS", arAddnlAmtsIds);
                
                foreach (var housingAssignmentDataContract in housingAssignmentDataContracts)
                {
                    Domain.Student.Entities.HousingAssignment housingAssignmentEntity = BuildHousingAssignmentEntity(housingAssignmentDataContract, arAddnlAmtsDataContracts, housingAssignmentDict, housingRequestDict);
                    housingAssignmentEntities.Add(housingAssignmentEntity);
                }
            }

            return housingAssignmentEntities.Any()? new Tuple<IEnumerable<HousingAssignment>, int>(housingAssignmentEntities, totalCount) : 
                new Tuple<IEnumerable<Domain.Student.Entities.HousingAssignment>, int>(null, 0);
        }

        /// <summary>
        /// Gets dictionary with colleague id and guid key pair for APPLICATIONS.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        string housingAssignmentCriteria = "WITH LDM.GUID.SECONDARY.FLD EQ '' AND LDM.GUID.SECONDARY.KEY EQ '' AND LDM.GUID.LDM.NAME EQ 'housing-assignments' AND LDM.GUID.PRIMARY.KEY EQ '{0}'";
        string housingRequestCriteria = "WITH LDM.GUID.SECONDARY.FLD EQ 'RMAS.INTG.KEY.IDX' AND LDM.GUID.LDM.NAME EQ 'housing-requests' AND LDM.GUID.SECONDARY.KEY EQ '{0}'";
        private async Task<Dictionary<string, string>> GetHousingGuidDictionary(IEnumerable<string> ids, string criteria)
        {
            if (ids == null || !Enumerable.Any<string>(ids))
            {
                throw new ArgumentNullException("Application id's are required.");
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var id in ids)
            {
                var guidRecords = await DataReader.SelectAsync("LDM.GUID", string.Format(criteria, id));
                if (!dict.ContainsKey(id))
                {
                    if (guidRecords != null && guidRecords.Any())
                    {
                        dict.Add(id, guidRecords[0]);
                    }
                }
            }
            return dict;
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
                throw new KeyNotFoundException(string.Format("No housing assignment was found for guid {0}.", guid));
            }

            //Get records from AR.ADDNL.AMTS
            string[] rmAssignmentIds = new[] { housingAssignmentDC.Recordkey };
            var arAddnlAmtsIds = await DataReader.SelectAsync("AR.ADDNL.AMTS", "WITH ARAA.ROOM.ASSIGNMENT EQ '?'", rmAssignmentIds);

            var arAddnlAmtsDataContracts = await DataReader.BulkReadRecordAsync<DataContracts.ArAddnlAmts>("AR.ADDNL.AMTS", arAddnlAmtsIds);

            var haDict = await GetHousingGuidDictionary(new List<string>() { housingAssignmentId }, housingAssignmentCriteria);
            var hrDict = await GetHousingGuidDictionary(new List<string>() { housingAssignmentId }, housingRequestCriteria);

            if(hrDict.Values.Contains(guid))
            {
                throw new KeyNotFoundException(string.Format("No housing assignment was found for guid {0}.", guid));
            }

            Domain.Student.Entities.HousingAssignment housingAssignmentEntity = BuildHousingAssignmentEntity(housingAssignmentDC, arAddnlAmtsDataContracts, haDict, hrDict);
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
        private Domain.Student.Entities.HousingAssignment BuildHousingAssignmentEntity(DataContracts.RoomAssignment source, IEnumerable<ArAddnlAmts> arAddnlAmtsDataContracts, IDictionary<string, string> housingAssignmentDict, IDictionary<string, string> housingRequestDict)
        {
            Domain.Student.Entities.HousingAssignment housingAssignmentEntity = new HousingAssignment(GetGuid(source.Recordkey, housingAssignmentDict), source.Recordkey,
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
