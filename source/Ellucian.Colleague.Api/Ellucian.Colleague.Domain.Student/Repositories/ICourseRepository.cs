// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface ICourseRepository : IEthosExtended
    {
        Task<IEnumerable<Course>> GetAsync();
        Task<IEnumerable<Course>> GetAsync(ICollection<string> courseIds);
        Task<Course> GetAsync(string courseId);
        Task<IEnumerable<Course>> GetCoursesByIdAsync(IEnumerable<string> courseIds);
        Task<Course> GetCourseByGuidAsync(string guid);
        Task<Course> GetCourseByGuid2Async(string guid, bool addToCollection= false);
        Task<Course> CreateCourseAsync(Course course, string source = null, string version = null);
        Task<Course> UpdateCourseAsync(Course course, string source = null, string version = null);
        Task<string> GetCourseGuidFromIdAsync(string primaryKey);
        Task<IEnumerable<Course>> GetNonCacheAsync();
        Task<IEnumerable<Course>> GetNonCacheAsync(string subject, string number, string academicLevel, string owningInstitutionUnit, string title, string instructionalMethods, string startOn, string endOn, string topic, string categories);
        Task<IEnumerable<Course>> GetAsync(string newSubject, string number, string newAcademicLevel, string newOwningInstitutionUnit, string title, string newInstructionalMethods, string newStartOn, string newEndOn, string newTopic, string newCategories);
        Task<Tuple<IEnumerable<Course>, int>> GetPagedCoursesAsync(int offset, int limit, string subject, string number, List<string> academicLevel, List<string> owningInstitutionUnit, string title, List<string> instructionalMethods, 
                string startOn, string endOn, string topic = "", string category = "", bool addToErrorCollection = false);
        Task<Tuple<IEnumerable<Course>, int>> GetPagedCoursesAsync(int offset, int limit, string subject, string number, List<string> academicLevel, List<string> owningInstitutionUnit, List<string> titles, List<string> instructionalMethods, 
                string startOn, string endOn, string topic = "", List<string> categories = null, string activeOn = "", bool addToErrorCollection = false);
        Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename);
    }
}
