// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IAddressTypeService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AddressType2>> GetAddressTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.AddressType2> GetAddressTypeByGuidAsync(string guid);
    }
}
