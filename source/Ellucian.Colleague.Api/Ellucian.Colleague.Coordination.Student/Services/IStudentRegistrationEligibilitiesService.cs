//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentRegistrationEligibilities services
    /// </summary>
    public interface IStudentRegistrationEligibilitiesService : IBaseService
    {

        Task<Ellucian.Colleague.Dtos.StudentRegistrationEligibilities> GetStudentRegistrationEligibilitiesAsync(string studentId, string academicPeriodId, bool bypassCache = false);
    }
}
