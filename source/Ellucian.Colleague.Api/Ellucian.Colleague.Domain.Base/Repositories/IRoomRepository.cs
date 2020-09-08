// Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for rooms
    /// </summary>
    public interface IRoomRepository
    {
        /// <summary>
        /// Rooms
        /// </summary>
        Task<IEnumerable<Room>> RoomsAsync();

        /// <summary>
        /// Get a collection of rooms
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of rooms</returns>
        Task<IEnumerable<Room>> GetRoomsAsync(bool ignoreCache = false);


        /// <summary>
        /// Get guid for Room code
        /// </summary>
        /// <param name="code">Room code</param>
        /// <returns>Guid</returns>
        Task<string> GetRoomsGuidAsync(string code);

        /// <summary>
        /// Get a collection of rooms
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of rooms</returns>
        Task<Tuple<IEnumerable<Room>, int>> GetRoomsWithPagingAsync(int offset, int limit, bool ignoreCache = false);

        /// <summary>
        /// Get a collection of rooms
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of rooms</returns>
        Task<Tuple<IEnumerable<Room>, int>> GetRoomsWithPaging2Async(int offset, int limit, bool ignoreCache = false, string building = "", string roomType = "");
    }
}
