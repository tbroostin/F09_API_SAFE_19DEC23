// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface ITestResultRepository
    {
        Task<IEnumerable<TestResult>> GetAsync(string studentId);
        Task<IEnumerable<TestResult>> GetTestResultsByIdsAsync(string[] stncIds);
    }
}
