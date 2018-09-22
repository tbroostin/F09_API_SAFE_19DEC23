using Ellucian.Colleague.Dtos.Base;
// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IStaffService
    {
        Task<IEnumerable<PersonRestriction>> GetStaffRestrictionsAsync(string staffId);

        Task<Staff> GetAsync(string staffId);
    }
}
