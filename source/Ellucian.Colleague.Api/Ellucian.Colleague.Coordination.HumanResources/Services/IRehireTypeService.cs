/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Base.Services;
using System;
namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IRehireTypeService : IBaseService 
    {
        System.Threading.Tasks.Task<Ellucian.Colleague.Dtos.RehireType> GetRehireTypeByGuidAsync(string guid);
        System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Ellucian.Colleague.Dtos.RehireType>> GetRehireTypesAsync(bool bypassCache = false);
    }
}
