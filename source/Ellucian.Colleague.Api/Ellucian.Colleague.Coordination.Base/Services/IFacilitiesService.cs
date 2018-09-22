// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IFacilitiesService : IBaseService
    {
        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all sites
        /// </summary>
        /// <returns>Collection of Site DTO objects</returns>
       Task<IEnumerable<Ellucian.Colleague.Dtos.Site2>> GetSites2Async(bool bypassCache);

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get a site from its ID
        /// </summary>
        /// <returns>Site DTO object</returns>
        Task<Ellucian.Colleague.Dtos.Site2> GetSite2Async(string id);

        /// <summary>
        /// Gets all buildings
        /// </summary>
        /// <returns>Collection of Building DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Building2>> GetBuildings2Async(bool bypassCache);

        /// <summary>
        /// Gets all buildings
        /// </summary>
        /// <returns>Collection of Building DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Building3>> GetBuildings3Async(bool bypassCache, string mapVisibility);

        /// <summary>
        /// Get a building from its GUID
        /// </summary>
        /// <returns>Building DTO object</returns>
        Task<Ellucian.Colleague.Dtos.Building2> GetBuilding2Async(string guid);

        /// <summary>
        /// Get a building from its GUID
        /// </summary>
        /// <returns>Building DTO object</returns>
        Task<Ellucian.Colleague.Dtos.Building3> GetBuilding3Async(string guid);

        /// <summary>
        /// Gets all building wings
        /// </summary>
        /// <returns>Collection of Building Wing DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.BuildingWing>> GetBuildingWingsAsync(bool bypassCache);

        /// <summary>
        /// Get a building wing from its GUID
        /// </summary>
        /// <returns>Building DTO object</returns>
        Task<Ellucian.Colleague.Dtos.BuildingWing> GetBuildingWingsByGuidAsync(string guid);
        
        /// <summary>
        /// Gets all rooms
        /// </summary>
        /// <returns>Collection of Room DTO objects</returns>
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Room3>, int>> GetRooms3Async(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Gets all rooms
        /// </summary>
        /// <returns>Collection of Room DTO objects</returns>
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Room3>, int>> GetRooms4Async(int offset, int limit, bool bypassCache = false, string building = "", string roomTypes = "");
        
        /// <summary>
        /// Get a room from its GUID
        /// </summary>
        /// <returns>Room DTO object</returns>
        Task<Ellucian.Colleague.Dtos.Room3> GetRoomById3Async(string guid);
        
        /// <summary>
        /// Check for room availability for a given date range, start and end time, and frequency
        /// </summary>
        /// <param name="request">Room availability request</param>
        /// <returns>Collection of Room DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Room2>> CheckRoomAvailability3Async(Dtos.RoomsAvailabilityRequest2 request);

        /// <summary>
        /// Check for room availability for a given date range, start and end time, and frequency
        /// </summary>
        /// <param name="request">Room availability request</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of Room DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Room3>> CheckRoomAvailability4Async(Dtos.RoomsAvailabilityRequest3 request, bool bypassCache = false);


        /// <summary>
        /// Check for room availability for a given date range, start and end time, and frequency, and return minimal room object 
        /// </summary>
        /// <returns>RoomsMinimumResponse DTO object</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.RoomsMinimumResponse>> GetRoomsMinimumAsync(Dtos.RoomsAvailabilityRequest2 request);  
    }
}