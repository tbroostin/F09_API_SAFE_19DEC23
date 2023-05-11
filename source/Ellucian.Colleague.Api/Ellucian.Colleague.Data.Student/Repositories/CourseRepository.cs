// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class CourseRepository : BaseColleagueRepository, ICourseRepository
    {
        private Ellucian.Colleague.Data.Student.DataContracts.CdDefaults courseParameters;
        private IDictionary<string, CreditType> types;
        private const string _modelName = "Course";
        private const string _coursesCacheName = "AllCourses";
        private bool addToErrorCollection = false;
        private RepositoryException exception;
        const string AllCoursessCacheKey = "AllCoursessCacheKey";
        const int AllCoursessCacheKeyTimeout = 20;

        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        public CourseRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Use default cache timeout value
            CacheTimeout = Level1CacheTimeoutValue;

            types = new Dictionary<string, CreditType>();
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get all courses from Colleague (or cache)
        /// </summary>
        /// <returns>List of course domain objects</returns>
        public async Task<IEnumerable<Course>> GetAsync()
        {
            // TODO SSS : Remove: performance diagnostics
            logger.Info("Start Course Repository Get()... ");
            var watch = new Stopwatch();
            watch.Start();

            // Get the dictionary of courses
            var courses = await GetAllCoursesAsync();

            // TODO SSS: Remove: performance diagnostics
            watch.Stop();
            logger.Info("Course retrieval complete in " + watch.ElapsedMilliseconds.ToString());

            return courses.Values;
        }

        /// <summary>
        /// Get specific courses
        /// </summary>
        /// <param name="courseIds">list of course Ids</param>
        /// <returns>List of course domain objects</returns>
        public async Task<IEnumerable<Course>> GetAsync(ICollection<string> courseIds)
        {
            var courses = new List<Course>();
            if ((courseIds != null) && (courseIds.Count() != 0))
            {
                // Select the courses in the list of all courses that match the given list of ids
                // Throw an error if any items not found
                var allCourses = await GetAllCoursesAsync();
                string idsNotFound = "";
                foreach (var id in courseIds)
                {
                    try
                    {
                        Course course;
                        if (!allCourses.TryGetValue(id, out course))
                        {
                            course = await GetNewCourseAsync(id);
                        }
                        if (course != null)
                        {
                            courses.Add(course);
                        }
                        else
                        {
                            idsNotFound += " " + id;
                        }
                    }
                    catch (ColleagueSessionExpiredException)
                    {
                        throw;
                    }
                    catch
                    {
                        idsNotFound += " " + id;
                    }
                }
                if (idsNotFound != "")
                {
                    logger.Error("Courses not found for course Ids" + idsNotFound);
                }
            }
            return courses;
        }

        /// <summary>
        /// Returns a single course
        /// </summary>
        /// <param name="courseId">course Id</param>
        /// <returns>Course domain object</returns>
        public async Task<Course> GetAsync(string courseId)
        {
            Course course;
            try
            {
                var courses = await GetAllCoursesAsync();
                if (!courses.TryGetValue(courseId, out course))
                {
                    course = await GetNewCourseAsync(courseId);
                }
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new ArgumentException("Course not found for Id " + courseId);
            }
            return course;
        }

        /// <summary>
        /// Returns all cached or filtered list of course entities
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="number"></param>
        /// <param name="academicLevel"></param>
        /// <param name="owningInstitutionUnit"></param>
        /// <param name="title"></param>
        /// <param name="instructionalMethods"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <returns>IEnumerable<Course</returns>
        public async Task<IEnumerable<Course>> GetAsync(string subject, string number, string academicLevel, string owningInstitutionUnit, string title, string instructionalMethods, string startOn, string endOn, string topic, string categories)
        {
            string criteria = BuildCriteria(subject, number, new List<string>() { academicLevel }, new List<string> { owningInstitutionUnit },
                new List<string>() { title }, new List<string>() { instructionalMethods }, startOn, endOn, topic, new List<string>() { categories }, null);

            if (string.IsNullOrEmpty(criteria))
            {
                var courses = await GetAllCoursesAsync();
                return courses.Values;
            }
            else
            {
                var courseIds = await DataReader.SelectAsync("COURSES", criteria);

                var bulkData = await DataReader.BulkReadRecordAsync<Courses>("COURSES", courseIds);

                var coursesData = new List<Courses>();
                coursesData.AddRange(bulkData);

                var courses = await BuildCoursesAsync(coursesData, await GetCourseRequirements(coursesData));
                return courses.Values;
            }
        }

        /// <summary>
        /// Gets all courses from the database
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Course>> GetNonCacheAsync(string subject, string number, string academicLevel, string owningInstitutionUnit, string title, string instructionalMethods, string startOn, string endOn, string topic, string categories)
        {
            string criteria = BuildCriteria(subject, number, new List<string>() { academicLevel }, new List<string>() { owningInstitutionUnit },
                new List<string>() { title }, new List<string>() { instructionalMethods }, startOn, endOn, topic, new List<string>() { categories }, null);

            // Select from COURSES to get all the Ids
            var courseIds = await DataReader.SelectAsync("COURSES", criteria);
            // Retrieve courses in chunks from the database
            var coursesData = new List<Courses>();
            for (int i = 0; i < courseIds.Count(); i += readSize)
            {
                var subList = courseIds.Skip(i).Take(readSize).ToArray();
                var bulkData = await DataReader.BulkReadRecordAsync<Courses>("COURSES", subList);
                coursesData.AddRange(bulkData);
            }

            var courseList = await BuildCoursesAsync(coursesData, await GetCourseRequirements(coursesData));
            return courseList.Values;
        }

        /// <summary>
        /// Gets all courses from the database using custom paging
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Course>, int>> GetPagedCoursesAsync(int offset, int limit, string subject,
            string number, List<string> academicLevel, List<string> owningInstitutionUnit, string title, List<string> instructionalMethods, string startOn, string endOn, string topic = "", string category = "", bool addToCollection = false)
        {
            var titles = new List<string>();
            if (!string.IsNullOrEmpty(title)) titles.Add(title);
            var categories = new List<string>();
            if (!string.IsNullOrEmpty(category)) categories.Add(category);
            return await GetPagedCoursesAsync(offset, limit, subject, number, academicLevel, owningInstitutionUnit, titles, instructionalMethods, startOn, endOn, topic, categories, "", addToCollection);
        }


        /// <summary>
        /// Gets all courses from the database using custom paging
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Course>, int>> GetPagedCoursesAsync(int offset, int limit, string subject, string number, List<string> academicLevel,
            List<string> owningInstitutionUnit, List<string> titles, List<string> instructionalMethods, string startOn, string endOn, string topic = "", List<string> categories = null, string activeOn = "", bool addToCollection = false)
        {
            //flag determines if errors will be returned as part of a collection.
            this.addToErrorCollection = addToCollection;
            Dictionary<string, Course> courseList = null;
            int totalCount = 0;

            try
            {
                string coursesKey = CacheSupport.BuildCacheKey( AllCoursessCacheKey,
                                                            string.IsNullOrEmpty( subject ) ? "" : subject,
                                                            string.IsNullOrEmpty( number ) ? "" : number,
                                                            academicLevel != null && academicLevel.Any() ? academicLevel : null,
                                                            owningInstitutionUnit != null && owningInstitutionUnit.Any() ? owningInstitutionUnit : null,
                                                            titles != null && titles.Any() ? titles : null,
                                                            instructionalMethods != null && instructionalMethods.Any() ? instructionalMethods : null,
                                                            string.IsNullOrEmpty( startOn ) ? "" : startOn,
                                                            string.IsNullOrEmpty( endOn ) ? "" : endOn,
                                                            string.IsNullOrEmpty( topic ) ? "" : topic,
                                                            categories != null && categories.Any() ? categories : null,
                                                            string.IsNullOrEmpty( activeOn ) ? "" : activeOn);


                var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                       this,
                       ContainsKey,
                       GetOrAddToCacheAsync,
                       AddOrUpdateCacheAsync,
                       transactionInvoker,
                       coursesKey,
                       "",
                       offset,
                       limit,
                       AllCoursessCacheKeyTimeout,
                       async () =>
                       {
                           string criteria = BuildCriteria( subject, number, academicLevel, owningInstitutionUnit, titles, instructionalMethods, startOn, endOn, topic, categories, activeOn );

                           string[] courseIds = new string[] { };

                           courseIds = await DataReader.SelectAsync( "COURSES", criteria );

                           return new CacheSupport.KeyCacheRequirements()
                           {
                               limitingKeys = courseIds != null && courseIds.Any() ? courseIds.Distinct().ToList() : null,
                               criteria = criteria,
                           };
                       } );


                if( keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any() )
                {
                    return new Tuple<IEnumerable<Course>, int>( new List<Course>(), 0 );
                }

                totalCount = keyCacheObject.TotalCount.Value;

                var subList = keyCacheObject.Sublist.ToArray();

                var coursesData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.Courses>("COURSES", subList);

                if (coursesData.Equals(default(BulkReadOutput<DataContracts.Courses>)))
                {
                    return new Tuple<IEnumerable<Course>, int>(new List<Course>(), totalCount);
                }
                if ((coursesData.InvalidKeys != null && coursesData.InvalidKeys.Any())
                        || (coursesData.InvalidRecords != null && coursesData.InvalidRecords.Any()))
                {
                    var repositoryException = new RepositoryException();

                    if (coursesData.InvalidKeys.Any())
                    {
                        repositoryException.AddErrors(coursesData.InvalidKeys
                            .Select(key => new RepositoryError("invalid.key",
                             string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                    }
                    if (coursesData.InvalidRecords.Any())
                    {
                        repositoryException.AddErrors(coursesData.InvalidRecords
                           .Select(r => new RepositoryError("invalid.record",
                            string.Format("Error: '{0}' ", r.Value))
                           { SourceId = r.Key }));
                    }
                    throw repositoryException;
                }

                courseList = await BuildCoursesAsync(coursesData.BulkRecordsRead.ToList(), await GetCourseRequirements(coursesData.BulkRecordsRead.ToList()));
            }
            catch (Exception ex)
            {
                LogRepoError(ex.Message);
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return courseList.Any() ? new Tuple<IEnumerable<Course>, int>(courseList.Values, totalCount) : new Tuple<IEnumerable<Course>, int>(new List<Course>(), 0);
        }

        /// <summary>
        /// Builds the search criteria
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="number"></param>
        /// <param name="academicLevel"></param>
        /// <param name="owningInstitutionUnit"></param>
        /// <param name="title"></param>
        /// <param name="instructionalMethods"></param>
        /// <param name="startOn"></param>
        /// <param name="endOn"></param>
        /// <param name="topic"></param>
        /// <param name="categories"></param>
        /// <param name="activeOn"></param>
        /// <returns></returns>
        private static string BuildCriteria(string subject, string number, List<string> academicLevels, List<string> owningInstitutionUnits, List<string> titles, List<string> instructionalMethods, string startOn, string endOn, string topic, List<string> categories, string activeOn)
        {
            string criteria = string.Empty;

            // Index value of course subject and number
            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(number))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH CRS.SUBJECT.NO EQ '" + subject + "*" + number + "'";
            }
            else
            {
                // Index on CRS.SUBJECT
                if (!string.IsNullOrEmpty(subject))
                {
                    criteria += "WITH CRS.SUBJECT EQ '" + subject + "'";
                }

                // Index on CRS.NO
                if (!string.IsNullOrEmpty(number))
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        criteria += " AND ";
                    }
                    criteria += "WITH CRS.NO EQ '" + number + "'";
                }
            }

            if (academicLevels != null && academicLevels.Any())
            {
                foreach (var academicLevel in academicLevels)
                {
                    if (!string.IsNullOrEmpty(academicLevel))
                    {
                        if (!string.IsNullOrEmpty(criteria))
                        {
                            criteria += " AND ";
                        }
                        criteria += "WITH CRS.ACAD.LEVEL EQ '" + academicLevel + "'";
                    }
                }
            }
            if (owningInstitutionUnits != null && owningInstitutionUnits.Any())
            {
                foreach (var owningInstitutionUnit in owningInstitutionUnits)
                {
                    if (!string.IsNullOrEmpty(owningInstitutionUnit))
                    {
                        if (!string.IsNullOrEmpty(criteria))
                        {
                            criteria += " AND ";
                        }
                        criteria += "WITH CRS.DEPTS EQ '" + owningInstitutionUnit + "'";
                    }
                }
            }
            foreach (var title in titles)
            {
                if (!string.IsNullOrEmpty(title))
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        criteria += " AND ";
                    }
                    // Index on CRS.TITLE.INDEX for short title.
                    // Can't use CRS.TITLE.INDEX because it's a soundex index.
                    criteria += "WITH CRS.TITLE LIKE '..." + title + "...' OR CRS.SHORT.TITLE LIKE '..." + title + "...'";
                }
            }
            if (instructionalMethods != null && instructionalMethods.Any())
            {
                foreach (var instructionalMethod in instructionalMethods)
                {
                    if (!string.IsNullOrEmpty(instructionalMethod))
                    {
                        if (!string.IsNullOrEmpty(criteria))
                        {
                            criteria += " AND ";
                        }
                        criteria += "WITH CRS.INSTR.METHODS EQ '" + instructionalMethod + "'";
                    }
                }
            }

            if (!string.IsNullOrEmpty(startOn))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH CRS.START.DATE EQ '" + startOn + "'";
            }

            if (!string.IsNullOrEmpty(endOn))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH CRS.END.DATE EQ '" + endOn + "'";
            }

            if (!string.IsNullOrEmpty(activeOn))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH CRS.START.DATE LE '" + activeOn + "' AND (CRS.END.DATE GE '" + activeOn + "' OR CRS.END.DATE EQ '')";
            }

            if (!string.IsNullOrEmpty(topic))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += "WITH CRS.TOPIC.CODE EQ '" + topic + "'";
            }
            if (categories != null && categories.Any())
            {
                foreach (var category in categories)
                {
                    if (!string.IsNullOrEmpty(category))
                    {
                        if (!string.IsNullOrEmpty(criteria))
                        {
                            criteria += " AND ";
                        }
                        criteria += "WITH CRS.COURSE.TYPES EQ '" + category + "'";
                    }
                }
            }

            return criteria;
        }


        /// <summary>
        /// Returns a single course
        /// </summary>
        /// <param name="courseId">Course GUID</param>
        /// <returns>Course domain object</returns>
        public async Task<Course> GetCourseByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID must be specified.");
            }

            string courseId = GetRecordKeyFromGuid(guid);
            if (string.IsNullOrEmpty(courseId))
            {
                throw new KeyNotFoundException("Invalid course GUID: " + guid);
            }

            return await GetNewCourseAsync(courseId);
        }


        /// <summary>
        /// Get the course record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<string> GetCourseIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Course GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Course GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "COURSES")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, COURSES", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("GUID.Not.Found", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Returns a single course
        /// </summary>
        /// <param name="courseId">Course GUID</param>
        /// <returns>Course domain object</returns>
        public async Task<Course> GetCourseByGuid2Async(string guid, bool addToCollection = false)
        {
            this.addToErrorCollection = addToCollection;

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID must be specified.");
            }

            Course course = null;
            try
            {
                string courseId = await GetCourseIdFromGuidAsync(guid);

                if (string.IsNullOrEmpty(courseId))
                {
                    LogRepoError("No course was found for GUID '" + guid + "'");
                    throw exception;
                }

                course = await GetNewCourseAsync(courseId);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (KeyNotFoundException)
            {
                LogRepoError("No course was found for GUID '" + guid + "'");
            }
            catch (Exception ex)
            {
                LogRepoError(ex.Message);
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return course;
        }

        /// <summary>
        /// Creates a course
        /// </summary>
        /// <param name="course">Course to be created</param>
        /// <param name="source">Source for the create</param>
        /// <param name="version"Version of API used</param>
        /// <returns>Created course entity</returns>
        public async Task<Course> CreateCourseAsync(Course course, string source = null, string version = null)
        {
            return await MaintainCourseAsync(course, source, false, version);
        }

        /// <summary>
        /// Updates a course
        /// </summary>
        /// <param name="course">Course to be updated</param>
        /// <param name="source">Source for the update</param>
        /// <param name="version"Version of API used</param>
        /// <returns>Updated course entity</returns>
        public async Task<Course> UpdateCourseAsync(Course course, string source = null, string version = null)
        {
            return await MaintainCourseAsync(course, source, true, version);
        }

        /// <summary>
        /// Get a new course direct from the database
        /// </summary>
        /// <param name="courseId">Course ID</param>
        /// <returns>The specified course</returns>
        private async Task<Course> GetNewCourseAsync(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException("courseId");
            }

            Courses course = await DataReader.ReadRecordAsync<Courses>(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Invalid course ID: " + courseId);
            }

            var courses = new List<Courses>() { course };
            Collection<AcadReqmts> acadReqs = await GetCourseRequirements(courses);
            var courseDict = await BuildCoursesAsync(courses, acadReqs);
            Course newCourse = null;

            if (courseDict != null && courseDict.Any())
            {
                newCourse = courseDict.FirstOrDefault().Value;
            }
            return newCourse;
        }

        /// <summary>
        /// Get Course Objects based on the Course Ids
        /// </summary>
        /// <param name="courseIds">Comes from Body on a Post Transaction</param>
        /// <returns>list of Course Objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>> GetCoursesByIdAsync(IEnumerable<string> courseIds)
        {
            // Retrieve courses in chunks from the database
            var courses = new List<Course>();

            if (courseIds != null && courseIds.Count() > 0)
            {
                // get course data
                Collection<Ellucian.Colleague.Data.Student.DataContracts.Courses> coursesData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Courses>("COURSES", courseIds.ToArray());
                if (coursesData != null && coursesData.Any())
                {
                    var requisiteRequirements = coursesData.Select(rr => rr.CrsReqs).Where(r => r.Any(x => x.Length > 0)).ToList();
                    List<string> requisiteRequirementIds = new List<string>();
                    if (requisiteRequirements != null && requisiteRequirements.Count() > 0)
                    {
                        foreach (var requisiteRequirement in requisiteRequirements)
                        {
                            if (requisiteRequirement != null && requisiteRequirement.Count() > 0)
                            {
                                foreach (var reqId in requisiteRequirement)
                                {
                                    if (!requisiteRequirementIds.Contains(reqId))
                                    {
                                        requisiteRequirementIds.Add(reqId);
                                    }
                                }
                            }
                        }
                    }
                    Collection<AcadReqmts> requirementData = await DataReader.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", requisiteRequirementIds.ToArray());

                    var courseList = await BuildCoursesAsync(coursesData, requirementData);

                    foreach (var id in courseIds)
                    {
                        courses.Add(courseList[id]);
                    }
                    return courses;
                }
            }
            return courses;
        }

        /// <summary>
        /// Update the courses cache
        /// </summary>
        /// <param name="course">Course to add to or update in the cache</param>
        private async Task UpdateCacheAsync(Course course)
        {
            // Get the cache and see if this course is in it
            var courses = await GetAllCoursesAsync();
            Dictionary<string, Course> courseDict = courses.ToDictionary(x => x.Key, y => y.Value);
            Course cachedCourse = null;
            if (courseDict.TryGetValue(course.Id, out cachedCourse))
            {
                // Course is in cache - replace it with the new version
                courseDict[course.Id] = course;
            }
            else
            {
                // Course is not in the cache, so add it
                courseDict.Add(course.Id, cachedCourse);
            }
            // Update the cache
            AddOrUpdateCache<Dictionary<string, Course>>(_coursesCacheName, courseDict);
        }

        /// <summary>
        /// Gets courses from the database, or from cache. 
        /// </summary>
        /// <returns>CourseId-keyed Dictionary of Course domain objects</returns>
        private async Task<IDictionary<string, Course>> GetAllCoursesAsync()
        {
            var courseDict = await GetOrAddToCacheAsync<Dictionary<string, Course>>(_coursesCacheName,
            async () =>
            {
                // TODO SSS: Remove: performance diagnostics
                logger.Info("Getting courses from database... ");

                // Select from COURSES to get all the Ids
                var courseIds = await DataReader.SelectAsync("COURSES", "");
                // Retrieve courses in chunks from the database
                var coursesData = new List<Courses>();
                for (int i = 0; i < courseIds.Count(); i += readSize)
                {
                    var subList = courseIds.Skip(i).Take(readSize).ToArray();
                    var bulkData = await DataReader.BulkReadRecordAsync<Courses>("COURSES", subList);
                    coursesData.AddRange(bulkData);
                }

                var courseList = await BuildCoursesAsync(coursesData, await GetCourseRequirements(coursesData));
                return courseList;
            }
            );
            return courseDict;
        }

        /// <summary>
        /// Gets all courses from the database
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Course>> GetNonCacheAsync()
        {
            // Select from COURSES to get all the Ids
            var courseIds = await DataReader.SelectAsync("COURSES", "");
            // Retrieve courses in chunks from the database
            var coursesData = new List<Courses>();
            for (int i = 0; i < courseIds.Count(); i += readSize)
            {
                var subList = courseIds.Skip(i).Take(readSize).ToArray();
                var bulkData = await DataReader.BulkReadRecordAsync<Courses>("COURSES", subList);
                coursesData.AddRange(bulkData);
            }

            var courseList = await BuildCoursesAsync(coursesData, await GetCourseRequirements(coursesData));
            return courseList.Values;
        }

        private async Task<Collection<AcadReqmts>> GetCourseRequirements(List<Courses> coursesData)
        {
            var requisiteRequirements = coursesData.Select(rr => rr.CrsReqs).Where(r => r.Any(x => x.Length > 0));
            List<string> requisiteRequirementIds = new List<string>();
            if (requisiteRequirements != null && requisiteRequirements.Count() > 0)
            {
                foreach (var requisiteRequirement in requisiteRequirements)
                {
                    if (requisiteRequirement != null && requisiteRequirement.Count() > 0)
                    {
                        foreach (var reqId in requisiteRequirement)
                        {
                            if (!requisiteRequirementIds.Contains(reqId))
                            {
                                requisiteRequirementIds.Add(reqId);
                            }
                        }
                    }
                }
            }
            Collection<AcadReqmts> requirementData = await DataReader.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", requisiteRequirementIds.ToArray());
            return requirementData;
        }

        private async Task<Dictionary<string, Course>> BuildCoursesAsync(IEnumerable<Courses> courseData, Collection<AcadReqmts> requirementData)
        {
            var sessionCycles = await GetSessionCycleDescriptionsAsync();
            var yearlyCycles = await GetYearlyCycleDescriptionsAsync();
            var courseStatuses = await GetCourseStatusesAsync();
            var courseTypes = await GetCourseTypesAsync();
            var courseParameters = await GetCourseParametersAsync();

            char _VM = Convert.ToChar(DynamicArray.VM);
            var courses = new Dictionary<string, Course>();
            Course course = null;
            // If no data passed in, return a null collection
            if (courseData != null)
            {
                foreach (var crs in courseData)
                {
                    try
                    {
                        // Get the list of course statuses and identify the most recent one as the current status
                        List<CourseApproval> courseApprovals = new List<CourseApproval>();
                        if (crs.ApprovalStatusEntityAssociation != null && crs.ApprovalStatusEntityAssociation.Any())
                        {
                            int index = 0;
                            foreach (var approval in crs.ApprovalStatusEntityAssociation)
                            {
                                if (string.IsNullOrEmpty(approval.CrsStatusAssocMember))
                                {
                                    logger.Info("Course Approvals exists for Course Id '" + crs.Recordkey + "' that do not have a status provided.");
                                    continue;
                                }
                                try
                                {
                                    // The CourseApprovals initialization method has dates as non-nullable and
                                    // will bomb with "Nullable object must have a value" if we try to pass them
                                    // into the constructor without values.  Provide a more accurate error if these
                                    // cases.
                                    if (approval.CrsStatusDateAssocMember == null || !approval.CrsStatusDateAssocMember.HasValue)
                                    {
                                        throw new ArgumentNullException("statusDate", "Status date must be supplied.");
                                    }
                                    if (approval.CrsApprovalDateAssocMember == null || !approval.CrsApprovalDateAssocMember.HasValue)
                                    {
                                        throw new ArgumentNullException("date", "Approval date must be supplied.");
                                    }
                                    var courseApproval = new CourseApproval(approval.CrsStatusAssocMember, approval.CrsStatusDateAssocMember.Value, approval.CrsApprovalAgencyIdsAssocMember, approval.CrsApprovalIdsAssocMember, approval.CrsApprovalDateAssocMember.Value);
                                    courseApproval.Status = ConvertStatusStringToCourseStatus(approval.CrsStatusAssocMember, courseStatuses);
                                    courseApprovals.Add(courseApproval);
                                }
                                catch (Exception ex)
                                {
                                    var msg = "Exception occurred while processing Course Approvals for Course Id.";
                                    logger.Info(msg + " " + crs.Recordkey);
                                    logger.Info(ex.Message);
                                    // Only report the current (first) status in the association.
                                    // Any historical data missing approvals can be ignored.
                                    if (index == 0)
                                    {
                                        LogRepoError(msg + " " + ex.Message, crs.RecordGuid, crs.Recordkey);
                                    }
                                }
                                index++;
                            }
                        }

                        List<OfferingDepartment> departments = new List<OfferingDepartment>();
                        if (crs.CourseDeptsEntityAssociation != null && crs.CourseDeptsEntityAssociation.Count > 0)
                        {
                            foreach (var offeringDept in crs.CourseDeptsEntityAssociation)
                            {
                                try
                                {
                                    var department = new OfferingDepartment(offeringDept.CrsDeptsAssocMember, offeringDept.CrsDeptPctsAssocMember.GetValueOrDefault());
                                    departments.Add(department);
                                }
                                catch (Exception ex)
                                {
                                    var msg = "Exception occurred while processing Offering Departments for Course Id.";
                                    logger.Info(msg + " " + crs.Recordkey);
                                    logger.Info(ex.Message);
                                    LogRepoError(msg + " " + ex.Message, crs.RecordGuid, crs.Recordkey);
                                }
                            }
                        }


                        try
                        {
                            course = new Course(crs.Recordkey,
                                                crs.CrsShortTitle,
                                                crs.CrsTitle,
                                                departments,
                                                crs.CrsSubject,
                                                crs.CrsNo,
                                                crs.CrsAcadLevel,
                                                crs.CrsLevels,
                                                crs.CrsMinCred,
                                                crs.CrsCeus,
                                                courseApprovals);

                            course.VerifyGrades = !string.IsNullOrEmpty(crs.CrsOvrVerifyGrades) ? crs.CrsOvrVerifyGrades.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false : default(bool?);
                            if (crs.CrsDesc != null)
                            {
                                course.Description = crs.CrsDesc.Replace(_VM, ' ');
                            }
                            course.LocationCodes = crs.CrsLocations;
                            course.TermSessionCycle = crs.CrsSessionCycle;
                            course.TermYearlyCycle = crs.CrsYearlyCycle;
                            course.TermsOffered = sessionCycles.Where(sc => sc.Recordkey == crs.CrsSessionCycle).Select(sc => sc.ScDesc).FirstOrDefault();
                            course.YearsOffered = yearlyCycles.Where(yc => yc.Recordkey == crs.CrsYearlyCycle).Select(yc => yc.YcDesc).FirstOrDefault();
                            course.StartDate = crs.CrsStartDate;
                            course.EndDate = crs.CrsEndDate;
                            course.Name = crs.CrsName;
                            course.TopicCode = crs.CrsTopicCode;

                            if (crs.CrsBillingCred != null)
                            {
                                course.BillingCredits = crs.CrsBillingCred;
                            }

                            //
                            // Added fields for Student Success project (srm)
                            //
                            course.FederalCourseClassification = crs.CrsCip;
                            course.LocalCourseClassifications = crs.CrsLocalGovtCodes;
                            course.CourseTypeCodes = crs.CrsCourseTypes;
                            course.LocalCreditType = crs.CrsCredType;
                            course.Guid = crs.RecordGuid;
                            course.MaximumCredits = crs.CrsMaxCred;
                            course.VariableCreditIncrement = crs.CrsVarCredIncrement;
                            course.GradeSchemeCode = crs.CrsGradeScheme;
                            course.AllowAudit = courseParameters.CdAllowAuditFlag == "Y";
                            course.AllowPassNoPass = courseParameters.CdAllowPassNopassFlag == "Y";
                            course.AllowWaitlist = courseParameters.CdAllowWaitlistFlag == "Y";
                            course.IsInstructorConsentRequired = courseParameters.CdFacultyConsentFlag == "Y";
                            course.OnlyPassNoPass = courseParameters.CdOnlyPassNopassFlag == "Y";
                            course.WaitlistRatingCode = courseParameters.CdWaitlistRating;
                            course.ExternalSource = crs.CrsExternalSource;
                            course.ShowDropRoster = string.Equals(crs.CrsShowDropRosterFlag, "Y", StringComparison.OrdinalIgnoreCase);
                        }
                        catch (Exception e)
                        {
                            var msg = string.Format("Unable to create course for record ID {0} with guid {1}.  Exception: {2}", crs.Recordkey, crs.RecordGuid, e.Message);
                            logger.Error(msg);
                            LogRepoError(msg, crs.RecordGuid, crs.Recordkey);

                            // throw new RepositoryException(msg);
                            // Removed because breaking self service.  Will revisit with new error standards in 1.25
                        }

                        if (course != null)
                        {

                            // v20 changes add Instructional method details object
                            if (crs.CourseContactEntityAssociation != null && crs.CourseContactEntityAssociation.Count > 0)
                            {
                                foreach (var crsContact in crs.CourseContactEntityAssociation)
                                {
                                    try
                                    {
                                        var isAdded = course.AddInstructionalMethodCode(crsContact.CrsInstrMethodsAssocMember);
                                        if (isAdded)
                                        {
                                            course.AddInstructionalMethodHours(crsContact.CrsContactHoursAssocMember);
                                            course.AddInstructionalMethodPeriod(crsContact.CrsContactMeasuresAssocMember);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        var msg = "Exception occurred while processing Instructional Method codes for Course Id";
                                        logger.Info(msg + " " + crs.Recordkey);
                                        logger.Info(ex.Message);
                                        LogRepoError(msg, crs.RecordGuid, crs.Recordkey);
                                    }
                                }
                            }

                            if (crs.CrsInstrMethods != null && crs.CrsInstrMethods.Any())
                            {
                                foreach (var instrMethod in crs.CrsInstrMethods)
                                {
                                    try
                                    {
                                        course.AddInstructionalMethodCode(instrMethod);
                                    }
                                    catch (Exception ex)
                                    {
                                        var msg = "Exception occurred while processing Instructional Method codes for Course Id";
                                        logger.Info(msg + " " + crs.Recordkey);
                                        logger.Info(ex.Message);
                                        LogRepoError(msg, crs.RecordGuid, crs.Recordkey);
                                    }
                                }
                            }

                            if (crs.CrsCourseTypes != null)
                            {
                                foreach (var type in crs.CrsCourseTypes)
                                {
                                    try
                                    {
                                        course.AddType(type);
                                    }
                                    catch (Exception ex)
                                    {
                                        var msg = "Exception occurred while processing Type values for Course Id";
                                        logger.Info(msg + " " + crs.Recordkey);
                                        logger.Info(ex.Message);
                                        LogRepoError(msg, crs.RecordGuid, crs.Recordkey);
                                    }
                                }
                            }

                            if (courseParameters.CdReqsConvertedFlag != "Y")
                            {
                                // Update the requisite list using the course coreq list and prereq code fields from the Colleague course.
                                var requisites = new List<Requisite>();

                                if (crs.CourseCoreqsEntityAssociation != null)
                                {
                                    foreach (var item in crs.CourseCoreqsEntityAssociation)
                                    {
                                        // The data accessor may create a single association member with empty values. 
                                        // Therefore, checking for null or empty course id, as well as catching any other possibilities.
                                        if (!(string.IsNullOrEmpty(item.CrsCoreqCoursesAssocMember)))
                                        {
                                            try
                                            {
                                                // For the old style corequisites we will default the completion order to concurrent and fill in the course id.
                                                requisites.Add(new Requisite(item.CrsCoreqCoursesAssocMember, item.CrsCoreqCoursesReqdFlagAssocMember == "Y" ? true : false));
                                            }
                                            catch (Exception ex)
                                            {
                                                // Don't do anything if an exception occurs, just move on. Corequisite course missing or invalid.
                                                logger.Error(ex, "Corequisite course missing or invalid.");
                                            }

                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(crs.CrsPrereqs))
                                {
                                    try
                                    {
                                        // for the old style single prerequisite we will default it to required, not protected, and make completion order "previous".
                                        requisites.Add(new Requisite(crs.CrsPrereqs, true, RequisiteCompletionOrder.Previous, false));

                                    }
                                    catch (Exception ex)
                                    {
                                        // Don't do anything. Skip this prereq and move on.
                                        logger.Error(ex, "Unable to get prerequisite.");
                                    }

                                }
                                if (requisites.Any())
                                {
                                    course.Requisites = requisites;
                                }
                            }
                            else
                            {
                                // If the Colleague data is now in the new format update requisites using the new Requisite field from the Colleague course.
                                if (crs.CrsReqs != null && crs.CrsReqs.Any())
                                {
                                    var requisites = new List<Requisite>();
                                    foreach (var req in crs.CrsReqs)
                                    {
                                        if (!string.IsNullOrEmpty(req))
                                        {
                                            var requirement = requirementData.Where(r => r.Recordkey == req).FirstOrDefault();
                                            if (requirement != null)
                                            {
                                                bool isRequired = requirement.AcrReqsEnforcement == "RQ" ? true : false;
                                                bool isProtected = requirement.AcrReqsProtectFlag == "Y" ? true : false;
                                                // If the ACR.REQS.TIMING field is other than C or E we will default to Previous.
                                                RequisiteCompletionOrder completionOrder = RequisiteCompletionOrder.Previous;
                                                if (requirement.AcrReqsTiming == "C")
                                                {
                                                    completionOrder = RequisiteCompletionOrder.Concurrent;
                                                }
                                                if (requirement.AcrReqsTiming == "E")
                                                {
                                                    completionOrder = RequisiteCompletionOrder.PreviousOrConcurrent;
                                                }
                                                requisites.Add(new Requisite(req, isRequired, completionOrder, isProtected));
                                            }
                                            else
                                            {
                                                // Could not find the requirement for this requisite. Discard and log error.
                                                var requisiteError = "Course requisite item points to non-existent requirement";
                                                LogDataError("Course requisite", crs.Recordkey, crs, null, requisiteError);
                                                LogRepoError(requisiteError, crs.RecordGuid, crs.Recordkey);
                                            }
                                        }
                                    }
                                    if (requisites.Any())
                                    {
                                        course.Requisites = requisites;
                                    }
                                }
                            }
                            // Find all the equated courses
                            var equatedCourseIds = await FindEquatesAsync(course.Id, crs.CrsEquateCodes);
                            foreach (var courseId in equatedCourseIds)
                            {
                                try
                                {
                                    course.AddEquatedCourseId(courseId);
                                }
                                catch (Exception ex)
                                {
                                    logger.Info("Could not add equated course id for course " + crs.Recordkey);
                                    logger.Info(ex.Message);
                                    LogRepoError(ex.Message, crs.RecordGuid, crs.Recordkey);
                                }
                            }
                            courses[course.Id] = course;

                            // set pseudo course flag - based off of a special processing code (P) on the course type valcode.
                            if (crs.CrsCourseTypes != null)
                            {
                                try
                                {
                                    var courseTypeValcodeMatches = crs.CrsCourseTypes.SelectMany(x => courseTypes.ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == x)).Distinct().ToList();
                                    if (courseTypeValcodeMatches != null && courseTypeValcodeMatches.Any())
                                    {
                                        if (courseTypeValcodeMatches.Where(a => a.ValActionCode1AssocMember != null && a.ValActionCode1AssocMember.ToUpper() == "P").FirstOrDefault() != null)
                                        {
                                            course.IsPseudoCourse = true;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, "Unable to set course flag from special processing code (P) on the course type valcode.");
                                }
                            }

                            // Set indicator flags
                            course.AllowAudit = crs.CrsAllowAuditFlag == "Y";
                            course.AllowPassNoPass = crs.CrsAllowPassNopassFlag == "Y";
                            course.OnlyPassNoPass = crs.CrsOnlyPassNopassFlag == "Y";
                            course.AllowWaitlist = crs.CrsAllowWaitlistFlag == "Y";
                            course.AllowToCountCourseRetakeCredits = crs.CrsCountRetakeCredFlag == "Y";
                            //course.AllowWaitlistMultipleSections = crs.CrsWaitlistMultSectFlag == "Y";

                            // Set the cycle restrictions by location
                            if (crs.CourseLocationCyclesEntityAssociation != null)
                            {
                                foreach (var locCycle in crs.CourseLocationCyclesEntityAssociation)
                                {
                                    // The data accessor may create a single association member with empty values. 
                                    if (!(string.IsNullOrEmpty(locCycle.CrsClcLocationAssocMember)))
                                    {
                                        try
                                        {
                                            course.AddLocationCycleRestriction(new LocationCycleRestriction(locCycle.CrsClcLocationAssocMember, locCycle.CrsClcSessionCycleAssocMember, locCycle.CrsClcYearlyCycleAssocMember));
                                        }
                                        catch (Exception ex)
                                        {
                                            // Don't do anything if an exception occurs, just move on. Indicates a null restriction or one with a null or empty location which is invalid.
                                            logger.Error(ex, "Invalid restriction.");
                                        }

                                    }
                                }
                            }
                        }
                    }
                    catch (ColleagueSessionExpiredException)
                    {
                        throw;
                    }
                    catch (RepositoryException rex)
                    {
                        throw rex;
                    }
                    catch (Exception ex)
                    {
                        LogDataError("Course", crs.Recordkey, crs, ex);
                    }
                }
            }
            return courses;
        }


        /// <summary>
        /// Checks addErrorToCollection boolean to determine of error message should be added to the repository exception
        /// Note - the exception is not thrown, but is only added to the collection.
        /// </summary>
        /// <param name="dataName"></param>
        /// <param name="id"></param>
        /// <param name="dataObject"></param>
        /// <param name="ex"></param>
        private void LogRepoError(string errorMessage, string guid = "", string sourceId = "")
        {
            if (addToErrorCollection)
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", errorMessage)
                {
                    SourceId = string.IsNullOrEmpty(sourceId) ? "" : sourceId,
                    Id = string.IsNullOrEmpty(guid) ? "" : guid
                });
            }
        }


        // Returns the SessionCycles data contract objects so that courses can be populated with the description
        private async Task<IEnumerable<SessionCycles>> GetSessionCycleDescriptionsAsync()
        {
            var sessionCycles = await GetOrAddToCacheAsync<Collection<SessionCycles>>("AllSessionCycles",
              async () =>
              {
                  // function to get sessionCycles from database if not in cache.
                  var scList = await DataReader.BulkReadRecordAsync<SessionCycles>("SESSION.CYCLES", "");
                  if (scList == null)
                  {
                      // make sure an empty list is returned, never a null
                      scList = new Collection<SessionCycles>();
                  }
                  return scList;
              }
            );
            return sessionCycles;
        }

        // Returns the YearlyCycles data contract objects so that courses can be populated with the description
        private async Task<IEnumerable<YearlyCycles>> GetYearlyCycleDescriptionsAsync()
        {
            var yearlyCycles = await GetOrAddToCacheAsync<Collection<YearlyCycles>>("AllYearlyCycles",
               async () =>
               {
                   // function to get sessionCycles from database if not in cache.
                   var ycList = await DataReader.BulkReadRecordAsync<YearlyCycles>("YEARLY.CYCLES", "");
                   if (ycList == null)
                   {
                       // make sure an empty list is returned, never a null
                       ycList = new Collection<YearlyCycles>();
                   }
                   return ycList;
               }
            );
            return yearlyCycles;
        }

        /// <summary>
        /// Translate course status external code to one of the enumerated values
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="courseStatuses"></param>
        /// <returns></returns>
        private CourseStatus ConvertStatusStringToCourseStatus(string statusCode, ApplValcodes courseStatuses)
        {
            if (!string.IsNullOrEmpty(statusCode))
            {
                var codeItem = courseStatuses.ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == statusCode).FirstOrDefault();
                CourseStatus cs;
                if (codeItem == null)
                {
                    cs = CourseStatus.Unknown;
                    return cs;
                }
                switch (codeItem.ValActionCode1AssocMember)
                {

                    case "1":
                        cs = CourseStatus.Active;
                        break;
                    case "2":
                        cs = CourseStatus.Terminated;
                        break;
                    default:
                        cs = CourseStatus.Unknown;
                        break;
                }
                return cs;
            }
            else
            {
                return CourseStatus.Unknown;
            }
        }

        /// <summary>
        /// Translate credit type enumerated value to one of the external codes
        /// </summary>
        /// <param name="status">CreditType enumerated value</param>
        /// <param name="creditTypes">List of credit types</param>
        /// <returns></returns>
        private string ConvertCreditTypeToCreditTypeString(CreditType type, IEnumerable<CredTypes> creditTypes)
        {
            string typeString;
            switch (type)
            {
                case CreditType.Institutional:
                    typeString = "I";
                    break;
                case CreditType.ContinuingEducation:
                    typeString = "C";
                    break;
                case CreditType.Transfer:
                    typeString = "T";
                    break;
                default:
                    typeString = "O";
                    break;
            }
            return creditTypes.Where(ct => ct.CrtpCategory == typeString).FirstOrDefault().Recordkey;
        }

        /// <summary>
        /// Get Courses Statuses using the COURSE.STATUSES valcode table 
        /// </summary>
        /// <returns>ApplValcodes<string, CourseStatus></returns>
        private async Task<ApplValcodes> GetCourseStatusesAsync()
        {
            var courseStatuses = await GetOrAddToCacheAsync<ApplValcodes>("CourseStatuses",
               async () =>
               {
                   ApplValcodes courseStatusesValcode = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.STATUSES");

                   if (courseStatusesValcode == null)
                   {
                       var errorMessage = "Unable to access COURSE.STATUSES valcode table.";
                       logger.Info(errorMessage);
                       throw new ColleagueWebApiException(errorMessage);
                   }
                   return courseStatusesValcode;
               }
                );
            return courseStatuses;
        }

        /// <summary>
        /// Get Courses Statuses using the COURSE.STATUSES valcode table 
        /// </summary>
        /// <returns>ApplValcodes<string, CourseStatus></returns>
        private async Task<IEnumerable<CredTypes>> GetCreditTypesAsync()
        {
            var creditTypes = await GetOrAddToCacheAsync<IEnumerable<CredTypes>>("CreditTypes",
               async () =>
               {
                   var credTypesData = await DataReader.BulkReadRecordAsync<CredTypes>("CRED.TYPES", "");
                   if (credTypesData == null)
                   {
                       // make sure an empty list is returned, never a null
                       credTypesData = new Collection<CredTypes>();
                   }
                   return credTypesData;
               }
            );
            return creditTypes;
        }

        /// <summary>
        /// Get course equates from the database
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, List<string>>> GetCourseEquatesAsync()
        {
            var courseEquates = await GetOrAddToCacheAsync<Dictionary<string, List<string>>>("AllCourseEquates",
              async () =>
              {
                  // function to get course equates from database if not in cache.
                  var equateList = await DataReader.BulkReadRecordAsync<CourseEquateCodes>("COURSE.EQUATE.CODES", "");
                  if (equateList == null)
                  {
                      // make sure an empty list is returned, never a null
                      equateList = new Collection<CourseEquateCodes>();
                  }
                  var equateDict = new Dictionary<string, List<string>>();
                  foreach (var item in equateList)
                  {
                      equateDict.Add(item.Recordkey, item.CecCourses);
                  }
                  return equateDict;
              }
            );
            return courseEquates;
        }

        /// <summary>
        /// Gets all equate codes from the repository and finds all of the courses that can be
        /// considered "equates" of this course. 
        /// Course equate codes consist of a list of course ids that are related to the code (record key).
        /// Two courses are equated if they have exactly the same equate codes.
        /// The two courses cannot be equated if one course has more equate codes than the other.
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="equateCodes"></param>
        /// <returns></returns>
        private async Task<List<string>> FindEquatesAsync(string courseId, List<string> equateCodes)
        {
            var equatedCourses = new List<string>();
            if (equateCodes.Count() > 0)
            {
                // Get all course equates
                var courseEquates = await GetCourseEquatesAsync();
                // Get all equate codes for this course
                var thisCourseEquates = courseEquates.Where(c => c.Value.Contains(courseId)).ToList();
                // Loop through each equate 
                foreach (var equate in thisCourseEquates)
                {
                    // loop through every course in the equate (except this course) 
                    foreach (var equateCourseId in equate.Value)
                    {
                        if (equateCourseId != courseId)
                        {
                            // get the equates for this possibly equated course
                            var otherCourseEquates = courseEquates.Where(c => c.Value.Contains(equateCourseId));

                            //need to find if  equatedCoursId will be added as equated course to current courseId
                            //it is only possible when current courseId equates contains other course equates then other course equates course id is equated course
                            //or we can say otherCouseEquates is subset of thisCourseEquates then course is added as equated course id
                            HashSet<string> thisCourseEquatesSet = new HashSet<string>(thisCourseEquates.Select(eq => eq.Key));
                            HashSet<string> otherCourseEquatesSet = new HashSet<string>(otherCourseEquates.Select(eq => eq.Key));
                            if (thisCourseEquatesSet != null && otherCourseEquatesSet != null)
                            {
                                bool isSubset = otherCourseEquatesSet.IsSubsetOf(thisCourseEquatesSet);
                                if (isSubset == true)
                                {
                                    equatedCourses.Add(equateCourseId);
                                }
                            }
                        }
                    }
                }
            }
            // Return all courses found with these equate codes, but exclude this course Id
            return equatedCourses.Where(ec => ec != courseId).ToList();
        }

        /// <summary>
        /// Get Courses Types using the COURSE.TYPES valcode table 
        /// </summary>
        /// <returns>ApplValcodes<string, CourseStatus></returns>
        private async Task<ApplValcodes> GetCourseTypesAsync()
        {
            var courseTypes = await GetOrAddToCacheAsync<ApplValcodes>("CourseTypes",
               async () =>
               {
                   ApplValcodes courseTypesValcode = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES");

                   if (courseTypesValcode == null)
                   {
                       var errorMessage = "Unable to access COURSE.TYPES valcode table.";
                       logger.Info(errorMessage);
                       throw new ColleagueWebApiException(errorMessage);
                   }
                   return courseTypesValcode;
               }
                );
            return courseTypes;
        }

        private async Task<Ellucian.Colleague.Data.Student.DataContracts.CdDefaults> GetCourseParametersAsync()
        {
            if (courseParameters != null)
            {
                return courseParameters;
            }
            // Overriding cache timeout to be Level1 Cache time out for data that rarely changes.
            courseParameters = await GetOrAddToCacheAsync<Ellucian.Colleague.Data.Student.DataContracts.CdDefaults>("CourseParameters",
               async () =>
               {
                   Data.Student.DataContracts.CdDefaults courseParams = await DataReader.ReadRecordAsync<Data.Student.DataContracts.CdDefaults>("ST.PARMS", "CD.DEFAULTS");
                   if (courseParams == null)
                   {
                       var errorMessage = "Unable to access course parameters CD.DEFAULTS to determine CoreqPrereq convertion flag. Defaulting to unconverted.";
                       logger.Info(errorMessage);
                       // If we cannot read the course parameters - default to "unconverted".
                       // throw new ColleagueWebApiException(errorMessage);
                       Data.Student.DataContracts.CdDefaults newCourseParams = new Data.Student.DataContracts.CdDefaults();
                       newCourseParams.CdReqsConvertedFlag = "N";
                       courseParams = newCourseParams;
                   }
                   return courseParams;
               }, Level1CacheTimeoutValue);
            return courseParameters;

        }

        /// <summary>
        /// Get the CreditType enumeration value for a credit type code
        /// </summary>
        /// <param name="typecode">Credit Type code</param>
        /// <returns>CreditType enumeration value</returns>
        private async Task<CreditType> GetCreditTypeAsync(string typecode)
        {
            if (types.Count() == 0)
            {
                // STC.TYPE => CRED.TYPES => CRTP.CODE, CRTP.CATEGORY

                var creditTypes = await GetOrAddToCacheAsync<List<CredTypes>>("AllCreditTypes",
                    async () =>
                    {
                        var credTypes = await DataReader.BulkReadRecordAsync<CredTypes>("CRED.TYPES", "");
                        return credTypes.ToList();
                    }
                , Level1CacheTimeoutValue);

                // There is a Valcode CRED.TYPE.TYPES but it's not used for anything.  In Colleague, things
                // either have a CRTP.CATEGORY of "I" or they don't.

                if (creditTypes != null)
                {
                    foreach (var ct in creditTypes)
                    {
                        CreditType type;

                        switch (ct.CrtpCategory)
                        {
                            case "I":
                                {
                                    type = CreditType.Institutional;
                                    break;
                                }
                            case "C":
                                {
                                    type = CreditType.ContinuingEducation;
                                    break;
                                }
                            case "T":
                                {
                                    type = CreditType.Transfer;
                                    break;
                                }
                            default:
                                {
                                    type = CreditType.Other;
                                    break;
                                }

                        }
                        types.Add(ct.Recordkey, type);
                    }
                }
                else
                {
                    throw new ColleagueWebApiException("Transaction failed to return credit types");
                }
            }

            if (typecode != null && typecode != "" && types.Keys.Contains(typecode))
            {
                return types[typecode];
            }
            else
            {
                return CreditType.Other;
            }
        }

        /// <summary>
        /// Offering departments.
        /// </summary>
        private async Task<IEnumerable<AcademicDepartment>> AcademicDepartmentsAsync()
        {
            return await GetGuidCodeItemAsync<Depts, AcademicDepartment>("AllOfferingDepartments", "DEPTS",
                (d, g) => new AcademicDepartment(g, d.Recordkey, d.DeptsDesc, "A".Equals(d.DeptsActiveFlag)) { AcademicLevelCode = d.DeptsAcadLevel, GradeSchemeCode = d.DeptsGradeScheme });
        }

        /// <summary>
        /// Creates or updates a course
        /// </summary>
        /// <param name="course">Course to be maintained</param>
        /// <param name="source">Source for the maintenance</param>
        /// <param name="isUpdating">Indicates whether a course is being updated</param>
        /// <returns>Created or updated course entity</returns>
        private async Task<Course> MaintainCourseAsync(Course course, string source, bool isUpdating, string version)
        {
            if (course == null)
            {
                throw new ArgumentNullException("course", "Course must be provided.");
            }

            string courseId = null;
            string externalSource = null;
            if (isUpdating)
            {
                if (!string.IsNullOrEmpty(course.Guid))
                {
                    courseId = GetRecordKeyFromGuid(course.Guid);
                    if (string.IsNullOrEmpty(courseId))
                    {
                        isUpdating = false;
                        externalSource = source;
                    }
                }
                else
                {
                    isUpdating = false;
                    externalSource = source;
                }
            }
            else
            {
                externalSource = source;
            }

            var courseStatuses = await GetCourseStatusesAsync();
            var creditTypes = await GetCreditTypesAsync();

            var request = new UpdateCoursesRequest()
            {
                Version = version,
                CoursesId = courseId,
                CrsGuid = course.Guid,
                CrsSubject = course.SubjectCode,
                CrsNo = course.Number,
                CrsStartDate = course.StartDate,
                CrsEndDate = course.EndDate,
                CrsCredType = course.LocalCreditType,
                CrsMinCred = course.MinimumCredits,
                CrsMaxCred = course.MaximumCredits,
                CrsVarCredIncrement = course.VariableCreditIncrement,
                CrsCeus = course.Ceus,
                CrsAcadLevel = course.AcademicLevelCode,
                CrsGradeScheme = course.GradeSchemeCode,
                CrsShortTitle = course.Title,
                CrsTitle = course.LongTitle,
                CrsDesc = course.Description,
                CrsExternalSource = externalSource,
                CrsBillingCred = course.BillingCredits,
                CrsWaitlistMultSectFlag = course.AllowWaitlistMultipleSections,
                CrsCourseTypes = course.CourseTypeCodes == null ? null : course.CourseTypeCodes.ToList(),
                CrsTopicCode = course.TopicCode,
                CrsCip = course.FederalCourseClassification,
                CrsLevels = course.CourseLevelCodes
            };

            // Get Extended Data names and values
            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }


            if (course.Departments != null && course.Departments.Any())
            {
                foreach (var dept in course.Departments)
                {
                    request.CourseDepts.Add(new CourseDepts()
                    {
                        CrsDepts = dept.AcademicDepartmentCode,
                        CrsDeptPcts = dept.ResponsibilityPercentage
                    });
                }
            }

            if (course.CourseApprovals != null && course.CourseApprovals.Any())
            {
                foreach (var approvalInfo in course.CourseApprovals)
                {
                    request.ApprovalStatus.Add(new ApprovalStatus()
                    {
                        CrsApprovalIds = approvalInfo.ApprovingPersonId,
                        CrsApprovalAgencyIds = approvalInfo.ApprovingAgencyId,
                        CrsApprovalDate = approvalInfo.Date,
                        CrsStatus = approvalInfo.StatusCode,
                        CrsStatusDate = approvalInfo.StatusDate
                    });
                }
            }

            // V20 changes
            if (course.InstructionalMethodCodes != null && course.InstructionalMethodCodes.Any())
            {
                foreach (var code in course.InstructionalMethodCodes)
                {
                    var pos = course.InstructionalMethodCodes.IndexOf(code);
                    decimal? methodHours = null;
                    string methodPeriod = null;
                    if (course.InstructionalMethodContactHours != null && course.InstructionalMethodContactHours.Count > pos)
                    {
                        methodHours = course.InstructionalMethodContactHours[pos];
                    }
                    if (course.InstructionalMethodContactPeriods != null && course.InstructionalMethodContactPeriods.Count > pos)
                    {
                        methodPeriod = course.InstructionalMethodContactPeriods[pos];
                    }
                    request.CourseContact.Add(new CourseContact()
                    {
                        CrsInstrMethods = code,
                        CrsContactHours = methodHours,
                        CrsContactMeasures = methodPeriod
                    });
                }
            }

            var response = await transactionInvoker.ExecuteAsync<UpdateCoursesRequest, UpdateCoursesResponse>(request);

            if (response.ErrorMsg != null && response.ErrorMsg.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating courses '{0}':", request.CrsGuid);
                var exception = new RepositoryException(errorMessage);
                response.ErrorMsg.ForEach(error => exception.AddError(new RepositoryError("Create.Update.Exception", error)
                {
                    Id = request.CrsGuid
                }
                ));
                logger.Error(errorMessage);
                throw exception;
            }

            // Create the new object to be returned
            var createdCourse = await GetCourseByGuidAsync(response.CrsGuid);

            return createdCourse;
        }

        /// <summary>
        /// Get the GUID for a course.
        /// </summary>
        /// <param name="primaryKey">The value of the primary key</param>
        /// <returns>The corresponding GUID</returns>
        public async Task<string> GetCourseGuidFromIdAsync(string primaryKey)
        {
            if (string.IsNullOrEmpty(primaryKey))
                throw new ArgumentNullException("primaryKey", "The primary key is a required argument.");

            var lookup = new RecordKeyLookup("COURSES", primaryKey, false);
            var result = await DataReader.SelectAsync(new RecordKeyLookup[] { lookup });
            if (result != null && result.Count > 0)
            {
                RecordKeyLookupResult lookupResult = null;
                if (result.TryGetValue(lookup.ResultKey, out lookupResult))
                {
                    if (lookupResult != null)
                    {
                        return lookupResult.Guid;
                    }
                }
            }

            throw new ArgumentOutOfRangeException("primaryKey", "No GUID found for course ID " + primaryKey);
        }

        /// <summary>
        /// Using a collection of  ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of  ids</param>
        /// <returns>Dictionary consisting of a ids (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids, string filename)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup(filename, p, false)).ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryException(string.Format("Error occured while getting guids for {0}.", filename), ex); ;
            }

            return guidCollection;
        }
    }
}