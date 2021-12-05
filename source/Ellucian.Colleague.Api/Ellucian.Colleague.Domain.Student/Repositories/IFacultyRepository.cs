// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IFacultyRepository
    {
        Task<Faculty> GetAsync(string id);
        [Obsolete("Deprecated on version 1.2 of the Api. Use Get for a single id instead going forward.")]
        Task<IEnumerable<Faculty>> GetAsync(IEnumerable<string> ids);
        Task<IEnumerable<Faculty>> GetFacultyByIdsAsync(IEnumerable<string> facultyIds);
        Task<IEnumerable<string>> SearchFacultyIdsAsync(bool facultyOnlyFlag = false, bool advisorOnlyFlag = true);
        Task<IEnumerable<Domain.Student.Entities.FacultyOfficeHours>> GetFacultyOfficeHoursByIdsAsync(IEnumerable<string> facultyIds);
        Task<FacultyDropRegistrationPermissions> GetFacultyRegistrationEligibilityAsync(string personId);
    }
}
