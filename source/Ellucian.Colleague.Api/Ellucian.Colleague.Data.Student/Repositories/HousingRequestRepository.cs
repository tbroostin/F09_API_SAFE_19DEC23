// Copyright 2017-2023 Ellucian Company L.P. and its affiliates.

//using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class HousingRequestRepository : BaseColleagueRepository, IHousingRequestRepository
    {
        readonly int readSize;
        const string AllHousingRequestsRecordsCache = "AllHousingRequestsRecordKeys";
        const int AllHousingRequestsRecordsCacheTimeout = 20;
        RepositoryException exception = new RepositoryException();

        /// <summary>
        /// ...ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public HousingRequestRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Returns a tuple containing housing requests entities
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<HousingRequest>, int>> GetHousingRequestsAsync(int offset, int limit, bool bypassCache)
        {

            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllHousingRequestsRecordsCache);
            List<HousingRequest> housingRequests = new List<HousingRequest>();

            if (limit == 0) limit = readSize;
            int totalCount = 0;
            var selectionCriteria = new StringBuilder();

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                "ROOM.ASSIGNMENT",
                offset,
                limit,
                AllHousingRequestsRecordsCacheTimeout,
                async () =>
                {
                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        criteria = "WITH RMAS.PERSON.ID NE '' AND WITH RMAS.START.DATE NE '' AND WITH RMAS.STATUS NE '' AND WITH RMAS.INTG.KEY.IDX NE '' BY.EXP RMAS.INTG.KEY.IDX"
                    };
                    return requirements;
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<HousingRequest>, int>(new List<HousingRequest>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;

            var subList = keyCacheObject.Sublist.ToArray();
            if (subList == null || !subList.Any())
            {
                return new Tuple<IEnumerable<HousingRequest>, int>(new List<HousingRequest>(), 0);
            }

            var roomAssignmentContracts = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.RoomAssignment>("ROOM.ASSIGNMENT", subList);
            if ((roomAssignmentContracts.InvalidKeys != null && roomAssignmentContracts.InvalidKeys.Any()) ||
               roomAssignmentContracts.InvalidRecords != null && roomAssignmentContracts.InvalidRecords.Any())
            {
                if (roomAssignmentContracts.InvalidKeys.Any())
                {
                    exception.AddErrors(roomAssignmentContracts.InvalidKeys
                        .Select(key => new RepositoryError("Bad.Data",
                        string.Format("Unable to locate the following ROOM.ASSIGNMENT key '{0}'.", key.ToString()))));
                }
                if (roomAssignmentContracts.InvalidRecords.Any())
                {
                    exception.AddErrors(roomAssignmentContracts.InvalidRecords
                       .Select(r => new RepositoryError("Bad.Data",
                       string.Format("Error: '{0}' ", r.Value))
                       { SourceId = r.Key }));
                }
            }

            if (roomAssignmentContracts.BulkRecordsRead != null)
            {
                var roomPreferencesList = roomAssignmentContracts.BulkRecordsRead.Select(i => i.RmasPreference).Distinct().ToArray();

                //Then get all room preferences based on the Id's from room assignment.
                var roomPreferences = await DataReader.BulkReadRecordAsync<RoomPreferences>("ROOM.PREFERENCES", roomPreferencesList);

                Dictionary<string, string> dict = null;
                try
                {
                    dict = await GetGuidsCollectionAsync(subList);
                }
                catch (Exception ex)
                {
                    // Suppress any possible exception with missing primary GUIDs.  We will report any missing GUIDs in a collection as
                    // we process the list of room assignments   
                    logger.Error(ex, "Unable to get guid for room preference.");
                }
                if (dict == null || !dict.Any())
                {
                    exception.AddError(new RepositoryError("Bad.Data", "GUIDs not found for ROOM.ASSIGNMENT with RMAS.INTG.KEY.IDX."));
                    throw exception;
                }

                foreach (var roomAssignmentContract in roomAssignmentContracts.BulkRecordsRead)
                {
                    if (roomAssignmentContract != null)
                    {

                        try
                        {
                            housingRequests.Add(BuildHousingRequest(roomAssignmentContract, dict, roomPreferences));
                        }
                        catch (Exception e)
                        {
                            exception.AddError(new RepositoryError("Bad.Data", e.Message)
                            {
                                SourceId = roomAssignmentContract.Recordkey,
                                Id = roomAssignmentContract.RecordGuid
                            });
                        }
                    }
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return housingRequests.Any() ?
               new Tuple<IEnumerable<Domain.Student.Entities.HousingRequest>, int>(housingRequests, totalCount) :
               new Tuple<IEnumerable<Domain.Student.Entities.HousingRequest>, int>(new List<Domain.Student.Entities.HousingRequest>(), 0);

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// /// <summary>
        /// Returns a housing request entity
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<HousingRequest> GetHousingRequestByGuidAsync(string guid)
        {
            var entity = await this.GetRecordInfoFromGuidAsync(guid);

            if (entity == null)
            {
                throw new KeyNotFoundException(string.Format("No housing-requests was found for id {0}", guid));
            }
            if (entity.Entity != "ROOM.ASSIGNMENT")
            {
                exception.AddError(new RepositoryError("GUID.Wrong.Type", "GUID '" + guid + "' has different entity, '" + entity.Entity + "', than expected, 'ROOM.ASSIGNMENT'"));
                throw exception;
            }
            var roomAssignmentId = entity.PrimaryKey;
            if (string.IsNullOrWhiteSpace(roomAssignmentId))
            {
                throw new KeyNotFoundException(string.Format("No housing-requests was found for id {0}", guid));
            }
            var roomAssignmentDataContract = await DataReader.ReadRecordAsync<DataContracts.RoomAssignment>("ROOM.ASSIGNMENT", roomAssignmentId);
            if (roomAssignmentDataContract == null)
            {
                throw new KeyNotFoundException(string.Format("Unable to locate room assignment record with id of '{0}'", roomAssignmentId));
            }
            //Then get all room preferences based on the Id's from room assignment.

            var roomPreferencesDataContract = await DataReader.BulkReadRecordAsync<RoomPreferences>("ROOM.PREFERENCES", new string[] { roomAssignmentDataContract.RmasPreference });

            Dictionary<string, string> dict = null;
            try
            {
                dict = await GetGuidsCollectionAsync(new List<string>() { entity.PrimaryKey });
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                exception.AddError(new RepositoryError("Bad.Data", "Guids not found for ROOM.ASSIGNMENT with RMAS.INTG.KEY.IDX."));
                throw exception;
            }
            if (dict == null || !dict.Any())
            {
                exception.AddError(new RepositoryError("Bad.Data", "Guids not found for ROOM.ASSIGNMENT with RMAS.INTG.KEY.IDX."));
                throw exception;
            }

            HousingRequest housingRequest = BuildHousingRequest(roomAssignmentDataContract, dict, roomPreferencesDataContract);

            if (exception != null && exception.Errors.Any())
                throw exception;

            return housingRequest;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Returns key for the housing request.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<string> GetHousingRequestKeyAsync(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("guid");
                }

                var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
                if (idDict == null || idDict.Count == 0)
                {
                    throw new KeyNotFoundException("Housing request GUID " + guid + " not found.");
                }

                var foundEntry = idDict.FirstOrDefault();
                if (foundEntry.Value == null)
                {
                    throw new KeyNotFoundException("Housing request GUID " + guid + " lookup failed.");
                }

                if (foundEntry.Value.Entity != "ROOM.ASSIGNMENT")
                {
                    throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, ROOM.ASSIGNMENT");
                }

                return foundEntry.Value.PrimaryKey;
            }
            catch (KeyNotFoundException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all the guids for the person keys
        /// </summary>
        /// <param name="personRecordKeys"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> personRecordKeys)
        {
            if (personRecordKeys != null && !personRecordKeys.Any())
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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Create/Update housing request.
        /// </summary>
        /// <param name="housingRequestEntity"></param>
        /// <returns></returns>
        public async Task<HousingRequest> UpdateHousingRequestAsync(HousingRequest source)
        {
            try
            {
                if (source == null)
                {
                    throw new ArgumentNullException("housingRequest", "Housing request must be provided.");
                }
                /*
                    There are future changes are coming to the API, 
                */
                CreateUpdateRoomReqRequest request = new CreateUpdateRoomReqRequest()
                {
                    Guid = source.Guid,
                    //RoomRequestsId = source.RecordKey,
                    RoomAssignmentId = source.RecordKey,
                    AcademicPeriodId = string.IsNullOrEmpty(source.Term) ? string.Empty : source.Term,
                    EndOn = source.EndDate.HasValue ? source.EndDate.Value.Date : default(DateTime?),
                    LotteryNumber = source.LotteryNo,
                    PersonId = source.PersonId,
                    StartOn = source.StartDate.HasValue ? source.StartDate.Value.Date : default(DateTime?),
                    Status = source.Status,
                    BuildingPreferences = BuildPreference(source.RoomPreferences),
                    FloorCharacteristic = source.FloorCharacteristic,
                    FloorCharacteristicRequired = source.FloorCharacteristicReqd,
                    RoomCharacteristicPreferences = BuildRoomCharacteristicPreferences(source.RoomCharacerstics),
                    RoommateCharacteristicPreferences = BuildRoommateCharacteristicPreferences(source.RoommateCharacteristicPreferences),
                    RoommatePreferences = BuildRoommatePreferences(source.RoommatePreferences)
                };

                var extendedDataTuple = GetEthosExtendedDataLists();
                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    request.ExtendedNames = extendedDataTuple.Item1;
                    request.ExtendedValues = extendedDataTuple.Item2;
                }

                var updateResponse = await transactionInvoker.ExecuteAsync<CreateUpdateRoomReqRequest, CreateUpdateRoomReqResponse>(request);

                if (updateResponse.CreateUpdateRoomRequestErrors != null && updateResponse.CreateUpdateRoomRequestErrors.Any())
                {
                    var errorMessage = new StringBuilder();
                    errorMessage.Append(string.Format("Error(s) occurred updating housing request for guid: '{0}': ", request.Guid));
                    updateResponse.CreateUpdateRoomRequestErrors.ForEach(err =>
                    {
                        errorMessage.Append(string.Format("{0}{1}", Environment.NewLine, err.ErrorMessages));
                        logger.Error(errorMessage.ToString());
                    });

                    throw new InvalidOperationException(errorMessage.ToString());
                }

                return await this.GetHousingRequestByGuidAsync(updateResponse.Guid);
            }
            catch (ArgumentNullException e)
            {
                throw e;
            }
            catch (InvalidOperationException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>        
        /// <summary>
        /// Builds housing request
        /// </summary>
        /// <param name="housingReqRec"></param>
        /// <param name="roomPreferences"></param>
        /// <returns></returns>
        private HousingRequest BuildHousingRequest(RoomAssignment roomAssignmentDataContract, Dictionary<string, string> dict, Collection<RoomPreferences> roomPreferences)
        {
            HousingRequest housingRequest = null;
            string guid = string.Empty;
            string key = string.Concat(roomAssignmentDataContract.Recordkey, "|", roomAssignmentDataContract.Recordkey);
            dict.TryGetValue(key, out guid);
            if (string.IsNullOrEmpty(guid))
            {
                exception.AddError(new RepositoryError("Bad.Data", string.Concat("Guid not found for ROOM.ASSIGNMENT " + roomAssignmentDataContract.Recordkey)));
            }
            else
            {
                var roomPreference = roomPreferences.FirstOrDefault(i => i.Recordkey.Equals(roomAssignmentDataContract.RmasPreference, StringComparison.OrdinalIgnoreCase));

                var status = BuildStatus(roomAssignmentDataContract.RmasStatusesEntityAssociation);

                housingRequest = new HousingRequest(guid, roomAssignmentDataContract.Recordkey,
                    roomAssignmentDataContract.RmasStartDate.HasValue ? roomAssignmentDataContract.RmasStartDate : default(DateTime?), status)
                {
                    EndDate = roomAssignmentDataContract.RmasEndDate,
                    LotteryNo = roomAssignmentDataContract.RmasLotteryNo,
                    PersonId = roomAssignmentDataContract.RmasPersonId,
                    Term = roomAssignmentDataContract.RmasTerm,
                    RoomPreferences = roomPreference == null ? null : BuildRoomPreferences(roomPreference),
                    RoomCharacerstics = roomPreference == null ? null : BuildRoomCharateristics(roomPreference),
                    RoommateCharacteristicPreferences = roomPreference == null ? null : BuildRoomateCharacteristicPreferences(roomPreference),
                    RoommatePreferences = roomPreference == null ? null : BuildRoommatePreference(roomPreference),
                    FloorCharacteristic = roomPreference == null ? null : roomPreference.RmprFloorPreference,
                    FloorCharacteristicReqd = roomPreference == null ? null : roomPreference.RmprFloorReqdFlag
                };
            }
            return housingRequest;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Builds room characteristic
        /// </summary>
        /// <param name="roomPreference"></param>
        /// <returns></returns>
        private IEnumerable<RoomCharacteristicPreference> BuildRoomCharateristics(RoomPreferences roomPreference)
        {
            List<RoomCharacteristicPreference> rmCharPrefs = new List<RoomCharacteristicPreference>();

            if (roomPreference.RoomCharsEntityAssociation != null && roomPreference.RoomCharsEntityAssociation.Any())
            {
                foreach (var rmChar in roomPreference.RoomCharsEntityAssociation)
                {
                    RoomCharacteristicPreference rmCharPref = new RoomCharacteristicPreference()
                    {
                        RoomCharacteristic = rmChar.RmprRoomCharsAssocMember,
                        RoomCharacteristicRequired = rmChar.RmprRoomCharReqdFlagAssocMember
                    };
                    rmCharPrefs.Add(rmCharPref);
                }
            }
            return rmCharPrefs.Any() ? rmCharPrefs : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Builds status
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private string BuildStatus(List<RoomAssignmentRmasStatuses> sources)
        {
            if (sources == null || !sources.Any())
            {
                return string.Empty;
            }
            // Colleague considers the topmost/first status in the list to be the effective status.  Colleague does not keep it in sorted order.
            var roomRequestsIntgRoomReqIntgStatus = sources.FirstOrDefault();
            return roomRequestsIntgRoomReqIntgStatus == null ? string.Empty : roomRequestsIntgRoomReqIntgStatus.RmasStatusAssocMember;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Builds room preferences entity
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private List<RoomPreference> BuildRoomPreferences(RoomPreferences source)
        {
            List<RoomPreference> roomPrefs = new List<RoomPreference>();
            if (source != null && source.RmprPreferencesEntityAssociation.Any())
            {
                foreach (var RmprPreferenceEntity in source.RmprPreferencesEntityAssociation)
                {
                    RoomPreference roomPref = new RoomPreference()
                    {
                        Building = RmprPreferenceEntity.RmprBldgPreferencesAssocMember,
                        BuildingReqdFlag = RmprPreferenceEntity.RmprBldgReqdFlagAssocMember,
                        Floor = RmprPreferenceEntity.RmprBldgFloorPreferencesAssocMember.ToString(),
                        FloorReqd = RmprPreferenceEntity.RmprBldgFloorReqdFlagAssocMember,
                        Room = RmprPreferenceEntity.RmprRoomPreferencesAssocMember,
                        RoomReqdFlag = RmprPreferenceEntity.RmprRoomReqdFlagAssocMember,
                        Wing = RmprPreferenceEntity.RmprBldgWingPreferencesAssocMember,
                        WingReqdFlag = RmprPreferenceEntity.RmprWingReqdFlagAssocMember
                    };
                    roomPrefs.Add(roomPref);
                }
            }
            return roomPrefs.Any() ? roomPrefs : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Builds roommate preferences entity
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private List<RoommateCharacteristicPreference> BuildRoomateCharacteristicPreferences(RoomPreferences source)
        {
            List<RoommateCharacteristicPreference> roommateCharPrefs = new List<RoommateCharacteristicPreference>();
            if (source.RoommateCharsEntityAssociation != null && source.RoommateCharsEntityAssociation.Any())
            {
                foreach (var RoommateChar in source.RoommateCharsEntityAssociation)
                {
                    RoommateCharacteristicPreference roomatePref = new RoommateCharacteristicPreference()
                    {
                        RoommateCharacteristic = RoommateChar.RmprRoommateCharsAssocMember,
                        RoommateCharacteristicRequired = RoommateChar.RmprMateCharsReqdFlagAssocMember
                    };
                    roommateCharPrefs.Add(roomatePref);
                }
            }
            return roommateCharPrefs.Any() ? roommateCharPrefs : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Build Roommate Preference
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private List<RoommatePreference> BuildRoommatePreference(RoomPreferences source)
        {
            List<RoommatePreference> roommatePrefs = new List<RoommatePreference>();
            if (source.RoommatePreferencesEntityAssociation != null && source.RoommatePreferencesEntityAssociation.Any())
            {
                foreach (var roommatePreference in source.RoommatePreferencesEntityAssociation)
                {
                    RoommatePreference roomatePref = new RoommatePreference()
                    {
                        RoommateId = roommatePreference.RmprRoommatePreferencesAssocMember,
                        RoommateRequired = roommatePreference.RmprRoommateReqdFlagAssocMember
                    };
                    roommatePrefs.Add(roomatePref);
                }
            }
            return roommatePrefs.Any() ? roommatePrefs : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Builds preferences.
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private List<BuildingPreferences> BuildPreference(List<RoomPreference> sources)
        {
            List<BuildingPreferences> buildingPrefs = new List<BuildingPreferences>();
            if (sources != null && sources.Any())
            {
                foreach (var source in sources)
                {
                    BuildingPreferences buildingPref = new BuildingPreferences()
                    {
                        Buildings = source.Building,
                        BuildingsRequired = source.BuildingReqdFlag,
                        Floors = source.Floor,
                        FloorsRequired = source.FloorReqd,
                        Rooms = source.Room,
                        RoomsRequired = source.RoomReqdFlag,
                        Sites = source.Site,
                        Wings = source.Wing,
                        WingsRequired = source.WingReqdFlag
                    };
                    buildingPrefs.Add(buildingPref);
                }
            }
            return buildingPrefs.Any() ? buildingPrefs : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Builds room characterstic preference
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private List<RoomCharacteristicPreferences> BuildRoomCharacteristicPreferences(IEnumerable<RoomCharacteristicPreference> sources)
        {
            List<RoomCharacteristicPreferences> rmcharPrefs = new List<RoomCharacteristicPreferences>();

            if (sources != null && sources.Any())
            {
                foreach (var source in sources)
                {
                    RoomCharacteristicPreferences rmCharPref = new RoomCharacteristicPreferences()
                    {
                        RoomCharacteristics = source.RoomCharacteristic,
                        RoomCharacteristicsRequired = source.RoomCharacteristicRequired
                    };
                    rmcharPrefs.Add(rmCharPref);
                }
            }
            return rmcharPrefs.Any() ? rmcharPrefs : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Builds roommate characterstics preference.
        /// </summary>
        /// <param name="sources"></param>
        ///// <returns></returns>
        private List<RoommateCharacteristicPreferences> BuildRoommateCharacteristicPreferences(List<RoommateCharacteristicPreference> sources)
        {
            List<RoommateCharacteristicPreferences> rmmateCharPrefs = new List<RoommateCharacteristicPreferences>();

            if (sources != null && sources.Any())
            {
                foreach (var source in sources)
                {
                    RoommateCharacteristicPreferences rmmateCharPref = new RoommateCharacteristicPreferences()
                    {
                        RoommateCharacteristics = source.RoommateCharacteristic,
                        RoommateCharacteristicsReqd = source.RoommateCharacteristicRequired
                    };
                    rmmateCharPrefs.Add(rmmateCharPref);
                }

            }

            return rmmateCharPrefs.Any() ? rmmateCharPrefs : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Builds roommate preference.
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private List<RoommatePreferences> BuildRoommatePreferences(List<RoommatePreference> sources)
        {
            List<RoommatePreferences> rmmatePrefs = new List<RoommatePreferences>();

            if (sources != null && sources.Any())
            {
                foreach (var source in sources)
                {
                    RoommatePreferences rmmatePref = new RoommatePreferences()
                    {
                        Roommates = source.RoommateId,
                        RoommatesRequired = source.RoommateRequired
                    };
                    rmmatePrefs.Add(rmmatePref);
                }
            }

            return rmmatePrefs.Any() ? rmmatePrefs : null;
        }

        /// <summary>
        /// Using a collection of ids with guids
        ///  get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of ids</param>
        /// <returns>Dictionary consisting of a room.assignment.id with guids from secondary key</returns>
        private async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(key => new RecordKeyLookup("ROOM.ASSIGNMENT", key,
                    "RMAS.INTG.KEY.IDX", key, false))
                    .ToArray();

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
                                guidCollection.Add(string.Concat(splitKeys[1], "|", splitKeys[2]), recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Format("Error occured while getting guids for {0}.", "ROOM.ASSIGNMENT"), ex);
            }

            return guidCollection;
        }
    }
}
