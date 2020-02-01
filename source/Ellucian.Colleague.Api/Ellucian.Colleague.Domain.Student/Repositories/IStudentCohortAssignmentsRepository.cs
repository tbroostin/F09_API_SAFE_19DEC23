// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentCohortAssignmentsRepository
    {
        Task<Tuple<IEnumerable<StudentCohortAssignment>, int>> GetStudentCohortAssignmentsAsync(int offset, int limit, StudentCohortAssignment criteriaObj = null, Dictionary<string, string> filterQualifiers = null);
        Task<StudentCohortAssignment> GetStudentCohortAssignmentByIdAsync(string id);
    }
}
