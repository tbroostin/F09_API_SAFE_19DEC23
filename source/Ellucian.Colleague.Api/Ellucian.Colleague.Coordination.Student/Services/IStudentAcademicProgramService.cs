// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IStudentAcademicProgramService : IBaseService
    {
        Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms> GetStudentAcademicProgramByGuidAsync(string guid);
        Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms> CreateStudentAcademicProgramAsync(Ellucian.Colleague.Dtos.StudentAcademicPrograms acadProgEnroll);
        Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms> UpdateStudentAcademicProgramAsync(Ellucian.Colleague.Dtos.StudentAcademicPrograms acadProgEnroll);
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPrograms>, int>> GetStudentAcademicProgramsAsync(int offset, int limit, bool bypassCache = false, string student = "", 
            string startOn = "", string endOn = "", string program = "", string catalog = "", string enrollmentStatus = "", string programOwner = "", string site = "", string academicLevel = "", string graduatedOn = "",
            string credential = "", string graduatedAcademicPeriod = "", string completeStatus = "");

        Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms2> GetStudentAcademicProgramByGuid2Async(string guid);
        Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms2> CreateStudentAcademicProgram2Async(Ellucian.Colleague.Dtos.StudentAcademicPrograms2 acadProgEnroll);
        Task<Ellucian.Colleague.Dtos.StudentAcademicPrograms2> UpdateStudentAcademicProgram2Async(Ellucian.Colleague.Dtos.StudentAcademicPrograms2 acadProgEnroll);
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAcademicPrograms2>, int>> GetStudentAcademicPrograms2Async(int offset, int limit, bool bypassCache = false, string student = "",
            string startOn = "", string endOn = "", string program = "", string enrollmentStatus = "", string site = "", string academicLevel = "", string graduatedOn = "", List<string> credential = null, string graduatedAcademicPeriod = "");
    }
}
