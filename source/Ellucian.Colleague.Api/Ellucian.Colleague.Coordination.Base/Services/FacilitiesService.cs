// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using System.Diagnostics;
using Ellucian.Colleague.Domain.Exceptions;
using PersonRoleType = Ellucian.Colleague.Domain.Base.Entities.PersonRoleType;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class FacilitiesService : BaseCoordinationService, IFacilitiesService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger repoLogger;
        private const string _dataOrigin = "Colleague";

        private IEnumerable<Domain.Base.Entities.Building> _buildingEntities = null;
        private IEnumerable<Site2> _site2Dtos = null;
        private IEnumerable<Domain.Base.Entities.RoomTypes> _roomTypesEntities = null;
        private IEnumerable<Domain.Base.Entities.RoomWing> _roomWingEntities = null;
        private IEnumerable<Domain.Base.Entities.RoomCharacteristic> _roomCharacteristicsEntities = null;
        private IEnumerable<Domain.Base.Entities.Location> _locationEntities = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacilitiesService"/> class.
        /// </summary>
        /// <param name="referenceDataRepository">The reference data repository.</param>
        /// <param name="configurationRepository">The configuration repository.</param>
        /// <param name="roomRepository">The room repository.</param>
        /// <param name="eventRepository">The event repository.</param>
        /// <param name="personRepository">Person Repo</param>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        public FacilitiesService(IReferenceDataRepository referenceDataRepository, IConfigurationRepository configurationRepository,
            IRoomRepository roomRepository, IEventRepository eventRepository, IPersonRepository personRepository,
            IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _configurationRepository = configurationRepository;
            _roomRepository = roomRepository;
            _eventRepository = eventRepository;

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.repoLogger = logger;
        }

        /// <summary>
        /// Gets building entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>Domain.Base.Entities.Building collection</returns>
        private async Task<IEnumerable<Domain.Base.Entities.Building>> GetBuildingsAsync(bool bypassCache)
        {
            if (_buildingEntities == null)
            {
                _buildingEntities = await _referenceDataRepository.GetBuildingsAsync(bypassCache);
            }
            return _buildingEntities;
        }

        /// <summary>
        /// Gets RoomType entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>Domain.Base.Entities.RoomTypes collection</returns>
        private async Task<IEnumerable<Domain.Base.Entities.RoomTypes>> GetRoomTypesAsync(bool bypassCache)
        {
            if (_roomTypesEntities == null)
            {
                _roomTypesEntities = await _referenceDataRepository.GetRoomTypesAsync(bypassCache);
            }
            return _roomTypesEntities;
        }

        /// <summary>
        /// Gets RoomWing entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>Domain.Base.Entities.RoomWing collection</returns>
        private async Task<IEnumerable<Domain.Base.Entities.RoomWing>> GetRoomWingsAsync(bool bypassCache)
        {
            if (_roomWingEntities == null)
            {
                _roomWingEntities = await _referenceDataRepository.GetRoomWingsAsync(bypassCache);
            }
            return _roomWingEntities;
        }

        /// <summary>
        /// Gets RoomCharacteristic entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>Domain.Base.Entities.RoomCharacteristic collection</returns>
        private async Task<IEnumerable<Domain.Base.Entities.RoomCharacteristic>> GetRoomCharacteristicsAsync(bool bypassCache)
        {
            if (_roomCharacteristicsEntities == null)
            {
                _roomCharacteristicsEntities = await _referenceDataRepository.GetRoomCharacteristicsAsync(bypassCache);
            }
            return _roomCharacteristicsEntities;
        }

        /// <summary>
        /// Gets Location entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>Domain.Base.Entities.Location collection</returns>
        private async Task<IEnumerable<Domain.Base.Entities.Location>> GetLocationsAsync(bool bypassCache)
        {
            if (_locationEntities == null)
            {
                _locationEntities = await _referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _locationEntities;
        }

        /// <summary>
        /// convert credentials from person
        /// </summary>
        /// <param name="person"></param>
        /// <returns>Credentials</returns>
        private async Task<IEnumerable<Dtos.DtoProperties.CredentialDtoProperty>> GetPersonCredentials(Domain.Base.Entities.PersonIntegration person)
        {
            // Colleague Person ID
            var credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
            {
                new Dtos.DtoProperties.CredentialDtoProperty()
                {
                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                    Value = person.Id
                }
            };
            // Elevate ID
            if (person.PersonAltIds != null && person.PersonAltIds.Count() > 0)
            {
                //Produce an error if there are more than one elevate id's, it means bad data
                if (person.PersonAltIds.Count(altId => altId.Type.Equals("ELEV", StringComparison.OrdinalIgnoreCase)) > 1)
                {
                    throw new InvalidOperationException("You cannot have more than one elevate id.");
                }
                var elevPersonAltId = person.PersonAltIds.FirstOrDefault(a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                {
                    credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty()
                    {
                        Type = Dtos.EnumProperties.CredentialType.ElevateID,
                        Value = elevPersonAltId.Id
                    });
                }
            }
            // SSN
            if (!string.IsNullOrEmpty(person.GovernmentId))
            {
                var type = Dtos.EnumProperties.CredentialType.Sin;
                var countryCode = await _personRepository.GetHostCountryAsync();
                if (countryCode.Equals("USA", StringComparison.OrdinalIgnoreCase))
                {
                    type = Dtos.EnumProperties.CredentialType.Ssn;
                }
                credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty()
                {
                    Type = type,
                    Value = person.GovernmentId
                });
            }
            return credentials;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all sites
        /// </summary>
        /// <returns>Collection of Site DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Site2>> GetSites2Async(bool bypassCache)
        {
            if (_site2Dtos == null)
            {
                var siteCollection = new List<Ellucian.Colleague.Dtos.Site2>();

                var siteEntities = await GetLocationsAsync(bypassCache);
                if (siteEntities != null && siteEntities.Any())
                {
                    foreach (var site in siteEntities)
                    {
                        siteCollection.Add(ConvertLocationEntityToSiteDto2(site));
                    }
                }
                _site2Dtos = siteCollection;
            }
            return _site2Dtos;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get a site from its ID
        /// </summary>
        /// <returns>Site DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Site2> GetSite2Async(string id)
        {
            try
            {
                return ConvertLocationEntityToSiteDto2((await _referenceDataRepository.GetLocationsAsync(true)).First(s => s.Guid == id));
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Site not found for ID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Gets all buildings
        /// </summary>
        /// <returns>Collection of Building DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Building3>> GetBuildings3Async(bool bypassCache, string mapVisibility)
        {
            var buildingCollection = new List<Ellucian.Colleague.Dtos.Building3>();

            var buildingEntities = await _referenceDataRepository.GetBuildings2Async(bypassCache);
            if (buildingEntities != null && buildingEntities.Count() > 0)
            {
                foreach (var building in buildingEntities)
                {
                    bool skipBuilding = false;
                    if (!string.IsNullOrEmpty(mapVisibility))
                    {
                        var visibleForMobile = BuildingMapVisibility.NotVisible;
                        if (!string.IsNullOrEmpty(building.ExportToMobile) && building.ExportToMobile.Equals("Y"))
                        {
                            visibleForMobile = BuildingMapVisibility.Visible;
                        }
                        if (mapVisibility.ToLower() != visibleForMobile.ToString().ToLower())
                        {
                            skipBuilding = true;
                        }
                    }
                    if (skipBuilding == false)
                    {
                        buildingCollection.Add(await ConvertBuildingEntityToDto3(building, bypassCache));
                    }
                }
            }
            return buildingCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Gets all buildings
        /// </summary>
        /// <returns>Collection of Building DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Building2>> GetBuildings2Async(bool bypassCache)
        {
            var buildingCollection = new List<Ellucian.Colleague.Dtos.Building2>();

            var buildingEntities = await _referenceDataRepository.GetBuildingsAsync(bypassCache);
            if (buildingEntities != null && buildingEntities.Count() > 0)
            {
                foreach (var building in buildingEntities)
                {
                    buildingCollection.Add(ConvertBuildingEntityToDto2(building));
                }
            }
            return buildingCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Get a building from its GUID
        /// </summary>
        /// <returns>Building DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Building2> GetBuilding2Async(string guid)
        {
            try
            {
                return ConvertBuildingEntityToDto2((await _referenceDataRepository.GetBuildingsAsync(true)).Where(b => b.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Building not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Get a building from its GUID
        /// </summary>
        /// <returns>Building DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Building3> GetBuilding3Async(string guid)
        {
            try
            {
                bool bypassCache = false;
                return await ConvertBuildingEntityToDto3((await _referenceDataRepository.GetBuildings2Async(true)).Where(b => b.Guid == guid).First(), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Building not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Building not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all building wings
        /// </summary>
        /// <returns>Collection of BuildingWing DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.BuildingWing>> GetBuildingWingsAsync(bool bypassCache)
        {
            var buildingCollection = new List<Ellucian.Colleague.Dtos.BuildingWing>();

            var buildingWingsEntities = await _referenceDataRepository.GetRoomWingsAsync(bypassCache);
            if (buildingWingsEntities != null && buildingWingsEntities.Any())
            {
                foreach (var buildingWing in buildingWingsEntities)
                {
                    buildingCollection.Add(ConvertRoomWingsEntityToBuildingWingsDto(buildingWing));
                }
            }
            return buildingCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a building wing from its GUID
        /// </summary>
        /// <returns>BuildingWing DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.BuildingWing> GetBuildingWingsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new NullReferenceException("Guid is a required field.");
            }
            try
            {
                return ConvertRoomWingsEntityToBuildingWingsDto((await _referenceDataRepository.GetRoomWingsAsync(true)).First(b => b.Guid == guid));
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Building Wing not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all rooms
        /// </summary>
        /// <returns>Collection of Room DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Room>> GetRoomsAsync(bool bypassCache)
        {
            var roomCollection = new List<Ellucian.Colleague.Dtos.Room>();

            var roomEntities = await _roomRepository.GetRoomsAsync(bypassCache);
            if (roomEntities != null && roomEntities.Count() > 0)
            {
                foreach (var room in roomEntities)
                {
                    roomCollection.Add(await ConvertRoomEntityToDtoAsync(room));
                }
            }
            return roomCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all rooms
        /// </summary>
        /// <returns>Collection of Room DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.Room3>, int>> GetRooms3Async(int offset, int limit, bool bypassCache)
        {
            var roomCollection = new List<Ellucian.Colleague.Dtos.Room3>();

            var roomEntities = await _roomRepository.GetRoomsWithPagingAsync(offset, limit, bypassCache);
            if (roomEntities != null && roomEntities.Item1.Any())
            {
                foreach (var room in roomEntities.Item1)
                {
                    roomCollection.Add(await ConvertRoomEntityToDto3Async(room, bypassCache));
                }
            }
            return (roomCollection.Any()) ?
                    new Tuple<IEnumerable<Dtos.Room3>, int>(roomCollection, roomEntities.Item2) :
                    new Tuple<IEnumerable<Dtos.Room3>, int>(roomCollection, 0);
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all rooms
        /// </summary>
        /// <returns>Collection of Room DTO objects</returns>
        public async Task<Tuple<IEnumerable<Dtos.Room3>, int>> GetRooms4Async(int offset, int limit, bool bypassCache, string building = "", string roomType = "")
        {
            var roomCollection = new List<Ellucian.Colleague.Dtos.Room3>();
            var newBuilding = string.Empty;
            var newRoomType = string.Empty;

            if (!string.IsNullOrEmpty(building))
            {
                try
                {
                    newBuilding = ConvertGuidToCode(await GetBuildingsAsync(bypassCache), building);
                    if (string.IsNullOrEmpty(newBuilding))
                        return new Tuple<IEnumerable<Dtos.Room3>, int>(roomCollection, 0);
                    //throw new ArgumentException("Invalid Building '" + building + "' in the arguments");
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.Room3>, int>(roomCollection, 0);
                }
            }
            if (!string.IsNullOrEmpty(roomType))
            {
                try
                {
                    newRoomType = await ConvertRoomTypeToCode(roomType, bypassCache);
                    if (string.IsNullOrEmpty(newRoomType))
                        return new Tuple<IEnumerable<Dtos.Room3>, int>(roomCollection, 0);
                }
                catch
                {
                    return new Tuple<IEnumerable<Dtos.Room3>, int>(roomCollection, 0);
                }
            }

            var roomEntities = await _roomRepository.GetRoomsWithPaging2Async(offset, limit, bypassCache, newBuilding, newRoomType);
            if (roomEntities != null && roomEntities.Item1.Any())
            {
                foreach (var room in roomEntities.Item1)
                {
                    roomCollection.Add(await ConvertRoomEntityToDto3Async(room, bypassCache));
                }
            }
            return (roomCollection.Any()) ?
                    new Tuple<IEnumerable<Dtos.Room3>, int>(roomCollection, roomEntities.Item2) :
                    new Tuple<IEnumerable<Dtos.Room3>, int>(roomCollection, 0);
        }

        /// <returns>roomTypesCodes</returns>
        private async Task<string> ConvertRoomTypeToCode(string category, bool bypassCache = false)
        {
            var roomTypeCode = string.Empty;
            if (!string.IsNullOrEmpty(category))
            {
                //var roomType = (await GetRoomTypesAsync(bypassCache)).Where(rt => rt.Type.ToString().Equals(category, StringComparison.OrdinalIgnoreCase));
                var roomType = (await GetRoomTypesAsync(bypassCache)).Where(rt => !category.Equals("other", StringComparison.OrdinalIgnoreCase) ? 
                    rt.Type.ToString().Equals(category, StringComparison.OrdinalIgnoreCase) : 
                    rt.Type.ToString().Equals(category, StringComparison.OrdinalIgnoreCase) || rt.Type == null);
                if (roomType.Any())
                {
                    foreach (var stat in roomType)
                    {
                        roomTypeCode += "'" + stat.Code + "' ";
                        //roomTypeCode += stat.Code;
                    }
                }
                else
                    throw new ArgumentException("Invalid roomTypes.type of '" + category + "' in the arguments.");

            }
            return roomTypeCode;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a room from its ID
        /// </summary>
        /// <returns>Room DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Room3> GetRoomById3Async(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new NullReferenceException("Guid is required");

            }
            try
            {
                return await ConvertRoomEntityToDto3Async((await _roomRepository.GetRoomsAsync(true)).First(r => r.Guid == id), true);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Room not found for ID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Check for room availability for a given date range, start and end time, and frequency
        /// </summary>
        /// <param name="request">Room availability request</param>
        /// <returns>RoomAvailabilityResponse DTO object</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Room2>> CheckRoomAvailability2Async(Dtos.RoomsAvailabilityRequest2 request)
        {
            var availableRooms = (await GetAvailableRoomsAsync(request)).ToList();

            var roomsWithAvailability = new List<Dtos.Room2>();
            if (availableRooms.Count == 0)
            {
                roomsWithAvailability = new List<Dtos.Room2>();
                return roomsWithAvailability;
            }

            foreach (var room in availableRooms)
            {
                roomsWithAvailability.Add((await ConvertRoomEntityToDto2Async(room)));
            }

            return roomsWithAvailability;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Check for room availability for a given date range, start and end time, and frequency
        /// </summary>
        /// <param name="request">Room availability request</param>
        /// <returns>RoomAvailabilityResponse DTO object</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Room2>> CheckRoomAvailability3Async(Dtos.RoomsAvailabilityRequest2 request)
        {
            var roomsWithAvailability = new List<Dtos.Room2>();

            try
            {
                var availableRooms = (await GetAvailableRooms2Async(request)).ToList();

                if (availableRooms.Count == 0)
                {
                    return roomsWithAvailability;
                }

                foreach (var room in availableRooms)
                {
                    var convertedRoom = (await ConvertRoomEntityToDto2Async(room));
                    if (convertedRoom != null && convertedRoom.RoomTypes == null)
                        convertedRoom.RoomTypes = request.RoomType;
                    roomsWithAvailability.Add(convertedRoom);
                }
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            return roomsWithAvailability;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Check for room availability for a given date range, start and end time, and frequency
        /// </summary>
        /// <param name="request">Room availability request</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>RoomAvailabilityResponse DTO object</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Room3>> CheckRoomAvailability4Async(Dtos.RoomsAvailabilityRequest3 request, bool bypassCache = false)
        {
            var roomsWithAvailability = new List<Dtos.Room3>();

            try
            {
                var availableRooms = (await GetAvailableRooms3Async(request, bypassCache)).ToList();

                if (availableRooms.Count == 0)
                {
                    return roomsWithAvailability;
                }

                foreach (var room in availableRooms)
                {
                    var convertedRoom = (await ConvertRoomEntityToDto3Async(room));
                    if (convertedRoom != null && convertedRoom.RoomTypes == null)
                        convertedRoom.RoomTypes = request.RoomType;
                    roomsWithAvailability.Add(convertedRoom);
                }
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            catch (IntegrationApiException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            return roomsWithAvailability;
        }
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an RoomsMinimumResponse
        /// </summary>
        /// <returns>RoomsMinimumResponse DTO object</returns>
        public async Task<IEnumerable<Dtos.RoomsMinimumResponse>> GetRoomsMinimumAsync(RoomsAvailabilityRequest2 request)
        {

            var availableRooms = (await GetAvailableRoomsAsync(request)).ToList();

            var roomsWithAvailability = new List<Dtos.RoomsMinimumResponse>();
            if (availableRooms.Count == 0)
            {
                roomsWithAvailability = new List<Dtos.RoomsMinimumResponse>();
                return roomsWithAvailability;
            }

            foreach (var room in availableRooms)
            {
                roomsWithAvailability.Add((await ConvertRoomEntityToRoomsMinimimDtoAsync(room)));
            }

            return roomsWithAvailability;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an list of available rooms
        /// </summary>
        /// <returns>Room Entity object</returns>
        private async Task<IEnumerable<Domain.Base.Entities.Room>> GetAvailableRoomsAsync(RoomsAvailabilityRequest2 request)
        {
            if (request == null)
            {
                // Integration API error InstructionalEvent.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                {
                    Code = "Global.SchemaValidation.Error",
                    Message = "Unable to parse request."
                });
                throw ex;
            }

            // 1. Validate the request data
            ValidateRoomAvailabilityRequestData2(request);

            // 2. Get all rooms and filter based on site, location and/or roomtype
            var allRoomsAsync = await _roomRepository.RoomsAsync();

            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                logger.Info("Facilities Timing: (CheckRoomAvailability) _FilterRoomsByRoomsAvailabilityRequest started");
                watch.Start();
            }

            var filteredRoomsList = await FilterRoomsByRoomsAvailabilityRequestAsync(request, allRoomsAsync);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info(
                    "Facilities Timing: (CheckRoomAvailability) _FilterRoomsByRoomsAvailabilityRequest completed in " +
                    watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // 3. Identify Rooms with sufficient capacity for each occupancy
            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) Identify rooms with capacity");
                watch.Restart();
            }
            var maxOccupancy = request.Occupancies.Max(o => o.MaximumOccupancy);
            var roomsWithCapacity = RoomAvailabilityService.GetRoomsWithCapacity(filteredRoomsList, maxOccupancy);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("Facilities Timing: (CheckRoomAvailability) Identify rooms with capacity completed in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }

            if (roomsWithCapacity == null || !roomsWithCapacity.Any())
            {
                // No rooms with sufficient capacity
                var ex = new IntegrationApiException("Insufficient Room Capacity");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                {
                    Code = "RoomsAvailabilityRequest.InsufficientRoomCapacity",
                    Message = "No rooms available with a capacity of at least " + maxOccupancy.ToString()
                });
                throw ex;
            }

            // 4. Build list of dates on which rooms must be available
            var campusCalendarId = _configurationRepository.GetDefaultsConfiguration().CampusCalendarId;
            var campusCalendar = _eventRepository.GetCalendar(campusCalendarId);
            var frequency = ConvertFrequencyType2EnumDtoToFrequencyTypeDomainEnum(request.Recurrence.RepeatRule.Type);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) _GetRoomAvailabilityMeetingDates starting");
                watch.Restart();
            }

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(request.Recurrence.RepeatRule,
                request.Recurrence.TimePeriod, frequency, campusCalendar);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info(
                    "Facilities Timing: (CheckRoomAvailability) _GetRoomAvailabilityMeetingDates completed in " +
                    watch.ElapsedMilliseconds.ToString() + " ms");
            }

            if (meetingDates == null || !meetingDates.Any())
            {
                // No meeting dates for the supplied criteria
                var ex = new IntegrationApiException("No Meeting Dates");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                {
                    Code = "RoomsAvailabilityRequest.NoMeetingDates",
                    Message = "No meeting dates were identified for "
                              + request.Recurrence.TimePeriod.StartOn.Value.Date.ToString()
                              + " to " + request.Recurrence.TimePeriod.EndOn.Value.Date.ToString()
                              + " between " + request.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().ToString()
                              + " and " + request.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().ToString()
                              + " recurring " + frequency.ToString()
                });
                throw ex;
            }

            // 5. Get IDs of rooms with schedule conflicts based on meeting dates and times
            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) _GetRoomIdsWithConflicts starting");
                watch.Restart();
            }

            var roomsWithCapacityForLookup = roomsWithCapacity.Select(r => r.Id).Distinct().ToArray();
            var roomIdsWithConflicts = _eventRepository.GetRoomIdsWithConflicts2(request.Recurrence.TimePeriod.StartOn.Value,
                request.Recurrence.TimePeriod.EndOn.Value, meetingDates, roomsWithCapacityForLookup);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("Facilities Timing: (CheckRoomAvailability) _GetRoomIdsWithConflicts completed in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // 6. Eliminate rooms with conflicts from list of rooms with capacity
            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) Eliminate rooms with conflicts starting");
                watch.Restart();
            }

            var availableRooms = roomsWithCapacity.ToList();
            if (roomIdsWithConflicts != null && roomIdsWithConflicts.Any())
            {
                availableRooms = roomsWithCapacity.Where(r => !roomIdsWithConflicts.Contains(r.Id)).ToList();
            }

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("Facilities Timing: (CheckRoomAvailability) Eliminate rooms with conflicts completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            return availableRooms;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an list of available rooms
        /// </summary>
        /// <returns>Room Entity object</returns>
        private async Task<IEnumerable<Domain.Base.Entities.Room>> GetAvailableRooms2Async(RoomsAvailabilityRequest2 request)
        {
            if (request == null)
            {
                // Integration API error InstructionalEvent.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                {
                    Code = "Global.SchemaValidation.Error",
                    Message = "Unable to parse request."
                });
                throw ex;
            }

            // 1. Validate the request data
            ValidateRoomAvailabilityRequestData2(request);

            // 2. Get all rooms and filter based on site, location and/or roomtype
            var allRoomsAsync = await _roomRepository.RoomsAsync();

            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                logger.Info("Facilities Timing: (CheckRoomAvailability) _FilterRoomsByRoomsAvailabilityRequest started");
                watch.Start();
            }

            var filteredRoomsList = await FilterRoomsByRoomsAvailabilityRequestAsync(request, allRoomsAsync);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info(
                    "Facilities Timing: (CheckRoomAvailability) _FilterRoomsByRoomsAvailabilityRequest completed in " +
                    watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // 3. Identify Rooms with sufficient capacity for each occupancy
            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) Identify rooms with capacity");
                watch.Restart();
            }
            var maxOccupancy = request.Occupancies.Max(o => o.MaximumOccupancy);
            var roomsWithCapacity = RoomAvailabilityService.GetRoomsWithCapacity(filteredRoomsList, maxOccupancy);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("Facilities Timing: (CheckRoomAvailability) Identify rooms with capacity completed in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }

            if (roomsWithCapacity == null || !roomsWithCapacity.Any())
            {
                // No rooms with sufficient capacity
                var ex = new IntegrationApiException("Insufficient Room Capacity");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                {
                    Code = "RoomsAvailabilityRequest.InsufficientRoomCapacity",
                    Message = "No rooms available with a capacity of at least " + maxOccupancy.ToString()
                });
                throw ex;
            }

            // 4. Build list of dates on which rooms must be available
            var campusCalendarId = _configurationRepository.GetDefaultsConfiguration().CampusCalendarId;
            var campusCalendar = _eventRepository.GetCalendar(campusCalendarId);
            var frequency = ConvertFrequencyType2EnumDtoToFrequencyTypeDomainEnum(request.Recurrence.RepeatRule.Type);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) _GetRoomAvailabilityMeetingDates starting");
                watch.Restart();
            }

            var meetingDates = RecurrenceUtility.GetRecurrenceDates(request.Recurrence.RepeatRule,
                request.Recurrence.TimePeriod, frequency, campusCalendar);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info(
                    "Facilities Timing: (CheckRoomAvailability) _GetRoomAvailabilityMeetingDates completed in " +
                    watch.ElapsedMilliseconds.ToString() + " ms");
            }

            if (meetingDates == null || !meetingDates.Any())
            {
                // No meeting dates for the supplied criteria
                var ex = new IntegrationApiException("No Meeting Dates");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                {
                    Code = "RoomsAvailabilityRequest.NoMeetingDates",
                    Message = "No meeting dates were identified for "
                              + request.Recurrence.TimePeriod.StartOn.Value.Date.ToString()
                              + " to " + request.Recurrence.TimePeriod.EndOn.Value.Date.ToString()
                              + " between " + request.Recurrence.TimePeriod.StartOn.Value.ToLocalTime().ToString()
                              + " and " + request.Recurrence.TimePeriod.EndOn.Value.ToLocalTime().ToString()
                              + " recurring " + frequency.ToString()
                });
                throw ex;
            }

            // 5. Get IDs of rooms with schedule conflicts based on meeting dates and times
            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) _GetRoomIdsWithConflicts starting");
                watch.Restart();
            }

            var roomsWithCapacityForLookup = roomsWithCapacity.Select(r => r.Id).Distinct().ToArray();
            var roomIdsWithConflicts = await _eventRepository.GetRoomIdsWithConflicts3Async(request.Recurrence.TimePeriod.StartOn.Value,
                request.Recurrence.TimePeriod.EndOn.Value, meetingDates, roomsWithCapacityForLookup);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("Facilities Timing: (CheckRoomAvailability) _GetRoomIdsWithConflicts completed in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // 6. Eliminate rooms with conflicts from list of rooms with capacity
            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) Eliminate rooms with conflicts starting");
                watch.Restart();
            }

            var availableRooms = roomsWithCapacity.ToList();
            if (roomIdsWithConflicts != null && roomIdsWithConflicts.Any())
            {
                availableRooms = roomsWithCapacity.Where(r => !roomIdsWithConflicts.Contains(r.Id)).ToList();
            }

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("Facilities Timing: (CheckRoomAvailability) Eliminate rooms with conflicts completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            return availableRooms;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an list of available rooms
        /// </summary>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Room Entity object</returns>
        private async Task<IEnumerable<Domain.Base.Entities.Room>> GetAvailableRooms3Async(RoomsAvailabilityRequest3 request, bool bypassCache = false)
        {
            if (request == null)
            {
                // Integration API error InstructionalEvent.NotFound
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                {
                    Code = "Global.SchemaValidation.Error",
                    Message = "Unable to parse request."
                });
                throw ex;
            }

            // 1. Validate the request data
            ValidateRoomAvailabilityRequestData3(request);

            // 2. Get all rooms and filter based on site, location and/or roomtype
            var allRoomsAsync = await _roomRepository.GetRoomsAsync(bypassCache);

            Stopwatch watch = null;
            if (logger.IsInfoEnabled)
            {
                watch = new Stopwatch();
                logger.Info("Facilities Timing: (CheckRoomAvailability) _FilterRoomsByRoomsAvailabilityRequest started");
                watch.Start();
            }

            var filteredRoomsList = await FilterRoomsByRoomsAvailabilityRequest2Async(request, allRoomsAsync);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info(
                    "Facilities Timing: (CheckRoomAvailability) _FilterRoomsByRoomsAvailabilityRequest completed in " +
                    watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // 3. Identify Rooms with sufficient capacity for each occupancy
            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) Identify rooms with capacity");
                watch.Restart();
            }
            var maxOccupancy = request.Occupancies == null ? 0 : request.Occupancies.Max(o => o.MaximumOccupancy);

            var roomsWithCapacity = (maxOccupancy == 0) ? null
                :  RoomAvailabilityService.GetRoomsWithCapacity(filteredRoomsList, maxOccupancy);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("Facilities Timing: (CheckRoomAvailability) Identify rooms with capacity completed in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }

            if (roomsWithCapacity == null || !roomsWithCapacity.Any())
            {
                return new List<Domain.Base.Entities.Room>();
            }
               

            // 4. Build list of dates on which rooms must be available
            var campusCalendarId = _configurationRepository.GetDefaultsConfiguration().CampusCalendarId;
            var campusCalendar = _eventRepository.GetCalendar(campusCalendarId);
            var frequency = ConvertFrequencyType2EnumDtoToFrequencyTypeDomainEnum(request.Recurrence.RepeatRule.Type);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) _GetRoomAvailabilityMeetingDates starting");
                watch.Restart();
            }

            var meetingDates = RecurrenceUtility.GetRecurrenceDates2(request.Recurrence.RepeatRule,
                request.Recurrence.TimePeriod, frequency, campusCalendar);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info(
                    "Facilities Timing: (CheckRoomAvailability) _GetRoomAvailabilityMeetingDates completed in " +
                    watch.ElapsedMilliseconds.ToString() + " ms");
            }

            if (meetingDates == null || !meetingDates.Any())
            {
                return new List<Domain.Base.Entities.Room>();
            }

            // 5. Get IDs of rooms with schedule conflicts based on meeting dates and times
            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) _GetRoomIdsWithConflicts starting");
                watch.Restart();
            }

            var roomsWithCapacityForLookup = roomsWithCapacity.Select(r => r.Id).Distinct().ToArray();

            DateTimeOffset endOn = request.Recurrence.TimePeriod.EndOn.Value;
            bool isMidnight = false;
            if (request.Recurrence.TimePeriod.EndOn == default(DateTimeOffset))
            {
                var lastDate = meetingDates.OrderBy(md => md.Date).LastOrDefault().Date;
                endOn = DateTime.SpecifyKind(lastDate, DateTimeKind.Utc);
                isMidnight = true;
            }

            var roomIdsWithConflicts = await _eventRepository.GetRoomIdsWithConflicts3Async(request.Recurrence.TimePeriod.StartOn.Value,
                endOn, meetingDates, roomsWithCapacityForLookup, isMidnight);

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("Facilities Timing: (CheckRoomAvailability) _GetRoomIdsWithConflicts completed in " +
                            watch.ElapsedMilliseconds.ToString() + " ms");
            }

            // 6. Eliminate rooms with conflicts from list of rooms with capacity
            if ((logger.IsInfoEnabled) && (watch != null))
            {
                logger.Info("Facilities Timing: (CheckRoomAvailability) Eliminate rooms with conflicts starting");
                watch.Restart();
            }

            var availableRooms = roomsWithCapacity.ToList();
            if (roomIdsWithConflicts != null && roomIdsWithConflicts.Any())
            {
                availableRooms = roomsWithCapacity.Where(r => !roomIdsWithConflicts.Contains(r.Id)).ToList();
            }

            if ((logger.IsInfoEnabled) && (watch != null))
            {
                watch.Stop();
                logger.Info("Facilities Timing: (CheckRoomAvailability) Eliminate rooms with conflicts completed in " + watch.ElapsedMilliseconds.ToString() + " ms");
            }
            return availableRooms;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Filter Rooms By RoomsAvailabilityRequest 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="allRooms"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room>> FilterRoomsByRoomsAvailabilityRequestAsync(Dtos.RoomsAvailabilityRequest2 request, IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room> allRooms)
        {
            if (request == null)
            {
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                {
                    Code = "Global.SchemaValidation.Error",
                    Message = "Unable to parse request."
                });
                throw ex;
            }
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room> filteredRoomsList = null;

            if (request.Building != null && request.Building.Id != string.Empty)
            {
                filteredRoomsList = await FilterRoomsByBuildingAsync(allRooms, request.Building.Id);
                if (filteredRoomsList == null || !filteredRoomsList.Any())
                {
                    var ex = new IntegrationApiException("Room Filter");
                    ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                    {
                        Code = "RoomsAvailabilityRequest.RoomFilter",
                        Message = string.Format("No rooms available with at location.   BuildingID {0}", request.Building.Id)
                    });
                    throw ex;
                }
            }
            // Only filter by sites if buildingID not provided.
            if ((filteredRoomsList == null) && (request.Site != null && request.Site.Id != string.Empty))
            {
                filteredRoomsList = await FilterRoomsBySiteAsync(allRooms, request.Site.Id);
                if (filteredRoomsList == null || !filteredRoomsList.Any())
                {
                    var ex = new IntegrationApiException("Room Filter");
                    ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                    {
                        Code = "RoomsAvailabilityRequest.RoomFilter",
                        Message = string.Format("No rooms available with at location.  SiteID {0}. ", request.Site.Id)
                    });
                    throw ex;
                }
            }

            if ((request.RoomType != null) && (request.RoomType.Any()))
            {
                filteredRoomsList = await FilterRoomsByRoomTypeAsync(filteredRoomsList != null && filteredRoomsList.Any() ? filteredRoomsList : allRooms, request.RoomType);
                if (filteredRoomsList == null || !filteredRoomsList.Any())
                {
                    var ex = new IntegrationApiException("Room Filter");
                    ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                    {
                        Code = "RoomsAvailabilityRequest.RoomFilter",
                        Message = "No rooms available with at location with provided roomtype(s)."
                    });
                    throw ex;
                }
            }

            if (filteredRoomsList == null || !filteredRoomsList.Any()) filteredRoomsList = allRooms.Distinct();

            return filteredRoomsList;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Filter Rooms By RoomsAvailabilityRequest 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="allRooms"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room>> FilterRoomsByRoomsAvailabilityRequest2Async(Dtos.RoomsAvailabilityRequest3 request, IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room> allRooms)
        {
            if (request == null)
            {
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                {
                    Code = "Global.SchemaValidation.Error",
                    Message = "Unable to parse request."
                });
                throw ex;
            }
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room> filteredRoomsList = null;

            if (request.Building != null && request.Building.Id != string.Empty)
            {
                filteredRoomsList = await FilterRoomsByBuildingAsync(allRooms, request.Building.Id);
                if (filteredRoomsList == null || !filteredRoomsList.Any())
                {
                    var ex = new IntegrationApiException("Room Filter");
                    ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                    {
                        Code = "RoomsAvailabilityRequest.RoomFilter",
                        Message = string.Format("No rooms available with at location.   BuildingID {0}", request.Building.Id)
                    });
                    throw ex;
                }
            }
            // Only filter by sites if buildingID not provided.
            if ((filteredRoomsList == null) && (request.Site != null && request.Site.Id != string.Empty))
            {
                filteredRoomsList = await FilterRoomsBySiteAsync(allRooms, request.Site.Id);
                if (filteredRoomsList == null || !filteredRoomsList.Any())
                {
                    var ex = new IntegrationApiException("Room Filter");
                    ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                    {
                        Code = "RoomsAvailabilityRequest.RoomFilter",
                        Message = string.Format("No rooms available with at location.  SiteID {0}. ", request.Site.Id)
                    });
                    throw ex;
                }
            }

            if ((request.RoomType != null) && (request.RoomType.Any()))
            {
                filteredRoomsList = await FilterRoomsByRoomTypeAsync(filteredRoomsList != null && filteredRoomsList.Any() ? filteredRoomsList : allRooms, request.RoomType);
                if (filteredRoomsList == null || !filteredRoomsList.Any())
                {
                    var ex = new IntegrationApiException("Room Filter");
                    ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError()
                    {
                        Code = "RoomsAvailabilityRequest.RoomFilter",
                        Message = "No rooms available with at location with provided roomtype(s)."
                    });
                    throw ex;
                }
            }

            if (filteredRoomsList == null || !filteredRoomsList.Any()) filteredRoomsList = allRooms.Distinct();

            return filteredRoomsList;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        ///  Filter rooms by building
        /// </summary>
        /// <param name="rooms"></param>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room>> FilterRoomsByBuildingAsync(IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room> rooms, string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                throw new ArgumentNullException("buildingId", "buildingId cannot be null or empty.");
            }
            if (rooms == null || !rooms.Any())
            {
                throw new ArgumentNullException("rooms", "rooms cannot be null or empty.");
            }

            var allBuildings = await _referenceDataRepository.GetBuildingsAsync(true);
            if (allBuildings == null)
            {
                var ex = new IntegrationApiException("Unable to extract BUILDINGS from code file");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.NotFound", Message = "Buildings not found " });
                throw ex;
            }

            var building = allBuildings.FirstOrDefault(b => b.Guid == buildingId);
            if (building == null)
            {
                var ex = new IntegrationApiException("Building not found");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.NotFound", Message = "Building not found. Id: " + buildingId });
                throw ex;
            }

            var roomCollection = rooms.Where(r => r.BuildingCode == building.Code).ToList();

            return roomCollection.Distinct().AsEnumerable();
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Filter rooms by site/location
        /// </summary>
        /// <param name="rooms"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room>> FilterRoomsBySiteAsync(IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room> rooms, string siteId)
        {
            if (string.IsNullOrEmpty(siteId))
            {
                throw new ArgumentNullException("siteId", "siteId cannot be null or empty.");
            }
            if (rooms == null || !rooms.Any())
            {
                throw new ArgumentNullException("rooms", "rooms cannot be null or empty.");
            }

            var allLocations = await _referenceDataRepository.GetLocationsAsync(true);
            if (allLocations == null)
            {
                var ex = new IntegrationApiException("Unable to extract locations from LOCATIONS code file");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.NotFound", Message = "Locations not found " });
                throw ex;
            }

            var location = allLocations.FirstOrDefault(s => s.Guid == siteId);

            if (location == null)
            {
                var ex = new IntegrationApiException("Location not found");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.NotFound", Message = "Location not found " });
                throw ex;
            }

            var roomCollection = rooms.Where(r => location.BuildingCodes.Contains(r.BuildingCode));
            return roomCollection.Distinct().AsEnumerable();

        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        ///  Filter rooms by room type
        /// </summary>
        /// <param name="allRooms"></param>
        /// <param name="roomTypes"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room>> FilterRoomsByRoomTypeAsync(IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Room> allRooms, IEnumerable<Dtos.RoomType> roomTypes)
        {
            if (roomTypes == null || !roomTypes.Any())
            {
                throw new ArgumentNullException("roomTypes", "RoomTypes cannot be null or empty.");
            }
            if (allRooms == null || !allRooms.Any())
            {
                throw new ArgumentNullException("allRooms", "AllRooms cannot be null or empty.");
            }

            var roomCollection = new List<Ellucian.Colleague.Domain.Base.Entities.Room>();
            var allRoomTypes = await _referenceDataRepository.GetRoomTypesAsync(true);

            //if the consumer does not have any configured roomtypes, then return all rooms
            if ((allRoomTypes == null) || (!allRoomTypes.Any()))
                return allRooms.Distinct().AsEnumerable();


            foreach (Dtos.RoomType roomType in roomTypes)
            {
                List<Domain.Base.Entities.RoomTypes> types = null;
                // if the provided room type is Other  - we also want to return any roomtype that is null
                if (roomType.Type == RoomTypeTypes.Other)
                    types = allRoomTypes.Where(x => x.Type == null || x.Type == Domain.Base.Entities.RoomType.Other).ToList();
                else
                    types = allRoomTypes.Where(x => ConvertRoomTypeDomainEnumToRoomLayoutTypeDtoEnum(x.Type) == roomType.Type).ToList();

                if (types != null)
                {
                    foreach (var type in types)
                    {
                        //  roomCollection.AddRange(allRooms.Where(x => x.RoomType.Contains(type.Code)).ToList());
                        roomCollection.AddRange(allRooms.Where(x => !string.IsNullOrEmpty(x.RoomType)
                            && x.RoomType == type.Code).ToList());
                    }
                }
            }

            return roomCollection.Distinct().AsEnumerable();
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Location domain entity to its corresponding Site DTO
        /// </summary>
        /// <param name="source">Location domain entity</param>
        /// <returns>Site DTO</returns>
        private Ellucian.Colleague.Dtos.Site2 ConvertLocationEntityToSiteDto2(Ellucian.Colleague.Domain.Base.Entities.Location source)
        {
            var site = new Ellucian.Colleague.Dtos.Site2();

            //site.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            site.Id = source.Guid;
            site.Code = source.Code;
            site.Title = source.Description;
            site.Description = null;
            site.OrganizationGuid = null;

            return site;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Building domain entity to its corresponding Building DTO
        /// </summary>
        /// <param name="source">Building domain entity</param>
        /// <returns>Building DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Building3> ConvertBuildingEntityToDto3(Ellucian.Colleague.Domain.Base.Entities.Building source, bool bypassCache)
        {
            var building = new Ellucian.Colleague.Dtos.Building3();

            //building.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            building.Id = source.Guid;
            building.Code = source.Code;
            building.Title = source.Description;
            if (!string.IsNullOrEmpty(source.LongDescription))
            {
                building.Description = source.LongDescription;
            }
            if (!string.IsNullOrEmpty(source.ImageUrl))
            {
                building.ImageUrl = source.ImageUrl;
            }
            if (!string.IsNullOrEmpty(source.AdditionalServices))
            {
                building.Characteristics = new List<string>();
                if (!string.IsNullOrEmpty(source.AdditionalServices))
                {
                    building.Characteristics.Add(source.AdditionalServices);
                }
            }
            if (!string.IsNullOrEmpty(source.Comments))
            {
                building.Comments = new List<string>();
                if (!string.IsNullOrEmpty(source.Comments))
                {
                    building.Comments.Add(source.Comments);
                }
            }
            if (source.AddressLines != null && source.AddressLines.Any())
            {
                var address = new Dtos.DtoProperties.BuildingAddress()
                {
                    AddressLines = source.AddressLines,
                    Place = await BuildAddressPlace(source.Country,
                    source.City, source.State, source.PostalCode, bypassCache),
                    Latitude = source.Latitude,
                    Longitude = source.Longitude
                };
                building.Address = address;
            }


            var site = _referenceDataRepository.Locations.FirstOrDefault(l => l.Code == source.LocationId);
            if (site != null)
            {
                building.SiteGuid = new Dtos.GuidObject2(site.Guid);
            }

            return building;
        }

        /// <summary>
        /// Build an AddressPlace DTO from address components
        /// </summary>
        /// <param name="addressCountry"></param>
        /// <param name="addressCity"></param>
        /// <param name="addressState"></param>
        /// <param name="addressZip"></param>
        /// <param name="hostCountry"></param>
        /// <param name="bypassCache"></param>
        /// <returns><see cref="AddressPlace"></returns>
        private async Task<AddressPlace> BuildAddressPlace(string addressCountry, string addressCity,
            string addressState, string addressZip, bool bypassCache = false)
        {
            var addressCountryDto = new Dtos.AddressCountry();
            Dtos.AddressRegion region = null;
            Domain.Base.Entities.Country country = null;
            if (!string.IsNullOrEmpty(addressCountry))
                country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressCountry);
            else
            {
                if (!string.IsNullOrEmpty(addressState))
                {
                    var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressState);
                    if (states != null)
                    {
                        if (!string.IsNullOrEmpty(states.CountryCode))
                        {
                            country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.Code == states.CountryCode);
                        }
                    }
                }
                if (country == null)
                {
                    var hostCountry = await GetHostCountry();
                    if (hostCountry == "USA" || string.IsNullOrEmpty(hostCountry))
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "USA");
                    else
                        country = (await GetAllCountriesAsync(bypassCache)).FirstOrDefault(x => x.IsoAlpha3Code == "CAN");
                }
            }
            if (country == null)
            {
                if (!string.IsNullOrEmpty(addressCountry))
                {
                    throw new KeyNotFoundException("Unable to locate ISO country code for " + addressCountry);
                }
                throw new KeyNotFoundException("Unable to locate ISO country code for " + addressCountry);
            }

            //need to check to make sure ISO code is there.
            if (country != null && string.IsNullOrEmpty(country.IsoAlpha3Code))
                throw new ArgumentException("Unable to locate ISO country code for " + country.Code);

            switch (country.IsoAlpha3Code)
            {
                case "USA":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.USA;
                    addressCountryDto.PostalTitle = "UNITED STATES OF AMERICA";
                    break;
                case "CAN":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.CAN;
                    addressCountryDto.PostalTitle = "CANADA";
                    break;
                case "AUS":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.AUS;
                    addressCountryDto.PostalTitle = "AUSTRALIA";
                    break;
                case "BRA":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.BRA;
                    addressCountryDto.PostalTitle = "BRAZIL";
                    break;
                case "MEX":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.MEX;
                    addressCountryDto.PostalTitle = "MEXICO";
                    break;
                case "NLD":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.NLD;
                    addressCountryDto.PostalTitle = "NETHERLANDS";
                    break;
                case "GBR":
                    addressCountryDto.Code = Dtos.EnumProperties.IsoCode.GBR;
                    addressCountryDto.PostalTitle = "UNITED KINGDOM OF GREAT BRITAIN AND NORTHERN IRELAND";
                    break;
                default:
                    try
                    {
                        addressCountryDto.Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), country.IsoAlpha3Code);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Concat(ex.Message, "For the Country: '", addressCountry, "'. ISOCode Not found: ", country.IsoAlpha3Code));
                    }

                    addressCountryDto.PostalTitle = country.Description.ToUpper();
                    break;
            }

            if (!string.IsNullOrEmpty(addressState))
            {
                var states = (await GetAllStatesAsync(bypassCache)).FirstOrDefault(x => x.Code == addressState);
                if (states != null)
                {
                    region = new Dtos.AddressRegion();
                    region.Code = string.Concat(country.IsoCode, "-", states.Code);
                    region.Title = states.Description;
                }
                else
                {
                    throw new ArgumentException(string.Concat("Description not found for for the state: '", addressState, "' or an error has occurred retrieving that value. "));
                }
            }

            if (region != null)
            {
                addressCountryDto.Region = region;
            }

            if (!string.IsNullOrEmpty(addressCity))
            {
                addressCountryDto.Locality = addressCity;
            }

            if (!string.IsNullOrEmpty(addressZip))
            {
                addressCountryDto.PostalCode = addressZip;
            }

            if (country != null)
                addressCountryDto.Title = country.Description;

            if (addressCountryDto != null)
            {
                return new AddressPlace()
                {
                    Country = addressCountryDto
                };
            }

            return null;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Building domain entity to its corresponding Building DTO
        /// </summary>
        /// <param name="source">Building domain entity</param>
        /// <returns>Building DTO</returns>
        private Ellucian.Colleague.Dtos.Building2 ConvertBuildingEntityToDto2(Ellucian.Colleague.Domain.Base.Entities.Building source)
        {
            var building = new Ellucian.Colleague.Dtos.Building2();

            //building.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            building.Id = source.Guid;
            building.Code = source.Code;
            building.Title = source.Description;
            building.Description = source.LongDescription;

            var site = _referenceDataRepository.Locations.FirstOrDefault(l => l.Code == source.LocationId);
            if (site != null)
            {
                building.SiteGuid = new Dtos.GuidObject2(site.Guid);
            }

            return building;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a RoomWing domain entity to its corresponding BuildingWing DTO
        /// </summary>
        /// <param name="source">BuildingWing domain entity</param>
        /// <returns>RoomWing DTO</returns>
        private Ellucian.Colleague.Dtos.BuildingWing ConvertRoomWingsEntityToBuildingWingsDto(Ellucian.Colleague.Domain.Base.Entities.RoomWing source)
        {
            var buildingWing = new Ellucian.Colleague.Dtos.BuildingWing
            {
                Id = source.Guid,
                Code = source.Code,
                Title = source.Description,

            };

            return buildingWing;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Room domain entity to its corresponding Room DTO
        /// </summary>
        /// <param name="source">Room domain entity</param>
        /// <returns>Room DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Room> ConvertRoomEntityToDtoAsync(Ellucian.Colleague.Domain.Base.Entities.Room source)
        {
            var room = new Ellucian.Colleague.Dtos.Room();

            //room.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            room.Guid = source.Guid;
            room.Floor = source.Floor;
            room.Number = source.Number;
            room.Title = source.Name;

            var building = (await _referenceDataRepository.BuildingsAsync()).FirstOrDefault(b => b.Code == source.BuildingCode);
            string buildingGuid = (building != null) ? building.Guid : null;
            room.BuildingGuid = new Dtos.GuidObject(buildingGuid);

            room.Description = (building != null) ? building.Description + " " + source.Number : null;

            room.Occupancies = new List<Dtos.Occupancy>() { new Dtos.Occupancy() { MaximumOccupancy = source.Capacity, RoomLayoutType = Dtos.RoomLayoutType.Classroom } };

            return room;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Room domain entity to its corresponding Room DTO
        /// </summary>
        /// <param name="source">Room domain entity</param>
        /// <returns>Room DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Room2> ConvertRoomEntityToDto2Async(Ellucian.Colleague.Domain.Base.Entities.Room source, bool bypassCache = false)
        {
            var room = new Ellucian.Colleague.Dtos.Room2();

            room.Id = source.Guid;
            room.Floor = source.Floor;
            room.Number = source.Number;
            room.Title = source.Name;

            var allBuildings = await _referenceDataRepository.BuildingsAsync();
            if (allBuildings == null)
                throw new KeyNotFoundException("An error occurred extracting all buildings from file BUILDINGS ");

            var building = allBuildings.FirstOrDefault(b => b.Code == source.BuildingCode);
            if (building != null)
            {
                room.Description = string.Concat(source.Description, " ", source.Number);
                room.BuildingGuid = new Dtos.GuidObject2(building.Guid);

                var allSites = await this.GetSites2Async(bypassCache);
                if (allSites == null)
                    throw new KeyNotFoundException("An error occurred extracting all sites from file LOCATIONS");

                var site = allSites.FirstOrDefault(s => building.LocationId != null && s.Code == building.LocationId);
                room.SiteGuid = (site != null) ? new Dtos.GuidObject2(site.Id) : null;
            }

            var allRoomTypes = await _referenceDataRepository.RoomTypesAsync();
            if (allRoomTypes == null)
                throw new KeyNotFoundException("An error occurred extracting all room types from file ROOM.TYPES ");


            var roomType = allRoomTypes.FirstOrDefault(rt => rt.Code == source.RoomType);
            var roomLayoutType = (roomType != null) ? ConvertRoomTypeDomainEnumToRoomLayoutTypeDtoEnum(roomType.Type) : RoomTypeTypes.Other;

            if ((roomType != null) && (roomType.Guid != string.Empty))
                room.RoomTypes = new List<Dtos.RoomType>() { new Dtos.RoomType() { RoomTypesGuid = new Dtos.GuidObject2(roomType.Guid), Type = roomLayoutType } };

            room.Occupancies = new List<Dtos.Occupancy2>() { new Dtos.Occupancy2() { MaximumOccupancy = source.Capacity, RoomLayoutType = RoomLayoutType2.Default } };

            return room;
        }



        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Room domain entity to its corresponding Room DTO
        /// </summary>
        /// <param name="source">Room domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>Room DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Room3> ConvertRoomEntityToDto3Async(Ellucian.Colleague.Domain.Base.Entities.Room source, bool bypassCache = false)
        {
            var room = new Ellucian.Colleague.Dtos.Room3();

            //room.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            room.Id = source.Guid;
            room.Floor = source.Floor;
            room.Number = source.Number;
            room.Title = source.Name;

            var building = (await GetBuildingsAsync(bypassCache)).FirstOrDefault(b => b.Code == source.BuildingCode);
            if (building != null)
            {
                room.Description = string.Concat(source.Description, " ", source.Number);
                room.BuildingGuid = new Dtos.GuidObject2(building.Guid);

                var site = (await GetSites2Async(bypassCache)).FirstOrDefault(s => building.LocationId != null
                       && s.Code.Equals(building.LocationId, StringComparison.OrdinalIgnoreCase));
                room.SiteGuid = (site != null) ? new Dtos.GuidObject2(site.Id) : null;
            }

            var roomType = (await GetRoomTypesAsync(bypassCache)).FirstOrDefault(rt => rt.Code == source.RoomType);
            var roomLayoutType = (roomType != null) ? ConvertRoomTypeDomainEnumToRoomLayoutTypeDtoEnum(roomType.Type) : RoomTypeTypes.Other;

            if ((roomType != null) && (roomType.Guid != string.Empty))
                room.RoomTypes = new List<Dtos.RoomType>() { new Dtos.RoomType() 
                { RoomTypesGuid = new Dtos.GuidObject2(roomType.Guid), Type = roomLayoutType } };

            room.Occupancies = new List<Dtos.Occupancy2>() { new Dtos.Occupancy2() 
                { MaximumOccupancy = source.Capacity, RoomLayoutType = RoomLayoutType2.Default } };

            if (!string.IsNullOrEmpty(source.Wing))
            {
                var roomWing = (await GetRoomWingsAsync(bypassCache)).FirstOrDefault(x => x.Code.Equals(source.Wing, StringComparison.OrdinalIgnoreCase));
                if (roomWing != null)
                {
                    room.Wing = new GuidObject2(roomWing.Guid);
                }
            }

            if ((source.Characteristics != null) && (source.Characteristics.Any()))
            {
                var roomCharacteristicCollection = new List<GuidObject2>();
                var roomCharacteristics = (await GetRoomCharacteristicsAsync(bypassCache)).ToList();
                foreach (var roomCharacteristic in source.Characteristics)
                {
                    var characteristic = roomCharacteristics.FirstOrDefault(x => x.Code.Equals(roomCharacteristic, StringComparison.OrdinalIgnoreCase));
                    if (characteristic != null)
                    {
                        roomCharacteristicCollection.Add(new GuidObject2(characteristic.Guid));
                    }
                }
                room.RoomCharacteristics = roomCharacteristicCollection;
            }

            return room;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Room domain entity to its corresponding Room DTO
        /// </summary>
        /// <param name="source">Room domain entity</param>
        /// <returns>Room DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.RoomsMinimumResponse> ConvertRoomEntityToRoomsMinimimDtoAsync(Ellucian.Colleague.Domain.Base.Entities.Room source)
        {
            var room = new RoomsMinimumResponse { Id = source.Guid };
            return room;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a RoomType domain enumeration value to its corresponding RoomLayoutType DTO enumeration value
        /// </summary>
        /// <param name="source">RoomType domain enumeration value</param>
        /// <returns>RoomType DTO enumeration value</returns>
        private Dtos.RoomTypeTypes ConvertRoomTypeDomainEnumToRoomLayoutTypeDtoEnum(Ellucian.Colleague.Domain.Base.Entities.RoomType? source)
        {
            if (source == null)
                return Dtos.RoomTypeTypes.Other;

            switch (source)
            {
                case Domain.Base.Entities.RoomType.Amphitheater:
                    return Dtos.RoomTypeTypes.Amphitheater;
                case Domain.Base.Entities.RoomType.Animalquarters:
                    return Dtos.RoomTypeTypes.Animalquarters;
                case Domain.Base.Entities.RoomType.Apartment:
                    return Dtos.RoomTypeTypes.Apartment;
                case Domain.Base.Entities.RoomType.Artstudio:
                    return Dtos.RoomTypeTypes.Artstudio;
                case Domain.Base.Entities.RoomType.Atrium:
                    return Dtos.RoomTypeTypes.Atrium;
                case Domain.Base.Entities.RoomType.Audiovisuallab:
                    return Dtos.RoomTypeTypes.Audiovisuallab;
                case Domain.Base.Entities.RoomType.Auditorium:
                    return Dtos.RoomTypeTypes.Auditorium;
                case Domain.Base.Entities.RoomType.Ballroom:
                    return Dtos.RoomTypeTypes.Ballroom;
                case Domain.Base.Entities.RoomType.Booth:
                    return Dtos.RoomTypeTypes.Booth;
                case Domain.Base.Entities.RoomType.Classroom:
                    return Dtos.RoomTypeTypes.Classroom;
                case Domain.Base.Entities.RoomType.Clinic:
                    return Dtos.RoomTypeTypes.Clinic;
                case Domain.Base.Entities.RoomType.Computerlaboratory:
                    return Dtos.RoomTypeTypes.Computerlaboratory;
                case Domain.Base.Entities.RoomType.Conferenceroom:
                    return Dtos.RoomTypeTypes.Conferenceroom;
                case Domain.Base.Entities.RoomType.Daycare:
                    return Dtos.RoomTypeTypes.Daycare;
                case Domain.Base.Entities.RoomType.Foodfacility:
                    return Dtos.RoomTypeTypes.Foodfacility;
                case Domain.Base.Entities.RoomType.Generalusefacility:
                    return Dtos.RoomTypeTypes.Generalusefacility;
                case Domain.Base.Entities.RoomType.Greenhouse:
                    return Dtos.RoomTypeTypes.Greenhouse;
                case Domain.Base.Entities.RoomType.Healthcarefacility:
                    return Dtos.RoomTypeTypes.Healthcarefacility;
                case Domain.Base.Entities.RoomType.House:
                    return Dtos.RoomTypeTypes.House;
                case Domain.Base.Entities.RoomType.Lecturehall:
                    return Dtos.RoomTypeTypes.Lecturehall;
                case Domain.Base.Entities.RoomType.Lounge:
                    return Dtos.RoomTypeTypes.Lounge;
                case Domain.Base.Entities.RoomType.Mechanicslab:
                    return Dtos.RoomTypeTypes.Mechanicslab;
                case Domain.Base.Entities.RoomType.Merchandisingroom:
                    return Dtos.RoomTypeTypes.Merchandisingroom;
                case Domain.Base.Entities.RoomType.Musicroom:
                    return Dtos.RoomTypeTypes.Musicroom;
                case Domain.Base.Entities.RoomType.Office:
                    return Dtos.RoomTypeTypes.Office;
                case Domain.Base.Entities.RoomType.Other:
                    return Dtos.RoomTypeTypes.Other;
                case Domain.Base.Entities.RoomType.Performingartsstudio:
                    return Dtos.RoomTypeTypes.Performingartsstudio;
                case Domain.Base.Entities.RoomType.Residencehallroom:
                    return Dtos.RoomTypeTypes.Residencehallroom;
                case Domain.Base.Entities.RoomType.Residentialdoubleroom:
                    return Dtos.RoomTypeTypes.Residentialdoubleroom;
                case Domain.Base.Entities.RoomType.Residentialsingleroom:
                    return Dtos.RoomTypeTypes.Residentialsingleroom;
                case Domain.Base.Entities.RoomType.Residentialsuiteroom:
                    return Dtos.RoomTypeTypes.Residentialsuiteroom;
                case Domain.Base.Entities.RoomType.Residentialtripleroom:
                    return Dtos.RoomTypeTypes.Residentialtripleroom;
                case Domain.Base.Entities.RoomType.Sciencelaboratory:
                    return Dtos.RoomTypeTypes.Sciencelaboratory;
                case Domain.Base.Entities.RoomType.Seminarroom:
                    return Dtos.RoomTypeTypes.Seminarroom;
                case Domain.Base.Entities.RoomType.Specialusefacility:
                    return Dtos.RoomTypeTypes.Specialusefacility;
                case Domain.Base.Entities.RoomType.Studyfacility:
                    return Dtos.RoomTypeTypes.Studyfacility;
                case Domain.Base.Entities.RoomType.Supportfacility:
                    return Dtos.RoomTypeTypes.Supportfacility;
                default:
                    return Dtos.RoomTypeTypes.Other;

            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a FrequencyType DTO enumeration value to its corresponding FrequencyType Domain enumeration value
        /// </summary>
        /// <param name="type">FrequencyType DTO enumeration value</param>
        /// <returns>FrequencyType Domain enumeration value</returns>
        private Ellucian.Colleague.Domain.Base.Entities.FrequencyType ConvertFrequencyTypeEnumDtoToFrequencyTypeDomainEnum(Dtos.FrequencyType type)
        {
            switch (type)
            {
                case Dtos.FrequencyType.Weekly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly;
                case Dtos.FrequencyType.Monthly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Monthly;
                case Dtos.FrequencyType.Yearly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Yearly;
                default:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Daily;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a FrequencyType2 DTO enumeration value to its corresponding FrequencyType Domain enumeration value
        /// </summary>
        /// <param name="type">FrequencyType2 DTO enumeration value</param>
        /// <returns>FrequencyType Domain enumeration value</returns>
        private Ellucian.Colleague.Domain.Base.Entities.FrequencyType ConvertFrequencyType2EnumDtoToFrequencyTypeDomainEnum(Dtos.FrequencyType2? type)
        {
            switch (type)
            {
                case Dtos.FrequencyType2.Weekly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Weekly;
                case Dtos.FrequencyType2.Monthly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Monthly;
                case Dtos.FrequencyType2.Yearly:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Yearly;
                default:
                    return Ellucian.Colleague.Domain.Base.Entities.FrequencyType.Daily;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Validate the content of a Room Availability Request
        /// </summary>
        /// <param name="request">Room Availability Request</param>
        private void ValidateRoomAvailabilityRequestData2(Dtos.RoomsAvailabilityRequest2 request)
        {
            if ((request == null) || (request.Recurrence == null) || (request.RoomType == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest", "Must provide a date range, time, frequency and roomType");
            if ((request.Recurrence.TimePeriod != null) && (request.Recurrence.TimePeriod.StartOn == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.TimePeriod.StartOn", "Must provide a start date");
            if (request.Recurrence.RepeatRule == null)
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.RepeatRule", "Must provide a repeat rule");


            if (request.Occupancies == null || request.Occupancies.Count() == 0)
            {
                // Integration API error
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.Occupancies.NotSpecified", Message = "occupancies cannot be null or empty." });
                throw ex;
            }
            if (request.Occupancies.Any(o => o.MaximumOccupancy <= 0))
            {
                // Integration API error
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.Occupancies.MaximumOccupancy.OutOfRange", Message = "maxOccupancy must be greater than zero." });
                throw ex;
            }

            switch (request.Recurrence.RepeatRule.Type)
            {
                case Dtos.FrequencyType2.Daily:
                    var repeatRuleDaily = (Dtos.RepeatRuleDaily)request.Recurrence.RepeatRule;
                    if (repeatRuleDaily == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleDaily", "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleDaily.Interval < 1) || (repeatRuleDaily.Interval > 365))
                    {
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleDaily.Interval.Invalid", Message = "Interval must be between 1 and 365." });
                        throw ex;
                    }
                    if ((repeatRuleDaily != null) && (repeatRuleDaily.Ends != null) && ((repeatRuleDaily.Ends.Date == null) && (repeatRuleDaily.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleDaily", "If providing a value for ends, must provide either a date or repetition value");
                    if ((repeatRuleDaily.Ends != null) && (repeatRuleDaily.Ends.Repetitions != null))
                    {
                        if ((repeatRuleDaily.Ends.Repetitions < 1) || (repeatRuleDaily.Ends.Repetitions > 365))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleDaily.Ends.Repetitions.Invalid", Message = "Ends.Repetitions must be between 1 and 365." });
                            throw ex;
                        }
                    }
                    break;
                case Dtos.FrequencyType2.Weekly:
                    var repeatRuleWeekly = (Dtos.RepeatRuleWeekly)request.Recurrence.RepeatRule;
                    if (repeatRuleWeekly == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleWeekly", "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleWeekly.Ends != null) && ((repeatRuleWeekly.Ends.Date == null) && (repeatRuleWeekly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleWeekly", "If providing a value for ends, must provide either a date or repetition value");

                    if (repeatRuleWeekly.DayOfWeek != null)
                    {
                        foreach (var dayOfWeek in repeatRuleWeekly.DayOfWeek)
                        {
                            if (!Enum.IsDefined(typeof(HedmDayOfWeek), dayOfWeek))
                            {
                                var ex = new IntegrationApiException("Validation exception");
                                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleWeekly.DayOfWeek", Message = "DayOfWeek contains an invalid value" });
                                throw ex;
                            }
                        }
                    }

                    if ((repeatRuleWeekly.Ends != null) && (repeatRuleWeekly.Ends.Repetitions != null))
                    {
                        if ((repeatRuleWeekly.Ends.Repetitions < 1) || (repeatRuleWeekly.Ends.Repetitions > 52))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleWeekly.Ends.Repetitions.Invalid", Message = "Ends.Repetitions must be between 1 and 52." });
                            throw ex;
                        }
                    }

                    if ((repeatRuleWeekly.Interval < 1) || (repeatRuleWeekly.Interval > 52))
                    {
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleWeekly.Interval.Invalid", Message = "Interval must be between 1 and 52." });
                        throw ex;
                    }
                    break;
                case Dtos.FrequencyType2.Monthly:
                    var repeatRuleMonthly = (Dtos.RepeatRuleMonthly)request.Recurrence.RepeatRule;
                    if ((repeatRuleMonthly) == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly", "Unable to determine Recurrence.RepeatRule");
                    if (repeatRuleMonthly.RepeatBy == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly", "Must provide a repeatBy rule");

                    if ((repeatRuleMonthly.RepeatBy.DayOfMonth == 0) && (repeatRuleMonthly.RepeatBy.DayOfWeek == null))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly", "If providing a RepeatBy.DayOfWeek, must provide an occurrence or day");
                    if ((repeatRuleMonthly.Ends != null) && ((repeatRuleMonthly.Ends.Date == null) && (repeatRuleMonthly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly", "If providing a value for ends, must provide either a date or repetition value");

                    if ((repeatRuleMonthly.RepeatBy.DayOfWeek != null) && (repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence != 0))
                    {
                        if ((repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence < -4) || (repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence > 4))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleMonthly.RepeatBy.DayOfWeek.Occurence.Invalid", Message = "RepeatBy.DayOfWeek.Occurence must be between -4 and 4." });
                            throw ex;
                        }
                    }

                    if (repeatRuleMonthly.RepeatBy.DayOfMonth != 0)
                    {
                        if ((repeatRuleMonthly.RepeatBy.DayOfMonth < 1) || (repeatRuleMonthly.RepeatBy.DayOfMonth > 31))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleMonthly.RepeatBy.DayOfMonth.Invalid", Message = "RepeatBy.DayOfMonth must be between 1 and 31." });
                            throw ex;
                        }
                    }

                    if ((repeatRuleMonthly.Ends != null) && (repeatRuleMonthly.Ends.Repetitions != null))
                    {
                        if ((repeatRuleMonthly.Ends.Repetitions < 1) || (repeatRuleMonthly.Ends.Repetitions > 12))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleMonthly.Ends.Repetitions.Invalid", Message = "Ends.Repetitions must be between 1 and 12." });
                            throw ex;
                        }
                    }

                    if ((repeatRuleMonthly.Interval < 1) || (repeatRuleMonthly.Interval > 12))
                    {
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleMonthly.Interval.Invalid", Message = "Interval must be between 1 and 12." });
                        throw ex;
                    }

                    break;
                case Dtos.FrequencyType2.Yearly:
                    break;
                default:
                    {// Integration API error
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.Recurrence.Invalid", Message = "Unable to determine recurrence pattern." });
                        throw ex;
                    }
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Validate the content of a Room Availability Request
        /// </summary>
        /// <param name="request">Room Availability Request</param>
        private void ValidateRoomAvailabilityRequestData3(Dtos.RoomsAvailabilityRequest3 request)
        {
            if ((request == null) || (request.Recurrence == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest", "Must provide a date range and/or time");
            if ((request.Recurrence.TimePeriod != null) && (request.Recurrence.TimePeriod.StartOn == null))
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.TimePeriod.StartOn", "Must provide a start date");
            if (request.Recurrence.RepeatRule == null)
                throw new ArgumentNullException("RoomsAvailabilityRequest.Recurrence.RepeatRule", "Must provide a repeat rule");

            if ((request.Occupancies != null) && (request.Occupancies.Any(o => o.MaximumOccupancy <= 0)))
            {
                // Integration API error
                var ex = new IntegrationApiException("Validation exception");
                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.Occupancies.MaximumOccupancy.OutOfRange", Message = "maxOccupancy must be greater than zero." });
                throw ex;
            }

            switch (request.Recurrence.RepeatRule.Type)
            {
                case Dtos.FrequencyType2.Daily:
                    var repeatRuleDaily = (Dtos.RepeatRuleDaily)request.Recurrence.RepeatRule;
                    if (repeatRuleDaily == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleDaily", "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleDaily.Interval < 1) || (repeatRuleDaily.Interval > 365))
                    {
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleDaily.Interval.Invalid", Message = "Interval must be between 1 and 365." });
                        throw ex;
                    }
                    if ((repeatRuleDaily != null) && (repeatRuleDaily.Ends != null) && ((repeatRuleDaily.Ends.Date == null) && (repeatRuleDaily.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleDaily", "If providing a value for ends, must provide either a date or repetition value");
                    if ((repeatRuleDaily.Ends != null) && (repeatRuleDaily.Ends.Repetitions != null))
                    {
                        if ((repeatRuleDaily.Ends.Repetitions < 1) || (repeatRuleDaily.Ends.Repetitions > 365))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleDaily.Ends.Repetitions.Invalid", Message = "Ends.Repetitions must be between 1 and 365." });
                            throw ex;
                        }
                    }
                    break;
                case Dtos.FrequencyType2.Weekly:
                    var repeatRuleWeekly = (Dtos.RepeatRuleWeekly)request.Recurrence.RepeatRule;
                    if (repeatRuleWeekly == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleWeekly", "Unable to determine Recurrence.RepeatRule");
                    if ((repeatRuleWeekly.Ends != null) && ((repeatRuleWeekly.Ends.Date == null) && (repeatRuleWeekly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleWeekly", "If providing a value for ends, must provide either a date or repetition value");

                    if (repeatRuleWeekly.DayOfWeek != null)
                    {
                        foreach (var dayOfWeek in repeatRuleWeekly.DayOfWeek)
                        {
                            if (!Enum.IsDefined(typeof(HedmDayOfWeek), dayOfWeek))
                            {
                                var ex = new IntegrationApiException("Validation exception");
                                ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleWeekly.DayOfWeek", Message = "DayOfWeek contains an invalid value" });
                                throw ex;
                            }
                        }
                    }

                    if ((repeatRuleWeekly.Ends != null) && (repeatRuleWeekly.Ends.Repetitions != null))
                    {
                        if ((repeatRuleWeekly.Ends.Repetitions < 1) || (repeatRuleWeekly.Ends.Repetitions > 52))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleWeekly.Ends.Repetitions.Invalid", Message = "Ends.Repetitions must be between 1 and 52." });
                            throw ex;
                        }
                    }

                    if ((repeatRuleWeekly.Interval < 1) || (repeatRuleWeekly.Interval > 52))
                    {
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleWeekly.Interval.Invalid", Message = "Interval must be between 1 and 52." });
                        throw ex;
                    }
                    break;
                case Dtos.FrequencyType2.Monthly:
                    var repeatRuleMonthly = (Dtos.RepeatRuleMonthly)request.Recurrence.RepeatRule;
                    if ((repeatRuleMonthly) == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly", "Unable to determine Recurrence.RepeatRule");
                    if (repeatRuleMonthly.RepeatBy == null)
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly", "Must provide a repeatBy rule");

                    if ((repeatRuleMonthly.RepeatBy.DayOfMonth == 0) && (repeatRuleMonthly.RepeatBy.DayOfWeek == null))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly", "If providing a RepeatBy.DayOfWeek, must provide an occurrence or day");
                    if ((repeatRuleMonthly.Ends != null) && ((repeatRuleMonthly.Ends.Date == null) && (repeatRuleMonthly.Ends.Repetitions == null)))
                        throw new ArgumentNullException("RoomsAvailabilityRequest.RepeatRuleMonthly", "If providing a value for ends, must provide either a date or repetition value");

                    if ((repeatRuleMonthly.RepeatBy.DayOfWeek != null) && (repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence != 0))
                    {
                        if ((repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence < -4) || (repeatRuleMonthly.RepeatBy.DayOfWeek.Occurrence > 4))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleMonthly.RepeatBy.DayOfWeek.Occurence.Invalid", Message = "RepeatBy.DayOfWeek.Occurence must be between -4 and 4." });
                            throw ex;
                        }
                    }

                    if (repeatRuleMonthly.RepeatBy.DayOfMonth != 0)
                    {
                        if ((repeatRuleMonthly.RepeatBy.DayOfMonth < 1) || (repeatRuleMonthly.RepeatBy.DayOfMonth > 31))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleMonthly.RepeatBy.DayOfMonth.Invalid", Message = "RepeatBy.DayOfMonth must be between 1 and 31." });
                            throw ex;
                        }
                    }

                    if ((repeatRuleMonthly.Ends != null) && (repeatRuleMonthly.Ends.Repetitions != null))
                    {
                        if ((repeatRuleMonthly.Ends.Repetitions < 1) || (repeatRuleMonthly.Ends.Repetitions > 12))
                        {
                            var ex = new IntegrationApiException("Validation exception");
                            ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleMonthly.Ends.Repetitions.Invalid", Message = "Ends.Repetitions must be between 1 and 12." });
                            throw ex;
                        }
                    }

                    if ((repeatRuleMonthly.Interval < 1) || (repeatRuleMonthly.Interval > 12))
                    {
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.RepeatRuleMonthly.Interval.Invalid", Message = "Interval must be between 1 and 12." });
                        throw ex;
                    }

                    break;
                case Dtos.FrequencyType2.Yearly:
                    break;
                default:
                    {// Integration API error
                        var ex = new IntegrationApiException("Validation exception");
                        ex.AddError(new Ellucian.Web.Http.Exceptions.IntegrationApiError() { Code = "RoomsAvailabilityRequest.Recurrence.Invalid", Message = "Unable to determine recurrence pattern." });
                        throw ex;
                    }
            }
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonSocialMediaDtoProperty>> GetPersonSocialMediaAsync(List<Ellucian.Colleague.Domain.Base.Entities.SocialMedia> mediaTypes)
        {
            List<Dtos.DtoProperties.PersonSocialMediaDtoProperty> socialMediaEntries = new List<Dtos.DtoProperties.PersonSocialMediaDtoProperty>();

            foreach (var mediaType in mediaTypes)
            {
                try
                {
                    var socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty();
                    if (mediaType.TypeCode.ToLowerInvariant() == "website")
                    {
                        string guid = "";
                        var socialMediaEntity = (await _referenceDataRepository.GetSocialMediaTypesAsync(false)).FirstOrDefault(ic => ic.Type.ToString() == mediaType.TypeCode);
                        if (socialMediaEntity != null)
                        {
                            guid = socialMediaEntity.Guid;
                            socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                            {
                                Type = new Dtos.DtoProperties.PersonSocialMediaType()
                                {
                                    Category = (Dtos.SocialMediaTypeCategory)Enum.Parse(typeof(Dtos.SocialMediaTypeCategory), mediaType.TypeCode.ToString()),
                                    Detail = new Dtos.GuidObject2(guid)
                                },
                                Address = mediaType.Handle
                            };
                        }
                        else
                        {
                            socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                            {
                                Type = new Dtos.DtoProperties.PersonSocialMediaType()
                                {
                                    Category = (Dtos.SocialMediaTypeCategory)Enum.Parse(typeof(Dtos.SocialMediaTypeCategory), mediaType.TypeCode.ToString())
                                },
                                Address = mediaType.Handle
                            };
                        }
                    }
                    else
                    {
                        var socialMediaEntity = (await _referenceDataRepository.GetSocialMediaTypesAsync(false)).FirstOrDefault(ic => ic.Code == mediaType.TypeCode);
                        socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                        {
                            Type = new Dtos.DtoProperties.PersonSocialMediaType()
                            {
                                Category = (Dtos.SocialMediaTypeCategory)Enum.Parse(typeof(Dtos.SocialMediaTypeCategory), socialMediaEntity.Type.ToString()),
                                Detail = new Dtos.GuidObject2(socialMediaEntity.Guid)
                            },
                            Address = mediaType.Handle
                        };
                    }
                    if (mediaType.IsPreferred) socialMedia.Preference = Dtos.EnumProperties.PersonPreference.Primary;

                    socialMediaEntries.Add(socialMedia);
                }
                catch
                {
                    // Do not include code since we couldn't find a category
                }
            }

            return socialMediaEntries;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonPhoneDtoProperty>> GetPhonesAsync(IEnumerable<Domain.Base.Entities.Phone> phoneEntities)
        {
            var phoneDtos = new List<Dtos.DtoProperties.PersonPhoneDtoProperty>();
            if (phoneEntities != null && phoneEntities.Any())
            {
                foreach (var phoneEntity in phoneEntities)
                {
                    string guid = "";
                    string category = "";
                    try
                    {
                        var phoneTypeEntity = (await _referenceDataRepository.GetPhoneTypesAsync(false)).FirstOrDefault(pt => pt.Code == phoneEntity.TypeCode);
                        guid = phoneTypeEntity.Guid;
                        category = phoneTypeEntity.PhoneTypeCategory.ToString();

                        var phoneDto = new Dtos.DtoProperties.PersonPhoneDtoProperty()
                        {
                            Number = phoneEntity.Number,
                            Extension = string.IsNullOrEmpty(phoneEntity.Extension) ? null : phoneEntity.Extension,
                            Type = new Dtos.DtoProperties.PersonPhoneTypeDtoProperty()
                            {
                                PhoneType = (Dtos.EnumProperties.PersonPhoneTypeCategory)Enum.Parse(typeof(Dtos.EnumProperties.PersonPhoneTypeCategory), category),
                                Detail = string.IsNullOrEmpty(guid) ? null : new Dtos.GuidObject2(guid)
                            },
                            CountryCallingCode = phoneEntity.CountryCallingCode
                        };
                        if (phoneEntity.IsPreferred) phoneDto.Preference = Dtos.EnumProperties.PersonPreference.Primary;

                        phoneDtos.Add(phoneDto);
                    }
                    catch
                    {
                        // do not fail if we can't find a guid from the code table or category
                        // Just exclude the phone number from the output.
                    }
                }
            }
            return phoneDtos;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonEmailDtoProperty>> GetEmailAddresses(IEnumerable<Domain.Base.Entities.EmailAddress> emailAddressEntities)
        {
            var emailAddressDtos = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
            if (emailAddressEntities != null && emailAddressEntities.Any())
            {
                foreach (var emailAddressEntity in emailAddressEntities)
                {
                    string guid = "";
                    string category = "";
                    try
                    {
                        var codeItem = (await _referenceDataRepository.GetEmailTypesAsync(false)).FirstOrDefault(pt => pt.Code == emailAddressEntity.TypeCode);
                        guid = codeItem.Guid;
                        category = codeItem.EmailTypeCategory.ToString();

                        var addressDto = new Dtos.DtoProperties.PersonEmailDtoProperty()
                        {
                            Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty()
                            {
                                EmailType = (Dtos.EmailTypeList)Enum.Parse(typeof(Dtos.EmailTypeList), category),
                                Detail = string.IsNullOrEmpty(guid) ? null : new Dtos.GuidObject2(guid)
                            },
                            Address = emailAddressEntity.Value
                        };
                        if (emailAddressEntity.IsPreferred) addressDto.Preference = Dtos.EnumProperties.PersonEmailPreference.Primary;

                        emailAddressDtos.Add(addressDto);
                    }
                    catch
                    {
                        // do not fail if we can't find a guid from the code table or translate the cateory
                        // Just exclude this email address.
                    }
                }
            }
            return emailAddressDtos;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonAddressDtoProperty>> GetAddressesAsync(IEnumerable<Domain.Base.Entities.Address> addressEntities)
        {
            var addressDtos = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
            if (addressEntities != null && addressEntities.Any())
            {
                foreach (var addressEntity in addressEntities)
                {
                    if (addressEntity != null)
                    {
                        var addressDto = new Dtos.DtoProperties.PersonAddressDtoProperty();
                        addressDto.address = new Dtos.PersonAddress() { Id = addressEntity.Guid };
                        var addressTypes = await _referenceDataRepository.GetAddressTypes2Async(false);
                        var type = addressTypes.FirstOrDefault(at => at.Code == addressEntity.TypeCode);
                        if (type != null)
                        {
                            addressDto.Type = new Dtos.DtoProperties.PersonAddressTypeDtoProperty();
                            addressDto.Type.AddressType = (Dtos.EnumProperties.AddressType)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), type.AddressTypeCategory.ToString());
                            addressDto.Type.Detail = new Dtos.GuidObject2(type.Guid);
                        }
                        else
                        {
                            throw new Exception(string.Concat("Address with id: ", addressEntity.Guid, " does not have a type set."));
                        }

                        if (addressEntity.IsPreferredResidence) addressDto.Preference = Dtos.EnumProperties.PersonPreference.Primary;
                        addressDto.AddressEffectiveStart = addressEntity.EffectiveStartDate;
                        addressDto.AddressEffectiveEnd = addressEntity.EffectiveEndDate;

                        //if (addressEntity.SeasonalDates != null)
                        //{
                        //    addressDto.SeasonalOccupancies = new List<Dtos.DtoProperties.PersonAddressRecurrenceDtoProperty>();
                        //    int year = DateTime.Today.Year;
                        //    foreach (var assocEntity in addressEntity.SeasonalDates)
                        //    {
                        //        try
                        //        {
                        //            int startMonth = int.Parse(assocEntity.StartOn.Split("/".ToCharArray())[0]);
                        //            int startDay = int.Parse(assocEntity.StartOn.Split("/".ToCharArray())[1]);
                        //            int endMonth = int.Parse(assocEntity.EndOn.Split("/".ToCharArray())[0]);
                        //            int endDay = int.Parse(assocEntity.EndOn.Split("/".ToCharArray())[1]);

                        //            var recurrence = new Dtos.Recurrence3()
                        //            {
                        //                TimePeriod = new Dtos.RepeatTimePeriod2()
                        //                {
                        //                    StartOn = new DateTime(year, startMonth, startDay),
                        //                    EndOn = new DateTime(year, endMonth, endDay)
                        //                }
                        //            };
                        //            recurrence.RepeatRule = new Dtos.RepeatRuleDaily()
                        //            {
                        //                Type = Dtos.FrequencyType2.Daily,
                        //                Interval = 1,
                        //                Ends = new Dtos.RepeatRuleEnds() { Date = new DateTime(year, endMonth, endDay) }
                        //            };
                        //            addressDto.SeasonalOccupancies.Add(new Dtos.DtoProperties.PersonAddressRecurrenceDtoProperty() { Recurrence = recurrence });
                        //        }
                        //        catch
                        //        {
                        //            // Invalid seasonal start or end dates, just ignore and don't include
                        //        }
                        //    }
                        //}

                        addressDtos.Add(addressDto);
                    }
                }
            }
            return addressDtos;
        }

        private IEnumerable<Domain.Base.Entities.Country> _countries = null;
        private async Task<IEnumerable<Domain.Base.Entities.Country>> GetAllCountriesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await _referenceDataRepository.GetCountryCodesAsync(bypassCache);
            }
            return _countries;
        }

        private IEnumerable<Domain.Base.Entities.State> _states = null;
        private async Task<IEnumerable<Domain.Base.Entities.State>> GetAllStatesAsync(bool bypassCache)
        {
            if (_states == null)
            {
                _states = await _referenceDataRepository.GetStateCodesAsync(bypassCache);
            }
            return _states;
        }

        /// <summary>
        /// Get host country
        /// </summary>
        private string _hostCountry;
        private async Task<string> GetHostCountry()
        {
            if (_hostCountry == null)
            {
                _hostCountry = await _referenceDataRepository.GetHostCountryAsync();
            }
            return _hostCountry;
        }
    }
}