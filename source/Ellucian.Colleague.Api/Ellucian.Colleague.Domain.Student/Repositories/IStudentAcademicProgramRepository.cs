// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
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
        Task<StudentAcademicProgram> CreateStudentAcademicProgramAsync(StudentAcademicProgram acadProgEnroll, string defaultInstitutionId);
        Task<StudentAcademicProgram> UpdateStudentAcademicProgramAsync(StudentAcademicProgram acadProgEnroll, string defaultInstitutionId);
        //Task<Tuple<IEnumerable<StudentAcademicProgram>, int>>GetStudentAcademicProgramsAsync(string defaultInstitutionId, int offset, int limit, bool bypassCache = false,
        //    string Program = "", string StartOn = "", string EndOn = "", string Student ="", string Catalog ="", string Status="", string programOwner = "", string site = "", 
        //    string academicLevel = "", string graduatedOn = "", string ccdCredential = "", string degreeCredential = "", string graduatedAcademicPeriod = "", 
        //    string completeStatus = "");

        Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms2Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false,
            string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "", string programOwner = "", string site = "",
            string academicLevel = "", string graduatedOn = "", List<string> ccdCredential = null, List<string> degreeCredential = null, string graduatedAcademicPeriod = "", 
            string completeStatus = "");
        Task<string> GetUnidataFormattedDate(string date);
    }
}
