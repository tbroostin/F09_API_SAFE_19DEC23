﻿// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Address services
    /// </summary>
    public interface IAddressService : IBaseService
    {
        Task<Dtos.Addresses> GetAddressesByGuidAsync(string guid);
        Task<Dtos.Addresses> GetAddressesByGuid2Async(string guid);

        Task<Tuple<IEnumerable<Dtos.Addresses>, int>> GetAddressesAsync(int offset, int limit, bool bypassCache = false);
        Task<Tuple<IEnumerable<Dtos.Addresses>, int>> GetAddresses2Async(int offset, int limit, string personFilter, bool bypassCache = false);

        Task<Dtos.Addresses> PutAddressesAsync(string id, Dtos.Addresses addressDto);
        Task<Dtos.Addresses> PutAddresses2Async(string id, Dtos.Addresses addressDto);

        //Task<Dtos.Addresses> PostAddressesAsync(Dtos.Addresses address);

        Task DeleteAddressesAsync(string id);

        Task QueryAddressPermissionAsync(IEnumerable<string> personIds);
    }
}
