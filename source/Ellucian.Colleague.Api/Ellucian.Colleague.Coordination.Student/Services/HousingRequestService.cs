//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class HousingRequestsService : BaseCoordinationService, IHousingRequestService
    {

        private readonly IHousingRequestRepository _housingRequestRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ITermRepository _termRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;

        public HousingRequestsService(
            IHousingRequestRepository housingRequestRepository,
            IPersonRepository personRepository,
            ITermRepository termRepository,
            IRoomRepository roomRepository,
            IReferenceDataRepository referenceDataRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _housingRequestRepository = housingRequestRepository;
            _personRepository = personRepository;
            _termRepository = termRepository;
            _roomRepository = roomRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
        }

        #region All the reference data

        /// <summary>
        /// Clears all reference data.
        /// </summary>
        private void ClearReferenceData()
        {
            _academicPeriods = null;
            _buildings = null;
            _buildingWings = null;
            _floorCharacteristics = null;
            _sites = null;
            _rooms = null;
            _roomCharacteristics = null;
            _roommateCharacteristics = null;
        }

        //Academic Periods
        private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriods;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicPeriod>> GetAcademicPeriods()
        {
            if (_academicPeriods == null)
            {
                var termEntities = await _termRepository.GetAsync();
                _academicPeriods = _termRepository.GetAcademicPeriods(termEntities);
            }
            return _academicPeriods;
        }

        //Buildings
        private IEnumerable<Domain.Base.Entities.Building> _buildings;
        private async Task<IEnumerable<Domain.Base.Entities.Building>> GetBuildings(bool bypassCache)
        {
            if (_buildings == null)
            {
                _buildings = await _referenceDataRepository.GetBuildingsAsync(bypassCache);
            }
            return _buildings;
        }

        //Sites
        private IEnumerable<Domain.Base.Entities.Location> _sites;
        private async Task<IEnumerable<Domain.Base.Entities.Location>> GetLocationsAsync(bool bypassCache)
        {
            if (_sites == null)
            {
                _sites = await _referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _sites;
        }

        //Building Wings
        private IEnumerable<Domain.Base.Entities.RoomWing> _buildingWings;
        private async Task<IEnumerable<Domain.Base.Entities.RoomWing>> GetBuildingWings(bool bypassCache)
        {
            if (_buildingWings == null)
            {
                _buildingWings = await _referenceDataRepository.GetRoomWingsAsync(bypassCache);
            }
            return _buildingWings;
        }

        //Floor characteristics
        private IEnumerable<Domain.Student.Entities.FloorCharacteristics> _floorCharacteristics;
        private async Task<IEnumerable<Domain.Student.Entities.FloorCharacteristics>> GetFloorCharacteristics(bool bypassCache)
        {
            if (_floorCharacteristics == null)
            {
                _floorCharacteristics = await _studentReferenceDataRepository.GetFloorCharacteristicsAsync(bypassCache);
            }
            return _floorCharacteristics;
        }

        //Rooms
        private IEnumerable<Domain.Base.Entities.Room> _rooms;
        private async Task<IEnumerable<Domain.Base.Entities.Room>> GetRooms(bool bypassCache)
        {
            if (_rooms == null)
            {
                _rooms = await _roomRepository.GetRoomsAsync(bypassCache);
            }
            return _rooms;
        }

        //Rooms Characteristsics
        private IEnumerable<Domain.Base.Entities.RoomCharacteristic> _roomCharacteristics;
        private async Task<IEnumerable<Domain.Base.Entities.RoomCharacteristic>> GetRoomCharacteristics(bool bypassCache)
        {
            if (_roomCharacteristics == null)
            {
                _roomCharacteristics = await _referenceDataRepository.GetRoomCharacteristicsAsync(bypassCache);
            }
            return _roomCharacteristics;
        }

        //Roommate Characteristsics
        private IEnumerable<Domain.Student.Entities.RoommateCharacteristics> _roommateCharacteristics;
        private async Task<IEnumerable<Domain.Student.Entities.RoommateCharacteristics>> GetRoommateCharacteristics(bool bypassCache)
        {
            if (_roommateCharacteristics == null)
            {
                _roommateCharacteristics = await _studentReferenceDataRepository.GetRoommateCharacteristicsAsync(bypassCache);
            }
            return _roommateCharacteristics;
        }

        //PersonGuids
        /// <summary>
        /// Person ids, guid key value pairs
        /// </summary>
        private IDictionary<string, string> _personGuidsDict;
        private async Task<IDictionary<string, string>> GetPersonGuidsAsync()
        {
            if (_personIds != null && _personIds.Any())
            {
                IDictionary<string, string> dict = await _housingRequestRepository.GetPersonGuidsAsync(_personIds);
                if (dict != null && dict.Any())
                {
                    _personGuidsDict = new Dictionary<string, string>();
                    dict.ToList().ForEach(i =>
                    {
                        if (!_personGuidsDict.ContainsKey(i.Key))
                        {
                            _personGuidsDict.Add(i.Key, i.Value);
                        }
                    });
                }
            }
            return _personGuidsDict;
        }

        /// <summary>
        /// Builds person record keys local cache
        /// </summary>
        private List<string> _personIds;
        private void BuildLocalPersonGuids(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.HousingRequest> housingRequestsEntities)
        {
            _personIds = new List<string>();
            if (housingRequestsEntities != null && housingRequestsEntities.Any())
            {
                List<string> personIds = new List<string>();

                var ids = housingRequestsEntities.Select(i => i.PersonId).Distinct().ToList();
                if (ids != null && ids.Any())
                {
                    personIds.AddRange(ids);
                }

                foreach (var housingRequestsEntity in housingRequestsEntities)
                {
                    if (housingRequestsEntity.RoommatePreferences != null && housingRequestsEntity.RoommatePreferences.Any())
                    {
                        housingRequestsEntity.RoommatePreferences.ForEach(entity =>
                        {
                            if (!personIds.Contains(entity.RoommateId))
                            {
                                personIds.Add(entity.RoommateId);
                            }
                        });
                    }
                }
                _personIds.AddRange(personIds);
            }           
        }    
        #endregion

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all housing-requests
        /// </summary>
        /// <returns>Collection of HousingRequests DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.HousingRequest>, int>> GetHousingRequestsAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckViewHousingRequestPermissions();

            var housingRequestsCollection = new List<Ellucian.Colleague.Dtos.HousingRequest>();

            Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.HousingRequest>, int> housingRequestsEntitiesTuple = await _housingRequestRepository.GetHousingRequestsAsync(offset, limit, bypassCache);
            if (housingRequestsEntitiesTuple == null || !housingRequestsEntitiesTuple.Item1.Any())
            {
                return new Tuple<IEnumerable<HousingRequest>, int>(housingRequestsCollection, 0);
            }

            if (housingRequestsEntitiesTuple != null && housingRequestsEntitiesTuple.Item1.Any())
            {
                housingRequestsCollection.AddRange((await ConvertHousingRequestsEntitiesToDtos(housingRequestsEntitiesTuple.Item1, bypassCache)).ToList());
            }
            var housingRequestTuple = new Tuple<IEnumerable<Ellucian.Colleague.Dtos.HousingRequest>, int>(housingRequestsCollection, housingRequestsEntitiesTuple.Item2);
            return housingRequestTuple;
        }   

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a HousingRequests from its GUID
        /// </summary>
        /// <returns>HousingRequests DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.HousingRequest> GetHousingRequestByGuidAsync(string guid, bool bypassCache = false)
        {
            CheckViewHousingRequestPermissions();

            var housingRequestEntity = await _housingRequestRepository.GetHousingRequestByGuidAsync(guid);
            BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingRequest>() { housingRequestEntity });
            return await ConvertHousingRequestsEntityToDto(housingRequestEntity, bypassCache);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets record key.
        /// </summary>
        /// <param name="housingRequestDto"></param>
        /// <returns></returns>
        private async Task<string> GetRecordKey(HousingRequest housingRequestDto)
        {
            return await _housingRequestRepository.GetHousingRequestKeyAsync(housingRequestDto.Id);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Creates housing request.
        /// </summary>
        /// <param name="housingRequest"></param>
        /// <returns></returns>
        public async Task<HousingRequest> CreateHousingRequestAsync(HousingRequest housingRequestDto)
        {
            if (housingRequestDto == null)
            {
                throw new ArgumentNullException("housingRequestDto", "Must provide a guid for housing request create.");
            }

            try
            {
                return await this.UpdateHousingRequestAsync(null, housingRequestDto);
            }
            catch (Exception e)
            {                
                throw e;
            }

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Updates housing request.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="housingRequest"></param>
        /// <returns></returns>
        public async Task<HousingRequest> UpdateHousingRequestAsync(string guid, HousingRequest housingRequest)
        {
            if (housingRequest == null)
            {
                throw new ArgumentNullException("housingRequestDto", "Must provide a guid for housing request update.");
            }         

            try
            {
                CheckCreateUpdateHousingRequestPermissions();

                _housingRequestRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                
                var housingRequestEntity = await ConvertDtoToEntity(guid, housingRequest);

                Domain.Student.Entities.HousingRequest updatedHousingRequestEntity = await _housingRequestRepository.UpdateHousingRequestAsync(housingRequestEntity);

                BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingRequest>() { updatedHousingRequestEntity });

                ClearReferenceData();

                return await ConvertHousingRequestsEntityToDto(updatedHousingRequestEntity, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts dto to entity.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="housingRequestDto"></param>
        /// <returns></returns>
        private async Task<Ellucian.Colleague.Domain.Student.Entities.HousingRequest> ConvertDtoToEntity(string guid, HousingRequest housingRequestDto)
        {   
            //Create HousingRequest entity
            string status = ConvertStatusDtoToEntity(housingRequestDto.Status);

            if ((housingRequestDto.EndOn == null) && (housingRequestDto.AcademicPeriods == null))
            {
                throw new InvalidOperationException("Academic period or End on date is required for housing request.");
            }
            
            Ellucian.Colleague.Domain.Student.Entities.HousingRequest housingRequestEntity = string.IsNullOrEmpty(guid) ?
                new Domain.Student.Entities.HousingRequest(housingRequestDto.Id, housingRequestDto.StartOn, status) :
                new Domain.Student.Entities.HousingRequest(housingRequestDto.Id, await GetRecordKey(housingRequestDto), housingRequestDto.StartOn, status);

            housingRequestEntity.EndDate = housingRequestDto.EndOn;
            housingRequestEntity.LotteryNo = housingRequestDto.PriorityNumber;
            if(housingRequestDto.Person == null || string.IsNullOrWhiteSpace(housingRequestDto.Person.Id))
            {
                throw new ArgumentNullException("person.id", string.Format("Person is required for housing request. Id: {0}", guid));
            }

            var personId = await _personRepository.GetPersonIdFromGuidAsync(housingRequestDto.Person.Id);
            housingRequestEntity.PersonId = personId;
            //academicPeriods
            ValidateAcademicPeriods(housingRequestDto);
            if (housingRequestDto.AcademicPeriods != null && housingRequestDto.AcademicPeriods.Any(i => !string.IsNullOrEmpty(i.Id)))
            {
                var acadPeriod = housingRequestDto.AcademicPeriods.FirstOrDefault();
                if (acadPeriod != null)
                {
                    var acadPeriodCode = (await GetAcademicPeriods()).FirstOrDefault(i => i.Guid.Equals(acadPeriod.Id, StringComparison.OrdinalIgnoreCase));
                    if (acadPeriodCode == null) 
                    {
                        throw new KeyNotFoundException(string.Format("No academic period found for guid: {0}", acadPeriod.Id));
                    }
                    housingRequestEntity.Term = acadPeriodCode.Code;
                }
            }

            //Validate all the business rules for the preferrences
            ValidatePreferrences(housingRequestDto);
            if (housingRequestDto.Preferences != null)
            {
                List<Domain.Student.Entities.RoomPreference> roomPrefEntities = new List<Domain.Student.Entities.RoomPreference>();
                foreach (var pref in housingRequestDto.Preferences)
                {
                    Domain.Student.Entities.RoomPreference rmPrefEntity = new Domain.Student.Entities.RoomPreference();
                    if (pref.Building != null && pref.Building.Preferred != null && !string.IsNullOrEmpty(pref.Building.Preferred.Id)) 
                    {
                        var building = (await GetBuildings(true)).FirstOrDefault(b => b.Guid.Equals(pref.Building.Preferred.Id, StringComparison.OrdinalIgnoreCase));
                        if (building == null)
                        {
                            throw new KeyNotFoundException(string.Format("No building found for guid: {0}", pref.Building.Preferred.Id));
                        }
                        rmPrefEntity.Building = building.Code;
                        rmPrefEntity.BuildingReqdFlag = ConvertRequiredDtoToEntity(pref.Building.Required);

                        //site
                        if (!string.IsNullOrEmpty(building.LocationId))
                        {
                            var site = (await GetLocationsAsync(true)).FirstOrDefault(i => i.Code.Equals(building.LocationId));
                            if (site == null)
                            {
                                throw new KeyNotFoundException(string.Format("No site found for guid: {0}", pref.Building.Preferred.Id));
                            }
                            if (pref.Site != null && pref.Site.Preferred != null &&
                                !string.IsNullOrEmpty(pref.Site.Preferred.Id) && !site.Guid.Equals(pref.Site.Preferred.Id, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new ArgumentException(string.Format("Site for guid: {0} is not valid for building guid: {1}", pref.Site.Preferred.Id, pref.Building.Preferred.Id));
                            }
                            rmPrefEntity.Site = site.Code;
                            //Site req flag has no effect in colleague
                            rmPrefEntity.SiteReqdFlag = pref.Site == null ? "No" : ConvertRequiredDtoToEntity(pref.Site.Required);
                        }
                        
                        //room
                        if (pref.Room != null)
                        {
                            var room = (await GetRooms(true)).FirstOrDefault(i => i.Guid.Equals(pref.Room.Preferred.Id));
                            if (room == null)
                            {
                                throw new KeyNotFoundException(string.Format("No room found for guid: {0}", pref.Room.Preferred.Id));
                            }
                            //Check to see if the room is part of the building provided in the payload.
                            if (!room.BuildingCode.Equals(building.Code, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidOperationException(string.Format("Room for guid: {0} is not valid for building guid: {1}", pref.Room.Preferred.Id, pref.Building.Preferred.Id));
                            }
                            rmPrefEntity.Room = room.Code;
                            rmPrefEntity.RoomReqdFlag = ConvertRequiredDtoToEntity(pref.Room.Required);
                        }

                        //wing
                        if (pref.Wing != null)
                        {
                            var wing = (await GetBuildingWings(true)).FirstOrDefault(i => i.Guid.Equals(pref.Wing.Preferred.Id));
                            if (wing == null)
                            {
                                throw new KeyNotFoundException(string.Format("No wing found for guid: {0}", pref.Wing.Preferred.Id));
                            }
                            rmPrefEntity.Wing = wing.Code;
                            rmPrefEntity.WingReqdFlag = ConvertRequiredDtoToEntity(pref.Wing.Required);
                        }

                        //floor
                        if (pref.Floor != null)
                        {                            
                            rmPrefEntity.Floor = pref.Floor.Preferred;
                            rmPrefEntity.FloorReqd = ConvertRequiredDtoToEntity(pref.Floor.Required);
                        }
                        roomPrefEntities.Add(rmPrefEntity);
                    }
                }
                if (roomPrefEntities.Any())
                {
                    housingRequestEntity.RoomPreferences = new List<Domain.Student.Entities.RoomPreference>();
                    housingRequestEntity.RoomPreferences.AddRange(roomPrefEntities);
                }
            }

            //Room Characteristics
            ValidateRoomCharacteristics(housingRequestDto);
            if (housingRequestDto.RoomCharacteristics != null)
            {
                List<Domain.Student.Entities.RoomCharacteristicPreference> roomCharacteristicPreference = new List<Domain.Student.Entities.RoomCharacteristicPreference>();
                foreach (var roomCharacteristic in housingRequestDto.RoomCharacteristics)
                {
                    var roomCharPref = (await GetRoomCharacteristics(true)).FirstOrDefault(i => i.Guid.Equals(roomCharacteristic.Preferred.Id, StringComparison.OrdinalIgnoreCase));
                    if (roomCharPref == null)
                    {
                        throw new KeyNotFoundException(string.Format("No room characteristic found for guid: {0}", roomCharacteristic.Preferred.Id));
                    }
                    roomCharacteristicPreference.Add(new Domain.Student.Entities.RoomCharacteristicPreference()
                    {
                        RoomCharacteristic = roomCharPref.Code,
                        RoomCharacteristicRequired = ConvertRequiredDtoToEntity(roomCharacteristic.Required)
                    });
                }
                if (roomCharacteristicPreference.Any())
                {
                    housingRequestEntity.RoomCharacerstics = roomCharacteristicPreference;
                }
            }

            //Floor Characteristics
            ValidateFloorCharacteristics(housingRequestDto);
            if (housingRequestDto.FloorCharacteristics != null)
            {
                var flrChar = (await GetFloorCharacteristics(true)).FirstOrDefault(i => i.Guid.Equals(housingRequestDto.FloorCharacteristics.Preferred.Id, StringComparison.OrdinalIgnoreCase));
                if (flrChar == null)
                {
                    throw new KeyNotFoundException(string.Format("No floor characteristic found for guid: {0}", housingRequestDto.FloorCharacteristics.Preferred.Id));
                }
                housingRequestEntity.FloorCharacteristic = flrChar.Code;
                housingRequestEntity.FloorCharacteristicReqd = ConvertRequiredDtoToEntity(housingRequestDto.FloorCharacteristics.Required);
            }

            //Roommate Preferences
            ValidateRoommatePreferences(housingRequestDto);
            if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any())
            {
                List<Domain.Student.Entities.RoommatePreference> roommatePreferenceEntities = new List<Domain.Student.Entities.RoommatePreference>();
                List<Domain.Student.Entities.RoommateCharacteristicPreference> roommateCharacteristicPreferenceEntities = new List<Domain.Student.Entities.RoommateCharacteristicPreference>();

                foreach (var roommatePreference in housingRequestDto.RoommatePreferences)
                {
                    if (roommatePreference.Roommate != null)
                    {
                        var personKey = await _personRepository.GetPersonIdFromGuidAsync(roommatePreference.Roommate.Preferred.Id);
                        if (string.IsNullOrEmpty(personKey))
                        {
                            throw new KeyNotFoundException(string.Format("No person found for guid: {0}", roommatePreference.Roommate.Preferred.Id));
                        }
                        Domain.Student.Entities.RoommatePreference roomatePrefEntity = new Domain.Student.Entities.RoommatePreference()
                        {
                            RoommateId = personKey,
                            RoommateRequired = ConvertRequiredDtoToEntity(roommatePreference.Roommate.Required)
                        };
                        roommatePreferenceEntities.Add(roomatePrefEntity);
                    }

                    if (roommatePreference.RoommateCharacteristic != null)
                    {
                        var roommateChar = (await GetRoommateCharacteristics(true)).FirstOrDefault(i => i.Guid.Equals(roommatePreference.RoommateCharacteristic.Preferred.Id));
                        if (roommateChar == null)
                        {
                            throw new KeyNotFoundException(string.Format("No roommate characteristic found for guid: {0}", roommatePreference.RoommateCharacteristic.Preferred.Id));
                        }
                        Domain.Student.Entities.RoommateCharacteristicPreference roommateCharacteristicPreferenceEntity = new Domain.Student.Entities.RoommateCharacteristicPreference()
                        {
                            RoommateCharacteristic = roommateChar.Code,
                            RoommateCharacteristicRequired = ConvertRequiredDtoToEntity(roommatePreference.RoommateCharacteristic.Required)
                        };
                        roommateCharacteristicPreferenceEntities.Add(roommateCharacteristicPreferenceEntity);
                    }
                }
                if (roommatePreferenceEntities.Any())
                {
                    housingRequestEntity.RoommatePreferences = new List<Domain.Student.Entities.RoommatePreference>();
                    housingRequestEntity.RoommatePreferences.AddRange(roommatePreferenceEntities);
                }
                if (roommateCharacteristicPreferenceEntities.Any())
                {
                    housingRequestEntity.RoommateCharacteristicPreferences = new List<Domain.Student.Entities.RoommateCharacteristicPreference>();
                    housingRequestEntity.RoommateCharacteristicPreferences.AddRange(roommateCharacteristicPreferenceEntities);
                }
            }
            return housingRequestEntity;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Validate academic periods.
        /// </summary>
        /// <param name="housingRequest"></param>
        private static void ValidateAcademicPeriods(HousingRequest housingRequest)
        {
            if (housingRequest.AcademicPeriods != null && housingRequest.AcademicPeriods.Any(i => string.IsNullOrEmpty(i.Id)))
            {
                throw new InvalidOperationException("The academic period id is required if academic period included.");
            }
            else if (housingRequest.AcademicPeriods != null && housingRequest.AcademicPeriods.Count() > 1)
            {
                throw new InvalidOperationException("More than one academic period is not allowed in PUT/POST.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Validate all preferences fields.
        /// </summary>
        /// <param name="housingRequestDto"></param>
        private static void ValidatePreferrences(HousingRequest housingRequestDto)
        {
            //Building
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Building == null))
            {
                throw new InvalidOperationException("The building is required if room, wing, or floor is included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Building != null && i.Building.Preferred == null))
            {
                throw new InvalidOperationException("The building preferred is required if included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Building != null && i.Building.Preferred != null && string.IsNullOrEmpty(i.Building.Preferred.Id)))
            {
                throw new InvalidOperationException("The building preferred id is required if preferred included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Building != null && i.Building.Required == Dtos.EnumProperties.RequiredPreference.NotSet))
            {
                throw new InvalidOperationException("The building required property is required if included.");
            }

            //Site
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Site != null && i.Site.Preferred == null))
            {
                throw new InvalidOperationException("The site preferred is required if site included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Site != null && i.Site.Preferred != null && string.IsNullOrEmpty(i.Site.Preferred.Id)))
            {
                throw new InvalidOperationException("The site preferred id is required if preferred included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Site != null && i.Site.Required == Dtos.EnumProperties.RequiredPreference.NotSet))
            {
                throw new InvalidOperationException("The site required property is required if included.");
            }

            //Check to make sure the payload doesnt include room along with either wing or floor or both
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Room != null && (i.Floor != null || i.Wing != null)))
            {
                throw new InvalidOperationException("Cannot enter a wing or floor if room has been entered.");
            }

            //Room
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Room != null && i.Room.Preferred == null))
            {
                throw new InvalidOperationException("The room preferred is required if included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Room != null && i.Room.Preferred != null && string.IsNullOrEmpty(i.Room.Preferred.Id)))
            {
                throw new InvalidOperationException("The room preferred id is required if preferred included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Room != null && i.Room.Required == Dtos.EnumProperties.RequiredPreference.NotSet))
            {
                throw new InvalidOperationException("The room required property is required if included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any())
            {
                var buildingRooms = housingRequestDto.Preferences
                    .Where(br => br.Building != null && br.Building.Preferred != null && !string.IsNullOrEmpty(br.Building.Preferred.Id) &&
                                 br.Room != null && br.Room.Preferred != null && !string.IsNullOrEmpty(br.Room.Preferred.Id))
                    .GroupBy(i => new { Bldg = i.Building.Preferred.Id, Room = i.Room.Preferred.Id })
                    .Select(c => new { TotalCount = c.Count() });
                if (buildingRooms.Any(i => i.TotalCount > 1))
                {
                    throw new InvalidOperationException(string.Format("Guid '{0}': You cannot have more than one same room and building preference together.", housingRequestDto.Id));
                }
            }

            //Floor
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Floor != null && string.IsNullOrEmpty(i.Floor.Preferred)))
            {
                throw new InvalidOperationException("The floor preferred  is required if preferred included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Floor != null && i.Floor.Required == Dtos.EnumProperties.RequiredPreference.NotSet))
            {
                throw new InvalidOperationException("The floor required property is required if included.");
            }

            //Wing
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Wing != null && i.Wing.Preferred == null))
            {
                throw new InvalidOperationException("The wing preferred is required if included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Wing != null && i.Wing.Preferred != null && string.IsNullOrEmpty(i.Wing.Preferred.Id)))
            {
                throw new InvalidOperationException("The wing preferred id is required if preferred included.");
            }
            if (housingRequestDto.Preferences != null && housingRequestDto.Preferences.Any(i => i.Wing != null && i.Wing.Required == Dtos.EnumProperties.RequiredPreference.NotSet))
            {
                throw new InvalidOperationException("The wing required property is required if included.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Validate room characteristsics.
        /// </summary>
        /// <param name="housingRequestDto"></param>
        private static void ValidateRoomCharacteristics(HousingRequest housingRequestDto)
        {
            if (housingRequestDto.RoomCharacteristics != null && housingRequestDto.RoomCharacteristics.Any())
            {
                if (housingRequestDto.RoomCharacteristics != null && housingRequestDto.RoomCharacteristics.Any(i => i.Preferred == null))
                {
                    throw new InvalidOperationException("The room characteristic prefered is required if room characteristics included.");
                }
                if (housingRequestDto.RoomCharacteristics != null && housingRequestDto.RoomCharacteristics.Any(i => i.Preferred != null && string.IsNullOrEmpty(i.Preferred.Id)))
                {
                    throw new InvalidOperationException("The room characteristic preferred id is required if preferred included.");
                }
                if (housingRequestDto.RoomCharacteristics != null && housingRequestDto.RoomCharacteristics.Any(i => i.Required == RequiredPreference.NotSet))
                {
                    throw new InvalidOperationException("The room characteristic required property is required if included.");
                }
                if (housingRequestDto.RoomCharacteristics != null && housingRequestDto.RoomCharacteristics.Any(i => i.Preferred != null))
                {
                    var count = housingRequestDto.RoomCharacteristics.GroupBy(i => i.Preferred.Id).Select(i => new { TotalCount = i.Count() }).ToList();
                    if (count.Any(i => i.TotalCount > 1))
                    {
                        throw new InvalidOperationException(string.Format("Guid '{0}': You cannot have same room characteristics preference more than once.", housingRequestDto.Id));
                    }
                }
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Validate floor characteristics.
        /// </summary>
        /// <param name="housingRequest"></param>
        private static void ValidateFloorCharacteristics(HousingRequest housingRequest)
        {
            if (housingRequest.FloorCharacteristics != null)
            {
                if (housingRequest.FloorCharacteristics.Preferred == null)
                {
                    throw new InvalidOperationException("The floor characteristic prefered is required if floor characteristics included.");
                }
                if (housingRequest.FloorCharacteristics.Preferred != null && string.IsNullOrEmpty(housingRequest.FloorCharacteristics.Preferred.Id))
                {
                    throw new InvalidOperationException("The floor characteristic preferred id is required if preferred included.");
                }
                if (housingRequest.FloorCharacteristics != null && housingRequest.FloorCharacteristics.Required == RequiredPreference.NotSet)
                {
                    throw new InvalidOperationException("The floor characteristic required property is required if included.");
                }
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Validate roommate preferences.
        /// </summary>
        /// <param name="housingRequestDto"></param>
        private static void ValidateRoommatePreferences(HousingRequest housingRequestDto)
        {
            if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any())
            {
                //Roommate Preferences
                if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any(i => i.Roommate != null && i.Roommate.Preferred == null))
                {
                    throw new InvalidOperationException("The roommate prefered is required if roommate included.");
                }
                if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any(i => i.Roommate != null &&
                    i.Roommate.Preferred != null && string.IsNullOrEmpty(i.Roommate.Preferred.Id)))
                {
                    throw new InvalidOperationException("The roommate preferred id is required if preferred included.");
                }
                if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any(i => i.Roommate != null && i.Roommate.Required == RequiredPreference.NotSet))
                {
                    throw new InvalidOperationException("The roommate required property is required if included.");
                }
                if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any(i => i.Roommate != null &&
                    i.Roommate.Preferred != null))
                {
                    var count = housingRequestDto.RoommatePreferences
                        .Where(rm => rm.Roommate != null && rm.Roommate.Preferred != null && !string.IsNullOrEmpty(rm.Roommate.Preferred.Id))
                        .GroupBy(i => i.Roommate.Preferred.Id).Select(c => new { TotalCount = c.Count() }).ToList();
                    if (count.Any(i => i.TotalCount > 1))
                    {
                        throw new InvalidOperationException(string.Format("Guid '{0}': You cannot have same roommate preference more than once.", housingRequestDto.Id));
                    }
                }

                //Roommate Characteristics
                if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any(i => i.RoommateCharacteristic != null && i.RoommateCharacteristic.Preferred == null))
                {
                    throw new InvalidOperationException("The roommate characteristic prefered is required if roommate characteristic included.");
                }
                if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any(i => i.RoommateCharacteristic != null &&
                    i.RoommateCharacteristic.Preferred != null && string.IsNullOrEmpty(i.RoommateCharacteristic.Preferred.Id)))
                {
                    throw new InvalidOperationException("The roommate characteristic preferred id is required if preferred included.");
                }
                if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any(i => i.RoommateCharacteristic != null && i.RoommateCharacteristic.Required == RequiredPreference.NotSet))
                {
                    throw new InvalidOperationException("The roommate characteristic required property is required if included.");
                }

                if (housingRequestDto.RoommatePreferences != null && housingRequestDto.RoommatePreferences.Any(i => i.RoommateCharacteristic != null &&
                    i.RoommateCharacteristic.Preferred != null))
                {
                    var count = housingRequestDto.RoommatePreferences
                        .Where(rm => rm.RoommateCharacteristic != null && rm.RoommateCharacteristic.Preferred != null && !string.IsNullOrEmpty(rm.RoommateCharacteristic.Preferred.Id))
                        .GroupBy(i => i.RoommateCharacteristic.Preferred.Id).Select(c => new { TotalCount = c.Count() }).ToList();
                    if (count.Any(i => i.TotalCount > 1))
                    {
                        throw new InvalidOperationException(string.Format("Guid '{0}': You cannot have same roommate characteristics preference more than once.", housingRequestDto.Id));
                    }
                }
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert dto value for PUT/POST.
        /// </summary>
        /// <param name="requiredPreference"></param>
        /// <returns></returns>
        private string ConvertRequiredDtoToEntity(RequiredPreference requiredPreference)
        {
            switch (requiredPreference)
            {
                case RequiredPreference.Mandatory:
                    return "Y";
                case RequiredPreference.Optional:
                    return "N";
                default:
                    return string.Empty;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts dto enum for PUT/POST.
        /// </summary>
        /// <param name="housingRequestsStatus"></param>
        /// <returns></returns>
        private string ConvertStatusDtoToEntity(HousingRequestsStatus housingRequestsStatus)
        {
            switch (housingRequestsStatus)
            {
                case HousingRequestsStatus.Submitted:
                    return "submitted";
                case HousingRequestsStatus.Approved:
                    throw new InvalidOperationException("Approved status is not permitted on a PUT/POST.");
                case HousingRequestsStatus.Rejected:
                    return "rejected";
                case HousingRequestsStatus.Withdrawn:
                    return "withdrawn";
                default:
                    throw new ArgumentException("Status is required.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts entities to dtos
        /// </summary>
        /// <param name="housingRequestsEntities"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Ellucian.Colleague.Dtos.HousingRequest>> ConvertHousingRequestsEntitiesToDtos(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.HousingRequest> housingRequestsEntities, bool bypassCache)
        {
            var housingRequestsCollection = new List<Ellucian.Colleague.Dtos.HousingRequest>();
            BuildLocalPersonGuids(housingRequestsEntities);

            foreach (var housingRequests in housingRequestsEntities)
            {
                housingRequestsCollection.Add(await ConvertHousingRequestsEntityToDto(housingRequests, bypassCache));
            }
            return housingRequestsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a HousingRequests domain entity to its corresponding HousingRequests DTO
        /// </summary>
        /// <param name="source">HousingRequests domain entity</param>
        /// <returns>HousingRequests DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.HousingRequest> ConvertHousingRequestsEntityToDto(Ellucian.Colleague.Domain.Student.Entities.HousingRequest source, bool bypassCache)
        {
            var housingRequest = new Ellucian.Colleague.Dtos.HousingRequest();

            housingRequest.Id = source.Guid;

            if (string.IsNullOrEmpty(source.Status))
            {
                throw new InvalidOperationException(string.Format("Status is required. Id: {0}", source.Guid));
            }

            housingRequest.Status = ConvertHousingRequestsStatusDomainEnumToHousingRequestsStatusDtoEnum(source.Status);

            if (!source.StartDate.HasValue)
            {
                throw new InvalidOperationException(string.Format("Start date is required. Id: {0}", source.Guid));
            }
            housingRequest.StartOn = source.StartDate.HasValue? source.StartDate.Value : default(DateTimeOffset?);
            housingRequest.EndOn = source.EndDate.HasValue ? source.EndDate.Value : default(DateTimeOffset?);
            long priorityNumber = 0;
            if (source.LotteryNo.HasValue && long.TryParse(source.LotteryNo.Value.ToString(), out priorityNumber))
            {
                housingRequest.PriorityNumber = priorityNumber;
            }

            if (string.IsNullOrEmpty(source.PersonId))
            {
                throw new InvalidOperationException(string.Format("Student id is required. Id: {0}", source.Guid));
            }

            //student
            var person = (await (GetPersonGuidsAsync())).FirstOrDefault(i => i.Key.Equals(source.PersonId, StringComparison.OrdinalIgnoreCase));
            if (person.Equals(default(KeyValuePair<string, string>)))
            {
                var error = string.Format("No person found for key {0}. ", source.PersonId);
                throw new KeyNotFoundException(error);
            }
            housingRequest.Person = new GuidObject2(person.Value);

            if (!string.IsNullOrEmpty(source.Term))
            {
                var acadPeriods = (await GetAcademicPeriods()).Where(i => i.Code.Equals(source.Term, StringComparison.OrdinalIgnoreCase)).ToList();
                if (acadPeriods == null || !acadPeriods.Any())
                {
                    throw new KeyNotFoundException(string.Format("No academic periods were found for term {0}", source.Term));
                }
                housingRequest.AcademicPeriods = new List<GuidObject2>();
                acadPeriods.ToList().ForEach(i => 
                {
                    var acadPeriod = new GuidObject2(i.Guid);
                    housingRequest.AcademicPeriods.Add(acadPeriod);
                });
            }
            //preferences
            if (source.RoomPreferences != null && source.RoomPreferences.Any())
            {
                housingRequest.Preferences = await ConvertRoomPreferencesEntitiesToDto(source.RoomPreferences, bypassCache);
            }
            //room characteristic
            if (source.RoomCharacerstics != null && source.RoomCharacerstics.Any())
            {
                housingRequest.RoomCharacteristics = await ConvertRoomCharacteristicEntitiesToDto(source.RoomCharacerstics, bypassCache);
            }
            //Floor characteristic
            if (!string.IsNullOrEmpty(source.FloorCharacteristic))
            {
                housingRequest.FloorCharacteristics = await ConvertFloorCharacteristEntityToDto(source, bypassCache);
            }
            //roommate characteristics & preferences
            if ((source.RoommatePreferences != null && source.RoommatePreferences.Any()) ||
                (source.RoommateCharacteristicPreferences != null && source.RoommateCharacteristicPreferences.Any()))
            {
                housingRequest.RoommatePreferences = await ConvertRoommatePreferencesEntitiesToDto(source, bypassCache);
            }
            return housingRequest;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a HousingRequestsStatus domain enumeration value to its corresponding HousingRequestsStatus DTO enumeration value
        /// </summary>
        /// <param name="source">HousingRequestsStatus domain enumeration value</param>
        /// <returns>HousingRequestsStatus DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.HousingRequestsStatus ConvertHousingRequestsStatusDomainEnumToHousingRequestsStatusDtoEnum(string source)
        {
            switch (source)
            {
                case "R":
                    return Dtos.EnumProperties.HousingRequestsStatus.Submitted;
                case "A":
                    return Dtos.EnumProperties.HousingRequestsStatus.Approved;
                case "C":
                    return Dtos.EnumProperties.HousingRequestsStatus.Rejected;
                case "T":
                    return Dtos.EnumProperties.HousingRequestsStatus.Withdrawn;
                default:
                    return Dtos.EnumProperties.HousingRequestsStatus.Submitted;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert room pref entities to dto
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Dtos.DtoProperties.HousingRequestPreferenceProperty>> ConvertRoomPreferencesEntitiesToDto(IEnumerable<Domain.Student.Entities.RoomPreference> sources, bool bypassCache)
        {
            List<Dtos.DtoProperties.HousingRequestPreferenceProperty> preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>();

            foreach (var source in sources)
            {
                Dtos.DtoProperties.HousingRequestPreferenceProperty roomPreference = new Dtos.DtoProperties.HousingRequestPreferenceProperty();
                //roomPreference.RoomPreference = new Dtos.DtoProperties.HousingPreferenceProperty();
                //buildings, site(if building is there then build site based on the code specified in building
                string buildingCode = string.Empty; 
                if (!string.IsNullOrEmpty(source.Building))
                {
                    var building = (await GetBuildings(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.Building, StringComparison.OrdinalIgnoreCase));
                    if (building == null)
                    {
                        throw new KeyNotFoundException(string.Format("No building was found for code {0}", source.Building));
                    }
                    buildingCode = building.Code;
                    roomPreference.Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                    {
                        Preferred = new GuidObject2(building.Guid),
                        Required = GetRequiredPreference(source.BuildingReqdFlag)
                    };
                    //site
                    if (!string.IsNullOrEmpty(building.LocationId))
                    {
                        var site = (await GetLocationsAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(building.LocationId, StringComparison.OrdinalIgnoreCase));
                        if (site == null)
                        {
                            throw new KeyNotFoundException(string.Format("No site was found for code {0}", building.LocationId));
                        }
                        roomPreference.Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                        {
                            Preferred = new GuidObject2(site.Guid),
                            Required = GetRequiredPreference(source.BuildingReqdFlag)
                        };
                    }
                }
                //Room
                if (!string.IsNullOrEmpty(source.Room))
                {
                    var room = (await GetRooms(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.Room) && i.BuildingCode.Equals(buildingCode, StringComparison.OrdinalIgnoreCase));
                    if (room == null)
                    {
                        throw new KeyNotFoundException(string.Format("No room was found for code {0}", source.Wing));
                    }
                    roomPreference.Room = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                    {
                        Preferred = new GuidObject2(room.Guid),
                        Required = GetRequiredPreference(source.RoomReqdFlag)
                    };
                }
                //wing
                if (!string.IsNullOrEmpty(source.Wing))
                {
                    var wing = (await GetBuildingWings(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.Wing));
                    if (wing == null)
                    {
                        throw new KeyNotFoundException(string.Format("No wing was found for code {0}", source.Wing));
                    }
                    roomPreference.Wing = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                    {
                        Preferred = new GuidObject2(wing.Guid),
                        Required = GetRequiredPreference(source.WingReqdFlag)
                    };
                }

                //floor
                if (!string.IsNullOrEmpty(source.Floor))
                {
                    roomPreference.Floor = new Dtos.DtoProperties.HousingFloorPreferenceRequiredProperty()
                    {
                        Preferred = source.Floor,
                        Required = GetRequiredPreference(source.FloorReqd)
                    };
                }
                preferences.Add(roomPreference);
            }
            return preferences.Any() ? preferences : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts to room characterstic dto property
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Dtos.DtoProperties.HousingPreferenceRequiredProperty>> ConvertRoomCharacteristicEntitiesToDto(IEnumerable<Domain.Student.Entities.RoomCharacteristicPreference> sources, bool bypassCache)
        {
            List<Dtos.DtoProperties.HousingPreferenceRequiredProperty> roomCharPrefs = new List<Dtos.DtoProperties.HousingPreferenceRequiredProperty>();
            if (sources != null && sources.Any())
            {
                foreach (var source in sources)
                {
                    Dtos.DtoProperties.HousingPreferenceRequiredProperty rmCharPref = new Dtos.DtoProperties.HousingPreferenceRequiredProperty();

                    var rmPref = (await GetRoomCharacteristics(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.RoomCharacteristic, StringComparison.OrdinalIgnoreCase));
                    if (rmPref == null)
                    {
                        throw new KeyNotFoundException(string.Format("No room characteristic found for code {0}", source.RoomCharacteristic));
                    }
                    rmCharPref.Preferred = new GuidObject2(rmPref.Guid);
                    rmCharPref.Required = GetRequiredPreference(source.RoomCharacteristicRequired);
                    roomCharPrefs.Add(rmCharPref);
                }
            }
            return roomCharPrefs.Any() ? roomCharPrefs : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts to floor characteristic dto property
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<Dtos.DtoProperties.HousingPreferenceRequiredProperty> ConvertFloorCharacteristEntityToDto(Domain.Student.Entities.HousingRequest source, bool bypassCache)
        {
            var flrCharPref = (await GetFloorCharacteristics(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.FloorCharacteristic, StringComparison.OrdinalIgnoreCase));
            if(flrCharPref == null)
            {
                throw new KeyNotFoundException(string.Format("No floor characteristic found for code {0}", source.FloorCharacteristic));
            }
            Dtos.DtoProperties.HousingPreferenceRequiredProperty floorCharacteristic = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
            {
                Preferred = new GuidObject2(flrCharPref.Guid),
                Required = GetRequiredPreference(source.FloorCharacteristicReqd)
            };
            return floorCharacteristic;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert entities to dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty>> ConvertRoommatePreferencesEntitiesToDto(Ellucian.Colleague.Domain.Student.Entities.HousingRequest source, bool bypassCache)
        {
            List<Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty> roommatePreferences = new List<Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty>();

            if (source.RoommatePreferences != null && source.RoommatePreferences.Any())
            {
                foreach (var roommate in source.RoommatePreferences)
                {
                    var person = (await GetPersonGuidsAsync()).FirstOrDefault(i => i.Key.Equals(roommate.RoommateId, StringComparison.OrdinalIgnoreCase));
                    if (person.Equals(default(KeyValuePair<string, string>)))
                    {
                        throw new KeyNotFoundException(string.Format("No roommate was found for roommate id {0}", roommate.RoommateId));
                    }
                    Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty roommatePreference = new Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty();
                    roommatePreference.Roommate = ConvertRoomatePreferencesEntityToDto(person, roommate);                    
                    roommatePreferences.Add(roommatePreference);
                }
            }
            if(source.RoommateCharacteristicPreferences != null && source.RoommateCharacteristicPreferences.Any())
            {
                foreach (var roommateCharacteristicPreference in source.RoommateCharacteristicPreferences)
                {
                    var roommateChar = (await GetRoommateCharacteristics(bypassCache)).FirstOrDefault(i => i.Code.Equals(roommateCharacteristicPreference.RoommateCharacteristic, StringComparison.OrdinalIgnoreCase));
                    if (roommateChar == null)
                    {
                        throw new KeyNotFoundException(string.Format("No roommate characteristic found for code {0}", roommateCharacteristicPreference.RoommateCharacteristic));
                    }
                    Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty roommateCharPref = new Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty();
                    roommateCharPref.RoommateCharacteristic = new Dtos.DtoProperties.HousingPreferenceRequiredProperty() 
                    {
                        Preferred = new GuidObject2(roommateChar.Guid),
                        Required = GetRequiredPreference(roommateCharacteristicPreference.RoommateCharacteristicRequired)
                    };
                    roommatePreferences.Add(roommateCharPref);
                }
            }

            return roommatePreferences.Any() ? roommatePreferences : null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts to roommate preference dto property
        /// </summary>
        /// <param name="person"></param>
        /// <param name="roommate"></param>
        /// <returns></returns>
        private Dtos.DtoProperties.HousingPreferenceRequiredProperty ConvertRoomatePreferencesEntityToDto(KeyValuePair<string, string> person, Domain.Student.Entities.RoommatePreference roommate)
        {
            Dtos.DtoProperties.HousingPreferenceRequiredProperty roommatePref = new Dtos.DtoProperties.HousingPreferenceRequiredProperty();
            if (roommate != null)
            {
                roommatePref.Preferred = new GuidObject2(person.Value);
                roommatePref.Required = GetRequiredPreference(roommate.RoommateRequired);
            }
            return roommatePref;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets required preference
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private RequiredPreference GetRequiredPreference(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                return source.ToUpper().Equals("Y", StringComparison.OrdinalIgnoreCase) ? RequiredPreference.Mandatory : RequiredPreference.Optional;
            }
            return RequiredPreference.Optional;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Checks housing request view permissions
        /// </summary>
        private void CheckViewHousingRequestPermissions()
        {
            // access is ok if the current user has the view housing request
            if (!HasPermission(StudentPermissionCodes.ViewHousingRequest))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view housing-requests.");
                throw new PermissionsException("User is not authorized to view housing-requests.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Checks housing request view permissions
        /// </summary>
        private void CheckCreateUpdateHousingRequestPermissions()
        {
            // access is ok if the current user has the view housing request
            if (!HasPermission(StudentPermissionCodes.CreateHousingRequest))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create or update housing-requests.");
                throw new PermissionsException("User is not authorized to create or update housing-requests.");
            }
        }
    }
}