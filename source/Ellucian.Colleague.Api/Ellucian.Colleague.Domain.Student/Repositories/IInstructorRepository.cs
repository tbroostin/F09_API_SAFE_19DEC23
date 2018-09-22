//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IPersoRepository
    {
        Task<Tuple<IEnumerable<Instructor>, int>> GetInstructorsAsync(int offset, int limit, string instructor, string primaryLocation, bool bypassCache);
        Task<Instructor> GetInstructorByIdAsync(string guid);
        Task<Dictionary<string, string>> GetPersonGuidsAsync(IEnumerable<string> personRecordKeys);
        Task<IEnumerable<Domain.Student.Entities.TenureTypes>> GetTenureTypesAsync(bool ignoreCache);
    }
}
