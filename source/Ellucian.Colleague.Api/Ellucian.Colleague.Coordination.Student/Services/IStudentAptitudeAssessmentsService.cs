//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for StudentAptitudeAssessments services
    /// </summary>
    public interface IStudentAptitudeAssessmentsService : IBaseService
    {   
        /// <summary>
        /// Gets all student-aptitude-assessments
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>, int>> GetStudentAptitudeAssessmentsAsync(int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Gets all student-aptitude-assessments
        /// </summary>
        /// <param name="studentFilter">student ID for filtering</param>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments>, int>> GetStudentAptitudeAssessments2Async(string studentFilter, int offset, int limit, bool bypassCache = false);


        /// <summary>
        /// Gets all student-aptitude-assessments
        /// </summary>
        /// <param name="studentFilter">student ID for filtering</param>
        /// <param name="assessmentFilter">assessment ID for filtering</param>
        ///  <param name="personFilter">personFilter ID for named query filtering</param>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> objects</returns>          
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2>, int>> GetStudentAptitudeAssessments3Async(string studentFilter, 
            string assessmentFilter, string personFilter, int offset, int limit, bool bypassCache = false);

        /// <summary>
        /// Get a studentAptitudeAssessments by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAptitudeAssessments in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments> GetStudentAptitudeAssessmentsByGuidAsync(string guid, bool bypassCache = true);

        /// <summary>
        /// Get a studentAptitudeAssessments by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAptitudeAssessments in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments> GetStudentAptitudeAssessmentsByGuid2Async(string guid, bool bypassCache = true);

        /// <summary>
        /// Get a studentAptitudeAssessments by guid.
        /// </summary>
        /// <param name="guid">Guid of the studentAptitudeAssessments in Colleague.</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2> GetStudentAptitudeAssessmentsByGuid3Async(string guid, bool bypassCache = true);


        /// <summary>
        /// Update a studentAptitudeAssessments.
        /// </summary>
        /// <param name="studentAptitudeAssessments">The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments> UpdateStudentAptitudeAssessmentsAsync(Ellucian.Colleague.Dtos.StudentAptitudeAssessments studentAptitudeAssessments);


        /// <summary>
        /// Update a studentAptitudeAssessments.
        /// </summary>
        /// <param name="studentAptitudeAssessments">The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments2</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="StudentAptitudeAssessments2">studentAptitudeAssessments</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2> UpdateStudentAptitudeAssessments2Async(Ellucian.Colleague.Dtos.StudentAptitudeAssessments2 studentAptitudeAssessments);


        /// <summary>
        /// Create a studentAptitudeAssessments.
        /// </summary>
        /// <param name="studentAptitudeAssessments">The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="StudentAptitudeAssessments">studentAptitudeAssessments</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments> CreateStudentAptitudeAssessmentsAsync(Ellucian.Colleague.Dtos.StudentAptitudeAssessments studentAptitudeAssessments);


        /// <summary>
        /// Create a studentAptitudeAssessments.
        /// </summary>
        /// <param name="studentAptitudeAssessments">The <see cref="StudentAptitudeAssessments">studentAptitudeAssessments2</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="StudentAptitudeAssessments2">studentAptitudeAssessments</see></returns>
        Task<Ellucian.Colleague.Dtos.StudentAptitudeAssessments2> CreateStudentAptitudeAssessments2Async(Ellucian.Colleague.Dtos.StudentAptitudeAssessments2 studentAptitudeAssessments);

        /// <summary>
        /// Deletes student aptitude assessment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteStudentAptitudeAssessmentAsync(string id);
    }
}
