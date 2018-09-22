// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentRestrictionService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetStudentRestrictionsAsync(string studentId, bool useCache = true);
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetStudentRestrictions2Async(string studentId, bool useCache = true);
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetStudentRestrictionsByIdsAsync(IEnumerable<string> ids);
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetStudentRestrictionsByStudentIdsAsync(IEnumerable<string> studentIds);
    }
}
