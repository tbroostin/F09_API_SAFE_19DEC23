//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentAdvisorRelationships services
    /// </summary>
    public interface IStudentAdvisorRelationshipsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAdvisorRelationships>, int>> GetStudentAdvisorRelationshipsAsync(int offset, int limit, bool bypassCache = false, string student = "", string advisor = "" , string advisorType = "", string startAcademicPeriod = "");

        Task<Ellucian.Colleague.Dtos.StudentAdvisorRelationships> GetStudentAdvisorRelationshipsByGuidAsync(string id);
    }
}