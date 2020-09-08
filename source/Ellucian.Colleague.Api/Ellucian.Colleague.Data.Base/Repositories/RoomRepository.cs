// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using System.Text;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class RoomRepository : BaseColleagueRepository, IRoomRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public RoomRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get a collection of rooms
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of rooms</returns>
        public async Task<IEnumerable<Room>> GetRoomsAsync(bool ignoreCache)
        {
            string cacheKey = "AllRooms_GUID";

            // modify the cache key when anonymous to prevent an anonymous user from accessing non-anonymous data via the cache
            if (this.DataReader.IsAnonymous)
            {
                cacheKey = string.Join("_", cacheKey, "Anonymous");
            }

            var roomList = new List<Room>();
            if (ignoreCache)
            {
                roomList = await BuildRoomListAsync();
                await AddOrUpdateCacheAsync<IEnumerable<Room>>(cacheKey, roomList);
                return roomList;
            }

            // Get the codes from the cache if present; otherwise, read from Colleague
            var codes = await GetOrAddToCacheAsync<IEnumerable<Room>>(cacheKey,
                async () =>
                {
                    return await BuildRoomListAsync();
                }
            );

            return codes;
        }

        /// <summary>
        /// Rooms
        /// </summary>
        public async Task<IEnumerable<Room>> RoomsAsync()
        {
            return await GetRoomsAsync(false);

        }

        /// <summary>
        /// Get guid for Room code
        /// </summary>
        /// <param name="code">Room code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetRoomsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetRoomsAsync(false);
            Room codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Id.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetRoomsAsync(true);
                if (allCodesNoCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ROOMS', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Id.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ROOMS', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'ROOMS', Record ID:'", code, "'"));
            }
            return guid;

        }

        /// <summary>
        /// Get a collection of rooms
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of rooms</returns>
        public async Task<Tuple<IEnumerable<Room>, int>> GetRoomsWithPagingAsync(int offset, int limit, bool ignoreCache)
        {
            var rooms = new List<Room>();
            var roomsIds = await DataReader.SelectAsync("ROOMS", string.Empty);
            var totalCount = 0;
            totalCount = roomsIds.Count();

            if (totalCount == 0)
            {
                return new Tuple<IEnumerable<Room>, int>(rooms, 0);
            }

            Array.Sort(roomsIds);
            var sublist = roomsIds.Skip(offset).Take(limit).ToArray();

            var roomDataContract =
                await DataReader.BulkReadRecordAsync<Rooms>("ROOMS", sublist);

            foreach (var room in roomDataContract)
            {
                try
                {
                    rooms.Add(BuildRoom(room));
                }
                catch (ArgumentException ex)
                {
                    LogDataError("Room", room.Recordkey, room, ex);
                    throw new ArgumentException(string.Format("Guid: {0}, Room ID must contain a building code, an asterisk, and a room number. Room ID: {1}", room.RecordGuid, room.Recordkey));
                }
                catch (Exception ex)
                {
                    LogDataError("Room", room.Recordkey, room, ex);
                    throw new Exception(ex.Message);
                }
            }

            return rooms.Any() ?
                new Tuple<IEnumerable<Room>, int>(rooms, totalCount) :
                new Tuple<IEnumerable<Room>, int>(rooms, 0);

        }

        /// <summary>
        /// Get a collection of rooms
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of rooms</returns>
        public async Task<Tuple<IEnumerable<Room>, int>> GetRoomsWithPaging2Async(int offset, int limit, bool ignoreCache, string buildingCode = "", string roomTypeCode = "")
        {
            var rooms = new List<Room>();
            StringBuilder selectionCriteria = new StringBuilder();

            if (!string.IsNullOrEmpty(buildingCode))
            {
                selectionCriteria.Append("WITH ROOMS.BLDG.ID = '");
                selectionCriteria.Append(buildingCode);
                selectionCriteria.Append("'");
            }
            if (!string.IsNullOrEmpty(roomTypeCode))
            {
                if (!string.IsNullOrEmpty(selectionCriteria.ToString()))
                {
                    selectionCriteria.Append(" AND ");
                }
                selectionCriteria.Append("WITH ROOM.TYPE = ");
                selectionCriteria.Append(roomTypeCode);
                //selectionCriteria.Append("'");
            }

            var roomsIds = await DataReader.SelectAsync("ROOMS", selectionCriteria != null ? selectionCriteria.ToString() : string.Empty);
            var totalCount = 0;
            totalCount = roomsIds.Count();

            if (totalCount == 0)
            {
                return new Tuple<IEnumerable<Room>, int>(rooms, 0);
            }

            Array.Sort(roomsIds);
            var sublist = roomsIds.Skip(offset).Take(limit).ToArray();

            var roomDataContract =
                await DataReader.BulkReadRecordAsync<Rooms>("ROOMS", sublist);

            foreach (var room in roomDataContract)
            {
                try
                {
                    rooms.Add(BuildRoom(room));
                }
                catch (ArgumentException ex)
                {
                    LogDataError("Room", room.Recordkey, room, ex);
                    throw new ArgumentException(string.Format("Guid: {0}, Room ID must contain a building code, an asterisk, and a room number. Room ID: {1}", room.RecordGuid, room.Recordkey));
                }
                catch (Exception ex)
                {
                    LogDataError("Room", room.Recordkey, room, ex);
                    throw new Exception(ex.Message);
                }
            }

            return rooms.Any() ?
                new Tuple<IEnumerable<Room>, int>(rooms, totalCount) :
                new Tuple<IEnumerable<Room>, int>(rooms, 0);

        }

        private Room BuildRoom(Rooms source)
        {
            var room = new Room(source.RecordGuid, source.Recordkey, source.RoomName)
            {
                Floor = source.RoomFloor,
                Name = source.RoomName,
                Capacity = source.RoomCapacity.GetValueOrDefault(),
                RoomType = source.RoomType,
                Wing = source.RoomWing,
                Characteristics = source.RoomCharacteristics
            };

            return room;
        }


        /// <summary>
        /// Take the collection of Rooms records returned from Colleague and build a list of Room entities
        /// </summary>
        /// <param name="rooms">Collection of rooms</param>
        /// <returns>List of Room entities</returns>
        private async Task<List<Room>> BuildRoomListAsync()
        {
            var rooms = await DataReader.BulkReadRecordAsync<Rooms>("");
            if (rooms == null || rooms.Count == 0)
            {
                throw new KeyNotFoundException("No rooms were found.");
            }

            List<Room> roomList = new List<Room>();
            foreach (var source in rooms)
            {
                try
                {
                    var room = new Room(source.RecordGuid, source.Recordkey, source.RoomName)
                    {
                        Floor = source.RoomFloor,
                        Name = source.RoomName,
                        Capacity = source.RoomCapacity.GetValueOrDefault(),
                        RoomType = source.RoomType,
                        Wing = source.RoomWing,
                        Characteristics = source.RoomCharacteristics
                    };
                    roomList.Add(room);
                }
                catch (Exception ex)
                {
                    LogDataError("Room", source.Recordkey, source, ex);
                }
            }
            return roomList;
        }
    }
}