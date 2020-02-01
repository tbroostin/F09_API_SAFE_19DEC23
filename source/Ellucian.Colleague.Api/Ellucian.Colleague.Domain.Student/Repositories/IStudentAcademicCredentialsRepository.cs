// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for AcademicCredentials Repository
    /// </summary>
    public interface IStudentAcademicCredentialsRepository
    {
        Task<Tuple<IEnumerable<StudentAcademicCredential>, int>> GetStudentAcademicCredentialsAsync(int offset, int limit, StudentAcademicCredential criteriaEntity, 
            string[] filterPersonIds, string acadProgramFilter, Dictionary<string, string> filterQualifiers);
        Task<StudentAcademicCredential> GetStudentAcademicCredentialByGuidAsync(string guid);

        Task<Tuple<string, string, string>> GetAcadCredentialKeyAsync(string guid);
    }
}