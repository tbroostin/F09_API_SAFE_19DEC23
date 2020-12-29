// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentAcademicProgramRepository : IEthosExtended
    {
        Task<StudentAcademicProgram> GetStudentAcademicProgramByGuidAsync(string id, string defaultInstitutionId);
        Task<StudentAcademicProgram> GetStudentAcademicProgramByGuid2Async(string id, string defaultInstitutionId,  bool includeAcadCredentials = true);
        Task<StudentAcademicProgram> CreateStudentAcademicProgramAsync(StudentAcademicProgram acadProgEnroll, string defaultInstitutionId);
        Task<StudentAcademicProgram> UpdateStudentAcademicProgramAsync(StudentAcademicProgram acadProgEnroll, string defaultInstitutionId);

        Task<StudentAcademicProgram> CreateStudentAcademicProgram2Async(StudentAcademicProgram acadProgEnroll, string defaultInstitutionId);
        Task<StudentAcademicProgram> UpdateStudentAcademicProgram2Async(StudentAcademicProgram acadProgEnroll, string defaultInstitutionId);

        Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms2Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false,
        string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "", string programOwner = "", string site = "",
        string academicLevel = "", string graduatedOn = "", List<string> ccdCredential = null, List<string> degreeCredential = null, string graduatedAcademicPeriod = "",
        string completeStatus = "");

        Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms3Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false,
            string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "", string programOwner = "", string site = "",
            string academicLevel = "", string graduatedOn = "", List<string> ccdCredential = null, List<string> degreeCredential = null, string graduatedAcademicPeriod = "",
            string completeStatus = "", CurriculumObjectiveCategory curriculumObjective = CurriculumObjectiveCategory.NotSet, 
            bool includeAcadCredentials = true);

        Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms4Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false,
           string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "", string programOwner = "", string site = "",
           string academicLevel = "", string graduatedOn = "", List<string> ccdCredential = null, List<string> degreeCredential = null, string graduatedAcademicPeriod = "",
           string completeStatus = "", CurriculumObjectiveCategory curriculumObjective = CurriculumObjectiveCategory.NotSet,
           bool includeAcadCredentials = true);

        Task<string> GetUnidataFormattedDate(string date);

        Task<Dictionary<string, string>> GetStudentAcademicProgramGuidsCollectionAsync(IEnumerable<string> ids);

        Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicProgramsPersonFilterAsync(int offset, int limit,
           string[] filterPersonIds = null, string personFilter = "", bool bypassCache = false);

        Task<string> GetStudentAcademicProgramIdFromGuidAsync(string guid);
    }
}