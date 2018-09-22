// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonHoldTypeService: IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.PersonHoldType>> GetPersonHoldTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.PersonHoldType> GetPersonHoldTypeByGuid2Async(string id);
    }
}
