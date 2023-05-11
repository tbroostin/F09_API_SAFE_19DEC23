//Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

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
            var housingRequestsCollection = new List<Ellucian.Colleague.Dtos.HousingRequest>();
            Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.HousingRequest>, int> housingRequestsData = null;

            try
            {
                housingRequestsData = await _housingRequestRepository.GetHousingRequestsAsync(offset, limit, bypassCache);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }

            if (housingRequestsData != null)
            {
                var housingRequestsEntities = housingRequestsData.Item1;
                if (housingRequestsEntities != null && housingRequestsEntities.Any())
                {
                    housingRequestsCollection = (await ConvertHousingRequestsEntitiesToDtos(housingRequestsData.Item1, bypassCache)).ToList();
                }
            }
            else
            {
                return new Tuple<IEnumerable<Dtos.HousingRequest>, int>(housingRequestsCollection, 0);
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<Dtos.HousingRequest>, int>(housingRequestsCollection, housingRequestsData.Item2);
        }   

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a HousingRequests from its GUID
        /// </summary>
        /// <returns>HousingRequests DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.HousingRequest> GetHousingRequestByGuidAsync(string guid, bool bypassCache = false)
        {
            Dtos.HousingRequest housingRequestDto;
            try
            {
                var housingRequestEntities = new List<Domain.Student.Entities.HousingRequest>();
                var housingRequestEntity = await _housingRequestRepository.GetHousingRequestByGuidAsync(guid);
                housingRequestEntities.Add(housingRequestEntity);
                BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingRequest>() { housingRequestEntity });
                _personGuidsDict = await this._personRepository.GetPersonGuidsCollectionAsync(_personIds);
                housingRequestDto = await ConvertHousingRequestsEntityToDto(housingRequestEntity, bypassCache);                
            }

            catch (RepositoryException ex)
            {
                throw ex;
            }

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return housingRequestDto;
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
                throw;
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
                _housingRequestRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
                
                var housingRequestEntity = await ConvertDtoToEntity(guid, housingRequest);

                Domain.Student.Entities.HousingRequest updatedHousingRequestEntity = await _housingRequestRepository.UpdateHousingRequestAsync(housingRequestEntity);

                BuildLocalPersonGuids(new List<Ellucian.Colleague.Domain.Student.Entities.HousingRequest>() { updatedHousingRequestEntity });
                _personGuidsDict = await this._personRepository.GetPersonGuidsCollectionAsync(_personIds);
                ClearReferenceData();

                var housingRequestDto = await ConvertHousingRequestsEntityToDto(updatedHousingRequestEntity, true);

                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }

                return housingRequestDto;
            }
            catch (Exception e)
            {
                throw;
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
            string recordKey = null;
            try
            {
                recordKey = await GetRecordKey(housingRequestDto);
            }
            catch (Exception ex)
            {
                // Allow an exception if we couldn't find a record.  ConvertDtoToEntity can be called on creation of a new
                // record from Post/Put.
                logger.Info(ex, "Unable to retrieve housing request.");
            }
            Ellucian.Colleague.Domain.Student.Entities.HousingRequest housingRequestEntity = string.IsNullOrEmpty(recordKey) ?
                new Domain.Student.Entities.HousingRequest(housingRequestDto.Id, housingRequestDto.StartOn, status) :
                new Domain.Student.Entities.HousingRequest(housingRequestDto.Id, recordKey, housingRequestDto.StartOn, status);

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
            _personGuidsDict = await this._personRepository.GetPersonGuidsCollectionAsync(_personIds);

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
                IntegrationApiExceptionAddError(string.Format("Status is required."), "Bad.Data", source.Guid, source.RecordKey);
            }

            housingRequest.Status = ConvertHousingRequestsStatusDomainEnumToHousingRequestsStatusDtoEnum(source.Status);

            if (!source.StartDate.HasValue)
            {
                IntegrationApiExceptionAddError(string.Format("Start date is required."), "Bad.Data", source.Guid, source.RecordKey);
            }
            housingRequest.StartOn = source.StartDate.HasValue? source.StartDate.Value : default(DateTimeOffset?);
            housingRequest.EndOn = source.EndDate.HasValue ? source.EndDate.Value : default(DateTimeOffset?);
            long priorityNumber = 0;
            if (source.LotteryNo.HasValue && long.TryParse(source.LotteryNo.Value.ToString(), out priorityNumber))
            {
                housingRequest.PriorityNumber = priorityNumber;
            }

            //person
            if (string.IsNullOrEmpty(source.PersonId))
            {
                IntegrationApiExceptionAddError("Person is required.  ", "Bad.Data", source.Guid, source.RecordKey);
            }
            else
            {
                if (_personGuidsDict == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID : {0}", source.PersonId), "Bad.Data", source.Guid, source.RecordKey);
                }
                else
                {
                    var personGuid = string.Empty;
                    _personGuidsDict.TryGetValue(source.PersonId, out personGuid);
                    if (string.IsNullOrEmpty(personGuid))
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID : {0}", source.PersonId), "Bad.Data", source.Guid, source.RecordKey);
                    }
                    else
                    {
                        housingRequest.Person = new GuidObject2(personGuid);
                    }
                }
            }

            //term
            if (!string.IsNullOrEmpty(source.Term))
            {
                try
                {
                    var acadPeriod = await _termRepository.GetAcademicPeriodsGuidAsync(source.Term);

                    if (!string.IsNullOrEmpty(acadPeriod))
                    {
                        housingRequest.AcademicPeriods = new List<GuidObject2>(){new Dtos.GuidObject2(acadPeriod)};
                    }
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                        source.Guid, source.RecordKey);
                }
            }

            //preferences
            if (source.RoomPreferences != null && source.RoomPreferences.Any())
            {
                housingRequest.Preferences = await ConvertRoomPreferencesEntitiesToDto(source.RoomPreferences, source.Guid, source.RecordKey, bypassCache);
            }
            
            //room characteristic
            if (source.RoomCharacerstics != null && source.RoomCharacerstics.Any())
            {
                housingRequest.RoomCharacteristics = await ConvertRoomCharacteristicEntitiesToDto(source.RoomCharacerstics, source.Guid, source.RecordKey, bypassCache);
            }
            
            //Floor characteristic
            if (!string.IsNullOrEmpty(source.FloorCharacteristic))
            {
                housingRequest.FloorCharacteristics = await ConvertFloorCharacteristEntityToDto(source, source.Guid, source.RecordKey, bypassCache);
            }
            
            //roommate characteristics & preferences
            if ((source.RoommatePreferences != null && source.RoommatePreferences.Any()) ||
                (source.RoommateCharacteristicPreferences != null && source.RoommateCharacteristicPreferences.Any()))
            {
                housingRequest.RoommatePreferences = await ConvertRoommatePreferencesEntitiesToDto(source, source.Guid, source.RecordKey, bypassCache);
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
        private async Task<IEnumerable<Dtos.DtoProperties.HousingRequestPreferenceProperty>> ConvertRoomPreferencesEntitiesToDto(IEnumerable<Domain.Student.Entities.RoomPreference> sources, 
            string guid, string id, bool bypassCache)
        {
            List<Dtos.DtoProperties.HousingRequestPreferenceProperty> preferences = new List<Dtos.DtoProperties.HousingRequestPreferenceProperty>();

            foreach (var source in sources)
            {
                Dtos.DtoProperties.HousingRequestPreferenceProperty roomPreference = new Dtos.DtoProperties.HousingRequestPreferenceProperty();
                
                // building
                string buildingLocation = string.Empty;
                string buildingCode = string.Empty;
                if (!string.IsNullOrEmpty(source.Building))
                {
                    try
                    {
                        var buildings = await GetBuildings(bypassCache);
                        if (buildings.Any())
                        {
                            var building = (await GetBuildings(bypassCache)).FirstOrDefault(g => g.Code == source.Building);
                            if (building != null && !string.IsNullOrEmpty(building.Guid))
                            {
                                buildingLocation = building.LocationId;
                                buildingCode = building.Code;

                                roomPreference.Building = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                                {
                                    Preferred = new GuidObject2(building.Guid),
                                    Required = GetRequiredPreference(source.BuildingReqdFlag)
                                };
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Concat("Missing building GUID or invalid code of ", source.Building, "."),
                                    "GUID.Not.Found", guid, id);
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Concat("No buildings found."),
                                "GUID.Not.Found", guid, id);
                        }
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "GUID.Not.Found", guid, id);
                    }

                    //site
                    if (!string.IsNullOrEmpty(buildingLocation))
                    {
                        try
                        {
                            var location = await _referenceDataRepository.GetLocationsGuidAsync(buildingLocation);

                            if (!string.IsNullOrEmpty(location))
                            {
                                roomPreference.Site = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                                {
                                    Preferred = new GuidObject2(location),
                                    Required = GetRequiredPreference(source.BuildingReqdFlag)
                                };
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                guid, id);
                        }
                    }


                    //room
                    if (!string.IsNullOrEmpty(source.Room) && !string.IsNullOrEmpty(buildingCode))
                    {
                        try
                        {
                            //var room = await _roomRepository.GetRoomsGuidAsync(source.Room);
                            var room = (await GetRooms(bypassCache)).FirstOrDefault(i => i.Code.Equals(source.Room) && i.BuildingCode.Equals(buildingCode, StringComparison.OrdinalIgnoreCase));
                            if (room != null)
                            {
                                roomPreference.Room = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                                {
                                    Preferred = new GuidObject2(room.Guid),
                                    Required = GetRequiredPreference(source.RoomReqdFlag)
                                };
                            }
                            else
                            {
                                var message = string.Format("No room was found for room {0} and building {1}", source.Room, buildingCode);
                                IntegrationApiExceptionAddError(message, "GUID.Not.Found", guid, id);
                            }
                        }
                        catch (RepositoryException ex)
                        {
                            IntegrationApiExceptionAddError(ex, "GUID.Not.Found",
                                guid, id);
                        }
                    }
                }
                //wing
                if (!string.IsNullOrEmpty(source.Wing))
                {
                    var wings = await GetBuildingWings(bypassCache);
                    if (wings != null)
                    {
                        var wing = wings.FirstOrDefault(w => w.Code == source.Wing);
                        if (wing != null)
                        {
                            if (!string.IsNullOrEmpty(wing.Guid))
                            {
                                roomPreference.Wing = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                                {
                                    Preferred = new GuidObject2(wing.Guid),
                                    Required = GetRequiredPreference(source.WingReqdFlag)
                                };
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the wing '{0}'", source.Wing), "GUID.Not.Found", guid, id);
                            }                            ;
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to find the wing '{0}'", source.Wing), "Bad.Data", guid, id);
                        }
                    }
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
        private async Task<IEnumerable<Dtos.DtoProperties.HousingPreferenceRequiredProperty>> ConvertRoomCharacteristicEntitiesToDto(IEnumerable<Domain.Student.Entities.RoomCharacteristicPreference> sources,
            string guid, string id, bool bypassCache)
        {
            List<Dtos.DtoProperties.HousingPreferenceRequiredProperty> roomCharPrefs = new List<Dtos.DtoProperties.HousingPreferenceRequiredProperty>();

            if (sources != null && sources.Any())
            {
                foreach (var source in sources)
                {
                    if (!string.IsNullOrEmpty(source.RoomCharacteristic))
                    {
                        var roomChars = await GetRoomCharacteristics(bypassCache);
                        if (roomChars != null)
                        {
                            var roomChar = roomChars.FirstOrDefault(f => f.Code == source.RoomCharacteristic);
                            if (roomChar != null)
                            {
                                if (!string.IsNullOrEmpty(roomChar.Guid))
                                {
                                    var roomCharPref = new Dtos.DtoProperties.HousingPreferenceRequiredProperty();
                                    roomCharPref.Preferred = new GuidObject2(roomChar.Guid);
                                    roomCharPref.Required = GetRequiredPreference(source.RoomCharacteristicRequired);
                                    roomCharPrefs.Add(roomCharPref);
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the room characteristic '{0}'", source.RoomCharacteristic), "GUID.Not.Found", guid, id);
                                }
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Format("Unable to find the room characteristic '{0}'", source.RoomCharacteristic), "Bad.Data", guid, id);
                            }
                        }
                    }
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
        private async Task<Dtos.DtoProperties.HousingPreferenceRequiredProperty> ConvertFloorCharacteristEntityToDto(Domain.Student.Entities.HousingRequest source,
            string guid, string id, bool bypassCache)
        {
            Dtos.DtoProperties.HousingPreferenceRequiredProperty floorCharacteristic = new Dtos.DtoProperties.HousingPreferenceRequiredProperty();
            if (!string.IsNullOrEmpty(source.FloorCharacteristic))
            {
                var floorChars = await GetFloorCharacteristics(bypassCache);
                if (floorChars != null)
                {
                    var floorChar = floorChars.FirstOrDefault(f => f.Code == source.FloorCharacteristic);
                    if (floorChar != null)
                    {
                        if (!string.IsNullOrEmpty(floorChar.Guid))
                        {
                            floorCharacteristic = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                            {
                                Preferred = new GuidObject2(floorChar.Guid),
                                Required = GetRequiredPreference(source.FloorCharacteristicReqd)
                            };
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the floor characteristic '{0}'", source .FloorCharacteristic), "GUID.Not.Found", guid, id);
                        }
                    }
                    else
                    {
                        IntegrationApiExceptionAddError(string.Format("Unable to find the floor characteristic '{0}'", source.FloorCharacteristic), "Bad.Data", guid, id);
                    }
                }
            }
            return floorCharacteristic;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert entities to dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty>> ConvertRoommatePreferencesEntitiesToDto(Ellucian.Colleague.Domain.Student.Entities.HousingRequest source,
            string guid, string id, bool bypassCache)
        {
            List<Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty> roommatePreferences = new List<Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty>();

            if (source.RoommatePreferences != null && source.RoommatePreferences.Any())
            {
                foreach (var roommate in source.RoommatePreferences)
                {
                    if (roommate != null && !string.IsNullOrEmpty(roommate.RoommateId))
                    {
                        if (_personGuidsDict != null)
                        {
                            var personGuid = string.Empty;
                            _personGuidsDict.TryGetValue(roommate.RoommateId, out personGuid);
                            if (string.IsNullOrEmpty(personGuid))
                            {
                                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID : {0}", source.PersonId), "Bad.Data", source.Guid, source.RecordKey);
                            }
                            else
                            {
                                Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty roommatePreference = new Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty();
                                roommatePreference.Roommate = ConvertRoomatePreferencesEntityToDto(personGuid, roommate);
                                roommatePreferences.Add(roommatePreference);
                            }
                        }
                    }
                }
            }
            if(source.RoommateCharacteristicPreferences != null && source.RoommateCharacteristicPreferences.Any())
            {
                foreach (var roommateCharacteristicPreference in source.RoommateCharacteristicPreferences)
                {
                    Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty roommateCharPref = new Dtos.DtoProperties.HousingRequestRoommatePreferenceProperty();
                    if (roommateCharacteristicPreference != null && !string.IsNullOrEmpty(roommateCharacteristicPreference.RoommateCharacteristic))
                    {
                        var roommateChars = await GetRoommateCharacteristics(bypassCache);
                        if (roommateChars != null)
                        {
                            var roommateChar = roommateChars.FirstOrDefault(r => r.Code == roommateCharacteristicPreference.RoommateCharacteristic);
                            if (roommateChar != null)
                            {
                                if (!string.IsNullOrEmpty(roommateChar.Guid))
                                {
                                    roommateCharPref.RoommateCharacteristic = new Dtos.DtoProperties.HousingPreferenceRequiredProperty()
                                    {
                                        Preferred = new GuidObject2(roommateChar.Guid),
                                        Required = GetRequiredPreference(roommateCharacteristicPreference.RoommateCharacteristicRequired)
                                    };
                                }
                                else
                                {
                                    IntegrationApiExceptionAddError(string.Format("Unable to find the GUID for the floor characteristic '{0}'", source.FloorCharacteristic), "GUID.Not.Found", guid, id);
                                }
                            }
                            else
                            {
                                IntegrationApiExceptionAddError(string.Format("Unable to find the floor characteristic '{0}'", source.FloorCharacteristic), "Bad.Data", guid, id);
                            }
                        }
                    }
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
        private Dtos.DtoProperties.HousingPreferenceRequiredProperty ConvertRoomatePreferencesEntityToDto(string person, Domain.Student.Entities.RoommatePreference roommate)
        {
                Dtos.DtoProperties.HousingPreferenceRequiredProperty roommatePref = new Dtos.DtoProperties.HousingPreferenceRequiredProperty();
            if (roommate != null)
            {
                roommatePref.Preferred = new GuidObject2(person);
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
       
    }
}