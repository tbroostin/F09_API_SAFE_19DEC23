// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Student;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Course services
    /// </summary>
    public interface ICourseService : IBaseService
    {
        Task<IEnumerable<Course2>> GetCourses2Async(CourseQueryCriteria criteria);

        Task<IEnumerable<Ellucian.Colleague.Dtos.Course2>> GetCourses2Async(bool bypassCache);

        [Obsolete("Obsolete as of API version 1.3. Use the latest version of this method.")]
        /// <summary>
        /// OBSOLETE AS OF API VERSION 1.3. REPLACED BY GetSections2
        /// Gets all sections that are open for preregistration or registration for the course Ids specified.
        /// </summary>
        /// <param name="courseids">String of course Ids separated by commas</param>
        /// <returns>IEnumerable list of SectionDTOs</returns>
        Task<PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section>>> GetSectionsAsync(IEnumerable<string> courseIds, bool useCache);

        [Obsolete("Obsolete as of API version 1.5. Use the latest version of this method.")]
        /// <summary>
        /// Gets all sections that are open for preregistration or registration for the course Ids specified.
        /// </summary>
        /// <param name="courseids">String of course Ids separated by commas</param>
        /// <returns>IEnumerable list of SectionDTOs</returns>
        Task<PrivacyWrapper<IEnumerable<Section2>>> GetSections2Async(IEnumerable<string> courseIds, bool useCache);
        /// <summary>
        /// Gets all sections that are open for preregistration or registration for the course Ids specified.
        /// </summary>
        /// <param name="courseids">String of course Ids separated by commas</param>
        /// <returns>IEnumerable list of SectionDTOs</returns>
        Task<PrivacyWrapper<IEnumerable<Section3>>> GetSections3Async(IEnumerable<string> courseIds, bool useCache);
        [Obsolete("Obsolete as of API version 1.3. Use the latest version of this method.")]
        Task<Ellucian.Colleague.Dtos.Student.Course> GetCourseByIdAsync(string id);
        Task<Ellucian.Colleague.Dtos.Student.Course2> GetCourseById2Async(string id);
        Task<Ellucian.Colleague.Dtos.Course2> GetCourseByGuid2Async(string guid);
        Task<Ellucian.Colleague.Dtos.Course2> CreateCourse2Async(Ellucian.Colleague.Dtos.Course2 course);

        Task<Ellucian.Colleague.Dtos.Course2> UpdateCourse2Async(Ellucian.Colleague.Dtos.Course2 course);

        [Obsolete("Obsolete as of API version 1.3. Use the latest version of this method.")]
        Task<CoursePage> SearchAsync(CourseSearchCriteria criteria, int pageSize, int pageIndex);

        Task<CoursePage2> Search2Async(CourseSearchCriteria criteria, int pageSize, int pageIndex);
        Task<IEnumerable<Dtos.Student.Course>> GetCoursesByIdAsync(IEnumerable<string> courseIds);

        //V6 Changes
        Task<Dtos.Course3> GetCourseByGuid3Async(string id);
        Task<Dtos.Course3> UpdateCourse3Async(Dtos.Course3 course);
        Task<Dtos.Course3> CreateCourse3Async(Dtos.Course3 course);
        Task<Tuple<IEnumerable<Dtos.Course3>, int>> GetCourses3Async(int offset, int limit, bool bypassCache, string subject, string number, string academicLevel, string owningInstitutionUnits, string title, string instructionalMethods, string schedulingStartOn, string schedulingEndOn);

        //V8 Changes
        Task<Dtos.Course4> GetCourseByGuid4Async(string id);
        Task<Dtos.Course4> UpdateCourse4Async(Dtos.Course4 course, bool bypassCache);
        Task<Dtos.Course4> CreateCourse4Async(Dtos.Course4 course, bool bypassCache);
        Task<Tuple<IEnumerable<Dtos.Course4>, int>> GetCourses4Async(int offset, int limit, bool bypassCache, string subject, string number, List<string> academicLevel, List<string> owningInstitutionUnits, string title, List<string> instructionalMethods, string schedulingStartOn, string schedulingEndOn);

        //V20 Changes
        Task<Dtos.Course5> GetCourseByGuid5Async(string id);
        Task<Dtos.Course5> UpdateCourse5Async(Dtos.Course5 course, bool bypassCache);
        Task<Dtos.Course5> CreateCourse5Async(Dtos.Course5 course, bool bypassCache);
        Task<Tuple<IEnumerable<Dtos.Course5>, int>> GetCourses5Async(int offset, int limit, bool bypassCache, string subject, string number, List<string> academicLevel, List<string> owningInstitutionUnits, List<string> titles, List<string> instructionalMethods, string schedulingStartOn, string schedulingEndOn, string topic, List<string> categories, string activeOn);
    }
}
