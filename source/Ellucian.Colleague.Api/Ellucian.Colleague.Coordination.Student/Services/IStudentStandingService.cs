// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentStandingService
    {
        Task<IEnumerable<Dtos.Student.StudentStanding>> GetAsync(IEnumerable<string> studentIds, string term = null, string currentTerm = null);
    }
}
