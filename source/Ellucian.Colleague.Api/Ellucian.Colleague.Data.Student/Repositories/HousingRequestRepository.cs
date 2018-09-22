// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

//using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
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
        private static char _VM = Convert.ToChar(DynamicArray.VM);

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
            try
            {
                List<HousingRequest> roomReqList = new List<HousingRequest>();

                string criteria = "WITH RMAS.PERSON.ID NE '' AND WITH RMAS.START.DATE NE '' AND WITH RMAS.STATUS NE '' BY.EXP RMAS.INTG.KEY.IDX";
                var rmAssignmentIds = await DataReader.SelectAsync("ROOM.ASSIGNMENT", criteria);

                var rmAssignmentIds2 = new List<string>();
                foreach (var rmAssignmentId in rmAssignmentIds)
                {
                    var perposKey = rmAssignmentId.Split(_VM)[0];
                    rmAssignmentIds2.Add(perposKey);
                }

                var criteria2 = "WITH RMAS.PERSON.ID NE '' AND WITH RMAS.START.DATE NE '' AND WITH RMAS.STATUS NE '' BY.EXP RMAS.INTG.KEY.IDX SAVING RMAS.INTG.KEY.IDX";
                var rmAssignmentIdxs = await DataReader.SelectAsync("ROOM.ASSIGNMENT", criteria2);

                var keys = new List<string>();
                var idx = 0;
                foreach (var rmAssignmentId2 in rmAssignmentIds2)
                {
                    keys.Add(String.Concat(rmAssignmentId2, "|", rmAssignmentIdxs.ElementAt(idx)));
                    idx++;
                }

                var totalCount = keys.Count();
                keys.Sort();
                var keysSubList = keys.Skip(offset).Take(limit).Distinct().ToArray();

                if (keysSubList.Any())
                {
                    var subList = new List<string>();

                    foreach (var key in keysSubList)
                    {
                        var applKey = key.Split('|')[0];
                        subList.Add(applKey);
                    }
                    var rmAssignmentDataContracts = await DataReader.BulkReadRecordAsync<RoomAssignment>("ROOM.ASSIGNMENT", subList.Distinct().ToArray());
                    if (rmAssignmentDataContracts == null || !rmAssignmentDataContracts.Any())
                    {
                        return new Tuple<IEnumerable<HousingRequest>, int>(roomReqList, 0);
                    }

                    var rmPrefList = rmAssignmentDataContracts.Select(i => i.RmasPreference).Distinct().ToArray();

                    //Then get all room preferences based on the Id's from room assignment.
                    var roomPreferences = await DataReader.BulkReadRecordAsync<RoomPreferences>("ROOM.PREFERENCES", rmPrefList);

                    foreach (var key in keysSubList)
                    {
                        var splitKey = key.Split('|');
                        var rmAssignmentKey = splitKey[0];
                        var rmReqKey = splitKey[1];

                        var rmAssignmentDataContract = rmAssignmentDataContracts.FirstOrDefault(ra => ra.Recordkey == rmAssignmentKey);
                        if (rmAssignmentDataContract == null)
                        {
                            throw new KeyNotFoundException(string.Format("No housing request was found for id {0}.", rmAssignmentKey));
                        }


                        if (!string.IsNullOrEmpty(rmAssignmentDataContract.RmasIntgKeyIdx) && rmReqKey.Equals(rmAssignmentDataContract.RmasIntgKeyIdx))
                        {
                            try
                            {
                                var rmRequestGuidInfo = await GetGuidFromRecordInfoAsync("ROOM.ASSIGNMENT", rmAssignmentKey, "RMAS.INTG.KEY.IDX", rmAssignmentDataContract.RmasIntgKeyIdx);
                                if (string.IsNullOrEmpty(rmRequestGuidInfo))
                                {
                                    throw new KeyNotFoundException(string.Format("No housing request was found for id {0}.", rmAssignmentKey));
                                }
                                rmAssignmentDataContract.RecordGuid = rmRequestGuidInfo;
                                HousingRequest roomRequest = BuildHousingRequest(rmAssignmentDataContract, roomPreferences);
                                roomReqList.Add(roomRequest);
                            }catch(RepositoryException e)
                            {
                                throw new ArgumentException(e.Message);
                            }
                        }
                    }
                }

                return roomReqList.Any() ? new Tuple<IEnumerable<HousingRequest>, int>(roomReqList, totalCount) :
                                           new Tuple<IEnumerable<HousingRequest>, int>(roomReqList, 0);
            }
            catch (Exception)
            {                
                throw;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// /// <summary>
        /// Returns a housing request entity
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<HousingRequest> GetHousingRequestByGuidAsync(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Guid is required.");
                }
                var housingRequestId = await this.GetHousingRequestKeyAsync(guid);

                if (string.IsNullOrEmpty(housingRequestId))
                {
                    throw new KeyNotFoundException(string.Format("No housing request was found for guid {0}.", guid));
                }
                RoomAssignment housingReqDataContract = null;
                try
                {
                    housingReqDataContract = await DataReader.ReadRecordAsync<RoomAssignment>("ROOM.ASSIGNMENT", housingRequestId);
                }
                catch(Exception e)
                {
                    throw new KeyNotFoundException(string.Format("No housing request was found for guid {0}.", guid));
                }

                if (housingReqDataContract == null)
                {
                    throw new KeyNotFoundException(string.Format("No housing request was found for guid {0}.", guid));
                }

                var rmRequestGuidInfo = await GetGuidFromRecordInfoAsync("ROOM.ASSIGNMENT", housingReqDataContract.Recordkey, "RMAS.INTG.KEY.IDX", housingReqDataContract.RmasIntgKeyIdx);
                if (string.IsNullOrEmpty(rmRequestGuidInfo))
                {
                    throw new KeyNotFoundException(string.Format("No housing request was found for guid {0}.", guid));
                }

                if (!rmRequestGuidInfo.Equals(guid, StringComparison.OrdinalIgnoreCase))
                {
                    throw new KeyNotFoundException(string.Format("No housing request was found for guid {0}.", guid));
                }
                housingReqDataContract.RecordGuid = rmRequestGuidInfo;

                //Then get all room preferences based on the Id's from room assignment.
                var roomPreferences = await DataReader.BulkReadRecordAsync<RoomPreferences>("ROOM.PREFERENCES", new string[] { housingReqDataContract.RmasPreference });                

                HousingRequest housingRequest = BuildHousingRequest(housingReqDataContract, roomPreferences);

                return housingRequest;
            }
            catch (Exception)
            {                
                throw;
            }
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
        private HousingRequest BuildHousingRequest(RoomAssignment roomAssignmentDataContract, Collection<RoomPreferences> roomPreferences)
        {
            var roomPreference = roomPreferences.FirstOrDefault(i => i.Recordkey.Equals(roomAssignmentDataContract.RmasPreference, StringComparison.OrdinalIgnoreCase));

            var status = BuildStatus(roomAssignmentDataContract.RmasStatusesEntityAssociation);

            HousingRequest housingRequest = new HousingRequest(roomAssignmentDataContract.RecordGuid, roomAssignmentDataContract.Recordkey,
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
    }
}
