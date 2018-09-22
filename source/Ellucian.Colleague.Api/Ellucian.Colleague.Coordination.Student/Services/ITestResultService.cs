// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface ITestResultService
    {
        [Obsolete("Obsolete on version 1.15 of the Api. Use Get2Async.")]
        Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult>> GetAsync(string studentId, string testType);
        Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult2>> Get2Async(string studentId, string testType);
        [Obsolete("Obsolete on version 1.15 of the Api. Use GetTestResults2ByIdsAsync.")]
        Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult>> GetTestResultsByIdsAsync(string[] stncIds, string testType);
        Task<IEnumerable<Ellucian.Colleague.Dtos.Student.TestResult2>> GetTestResults2ByIdsAsync(string[] stncIds, string testType);
    }
}
