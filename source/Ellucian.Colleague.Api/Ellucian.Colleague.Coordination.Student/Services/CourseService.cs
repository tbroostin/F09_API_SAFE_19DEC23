// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using slf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Course2 = Ellucian.Colleague.Dtos.Student.Course2;
using CreditCategory = Ellucian.Colleague.Domain.Student.Entities.CreditCategory;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// CourseService is an application service that responds to search requests for information from an institutions course catalog
    /// It relies upon Lucene.net, a free set of utilities to assist with building search engines
    /// Lucene cliff notes - the Lucene index stores a set of documents.  Each document contains fields that may be searchable.
    ///                    - queries can be built against the index using combinations of terms/words, indexed fields, and weight/booster values
    ///                    - see comments in the methods below for more detail
    /// </summary>
    [RegisterType]
    public class CourseService : BaseCoordinationService, ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IRequirementRepository _requirementRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ITermRepository _termRepository;
        private readonly IRuleRepository _ruleRepository;
        private readonly IStudentConfigurationRepository _studentConfigRepository;
        private readonly IConfigurationRepository _configurationRepository;

        // Lists for lazy caching of code collections
        private List<Domain.Base.Entities.Department> _departments = null;
        private List<Domain.Base.Entities.Location> _locations = null;
        private List<Domain.Student.Entities.AcademicDepartment> _academicDepartments = null;
        private List<Domain.Student.Entities.AcademicLevel> _academicLevels = null;
        private List<Domain.Student.Entities.CourseLevel> _courseLevels = null;
        private List<Domain.Student.Entities.CreditCategory> _creditCategories = null;
        private List<Domain.Student.Entities.GradeScheme> _gradeSchemes = null;
        private List<Domain.Student.Entities.InstructionalMethod> _instructionalMethods = null;
        private List<Domain.Student.Entities.AdministrativeInstructionalMethod> _administrativeInstructionalMethods = null;
        private List<Domain.Student.Entities.Subject> _subjects = null;
        private List<Domain.Student.Entities.TopicCode> _topicCodes = null;
        private IEnumerable<Department> _departmentEntities = null;
        private List<Domain.Student.Entities.ContactMeasure> _contactMeasures = null;
        private List<Domain.Student.Entities.CourseType> _courseCategories = null;
        private List<Domain.Student.Entities.CourseTitleType> _courseTitleTypes = null;
        private List<Domain.Student.Entities.CourseStatuses> _courseStatuses = null;

        //
        //private IEnumerable<CreditCategory> _creditCategoriesEntitites = null;
        //private IEnumerable<Domain.Student.Entities.GradeScheme> _gradeSchemesEntities = null;
        //private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevelsEntities = null;
        //private IEnumerable<Domain.Student.Entities.InstructionalMethod> _instructionalMethodEntites = null;
        //private IEnumerable<Domain.Student.Entities.CourseLevel> _courseLevelsEntities = null;
        //private IEnumerable<Domain.Student.Entities.Subject> _subjectEntities = null;


        /// <summary>
        /// Store the version of Lucene.net being used - required in the constructor of several Lucene objects
        /// </summary>
        private static Lucene.Net.Util.Version LuceneVersion = Lucene.Net.Util.Version.LUCENE_29;

        /// <summary>
        /// The course search engine's index, stored in memory
        /// </summary>
        private static RAMDirectory CourseIndex;
        private static object IndexLock = new object();
        private static DateTime indexBuildTime = new DateTime();

        /// <summary>
        /// A query analyzer, used in the construction and parsing of queries against the index
        /// </summary>
        private static StandardAnalyzer Analyzer = new StandardAnalyzer(LuceneVersion);

        /// <summary>
        /// Default value for the course delimiter
        /// </summary>
        public static string CourseDelimiter = "-";

        public CourseService(IAdapterRegistry adapterRegistry, ICourseRepository courseRepository,
            IReferenceDataRepository referenceDataRepository, IStudentReferenceDataRepository studentReferenceDataRepository,
            IRequirementRepository requirementRepository, ISectionRepository sectionRepository, ITermRepository termRepository,
            IRuleRepository ruleRepository, IStudentConfigurationRepository studentConfigRepository, IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger) :
            base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _courseRepository = courseRepository;
            _referenceDataRepository = referenceDataRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _requirementRepository = requirementRepository;
            _sectionRepository = sectionRepository;
            _termRepository = termRepository;
            _ruleRepository = ruleRepository;
            _studentConfigRepository = studentConfigRepository;
            _configurationRepository = configurationRepository;
        }

        private class CourseSectionResult
        {
            public List<Ellucian.Colleague.Domain.Student.Entities.Course> Courses = new List<Ellucian.Colleague.Domain.Student.Entities.Course>();
            public List<Ellucian.Colleague.Domain.Student.Entities.Section> Sections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
        }

        /// <summary>
        /// OBSOLETE AS OF API 1.3 - Use Search2
        /// Returns a page of courses and sections based on the supplied search and filters. Includes support of:
        ///    subject (course filter)
        ///    academic level (course filter)
        ///    course level (course filter)
        ///    location (section filter)
        /// </summary>
        /// <param name="criteria">Course search criteria</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="pageIndex">Page number to return</param>
        /// <returns></returns>
        [Obsolete("Obsolete as of API version 1.3. Use Search2")]
        public async Task<CoursePage> SearchAsync(CourseSearchCriteria criteria, int pageSize, int pageIndex)
        {
            // Courses and Sections Returned by filter
            var filterResult = await BuildFilterResultAsync(criteria, pageSize, pageIndex);

            // BUILD PAGE
            // Builds Course DTO and Associates ids of related sections to each course
            var filteredCourseDtos = BuildCourseSearchDto(filterResult);
            // Return only the requested page from the filtered subset
            CoursePage coursePage = new CoursePage(filteredCourseDtos, pageSize, pageIndex);

            // Build outgoing filters. Built against the results from the overall search and all current filters.
            var filterBasis = filterResult;
            coursePage.Subjects = BuildSubjectFilter(filterBasis.Courses, criteria);
            coursePage.AcademicLevels = BuildAcademicLevelFilter(filterBasis.Courses, criteria);
            coursePage.CourseLevels = BuildCourseLevelFilter(filterBasis.Courses, criteria);
            coursePage.Locations = await BuildLocationFilterAsync(filterBasis, criteria);
            coursePage.Faculty = BuildFacultyFilter(filterBasis.Sections, criteria);
            coursePage.DaysOfWeek = BuildDayOfWeekFilter(filterBasis.Sections, criteria);
            coursePage.CourseTypes = await BuildCourseTypeFilterAsync(filterBasis.Sections, criteria);
            coursePage.TopicCodes = BuildTopicCodeFilter(filterBasis, criteria);
            coursePage.Terms = BuildTermFilter(filterBasis.Sections, criteria);
            coursePage.EarliestTime = criteria.EarliestTime;
            coursePage.LatestTime = criteria.LatestTime;

            return coursePage;
        }

        /// <summary>
        /// Returns a page of courses and sections based on the supplied search and filters. Includes support of:
        ///    subject (course filter)
        ///    academic level (course filter)
        ///    course level (course filter)
        ///    location (section filter)
        /// </summary>
        /// <param name="criteria">Course search criteria</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="pageIndex">Page number to return</param>
        /// <returns>A CoursePage2 DTO which is essentially a data page of type CourseSearch2 along with associated filters</returns>
        public async Task<CoursePage2> Search2Async(CourseSearchCriteria criteria, int pageSize, int pageIndex)
        {
            // Courses and Sections Returned by filter
            var filterResult = await BuildFilterResultAsync(criteria, pageSize, pageIndex);

            // BUILD PAGE
            // Builds Course2 DTO and Associates ids of related sections to each course
            var filteredCourse2Dtos = BuildCourseSearch2Dto(filterResult);
            // Return only the requested page from the filtered subset
            CoursePage2 coursePage = new CoursePage2(filteredCourse2Dtos, pageSize, pageIndex);

            // Build outgoing filters. Built against the results from the overall search and all current filters.
            var filterBasis = filterResult;
            coursePage.Subjects = BuildSubjectFilter(filterBasis.Courses, criteria);
            coursePage.AcademicLevels = BuildAcademicLevelFilter(filterBasis.Courses, criteria);
            coursePage.CourseLevels = BuildCourseLevelFilter(filterBasis.Courses, criteria);
            coursePage.Locations = await BuildLocationFilterAsync(filterBasis, criteria);
            coursePage.Faculty = BuildFacultyFilter(filterBasis.Sections, criteria);
            coursePage.DaysOfWeek = BuildDayOfWeekFilter(filterBasis.Sections, criteria);
            coursePage.CourseTypes = await BuildCourseTypeFilterAsync(filterBasis.Sections, criteria);
            coursePage.TopicCodes = BuildTopicCodeFilter(filterBasis, criteria);
            coursePage.Terms = BuildTermFilter(filterBasis.Sections, criteria);
            coursePage.EarliestTime = criteria.EarliestTime;
            coursePage.LatestTime = criteria.LatestTime;
            coursePage.OnlineCategories = BuildOnlineCategoryFilter(filterBasis.Sections, criteria);
            coursePage.OpenSections = BuildOpenSectionsFilter(filterBasis.Sections, criteria);
            return coursePage;
        }

       

        public static void ClearIndex()
        {
            lock (IndexLock)
            {
                CourseIndex = null;
            }
        }

        private async Task<IEnumerable<Domain.Student.Entities.Course>> GetCatalogAsync()
        {
            // List of subjects to include in the catalog
            var catalogSubjectCodes = (await _studentReferenceDataRepository.GetSubjectsAsync()).Where(s => s.ShowInCourseSearch == true).Select(s => s.Code);

            // Get all courses in the given subjects
            var courses = (await _courseRepository.GetAsync())
                                .Where(c => c.IsCurrent == true && catalogSubjectCodes.Contains(c.SubjectCode) && c.IsPseudoCourse == false)
                                .OrderBy(c => c.SubjectCode)
                                .ThenBy(c => c.Number);

            return courses;
        }

        /// <summary>
        /// For a given set of courses - get the catalog sections associated to them making sure to NOT include any section that is not active and is not viewable in the catalog.
        /// </summary>
        /// <param name="courses">Courses for which associated sections are requested</param>
        /// <returns>Active Sections</returns>
        private async Task<IEnumerable<Domain.Student.Entities.Section>> GetSectionsForCoursesAsync(IEnumerable<Domain.Student.Entities.Course> courses)
        {
            IEnumerable<Domain.Student.Entities.Section> registeredSections = new List<Domain.Student.Entities.Section>();
            // Registration terms used to limit sections retrieved
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
             registeredSections = (await _sectionRepository.GetRegistrationSectionsAsync(registrationTerms));
            // Get all sections for the selected courses
            var sections = (from crs in courses
                            join sec in registeredSections.Where(s => s.IsActive && !s.HideInCatalog)
                            on crs.Id equals sec.CourseId into joinCourseSection
                            from crsSec in joinCourseSection
                            select crsSec);
            return sections;
        }

        #region Filter

        private async Task<CourseSectionResult> FilterResultsAsync(CourseSectionResult searchResult, CourseSearchCriteria criteria)
        {
            var sw = new Stopwatch();
            sw.Start();
            var filterResult = new CourseSectionResult();

            // Course filters--start with list of courses from search (keyword or requirement)
            filterResult.Courses = searchResult.Courses;
            // Filter courses based on selected subjects
            filterResult.Courses = FilterBySubjects(filterResult.Courses, criteria.Subjects).ToList();
            // filter courses based on selected course levels
            filterResult.Courses = FilterByCourseLevels(filterResult.Courses, criteria.CourseLevels).ToList();

            // LOCATION, TOPIC CODE, AND ACADEMIC LEVEL FILTERS APPLY TO BOTH COURSES AND SECTIONS
            bool updateCoursesFromSections = false;
            // If there is a location filter, filter the sections
            if ((criteria.Locations != null) && (criteria.Locations.Any()))
            {
                // Start with all the sections for the search list of courses. This step must be taken to ensure that if any
                // sections were identified during the initial (keyword) search, only those sections are included in the filtering,
                // and the sequence of those sections must be preserved as well.
                var filterSections = (await RetrieveSectionsForFilteredCoursesAsync(filterResult.Courses, searchResult.Sections)).ToList();
                // Filter the sections using location.
                filterResult.Sections = FilterSectionsByLocations(filterSections, criteria.Locations).ToList();
                // Filter courses based on selected locations 
                filterResult.Courses = FilterCoursesByLocations(filterResult.Courses, criteria.Locations).ToList();
                // Make sure we have a course for every filtered section (course may have been filtered out).
                updateCoursesFromSections = true;
            }
            else
            {
                // There are no values in the location filters, therefore get sections for only the courses.
                filterResult.Sections = (await RetrieveSectionsForFilteredCoursesAsync(filterResult.Courses, searchResult.Sections)).ToList();
            }

            // If there is a topic code filter, filter the sections first
            if ((criteria.TopicCodes != null) && (criteria.TopicCodes.Any()))
            {
                // Filter the sections using topic codes.
                filterResult.Sections = FilterSectionsByTopicCode(filterResult.Sections, criteria.TopicCodes).ToList();
                // Filter courses based on selected topic codes 
                filterResult.Courses = FilterByTopicCodes(filterResult.Courses, criteria.TopicCodes).ToList();
                // Make sure we have a course for every filtered section (course may have been filtered out).
                updateCoursesFromSections = true;
            }

            // If there is an academic level filter, filter the sections first
            if (criteria.AcademicLevels != null && criteria.AcademicLevels.Any())
            {
                // Filter the sections using academic levels.
                filterResult.Sections = FilterSectionsByAcademicLevel(filterResult.Sections, criteria.AcademicLevels).ToList();
                // filter courses based on selected academic levels
                filterResult.Courses = FilterByAcademicLevels(filterResult.Courses, criteria.AcademicLevels).ToList();
                // make sure we have a course for every filtered section (course may have been filtered out).
                updateCoursesFromSections = true;
            }

            if (updateCoursesFromSections)
            {
                // This means either location or topic or academic level or some combination of all 3 filters were built so we now need to make sure we have courses to match all the sections
                filterResult.Courses = (await RetrieveCoursesForFilteredSectionsAsync(filterResult)).ToList();
            }

            // Section Only filters
            bool sectionFiltersUsed = false;

            // Faculty filter
            if ((criteria.Faculty != null) && (criteria.Faculty.Any()))
            {
                filterResult.Sections = FilterSectionsByFaculty(filterResult.Sections, criteria.Faculty).ToList();
                sectionFiltersUsed = true;
            }

            // DayOfWeek filter
            if ((criteria.DaysOfWeek != null) && (criteria.DaysOfWeek.Any()))
            {
                // Filter sections based on selected days of the week
                filterResult.Sections = FilterSectionsByDayOfWeek(filterResult.Sections, criteria.DaysOfWeek).ToList();
                sectionFiltersUsed = true;
            }
            // Create a join of all section meetings to determine if the filter should be used
            var meetingCollection = from sec in filterResult.Sections
                                    from mtg in sec.Meetings
                                    select new { mtg, sec };
            // Determine earliest start time and latest end time
            var collectionEarliestTime = meetingCollection.Select(mc => mc.mtg.StartTime).Min().GetValueOrDefault().DateTime.TimeOfDay.TotalMinutes;
            var collectionLatestTime = meetingCollection.Select(mc => mc.mtg.EndTime).Max().GetValueOrDefault().DateTime.TimeOfDay.TotalMinutes;
            // If provided start/end times fall outside the criteria time range, filter sections
            if ((collectionEarliestTime < criteria.EarliestTime || collectionLatestTime > criteria.LatestTime) && (!(criteria.EarliestTime == criteria.LatestTime)))
            {
                filterResult.Sections = FilterSectionsByTimeOfDay(filterResult.Sections, criteria.EarliestTime, criteria.LatestTime).ToList();
                sectionFiltersUsed = true;
            }

            // Course Type filter
            if ((criteria.CourseTypes != null) && (criteria.CourseTypes.Any()))
            {
                filterResult.Sections = FilterSectionsByCourseType(filterResult.Sections, criteria.CourseTypes).ToList();
                sectionFiltersUsed = true;
            }

            // Term filter
            if ((criteria.Terms != null) && (criteria.Terms.Any()))
            {
                filterResult.Sections = FilterSectionsByTerm(filterResult.Sections, criteria.Terms).ToList();
                sectionFiltersUsed = true;
            }


            // Section Start and End Date filters
            if (criteria.SectionStartDate.HasValue || criteria.SectionEndDate.HasValue)
            {
                filterResult.Sections = FilterSectionsByDate(filterResult.Sections, criteria.SectionStartDate, criteria.SectionEndDate).ToList();
                sectionFiltersUsed = true;
            }

            // Online Category filter
            if ((criteria.OnlineCategories != null) && (criteria.OnlineCategories.Any()))
            {
                // Online Category filter. Do not flag the use of AnyOnlineCategory as a filter, since that option returns all sections.
                filterResult.Sections = FilterSectionsByOnlineCategory(filterResult.Sections, criteria.OnlineCategories).ToList();
                // Count this filter as "used" if filters are specified and it's not the "any" filter, which would include all sections
                sectionFiltersUsed = true;
            }

            //filter sections that are open 
            if (criteria.OpenSections)
            {
                var sectionsDict = filterResult.Sections.ToDictionary(s => s.Id, s => s);
                await FilterOpenSectionsOnlyAsync(sectionsDict);
                filterResult.Sections = sectionsDict.Select(s => s.Value).ToList();
                sectionFiltersUsed = true;
            }

            // When (and after) section-only filters have been applied, limit the courses to the filtered sections
            if (sectionFiltersUsed)
            {
                var courseIds = filterResult.Sections.Select(s => s.CourseId).Distinct();
                var limitedCourses = (from id in courseIds
                                      join resultCourse in filterResult.Courses
                                      on id equals resultCourse.Id into joinCourse
                                      from course in joinCourse
                                      select course).ToList();
                filterResult.Courses = limitedCourses;
            }

            sw.Stop();
            logger.Debug("CourseSearch: FilterResults: " + sw.ElapsedMilliseconds + "ms");
            return filterResult;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> FilterBySubjects(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseCollection, IEnumerable<string> subjects)
        {
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> filteredCollection = courseCollection;
            if (subjects != null && subjects.Any())
            {
                filteredCollection = from subject in subjects
                                     join course in courseCollection
                                     on subject equals course.SubjectCode into joinCourseSubject
                                     from filteredCourse in joinCourseSubject
                                     select filteredCourse;
            }

            return filteredCollection;
        }

        /// <summary>
        /// Filter a set of courses by topic codes
        /// </summary>
        /// <param name="courseCollection">The set of courses to filter</param>
        /// <param name="topicCodes">The list of topic codes that are used in the filter</param>
        /// <returns>A set of courses filtered by topic codes</returns>
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> FilterByTopicCodes(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseCollection, IEnumerable<string> topicCodes)
        {
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> filteredCollection = courseCollection;

            if (topicCodes != null && topicCodes.Any())
            {
                filteredCollection = (from topicCode in topicCodes
                                      join crs in courseCollection
                                      on topicCode equals crs.TopicCode into joinSectionTopicCode
                                      from filteredSection in joinSectionTopicCode
                                      select filteredSection).ToList();
            }

            return filteredCollection;
        }


        /// <summary>
        /// Assemble sections for the given courses, starting with the given sections.
        /// </summary>
        /// <param name="courses"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>> RetrieveSectionsForFilteredCoursesAsync(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> filteredCourses, IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections)
        {
            logger.Debug("Start RetrieveSectionsForFilteredCourses: " + filteredCourses.Count() + " courses and " + sections.Count() + " sections.");
            var sw = new Stopwatch();
            sw.Start();
            // Create a new list of sections to return
            var returnSections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            // Get the registration terms from the repository
            var registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            // First get the sections from the original incoming list, but only those for which there is a course in the incoming course list.
            foreach (var sec in sections)
            {
                if (filteredCourses.Select(c => c.Id).Contains(sec.CourseId))
                {
                    returnSections.Add(sec);
                }
            }
            // Get related sections from the repository if none found in the original list of sections
            List<string> courseIds = new List<string>();
            foreach (var crs in filteredCourses)
            {
                // Get sections for this course from the input list of sections (any sections found by the initial search)
                var searchSection = returnSections.Where(s => s.CourseId == crs.Id).FirstOrDefault();

                if ((searchSection == null))
                {
                    // If no sections for this course are already in the returnSections list, add this course Id into the list to be used to pull its sections from the repository.
                    courseIds.Add(crs.Id);
                }
            }
            // Get all the active sections for any course that did not have sections already in the return sections list.
            if (courseIds.Any())
            {
                returnSections.AddRange((await _sectionRepository.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.IsActive == true && !s.HideInCatalog));
            }

            sw.Stop();
            logger.Debug("CourseSearch: RetrieveSectionsForFilteredCourses returned " + returnSections.Count() + " sections in " + sw.ElapsedMilliseconds + "ms");
            return returnSections;
        }

        /// Ensure there is a course for every section in the filtered result.
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>> RetrieveCoursesForFilteredSectionsAsync(CourseSectionResult filterResult)
        {
            var sw = new Stopwatch();
            sw.Start();
            // Must go through all the sections and determine if the section's course exists in the
            // courses list. If not, get the course from the repository. Add the course to the end of
            // the list of filtered courses to preserve sequence from original keyword search result.
            foreach (var sec in filterResult.Sections)
            {
                Ellucian.Colleague.Domain.Student.Entities.Course course = null;
                if (filterResult.Courses.Any())
                {
                    course = filterResult.Courses.Where(c => c.Id == sec.CourseId).FirstOrDefault();
                }
                if (course == null)
                {
                    // Course is not currently in the filtered course list, get it from the repository
                    course = await _courseRepository.GetAsync(sec.CourseId);
                    if (course != null)
                    {
                        // Add course to the list of filtered courses
                        filterResult.Courses.Add(course);
                    }
                }
            }
            sw.Stop();
            logger.Debug("CourseSearch: RetrieveCoursesForFilteredSections: " + sw.ElapsedMilliseconds + "ms");
            return filterResult.Courses;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> FilterByAcademicLevels(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseCollection, IEnumerable<string> academicLevels)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Course> filteredCollection = courseCollection.ToList();
            if (academicLevels != null && academicLevels.Any())
            {
                filteredCollection = (from academicLevel in academicLevels
                                      join course in courseCollection
                                      on academicLevel equals course.AcademicLevelCode into joinCourseAcademicLevel
                                      from filteredCourse in joinCourseAcademicLevel
                                      select filteredCourse).ToList();
            }

            return filteredCollection;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> FilterByCourseLevels(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseCollection, IEnumerable<string> courseLevels)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Course> filteredCollection = courseCollection.ToList();
            if (courseLevels != null && courseLevels.Any())
            {
                // There may be multiple course levels on a course, so this linq is a little different from that of the other filters.
                var newCollection = from courseLevel in courseLevels
                                    let crsLevelCourses = from course in courseCollection
                                                          where course.CourseLevelCodes.Contains(courseLevel)
                                                          select course
                                    select new { crses = crsLevelCourses };
                filteredCollection = newCollection.SelectMany(c => c.crses).Distinct().ToList();
            }

            return filteredCollection;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> FilterCoursesByLocations(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseCollection, IEnumerable<string> locations)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Course> filteredCollection = courseCollection.ToList();
            if (locations != null && locations.Any())
            {
                // Select courses with one of the specified location or with no locations.
                // There may be multiple locations on a course. Null location means "all" locations.
                var newCollection = from location in locations
                                    let crsLocationCourses = from course in courseCollection
                                                             where (course.LocationCodes.Contains(location))
                                                             select course
                                    select new { crses = crsLocationCourses };
                filteredCollection = newCollection.SelectMany(c => c.crses).Distinct().ToList();
            }

            return filteredCollection;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByLocations(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, IEnumerable<string> locations)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Section> filteredSections;
            if (locations != null && locations.Any())
            {
                filteredSections = (from loc in locations
                                    join sec in sectionCollection
                                    on loc equals sec.Location into joinSectionLocation
                                    from filteredSection in joinSectionLocation
                                    select filteredSection).ToList();
            }
            else
            {
                filteredSections = sectionCollection.ToList();
            }
            return filteredSections;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByFaculty(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, IEnumerable<string> faculty)
        {
            // There may be multiple faculty assigned to a section, therefore the linq is a little different from other filters
            List<Ellucian.Colleague.Domain.Student.Entities.Section> filteredCollection = sectionCollection.ToList();
            if (faculty != null && faculty.Any())
            {
                var newCollection = from fac in faculty
                                    let secFacSections = from sec in sectionCollection
                                                         where sec.FacultyIds.Contains(fac)
                                                         select sec
                                    select new { sections = secFacSections };
                filteredCollection = newCollection.SelectMany(s => s.sections).Distinct().ToList();
            }

            return filteredCollection;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByDayOfWeek(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, IEnumerable<string> daysOfWeek)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Section> filteredCollection = sectionCollection.ToList();
            if (daysOfWeek != null && daysOfWeek.Any())
            {
                filteredCollection = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
                // Build a collection of day of week/section for query by day of week
                var dayCollection = from sec in sectionCollection
                                    from mtg in sec.Meetings
                                    from dayOfWk in mtg.Days
                                    select new { dayOfWk, sec };
                foreach (var day in daysOfWeek)
                {
                    // Add the list of sections that have the specified day of week in the meeting information
                    filteredCollection.AddRange(dayCollection.Where(dc => ((int)dc.dayOfWk).ToString() == day).Select(dc => dc.sec).Distinct().ToList());
                }
            }
            filteredCollection = filteredCollection.Distinct().ToList();
            return filteredCollection;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByTimeOfDay(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, int earliestTime, int latestTime)
        {
            var filteredCollection = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            // Create a join of all section meetings to determine if the filter should be used
            var meetingCollection = from sec in sectionCollection
                                    from mtg in sec.Meetings
                                    select new { mtg, sec };
            // Determine earliest start time and latest end time
            var collectionEarliestTime = meetingCollection.Select(mc => mc.mtg.StartTime).Min().GetValueOrDefault().DateTime.TimeOfDay.TotalMinutes;
            var collectionLatestTime = meetingCollection.Select(mc => mc.mtg.EndTime).Min().GetValueOrDefault().DateTime.TimeOfDay.TotalMinutes;
            // If provided start/end times fall outside the criteria time range, filter sections
            if (collectionEarliestTime > earliestTime || collectionLatestTime < latestTime)
            {
                // Select the sections that meet limited times by expanding and examining meetings of all provided sections
                filteredCollection = (from sec in sectionCollection
                                      from mtg in sec.Meetings
                                      where ((mtg.StartTime.GetValueOrDefault().DateTime.TimeOfDay == TimeSpan.MaxValue) ||
                                             (mtg.StartTime.GetValueOrDefault().DateTime.TimeOfDay.TotalMinutes >= earliestTime)) &&
                                            ((mtg.EndTime.GetValueOrDefault().DateTime.TimeOfDay == TimeSpan.MaxValue) ||
                                             (mtg.EndTime.GetValueOrDefault().DateTime.TimeOfDay.TotalMinutes <= latestTime))
                                      select sec).ToList();
            }
            return filteredCollection.Distinct();
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByCourseType(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, IEnumerable<string> courseTypes)
        {
            // There may be multiple course types assigned to a section, therefore the linq is a little different from the single-valued field filters
            List<Ellucian.Colleague.Domain.Student.Entities.Section> filteredCollection = sectionCollection.ToList();
            if (courseTypes != null && courseTypes.Any())
            {
                var newCollection = from type in courseTypes
                                    let secCourseTypeSections = from sec in sectionCollection
                                                                where sec.CourseTypeCodes.Contains(type)
                                                                select sec
                                    select new { sections = secCourseTypeSections };
                filteredCollection = newCollection.SelectMany(s => s.sections).Distinct().ToList();
            }

            return filteredCollection;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByTerm(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, IEnumerable<string> terms)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Section> filteredSections;
            if (terms != null && terms.Any())
            {
                filteredSections = (from term in terms
                                    join sec in sectionCollection
                                    on term equals sec.TermId into joinSectionLocation
                                    from filteredSection in joinSectionLocation
                                    select filteredSection).ToList();
            }
            else
            {
                filteredSections = sectionCollection.ToList();
            }
            return filteredSections;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByDate(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, DateTime? sectionStartDate, DateTime? sectionEndDate)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Section> filteredSections;

            // If there is both a start and end date supplied as criteria, select only those sections with a first meeting date between supplied dates,
            // AND with either no last meeting date or a last meeting date that is less than the supplied end date criteria.
            if (sectionStartDate.HasValue && sectionEndDate.HasValue)
            {
                filteredSections = sectionCollection.Where(s =>
                    (!s.FirstMeetingDate.HasValue || (DateTime.Compare(s.FirstMeetingDate.Value, sectionStartDate.Value) >= 0 && DateTime.Compare(s.FirstMeetingDate.Value, sectionEndDate.Value) <= 0))
                    && (!s.LastMeetingDate.HasValue || DateTime.Compare(s.LastMeetingDate.Value, sectionEndDate.Value) <= 0)).ToList();
            }
            else if (sectionStartDate.HasValue)
            {
                filteredSections = sectionCollection.Where(s => !s.FirstMeetingDate.HasValue || DateTime.Compare(s.FirstMeetingDate.Value, sectionStartDate.Value) >= 0).ToList();
            }
            else if (sectionEndDate.HasValue)
            {
                filteredSections = sectionCollection.Where(s => !s.LastMeetingDate.HasValue || DateTime.Compare(s.LastMeetingDate.Value, sectionEndDate.Value) <= 0).ToList();
            }
            else
            {
                filteredSections = sectionCollection.ToList();
            }
            return filteredSections;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByOnlineCategory(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, IEnumerable<string> onlineCategories)
        {
            var filteredSections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            // Get the dictionary that associates string to OnlineCategory enum value
            var onlineCategoriesDict = GetDomainOnlineCategoryDictionary();

            if (onlineCategories == null)
            {
                // If there are no online categories specified in the criteria, return all sections in the collection
                return sectionCollection;
            }
            else
            {
                // Otherwise, select only the sections that have one of the specified online categories
                foreach (var category in onlineCategories)
                {
                    try
                    {
                        // Get the OnlineCategory enum value for this string
                        var onlineCategory = onlineCategoriesDict[category];
                        // Select only the sections that match the specified online category
                        filteredSections.AddRange(sectionCollection.Where(sec => sec.OnlineCategory == onlineCategory));
                    }
                    catch
                    {
                        logger.Info("Online category value of " + category + " is not found in the domain, ignored by FilterSectionsByOnlineCategory");
                    }
                }
            }
            return filteredSections.Distinct();
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByTopicCode(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, IEnumerable<string> topicCodes)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Section> filteredSections;
            if (topicCodes != null && topicCodes.Any())
            {
                filteredSections = (from topicCode in topicCodes
                                    join sec in sectionCollection
                                    on topicCode equals sec.TopicCode into joinSectionTopic
                                    from filteredSection in joinSectionTopic
                                    select filteredSection).ToList();
            }
            else
            {
                filteredSections = sectionCollection.ToList();
            }
            return filteredSections;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> FilterSectionsByAcademicLevel(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sectionCollection, IEnumerable<string> academicLevels)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Section> filteredSections;
            if (academicLevels != null && academicLevels.Any())
            {
                filteredSections = (from academicLevel in academicLevels
                                    join sec in sectionCollection
                                    on academicLevel equals sec.AcademicLevelCode into joinSectionAcademicLevel
                                    from filteredSection in joinSectionAcademicLevel
                                    select filteredSection).ToList();
            }
            else
            {
                filteredSections = sectionCollection.ToList();
            }
            return filteredSections;
        }

        private Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.OnlineCategory> GetDomainOnlineCategoryDictionary()
        {
            // Convert an online category string value to a domain OnlineCategory value
            var onlineCategoriesDict = new Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.OnlineCategory>();
            var onlineCategories = Enum.GetValues(typeof(Domain.Student.Entities.OnlineCategory));
            foreach (var category in onlineCategories)
            {
                switch (category.ToString())
                {
                    case "Online":
                        onlineCategoriesDict.Add(category.ToString(), Domain.Student.Entities.OnlineCategory.Online);
                        break;
                    case "NotOnline":
                        onlineCategoriesDict.Add(category.ToString(), Domain.Student.Entities.OnlineCategory.NotOnline);
                        break;
                    case "Hybrid":
                        onlineCategoriesDict.Add(category.ToString(), Domain.Student.Entities.OnlineCategory.Hybrid);
                        break;
                    default:
                        logger.Info("Online category " + category.ToString() + " is valid in domain but being ignored in GetDomainOnlineCategoryDictionary");
                        break;
                }
            }
            return onlineCategoriesDict;
        }

        private async Task FilterOpenSectionsOnlyAsync(Dictionary<string, Ellucian.Colleague.Domain.Student.Entities.Section> sectionDict)
        {
            List<Ellucian.Colleague.Domain.Student.Entities.Section> openSections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            if (sectionDict != null)
            {
                List<string> sectionIds = sectionDict.Keys.ToList();
                if (sectionIds != null && sectionIds.Count > 0)
                {
                    // Open sections are those with no waitlist and with available seats greater than 0.
                    var sectionSeats = (await _sectionRepository.GetSectionsSeatsAsync(sectionIds)).Where(s => (s.Value.Waitlisted == 0) && (s.Value.Available == null || s.Value.Available > 0)).ToList();

                    //find the sections that were not open to remove from incoming/original list
                    IEnumerable<string> missingSectionIds = sectionIds.Except(sectionSeats.Select(s => s.Key));
                    if (missingSectionIds != null)
                    {
                        foreach (string sec in missingSectionIds.ToList())
                        {
                            sectionDict.Remove(sec);
                        }
                    }
                }
            }
            else
                return;
        }


        #endregion

        #region Build Outbound Search Filters

        private IEnumerable<Ellucian.Colleague.Dtos.Base.Filter> BuildSubjectFilter(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseCollection, CourseSearchCriteria criteria)
        {
            // Set of filters to return
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();
            // Get the unique list from the course collection
            var subjectCodes = courseCollection.Select(x => x.SubjectCode).Distinct();
            // For each item in the list, count the number of items and build a filter detail
            foreach (var subjectCode in subjectCodes)
            {
                var filterValueCount = courseCollection.Where(x => x.SubjectCode == subjectCode).Count();
                var filterDetail = new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Count = filterValueCount,
                    Value = subjectCode,
                    Selected = criteria.Subjects != null ? criteria.Subjects.Contains(subjectCode) : false
                };
                filter.Add(filterDetail);
            }
            return filter;
        }

        private IEnumerable<Ellucian.Colleague.Dtos.Base.Filter> BuildAcademicLevelFilter(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseCollection, CourseSearchCriteria criteria)
        {
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();
            // Get the unique list from the course collection
            var academicLevelCodes = courseCollection.Select(x => x.AcademicLevelCode).Distinct();
            // For each item in the list, count the number of items and build a filter detail
            foreach (var academicLevelCode in academicLevelCodes)
            {
                var filterValueCount = courseCollection.Where(x => x.AcademicLevelCode == academicLevelCode).Count();
                var filterDetail = new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Count = filterValueCount,
                    Value = academicLevelCode,
                    Selected = criteria.AcademicLevels != null ? criteria.AcademicLevels.Contains(academicLevelCode) : false
                };
                filter.Add(filterDetail);
            }
            return filter;
        }

        private IEnumerable<Ellucian.Colleague.Dtos.Base.Filter> BuildCourseLevelFilter(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courseCollection, CourseSearchCriteria criteria)
        {
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();
            // Get the unique list from the course collection
            List<string> courseLevelCodes = courseCollection.SelectMany(crs => crs.CourseLevelCodes).Distinct().ToList();
            // For each item in the list, count the number of items and build a filter detail
            foreach (string courseLevelCode in courseLevelCodes)
            {
                var filterValueCount = courseCollection.SelectMany(x => x.CourseLevelCodes).Where(clc => clc == courseLevelCode).Count();
                var filterDetail = new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Count = filterValueCount,
                    Value = courseLevelCode,
                    Selected = criteria.CourseLevels != null ? criteria.CourseLevels.Contains(courseLevelCode) : false
                };
                filter.Add(filterDetail);
            }
            return filter;
        }

        private async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Filter>> BuildLocationFilterAsync(CourseSectionResult result, CourseSearchCriteria criteria)
        {
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();
            // Get the unique list of locations from all courses and sections
            var locationCodes = new List<string>();
            locationCodes.AddRange(result.Courses.SelectMany(crs => crs.LocationCodes).Distinct().ToList());
            locationCodes.AddRange(result.Sections.Select(s => s.Location).Distinct().ToList());
            locationCodes = locationCodes.Distinct().ToList();

            // Get location entities that are filterable - only these should end up as filters returned.
            var allLocations = (await _referenceDataRepository.GetLocationsAsync(false)).ToList();
            var searchableLocationCodes = allLocations.Where(loc => !loc.HideInSelfServiceCourseSearch).Select(loc => loc.Code).ToList();
            // Reduce the locations list to only those that should be shown in course search
            locationCodes = locationCodes.Intersect(searchableLocationCodes).ToList();

            // For each location, count the courses with this location, or that have sections with this location
            foreach (string locationCode in locationCodes)
            {
                var filterCourseIds = new List<string>();
                // Get Ids of all courses with this location
                if ((result.Courses != null) && (result.Courses.Any()))
                {
                    filterCourseIds.AddRange(result.Courses.Where(c => c.LocationCodes.Contains(locationCode)).Select(c => c.Id).ToList());
                }
                // Get course Ids for all sections with this location
                if ((result.Sections != null) && (result.Sections.Any()))
                {
                    filterCourseIds.AddRange(result.Sections.Where(s => s.Location == locationCode).Select(s => s.CourseId).Distinct().ToList());
                }
                // Count distinct courses
                var filterValueCount = filterCourseIds.Distinct().Count();
                // Add filter detail line for this location
                var filterDetail = new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Count = filterValueCount,
                    Value = locationCode,
                    Selected = criteria.Locations != null ? criteria.Locations.Contains(locationCode) : false
                };
                filter.Add(filterDetail);
            }
            return filter;
        }


        private IEnumerable<Ellucian.Colleague.Dtos.Base.Filter> BuildFacultyFilter(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections, CourseSearchCriteria criteria)
        {
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();

            // Get the unique list of faculty from all courses and sections
            var facultyIds = sections.SelectMany(s => s.FacultyIds).Distinct().ToList();
            // Count the courses with this faculty
            foreach (string facId in facultyIds)
            {
                var filterSectionIds = new List<string>();
                // Get course Ids for all sections with this faculty
                if ((sections != null) && (sections.Any()))
                {
                    filterSectionIds.AddRange(sections.Where(s => s.FacultyIds.Contains(facId)).Select(s => s.CourseId).ToList());
                }
                // Count distinct courses
                var filterValueCount = filterSectionIds.Distinct().Count();
                // Add filter detail line for this faculty
                var filterDetail = new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Count = filterValueCount,
                    Value = facId,
                    Selected = criteria.Faculty != null ? criteria.Faculty.Contains(facId) : false
                };
                filter.Add(filterDetail);
            }
            return filter;
        }

        private IEnumerable<Ellucian.Colleague.Dtos.Base.Filter> BuildDayOfWeekFilter(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections, CourseSearchCriteria criteria)
        {
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();

            // Get the unique list of faculty from all courses and sections
            var daysOfWeek = sections.SelectMany(s => s.Meetings).SelectMany(m => m.Days).Distinct().ToList();
            // Build collection of section/day
            var dayCollection = from sec in sections
                                from mtg in sec.Meetings
                                from dayOfWk in mtg.Days
                                select new { dayOfWk, sec.CourseId };
            // Count the courses with this faculty
            foreach (var day in daysOfWeek)
            {
                int filterValueCount = 0;
                // Count course Ids for all sections found with this dayOfWeek
                if ((dayCollection != null) && (dayCollection.Any()))
                {
                    filterValueCount = dayCollection.Where(d => d.dayOfWk == day).Select(d => d.CourseId).Distinct().Count();
                }
                // Add filter detail line for this faculty
                var filterDetail = new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Count = filterValueCount,
                    Value = ((int)day).ToString(),
                    Selected = criteria.DaysOfWeek != null ? criteria.DaysOfWeek.Contains(((int)day).ToString()) : false
                };
                filter.Add(filterDetail);
            }
            return filter;
        }

        private async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Filter>> BuildCourseTypeFilterAsync(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections, CourseSearchCriteria criteria)
        {
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();

            // Get the unique list of course type codes currently in used for the sections. A sections can have multiple course types.
            var courseTypeCodes = sections.SelectMany(s => s.CourseTypeCodes).Distinct().ToList();
            // Get course type entities that are filterable - only these should end up as filters returned.
            var searchableCourseTypes = (await _studentReferenceDataRepository.GetCourseTypesAsync()).Where(c => c.ShowInCourseSearch).Select(ct => ct.Code);
            // Reduce the course types list to only those that should be shown in course search
            courseTypeCodes = courseTypeCodes.Intersect(searchableCourseTypes).ToList();

            // Count the courses with each course type
            foreach (string courseTypeCode in courseTypeCodes)
            {
                var filterSectionIds = new List<string>();
                // Get course Ids for all sections with this course type code
                if ((sections != null) && (sections.Any()))
                {
                    filterSectionIds.AddRange(sections.Where(s => s.CourseTypeCodes.Contains(courseTypeCode)).Select(s => s.CourseId).ToList());
                }
                // Count distinct courses
                var filterValueCount = filterSectionIds.Distinct().Count();
                // Add filter detail line for this course type
                var filterDetail = new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Count = filterValueCount,
                    Value = courseTypeCode,
                    Selected = criteria.CourseTypes != null ? criteria.CourseTypes.Contains(courseTypeCode) : false
                };
                filter.Add(filterDetail);
            }
            return filter;
        }

        private IEnumerable<Ellucian.Colleague.Dtos.Base.Filter> BuildTopicCodeFilter(CourseSectionResult result, CourseSearchCriteria criteria)
        {
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();

            // Get the unique list of topic codes from all sections and the courses so all possible are in the list of filterable topics.
            var topicCodes = new List<string>();
            topicCodes.AddRange(result.Courses.Select(crs => crs.TopicCode).Distinct().ToList());
            topicCodes.AddRange(result.Sections.Select(s => s.TopicCode).Distinct().ToList());
            topicCodes = topicCodes.Distinct().ToList();

            foreach (string topicCode in topicCodes)
            {
                // Note: Courses and their sections can have different topic codes and lists don't have to overlap.  
                // Need to combine the totals.

                var filterCourseIds = new List<string>();
                // Get Ids of all courses with this topic code
                if ((result.Courses != null) && (result.Courses.Any()))
                {
                    filterCourseIds.AddRange(result.Courses.Where(c => c.TopicCode == topicCode).Select(c => c.Id).ToList());
                }
                // Get course Ids for all sections with this location
                if ((result.Sections != null) && (result.Sections.Any()))
                {
                    filterCourseIds.AddRange(result.Sections.Where(s => s.TopicCode == topicCode).Select(s => s.CourseId).Distinct().ToList());
                }
                // Count distinct courses
                var filterValueCount = filterCourseIds.Distinct().Count();
                // Add filter detail line for this location
                var filterDetail = new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Count = filterValueCount,
                    Value = topicCode,
                    Selected = criteria.TopicCodes != null ? criteria.TopicCodes.Contains(topicCode) : false
                };
                filter.Add(filterDetail);
            }
            return filter;
        }

        private IEnumerable<Ellucian.Colleague.Dtos.Base.Filter> BuildTermFilter(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections, CourseSearchCriteria criteria)
        {
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();

            // Get the unique list of courseTypes from all sections
            var termCodes = sections.Select(s => s.TermId).Distinct().ToList();
            // Count the courses with each course type
            foreach (string termCode in termCodes)
            {
                var filterSectionIds = new List<string>();
                // Get course Ids for all sections with this course type code
                if ((sections != null) && (sections.Any()))
                {
                    filterSectionIds.AddRange(sections.Where(sec => sec.TermId == termCode).Select(sec => sec.CourseId).ToList());
                }
                // Count distinct courses
                var filterValueCount = filterSectionIds.Distinct().Count();
                // Add filter detail line for this course type
                var filterDetail = new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Count = filterValueCount,
                    Value = termCode,
                    Selected = criteria.Terms != null ? criteria.Terms.Contains(termCode) : false
                };
                filter.Add(filterDetail);
            }
            return filter;
        }

        private IEnumerable<Ellucian.Colleague.Dtos.Base.Filter> BuildOnlineCategoryFilter(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections, CourseSearchCriteria criteria)
        {
            var filter = new List<Ellucian.Colleague.Dtos.Base.Filter>();

            // Get a dictionary that associates string value to each OnlineCategory enum value
            var onlineCategoryDict = GetDomainOnlineCategoryDictionary();
            // Create a list of OnlineCategory items from the dto OnlineCategory values.
            var criteriaOnlineCategories = new List<Ellucian.Colleague.Domain.Student.Entities.OnlineCategory>();
            if (criteria.OnlineCategories != null)
            {
                foreach (var category in criteria.OnlineCategories)
                {
                    try
                    {
                        criteriaOnlineCategories.Add(onlineCategoryDict[category]);
                    }
                    catch
                    {
                        logger.Info("Online category " + category + " does not exist in the domain, ignored by BuildOnlineCategoryFilter");
                    }
                }
            }

            // Get the list of online categories from all sections
            var sectionOnlineCategories = sections.Select(sec => sec.OnlineCategory).Distinct();
            foreach (var onlineCategory in sectionOnlineCategories)
            {
                // count the sections with this online category
                var sectionCount = sections.Where(sec => sec.OnlineCategory == onlineCategory).Select(s => s.CourseId).Distinct().Count();
                // Add filter detail. Set as selected if this filter is in the criteria.
                filter.Add(new Ellucian.Colleague.Dtos.Base.Filter()
                {
                    Value = onlineCategory.ToString(),
                    Count = sectionCount,
                    Selected = criteriaOnlineCategories.Contains(onlineCategory)
                });
            }
            return filter;
        }
        private Ellucian.Colleague.Dtos.Base.Filter BuildOpenSectionsFilter(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections, CourseSearchCriteria criteria)
        {
            // count the sections 
            var sectionCount = sections == null ? 0 : sections.Count();
            // Add filter detail. Set as selected if this filter is in the criteria.
            var filter = new Ellucian.Colleague.Dtos.Base.Filter()
            {
                Value = null,
                Count = sectionCount,
                Selected = criteria.OpenSections
            };

            return filter;
        }
        #endregion

        #region SearchByCourseId
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>> SearchByIdAsync(List<string> courseids)
        {
            var courses = (await GetCatalogAsync()).ToList();
            var filtered = courses.Where(c => courseids.Contains(c.Id) || c.EquatedCourseIds.Intersect(courseids).Any());
            return filtered;
        }
        #endregion

        #region SearchByRequirement

        private async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>> SearchByRequirementGroupAsync(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> courses, Domain.Student.Entities.Requirements.Group group)
        {
            var final = new List<Ellucian.Colleague.Domain.Student.Entities.Course>();

            var filtered = new List<Ellucian.Colleague.Domain.Student.Entities.Course>(courses);

            // Remove courses that don't meet explicit take statements
            if (group.Courses.Count > 0)
            {
                filtered.RemoveAll(c => !group.Courses.Contains(c.Id));
            }
            if (group.FromCourses.Count > 0)
            {
                filtered.RemoveAll(c => !group.FromCourses.Contains(c.Id));
            }
            if (group.FromSubjects.Count > 0)
            {
                filtered.RemoveAll(c => !group.FromSubjects.Contains(c.SubjectCode));
            }
            if (group.FromDepartments.Count > 0)
            {
                filtered.RemoveAll(c => c.DepartmentCodes.All(d => !group.FromDepartments.Contains(d)));
            }
            if (group.FromLevels.Count > 0)
            {
                filtered.RemoveAll(c => c.CourseLevelCodes.All(l => !group.FromLevels.Contains(l)));
            }

            // Remove courses that are explicitly excluded
            if (group.ButNotCourses.Count > 0)
            {
                filtered.RemoveAll(c => group.ButNotCourses.Contains(c.Id));
            }
            if (group.ButNotSubjects.Count > 0)
            {
                filtered.RemoveAll(c => group.ButNotSubjects.Contains(c.SubjectCode));
            }
            if (group.ButNotDepartments.Count > 0)
            {
                filtered.RemoveAll(c => c.DepartmentCodes.All(d => group.ButNotDepartments.Contains(d)));
            }
            if (group.ButNotCourseLevels.Count > 0)
            {
                filtered.RemoveAll(c => c.CourseLevelCodes.All(l => group.ButNotCourseLevels.Contains(l)));
            }
            // Get all the course-based rules starting with this group and moving all the way up the program requirements chain
            var rules = group.CourseBasedRules;
            if (group.SubRequirement != null)
            {
                rules.AddRange(group.SubRequirement.CourseBasedRules);
                if (group.SubRequirement.Requirement != null)
                {
                    rules.AddRange(group.SubRequirement.Requirement.CourseBasedRules);
                    if (group.SubRequirement.Requirement.ProgramRequirements != null)
                    {
                        rules.AddRange(group.SubRequirement.Requirement.ProgramRequirements.CourseBasedRules);
                    }
                }
            }
            // Remove any duplicates
            rules = rules.Distinct().ToList();
            // If there are any course-based rules, Filter courses against them
            if (rules.Any())
            {
                filtered = (await FilterCoursesAgainstRulesAsync(filtered, rules)).ToList();
                final = final.Union(filtered).ToList();
            }
            else
            {
                final = filtered;
            }

            return final.OrderBy(c => c.SubjectCode).OrderBy(c => c.Number).AsEnumerable();
        }

        #endregion

        #region SearchByKeyword

        // Struct used for sorting items returned by search
        struct SearchResultItem
        {
            public string CourseId;
            public string SectionId;
            public string SortName;
            public float Score;

            public SearchResultItem(string courseId, string sectionId, string sortName, float score)
            {
                CourseId = courseId;
                SectionId = sectionId;
                SortName = sortName;
                Score = score;
            }
        }

        /// <summary>
        /// Given a keyword, or string of keywords, return a set of matching courses
        /// </summary>
        /// <param name="keyword">A keyword or query to search against the couse catalog.</param>
        /// <returns></returns>
        private async Task<CourseSectionResult> SearchByKeywordAsync(string keyword)
        {
            CourseSectionResult indexItems = new CourseSectionResult();
            // Get all the current courses and get active sections
            var sw = new Stopwatch();
            sw.Start();
            logger.Debug("CourseSearch: call GetCatalog");
            indexItems.Courses = (await GetCatalogAsync()).ToList();
            sw.Stop();
            logger.Debug("CourseSearch: GetCatalog: " + sw.ElapsedMilliseconds + "ms");

            sw.Reset();
            sw.Start();
            logger.Debug("CourseSearch: call GetSectionsForCourses");
            indexItems.Sections = (await GetSectionsForCoursesAsync(indexItems.Courses)).ToList();
            sw.Stop();
            logger.Debug("CourseSearch: GetSectionsForCourses: " + sw.ElapsedMilliseconds + "ms");

            // Build the index to be searched.
            logger.Debug("retrieval of subjects, departments, locations");
            var subjects = await _studentReferenceDataRepository.GetSubjectsAsync();
            var departments = await _referenceDataRepository.DepartmentsAsync();
            var locations = _referenceDataRepository.Locations;

            logger.Debug("CourseSearch: Get the lock on the index");
            lock (IndexLock)
            {
                // Get the timestamp from the last time the updated section cache was built
                var cacheTimestamp = _sectionRepository.GetChangedRegistrationSectionsCacheBuildTime();
                BuildIndex(indexItems, subjects, departments, locations, cacheTimestamp, logger);
            }
            logger.Debug("CourseSearch: Index build complete and unlocked");

            // Dictionary that contains ALL fields to be searched, with their boost values
            // Lucene allows the assigning of weights (boost values) to each field that is being searched.
            // In the course name has highest weight, then title, then subject, then everything else.
            var FieldBoosts = new Dictionary<string, float>();
            FieldBoosts.Add("name", 20f);
            FieldBoosts.Add("subject", 15f);
            FieldBoosts.Add("title", 5f);
            FieldBoosts.Add("course", 1f);
            FieldBoosts.Add("section", 1f);

            // The BooleanQuery query is the final, concatenated query that will be run against the index.  
            // It will contain subqueries against individual fields within the indexed documents.
            BooleanQuery query = new BooleanQuery(true);

            string queryString = "";
            // Simply use query parser to build a query item for each field given the user's query
            foreach (var fieldBoost in FieldBoosts)
            {
                // Create parser for this field
                var queryParser = new QueryParser(LuceneVersion, fieldBoost.Key, Analyzer);
                // Allow leading wildcard (if entered)
                queryParser.SetAllowLeadingWildcard(true);
                // Create query for this keyword against this field
                Query qu = queryParser.Parse(keyword);
                // Boost the results for this field
                qu.SetBoost(fieldBoost.Value);
                // Add to the overall query as an OR
                query.Add(qu, BooleanClause.Occur.SHOULD);
                queryString = query.ToString();
            }

            // Build the search engine, using the index.
            var searcher = new IndexSearcher(CourseIndex, true);

            // Get all matches
            sw.Reset();
            sw.Start();
            var topDocs = searcher.Search(query, Int16.MaxValue);
            sw.Stop();
            logger.Debug("CourseSearch: Search: " + sw.ElapsedMilliseconds + "ms");

            // Establish items that will be used to build the returned results
            CourseSectionResult searchResult = new CourseSectionResult();
            var courseIdsList = new List<string>();

            // For each document returned by the search, get the course or section, and add it to the result set.
            // Docs returned in descending score sequence, simply append to course/section list in sequence.
            var docs = topDocs.ScoreDocs;

            var sortedByScoreList = from doc in docs
                                    select new
                                    {
                                        CourseId = searcher.Doc(doc.doc).GetField("courseId").StringValue(),
                                        SectionId = searcher.Doc(doc.doc).GetField("sectionId").StringValue(),
                                        SortName = searcher.Doc(doc.doc).GetField("sortName").StringValue(),
                                        Name = searcher.Doc(doc.doc).GetField("name").StringValue(),
                                        Score = doc.score
                                    };

            // Get the courses with the matching name first, then append all the others in name sequence.
            // Correction made to put the results in descending score order so those with the highest score are at the top of the list.
            var sortedList = sortedByScoreList.Where(s => s.Name.IndexOf(keyword.ToUpper()) >= 0).OrderByDescending(s => s.Score).ThenBy(s => s.SortName).ToList();
            sortedList.AddRange(sortedByScoreList.Where(s => s.Name.IndexOf(keyword.ToUpper()) < 0).OrderByDescending(s => s.Score).ThenBy(s => s.SortName).ToList());

            //var sortedList = from doc in sortedByScoreList
            //                 orderby doc.Score descending, doc.SortName
            //                 select doc;

            foreach (var item in sortedList)
            {
                // Add the course to the result list of courses if not already there
                if (!(courseIdsList.Contains(item.CourseId)))
                {
                    searchResult.Courses.Add(indexItems.Courses.Where(c => c.Id == item.CourseId).First());
                    courseIdsList.Add(item.CourseId);
                }
                // Add the section to the section result list if this is a section document
                if (!(string.IsNullOrEmpty(item.SectionId)))
                {
                    var section = indexItems.Sections.Where(s => s.Id == item.SectionId).FirstOrDefault();
                    if (section != null)
                    {
                        searchResult.Sections.Add(section);
                    }
                }
            }

            return searchResult;
        }

        /// <summary>
        /// This method builds the Lucene index if it does not already exist. This method must be called
        /// in a thread safe way to ensure multiple threads are not creating the index simultaneously.
        /// </summary>
        /// <param name="indexItems"></param>
        /// <param name="subjects"></param>
        /// <param name="departments"></param>
        /// <param name="locations"></param>
        private static void BuildIndex(CourseSectionResult indexItems,
            IEnumerable<Domain.Student.Entities.Subject> subjects,
            IEnumerable<Domain.Base.Entities.Department> departments,
            IEnumerable<Domain.Base.Entities.Location> locations,
            DateTime cacheTimestamp,
            ILogger logger)
        {
            logger.Debug("CourseSearch: Start BuildIndex");

            // If the updated section cache is newer than the index, clear the index and rebuild it
            if (cacheTimestamp > indexBuildTime && CourseIndex != null)
            {
                logger.Debug("CourseSearch: BuildIndex - Cache is newer than index, close index and rebuild");

                CourseIndex.Close();
                CourseIndex = null;
            }

            if (CourseIndex == null)
            {
                // The index has not been created yet.
                logger.Debug("CourseSearch: BuildIndex - Create RAMDirectory for index");
                CourseIndex = new RAMDirectory();
            }
            else
            {
                // The index has been created, and it's static, so we're done.
                return;
            }

            // IndexWriter is a utility object that creates and maintains an index.
            IndexWriter indexWriter = new IndexWriter(CourseIndex, Analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            foreach (var course in indexItems.Courses)
            {
                // Create a new document that corresponds to the current course
                var doc = new Document();

                // build a whole-course index string as we go along - for AND queries
                string fieldString = "";
                var courseString = new StringBuilder();

                // Add the course/section ID to the document.  It is stored, but is not indexed.
                // (student's don't search by ID). Also add blank placeholder for section Id.
                doc.Add(new Field("courseId", course.Id, Field.Store.YES, Field.Index.NO));
                doc.Add(new Field("sectionId", "", Field.Store.YES, Field.Index.NO));

                // Course name (subject and course number) with no space or delimiter
                fieldString = " " + course.SubjectCode + course.Number;
                // Add this store-only value for sorting later
                doc.Add(new Field("sortName", fieldString, Field.Store.YES, Field.Index.NO));

                // Course name with a space between
                fieldString += " " + course.SubjectCode + " " + course.Number;
                // Course name with delimiter
                fieldString += " " + course.SubjectCode + CourseDelimiter + course.Number;
                // Add course names to entire course string 
                courseString.Append(fieldString);
                // Add course names to indexed document
                var f = new Field("name", fieldString, Field.Store.YES, Field.Index.ANALYZED);
                doc.Add(f);

                // Title
                fieldString = " " + course.Title;
                courseString.Append(fieldString);
                f = new Field("title", fieldString, Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(f);

                // Description (append to whole Course string)
                fieldString = " " + course.Description;
                courseString.Append(fieldString);

                // Subject code and description (append to whole Course string)
                fieldString = " " + course.SubjectCode;
                courseString.Append(fieldString);
                var subjString = fieldString;
                // Add subject description to the subject field
                var s = subjects.Where(sub => sub.Code == course.SubjectCode).FirstOrDefault();
                if (s != null)
                {
                    fieldString = " " + s.Description;
                    courseString.Append(fieldString);
                    subjString += " " + s.Description;
                }
                // Add field to score matches on subject code
                f = new Field("subject", subjString, Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(f);

                // Department code and description (append to whole Course string)
                // A course can have multiple departments, so add each of them as a separate field
                // Lucene allows field names to be entered multiple times.
                foreach (var department in course.DepartmentCodes)
                {
                    fieldString = " " + department;
                    courseString.Append(fieldString);
                    var dept = departments.Where(d => d.Code == department).FirstOrDefault();
                    if (dept != null)
                    {
                        fieldString = " " + dept.Description;
                        courseString.Append(fieldString);
                    }
                }

                // Location code and description (append to whole Course string)
                // A course can have multiple locations, so add each of them as a separate field
                foreach (var location in course.LocationCodes)
                {
                    fieldString = " " + location;
                    courseString.Append(fieldString);
                    var loc = locations.Where(l => l.Code == location).FirstOrDefault();
                    if (loc != null)
                    {
                        fieldString = " " + loc.Description;
                        courseString.Append(fieldString);
                    }
                }

                // Add the whole course string to the document
                f = new Field("course", courseString.ToString(), Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(f);

                // Add course document to the index
                indexWriter.AddDocument(doc);
            }

            foreach (var section in indexItems.Sections)
            {
                // First get the associated course for this section. Build section index
                // info ONLY if course exists.
                var course = indexItems.Courses.Where(c => c.Id == section.CourseId).FirstOrDefault();
                if (course != null)
                {
                    // Create a new document for this section
                    var doc = new Document();

                    // build a whole-section index string as we go along - for AND queries
                    string fieldString = "";
                    var sectionString = new StringBuilder();
                    Field f;

                    // Add the section ID to the document.  It is stored, but is not indexed
                    // Store in combination with course Id for uniqueness
                    doc.Add(new Field("courseId", section.CourseId, Field.Store.YES, Field.Index.NO));
                    doc.Add(new Field("sectionId", section.Id, Field.Store.YES, Field.Index.NO));

                    // Section Name (with all permutations of null, space and delimiter)
                    // null, null
                    fieldString = " " + course.SubjectCode + course.Number + section.Number;
                    // Add this store-only value for sorting later
                    doc.Add(new Field("sortName", fieldString, Field.Store.YES, Field.Index.NO));

                    if (!string.IsNullOrEmpty(section.CourseName))
                    {
                        //This was added because we wanted search could happen on legacy section data too
                        //For example- if a course number changed on existing course and there were sections 
                        //from old course then those sections should be searchable too.
                        // null, space
                        fieldString += " " + section.CourseName + " " + section.Number;
                        // null, delim
                        fieldString += " " + section.CourseName + CourseDelimiter + section.Number;
                        // space, null
                        fieldString += " " + section.CourseName + section.Number;
                    }
                    // null, space
                    fieldString += " " + course.SubjectCode + course.Number + " " + section.Number;
                    // null, delim
                    fieldString += " " + course.SubjectCode + course.Number + CourseDelimiter + section.Number;
                    // space, null
                    fieldString += " " + course.SubjectCode + " " + course.Number + section.Number; //remove this??
                    // space, space
                    fieldString += " " + course.SubjectCode + " " + course.Number + " " + section.Number;
                    // space, delim
                    fieldString += " " + course.SubjectCode + " " + course.Number + CourseDelimiter + section.Number;
                    // delim, null
                    fieldString += " " + course.SubjectCode + CourseDelimiter + course.Number + section.Number; //remove this??
                    // delim, space
                    fieldString += " " + course.SubjectCode + CourseDelimiter + course.Number + " " + section.Number;
                    // delim, delim
                    fieldString += " " + course.SubjectCode + CourseDelimiter + course.Number + CourseDelimiter + section.Number;
                    // Append to the whole section string
                    sectionString.Append(fieldString);
                    // Add Name field to the document
                    f = new Field("name", fieldString, Field.Store.YES, Field.Index.ANALYZED);
                    doc.Add(f);

                    // Section Title (often different from course title) (Append to whole Section string)
                    fieldString = " " + section.Title;
                    sectionString.Append(fieldString);
                    f = new Field("title", fieldString, Field.Store.NO, Field.Index.ANALYZED);
                    doc.Add(f);

                    // Department code and description. (Append to whole Section string)
                    // A section can have multiple departments, so add each of them as a separate field
                    // Lucene allows field names to be entered multiple times.
                    foreach (var department in section.Departments)
                    {
                        fieldString = " " + department.AcademicDepartmentCode;
                        sectionString.Append(fieldString);
                        var dept = departments.Where(d => d.Code == department.AcademicDepartmentCode).FirstOrDefault();
                        if (dept != null)
                        {
                            fieldString = " " + dept.Description;
                            sectionString.Append(fieldString);
                        }
                    }

                    // Section Location (Append to whole Section string)
                    fieldString = " " + section.Location;
                    sectionString.Append(fieldString);
                    var loc = locations.Where(l => l.Code == section.Location).FirstOrDefault();
                    if (loc != null)
                    {
                        fieldString = " " + loc.Description;
                        sectionString.Append(fieldString);
                    }

                    // Add the whole section string to the document
                    f = new Field("section", sectionString.ToString(), Field.Store.NO, Field.Index.ANALYZED);
                    doc.Add(f);

                    // Add the section document to the index
                    indexWriter.AddDocument(doc);
                }
            }

            indexWriter.Optimize();
            indexWriter.Close();

            // Update the index build time
            indexBuildTime = DateTime.Now;

            logger.Debug("CourseSearch: End BuildIndex");

            return;
        }

        #endregion

        /// <summary>
        /// OBSOLETE AS OF API 1.3 - Use GetCourseById2
        /// Gets a specific course by ID.
        /// </summary>
        /// <param name="id">The ID of the course</param>
        /// <returns>The associated course object</returns>
        [Obsolete("Obsolete as of API version 1.3. Use latest version of this method.")]
        public async Task<Ellucian.Colleague.Dtos.Student.Course> GetCourseByIdAsync(string id)
        {
            var course = await _courseRepository.GetAsync(id);
            return BuildCourseDto(course);
        }

        /// <summary>
        /// Gets a specific course by ID.
        /// </summary>
        /// <param name="id">The ID of the course</param>
        /// <returns>The associated course2 DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Student.Course2> GetCourseById2Async(string id)
        {
            var course = await _courseRepository.GetAsync(id);
            var courseDtoAdapter2 = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>();
            var course2Dto = courseDtoAdapter2.MapToType(course);
            return course2Dto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN Hedm</remarks>
        /// <summary>
        /// Gets a specific course by GUID.
        /// </summary>
        /// <param name="guid">The GUID of the course</param>
        /// <returns>The associated <see cref="Course2">Course</see> DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Course2> GetCourseByGuid2Async(string guid)
        {
            var course = await _courseRepository.GetCourseByGuidAsync(guid);

            var courseDto = await ConvertCourseEntityToDto2Async(course);
            return courseDto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Gets all courses
        /// </summary>
        /// <returns>Collection of <see cref="Course">Course</see> DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Course2>> GetCourses2Async(bool bypassCache)
        {
            var courseCollection = new List<Ellucian.Colleague.Dtos.Course2>();
            IEnumerable<Domain.Student.Entities.Course> courseEntities = null;

            if (bypassCache)
            {
                courseEntities = await _courseRepository.GetNonCacheAsync();
            }
            else
            {
                courseEntities = await _courseRepository.GetAsync();
            }

            if (courseEntities != null && courseEntities.Any())
            {
                foreach (var course in courseEntities)
                {
                    courseCollection.Add(await ConvertCourseEntityToDto2Async(course));
                }
            }
            return courseCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Creates a course
        /// </summary>
        /// <param name="course">A Course domain object</param>
        /// <returns>A Course DTO object for the created course</returns>
        public async Task<Ellucian.Colleague.Dtos.Course2> CreateCourse2Async(Ellucian.Colleague.Dtos.Course2 course)
        {
            // Confirm that user has permissions to create course
            CheckCoursePermission();

            //Convert the DTO to an entity, create the course, convert the resulting entity back to a DTO, and return it
            var courseEntity = await ConvertCourseDtoToEntity2Async(course);
            var source = course.MetadataObject != null ? course.MetadataObject.CreatedBy : null;
            var createdCourseEntity = await _courseRepository.CreateCourseAsync(courseEntity, source);
            return await ConvertCourseEntityToDto2Async(createdCourseEntity);
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Updates a course
        /// </summary>
        /// <param name="course">A Course domain object</param>
        /// <returns>A Course DTO object for the created course</returns>
        public async Task<Ellucian.Colleague.Dtos.Course2> UpdateCourse2Async(Ellucian.Colleague.Dtos.Course2 course)
        {
            if (course == null)
            {
                throw new ArgumentNullException("course payload is invalid", "Course DTO is required for PUT.");
            }
            // We must have a GUID so we can get the existing data
            if (string.IsNullOrEmpty(course.Id))
            {
                throw new KeyNotFoundException("Course must provide a GUID.");
            }

            // Confirm that user has permissions to update course
            CheckCoursePermission();

            //Convert the DTO to an entity, update the course, convert the resulting entity back to a DTO, and return it
            var courseEntity = await ConvertCourseDtoToEntity2Async(course);
            var source = course.MetadataObject != null ? course.MetadataObject.CreatedBy : null;
            var updatedCourseEntity = await _courseRepository.UpdateCourseAsync(courseEntity, source);
            return await ConvertCourseEntityToDto2Async(updatedCourseEntity);
        }


        /// <summary>
        /// Gets courses using the given list of course Ids
        /// </summary>
        /// <param name="courseIds">The list of ID of the courses to retrieve</param>
        /// <returns>A list of <see cref="Course2">Course2</see> dto objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.Course2>> GetCourses2Async(CourseQueryCriteria criteria)
        {
            if (criteria.CourseIds == null || criteria.CourseIds.Count() == 0)
            {
                string errorText = "At least one item must be provided in list of courseIds.";
                logger.Error(errorText);
                throw new ArgumentNullException("courseIds", errorText);
            }
            else
            {
                var coursesDto = new List<Ellucian.Colleague.Dtos.Student.Course2>();
                var courses = await _courseRepository.GetAsync(criteria.CourseIds.ToList());
                var courseDtoAdapter2 = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>();
                foreach (var crs in courses)
                {
                    try
                    {
                        coursesDto.Add(courseDtoAdapter2.MapToType(crs));
                    }
                    catch
                    {
                        logger.Error("Error adapting course " + crs.Id + " " + crs.Name);
                    }
                }
                return coursesDto;
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create and update courses.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCoursePermission()
        {
            bool hasCoursePermission = HasPermission(StudentPermissionCodes.CreateAndUpdateCourse);

            // User is not allowed to create or update courses without the appropriate permissions
            if (!hasCoursePermission)
            {
                var message = "User " + CurrentUser.UserId + " does not have permission to create or update courses.";
                logger.Info(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Build CourseSearch dto which extents Course dto by simply adding a list of associated section Ids to each course
        /// </summary>
        /// <param name="courses">List of courses</param>
        /// <param name="sections">List of sections</param>
        /// <returns>CourseSearch dto: Course dto + a list of sections for each course</returns>
        private IEnumerable<CourseSearch> BuildCourseSearchDto(CourseSectionResult searchResults)
        {
            var courseSearchList = new List<CourseSearch>();

            if (searchResults.Courses != null)
            {
                foreach (var course in searchResults.Courses)
                {
                    var courseSearchDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>();
                    var courseSearchDto = courseSearchDtoAdapter.MapToType(course);
                    courseSearchDto.Corequisites = new List<Ellucian.Colleague.Dtos.Student.Corequisite>();
                    // Now do the manual changes to convert the new style requisites into the old style prereq and coreqs.
                    if (course.Requisites.Any())
                    {
                        // Update the Prereq field for the course DTO with the first prereq
                        var prereq = course.Requisites.Where(r => r.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.Previous).FirstOrDefault();
                        if (prereq != null)
                        {
                            courseSearchDto.Prerequisites = prereq.RequirementCode;
                        }
                        // Update the Coreqs list for the course search DTO with the all of the "either" type of requisites
                        var coreqs = course.Requisites.Where(r => r.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.PreviousOrConcurrent).ToList();
                        if (coreqs != null && coreqs.Any())
                        {

                            List<Ellucian.Colleague.Dtos.Student.Corequisite> courseCorequistes = new List<Ellucian.Colleague.Dtos.Student.Corequisite>(); foreach (var coreq in coreqs)
                            {
                                if (!string.IsNullOrEmpty(coreq.CorequisiteCourseId))
                                {
                                    var courseCoreq = new Ellucian.Colleague.Dtos.Student.Corequisite() { Id = coreq.CorequisiteCourseId, Required = coreq.IsRequired };
                                    courseCorequistes.Add(courseCoreq);
                                }
                            }
                            courseSearchDto.Corequisites = courseCorequistes;
                        }

                    }
                    courseSearchDto.MatchingSectionIds = searchResults.Sections.Where(s => s.CourseId == course.Id).Select(s => s.Id).ToList();
                    courseSearchList.Add(courseSearchDto);
                }
            }

            return courseSearchList;
        }

        /// <summary>
        /// Build CourseSearch dto which extents Course dto by simply adding a list of associated section Ids to each course
        /// </summary>
        /// <param name="courses">List of courses</param>
        /// <param name="sections">List of sections</param>
        /// <returns>CourseSearch dto: Course dto + a list of sections for each course</returns>
        private IEnumerable<CourseSearch2> BuildCourseSearch2Dto(CourseSectionResult searchResults)
        {
            var courseSearchList = new List<CourseSearch2>();

            if (searchResults.Courses != null)
            {
                var courseSearchDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>();
                foreach (var course in searchResults.Courses)
                {
                    var courseSearchDto = courseSearchDtoAdapter.MapToType(course);

                    courseSearchDto.MatchingSectionIds = searchResults.Sections.Where(s => s.CourseId == course.Id).Select(s => s.Id).ToList();
                    courseSearchList.Add(courseSearchDto);
                }
            }

            return courseSearchList;
        }

        /// <summary>
        /// A helper method to transform a set of course domain objects into a set of course DTOs.
        /// </summary>
        /// <param name="courses">A set of course domain objects</param>
        /// <returns>A set of course DTOs</returns>
        private Ellucian.Colleague.Dtos.Student.Course BuildCourseDto(Ellucian.Colleague.Domain.Student.Entities.Course course)
        {
            var courseDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Student.Course>();
            var courseDto = courseDtoAdapter.MapToType(course);
            courseDto.Corequisites = new List<Ellucian.Colleague.Dtos.Student.Corequisite>();
            if (course.Requisites.Any())
            {
                // Update the prereq field with the first prereq
                var prereq = course.Requisites.Where(r => r.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.Previous).FirstOrDefault();
                if (prereq != null)
                {
                    courseDto.Prerequisites = prereq.RequirementCode;
                }
                // Update the Coreqs list for the course DTO with the all of the "either" type of requisites
                var coreqs = course.Requisites.Where(r => r.CompletionOrder == Domain.Student.Entities.RequisiteCompletionOrder.PreviousOrConcurrent).ToList();
                if (coreqs != null && coreqs.Any())
                {
                    List<Ellucian.Colleague.Dtos.Student.Corequisite> courseCorequistes = new List<Ellucian.Colleague.Dtos.Student.Corequisite>();
                    foreach (var coreq in coreqs)
                    {
                        if (!string.IsNullOrEmpty(coreq.CorequisiteCourseId))
                        {
                            var courseCoreq = new Ellucian.Colleague.Dtos.Student.Corequisite() { Id = coreq.CorequisiteCourseId, Required = coreq.IsRequired };
                            courseCorequistes.Add(courseCoreq);
                        }
                    }
                    courseDto.Corequisites = courseCorequistes;
                }
            }
            return courseDto;
        }

        /// <summary>
        /// OBSOLETE AS OF API VERSION 1.3. REPLACED BY GetSections2
        /// Gets all sections that are open for preregistration or registration for the course Ids specified.
        /// </summary>
        /// <param name="courseids">String of course Ids separated by commas</param>
        /// <returns>IEnumerable list of SectionDTOs</returns>
        [Obsolete("Obsolete as of API version 1.3. Use the latest version of this method.")]
        public async Task<PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section>>> GetSectionsAsync(IEnumerable<string> courseIds, bool useCache = true)
        {
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            if (useCache)
            {
                sections = (await _sectionRepository.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.IsActive == true);
            }
            else
            {
                sections = (await _sectionRepository.GetCourseSectionsNonCachedAsync(courseIds, registrationTerms)).Where(s => s.IsActive == true);
            }

            // Currently not ordering the results.
            return BuildPrivacyWrappedSectionDto(sections);
        }

        /// <summary>
        /// Gets all sections that are open for preregistration or registration for the course Ids specified.
        /// </summary>
        /// <param name="courseids">String of course Ids separated by commas</param>
        /// <returns>IEnumerable list of SectionDTOs</returns>
        [Obsolete("Obsolete as of API 1.5. Use the latest version of this method.")]
        public async Task<PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section2>>> GetSections2Async(IEnumerable<string> courseIds, bool useCache = true)
        {
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            // var sections = _sectionRepository.GetCourseSections(courseIds, registrationTerms).Where(s => s.IsActive == true);
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            if (useCache)
            {
                sections = (await _sectionRepository.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.IsActive == true);
            }
            else
            {
                sections = (await _sectionRepository.GetCourseSectionsNonCachedAsync(courseIds, registrationTerms)).Where(s => s.IsActive == true);
            }

            // Currently not ordering the results.
            return BuildPrivacyWrappedSectionDto2(sections);
        }

        /// <summary>
        /// Gets all sections that are open for preregistration or registration for the course Ids specified.
        /// </summary>
        /// <param name="courseids">String of course Ids separated by commas</param>
        /// <returns>IEnumerable list of SectionDTOs</returns>
        public async Task<PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section3>>> GetSections3Async(IEnumerable<string> courseIds, bool useCache = true)
        {
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> registrationTerms = await _termRepository.GetRegistrationTermsAsync();
            // var sections = _sectionRepository.GetCourseSections(courseIds, registrationTerms).Where(s => s.IsActive == true);
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            if (useCache)
            {
                sections = (await _sectionRepository.GetCourseSectionsCachedAsync(courseIds, registrationTerms)).Where(s => s.IsActive == true);
            }
            else
            {
                sections = (await _sectionRepository.GetCourseSectionsNonCachedAsync(courseIds, registrationTerms)).Where(s => s.IsActive == true);
            }

            // Currently not ordering the results.
            return BuildPrivacyWrappedSection3Dtos(sections);
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a Course domain entity to its corresponding Course DTO
        /// </summary>
        /// <param name="source">A <see cref="Course">Course</see> domain entity</param>
        /// <returns>A <see cref="Course">Course</see> DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.Course2> ConvertCourseEntityToDto2Async(Ellucian.Colleague.Domain.Student.Entities.Course source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A course must be supplied.");
            }

            var course = new Ellucian.Colleague.Dtos.Course2();

            course.Id = source.Guid;
            course.Subject = new Dtos.GuidObject2((await _studentReferenceDataRepository.GetSubjectsAsync()).Where(s => s.Code == source.SubjectCode).FirstOrDefault().Guid);
            course.CourseLevels = new List<Dtos.GuidObject2>();
            if (source.CourseLevelCodes != null && source.CourseLevelCodes.Count > 0)
            {
                var courseLevelGuids = new List<Dtos.GuidObject2>();
                var courseLevels = await _studentReferenceDataRepository.GetCourseLevelsAsync();
                foreach (var courseLevelCode in source.CourseLevelCodes)
                {

                    var level = courseLevels.Where(cl => cl.Code == courseLevelCode).FirstOrDefault();
                    if (level != null) courseLevelGuids.Add(new Dtos.GuidObject2(level.Guid));

                }
                course.CourseLevels = courseLevelGuids;
            }

            course.InstructionMethods = new List<Dtos.GuidObject2>();
            if (source.InstructionalMethodCodes != null && source.InstructionalMethodCodes.Count > 0)
            {
                var instructionMethodGuids = new List<Dtos.GuidObject2>();
                var instructionalMethods = await _studentReferenceDataRepository.GetInstructionalMethodsAsync();
                foreach (var instrMethodCode in source.InstructionalMethodCodes)
                {
                    var mthd = instructionalMethods.Where(im => im.Code == instrMethodCode).FirstOrDefault();
                    if (mthd != null) instructionMethodGuids.Add(new Dtos.GuidObject2(mthd.Guid));
                }
                course.InstructionMethods = instructionMethodGuids;
            }

            var acadLevel = (await _studentReferenceDataRepository.GetAcademicLevelsAsync()).Where(al => al.Code == source.AcademicLevelCode).FirstOrDefault();
            if (acadLevel != null)
                course.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(acadLevel.Guid) };

            course.GradeSchemes = new List<Dtos.GuidObject2>();
            if (!string.IsNullOrEmpty(source.GradeSchemeCode))
            {
                var gradeSchemeGuids = new List<Dtos.GuidObject2>();
                var scheme = (await _studentReferenceDataRepository.GetGradeSchemesAsync()).Where(gs => gs.Code == source.GradeSchemeCode).FirstOrDefault();
                if (scheme != null) gradeSchemeGuids.Add(new Dtos.GuidObject2(scheme.Guid));
                course.GradeSchemes = gradeSchemeGuids;
            }

            course.Title = source.LongTitle;
            course.Description = source.Description;

            // Determine the Department information for the course
            course.OwningOrganizations = new List<Ellucian.Colleague.Dtos.OfferingOrganization2>();
            var departments = new List<Ellucian.Colleague.Dtos.OfferingOrganization2>();
            if (source.Departments != null && source.Departments.Count > 0)
            {
                foreach (var offeringDept in source.Departments)
                {
                    var academicDepartment = (await _referenceDataRepository.DepartmentsAsync()).FirstOrDefault(d => d.Code == offeringDept.AcademicDepartmentCode);
                    if (academicDepartment != null)
                    {
                        var department = new Ellucian.Colleague.Dtos.OfferingOrganization2();
                        department.Organization.Id = academicDepartment.Guid;
                        department.Share = offeringDept.ResponsibilityPercentage;
                        departments.Add(department);
                    }
                }
                course.OwningOrganizations = departments;
            }

            // Use the Start Date (if supplied); otherwise, use the current date (unless end date is present and less than current date, then set start date to the end date value)
            course.EffectiveStartDate = source.StartDate.HasValue && source.StartDate != default(DateTime) ? source.StartDate.Value :
                (source.EndDate.HasValue && source.EndDate.Value < DateTime.Today ? source.EndDate.Value : DateTime.Today);
            course.EffectiveEndDate = source.EndDate;
            course.Number = source.Number;

            // Determine the Credit information for the course
            course.Credits = new List<Dtos.Credit2>();
            CreditCategory creditType = (await _studentReferenceDataRepository.GetCreditCategoriesAsync()).Where(ct => ct.Code == source.LocalCreditType).FirstOrDefault();
            if (creditType != null)
            {
                var creditCategory = new CreditIdAndTypeProperty();
                creditCategory.Detail = new GuidObject2(creditType.Guid);

                switch (creditType.CreditType)
                {
                    case CreditType.ContinuingEducation:
                        creditCategory.CreditType = CreditCategoryType2.ContinuingEducation;
                        break;
                    case CreditType.Institutional:
                        creditCategory.CreditType = CreditCategoryType2.Institutional;
                        break;
                    case CreditType.Transfer:
                        creditCategory.CreditType = CreditCategoryType2.Transfer;
                        break;
                    default:
                        creditCategory.CreditType = CreditCategoryType2.ContinuingEducation;
                        break;
                }

                course.Credits.Add(new Dtos.Credit2()
                {
                    CreditCategory = creditCategory,
                    Measure = (creditType.CreditType == CreditType.ContinuingEducation) ? Dtos.CreditMeasure2.CEU : Dtos.CreditMeasure2.Credit,
                    Minimum = source.Ceus.HasValue ? source.Ceus.Value : source.MinimumCredits.GetValueOrDefault(),
                    Maximum = source.MaximumCredits,
                    Increment = source.VariableCreditIncrement
                });
            }

            if (course.MetadataObject == null)
                course.MetadataObject = new Dtos.DtoProperties.MetaDataDtoProperty();
            course.MetadataObject.CreatedBy = string.IsNullOrEmpty(source.ExternalSource) ? "Colleague" : source.ExternalSource;

            return course;
        }
        /// <summary>
        /// get resources via list of IDs
        /// </summary>
        /// <param name="courseIds"></param>
        /// <returns>Course DTO Objects</returns>
        public async Task<IEnumerable<Dtos.Student.Course>> GetCoursesByIdAsync(IEnumerable<string> courseIds)
        {
            var CoursesDto = new List<Dtos.Student.Course>();
            var courseEntities = await _courseRepository.GetCoursesByIdAsync(courseIds);
            if (courseEntities != null && courseEntities.Any())
            {
                foreach (var course in courseEntities)
                {
                    CoursesDto.Add(BuildCourseDto(course));
                }
            }
            return CoursesDto;
        }

        /// <summary>
        /// Converts a Course DTO to its corresponding Course domain entity
        /// </summary>
        /// <param name="source">A <see cref="Course">Course</see> DTO</param>
        /// <returns>A<see cref="Course">Course</see> domain entity</returns>
        private async Task<Domain.Student.Entities.Course> ConvertCourseDtoToEntity2Async(Dtos.Course2 course)
        {

            if (course == null)
            {
                throw new ArgumentNullException("course payload is invalid", "A course must be supplied.");
            }
            if (string.IsNullOrEmpty(course.Title))
            {
                throw new ArgumentException("A course title must be provided.");
            }
            if (course.Subject == null || string.IsNullOrEmpty(course.Subject.Id))
            {
                throw new ArgumentException("Subject is required; no Id supplied");
            }
            if (string.IsNullOrEmpty(course.Number))
            {
                throw new ArgumentException("Course number is required.");
            }
            if (course.Number.Length > 7)
            {
                throw new ArgumentException("Course number cannot be longer than 7 characters.");
            }
            if (course.EffectiveStartDate == default(DateTime))
            {
                throw new ArgumentException("Course start date is required.");
            }

            if (course.CourseLevels != null && course.CourseLevels.Any())
            {
                foreach (var level in course.CourseLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException("Course Level id is a required field when Course Levels are in the message body.");
                    }
                }
            }

            if (course.InstructionMethods != null && course.InstructionMethods.Any())
            {
                foreach (var method in course.InstructionMethods)
                {
                    if (string.IsNullOrEmpty(method.Id))
                    {
                        throw new ArgumentException("Instructional Method id is a required field when Instructional Methods are in the message body.");
                    }
                }
            }

            if (course.AcademicLevels != null && course.AcademicLevels.Any())
            {
                foreach (var level in course.AcademicLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException("Academic Level id is a required field when Academic Levels are in the message body.");
                    }
                }
            }

            if (course.GradeSchemes != null && course.GradeSchemes.Any())
            {
                foreach (var scheme in course.GradeSchemes)
                {
                    if (string.IsNullOrEmpty(scheme.Id))
                    {
                        throw new ArgumentException("Grade Scheme id is a required field when Grade Schemes are in the message body.");
                    }
                }
            }

            if (course.OwningOrganizations != null && course.OwningOrganizations.Any())
            {
                foreach (var org in course.OwningOrganizations)
                {
                    if (org.Organization == null || string.IsNullOrEmpty(org.Organization.Id))
                    {
                        throw new ArgumentException("Organization id is a required field when Owning Organizations are in the message body.");
                    }

                    if (org.Share == null)
                    {
                        throw new ArgumentException("Ownership Percentage is a required field when Owning Organizations are in the message body.");
                    }
                }
            }

            if (course.Credits != null && course.Credits.Any())
            {
                foreach (var credit in course.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException("Credit Category is required if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required if for Credit Categories if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.Detail != null && string.IsNullOrEmpty(credit.CreditCategory.Detail.Id))
                    {
                        throw new ArgumentException("Credit Category id is required within the detail section of Credit Category if it is in the message body.");
                    }
                }
            }

            await ReadCourseCodesAsync(false);

            var courseConfig = await _studentConfigRepository.GetCurriculumConfigurationAsync();

            VerifyCurriculumConfiguration(courseConfig);

            // Set the subject based on the supplied GUID
            var subjectCode = ConvertGuidToCode(_subjects, course.Subject.Id);

            if (subjectCode == null)
            {
                throw new ArgumentException(string.Concat("Subject for id '", course.Subject.Id.ToString(), "' was not found. Valid Subject is required."));
            }

            // Set the list of departments/shares
            List<OfferingDepartment> offeringDepartments = new List<OfferingDepartment>();

            // If we have supplied owning organization, then use that first.
            if (course.OwningOrganizations != null && course.OwningOrganizations.Any())
            {
                foreach (var owningOrg in course.OwningOrganizations)
                {
                    var department = _departments.Where(d => d.Guid == owningOrg.Organization.Id).FirstOrDefault();
                    var academicDepartment = department != null ? _academicDepartments.FirstOrDefault(ad => ad.Code == department.Code) : null;
                    if (academicDepartment != null)
                    {
                        offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, owningOrg.Share.Value));
                    }
                }
            }

            // If we don't have owning organizations then try using the subject-department mapping 
            // to determine department based on subject
            if (offeringDepartments.Count() == 0 && courseConfig.SubjectDepartmentMapping.Items != null && courseConfig.SubjectDepartmentMapping.Items.Count > 0)
            {
                var deptMapping = courseConfig.SubjectDepartmentMapping.Items.Where(i => i.OriginalCode == subjectCode).FirstOrDefault();
                var deptCode = deptMapping != null ? deptMapping.NewCode : null;
                var department = deptCode != null ? _departments.Where(d => d.Code == deptCode).FirstOrDefault() : null;
                var academicDepartment = department != null ? _academicDepartments.Where(ad => ad.Code == department.Code).FirstOrDefault() : null;

                if (academicDepartment != null)
                {
                    offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, 100m));
                }
            }

            // If we still don't have a valid department then check the subject code.
            // If the subject code is also a valid academic department then use that.
            if (offeringDepartments.Count == 0 && !string.IsNullOrEmpty(subjectCode))
            {
                var department = subjectCode != null ? _departments.Where(d => d.Code == subjectCode).FirstOrDefault() : null;
                var academicDepartment = department != null ? _academicDepartments.Where(ad => ad.Code == department.Code).FirstOrDefault() : null;
                if (academicDepartment != null)
                {
                    offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, 100m));
                }
            }

            // Department could not be determined from supplied subject or owning organization
            if (offeringDepartments.Count == 0)
            {
                throw new ArgumentException("Department could not be determined for subject " + subjectCode);
            }

            // Set the academic level code based on the supplied GUID or the default if one is not supplied
            string acadLevelCode = null;
            if (course.AcademicLevels != null && course.AcademicLevels.Any())
            {
                foreach (var acadLevel in course.AcademicLevels)
                {
                    try
                    {
                        acadLevelCode = _academicLevels.Where(al => al.Guid == acadLevel.Id).First().Code;
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + acadLevel.Id + "' supplied for academicLevels");
                    }
                }
            }
            else
            {
                acadLevelCode = courseConfig.DefaultAcademicLevelCode;
            }

            // Set the list of course level codes based on the supplied GUIDs or the default if one is not supplied
            List<string> courseLevelCodes = new List<string>();
            if (course.CourseLevels != null && course.CourseLevels.Any())
            {
                foreach (var courseLevel in course.CourseLevels)
                {
                    try
                    {
                        courseLevelCodes.Add(_courseLevels.First(cl => cl.Guid == courseLevel.Id).Code);
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + courseLevel.Id + "' supplied for courseLevels");
                    }
                }
            }
            else
            {
                courseLevelCodes.Add(courseConfig.DefaultCourseLevelCode);
            }

            // Set the list of instruction method codes based on the supplied GUIDs
            List<string> instructionMethodCodes = new List<string>();
            if (course.InstructionMethods != null && course.InstructionMethods.Any())
            {
                foreach (var instrMethod in course.InstructionMethods)
                {
                    try
                    {
                        instructionMethodCodes.Add(_instructionalMethods.First(im => im.Guid == instrMethod.Id).Code);
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + instrMethod.Id + "' supplied for instructionalMethods");
                    }
                }
            }
            // If instructional method GUIDs are not supplied, use the ERP default
            else
            {
                instructionMethodCodes.Add(courseConfig.DefaultInstructionalMethodCode);
            }

            // Set the list of course approvals based on the supplied GUIDs
            List<CourseApproval> courseApprovals = new List<CourseApproval>();
            var approvingAgencyId = courseConfig.ApprovingAgencyId;
            var approverId = courseConfig.ApproverId;
            string statusCode;
            var status = CourseStatus.Unknown;
            var today = DateTime.Today;
            if (course.EffectiveEndDate == null || course.EffectiveEndDate >= today)
            {
                statusCode = courseConfig.CourseActiveStatusCode;
                status = CourseStatus.Active;
            }
            else
            {
                statusCode = courseConfig.CourseInactiveStatusCode;
                status = CourseStatus.Terminated;
            }

            courseApprovals.Add(new CourseApproval(statusCode, DateTime.Today, approvingAgencyId, approverId, DateTime.Today)
            {
                Status = status
            });

            // Set the list of grade scheme codes based on the supplied GUIDs
            string gradeSchemeCode = null;
            if (course.GradeSchemes != null && course.GradeSchemes.Any() && !string.IsNullOrEmpty(course.GradeSchemes.ToList()[0].Id))
            {
                try
                {
                    gradeSchemeCode = _gradeSchemes.First(gs => gs.Guid == course.GradeSchemes.ToList()[0].Id).Code;
                }
                catch
                {
                    throw new ArgumentException("Invalid Id '" + course.GradeSchemes.ToList()[0].Id + "' supplied for gradeSchemes");
                }
            }

            // Set the credit type and credits/CEUs for the course based on the supplied GUID, or using the ERP default 
            CreditCategory creditCategory = null;
            if (course.Credits != null &&
                course.Credits.Count > 0 &&
                course.Credits.ToList()[0].CreditCategory != null &&
                course.Credits.ToList()[0].CreditCategory.Detail != null &&
                !string.IsNullOrEmpty(course.Credits.ToList()[0].CreditCategory.Detail.Id))
            {
                creditCategory = _creditCategories.FirstOrDefault(ct => ct.Guid == course.Credits.ToList()[0].CreditCategory.Detail.Id);
                if (creditCategory == null)
                {
                    throw new ArgumentException("Invalid Id '" + course.Credits.ToList()[0].CreditCategory.Detail.Id + "' supplied for creditCategory");
                }
            }
            // If we don't have a GUID then check for a CreditType enumeration value
            if (creditCategory == null &&
                course.Credits.Count > 0 &&
                course.Credits.ToList()[0].CreditCategory != null &&
                course.Credits.ToList()[0].CreditCategory.CreditType != null)
            {
                // Find the credit category that matches the enumeration
                switch (course.Credits.ToList()[0].CreditCategory.CreditType)
                {
                    case (CreditCategoryType2.ContinuingEducation):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.ContinuingEducation);
                        break;
                    case (CreditCategoryType2.Institutional):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Institutional);
                        break;
                    case (CreditCategoryType2.Transfer):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Transfer);
                        break;
                }
            }
            if (creditCategory == null)
            {
                // Get default from course configuration
                creditCategory = _creditCategories.FirstOrDefault(ct => ct.Code == courseConfig.DefaultCreditTypeCode);
            }
            //If creditCategory is null then throw an exception
            if (creditCategory == null)
            {
                throw new ArgumentException(string.Format("Credit category {0} was not found", courseConfig.DefaultCreditTypeCode));
            }

            var creditTypeCode = creditCategory == null ? string.Empty : creditCategory.Code;
            var creditInfo = (course.Credits == null || course.Credits.Count == 0) ? null : course.Credits.ToList()[0];
            var measure = creditInfo == null ? null : creditInfo.Measure;
            decimal? minCredits;
            decimal? ceus;
            decimal? maxCredits;
            decimal? varIncrCredits;

            if (measure == Dtos.CreditMeasure2.CEU)
            {
                minCredits = null;
                maxCredits = null;
                varIncrCredits = null;
                ceus = creditInfo == null ? 0 : creditInfo.Minimum;
            }
            else
            {
                minCredits = creditInfo == null ? 0 : creditInfo.Minimum;
                maxCredits = creditInfo == null ? null : creditInfo.Maximum;
                varIncrCredits = creditInfo == null ? null : creditInfo.Increment;
                ceus = null;
            }

            var courseDelimeter = !string.IsNullOrEmpty(courseConfig.CourseDelimiter) ? courseConfig.CourseDelimiter : CourseDelimiter;

            //Handle GUID requiredness
            string guid = (course.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)) ? string.Empty : course.Id;

            // Build the course entity
            var courseEntity = new Ellucian.Colleague.Domain.Student.Entities.Course(null, course.Title, course.Title, offeringDepartments,
                subjectCode, course.Number, acadLevelCode, courseLevelCodes, minCredits, ceus, courseApprovals)
            {
                LocalCreditType = creditTypeCode,
                Description = course.Description,
                Guid = guid,
                GradeSchemeCode = gradeSchemeCode,
                StartDate = course.EffectiveStartDate,
                EndDate = course.EffectiveEndDate,
                Name = subjectCode + courseDelimeter + course.Number,
                MaximumCredits = maxCredits,
                VariableCreditIncrement = varIncrCredits,
                AllowPassNoPass = courseConfig.AllowPassNoPass,
                AllowAudit = courseConfig.AllowAudit,
                OnlyPassNoPass = courseConfig.OnlyPassNoPass,
                AllowWaitlist = courseConfig.AllowWaitlist,
                IsInstructorConsentRequired = courseConfig.IsInstructorConsentRequired,
                WaitlistRatingCode = courseConfig.WaitlistRatingCode
            };

            // Add any supplied instruction method codes to the entity
            if (instructionMethodCodes != null && instructionMethodCodes.Count > 0)
            {
                foreach (var instrMethod in instructionMethodCodes)
                {
                    courseEntity.AddInstructionalMethodCode(instrMethod);
                }
            }

            // Verify that all the data in the entity is valid
            CourseProcessor.ValidateCourseData(courseEntity, _academicLevels, _courseLevels, null, _creditCategories, _academicDepartments,
                _gradeSchemes, _instructionalMethods, _subjects, _locations, _topicCodes);

            return courseEntity;
        }

        /// <summary>
        /// OBSOLETE AS OF API VERSION 1.3. REPLACED BY BuildSectionDto2
        /// A helper method to transform a set of section domain objects into a set of section DTOs.
        /// </summary>
        /// <param name="sections">A set of section domain objects</param>
        /// <returns>A set of Section DTOs</returns>
        private PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section>> BuildPrivacyWrappedSectionDto(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections)
        {
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section>();
            var hasPrivacyRestriction = false;
            List<Dtos.Student.Section> sectionDtos = new List<Dtos.Student.Section>();

            foreach (var section in sections)
            {
                if (section != null)
                {
                    Dtos.Student.Section sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<IEnumerable<Dtos.Student.Section>>(sectionDtos, hasPrivacyRestriction);
        }

        /// <summary>
        /// A helper method to transform a set of section domain objects into a set of section DTOs.
        /// </summary>
        /// <param name="sections">A set of section domain objects</param>
        /// <returns>A set of Section2 DTOs</returns>
        private PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section2>> BuildPrivacyWrappedSectionDto2(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections)
        {
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section2>();
            var hasPrivacyRestriction = false;
            List<Dtos.Student.Section2> sectionDtos = new List<Dtos.Student.Section2>();

            foreach (var section in sections)
            {
                if (section != null)
                {
                    Dtos.Student.Section2 sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<IEnumerable<Dtos.Student.Section2>>(sectionDtos, hasPrivacyRestriction);
        }

        /// <summary>
        /// A helper method to transform a set of section domain objects into a set of section DTOs.
        /// </summary>
        /// <param name="sections">A set of section domain objects</param>
        /// <returns>A set of Section3 DTOs</returns>
        private PrivacyWrapper<IEnumerable<Ellucian.Colleague.Dtos.Student.Section3>> BuildPrivacyWrappedSection3Dtos(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> sections)
        {
            var sectionDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section3>();
            var hasPrivacyRestriction = false;
            List<Dtos.Student.Section3> sectionDtos = new List<Dtos.Student.Section3>();

            foreach (var section in sections)
            {
                if (section != null)
                {
                    Dtos.Student.Section3 sectionDto = sectionDtoAdapter.MapToType(section);
                    if (section.FacultyIds == null || !section.FacultyIds.Contains(CurrentUser.PersonId))
                    {
                        hasPrivacyRestriction = true;
                        sectionDto.ActiveStudentIds = new List<string>();
                    }
                    sectionDtos.Add(sectionDto);
                }
            }

            return new PrivacyWrapper<IEnumerable<Dtos.Student.Section3>>(sectionDtos, hasPrivacyRestriction);
        }

        /// <summary>
        /// Returns a CourseSectionResult based on the supplied search and filters. Includes support of:
        ///    subject (course filter)
        ///    academic level (course filter)
        ///    course level (course filter)
        ///    location (section filter)
        /// </summary>
        /// <param name="criteria">Course search criteria</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="pageIndex">Page number to return</param>
        /// <returns>CourseSectionResult - filter result</returns>
        private async Task<CourseSectionResult> BuildFilterResultAsync(CourseSearchCriteria criteria, int pageSize, int pageIndex)
        {
            // Courses and Sections Returned by search. Later used as basis for filter counts.
            var searchResult = new CourseSectionResult();

            // Courses and Sections Returned by filter
            var filterResult = new CourseSectionResult();

            // Just in case, clean up incoming identifiers
            string keyword = "";
            if (criteria.Keyword != null)
            {
                keyword = criteria.Keyword.Trim();
            }

            List<string> courseIds = null;
            if (criteria.CourseIds != null)
            {
                foreach (var courseid in criteria.CourseIds)
                {
                    if (!string.IsNullOrEmpty(courseid))
                    {
                        if (courseIds == null) courseIds = new List<string>();
                        courseIds.Add(courseid.Trim());
                    }
                }
            }

            var sectionIds = new List<string>();
            if (criteria.SectionIds != null)
            {
                foreach (var sectionid in criteria.SectionIds)
                {
                    if (!string.IsNullOrEmpty(sectionid))
                    {
                        sectionIds.Add(sectionid.Trim());
                    }
                }
            }

            // SEARCH BY KEYWORD
            // If a keyword has been specified, get all courses and sections and do a keyword search
            if (!(string.IsNullOrEmpty(keyword)))
            {
                // Call method to search both courses and sections
                searchResult = await SearchByKeywordAsync(keyword);

                // Filter courses and sections as specified in criteria
                filterResult = await FilterResultsAsync(searchResult, criteria);

                // Ensure there is a course for every section in the filter result
                // This could occur as a result of the keyword search
                // Sort results by subject/number (aka course name)
                // Remove line that sorted result by course name
                filterResult.Courses = (await RetrieveCoursesForFilteredSectionsAsync(filterResult)).ToList();
            }
            // SEARCH BY COURSE ID
            // If a course ID has been specified, get all courses with that ID and any equivalents
            else if (courseIds != null)
            {
                // Call methods to search both courses and sections
                searchResult.Courses = (await SearchByIdAsync(courseIds)).ToList();

                // Get associated sections
                searchResult.Sections = (await GetSectionsForCoursesAsync(searchResult.Courses)).ToList();

                // Filter courses and sections as specified in criteria (this should do nothing)
                filterResult = await FilterResultsAsync(searchResult, criteria);
            }

            // SEARCH BY REQUIREMENT GROUP
            // If a requirement group has been specified, search based on requirement, subrequirement and group
            else if (criteria.RequirementGroup != null)
            {
                Requirement requirement = null;
                try
                {
                    requirement = await _requirementRepository.GetAsync(criteria.RequirementGroup.RequirementCode);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format("Requirement Code {0} was not found", criteria.RequirementGroup.RequirementCode), ex);
                }
                Subrequirement subrequirement = null;
                try
                {
                    subrequirement = requirement.SubRequirements.Where(s => s.Id == criteria.RequirementGroup.SubRequirementId).First();
                }
                catch (InvalidOperationException ex)
                {
                    throw new ArgumentException(string.Format("Subrequirement Id {0} was not found in Requirement {1}", criteria.RequirementGroup.SubRequirementId, criteria.RequirementGroup.RequirementCode), ex);
                }
                Ellucian.Colleague.Domain.Student.Entities.Requirements.Group group = null;
                try
                {
                    group = subrequirement.Groups.Where(g => g.Id == criteria.RequirementGroup.GroupId).First();
                }
                catch (InvalidOperationException ex)
                {
                    throw new ArgumentException(string.Format("Group {0} was not found in Subrequirement {1}", criteria.RequirementGroup.GroupId, criteria.RequirementGroup.SubRequirementId), ex);
                }
                // Search all current courses with the given requirement.
                searchResult.Courses = (await GetCatalogAsync()).ToList();
                searchResult.Courses = (await SearchByRequirementGroupAsync(searchResult.Courses, group)).ToList();

                // Get associated sections
                searchResult.Sections = (await GetSectionsForCoursesAsync(searchResult.Courses)).ToList();

                // Filter courses and sections as specified in criteria
                filterResult = await FilterResultsAsync(searchResult, criteria);
            }

            // SEARCH BY REQUIREMENT CODE 
            // If a requirement code has been specified, as in the case of a prerequisite search, search on the entire requirement 
            else if (!string.IsNullOrEmpty(criteria.RequirementCode))
            {
                searchResult.Courses = new List<Domain.Student.Entities.Course>();

                // Search all current catalog courses for the given requirement.
                List<Domain.Student.Entities.Course> fullCatalog = (await GetCatalogAsync()).ToList();

                Requirement requirement = null;
                try
                {
                    requirement = await _requirementRepository.GetAsync(criteria.RequirementCode);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format("Requirement {0} was not found", criteria.RequirementCode), ex);
                }

                // Loop through all the subrequirements on this requirement. (In the case of a prereq there will only be 1.)
                foreach (var subRequirement in requirement.SubRequirements)
                {
                    foreach (var group in subRequirement.Groups)
                    {
                        var matchingCourses = await SearchByRequirementGroupAsync(fullCatalog, group);
                        if (matchingCourses != null && matchingCourses.Any())
                        {
                            searchResult.Courses.AddRange(matchingCourses);
                        }
                    }
                }

                // Get associated sections
                searchResult.Sections = (await GetSectionsForCoursesAsync(searchResult.Courses)).ToList();

                // Filter courses and sections as specified in criteria
                filterResult = await FilterResultsAsync(searchResult, criteria);
            }

            // GET SECTIONS (AND COURSES) FOR SPECIFIC SECTION IDS
            else if (sectionIds != null && sectionIds.Any())
            {
                // Get all sections for the specified Ids
                searchResult.Sections = (await _sectionRepository.GetCachedSectionsAsync(sectionIds)).Where(s => s.IsActive && !s.HideInCatalog).ToList();

                // Get the courses for these sections
                var sectionCourseIds = searchResult.Sections.Select(s => s.CourseId).Distinct();
                if (sectionCourseIds.Any())
                {
                    searchResult.Courses = (await GetCatalogAsync()).Where(c => sectionCourseIds.Contains(c.Id)).ToList();
                }

                // Further filter courses and sections as specified in criteria
                filterResult = await FilterResultsAsync(searchResult, criteria);

                return filterResult;
            }

            // FILTER ALL COURSES AND SECTIONS
            // By default, get all courses, sort by subject/number
            else
            {
                // Get all courses
                searchResult.Courses = (await GetCatalogAsync()).ToList();

                // Get all sections
                searchResult.Sections = (await GetSectionsForCoursesAsync(searchResult.Courses)).ToList();

                // Filter courses and sections as specified in criteria
                filterResult = await FilterResultsAsync(searchResult, criteria);
            }
            return filterResult;

        }

        private async Task<IEnumerable<Domain.Student.Entities.Course>> FilterCoursesAgainstRulesAsync(IEnumerable<Domain.Student.Entities.Course> courses, IEnumerable<RequirementRule> rules)
        {
            if (rules == null || rules.Count() == 0)
            {
                return courses;
            }
            else
            {
                // Pair each course up with each rule
                var courseRuleRequests = new List<RuleRequest<Domain.Student.Entities.Course>>();
                foreach (var course in courses)
                {
                    foreach (var rule in rules)
                    {
                        courseRuleRequests.Add(new RuleRequest<Domain.Student.Entities.Course>(rule.CourseRule, course));
                    }
                }

                // Evaluate courses against rules
                var courseResults = await _ruleRepository.ExecuteAsync<Domain.Student.Entities.Course>(courseRuleRequests);

                // Add each course that did not fail any rule to the returned list
                var coursesThatPassRules = new List<Domain.Student.Entities.Course>();
                foreach (var course in courses)
                {
                    // Make sure that there are rules for the given course, and that none of the rules contains a false result
                    if ((courseResults.Where(cr => cr.Context == course).Any()) && courseResults.Where(cr => cr.Context == course && cr.Passed != true).Count() == 0)
                    {
                        coursesThatPassRules.Add(course);
                    }
                }

                return coursesThatPassRules;
            }
        }

        /// <summary>
        /// Translate credit type enumerated value to one of the external codes
        /// </summary>
        /// <param name="creditTypeCategory">Credit Type category</param>
        /// <returns>CreditType enumeration value</returns>
        private CreditType ConvertCreditTypeCategoryToCreditType(string creditTypeCategory)
        {
            var creditType = CreditType.Other;
            switch (creditTypeCategory)
            {
                case "I":
                    creditType = CreditType.Institutional;
                    break;
                case "C":
                    creditType = CreditType.ContinuingEducation;
                    break;
                case "T":
                    creditType = CreditType.Transfer;
                    break;
                default:
                    creditType = CreditType.Other;
                    break;
            }
            return creditType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Converts a CreditType entity enumeration value to its corresponding CreditType DTO enumeration value
        /// </summary>
        /// <param name="creditType">Credit Type entity enumeration value</param>
        /// <returns>CreditType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.CreditCategoryType GetDtoCreditType(CreditType creditType)
        {
            switch (creditType)
            {
                case CreditType.ContinuingEducation:
                    return Dtos.CreditCategoryType.ContinuingEducation;
                case CreditType.Institutional:
                    return Dtos.CreditCategoryType.Institutional;
                case CreditType.Transfer:
                    return Dtos.CreditCategoryType.Transfer;
                default:
                    return Dtos.CreditCategoryType.Other;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Converts a CreditType DTO enumeration value to its corresponding CreditType entity enumeration value
        /// </summary>
        /// <param name="creditType">Credit Type DTO enumeration value</param>
        /// <returns>CreditType entity enumeration value</returns>
        private CreditType GetCreditType(Ellucian.Colleague.Dtos.CreditCategoryType? creditType)
        {
            switch (creditType)
            {
                case Dtos.CreditCategoryType.ContinuingEducation:
                    return CreditType.ContinuingEducation;
                case Dtos.CreditCategoryType.Institutional:
                    return CreditType.Institutional;
                case Dtos.CreditCategoryType.Transfer:
                    return CreditType.Transfer;
                default:
                    return CreditType.Other;
            }
        }


        /// <summary>
        /// Converts a Contact Hours DTO enumeration value to its corresponding entity enumeration value
        /// </summary>
        /// <param name="period">Contact Hours DTO enumeration value</param>
        /// <returns>ContactPeriod entity enumeration value</returns>
        private string ConvertContactHoursPeriodDtoToEntity(Dtos.EnumProperties.ContactHoursPeriod period)
        {

            switch (period)
            {
                case Dtos.EnumProperties.ContactHoursPeriod.Day:
                    return _contactMeasures.First(cm => cm.ContactPeriod == ContactPeriod.day).Code;
                case Dtos.EnumProperties.ContactHoursPeriod.Month:
                    return _contactMeasures.First(cm => cm.ContactPeriod == ContactPeriod.month).Code;
                case Dtos.EnumProperties.ContactHoursPeriod.Week:
                    return _contactMeasures.First(cm => cm.ContactPeriod == ContactPeriod.week).Code;
                case Dtos.EnumProperties.ContactHoursPeriod.Term:
                    return _contactMeasures.First(cm => cm.ContactPeriod == ContactPeriod.term).Code;
                default:
                    return "";
            }
        }
        /// <summary>
        /// Converts a Contact period Entity enum to a ContactHoursPeriod DTO enum
        /// </summary>
        /// <returns></returns>
        private ContactHoursPeriod ConvertContactHoursPeriodEntityToDto(ContactPeriod period)
        {
            switch (period)
            {
                case ContactPeriod.day:
                    return ContactHoursPeriod.Day;
                case ContactPeriod.month:
                    return ContactHoursPeriod.Month;
                case ContactPeriod.week:
                    return ContactHoursPeriod.Week;
                case ContactPeriod.term:
                    return ContactHoursPeriod.Term;
                default:
                    return ContactHoursPeriod.NotSet;
            }
        }

        private async Task ReadCourseCodesAsync(bool ignoreCache)
        {
            if (_departments == null)
            {
                _departments = (await _referenceDataRepository.GetDepartmentsAsync(ignoreCache)).ToList();
            }
            if (_locations == null)
            {
                _locations = (await _referenceDataRepository.GetLocationsAsync(ignoreCache)).ToList();
            }
            if (_academicDepartments == null)
            {
                _academicDepartments = (await _studentReferenceDataRepository.GetAcademicDepartmentsAsync(ignoreCache)).ToList();
            }
            if (_academicLevels == null)
            {
                _academicLevels = (await _studentReferenceDataRepository.GetAcademicLevelsAsync(ignoreCache)).ToList();
            }
            if (_courseLevels == null)
            {
                _courseLevels = (await _studentReferenceDataRepository.GetCourseLevelsAsync(ignoreCache)).ToList();
            }
            if (_creditCategories == null)
            {
                _creditCategories = (await _studentReferenceDataRepository.GetCreditCategoriesAsync(ignoreCache)).ToList();
            }
            if (_gradeSchemes == null)
            {
                _gradeSchemes = (await _studentReferenceDataRepository.GetGradeSchemesAsync(ignoreCache)).ToList();
            }
            if (_instructionalMethods == null)
            {
                _instructionalMethods = (await _studentReferenceDataRepository.GetInstructionalMethodsAsync(ignoreCache)).ToList();
            }
            if (_administrativeInstructionalMethods == null)
            {
                _administrativeInstructionalMethods = (await _studentReferenceDataRepository.GetAdministrativeInstructionalMethodsAsync(ignoreCache)).ToList();
            }
            if (_subjects == null)
            {
                _subjects = (await _studentReferenceDataRepository.GetSubjectsAsync(ignoreCache)).ToList();
            }
            if (_topicCodes == null)
            {
                _topicCodes = (await _studentReferenceDataRepository.GetTopicCodesAsync(ignoreCache)).ToList();
            }
            if (_contactMeasures == null)
            {
                _contactMeasures = (await _studentReferenceDataRepository.GetContactMeasuresAsync(ignoreCache)).ToList();
            }
            if (_courseCategories == null)
            {
                _courseCategories = (await _studentReferenceDataRepository.GetCourseTypesAsync(ignoreCache)).ToList();
            }
            if (_courseStatuses == null)
            {
                _courseStatuses = (await _studentReferenceDataRepository.GetCourseStatusesAsync(ignoreCache)).ToList();
            }
            if (_courseTitleTypes == null)
            {
                _courseTitleTypes = (await _studentReferenceDataRepository.GetCourseTitleTypesAsync(ignoreCache)).ToList();
            }
        }

        /// <summary>
        /// Verification method to ensure that all integration parameters required for course processing are defined
        /// </summary>
        /// <param name="config">Curriculum Configuration</param>
        private void VerifyCurriculumConfiguration(CurriculumConfiguration config)
        {
            if (config == null)
            {
                throw new ConfigurationException("Curriculum Configuration setup is not complete.");
            }
            if (config.SubjectDepartmentMapping == null || config.SubjectDepartmentMapping.Items == null || config.SubjectDepartmentMapping.Items.Count == 0)
            {
                throw new ConfigurationException("Subject-to-department mapping setup is not complete.");
            }
            if (string.IsNullOrEmpty(config.DefaultAcademicLevelCode))
            {
                throw new ConfigurationException("A default academic level code must be specified.");
            }
            if (string.IsNullOrEmpty(config.DefaultCourseLevelCode))
            {
                throw new ConfigurationException("A default course level code must be specified.");
            }
            if (string.IsNullOrEmpty(config.ApproverId) && string.IsNullOrEmpty(config.ApprovingAgencyId))
            {
                throw new ConfigurationException("Either an approver ID or approving agency ID or both must be specified.");
            }
            if (string.IsNullOrEmpty(config.CourseActiveStatusCode))
            {
                throw new ConfigurationException("A default course active status code must be specified.");
            }
            if (string.IsNullOrEmpty(config.CourseInactiveStatusCode))
            {
                throw new ConfigurationException("A default course inactive status code must be specified.");
            }
        }

        #region V6 Changes

        /// <summary>
        /// Gets all or filtered courses
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="number"></param>
        /// <param name="academicLevel"></param>
        /// <param name="owningInstitutionUnits"></param>
        /// <param name="title"></param>
        /// <param name="instructionalMethods"></param>
        /// <param name="schedulingStartOn"></param>
        /// <param name="schedulingEndOn"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.Course3>, int>> GetCourses3Async(int offset, int limit, bool bypassCache, string subject, string number, string academicLevel, string owningInstitutionUnits, string title, string instructionalMethods, string schedulingStartOn, string schedulingEndOn)
        {
            var courseCollection = new List<Dtos.Course3>();
            int totalCount = 0;
            List<string> newAcademicLevel = null, newOwningInstitutionUnit = null, newInstructionalMethods = null;
            var newSubject = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(subject))
                {
                    newSubject = ConvertGuidToCode(await GetSubjectAsync(bypassCache), subject);
                    if (string.IsNullOrEmpty(newSubject))
                    {
                        return new Tuple<IEnumerable<Course3>, int>(new List<Course3>(), 0);
                    }
                }
                var newStartOn = (string.IsNullOrEmpty(schedulingStartOn) ? string.Empty : await ConvertDateArgument(schedulingStartOn));
                var newEndOn = (string.IsNullOrEmpty(schedulingEndOn) ? string.Empty : await ConvertDateArgument(schedulingEndOn));

                if (!string.IsNullOrEmpty(academicLevel))
                {
                    newAcademicLevel = ConvertGuidToCodeCollection(await GetAcademicLevelsAsync(bypassCache), new List<string>() { academicLevel });
                    if ((newAcademicLevel == null) || (!newAcademicLevel.Any()))
                    {
                        return new Tuple<IEnumerable<Course3>, int>(new List<Course3>(), 0);
                    }
                }
                if (!string.IsNullOrEmpty(owningInstitutionUnits))
                {
                    newOwningInstitutionUnit = ConvertGuidToCodeCollection(await GetDepartmentsAsync(bypassCache), new List<string>() { owningInstitutionUnits });
                    if ((newOwningInstitutionUnit == null) || (!newOwningInstitutionUnit.Any()))
                    {
                        return new Tuple<IEnumerable<Course3>, int>(new List<Course3>(), 0);
                    }
                }
                if (!string.IsNullOrEmpty(instructionalMethods))
                {
                    newInstructionalMethods = ConvertGuidToCodeCollection(await GetInstructionalMethodsAsync(bypassCache), new List<string>() { instructionalMethods });
                    if ((newInstructionalMethods == null) || (!newInstructionalMethods.Any()))
                    {
                        return new Tuple<IEnumerable<Course3>, int>(new List<Course3>(), 0);
                    }
                }
                var empty = string.Empty;

                Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int> courseEntities
                    = await _courseRepository.GetPagedCoursesAsync(offset, limit, newSubject, number, newAcademicLevel,
                   newOwningInstitutionUnit, title, newInstructionalMethods, newStartOn, newEndOn, empty, empty);
                totalCount = courseEntities.Item2;
                foreach (var course in courseEntities.Item1)
                {
                    courseCollection.Add(await ConvertCourseEntityToDto3Async(course, false));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred retrieving courses: " + ex.Message, ex.InnerException);
            }
            return courseCollection.Any() ? new Tuple<IEnumerable<Course3>, int>(courseCollection, totalCount) :
                    new Tuple<IEnumerable<Course3>, int>(new List<Course3>(), 0);
        }

        /// <summary>
        /// Converts to unidata date format
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private async Task<string> ConvertDateArgument(string date)
        {
            try
            {
                return await _sectionRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Date format in arguments");
            }
        }

        /// <summary>
        /// Convert a GUID to a code in a code file
        /// </summary>
        /// <param name="codeList">Source list of codes, must inherit GuidCodeItem</param>
        /// <param name="guid">GUID corresponding to a code</param>
        /// <returns>The code corresponding to the GUID</returns>
        protected new static string ConvertGuidToCode(IEnumerable<Domain.Entities.GuidCodeItem> codeList, string guid)
        {
            if (codeList == null || codeList.Count() == 0)
            {
                throw new ArgumentNullException("codeList");
            }
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var entity = codeList.FirstOrDefault(c => c.Guid == guid);
            return entity == null ? null : entity.Code;
        }

        /// <summary>
        /// Convert a GUID to a code in a code file
        /// </summary>
        /// <param name="codeList">Source list of codes, must inherit GuidCodeItem</param>
        /// <param name="guid">GUID corresponding to a code</param>
        /// <returns>The code corresponding to the GUID</returns>
        protected List<string> ConvertGuidToCodeCollection(IEnumerable<Domain.Entities.GuidCodeItem> codeList, List<string> guids)
        {

            if (codeList == null || codeList.Count() == 0)
            {
                throw new ArgumentNullException("codeList");
            }
            if (guids == null)
            {
                return null;
            }

            var guidCollection = new List<string>();

            foreach (var guid in guids)
            {
                var entity = codeList.FirstOrDefault(c => c.Guid == guid);
                if (entity != null)
                {
                    guidCollection.Add(entity.Code);
                }
            }

            return guidCollection;
        }

        /// <summary>
        /// Gets a course by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.Course3</returns>
        public async Task<Dtos.Course3> GetCourseByGuid3Async(string id)
        {
            var course = await _courseRepository.GetCourseByGuidAsync(id);

            var courseDto = await ConvertCourseEntityToDto3Async(course, true);
            return courseDto;
        }


        /// <summary>
        /// Update a course
        /// </summary>
        /// <param name="course"></param>
        /// <returns>Dtos.Course3</returns>
        public async Task<Dtos.Course3> UpdateCourse3Async(Course3 course)
        {
            if (course == null)
            {
                throw new ArgumentNullException("course payload is invalid", "Course DTO is required for PUT.");
            }
            // We must have a GUID so we can get the existing data
            if (string.IsNullOrEmpty(course.Id))
            {
                throw new KeyNotFoundException("Course must provide a GUID.");
            }

            // Confirm that user has permissions to update course
            CheckCoursePermission();

            _courseRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, update the course, convert the resulting entity back to a DTO, and return it
            var courseEntity = await ConvertCourseDtoToEntity3Async(course, false);
            var source = course.MetadataObject != null ? course.MetadataObject.CreatedBy : null;
            var updatedCourseEntity = await _courseRepository.UpdateCourseAsync(courseEntity, source);
            return await ConvertCourseEntityToDto3Async(updatedCourseEntity, true);
        }

        /// <summary>
        /// Creates a new course
        /// </summary>
        /// <param name="course"></param>
        /// <returns>Dtos.Course3</returns>
        public async Task<Dtos.Course3> CreateCourse3Async(Course3 course)
        {
            // Confirm that user has permissions to create course
            CheckCoursePermission();
            _courseRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, create the course, convert the resulting entity back to a DTO, and return it
            var courseEntity = await ConvertCourseDtoToEntity3Async(course, false);
            var source = course.MetadataObject != null ? course.MetadataObject.CreatedBy : null;
            var createdCourseEntity = await _courseRepository.CreateCourseAsync(courseEntity, source);
            return await ConvertCourseEntityToDto3Async(createdCourseEntity, false);
        }

        /// <summary>
        /// Converts entities to dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Dtos.Course3</returns>
        private async Task<Dtos.Course3> ConvertCourseEntityToDto3Async(Ellucian.Colleague.Domain.Student.Entities.Course source, bool bypassCache)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A course must be supplied.");
            }

            var course = new Ellucian.Colleague.Dtos.Course3();

            course.Id = source.Guid;
            var subjects = await GetSubjectAsync(bypassCache);
            if ((subjects != null) && (subjects.Any()))
            {
                var subject = subjects.FirstOrDefault(s => s.Code == source.SubjectCode);
                if (subject != null)
                {
                    course.Subject = new Dtos.GuidObject2(subject.Guid);
                }
            }
            course.CourseLevels = new List<Dtos.GuidObject2>();
            if (source.CourseLevelCodes != null && source.CourseLevelCodes.Count > 0)
            {
                var courseLevelGuids = new List<Dtos.GuidObject2>();
                var courseLevels = await GetCourseLevelsAsync(bypassCache);
                if ((courseLevels != null) && (courseLevels.Any()))
                {
                    foreach (var courseLevelCode in source.CourseLevelCodes)
                    {
                        var courseLevel = courseLevels.FirstOrDefault(cl => cl.Code == courseLevelCode);
                        if (courseLevel != null)
                        {
                            courseLevelGuids.Add(new Dtos.GuidObject2(courseLevel.Guid));
                        }
                    }
                }
                if (courseLevelGuids.Any())
                {
                    course.CourseLevels = courseLevelGuids;
                }
            }

            course.InstructionMethods = new List<Dtos.GuidObject2>();
            if (source.InstructionalMethodCodes != null && source.InstructionalMethodCodes.Count > 0)
            {
                var instructionMethodGuids = new List<Dtos.GuidObject2>();
                var instructionalMethods = await GetInstructionalMethodsAsync(bypassCache);
                if ((instructionalMethods != null) && (instructionalMethods.Any()))
                {
                    foreach (var instrMethodCode in source.InstructionalMethodCodes)
                    {
                        var instructionalMethod = instructionalMethods.FirstOrDefault(im => im.Code == instrMethodCode);
                        if (instructionalMethod != null)
                        {
                            instructionMethodGuids.Add(new Dtos.GuidObject2(instructionalMethod.Guid));
                        }
                    }
                }
                if (instructionMethodGuids.Any())
                {
                    course.InstructionMethods = instructionMethodGuids;
                }
            }

            if (!(string.IsNullOrEmpty(source.AcademicLevelCode)))
            {
                var academicLevels = await GetAcademicLevelsAsync(bypassCache);
                if ((academicLevels != null) && (academicLevels.Any()))
                {
                    var acadLevel = academicLevels.FirstOrDefault(al => al.Code == source.AcademicLevelCode);
                    if (acadLevel != null)
                        course.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(acadLevel.Guid) };
                }
            }

            if (!string.IsNullOrEmpty(source.GradeSchemeCode))
            {
                var gradeSchemes = await GetGradeSchemesAsync(bypassCache);
                if ((gradeSchemes != null) && (gradeSchemes.Any()))
                {
                    var gradeScheme = gradeSchemes.FirstOrDefault(gs => gs.Code == source.GradeSchemeCode);
                    if (gradeScheme != null)
                    {
                        course.GradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2((gradeScheme.Guid)) };
                    }
                }
            }

            course.Title = source.LongTitle;
            course.Description = source.Description;

            // Determine the Department information for the course
            course.OwningInstitutionUnits = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();
            var departments = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();

            if (source.Departments != null && source.Departments.Any())
            {
                var allDepartments = await GetDepartmentsAsync(bypassCache);
                if ((allDepartments != null) && (allDepartments.Any()))
                {
                    foreach (var offeringDept in source.Departments)
                    {
                        var academicDepartment = (allDepartments.FirstOrDefault(d => d.Code == offeringDept.AcademicDepartmentCode));
                        if (academicDepartment != null)
                        {
                            var department = new Ellucian.Colleague.Dtos.OwningInstitutionUnit
                            {
                                InstitutionUnit = { Id = academicDepartment.Guid },
                                OwnershipPercentage = offeringDept.ResponsibilityPercentage
                            };
                            departments.Add(department);
                        }
                    }
                }
                if ((departments != null) && (departments.Any()))
                {
                    course.OwningInstitutionUnits = departments;
                }
            }

            // Use the Start Date (if supplied); otherwise, use the current date (unless end date is present and less than current date, then set start date to the end date value)
            var today = DateTime.Today;
            course.EffectiveStartDate = source.StartDate.HasValue && source.StartDate != default(DateTime) ? source.StartDate.Value :
                (source.EndDate.HasValue && source.EndDate.Value < DateTime.Today ? source.EndDate.Value : new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Unspecified));
            course.EffectiveEndDate = source.EndDate;
            course.Number = source.Number;

            // Determine the Credit information for the course
            course.Credits = new List<Dtos.Credit3>();
            var creditCategories = await GetCreditCategoriesAsync(bypassCache);
            if ((creditCategories != null) && (creditCategories.Any()))
            {
                var creditType = creditCategories.FirstOrDefault(ct => ct.Code == source.LocalCreditType);
                if (creditType != null)
                {
                    var creditCategory = new CreditIdAndTypeProperty2() { Detail = new GuidObject2(creditType.Guid) };

                    switch (creditType.CreditType)
                    {
                        case CreditType.ContinuingEducation:
                            creditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                        case CreditType.Institutional:
                            creditCategory.CreditType = CreditCategoryType3.Institutional;
                            break;
                        case CreditType.Transfer:
                            creditCategory.CreditType = CreditCategoryType3.Transfer;
                            break;
                        case CreditType.Exchange:
                            creditCategory.CreditType = CreditCategoryType3.Exchange;
                            break;
                        case CreditType.Other:
                            creditCategory.CreditType = CreditCategoryType3.Other;
                            break;
                        case CreditType.None:
                            creditCategory.CreditType = CreditCategoryType3.NoCredit;
                            break;
                        default:
                            creditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                    }
                    if (source.Ceus.HasValue)
                    {
                        course.Credits.Add(new Dtos.Credit3()
                        {
                            CreditCategory = creditCategory,
                            Measure = Dtos.CreditMeasure2.CEU,
                            Minimum = source.Ceus,
                        });
                    }
                    if (source.MinimumCredits.HasValue)
                    {
                        course.Credits.Add(new Dtos.Credit3()
                        {
                            CreditCategory = creditCategory,
                            Measure = Dtos.CreditMeasure2.Credit,
                            Minimum = source.MinimumCredits,
                            Maximum = source.MaximumCredits,
                            Increment = source.VariableCreditIncrement
                        });
                    }
                }
            }

            if (course.MetadataObject == null)
                course.MetadataObject = new Dtos.DtoProperties.MetaDataDtoProperty();
            course.MetadataObject.CreatedBy = string.IsNullOrEmpty(source.ExternalSource) ? "Colleague" : source.ExternalSource;

            return course;
        }

        /// <summary>
        /// Gets credit category entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<CreditCategory>> GetCreditCategoriesAsync(bool bypassCache)
        {
            if (_creditCategories == null)
            {
                _creditCategories = (await _studentReferenceDataRepository.GetCreditCategoriesAsync(bypassCache)).ToList();
            }
            return _creditCategories;
        }

        /// <summary>
        /// Gets department entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>IEnumerable<Department></returns>
        private async Task<IEnumerable<Department>> GetDepartmentsAsync(bool bypassCache)
        {
            if (_departmentEntities == null)
            {
                _departmentEntities = await _referenceDataRepository.GetDepartmentsAsync(bypassCache);
            }
            return _departmentEntities;
        }

        /// <summary>
        /// Gets grade scheme  entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>IEnumerable<Domain.Student.Entities.GradeScheme></returns>
        private async Task<IEnumerable<Domain.Student.Entities.GradeScheme>> GetGradeSchemesAsync(bool bypassCache)
        {
            if (_gradeSchemes == null)
            {
                _gradeSchemes = (await _studentReferenceDataRepository.GetGradeSchemesAsync(bypassCache)).ToList();
            }
            return _gradeSchemes;
        }

        /// <summary>
        /// Gets academic level entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>IEnumerable<Domain.Student.Entities.AcademicLevel></returns>
        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> GetAcademicLevelsAsync(bool bypassCache)
        {
            if (_academicLevels == null)
            {
                _academicLevels = (await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache)).ToList();
            }
            return _academicLevels;
        }

        /// <summary>
        ///  Gets instructional method entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>List of <see cref="Domain.Student.Entities.InstructionalMethod"/> </returns>
        private async Task<IEnumerable<Domain.Student.Entities.InstructionalMethod>> GetInstructionalMethodsAsync(bool bypassCache)
        {
            if (_instructionalMethods == null)
            {
                _instructionalMethods = (await _studentReferenceDataRepository.GetInstructionalMethodsAsync(bypassCache)).ToList();
            }
            return _instructionalMethods;
        }

        /// <summary>
        ///  Gets administrative instructional method entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>List of <see cref="Domain.Student.Entities.AdministrativeInstructionalMethod"/></returns>
        private async Task<IEnumerable<Domain.Student.Entities.AdministrativeInstructionalMethod>> GetAdministrativeInstructionalMethodsAsync(bool bypassCache)
        {
            if (_administrativeInstructionalMethods == null)
            {
                _administrativeInstructionalMethods = (await _studentReferenceDataRepository.GetAdministrativeInstructionalMethodsAsync(bypassCache)).ToList();
            }
            return _administrativeInstructionalMethods;
        }

        /// <summary>
        /// Gets course level entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>IEnumerable<Domain.Student.Entities.CourseLevel></returns>
        private async Task<IEnumerable<Domain.Student.Entities.CourseLevel>> GetCourseLevelsAsync(bool bypassCache)
        {
            if (_courseLevels == null)
            {
                _courseLevels = (await _studentReferenceDataRepository.GetCourseLevelsAsync(bypassCache)).ToList();
            }
            return _courseLevels;
        }

        /// <summary>
        /// Gets subject entities
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>IEnumerable<Domain.Student.Entities.Subject></returns>
        private async Task<IEnumerable<Domain.Student.Entities.Subject>> GetSubjectAsync(bool bypassCache)
        {
            if (_subjects == null)
            {
                _subjects = (await _studentReferenceDataRepository.GetSubjectsAsync(bypassCache)).ToList();
            }
            return _subjects;
        }

        /// <summary>
        /// Converts a Course DTO to its corresponding Course domain entity
        /// </summary>
        /// <param name="source">A <see cref="Course">Course</see> DTO</param>
        /// <returns>A<see cref="Course">Course</see> domain entity</returns>
        private async Task<Domain.Student.Entities.Course> ConvertCourseDtoToEntity3Async(Dtos.Course3 course, bool bypassCache)
        {

            if (course == null)
            {
                throw new ArgumentNullException("course payload is invalid", "A course must be supplied.");
            }
            if (string.IsNullOrEmpty(course.Title))
            {
                throw new ArgumentException("A course title must be provided.");
            }
            if (course.Subject == null || string.IsNullOrEmpty(course.Subject.Id))
            {
                throw new ArgumentException("Subject is required; no Id supplied");
            }
            if (string.IsNullOrEmpty(course.Number))
            {
                throw new ArgumentException("Course number is required.");
            }
            if (course.Number.Length > 7)
            {
                throw new ArgumentException("Course number cannot be longer than 7 characters.");
            }
            if (course.EffectiveStartDate == default(DateTime))
            {
                throw new ArgumentException("Course start date is required.");
            }

            if (course.CourseLevels != null && course.CourseLevels.Any())
            {
                foreach (var level in course.CourseLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException("Course Level id is a required field when Course Levels are in the message body.");
                    }
                }
            }

            if (course.InstructionMethods != null && course.InstructionMethods.Any())
            {
                foreach (var method in course.InstructionMethods)
                {
                    if (string.IsNullOrEmpty(method.Id))
                    {
                        throw new ArgumentException("Instructional Method id is a required field when Instructional Methods are in the message body.");
                    }
                }
            }

            if (course.AcademicLevels != null && course.AcademicLevels.Any())
            {
                foreach (var level in course.AcademicLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException("Academic Level id is a required field when Academic Levels are in the message body.");
                    }
                }
            }

            if (course.GradeSchemes != null && course.GradeSchemes.Any())
            {
                foreach (var scheme in course.GradeSchemes)
                {
                    if (string.IsNullOrEmpty(scheme.Id))
                    {
                        throw new ArgumentException("Grade Scheme id is a required field when Grade Schemes are in the message body.");
                    }
                }
            }

            if (course.OwningInstitutionUnits != null && course.OwningInstitutionUnits.Any())
            {
                foreach (var org in course.OwningInstitutionUnits)
                {
                    if (org.InstitutionUnit == null || string.IsNullOrEmpty(org.InstitutionUnit.Id))
                    {
                        throw new ArgumentException("Institution Unit id is a required field when Owning Organizations are in the message body.");
                    }
                }
            }

            if (course.Credits != null && course.Credits.Any())
            {
                if (course.Credits.Count() > 2)
                {
                    throw new ArgumentException("A maximum of 2 entries are allowed in the Credits array.");
                }
                if (course.Credits.Count() == 2)
                {
                    if (course.Credits.ElementAt(0).CreditCategory.CreditType != course.Credits.ElementAt(1).CreditCategory.CreditType)
                    {
                        throw new ArgumentException("The same Credit Type must be used for each entry in the Credits array.");
                    }
                    if (!(course.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && course.Credits.ElementAt(1).Measure == CreditMeasure2.Credit)
                        && !(course.Credits.ElementAt(0).Measure == CreditMeasure2.Credit && course.Credits.ElementAt(1).Measure == CreditMeasure2.CEU)
                        && !(course.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && course.Credits.ElementAt(1).Measure == CreditMeasure2.Hours)
                        && !(course.Credits.ElementAt(0).Measure == CreditMeasure2.Hours && course.Credits.ElementAt(1).Measure == CreditMeasure2.CEU))
                    {
                        throw new ArgumentException("Invalid combination of measures '" + course.Credits.ElementAt(0).Measure
                            + "' and '" + course.Credits.ElementAt(1).Measure + "'");
                    }
                }
                foreach (var credit in course.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException("Credit Category is required if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required if for Credit Categories if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.Detail != null && string.IsNullOrEmpty(credit.CreditCategory.Detail.Id))
                    {
                        throw new ArgumentException("Credit Category id is required within the detail section of Credit Category if it is in the message body.");
                    }
                    if (credit.Increment == null && credit.Maximum != null)
                    {
                        throw new ArgumentException("Credit Increment is required when Credit Maximum exists.");
                    }
                    if (credit.Maximum == null && credit.Increment != null)
                    {
                        throw new ArgumentException("Credit Maximum is required when Credit Increment exists.");
                    }
                    if (credit.Maximum != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Maximum cannot exist when Credit Measure is 'ceu'.");
                    }
                    if (credit.Increment != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Increment cannot exist when Credit Measure is 'ceu'.");
                    }
                }
            }
            await ReadCourseCodesAsync(bypassCache);

            var courseConfig = await _studentConfigRepository.GetCurriculumConfigurationAsync();

            VerifyCurriculumConfiguration(courseConfig);

            // Set the subject based on the supplied GUID
            var subjectCode = ConvertGuidToCode(_subjects, course.Subject.Id);

            if (subjectCode == null)
            {
                throw new ArgumentException(string.Concat("Subject for id '", course.Subject.Id.ToString(), "' was not found. Valid Subject is required."));
            }

            // Set the list of departments/shares
            List<OfferingDepartment> offeringDepartments = new List<OfferingDepartment>();

            // If we have supplied owning organization, then use that first.
            if (course.OwningInstitutionUnits != null && course.OwningInstitutionUnits.Any())
            {
                foreach (var owningInstitutionUnit in course.OwningInstitutionUnits)
                {
                    var division = (await _referenceDataRepository.GetDivisionsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (division != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'division' is not supported.");
                    }

                    var school = (await _referenceDataRepository.GetSchoolsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (school != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'school' is not supported.");
                    }

                    var department = _departments.Where(d => d.Guid == owningInstitutionUnit.InstitutionUnit.Id).FirstOrDefault();
                    var academicDepartment = department != null ? _academicDepartments.FirstOrDefault(ad => ad.Code == department.Code) : null;
                    if (academicDepartment != null)
                    {
                        offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, owningInstitutionUnit.OwnershipPercentage));
                    }
                }
            }

            // If we don't have owning organizations then try using the subject-department mapping 
            // to determine department based on subject
            if (offeringDepartments.Count() == 0 && courseConfig.SubjectDepartmentMapping.Items != null && courseConfig.SubjectDepartmentMapping.Items.Count > 0)
            {
                var deptMapping = courseConfig.SubjectDepartmentMapping.Items.Where(i => i.OriginalCode == subjectCode).FirstOrDefault();
                var deptCode = deptMapping != null ? deptMapping.NewCode : null;
                var department = deptCode != null ? _departments.Where(d => d.Code == deptCode).FirstOrDefault() : null;
                var academicDepartment = department != null ? _academicDepartments.Where(ad => ad.Code == department.Code).FirstOrDefault() : null;

                if (academicDepartment != null)
                {
                    offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, 100m));
                }
            }

            // If we still don't have a valid department then check the subject code.
            // If the subject code is also a valid academic department then use that.
            if (offeringDepartments.Count == 0 && !string.IsNullOrEmpty(subjectCode))
            {
                var department = subjectCode != null ? _departments.Where(d => d.Code == subjectCode).FirstOrDefault() : null;
                var academicDepartment = department != null ? _academicDepartments.Where(ad => ad.Code == department.Code).FirstOrDefault() : null;
                if (academicDepartment != null)
                {
                    offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, 100m));
                }
            }

            // Department could not be determined from supplied subject or owning organization
            if (offeringDepartments.Count == 0)
            {
                throw new ArgumentException("Department could not be determined for subject " + subjectCode);
            }

            // Set the academic level code based on the supplied GUID or the default if one is not supplied
            string acadLevelCode = null;
            if (course.AcademicLevels != null && course.AcademicLevels.Any())
            {
                foreach (var acadLevel in course.AcademicLevels)
                {
                    try
                    {
                        acadLevelCode = _academicLevels.Where(al => al.Guid == acadLevel.Id).First().Code;
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + acadLevel.Id + "' supplied for academicLevels");
                    }
                }
            }
            else
            {
                acadLevelCode = courseConfig.DefaultAcademicLevelCode;
            }

            // Set the list of course level codes based on the supplied GUIDs or the default if one is not supplied
            List<string> courseLevelCodes = new List<string>();
            if (course.CourseLevels != null && course.CourseLevels.Any())
            {
                foreach (var courseLevel in course.CourseLevels)
                {
                    try
                    {
                        courseLevelCodes.Add(_courseLevels.First(cl => cl.Guid == courseLevel.Id).Code);
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + courseLevel.Id + "' supplied for courseLevels");
                    }
                }
            }
            else
            {
                courseLevelCodes.Add(courseConfig.DefaultCourseLevelCode);
            }

            // Set the list of instruction method codes based on the supplied GUIDs
            List<string> instructionMethodCodes = new List<string>();
            if (course.InstructionMethods != null && course.InstructionMethods.Any())
            {
                foreach (var instrMethod in course.InstructionMethods)
                {
                    try
                    {
                        instructionMethodCodes.Add(_instructionalMethods.First(im => im.Guid == instrMethod.Id).Code);
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + instrMethod.Id + "' supplied for instructionalMethods");
                    }
                }
            }
            // If instructional method GUIDs are not supplied, use the ERP default
            else
            {
                instructionMethodCodes.Add(courseConfig.DefaultInstructionalMethodCode);
            }

            // Set the list of course approvals based on the supplied GUIDs
            List<CourseApproval> courseApprovals = new List<CourseApproval>();
            var approvingAgencyId = courseConfig.ApprovingAgencyId;
            var approverId = courseConfig.ApproverId;
            string statusCode;
            var status = CourseStatus.Unknown;
            var today = DateTime.Today;
            if (course.EffectiveEndDate == null || course.EffectiveEndDate >= today)
            {
                statusCode = courseConfig.CourseActiveStatusCode;
                status = CourseStatus.Active;
            }
            else
            {
                statusCode = courseConfig.CourseInactiveStatusCode;
                status = CourseStatus.Terminated;
            }

            courseApprovals.Add(new CourseApproval(statusCode, DateTime.Today, approvingAgencyId, approverId, DateTime.Today)
            {
                Status = status
            });

            // Set the list of grade scheme codes based on the supplied GUIDs
            string gradeSchemeCode = null;
            if (course.GradeSchemes != null && course.GradeSchemes.Any() && !string.IsNullOrEmpty(course.GradeSchemes.ToList()[0].Id))
            {
                try
                {
                    gradeSchemeCode = _gradeSchemes.First(gs => gs.Guid == course.GradeSchemes.ToList()[0].Id).Code;
                }
                catch
                {
                    throw new ArgumentException("Invalid Id '" + course.GradeSchemes.ToList()[0].Id + "' supplied for gradeSchemes");
                }
            }

            // Set the credit type and credits/CEUs for the course based on the supplied GUID, or using the ERP default 
            CreditCategory creditCategory = null;
            if (course.Credits != null &&
                course.Credits.Any() &&
                course.Credits.ToList()[0].CreditCategory != null &&
                course.Credits.ToList()[0].CreditCategory.Detail != null &&
                !string.IsNullOrEmpty(course.Credits.ToList()[0].CreditCategory.Detail.Id))
            {
                creditCategory = _creditCategories.FirstOrDefault(ct => ct.Guid == course.Credits.ToList()[0].CreditCategory.Detail.Id);
                if (creditCategory == null)
                {
                    throw new ArgumentException("Invalid Id '" + course.Credits.ToList()[0].CreditCategory.Detail.Id + "' supplied for creditCategory");
                }
            }
            // If we don't have a GUID then check for a CreditType enumeration value
            if (creditCategory == null &&
                course.Credits != null &&
                course.Credits.Any() &&
                course.Credits.ToList()[0].CreditCategory != null &&
                course.Credits.ToList()[0].CreditCategory.CreditType != null)
            {
                if (course.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.Exam ||
                    course.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.WorkLifeExperience)
                {
                    throw new InvalidOperationException("Credit category type 'exam' or 'workLifeExperience' are not supported.");
                }

                // Find the credit category that matches the enumeration
                switch (course.Credits.ToList()[0].CreditCategory.CreditType)
                {
                    case (CreditCategoryType3.ContinuingEducation):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.ContinuingEducation);
                        break;
                    case (CreditCategoryType3.Institutional):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Institutional);
                        break;
                    case (CreditCategoryType3.Transfer):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Transfer);
                        break;
                    case (CreditCategoryType3.Exchange):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Exchange);
                        break;
                    case (CreditCategoryType3.Other):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Other);
                        break;
                    case (CreditCategoryType3.NoCredit):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.None);
                        break;
                }
            }
            if (creditCategory == null)
            {
                // Get default from course configuration
                creditCategory = _creditCategories.FirstOrDefault(ct => ct.Code == courseConfig.DefaultCreditTypeCode);
            }
            //If creditCategory is null then throw an exception
            if (creditCategory == null)
            {
                throw new ArgumentException(string.Format("Credit category {0} was not found", courseConfig.DefaultCreditTypeCode));
            }


            var creditTypeCode = creditCategory == null ? string.Empty : creditCategory.Code;

            decimal? minCredits;
            decimal? ceus;
            decimal? maxCredits;
            decimal? varIncrCredits;
            minCredits = null;
            maxCredits = null;
            varIncrCredits = null;
            ceus = null;
            foreach (var credits in course.Credits)
            {
                var creditInfo = (course.Credits == null || course.Credits.Count == 0) ? null : credits;
                var measure = creditInfo == null ? null : creditInfo.Measure;
                if (measure == Dtos.CreditMeasure2.CEU)
                {
                    ceus = creditInfo == null ? 0 : creditInfo.Minimum;
                }
                else
                {
                    minCredits = creditInfo == null ? 0 : creditInfo.Minimum;
                    maxCredits = creditInfo == null ? null : creditInfo.Maximum;
                    varIncrCredits = creditInfo == null ? null : creditInfo.Increment;
                }
            }
            var courseDelimeter = !string.IsNullOrEmpty(courseConfig.CourseDelimiter) ? courseConfig.CourseDelimiter : CourseDelimiter;

            //Handle GUID requiredness
            string guid = (course.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)) ? string.Empty : course.Id;

            // Build the course entity
            var courseEntity = new Ellucian.Colleague.Domain.Student.Entities.Course(null, course.Title, course.Title, offeringDepartments,
                subjectCode, course.Number, acadLevelCode, courseLevelCodes, minCredits, ceus, courseApprovals)
            {
                LocalCreditType = creditTypeCode,
                Description = course.Description,
                Guid = guid,
                GradeSchemeCode = gradeSchemeCode,
                StartDate = course.EffectiveStartDate,
                EndDate = course.EffectiveEndDate,
                Name = subjectCode + courseDelimeter + course.Number,
                MaximumCredits = maxCredits,
                VariableCreditIncrement = varIncrCredits,
                AllowPassNoPass = courseConfig.AllowPassNoPass,
                AllowAudit = courseConfig.AllowAudit,
                OnlyPassNoPass = courseConfig.OnlyPassNoPass,
                AllowWaitlist = courseConfig.AllowWaitlist,
                IsInstructorConsentRequired = courseConfig.IsInstructorConsentRequired,
                WaitlistRatingCode = courseConfig.WaitlistRatingCode
            };

            // Add any supplied instruction method codes to the entity
            if (instructionMethodCodes != null && instructionMethodCodes.Count > 0)
            {
                foreach (var instrMethod in instructionMethodCodes)
                {
                    courseEntity.AddInstructionalMethodCode(instrMethod);
                }
            }

            // Verify that all the data in the entity is valid
            CourseProcessor.ValidateCourseData(courseEntity, _academicLevels, _courseLevels, null, _creditCategories, _academicDepartments,
                _gradeSchemes, _instructionalMethods, _subjects, _locations, _topicCodes);

            return courseEntity;
        }

        #endregion


        #region V8 Changes

        /// <summary>
        /// GetCourses4Async
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="subject"></param>
        /// <param name="number"></param>
        /// <param name="academicLevel"></param>
        /// <param name="owningInstitutionUnits"></param>
        /// <param name="title"></param>
        /// <param name="instructionalMethods"></param>
        /// <param name="schedulingStartOn"></param>
        /// <param name="schedulingEndOn"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.Course4>, int>> GetCourses4Async(int offset, int limit, bool bypassCache, string subject, string number, List<string> academicLevel,
            List<string> owningInstitutionUnits, string title, List<string> instructionalMethods, string schedulingStartOn, string schedulingEndOn)
        {
            var courseCollection = new List<Dtos.Course4>();
            int totalCount = 0;
            List<string> newAcademicLevel = null, newOwningInstitutionUnit = null, newInstructionalMethods = null;
            var newSubject = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(subject))
                {
                    newSubject = (string.IsNullOrEmpty(subject) ? string.Empty : ConvertGuidToCode(await GetSubjectAsync(bypassCache), subject));
                    if (string.IsNullOrEmpty(newSubject))
                    {
                        return new Tuple<IEnumerable<Course4>, int>(new List<Course4>(), 0);
                    }
                }
                var newStartOn = (string.IsNullOrEmpty(schedulingStartOn) ? string.Empty : await ConvertDateArgument(schedulingStartOn));
                var newEndOn = (string.IsNullOrEmpty(schedulingEndOn) ? string.Empty : await ConvertDateArgument(schedulingEndOn));

                if (academicLevel != null && academicLevel.Any())
                {
                    newAcademicLevel = ConvertGuidToCodeCollection(await GetAcademicLevelsAsync(bypassCache), academicLevel);
                    if ((newAcademicLevel == null) || (!newAcademicLevel.Any()))
                    {
                        return new Tuple<IEnumerable<Course4>, int>(new List<Course4>(), 0);
                    }
                }
                if (owningInstitutionUnits != null && owningInstitutionUnits.Any())
                {
                    newOwningInstitutionUnit = ConvertGuidToCodeCollection(await GetDepartmentsAsync(bypassCache), owningInstitutionUnits);
                    if ((newOwningInstitutionUnit == null) || (!newOwningInstitutionUnit.Any()))
                    {
                        return new Tuple<IEnumerable<Course4>, int>(new List<Course4>(), 0);
                    }
                }
                if (instructionalMethods != null && instructionalMethods.Any())
                {
                    newInstructionalMethods = ConvertGuidToCodeCollection(await GetInstructionalMethodsAsync(bypassCache), instructionalMethods);
                    if ((newInstructionalMethods == null) || (!newInstructionalMethods.Any()))
                    {
                        return new Tuple<IEnumerable<Course4>, int>(new List<Course4>(), 0);
                    }
                }

                var courseEntities = await _courseRepository.GetPagedCoursesAsync(offset, limit, newSubject, number, newAcademicLevel, newOwningInstitutionUnit, title, newInstructionalMethods, newStartOn, newEndOn);
                totalCount = courseEntities.Item2;
                foreach (var course in courseEntities.Item1)
                {
                    courseCollection.Add(await ConvertCourseEntityToDto4Async(course, false));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred retrieving courses: " + ex.Message, ex.InnerException);
            }
            return courseCollection.Any() ? new Tuple<IEnumerable<Course4>, int>(courseCollection, totalCount) :
                    new Tuple<IEnumerable<Course4>, int>(new List<Course4>(), 0);
        }

        /// <summary>
        /// Gets a course by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.Course4</returns>
        public async Task<Dtos.Course4> GetCourseByGuid4Async(string id)
        {
            var course = await _courseRepository.GetCourseByGuidAsync(id);

            var courseDto = await ConvertCourseEntityToDto4Async(course, true);
            return courseDto;
        }

        /// <summary>
        /// Update a course
        /// </summary>
        /// <param name="course"></param>
        /// <returns>Dtos.Course4</returns>
        public async Task<Dtos.Course4> UpdateCourse4Async(Course4 course, bool bypassCache)
        {
            if (course == null)
            {
                throw new ArgumentNullException("course payload is invalid", "Course DTO is required for PUT.");
            }
            // We must have a GUID so we can get the existing data
            if (string.IsNullOrEmpty(course.Id))
            {
                throw new KeyNotFoundException("Course must provide a GUID.");
            }

            // Confirm that user has permissions to update course
            CheckCoursePermission();

            _courseRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            //Convert the DTO to an entity, update the course, convert the resulting entity back to a DTO, and return it
            var courseEntity = await ConvertCourseDtoToEntity4Async(course, bypassCache);
            var source = course.MetadataObject != null ? course.MetadataObject.CreatedBy : null;
            var version = "8";
            var updatedCourseEntity = await _courseRepository.UpdateCourseAsync(courseEntity, source, version);
            return await ConvertCourseEntityToDto4Async(updatedCourseEntity, true);
        }

        /// <summary>
        /// Creates a new course
        /// </summary>
        /// <param name="course"></param>
        /// <returns>Dtos.Course4</returns>
        public async Task<Dtos.Course4> CreateCourse4Async(Course4 course, bool bypassCache)
        {
            // Confirm that user has permissions to create course
            CheckCoursePermission();
            _courseRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            //Convert the DTO to an entity, create the course, convert the resulting entity back to a DTO, and return it
            var courseEntity = await ConvertCourseDtoToEntity4Async(course, bypassCache);
            var source = course.MetadataObject != null ? course.MetadataObject.CreatedBy : null;
            var version = "8";
            var createdCourseEntity = await _courseRepository.CreateCourseAsync(courseEntity, source, version);
            return await ConvertCourseEntityToDto4Async(createdCourseEntity, true);
        }

        /// <summary>
        /// Converts entities to dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Dtos.Course4</returns>
        private async Task<Dtos.Course4> ConvertCourseEntityToDto4Async(Ellucian.Colleague.Domain.Student.Entities.Course source, bool bypassCache)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A course must be supplied.");
            }

            var course = new Ellucian.Colleague.Dtos.Course4();

            course.Id = source.Guid;
            var subjects = await GetSubjectAsync(bypassCache);
            if ((subjects != null) && (subjects.Any()))
            {
                var subject = subjects.FirstOrDefault(s => s.Code == source.SubjectCode);
                if (subject != null)
                {
                    course.Subject = new Dtos.GuidObject2(subject.Guid);
                }
            }
            course.CourseLevels = new List<Dtos.GuidObject2>();
            if (source.CourseLevelCodes != null && source.CourseLevelCodes.Count > 0)
            {
                var courseLevelGuids = new List<Dtos.GuidObject2>();
                var courseLevels = await GetCourseLevelsAsync(bypassCache);
                if ((courseLevels != null) && (courseLevels.Any()))
                {
                    foreach (var courseLevelCode in source.CourseLevelCodes)
                    {
                        var courseLevel = courseLevels.FirstOrDefault(cl => cl.Code == courseLevelCode);
                        if (courseLevel != null)
                        {
                            courseLevelGuids.Add(new Dtos.GuidObject2(courseLevel.Guid));
                        }
                    }
                }
                if (courseLevelGuids.Any())
                {
                    course.CourseLevels = courseLevelGuids;
                }
            }

            course.InstructionMethods = new List<Dtos.GuidObject2>();
            if (source.InstructionalMethodCodes != null && source.InstructionalMethodCodes.Count > 0)
            {
                var instructionMethodGuids = new List<Dtos.GuidObject2>();
                var instructionalMethods = await GetInstructionalMethodsAsync(bypassCache);
                if ((instructionalMethods != null) && (instructionalMethods.Any()))
                {
                    foreach (var instrMethodCode in source.InstructionalMethodCodes)
                    {
                        var instructionalMethod = instructionalMethods.FirstOrDefault(im => im.Code == instrMethodCode);
                        if (instructionalMethod != null)
                        {
                            instructionMethodGuids.Add(new Dtos.GuidObject2(instructionalMethod.Guid));
                        }
                    }
                }
                if (instructionMethodGuids.Any())
                {
                    course.InstructionMethods = instructionMethodGuids;
                }
            }

            if (!(string.IsNullOrEmpty(source.AcademicLevelCode)))
            {
                var academicLevels = await GetAcademicLevelsAsync(bypassCache);
                if ((academicLevels != null) && (academicLevels.Any()))
                {
                    var acadLevel = academicLevels.FirstOrDefault(al => al.Code == source.AcademicLevelCode);
                    if (acadLevel != null)
                        course.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(acadLevel.Guid) };
                }
            }

            if (!string.IsNullOrEmpty(source.GradeSchemeCode))
            {
                var gradeSchemes = await GetGradeSchemesAsync(bypassCache);
                if ((gradeSchemes != null) && (gradeSchemes.Any()))
                {
                    var gradeScheme = gradeSchemes.FirstOrDefault(gs => gs.Code == source.GradeSchemeCode);
                    if (gradeScheme != null)
                    {
                        course.GradeSchemes = new List<Dtos.GuidObject2> { new Dtos.GuidObject2((gradeScheme.Guid)) };
                    }
                }
            }

            course.Title = source.LongTitle;
            course.Description = source.Description;

            // Determine the Department information for the course
            course.OwningInstitutionUnits = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();
            var departments = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();

            if (source.Departments != null && source.Departments.Any())
            {
                var allDepartments = await GetDepartmentsAsync(bypassCache);
                if ((allDepartments != null) && (allDepartments.Any()))
                {
                    foreach (var offeringDept in source.Departments)
                    {
                        var academicDepartment = (allDepartments.FirstOrDefault(d => d.Code == offeringDept.AcademicDepartmentCode));
                        if (academicDepartment != null)
                        {
                            var department = new Ellucian.Colleague.Dtos.OwningInstitutionUnit
                            {
                                InstitutionUnit = { Id = academicDepartment.Guid },
                                OwnershipPercentage = offeringDept.ResponsibilityPercentage
                            };
                            departments.Add(department);
                        }
                    }
                }
                if ((departments != null) && (departments.Any()))
                {
                    course.OwningInstitutionUnits = departments;
                }
            }

            // Use the Start Date (if supplied); otherwise, use the current date (unless end date is present and less than current date, then set start date to the end date value)
            var today = DateTime.Today;
            course.EffectiveStartDate = source.StartDate.HasValue && source.StartDate != default(DateTime) ? source.StartDate.Value :
                (source.EndDate.HasValue && source.EndDate.Value < DateTime.Today ? source.EndDate.Value : new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Unspecified));
            course.EffectiveEndDate = source.EndDate;
            course.Number = source.Number;

            // Determine the Credit information for the course
            course.Credits = new List<Dtos.Credit3>();
            var creditCategories = await GetCreditCategoriesAsync(bypassCache);
            if ((creditCategories != null) && (creditCategories.Any()))
            {
                var creditType = creditCategories.FirstOrDefault(ct => ct.Code == source.LocalCreditType);
                if (creditType != null)
                {
                    var creditCategory = new CreditIdAndTypeProperty2() { Detail = new GuidObject2(creditType.Guid) };

                    switch (creditType.CreditType)
                    {
                        case CreditType.ContinuingEducation:
                            creditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                        case CreditType.Institutional:
                            creditCategory.CreditType = CreditCategoryType3.Institutional;
                            break;
                        case CreditType.Transfer:
                            creditCategory.CreditType = CreditCategoryType3.Transfer;
                            break;
                        case CreditType.Exchange:
                            creditCategory.CreditType = CreditCategoryType3.Exchange;
                            break;
                        case CreditType.Other:
                            creditCategory.CreditType = CreditCategoryType3.Other;
                            break;
                        case CreditType.None:
                            creditCategory.CreditType = CreditCategoryType3.NoCredit;
                            break;
                        default:
                            creditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                    }
                    if (source.Ceus.HasValue)
                    {
                        course.Credits.Add(new Dtos.Credit3()
                        {
                            CreditCategory = creditCategory,
                            Measure = Dtos.CreditMeasure2.CEU,
                            Minimum = source.Ceus,
                        });
                    }
                    if (source.MinimumCredits.HasValue)
                    {
                        course.Credits.Add(new Dtos.Credit3()
                        {
                            CreditCategory = creditCategory,
                            Measure = Dtos.CreditMeasure2.Credit,
                            Minimum = source.MinimumCredits,
                            Maximum = source.MaximumCredits,
                            Increment = source.VariableCreditIncrement
                        });
                    }
                    if (source.BillingCredits != null && source.BillingCredits.HasValue)
                    {
                        course.Billing = new BillingCreditDtoProperty() { Minimum = (decimal)source.BillingCredits };
                    }
                }
            }

            if (course.MetadataObject == null)
                course.MetadataObject = new Dtos.DtoProperties.MetaDataDtoProperty();
            course.MetadataObject.CreatedBy = string.IsNullOrEmpty(source.ExternalSource) ? "Colleague" : source.ExternalSource;

            return course;
        }

        /// <summary>
        /// Converts a Course DTO to its corresponding Course domain entity
        /// </summary>
        /// <param name="source">A <see cref="Course">Course</see> DTO</param>
        /// <returns>A<see cref="Course">Course</see> domain entity</returns>
        private async Task<Domain.Student.Entities.Course> ConvertCourseDtoToEntity4Async(Dtos.Course4 course, bool bypassCache)
        {

            if (course == null)
            {
                throw new ArgumentNullException("course payload is invalid", "A course must be supplied.");
            }
            if (string.IsNullOrEmpty(course.Title))
            {
                throw new ArgumentException("A course title must be provided.");
            }
            if (course.Subject == null || string.IsNullOrEmpty(course.Subject.Id))
            {
                throw new ArgumentException("Subject is required; no Id supplied");
            }
            if (string.IsNullOrEmpty(course.Number))
            {
                throw new ArgumentException("Course number is required.");
            }
            if (course.Number.Length > 7)
            {
                throw new ArgumentException("Course number cannot be longer than 7 characters.");
            }
            if (course.EffectiveStartDate == default(DateTime))
            {
                throw new ArgumentException("Course start date is required.");
            }

            if (course.CourseLevels != null && course.CourseLevels.Any())
            {
                foreach (var level in course.CourseLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException("Course Level id is a required field when Course Levels are in the message body.");
                    }
                }
            }

            if (course.InstructionMethods != null && course.InstructionMethods.Any())
            {
                foreach (var method in course.InstructionMethods)
                {
                    if (string.IsNullOrEmpty(method.Id))
                    {
                        throw new ArgumentException("Instructional Method id is a required field when Instructional Methods are in the message body.");
                    }
                }
            }

            if (course.AcademicLevels != null && course.AcademicLevels.Any())
            {
                foreach (var level in course.AcademicLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException("Academic Level id is a required field when Academic Levels are in the message body.");
                    }
                }
            }

            if (course.GradeSchemes != null && course.GradeSchemes.Any())
            {
                foreach (var scheme in course.GradeSchemes)
                {
                    if (string.IsNullOrEmpty(scheme.Id))
                    {
                        throw new ArgumentException("Grade Scheme id is a required field when Grade Schemes are in the message body.");
                    }
                }
            }

            if (course.OwningInstitutionUnits != null && course.OwningInstitutionUnits.Any())
            {
                foreach (var org in course.OwningInstitutionUnits)
                {
                    if (org.InstitutionUnit == null || string.IsNullOrEmpty(org.InstitutionUnit.Id))
                    {
                        throw new ArgumentException("Institution Unit id is a required field when Owning Organizations are in the message body.");
                    }

                    if (org.OwnershipPercentage == null)
                    {
                        throw new ArgumentException("Ownership Percentage is a required field when Owning Organizations are in the message body.");
                    }
                }
            }

            if (course.Credits != null && course.Credits.Any())
            {
                if (course.Credits.Count() > 2)
                {
                    throw new ArgumentException("A maximum of 2 entries are allowed in the Credits array.");
                }
                if (course.Credits.Count() == 2)
                {
                    if (course.Credits.ElementAt(0).CreditCategory.CreditType != course.Credits.ElementAt(1).CreditCategory.CreditType)
                    {
                        throw new ArgumentException("The same Credit Type must be used for each entry in the Credits array.");
                    }
                    if (!(course.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && course.Credits.ElementAt(1).Measure == CreditMeasure2.Credit)
                        && !(course.Credits.ElementAt(0).Measure == CreditMeasure2.Credit && course.Credits.ElementAt(1).Measure == CreditMeasure2.CEU)
                        && !(course.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && course.Credits.ElementAt(1).Measure == CreditMeasure2.Hours)
                        && !(course.Credits.ElementAt(0).Measure == CreditMeasure2.Hours && course.Credits.ElementAt(1).Measure == CreditMeasure2.CEU))
                    {
                        throw new ArgumentException("Invalid combination of measures '" + course.Credits.ElementAt(0).Measure
                            + "' and '" + course.Credits.ElementAt(1).Measure + "'");
                    }
                }
                foreach (var credit in course.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException("Credit Category is required if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required if for Credit Categories if Credits are in the message body.");
                    }

                    if (credit.CreditCategory.Detail != null && string.IsNullOrEmpty(credit.CreditCategory.Detail.Id))
                    {
                        throw new ArgumentException("Credit Category id is required within the detail section of Credit Category if it is in the message body.");
                    }
                    if (credit.Increment == null && credit.Maximum != null)
                    {
                        throw new ArgumentException("Credit Increment is required when Credit Maximum exists.");
                    }
                    if (credit.Maximum == null && credit.Increment != null)
                    {
                        throw new ArgumentException("Credit Maximum is required when Credit Increment exists.");
                    }
                    if (credit.Maximum != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Maximum cannot exist when Credit Measure is 'ceu'.");
                    }
                    if (credit.Increment != null && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Increment cannot exist when Credit Measure is 'ceu'.");
                    }
                    if (course.Billing != null && course.Billing.Minimum == null)
                    {
                        throw new ArgumentException("Billing credits minimum is required for billing credits.");
                    }
                    if (course.Billing != null && course.Billing.Minimum < 0)
                    {
                        throw new ArgumentException("Billing credits minimum cannot be less than zero.");
                    }
                    if (course.Billing != null && course.Billing.Maximum != null && course.Billing.Minimum != course.Billing.Maximum)
                    {
                        throw new ArgumentException("Billing minimum and maximum credits must be equal.");
                    }
                    if (course.Billing != null && course.Billing.Increment != null && course.Billing.Increment != 0)
                    {
                        throw new ArgumentException("Billing credits increment must be zero.");
                    }

                }
            }
            await ReadCourseCodesAsync(bypassCache);

            var courseConfig = await _studentConfigRepository.GetCurriculumConfigurationAsync();

            VerifyCurriculumConfiguration(courseConfig);

            // Set the subject based on the supplied GUID
            var subjectCode = ConvertGuidToCode(_subjects, course.Subject.Id);

            if (subjectCode == null)
            {
                throw new ArgumentException(string.Concat("Subject for id '", course.Subject.Id.ToString(), "' was not found. Valid Subject is required."));
            }

            // Set the list of departments/shares
            List<OfferingDepartment> offeringDepartments = new List<OfferingDepartment>();

            // If we have supplied owning organization, then use that first.
            if (course.OwningInstitutionUnits != null && course.OwningInstitutionUnits.Any())
            {
                foreach (var owningInstitutionUnit in course.OwningInstitutionUnits)
                {
                    var division = (await _referenceDataRepository.GetDivisionsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (division != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'division' is not supported.");
                    }

                    var school = (await _referenceDataRepository.GetSchoolsAsync(true))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (school != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'school' is not supported.");
                    }

                    var department = _departments.Where(d => d.Guid == owningInstitutionUnit.InstitutionUnit.Id).FirstOrDefault();
                    var academicDepartment = department != null ? _academicDepartments.FirstOrDefault(ad => ad.Code == department.Code) : null;
                    if (academicDepartment != null)
                    {
                        offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, owningInstitutionUnit.OwnershipPercentage));
                    }
                }
            }

            // If we don't have owning organizations then try using the subject-department mapping 
            // to determine department based on subject
            if (offeringDepartments.Count() == 0 && courseConfig.SubjectDepartmentMapping.Items != null && courseConfig.SubjectDepartmentMapping.Items.Count > 0)
            {
                var deptMapping = courseConfig.SubjectDepartmentMapping.Items.Where(i => i.OriginalCode == subjectCode).FirstOrDefault();
                var deptCode = deptMapping != null ? deptMapping.NewCode : null;
                var department = deptCode != null ? _departments.Where(d => d.Code == deptCode).FirstOrDefault() : null;
                var academicDepartment = department != null ? _academicDepartments.Where(ad => ad.Code == department.Code).FirstOrDefault() : null;

                if (academicDepartment != null)
                {
                    offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, 100m));
                }
            }

            // If we still don't have a valid department then check the subject code.
            // If the subject code is also a valid academic department then use that.
            if (offeringDepartments.Count == 0 && !string.IsNullOrEmpty(subjectCode))
            {
                var department = subjectCode != null ? _departments.Where(d => d.Code == subjectCode).FirstOrDefault() : null;
                var academicDepartment = department != null ? _academicDepartments.Where(ad => ad.Code == department.Code).FirstOrDefault() : null;
                if (academicDepartment != null)
                {
                    offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, 100m));
                }
            }

            // Department could not be determined from supplied subject or owning organization
            if (offeringDepartments.Count == 0)
            {
                throw new ArgumentException("Department could not be determined for subject " + subjectCode);
            }

            // Set the academic level code based on the supplied GUID or the default if one is not supplied
            string acadLevelCode = null;
            if (course.AcademicLevels != null && course.AcademicLevels.Any())
            {
                foreach (var acadLevel in course.AcademicLevels)
                {
                    try
                    {
                        acadLevelCode = _academicLevels.Where(al => al.Guid == acadLevel.Id).First().Code;
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + acadLevel.Id + "' supplied for academicLevels");
                    }
                }
            }
            else
            {
                acadLevelCode = courseConfig.DefaultAcademicLevelCode;
            }

            // Set the list of course level codes based on the supplied GUIDs or the default if one is not supplied
            List<string> courseLevelCodes = new List<string>();
            if (course.CourseLevels != null && course.CourseLevels.Any())
            {
                foreach (var courseLevel in course.CourseLevels)
                {
                    try
                    {
                        courseLevelCodes.Add(_courseLevels.First(cl => cl.Guid == courseLevel.Id).Code);
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + courseLevel.Id + "' supplied for courseLevels");
                    }
                }
            }
            else
            {
                courseLevelCodes.Add(courseConfig.DefaultCourseLevelCode);
            }

            // Set the list of instruction method codes based on the supplied GUIDs
            List<string> instructionMethodCodes = new List<string>();
            if (course.InstructionMethods != null && course.InstructionMethods.Any())
            {
                foreach (var instrMethod in course.InstructionMethods)
                {
                    try
                    {
                        instructionMethodCodes.Add(_instructionalMethods.First(im => im.Guid == instrMethod.Id).Code);
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + instrMethod.Id + "' supplied for instructionalMethods");
                    }
                }
            }
            // If instructional method GUIDs are not supplied, use the ERP default
            else
            {
                instructionMethodCodes.Add(courseConfig.DefaultInstructionalMethodCode);
            }

            // Set the list of course approvals based on the supplied GUIDs
            List<CourseApproval> courseApprovals = new List<CourseApproval>();
            var approvingAgencyId = courseConfig.ApprovingAgencyId;
            var approverId = courseConfig.ApproverId;
            string statusCode;
            var status = CourseStatus.Unknown;
            var today = DateTime.Today;
            if (course.EffectiveEndDate == null || course.EffectiveEndDate >= today)
            {
                statusCode = courseConfig.CourseActiveStatusCode;
                status = CourseStatus.Active;
            }
            else
            {
                statusCode = courseConfig.CourseInactiveStatusCode;
                status = CourseStatus.Terminated;
            }

            courseApprovals.Add(new CourseApproval(statusCode, DateTime.Today, approvingAgencyId, approverId, DateTime.Today)
            {
                Status = status
            });

            // Set the list of grade scheme codes based on the supplied GUIDs
            string gradeSchemeCode = null;
            if (course.GradeSchemes != null && course.GradeSchemes.Any() && !string.IsNullOrEmpty(course.GradeSchemes.ToList()[0].Id))
            {
                try
                {
                    gradeSchemeCode = _gradeSchemes.First(gs => gs.Guid == course.GradeSchemes.ToList()[0].Id).Code;
                }
                catch
                {
                    throw new ArgumentException("Invalid Id '" + course.GradeSchemes.ToList()[0].Id + "' supplied for gradeSchemes");
                }
            }

            // Set the credit type and credits/CEUs for the course based on the supplied GUID, or using the ERP default 
            CreditCategory creditCategory = null;
            if (course.Credits != null &&
                course.Credits.Any() &&
                course.Credits.ToList()[0].CreditCategory != null &&
                course.Credits.ToList()[0].CreditCategory.Detail != null &&
                !string.IsNullOrEmpty(course.Credits.ToList()[0].CreditCategory.Detail.Id))
            {
                creditCategory = _creditCategories.FirstOrDefault(ct => ct.Guid == course.Credits.ToList()[0].CreditCategory.Detail.Id);
                if (creditCategory == null)
                {
                    throw new ArgumentException("Invalid Id '" + course.Credits.ToList()[0].CreditCategory.Detail.Id + "' supplied for creditCategory");
                }
            }
            // If we don't have a GUID then check for a CreditType enumeration value
            if (creditCategory == null &&
                course.Credits != null &&
                course.Credits.Any() &&
                course.Credits.ToList()[0].CreditCategory != null &&
                course.Credits.ToList()[0].CreditCategory.CreditType != null)
            {
                if (course.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.Exam ||
                    course.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.WorkLifeExperience)
                {
                    throw new InvalidOperationException("Credit category type 'exam' or 'workLifeExperience' are not supported.");
                }

                // Find the credit category that matches the enumeration
                switch (course.Credits.ToList()[0].CreditCategory.CreditType)
                {
                    case (CreditCategoryType3.ContinuingEducation):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.ContinuingEducation);
                        break;
                    case (CreditCategoryType3.Institutional):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Institutional);
                        break;
                    case (CreditCategoryType3.Transfer):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Transfer);
                        break;
                    case (CreditCategoryType3.Exchange):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Exchange);
                        break;
                    case (CreditCategoryType3.Other):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Other);
                        break;
                    case (CreditCategoryType3.NoCredit):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.None);
                        break;
                }
            }
            if (creditCategory == null)
            {
                // Get default from course configuration
                creditCategory = _creditCategories.FirstOrDefault(ct => ct.Code == courseConfig.DefaultCreditTypeCode);
            }
            //If creditCategory is null then throw an exception
            if (creditCategory == null)
            {
                throw new ArgumentException(string.Format("Credit category {0} was not found", courseConfig.DefaultCreditTypeCode));
            }


            var creditTypeCode = creditCategory == null ? string.Empty : creditCategory.Code;

            decimal? minCredits;
            decimal? ceus;
            decimal? maxCredits;
            decimal? varIncrCredits;
            minCredits = null;
            maxCredits = null;
            varIncrCredits = null;
            ceus = null;
            foreach (var credits in course.Credits)
            {
                var creditInfo = (course.Credits == null || course.Credits.Count == 0) ? null : credits;
                var measure = creditInfo == null ? null : creditInfo.Measure;
                if (measure == Dtos.CreditMeasure2.CEU)
                {
                    ceus = creditInfo == null ? 0 : creditInfo.Minimum;
                }
                else
                {
                    minCredits = creditInfo == null ? 0 : creditInfo.Minimum;
                    maxCredits = creditInfo == null ? null : creditInfo.Maximum;
                    varIncrCredits = creditInfo == null ? null : creditInfo.Increment;
                }
            }
            var courseDelimeter = !string.IsNullOrEmpty(courseConfig.CourseDelimiter) ? courseConfig.CourseDelimiter : CourseDelimiter;

            //Handle GUID requiredness
            string guid = (course.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)) ? string.Empty : course.Id;

            // Build the course entity
            var courseEntity = new Ellucian.Colleague.Domain.Student.Entities.Course(null, course.Title, course.Title, offeringDepartments,
                subjectCode, course.Number, acadLevelCode, courseLevelCodes, minCredits, ceus, courseApprovals)
            {
                LocalCreditType = creditTypeCode,
                Description = course.Description,
                Guid = guid,
                GradeSchemeCode = gradeSchemeCode,
                StartDate = course.EffectiveStartDate,
                EndDate = course.EffectiveEndDate,
                Name = subjectCode + courseDelimeter + course.Number,
                MaximumCredits = maxCredits,
                VariableCreditIncrement = varIncrCredits,
                AllowPassNoPass = courseConfig.AllowPassNoPass,
                AllowAudit = courseConfig.AllowAudit,
                OnlyPassNoPass = courseConfig.OnlyPassNoPass,
                AllowWaitlist = courseConfig.AllowWaitlist,
                IsInstructorConsentRequired = courseConfig.IsInstructorConsentRequired,
                WaitlistRatingCode = courseConfig.WaitlistRatingCode
            };

            // Add any supplied instruction method codes to the entity
            if (instructionMethodCodes != null && instructionMethodCodes.Count > 0)
            {
                foreach (var instrMethod in instructionMethodCodes)
                {
                    courseEntity.AddInstructionalMethodCode(instrMethod);
                }
            }

            // Add billing credits if they exist
            if (course.Billing != null)
            {
                courseEntity.BillingCredits = course.Billing.Minimum;
            }

            // Verify that all the data in the entity is valid
            CourseProcessor.ValidateCourseData(courseEntity, _academicLevels, _courseLevels, null, _creditCategories, _academicDepartments,
                _gradeSchemes, _instructionalMethods, _subjects, _locations, _topicCodes);

            return courseEntity;
        }

        #endregion


        #region V16 Changes

        /// <summary>
        /// GetCourses5Async
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <param name="subject"></param>
        /// <param name="number"></param>
        /// <param name="academicLevel"></param>
        /// <param name="owningInstitutionUnits"></param>
        /// <param name="title"></param>
        /// <param name="instructionalMethods"></param>
        /// <param name="schedulingStartOn"></param>
        /// <param name="schedulingEndOn"></param>
        /// <param name="activeOn"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.Course5>, int>> GetCourses5Async(int offset, int limit, bool bypassCache, string subject, string number, List<string> academicLevel,
            List<string> owningInstitutionUnits, List<string> titles, List<string> instructionalMethods, string schedulingStartOn, string schedulingEndOn, string topic, List<string> categories, string activeOn)
        {
            var courseCollection = new List<Dtos.Course5>();
            int totalCount = 0;
            List<string> newAcademicLevel = null, newOwningInstitutionUnit = null, newInstructionalMethods = null, newCategories = null;
            string newSubject = string.Empty, newTopic = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(subject))
                {
                    newSubject = (string.IsNullOrEmpty(subject) ? string.Empty : ConvertGuidToCode(await GetSubjectAsync(bypassCache), subject));
                    if (string.IsNullOrEmpty(newSubject))
                    {
                        return new Tuple<IEnumerable<Course5>, int>(new List<Course5>(), 0);
                    }
                }
                var newStartOn = (string.IsNullOrEmpty(schedulingStartOn) ? string.Empty : await ConvertDateArgument(schedulingStartOn));
                var newEndOn = (string.IsNullOrEmpty(schedulingEndOn) ? string.Empty : await ConvertDateArgument(schedulingEndOn));
                var newActiveOn = (string.IsNullOrEmpty(activeOn) ? string.Empty : await ConvertDateArgument(activeOn));

                if (academicLevel != null && academicLevel.Any())
                {
                    newAcademicLevel = ConvertGuidToCodeCollection(await GetAcademicLevelsAsync(bypassCache), academicLevel);
                    if ((newAcademicLevel == null) || (!newAcademicLevel.Any()))
                    {
                        return new Tuple<IEnumerable<Course5>, int>(new List<Course5>(), 0);
                    }
                }
                if (owningInstitutionUnits != null && owningInstitutionUnits.Any())
                {
                    newOwningInstitutionUnit = ConvertGuidToCodeCollection(await GetDepartmentsAsync(bypassCache), owningInstitutionUnits);
                    if ((newOwningInstitutionUnit == null) || (!newOwningInstitutionUnit.Any()))
                    {
                        return new Tuple<IEnumerable<Course5>, int>(new List<Course5>(), 0);
                    }
                }
                if (instructionalMethods != null && instructionalMethods.Any())
                {
                    newInstructionalMethods = ConvertGuidToCodeCollection(await GetInstructionalMethodsAsync(bypassCache), instructionalMethods);
                    if ((newInstructionalMethods == null) || (!newInstructionalMethods.Any()))
                    {
                        return new Tuple<IEnumerable<Course5>, int>(new List<Course5>(), 0);
                    }
                }
                if (categories != null && categories.Any())
                {
                    newCategories = ConvertGuidToCodeCollection(await _studentReferenceDataRepository.GetCourseTypesAsync(bypassCache), categories);
                    if ((newCategories == null) || (!newCategories.Any()))
                    {
                        return new Tuple<IEnumerable<Course5>, int>(new List<Course5>(), 0);
                    }
                }
                if (!string.IsNullOrEmpty(topic))
                {
                    newTopic = (string.IsNullOrEmpty(topic) ? string.Empty : ConvertGuidToCode(await _studentReferenceDataRepository.GetTopicCodesAsync(bypassCache), topic));
                    if (string.IsNullOrEmpty(newTopic))
                    {
                        return new Tuple<IEnumerable<Course5>, int>(new List<Course5>(), 0);
                    }
                }

                var courseEntities = await _courseRepository.GetPagedCoursesAsync(offset, limit, newSubject, number, newAcademicLevel, newOwningInstitutionUnit, titles, newInstructionalMethods, newStartOn, newEndOn, newTopic, newCategories, newActiveOn);
                totalCount = courseEntities.Item2;
                foreach (var course in courseEntities.Item1)
                {
                    courseCollection.Add(await ConvertCourseEntityToDto5Async(course, false));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred retrieving courses: " + ex.Message, ex.InnerException);
            }
            return courseCollection.Any() ? new Tuple<IEnumerable<Course5>, int>(courseCollection, totalCount) :
                    new Tuple<IEnumerable<Course5>, int>(new List<Course5>(), 0);
        }

        /// <summary>
        /// Gets a course by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dtos.Course4</returns>
        public async Task<Dtos.Course5> GetCourseByGuid5Async(string id)
        {
            var course = await _courseRepository.GetCourseByGuidAsync(id);

            var courseDto = await ConvertCourseEntityToDto5Async(course, true);
            return courseDto;
        }

        /// <summary>
        /// Update a course
        /// </summary>
        /// <param name="course"></param>
        /// <returns>Dtos.Course4</returns>
        public async Task<Dtos.Course5> UpdateCourse5Async(Course5 course, bool bypassCache)
        {
            if (course == null)
            {
                throw new ArgumentNullException("course payload is invalid", "Course DTO is required for PUT.");
            }
            // We must have a GUID so we can get the existing data
            if (string.IsNullOrEmpty(course.Id))
            {
                throw new KeyNotFoundException("Course must provide a GUID.");
            }

            // Confirm that user has permissions to update course
            CheckCoursePermission();

            //Convert the DTO to an entity, update the course, convert the resulting entity back to a DTO, and return it
            var courseEntity = await ConvertCourseDtoToEntity5Async(course, bypassCache);
            var source = course.MetadataObject != null ? course.MetadataObject.CreatedBy : null;
            var version = "16";
            var updatedCourseEntity = await _courseRepository.UpdateCourseAsync(courseEntity, source, version);
            return await ConvertCourseEntityToDto5Async(updatedCourseEntity, true);
        }

        /// <summary>
        /// Creates a new course
        /// </summary>
        /// <param name="course"></param>
        /// <returns>Dtos.Course4</returns>
        public async Task<Dtos.Course5> CreateCourse5Async(Course5 course, bool bypassCache)
        {
            // Confirm that user has permissions to create course
            CheckCoursePermission();

            //Convert the DTO to an entity, create the course, convert the resulting entity back to a DTO, and return it
            var courseEntity = await ConvertCourseDtoToEntity5Async(course, bypassCache);
            var source = course.MetadataObject != null ? course.MetadataObject.CreatedBy : null;
            var version = "16";
            var createdCourseEntity = await _courseRepository.CreateCourseAsync(courseEntity, source, version);
            return await ConvertCourseEntityToDto5Async(createdCourseEntity, true);
        }

        /// <summary>
        /// Converts entities to dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Dtos.Course5</returns>
        private async Task<Dtos.Course5> ConvertCourseEntityToDto5Async(Ellucian.Colleague.Domain.Student.Entities.Course source, bool bypassCache)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A course must be supplied.");
            }

            if (_contactMeasures == null)
            {
                _contactMeasures = (await _studentReferenceDataRepository.GetContactMeasuresAsync(bypassCache)).ToList();
            }
            if (_courseCategories == null)
            {
                _courseCategories = (await _studentReferenceDataRepository.GetCourseTypesAsync(bypassCache)).ToList();
            }
            if (_topicCodes == null)
            {
                _topicCodes = (await _studentReferenceDataRepository.GetTopicCodesAsync(bypassCache)).ToList();
            }
            if (_courseLevels == null)
            {
                _courseLevels = (await _studentReferenceDataRepository.GetCourseLevelsAsync(bypassCache)).ToList();
            }
            if (_courseStatuses == null)
            {
                _courseStatuses = (await _studentReferenceDataRepository.GetCourseStatusesAsync(bypassCache)).ToList();
            }
            if (_courseTitleTypes == null)
            {
                _courseTitleTypes = (await _studentReferenceDataRepository.GetCourseTitleTypesAsync(bypassCache)).ToList();
            }

            var course = new Ellucian.Colleague.Dtos.Course5();

            course.Id = source.Guid;
            var subjects = await GetSubjectAsync(bypassCache);
            if ((subjects != null) && (subjects.Any()))
            {
                var subject = subjects.FirstOrDefault(s => s.Code == source.SubjectCode);
                if (subject != null)
                {
                    course.Subject = new Dtos.GuidObject2(subject.Guid);
                }
            }
            course.CourseLevels = new List<Dtos.GuidObject2>();
            if (source.CourseLevelCodes != null && source.CourseLevelCodes.Count > 0)
            {
                var courseLevelGuids = new List<Dtos.GuidObject2>();
                foreach (var courseLevelCode in source.CourseLevelCodes)
                {
                    var courseLevel = _courseLevels.FirstOrDefault(cl => cl.Code == courseLevelCode);
                    if (courseLevel != null)
                    {
                        courseLevelGuids.Add(new Dtos.GuidObject2(courseLevel.Guid));
                    }
                }
                if (courseLevelGuids.Any())
                {
                    course.CourseLevels = courseLevelGuids;
                }
            }

            // New object InstructionalMethodDetails with v16

            course.InstructionalMethodDetails = new List<InstructionalMethodDetail>();
            if (source.InstructionalMethodCodes != null && source.InstructionalMethodCodes.Any())
            {
                var instructionalMethods = await GetInstructionalMethodsAsync(bypassCache);
                var administrativeInstructionalMethods = await GetAdministrativeInstructionalMethodsAsync(bypassCache);
                if ((instructionalMethods != null) && (instructionalMethods.Any()))
                {
                    foreach (var instrMethodCode in source.InstructionalMethodCodes)
                    {
                        var position = source.InstructionalMethodCodes.IndexOf(instrMethodCode);
                        var instructionalMethod = instructionalMethods.FirstOrDefault(im => im.Code == instrMethodCode);
                        if (instructionalMethod == null)
                        {
                            throw new ArgumentException("Instructional method '" + instrMethodCode + "' GUID could not be found. ", "instructionalMethod");
                        }
                        ContactHoursPeriod instrMethodPeriod = ContactHoursPeriod.NotSet;
                        decimal? instrMethodHours = null;

                        if (instructionalMethod != null && !string.IsNullOrEmpty(instructionalMethod.Guid))
                        {
                            var instrMethodDetail = new InstructionalMethodDetail()
                            {
                                InstructionalMethod = new GuidObject2(instructionalMethod.Guid)
                            };
                            course.InstructionalMethodDetails.Add(instrMethodDetail);

                            if (source.InstructionalMethodContactPeriods != null && source.InstructionalMethodContactPeriods.Count == source.InstructionalMethodCodes.Count)
                            {
                                if (!string.IsNullOrEmpty(source.InstructionalMethodContactPeriods[position]))
                                {
                                    instrMethodPeriod = ConvertContactHoursPeriodEntityToDto(_contactMeasures.First(cm => cm.Code == source.InstructionalMethodContactPeriods[position]).ContactPeriod);
                                }
                            }
                            if (source.InstructionalMethodContactHours != null && source.InstructionalMethodContactHours.Count == source.InstructionalMethodCodes.Count)
                            {
                                instrMethodHours = source.InstructionalMethodContactHours[position];
                            }
                            var administrativeInstructionalMethod = administrativeInstructionalMethods.FirstOrDefault(im => im.Code == instrMethodCode);
                            if (administrativeInstructionalMethod == null)
                            {
                                throw new ArgumentException("Administrative Instructional method '" + instrMethodCode + "' GUID could not be found. ", "instructionalMethod");
                            }
                            if (administrativeInstructionalMethod != null && !string.IsNullOrEmpty(administrativeInstructionalMethod.Guid) && instrMethodHours != null)
                            {
                                var instrMethodHoursDetail = new CoursesHoursDtoProperty()
                                {
                                    AdministrativeInstructionalMethod = new GuidObject2(administrativeInstructionalMethod.Guid)
                                };

                                if (instrMethodHours != null)
                                {
                                    instrMethodHoursDetail.Minimum = instrMethodHours;
                                }
                                if (instrMethodPeriod != ContactHoursPeriod.NotSet)
                                {
                                    instrMethodHoursDetail.Interval = instrMethodPeriod;
                                }
                                if (course.Hours == null)
                                {
                                    course.Hours = new List<CoursesHoursDtoProperty>();
                                }
                                course.Hours.Add(instrMethodHoursDetail);
                            }
                        }
                    }
                }
            }


            if (!(string.IsNullOrEmpty(source.AcademicLevelCode)))
            {
                var academicLevels = await GetAcademicLevelsAsync(bypassCache);
                if ((academicLevels != null) && (academicLevels.Any()))
                {
                    var acadLevel = academicLevels.FirstOrDefault(al => al.Code == source.AcademicLevelCode);
                    if (acadLevel != null && !string.IsNullOrEmpty(acadLevel.Guid))
                        course.AcademicLevels = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2(acadLevel.Guid) };
                }
            }

            if (!string.IsNullOrEmpty(source.GradeSchemeCode))
            {
                var gradeSchemes = await GetGradeSchemesAsync(bypassCache);
                if ((gradeSchemes != null) && (gradeSchemes.Any()))
                {
                    var gradeScheme = gradeSchemes.FirstOrDefault(gs => gs.Code == source.GradeSchemeCode);
                    if (gradeScheme != null && !string.IsNullOrEmpty(gradeScheme.Guid))
                    {
                        course.GradeSchemes = new List<GradeSchemesDtoProperty>()
                        {
                            new GradeSchemesDtoProperty()
                            {
                             GradeScheme = new Dtos.GuidObject2((gradeScheme.Guid)),
                             Usage = CoursesUsage.Default
                            }
                        };
                    }
                }
            }

            course.Titles = new List<CoursesTitlesDtoProperty>();
            var shortEntity = _courseTitleTypes.FirstOrDefault(ctt => ctt.Code.ToLower() == "short");
            if (shortEntity != null && !string.IsNullOrEmpty(shortEntity.Guid))
            {
                var shortTitle = new CoursesTitlesDtoProperty() { Type = new GuidObject2(shortEntity.Guid), Value = source.Title };
                course.Titles.Add(shortTitle);
            }
            else
            {
                throw new ArgumentNullException("titles.type", "The record 'SHORT' or it's GUID is missing from course title types. ");
            }
            var longEntity = _courseTitleTypes.FirstOrDefault(ctt => ctt.Code.ToLower() == "long");
            if (longEntity != null && !string.IsNullOrEmpty(longEntity.Guid))
            {
                var longTitle = new CoursesTitlesDtoProperty() { Type = new GuidObject2(longEntity.Guid), Value = source.LongTitle };
                course.Titles.Add(longTitle);
            }
            else
            {
                throw new ArgumentNullException("titles.type", "The record 'LONG' or it's GUID is missing from course title types. ");
            }

            if (!string.IsNullOrWhiteSpace(source.Description))
                course.Description = source.Description;

            // Determine the Department information for the course
            course.OwningInstitutionUnits = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();
            var departments = new List<Ellucian.Colleague.Dtos.OwningInstitutionUnit>();

            if (source.Departments != null && source.Departments.Any())
            {
                var allDepartments = await GetDepartmentsAsync(bypassCache);
                if ((allDepartments != null) && (allDepartments.Any()))
                {
                    foreach (var offeringDept in source.Departments)
                    {
                        var academicDepartment = (allDepartments.FirstOrDefault(d => d.Code == offeringDept.AcademicDepartmentCode));
                        if (academicDepartment != null)
                        {
                            var department = new Ellucian.Colleague.Dtos.OwningInstitutionUnit
                            {
                                InstitutionUnit = { Id = academicDepartment.Guid },
                                OwnershipPercentage = offeringDept.ResponsibilityPercentage
                            };
                            departments.Add(department);
                        }
                    }
                }
                if ((departments != null) && (departments.Any()))
                {
                    course.OwningInstitutionUnits = departments;
                }
            }

            course.EffectiveStartDate = source.StartDate;
            course.EffectiveEndDate = source.EndDate;
            course.Number = source.Number;

            // Determine the Credit information for the course
            course.Credits = new List<Dtos.Credit3>();
            var creditCategories = await GetCreditCategoriesAsync(bypassCache);
            if ((creditCategories != null) && (creditCategories.Any()))
            {
                var creditType = creditCategories.FirstOrDefault(ct => ct.Code == source.LocalCreditType);
                if (creditType != null)
                {
                    var creditCategory = new CreditIdAndTypeProperty2() { Detail = new GuidObject2(creditType.Guid) };

                    switch (creditType.CreditType)
                    {
                        case CreditType.ContinuingEducation:
                            creditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                        case CreditType.Institutional:
                            creditCategory.CreditType = CreditCategoryType3.Institutional;
                            break;
                        case CreditType.Transfer:
                            creditCategory.CreditType = CreditCategoryType3.Transfer;
                            break;
                        case CreditType.Exchange:
                            creditCategory.CreditType = CreditCategoryType3.Exchange;
                            break;
                        case CreditType.Other:
                            creditCategory.CreditType = CreditCategoryType3.Other;
                            break;
                        case CreditType.None:
                            creditCategory.CreditType = CreditCategoryType3.NoCredit;
                            break;
                        default:
                            creditCategory.CreditType = CreditCategoryType3.ContinuingEducation;
                            break;
                    }
                    if (source.Ceus.HasValue)
                    {
                        course.Credits.Add(new Dtos.Credit3()
                        {
                            CreditCategory = creditCategory,
                            Measure = Dtos.CreditMeasure2.CEU,
                            Minimum = source.Ceus,
                        });
                    }
                    if (source.MinimumCredits.HasValue)
                    {
                        course.Credits.Add(new Dtos.Credit3()
                        {
                            CreditCategory = creditCategory,
                            Measure = Dtos.CreditMeasure2.Credit,
                            Minimum = source.MinimumCredits,
                            Maximum = source.MaximumCredits,
                            Increment = source.VariableCreditIncrement
                        });
                    }
                    if (source.BillingCredits != null)
                    {
                        course.Billing = new BillingCreditDtoProperty() { Minimum = (decimal)source.BillingCredits };
                    }
                }
            }

            if (source.AllowWaitlistMultipleSections != null)
            {
                course.WaitlistMultipleSections = (bool)source.AllowWaitlistMultipleSections ? WaitlistMultiSectionFlag.Allowed : WaitlistMultiSectionFlag.NotAllowed;
            }
            if (!string.IsNullOrEmpty(source.TopicCode))
            {
                var topicEntity = _topicCodes.FirstOrDefault(tc => tc.Code == source.TopicCode);
                if (topicEntity != null && !string.IsNullOrEmpty(topicEntity.Guid))
                {
                    course.Topic = new GuidObject2(topicEntity.Guid);
                }
            }
            if (source.CourseTypeCodes != null && source.CourseTypeCodes.Any())
            {
                foreach (var cat in source.CourseTypeCodes)
                {
                    var categoryEntity = _courseCategories.FirstOrDefault(cc => cc.Code == cat);
                    if (categoryEntity != null && !string.IsNullOrEmpty(categoryEntity.Guid))
                    {
                        if (course.Categories == null)
                        {
                            course.Categories = new List<GuidObject2>();
                        }
                        course.Categories.Add(new GuidObject2(categoryEntity.Guid));
                    }
                }
            }
            if (source.CourseApprovals != null && source.CourseApprovals.Any())
            {
                var approval = source.CourseApprovals.ElementAtOrDefault(0);
                if (approval != null)
                {
                    var courseStatusEntity = _courseStatuses.FirstOrDefault(cs => cs.Code == approval.StatusCode);
                    if (courseStatusEntity != null)
                    {
                        course.Status = new GuidObject2(courseStatusEntity.Guid);
                    }
                }
            }

            if (course.MetadataObject == null)
                course.MetadataObject = new Dtos.DtoProperties.MetaDataDtoProperty();
            course.MetadataObject.CreatedBy = string.IsNullOrEmpty(source.ExternalSource) ? "Colleague" : source.ExternalSource;

            return course;
        }

        /// <summary>
        /// Converts a Course DTO to its corresponding Course domain entity
        /// </summary>
        /// <param name="source">A <see cref="Dtos.Course5">Course</see> DTO</param>
        /// <returns>A<see cref="Course">Course</see> domain entity</returns>
        private async Task<Domain.Student.Entities.Course> ConvertCourseDtoToEntity5Async(Dtos.Course5 course, bool bypassCache)
        {

            if (course == null)
            {
                throw new ArgumentNullException("The request body for course is invalid or missing. ", "courses");
            }
            if (course.Titles == null || !course.Titles.Any())
            {
                throw new ArgumentException("At least one course title must be provided. ", "titles.value");
            }
            foreach (var title in course.Titles)
            {
                if (string.IsNullOrEmpty(title.Value))
                {
                    throw new ArgumentException("The title value must be provided for the title object. ", "titles.value");
                }
            }
            if (course.Status == null || string.IsNullOrEmpty(course.Status.Id))
            {
                throw new ArgumentException("Status is required; no Id supplied. ", "status.id");
            }
            if (course.Subject == null || string.IsNullOrEmpty(course.Subject.Id))
            {
                throw new ArgumentException("Subject is required; no Id supplied. ", "subject.id");
            }
            if (string.IsNullOrEmpty(course.Number))
            {
                throw new ArgumentException("Course number is required. ", "number");
            }
            if (course.Number.Length > 7)
            {
                throw new ArgumentException("Course number cannot be longer than 7 characters. ", "number");
            }
            if (course.EffectiveStartDate != null && course.EffectiveEndDate != null)
            {
                if (course.EffectiveEndDate < course.EffectiveStartDate)
                {
                    throw new ArgumentException("schedulingEndOn cannot be earlier than schedulingStartOn. ", "schedulingStartOn");
                }
            }

            if (course.CourseLevels != null && course.CourseLevels.Any())
            {                
                foreach (var level in course.CourseLevels)
                {
                    if (string.IsNullOrEmpty(level.Id))
                    {
                        throw new ArgumentException("Course Level id is a required field when Course Levels are defined. ", "courseLevels.id");
                    }
                }
            }

            if (course.GradeSchemes != null && course.GradeSchemes.Any())
            {
                foreach (var scheme in course.GradeSchemes)
                {
                    if (scheme.GradeScheme == null || string.IsNullOrEmpty(scheme.GradeScheme.Id))
                    {
                        throw new ArgumentException("Grade Scheme id is a required field when Grade Schemes are defined. ", "gradeSchemes.gradeScheme.id");
                    }
                }
            }

            if (course.OwningInstitutionUnits != null && course.OwningInstitutionUnits.Any())
            {
                foreach (var org in course.OwningInstitutionUnits)
                {
                    if (org.InstitutionUnit == null || string.IsNullOrEmpty(org.InstitutionUnit.Id))
                    {
                        throw new ArgumentException("Institution Unit id is a required field when Owning Organizations are defined. ", "owningInstitutionUnits.institutionUnit.id");
                    }

                    if (org.OwnershipPercentage == 0)
                    {
                        throw new ArgumentException("Ownership Percentage is a required field when Owning Organizations are defined. ", "owningInstitutionUnits.ownershipPercentage");
                    }
                }
            }

            if (course.Credits != null && course.Credits.Any())
            {
                if (course.Credits.Count() > 2)
                {
                    throw new ArgumentException("A maximum of 2 entries are allowed in the Credits array. ", "course.credits");
                }
                if (course.Credits.Count() == 2)
                {
                    if (course.Credits.Any() && course.Credits.ElementAt(0).CreditCategory != null && course.Credits.ElementAt(1).CreditCategory != null && 
                        course.Credits.ElementAt(0).CreditCategory.CreditType != course.Credits.ElementAt(1).CreditCategory.CreditType)
                    {
                        throw new ArgumentException("The same Credit Type must be used for each entry in the Credits array. ", "credits.creditCategory.creditType");
                    }
                    if (!(course.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && course.Credits.ElementAt(1).Measure == CreditMeasure2.Credit)
                        && !(course.Credits.ElementAt(0).Measure == CreditMeasure2.Credit && course.Credits.ElementAt(1).Measure == CreditMeasure2.CEU)
                        && !(course.Credits.ElementAt(0).Measure == CreditMeasure2.CEU && course.Credits.ElementAt(1).Measure == CreditMeasure2.Hours)
                        && !(course.Credits.ElementAt(0).Measure == CreditMeasure2.Hours && course.Credits.ElementAt(1).Measure == CreditMeasure2.CEU))
                    {
                        throw new ArgumentException("Invalid combination of measures '" + course.Credits.ElementAt(0).Measure
                            + "' and '" + course.Credits.ElementAt(1).Measure + "'. ", "credits.measure");
                    }
                }
                foreach (var credit in course.Credits)
                {
                    if (credit.CreditCategory == null)
                    {
                        throw new ArgumentException("Credit Category is required if Credits are defined. ", "credit.creditCategory");
                    }

                    if (credit.CreditCategory.CreditType == null)
                    {
                        throw new ArgumentException("Credit Type is required if for Credit Categories if Credits are defined. ", "credit.creditCategory.creditType");
                    }

                    if (credit.CreditCategory.Detail != null && string.IsNullOrEmpty(credit.CreditCategory.Detail.Id))
                    {
                        throw new ArgumentException("Credit Category id is required within the detail section of Credit Category is defined. ", "credit.creditCategory.detail.id");
                    }
                    if (credit.Increment == null && credit.Maximum != null)
                    {
                        throw new ArgumentException("Credit Increment is required when Credit Maximum exists. ", "credit.maximum");
                    }
                    if (credit.Maximum == null && credit.Increment != null)
                    {
                        throw new ArgumentException("Credit Maximum is required when Credit Increment exists. ", "credit.increment");
                    }
                    if ((credit.Maximum != null || credit.Increment != null) && credit.Measure == CreditMeasure2.CEU)
                    {
                        throw new ArgumentException("Credit Maximum/Increment cannot exist when Credit Measure is 'ceu'. ", "credit.measure");
                    }
                    //if (credit.Increment != null && credit.Measure == CreditMeasure2.CEU)
                    //{
                    //    throw new ArgumentException("Credit Increment cannot exist when Credit Measure is 'ceu'. ", "credit.measure");
                    //}
                }

                if (course.Billing != null && course.Billing.Minimum < 0)
                {
                    throw new ArgumentException("Billing credits minimum cannot be less than zero. ", "billing.minimum");
                }
                if (course.Billing != null && course.Billing.Maximum != null && course.Billing.Minimum != course.Billing.Maximum)
                {
                    throw new ArgumentException("Billing minimum and maximum credits must be equal. ", "billing.minimum");
                }
                if (course.Billing != null && course.Billing.Increment != null && course.Billing.Increment != 0)
                {
                    throw new ArgumentException("Billing credits increment must be zero. ", "billing.increment");
                }
                if (course.Topic != null && string.IsNullOrEmpty(course.Topic.Id))
                {
                    throw new ArgumentException("Id must be supplied for a Topic. ", "topic.id");
                }
            }
            // Check for duplicates in the instructionalMethodDetails array
            var testInstructionalMethods = new List<string>();
            if (course.InstructionalMethodDetails != null && course.InstructionalMethodDetails.Any())
            {
                foreach (var detail in course.InstructionalMethodDetails)
                {
                    if (detail.InstructionalMethod != null && !string.IsNullOrEmpty(detail.InstructionalMethod.Id))
                    {
                        if (testInstructionalMethods.Contains(detail.InstructionalMethod.Id))
                        {
                            throw new ArgumentException(string.Format("Duplicate instructional method '{0}' is not allowed. ", detail.InstructionalMethod.Id), "instructionalMethodDetail.instructionalMethod.id");
                        }
                        testInstructionalMethods.Add(detail.InstructionalMethod.Id);
                    }
                }
            }
            // Check to make sure required data is included in the hours array and there are no duplicates
            testInstructionalMethods = new List<string>();
            if (course.Hours != null && course.Hours.Any())
            {
                foreach (var hours in course.Hours)
                {
                    if (hours.AdministrativeInstructionalMethod != null && !string.IsNullOrEmpty(hours.AdministrativeInstructionalMethod.Id))
                    {
                        if (testInstructionalMethods.Contains(hours.AdministrativeInstructionalMethod.Id))
                        {
                            throw new ArgumentException(string.Format("Duplicate instructional method '{0}' is not allowed. ", hours.AdministrativeInstructionalMethod.Id), "hours.administrativeInstructionalMethod.id");
                        }
                        testInstructionalMethods.Add(hours.AdministrativeInstructionalMethod.Id);
                        if (hours.Minimum == null)
                        {
                            throw new ArgumentException(string.Format("hours minimum is required for administrativeInstructionalMethod '{0}'. ", hours.AdministrativeInstructionalMethod.Id), "hours.minimum");
                        }
                    }
                }
            }

            await ReadCourseCodesAsync(bypassCache);

            var courseConfig = await _studentConfigRepository.GetCurriculumConfigurationAsync();

            VerifyCurriculumConfiguration(courseConfig);

            // Set the subject based on the supplied GUID
            var subjectCode = ConvertGuidToCode(_subjects, course.Subject.Id);

            if (subjectCode == null)
            {
                throw new ArgumentException(string.Concat("Subject for id '", course.Subject.Id.ToString(), "' was not found. Valid Subject is required. "), "subject.id");
            }

            // Set the list of departments/shares
            List<OfferingDepartment> offeringDepartments = new List<OfferingDepartment>();

            // If we have supplied owning organization, then use that first.
            if (course.OwningInstitutionUnits != null && course.OwningInstitutionUnits.Any())
            {
                foreach (var owningInstitutionUnit in course.OwningInstitutionUnits)
                {
                    var division = (await _referenceDataRepository.GetDivisionsAsync(bypassCache))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (division != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'division' is not supported.");
                    }

                    var school = (await _referenceDataRepository.GetSchoolsAsync(bypassCache))
                                            .FirstOrDefault(div => div.Guid.Equals(owningInstitutionUnit.InstitutionUnit.Id, StringComparison.OrdinalIgnoreCase));
                    if (school != null)
                    {
                        throw new InvalidOperationException("Owning institution unit of type 'school' is not supported.");
                    }

                    var department = _departments.Where(d => d.Guid == owningInstitutionUnit.InstitutionUnit.Id).FirstOrDefault();
                    var academicDepartment = department != null ? _academicDepartments.FirstOrDefault(ad => ad.Code == department.Code) : null;
                    if (academicDepartment != null)
                    {
                        offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, owningInstitutionUnit.OwnershipPercentage));
                    }
                }
            }

            // If we don't have owning organizations then try using the subject-department mapping 
            // to determine department based on subject
            if (offeringDepartments.Count() == 0 && courseConfig.SubjectDepartmentMapping.Items != null && courseConfig.SubjectDepartmentMapping.Items.Count > 0)
            {
                var deptMapping = courseConfig.SubjectDepartmentMapping.Items.Where(i => i.OriginalCode == subjectCode).FirstOrDefault();
                var deptCode = deptMapping != null ? deptMapping.NewCode : null;
                var department = deptCode != null ? _departments.Where(d => d.Code == deptCode).FirstOrDefault() : null;
                var academicDepartment = department != null ? _academicDepartments.Where(ad => ad.Code == department.Code).FirstOrDefault() : null;

                if (academicDepartment != null)
                {
                    offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, 100m));
                }
            }

            // If we still don't have a valid department then check the subject code.
            // If the subject code is also a valid academic department then use that.
            if (offeringDepartments.Count == 0 && !string.IsNullOrEmpty(subjectCode))
            {
                var department = subjectCode != null ? _departments.Where(d => d.Code == subjectCode).FirstOrDefault() : null;
                var academicDepartment = department != null ? _academicDepartments.Where(ad => ad.Code == department.Code).FirstOrDefault() : null;
                if (academicDepartment != null)
                {
                    offeringDepartments.Add(new OfferingDepartment(academicDepartment.Code, 100m));
                }
            }

            // Department could not be determined from supplied subject or owning organization
            if (offeringDepartments.Count == 0)
            {
                throw new ArgumentException("Department could not be determined for subject '" + subjectCode + "'. ", "subject.id");
            }

            // Set the academic level code based on the supplied GUID or the default if one is not supplied
            string acadLevelCode = null;
            if (course.AcademicLevels != null && course.AcademicLevels.Any())
            {
                int counter = 1;
                foreach (var acadLevel in course.AcademicLevels)
                {
                    try
                    {
                        //Colleague only permits a single academic level for a course, so a POST or PUT request w/ multiple academic levels should respond with an error.
                        if (counter > 1)
                        {
                            throw new InvalidOperationException("A single academic level is permitted.");
                        }

                        if (string.IsNullOrEmpty(acadLevel.Id))
                        {
                            throw new ArgumentException("Academic Level id is a required field when Academic Levels are defined. ", "academicLevels.id");
                        }

                        var acadLvl = _academicLevels.Where(al => al.Guid == acadLevel.Id).FirstOrDefault();
                        if(acadLvl == null)
                        {
                            throw new ArgumentNullException("Invalid Id '" + acadLevel.Id + "' supplied for academicLevels. ", "academicLevels.id");
                        }
                        acadLevelCode = acadLvl.Code;
                        counter++;
                    }
                    catch(Exception)
                    {
                        throw;
                    }
                }
            }
            else
            {
                acadLevelCode = courseConfig.DefaultAcademicLevelCode;
            }

            // Set the list of course level codes based on the supplied GUIDs or the default if one is not supplied
            List<string> courseLevelCodes = new List<string>();
            if (course.CourseLevels != null && course.CourseLevels.Any())
            {
                foreach (var courseLevel in course.CourseLevels)
                {
                    try
                    {
                        courseLevelCodes.Add(_courseLevels.First(cl => cl.Guid == courseLevel.Id).Code);
                    }
                    catch
                    {
                        throw new ArgumentException("Invalid Id '" + courseLevel.Id + "' supplied for courseLevels. ", "courseLevels.id");
                    }
                }
            }
            else
            {
                courseLevelCodes.Add(courseConfig.DefaultCourseLevelCode);
            }

            // Set the list of course approvals based on the supplied GUIDs
            List<CourseApproval> courseApprovals = new List<CourseApproval>();
            var approvingAgencyId = courseConfig.ApprovingAgencyId;
            var approverId = courseConfig.ApproverId;
            string statusCode = string.Empty;
            string statusTitle = string.Empty;
            var status = CourseStatus.Unknown;
            var today = DateTime.Today;
            // Get status from payload.
            if (course.Status != null && !string.IsNullOrEmpty(course.Status.Id))
            {
                var statusEntity = _courseStatuses.FirstOrDefault(sc => sc.Guid == course.Status.Id);
                if (statusEntity == null)
                {
                    throw new ArgumentException(string.Format("Status id '{0}' is invalid. ", course.Status.Id), "status.id");
                }
                if (string.IsNullOrEmpty(statusEntity.Code))
                    throw new ArgumentException(string.Format("Status id '{0}' is invalid. ", course.Status.Id), "status.id");
                statusCode = statusEntity.Code;
                statusTitle = statusEntity.Description;
                status = statusEntity.Status;
            }
            if (status == CourseStatus.Terminated)
            {
                if (course.EffectiveEndDate == null)
                {
                    throw new ArgumentException(string.Format("The scheduling end on date must have a value when using a status of '{0}'. ", course.Status.Id), "schedulingEndOn");
                }
            }
            courseApprovals.Add(new CourseApproval(statusCode, DateTime.Today, approvingAgencyId, approverId, DateTime.Today)
            {
                Status = status
            });

            // Set the list of grade scheme codes based on the supplied GUIDs
            string gradeSchemeCode = null;
            if (course.GradeSchemes != null && course.GradeSchemes.Any() && course.GradeSchemes.ToList()[0].GradeScheme != null && !string.IsNullOrEmpty(course.GradeSchemes.ToList()[0].GradeScheme.Id))
            {
                try
                {
                    gradeSchemeCode = _gradeSchemes.First(gs => gs.Guid == course.GradeSchemes.ToList()[0].GradeScheme.Id).Code;
                }
                catch
                {
                    throw new ArgumentException("Invalid Id '" + course.GradeSchemes.ToList()[0].GradeScheme.Id + "' supplied for gradeSchemes. ", "gradeSchemes.gradeScheme.id");
                }
            }

            // Set the credit type and credits/CEUs for the course based on the supplied GUID, or using the ERP default 
            CreditCategory creditCategory = null;
            if (course.Credits != null &&
                course.Credits.Any() &&
                course.Credits.ToList()[0].CreditCategory != null &&
                course.Credits.ToList()[0].CreditCategory.Detail != null &&
                !string.IsNullOrEmpty(course.Credits.ToList()[0].CreditCategory.Detail.Id))
            {
                creditCategory = _creditCategories.FirstOrDefault(ct => ct.Guid == course.Credits.ToList()[0].CreditCategory.Detail.Id);
                if (creditCategory == null)
                {
                    throw new ArgumentException("Invalid Id '" + course.Credits.ToList()[0].CreditCategory.Detail.Id + "' supplied for creditCategory. ", "credits.creditCategory.detail.id");
                }
            }
            // If we don't have a GUID then check for a CreditType enumeration value
            if (creditCategory == null &&
                course.Credits != null &&
                course.Credits.Any() &&
                course.Credits.ToList()[0].CreditCategory != null &&
                course.Credits.ToList()[0].CreditCategory.CreditType != null)
            {
                if (course.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.Exam ||
                    course.Credits.ToList()[0].CreditCategory.CreditType == CreditCategoryType3.WorkLifeExperience)
                {
                    throw new InvalidOperationException("Credit category type 'exam' or 'workLifeExperience' are not supported. ");
                }

                // Find the credit category that matches the enumeration
                switch (course.Credits.ToList()[0].CreditCategory.CreditType)
                {
                    case (CreditCategoryType3.ContinuingEducation):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.ContinuingEducation);
                        break;
                    case (CreditCategoryType3.Institutional):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Institutional);
                        break;
                    case (CreditCategoryType3.Transfer):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Transfer);
                        break;
                    case (CreditCategoryType3.Exchange):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Exchange);
                        break;
                    case (CreditCategoryType3.Other):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.Other);
                        break;
                    case (CreditCategoryType3.NoCredit):
                        creditCategory = _creditCategories.FirstOrDefault(cc => cc.CreditType == CreditType.None);
                        break;
                }
            }
            if (creditCategory == null)
            {
                // Get default from course configuration
                creditCategory = _creditCategories.FirstOrDefault(ct => ct.Code == courseConfig.DefaultCreditTypeCode);
            }
            //If creditCategory is null then throw an exception
            if (creditCategory == null)
            {
                throw new ArgumentException(string.Format("Credit category {0} was not found. ", courseConfig.DefaultCreditTypeCode), "credits.creditCategory.creditType");
            }


            var creditTypeCode = creditCategory == null ? string.Empty : creditCategory.Code;

            decimal? minCredits;
            decimal? ceus;
            decimal? maxCredits;
            decimal? varIncrCredits;
            minCredits = null;
            maxCredits = null;
            varIncrCredits = null;
            ceus = null;
            if (course.Credits != null)
            {
                foreach (var credits in course.Credits)
                {
                    var creditInfo = (course.Credits == null || course.Credits.Count == 0) ? null : credits;
                    var measure = creditInfo == null ? null : creditInfo.Measure;
                    if (measure == Dtos.CreditMeasure2.CEU)
                    {
                        ceus = creditInfo == null ? 0 : creditInfo.Minimum;
                    }
                    else
                    {
                        minCredits = creditInfo == null ? 0 : creditInfo.Minimum;
                        maxCredits = creditInfo == null ? null : creditInfo.Maximum;
                        varIncrCredits = creditInfo == null ? null : creditInfo.Increment;
                    }
                }
            }
            var courseDelimeter = !string.IsNullOrEmpty(courseConfig.CourseDelimiter) ? courseConfig.CourseDelimiter : CourseDelimiter;

            //Handle GUID requiredness
            string guid = (course.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase)) ? string.Empty : course.Id;

            // Optional flag for allowing waitlist of multiple sections
            bool? waitlistMultiSections = null;
            if (course.WaitlistMultipleSections != null && course.WaitlistMultipleSections != 0)
            {
                waitlistMultiSections = (course.WaitlistMultipleSections == WaitlistMultiSectionFlag.Allowed);

            }

            string topicCode = null;
            if (course.Topic != null)
            {
                if (string.IsNullOrEmpty(course.Topic.Id))
                {
                    throw new ArgumentException("Course topic must contain a valid Id. ", "topic.id");
                }
                try
                {
                    topicCode = (_topicCodes.FirstOrDefault(cc => cc.Guid == course.Topic.Id)).Code;
                }
                catch
                {
                    throw new ArgumentException("Invalid Id '" + course.Topic.Id + "' supplied for topic. ", "topic.id");
                }
            }

            List<string> courseTypeCodes = null;
            if (course.Categories != null && course.Categories.Any())
            {
                courseTypeCodes = new List<string>();
                foreach (var cat in course.Categories)
                {
                    if (string.IsNullOrEmpty(cat.Id))
                    {
                        throw new ArgumentException("Course category must contain a valid Id. ", "categories.id");
                    }

                    var catEntity = _courseCategories.FirstOrDefault(cc => cc.Guid == cat.Id);
                    if (catEntity == null)
                    {
                        throw new ArgumentException("Invalid Id '" + cat.Id + "' supplied for course category. ", "categories.id");
                    }
                    if (!courseTypeCodes.Contains(catEntity.Code)) courseTypeCodes.Add(catEntity.Code);
                }
            }


            // Contact methods, period, hours

            List<string> contactMethodCodes = null;
            List<string> contactPeriods = null;
            List<decimal?> contactHours = null;

            if (course.Hours != null && course.Hours.Any())
            {
                foreach (var mthd in course.Hours)
                {
                    if (mthd.AdministrativeInstructionalMethod == null || string.IsNullOrEmpty(mthd.AdministrativeInstructionalMethod.Id))
                    {
                        throw new ArgumentException("Instructional Method detail requires an instructional method Id. ", "hours.administrativeInstructionalMethod.id");
                    }
                    var method = _administrativeInstructionalMethods.FirstOrDefault(im => im.Guid.Equals(mthd.AdministrativeInstructionalMethod.Id, StringComparison.OrdinalIgnoreCase));
                    if (method != null)
                    {
                        if (contactMethodCodes == null)
                        {
                            contactMethodCodes = new List<string>();
                            contactPeriods = new List<string>();
                            contactHours = new List<decimal?>();
                        }
                        contactMethodCodes.Add(method.Code);
                    }
                    else
                    {
                        throw new ArgumentException("Instructional method GUID '" + mthd.AdministrativeInstructionalMethod.Id + "' could not be found. ", "hours.administrativeInstructionalMethod.id");
                    }
                    if (mthd.Minimum.HasValue)
                    {
                        contactHours.Add(mthd.Minimum);

                        ContactMeasure contactPeriod = null;
                        if ((mthd.Interval != null) && (mthd.Interval != ContactHoursPeriod.NotSet))
                        {

                            contactPeriod = _contactMeasures.FirstOrDefault(cm => cm.ContactPeriod.ToString() == mthd.Interval.ToString().ToLowerInvariant());

                            if (contactPeriod == null)
                            {
                                throw new ArgumentException("Hours interval value '" + mthd.Interval.ToString() + "' could not be matched to a contact measure. ", "hours.interval");
                            }
                            contactPeriods.Add(contactPeriod.Code);
                        }
                        else
                        {
                            contactPeriods.Add(null);
                        }
                        // Maximum and Increment are not published or consumed by Colleague per spec
                        // but if they were they'd go here.
                    }
                    else
                    {
                        contactHours.Add(null);
                        contactPeriods.Add(null);
                    }
                }
            }
            // Add in any missing instructional methods from instructional method details
            if (course.InstructionalMethodDetails != null && course.InstructionalMethodDetails.Any())
            {
                foreach (var mthd in course.InstructionalMethodDetails)
                {
                    if (mthd.InstructionalMethod != null && !string.IsNullOrEmpty(mthd.InstructionalMethod.Id))
                    {
                        var method = _instructionalMethods.FirstOrDefault(im => im.Guid.Equals(mthd.InstructionalMethod.Id, StringComparison.OrdinalIgnoreCase));
                        if (method != null)
                        {
                            if (contactMethodCodes == null)
                            {
                                contactMethodCodes = new List<string>();
                            }
                            if (!contactMethodCodes.Contains(method.Code))
                            {
                                contactMethodCodes.Add(method.Code);
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Instructional method GUID '" + mthd.InstructionalMethod.Id + "' could not be found. ", "instructionalMethodDetails.instructionalMethod.id");
                        }
                    }
                }
            }
            // Determine both short and long titles
            string shortTitle = "", longTitle = "", defaultTitle = "";
            if (course.Titles != null && course.Titles.Any())
            {
                var shortEntity = _courseTitleTypes.FirstOrDefault(ctt => ctt.Code.ToLower() == "short");
                if (shortEntity == null || string.IsNullOrEmpty(shortEntity.Guid))
                {
                    throw new ArgumentNullException("titles.type", "The record 'SHORT' or it's GUID is missing from course title types. ");
                }
                var longEntity = _courseTitleTypes.FirstOrDefault(ctt => ctt.Code.ToLower() == "long");
                if (longEntity == null || string.IsNullOrEmpty(longEntity.Guid))
                {
                    throw new ArgumentNullException("titles.type", "The record 'LONG' or it's GUID is missing from course title types. ");
                }
                foreach (var title in course.Titles)
                {
                    if (title.Type != null && !string.IsNullOrEmpty(title.Type.Id) && !string.IsNullOrEmpty(title.Value))
                    {
                        if (title.Type.Id.Equals(shortEntity.Guid, StringComparison.OrdinalIgnoreCase)) shortTitle = title.Value;
                        if (title.Type.Id.Equals(longEntity.Guid, StringComparison.OrdinalIgnoreCase)) longTitle = title.Value;
                    }
                    if (!string.IsNullOrEmpty(title.Value))
                    {
                        defaultTitle = title.Value;
                    }
                }
            }
            if (string.IsNullOrEmpty(shortTitle)) shortTitle = longTitle;
            if (string.IsNullOrEmpty(longTitle)) longTitle = shortTitle;
            // Make sure we have a title if we submit only value with no type.
            if (string.IsNullOrEmpty(shortTitle) && string.IsNullOrEmpty(longTitle))
            {
                shortTitle = defaultTitle;
                longTitle = defaultTitle;
            }

            // Build the course entity
            var courseEntity = new Ellucian.Colleague.Domain.Student.Entities.Course(null, shortTitle, longTitle, offeringDepartments,
                subjectCode, course.Number, acadLevelCode, courseLevelCodes, minCredits, ceus, courseApprovals)
            {
                LocalCreditType = creditTypeCode,
                Description = course.Description,
                Guid = guid,
                GradeSchemeCode = gradeSchemeCode,
                StartDate = course.EffectiveStartDate,
                EndDate = course.EffectiveEndDate,
                Name = subjectCode + courseDelimeter + course.Number,
                MaximumCredits = maxCredits,
                VariableCreditIncrement = varIncrCredits,
                AllowPassNoPass = courseConfig.AllowPassNoPass,
                AllowAudit = courseConfig.AllowAudit,
                OnlyPassNoPass = courseConfig.OnlyPassNoPass,
                AllowWaitlist = courseConfig.AllowWaitlist,
                IsInstructorConsentRequired = courseConfig.IsInstructorConsentRequired,
                WaitlistRatingCode = courseConfig.WaitlistRatingCode,
                AllowWaitlistMultipleSections = waitlistMultiSections,
                TopicCode = topicCode,
                CourseTypeCodes = courseTypeCodes,
            };

            // Add billing credits if they exist
            if (course.Billing != null)
            {
                courseEntity.BillingCredits = course.Billing.Minimum;
            }


            // Add instructional method data if supplied

            if (contactMethodCodes != null)
            {
                contactMethodCodes.ForEach(cm => courseEntity.AddInstructionalMethodCode(cm));
            }
            if (contactPeriods != null)
            {
                contactPeriods.ForEach(cp => courseEntity.AddInstructionalMethodPeriod(cp));
            }
            if (contactHours != null)
            {
                contactHours.ForEach(ch => courseEntity.AddInstructionalMethodHours(ch));
            }


            // Verify that all the data in the entity is valid
            CourseProcessor.ValidateCourseData(courseEntity, _academicLevels, _courseLevels, null, _creditCategories, _academicDepartments,
                _gradeSchemes, _instructionalMethods, _subjects, _locations, _topicCodes);

            return courseEntity;
        }

        #endregion
    }
}
