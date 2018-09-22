// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonRestrictionTypeService
    {
        Task<List<Dtos.GuidObject>> GetActivePersonRestrictionTypesAsync(string guid);
    }
}
