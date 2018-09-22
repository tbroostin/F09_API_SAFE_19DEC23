// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentAcademicPeriodProfilesService : IBaseService
    {
        Task<Dtos.StudentAcademicPeriodProfiles> GetStudentAcademicPeriodProfileByGuidAsync(string guid);

        Task<Tuple<IEnumerable<Dtos.StudentAcademicPeriodProfiles>, int>> GetStudentAcademicPeriodProfilesAsync(int offset, int limit, bool bypassCache = false, 
            string person = "", string academicPeriod = "");
       
    }
}