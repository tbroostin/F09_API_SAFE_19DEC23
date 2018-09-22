// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonNameTypeService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.PersonNameTypeItem>> GetPersonNameTypesAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.PersonNameTypeItem> GetPersonNameTypeByIdAsync(string id);
        Task<IEnumerable<PersonNameTypeItem>> GetPersonNameTypes2Async(bool bypassCache);
        Task<PersonNameTypeItem> GetPersonNameTypeById2Async(string id);
    }
}