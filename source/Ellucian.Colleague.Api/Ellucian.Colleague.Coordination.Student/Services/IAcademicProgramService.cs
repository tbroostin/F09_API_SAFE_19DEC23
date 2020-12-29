using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Student;
// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IAcademicProgramService : IBaseService
    {
        /// <summary>
        /// Gets all academic programs
        /// </summary>
        /// <returns>Collection of Academic Programs DTO objects</returns>
        Task<IEnumerable<AcademicProgram>> GetAsync();

        /// <remarks>FOR USE WITH ELLUCIAN HeDM 6 </remarks>
        /// <summary>
        /// Gets all academic programs
        /// </summary>
        /// <returns>Collection of Academic Programs DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicProgram2>> GetAcademicProgramsV6Async(bool bypassCache);

        /// <remarks>FOR USE WITH ELLUCIAN HeDM 6</remarks>
        /// <summary>
        /// Get an academic period from its GUID
        /// </summary>
        /// <returns>Academic Period DTO object</returns>
        Task<Ellucian.Colleague.Dtos.AcademicProgram2> GetAcademicProgramByGuidV6Async(string guid);

        /// <remarks>FOR USE WITH ELLUCIAN HeDM 10 </remarks>
        /// <summary>
        /// Gets all academic programs
        /// </summary>
        /// <returns>Collection of Academic Programs DTO objects</returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicProgram3>> GetAcademicPrograms3Async(bool bypassCache);

        /// <remarks>FOR USE WITH ELLUCIAN HeDM 10</remarks>
        /// <summary>
        /// Get an academic period from its GUID
        /// </summary>
        /// <returns>Academic Period DTO object</returns>
        Task<Ellucian.Colleague.Dtos.AcademicProgram3> GetAcademicProgramByGuid3Async(string guid);

        /// <remarks>FOR USE WITH ELLUCIAN EEMD </remarks>
        /// <summary>
        /// Gets all academic-programs
        /// </summary>
         /// <param name="academicCatalog">academicCatalog guid from filter</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="Dtos.AcademicProgram4">academicPrograms</see> objects</returns>          
        Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicProgram4>> GetAcademicPrograms4Async(string academicCatalog = "", string recruitmentProgram = "", Dtos.AcademicProgram4 criteria = null, bool bypassCache = false);
        
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an academic period from its GUID
        /// </summary>
        /// <returns>Academic Period DTO object</returns>
        Task<Ellucian.Colleague.Dtos.AcademicProgram4> GetAcademicProgramByGuid4Async(string guid);

    }
}
