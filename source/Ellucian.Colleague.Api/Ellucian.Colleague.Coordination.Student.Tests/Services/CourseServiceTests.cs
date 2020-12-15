// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Data.Student;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Course = Ellucian.Colleague.Dtos.Student.Course;
using Course2 = Ellucian.Colleague.Dtos.Student.Course2;
using Section = Ellucian.Colleague.Dtos.Student.Section;
using Section2 = Ellucian.Colleague.Dtos.Student.Section2;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using SectionEntity = Ellucian.Colleague.Domain.Student.Entities.Section;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CourseServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role thirdPartyRole = new Ellucian.Colleague.Domain.Entities.Role(1, "ThirdPartyCanUpdateGrades");
            protected Ellucian.Colleague.Domain.Entities.Role createUpdateInstrEvent = new Ellucian.Colleague.Domain.Entities.Role(1, "UPDATE.ROOM.BOOKING");

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "STU1",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            // Represents a third party system like ILP
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ILP",
                            PersonId = "ILP",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ILP",
                            Roles = new List<string>() { "ThirdPartyCanUpdateGrades",
                                                     "CreateAndUpdateRoomBooking",
                                                     "CreateAndUpdateFacultyBooking", "UPDATE.ROOM.BOOKING"},
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            public class FacultytUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "678",
                            Name = "Samwise",
                            PersonId = "0000678",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise-faculty",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }


        /// <summary>
        /// Search: Test Base data of CoursePage - coverage for the results and filters
        /// </summary>
        [TestClass]
        public class CoursePageBase
        {
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IRuleRepository> ruleRepoMock;
            private IRuleRepository ruleRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private Mock<IRequirementRepository> requirementsRepoMock;
            private IRequirementRepository requirementsRepo;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
            private IStudentReferenceDataRepository studentReferenceRepo;
            private Mock<IStudentConfigurationRepository> configRepoMock;
            private IStudentConfigurationRepository configRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private CourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
            private IEnumerable<string> catalogSubjectCodes;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
            private ILogger logger;
            private Mock<ICurrentUserFactory> userFactoryMock;
            private ICurrentUserFactory userFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public async void Initialize()
            {
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                allCourses = new TestCourseRepository().GetAsync().Result;
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                catalogCourses = allCourses.Where(c => c.IsCurrent == true && catalogSubjectCodes.Contains(c.SubjectCode));

                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                ruleRepoMock = new Mock<IRuleRepository>();
                ruleRepo = ruleRepoMock.Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                allSections = new TestSectionRepository().GetAsync().Result;
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(allSections));

                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                allSubjects = new TestSubjectRepository().Get();
                allLocations = new TestLocationRepository().Get();
                allDepartments = new TestDepartmentRepository().Get();
                referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
                referenceRepoMock.Setup(repo => repo.GetDepartmentsAsync(false)).Returns(Task.FromResult(allDepartments));

                requirementsRepoMock = new Mock<IRequirementRepository>();
                requirementsRepo = requirementsRepoMock.Object;

                studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceRepo = studentReferenceRepoMock.Object;
                studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));
                catalogSubjectCodes = allSubjects.Where(s => s.ShowInCourseSearch == true).Select(s => s.Code);

                configRepoMock = new Mock<IStudentConfigurationRepository>();
                configRepo = configRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                userFactoryMock = new Mock<ICurrentUserFactory>();
                userFactory = userFactoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                var coursePageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>()).Returns(coursePageAdapter);
                var corequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>()).Returns(corequisiteAdapter);
                courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                    studentReferenceRepo, requirementsRepo, sectionRepoMock.Object, termRepoMock.Object,
                    ruleRepoMock.Object, configRepo, baseConfigurationRepository, userFactory, roleRepo, logger);

                CourseService.ClearIndex();
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseRepoMock = null;
                courseRepo = null;
                referenceRepoMock = null;
                referenceRepo = null;
                adapterRegistryMock = null;
                adapterRegistry = null;
                ruleRepoMock = null;
                courseService = null;
            }

            [TestMethod]
            public async Task EmptySearchReturnsValidCoursePageInfo()
            {
                // Arrange -- Set up empty search
                var pageSize = 10;
                var pageIndex = 2;
                var subjectFilterList = new CourseSearchCriteria();

                // Act -- Invoke Search service
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(subjectFilterList, pageSize, pageIndex);
#pragma warning restore 618

                // Assert
                // A single page of courses is returned
                Assert.AreEqual(pageSize, coursePage.CurrentPageItems.Count());
                // At least 5 pages (assuming >= 45 items, 10 items per page)
                Assert.IsTrue(coursePage.TotalPages > 4);
                // total item count matches all courses
                Assert.AreEqual(catalogCourses.Count(), coursePage.TotalItems);
                // Assert page size initialized
                Assert.AreEqual(pageSize, coursePage.PageSize);
                // Assert page index initialized
                Assert.AreEqual(pageIndex, coursePage.CurrentPageIndex);
                // Subject filter details contain many subjects
                Assert.IsTrue(coursePage.Subjects.Count() > 13);
                // CoursePage type is returned
                Assert.IsTrue(coursePage is CoursePage);
                // List of section Ids returned (approx 6 per course, there are 10 courses on this page, but some courses have 7 sections)
                var pageSectionIds = new List<string>();
                foreach (var item in coursePage.CurrentPageItems)
                {
                    pageSectionIds.AddRange(item.MatchingSectionIds);
                }
                Assert.IsTrue(pageSectionIds.Count() >= 60);
            }

            [TestMethod]
            public async Task AdjustsPageSizeAndIndex()
            {
                // Arrange -- build search request
                var pageSize = 0;
                var pageIndex = 1000;
                var subjectFilterList = new CourseSearchCriteria();

                // Act -- Invoke Search service
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(subjectFilterList, pageSize, pageIndex);
#pragma warning restore 618

                // Assert -- 
                // Page size increased from 0 to default
                Assert.IsTrue(coursePage.PageSize > 0);
                // Page index decreased to last page
                Assert.IsTrue(coursePage.CurrentPageIndex < pageIndex);
            }

            [TestMethod]
            public async Task FixesPageIndexOfZero()
            {
                // Arrange -- build search request
                var pageSize = 10;
                var pageIndex = 0;
                var subjectFilterList = new CourseSearchCriteria();

                // Act -- invoke search service 
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(subjectFilterList, pageSize, pageIndex);
#pragma warning restore 618

                // Assert -- page index has been adjusted to 1 from zero
                Assert.IsTrue(coursePage.CurrentPageIndex == 1);
                // Assert -- courses have been returned
                Assert.IsTrue(coursePage.CurrentPageItems.Count() > 0);
            }

            [TestMethod]
            public async Task FixesNegativePageIndexAndSize()
            {
                // Arrange -- build search request
                var pageSize = -15;
                var pageIndex = -10;
                var subjectFilterList = new CourseSearchCriteria();

                // Act -- invoke search service 
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(subjectFilterList, pageSize, pageIndex);
#pragma warning restore 618

                // Assert -- page index has been adjusted to 1 from zero
                Assert.IsTrue(coursePage.CurrentPageIndex == 1);
                // Assert -- page size has been adjusted to positive number (whatever default is)
                Assert.IsTrue(coursePage.PageSize > 0);
                // Assert -- courses have been returned
                Assert.IsTrue(coursePage.CurrentPageItems.Count() > 0);
            }

            [TestMethod]
            public async Task CourseDtoContainsAllCourseData()
            {
                // Arrange -- Set up empty search
                var pageSize = Int16.MaxValue;
                var pageIndex = 2;
                var emptySearchCriteria = new CourseSearchCriteria();

                // Act -- Invoke Search service
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(emptySearchCriteria, pageSize, pageIndex);
#pragma warning restore 618

                // Assert
                var courseDto = coursePage.CurrentPageItems.Where(c => c.Id == "42").First();
                var course = catalogCourses.Where(c => c.Id == "42").First();
                Assert.AreEqual(course.Id, courseDto.Id);
                Assert.AreEqual(course.SubjectCode, courseDto.SubjectCode);
                Assert.AreEqual(course.Number, courseDto.Number);
                Assert.AreEqual(course.MinimumCredits, courseDto.MinimumCredits);
                Assert.AreEqual(course.MaximumCredits, courseDto.MaximumCredits);
                Assert.AreEqual(course.Ceus, courseDto.Ceus);
                Assert.AreEqual(course.Title, courseDto.Title);
                Assert.AreEqual(course.Description, courseDto.Description);
                Assert.AreEqual(course.YearsOffered, courseDto.YearsOffered);
                Assert.AreEqual(course.TermsOffered, courseDto.TermsOffered);
                Assert.AreEqual(course.LocationCodes.Count(), courseDto.LocationCodes.Count());
            }

            [TestMethod]
            public async Task CourseDtoContainsPrerequisiteData()
            {
                // Arrange -- Set up empty search
                var pageSize = Int16.MaxValue;
                var pageIndex = 2;
                var emptySearchCriteria = new CourseSearchCriteria();

                // Act -- Invoke Search service
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(emptySearchCriteria, pageSize, pageIndex);
#pragma warning restore 618

                // Assert
                var courseDto = coursePage.CurrentPageItems.Where(c => c.Id == "186").First();
                var course = catalogCourses.Where(c => c.Id == "186").First();
                Assert.AreEqual(course.Requisites.ElementAt(0).RequirementCode, courseDto.Prerequisites);
            }
        }

        /// <summary>
        /// Search: Test Base data of CoursePage2 - coverage for the results and filters are under CoursePageBase
        /// </summary>
        [TestClass]
        public class Search2
        {
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IRuleRepository> ruleRepoMock;
            private IRuleRepository ruleRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private Mock<IRequirementRepository> requirementsRepoMock;
            private IRequirementRepository requirementsRepo;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
            private IStudentReferenceDataRepository studentReferenceRepo;
            private Mock<IStudentConfigurationRepository> configurationRepoMock;
            private IStudentConfigurationRepository configurationRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private CourseService courseService;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
            private IEnumerable<string> catalogSubjectCodes;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
            private ILogger logger;
            private Mock<ICurrentUserFactory> userFactoryMock;
            private ICurrentUserFactory userFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public async void Initialize()
            {
                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                allCourses = new TestCourseRepository().GetAsync().Result;
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                catalogCourses = allCourses.Where(c => c.IsCurrent == true && catalogSubjectCodes.Contains(c.SubjectCode));

                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                ruleRepoMock = new Mock<IRuleRepository>();
                ruleRepo = ruleRepoMock.Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                allSections = new TestSectionRepository().GetAsync().Result;
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(allSections));

                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                allSubjects = new TestSubjectRepository().Get();
                allLocations = new TestLocationRepository().Get();
                allDepartments = new TestDepartmentRepository().Get();
                referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
                referenceRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allLocations);
                referenceRepoMock.Setup(repo => repo.GetDepartmentsAsync(false)).Returns(Task.FromResult(allDepartments));

                requirementsRepoMock = new Mock<IRequirementRepository>();
                requirementsRepo = requirementsRepoMock.Object;

                studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceRepo = studentReferenceRepoMock.Object;
                studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));
                catalogSubjectCodes = allSubjects.Where(s => s.ShowInCourseSearch == true).Select(s => s.Code);

                configurationRepoMock = new Mock<IStudentConfigurationRepository>();
                configurationRepo = configurationRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                userFactoryMock = new Mock<ICurrentUserFactory>();
                userFactory = userFactoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                var coursePageAdapter2 = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>()).Returns(coursePageAdapter2);
                var requisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>()).Returns(requisiteAdapter);
                var locationCycleAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.LocationCycleRestriction, LocationCycleRestriction>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.LocationCycleRestriction, LocationCycleRestriction>()).Returns(locationCycleAdapter);
                courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                    studentReferenceRepo, requirementsRepo, sectionRepoMock.Object, termRepoMock.Object,
                    ruleRepoMock.Object, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);

                CourseService.ClearIndex();
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseRepoMock = null;
                courseRepo = null;
                referenceRepoMock = null;
                referenceRepo = null;
                adapterRegistryMock = null;
                adapterRegistry = null;
                courseService = null;
            }

            [TestMethod]
            public async Task Course2DtoContainsAllCourseData()
            {
                // Arrange -- Set up empty search
                var pageSize = Int16.MaxValue;
                var pageIndex = 2;
                var emptySearchCriteria = new CourseSearchCriteria();

                // Act -- Invoke Search service
                CoursePage2 course2Page = await courseService.Search2Async(emptySearchCriteria, pageSize, pageIndex);

                // Assert
                var course2Dto = course2Page.CurrentPageItems.Where(c => c.Id == "42").First();
                var course = catalogCourses.Where(c => c.Id == "42").First();
                Assert.AreEqual(course.Id, course2Dto.Id);
                Assert.AreEqual(course.SubjectCode, course2Dto.SubjectCode);
                Assert.AreEqual(course.Number, course2Dto.Number);
                Assert.AreEqual(course.MinimumCredits, course2Dto.MinimumCredits);
                Assert.AreEqual(course.MaximumCredits, course2Dto.MaximumCredits);
                Assert.AreEqual(course.Ceus, course2Dto.Ceus);
                Assert.AreEqual(course.Title, course2Dto.Title);
                Assert.AreEqual(course.Description, course2Dto.Description);
                Assert.AreEqual(course.YearsOffered, course2Dto.YearsOffered);
                Assert.AreEqual(course.TermsOffered, course2Dto.TermsOffered);
                Assert.AreEqual(course.TermYearlyCycle, course2Dto.TermYearlyCycle);
                Assert.AreEqual(course.TermSessionCycle, course2Dto.TermSessionCycle);
                Assert.AreEqual(course.LocationCodes.Count(), course2Dto.LocationCodes.Count());
            }

            [TestMethod]
            public async Task Course2DtoContainsAllCourseData_HiddenLocationsFromCourseNotIncluded()
            {
                // Data Setup -- course with all location codes, some of which will be filtered out
                var course = allCourses.Where(c => c.Id == "42").First();
                course.LocationCodes = allLocations.Select(loc => loc.Code).Distinct().ToList();

                // Arrange -- Set up empty search
                var pageSize = Int16.MaxValue;
                var pageIndex = 2;
                var emptySearchCriteria = new CourseSearchCriteria();

                // Act -- Invoke Search service
                CoursePage2 course2Page = await courseService.Search2Async(emptySearchCriteria, pageSize, pageIndex);
                var locationCount = allLocations.Where(loc => !loc.HideInSelfServiceCourseSearch).Count();

                // Assert
                var course2Dto = course2Page.CurrentPageItems.Where(c => c.Id == "42").First();
                Assert.AreEqual(course.Id, course2Dto.Id);
                Assert.AreEqual(course.SubjectCode, course2Dto.SubjectCode);
                Assert.AreEqual(course.Number, course2Dto.Number);
                Assert.AreEqual(course.MinimumCredits, course2Dto.MinimumCredits);
                Assert.AreEqual(course.MaximumCredits, course2Dto.MaximumCredits);
                Assert.AreEqual(course.Ceus, course2Dto.Ceus);
                Assert.AreEqual(course.Title, course2Dto.Title);
                Assert.AreEqual(course.Description, course2Dto.Description);
                Assert.AreEqual(course.YearsOffered, course2Dto.YearsOffered);
                Assert.AreEqual(course.TermsOffered, course2Dto.TermsOffered);
                Assert.AreEqual(course.TermYearlyCycle, course2Dto.TermYearlyCycle);
                Assert.AreEqual(course.TermSessionCycle, course2Dto.TermSessionCycle);
                Assert.AreEqual(course.LocationCodes.Count(), course2Dto.LocationCodes.Count());
                Assert.AreEqual(locationCount, course2Page.Locations.Count());
            }

            [TestMethod]
            public async Task Course2DtoContainsRequisiteData()
            {
                // Arrange -- Set up empty search
                var pageSize = Int16.MaxValue;
                var pageIndex = 2;
                var emptySearchCriteria = new CourseSearchCriteria();

                // Act -- Invoke Search service
                CoursePage2 course2Page = await courseService.Search2Async(emptySearchCriteria, pageSize, pageIndex);

                // Assert
                var course2Dto = course2Page.CurrentPageItems.Where(c => c.Id == "186").First();
                var course = catalogCourses.Where(c => c.Id == "186").First();
                Assert.AreEqual(course.Requisites.Count(), course2Dto.Requisites.Count());

            }

            [TestMethod]
            public async Task Course2DtoContainsLocationCycleRestrictionData()
            {
                // Arrange -- Set up empty search
                var pageSize = Int16.MaxValue;
                var pageIndex = 2;
                var emptySearchCriteria = new CourseSearchCriteria();

                // Act -- Invoke Search service
                CoursePage2 course2Page = await courseService.Search2Async(emptySearchCriteria, pageSize, pageIndex);

                // Assert
                var course2Dto = course2Page.CurrentPageItems.Where(c => c.Id == "46").First();
                var course = catalogCourses.Where(c => c.Id == "46").First();
                Assert.AreEqual(course.LocationCycleRestrictions.Count(), course2Dto.LocationCycleRestrictions.Count());

            }

            [TestMethod]
            public async Task Search2Async_HidingSections()
            {
                // Arrange -- Set up wide open search
                var pageSize = Int16.MaxValue;
                var pageIndex = 2;
                var emptySearchCriteria = new CourseSearchCriteria() { CourseIds = new List<string>() { "7714", "7715" } };

                // Act -- Invoke Search service
                CoursePage2 course2Page = await courseService.Search2Async(emptySearchCriteria, pageSize, pageIndex);

                // Assert
                var specificSections = allSections.Where(sec => sec.CourseId == "7714" || sec.CourseId == "7715");

                var hiddenSections = allSections.Where(sec => (sec.CourseId == "7714" || sec.CourseId == "7715") && sec.HideInCatalog).Select(s => s.Id);

                // Reality Check to be sure some of the sections are flagged as hidden
                Assert.IsTrue(hiddenSections.Count() > 0);
                var nonhiddenSections = allSections.Where(sec => (sec.CourseId == "7714" || sec.CourseId == "7715") && !sec.HideInCatalog).Select(s => s.Id);
                var matchingSections = course2Page.CurrentPageItems.SelectMany(cp => cp.MatchingSectionIds).Distinct().ToList();

                Assert.AreEqual(specificSections.Count() - hiddenSections.Count(), matchingSections.Count());
                foreach (var sec in matchingSections)
                {
                    Assert.IsTrue(!hiddenSections.Contains(sec));
                    Assert.IsTrue(nonhiddenSections.Contains(sec));
                }
            }
        }

        [TestClass]
        public class Filters
        {
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IRuleRepository> ruleRepoMock;
            private IRuleRepository ruleRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private CourseService courseService;
            private Mock<IRequirementRepository> requirementsRepoMock;
            private IRequirementRepository requirementsRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<ICurrentUserFactory> userFactoryMock;
            private ICurrentUserFactory userFactory;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
            private IStudentReferenceDataRepository studentReferenceRepo;
            private Mock<IStudentConfigurationRepository> configurationRepoMock;
            private IStudentConfigurationRepository configurationRepo;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
            private IEnumerable<string> catalogSubjectCodes;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> catalogSubjects;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel> allAcademicLevels;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TopicCode> allTopicCodes;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CourseType> allCourseTypes;
            private ILogger logger;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public async void Initialize()
            {

                studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceRepo = studentReferenceRepoMock.Object;
                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                allSubjects = new TestSubjectRepository().Get();
                catalogSubjects = allSubjects.Where(s => s.ShowInCourseSearch == true);
                catalogSubjectCodes = catalogSubjects.Select(s => s.Code);
                allLocations = new TestLocationRepository().Get();
                allDepartments = new TestDepartmentRepository().Get();
                allAcademicLevels = new TestAcademicLevelRepository().GetAsync().Result;
                allTopicCodes = new TestTopicCodeRepository().Get();
                allCourseTypes = new TestCourseTypeRepository().Get();
                studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));
                referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
                referenceRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allLocations);
                referenceRepoMock.Setup(repo => repo.GetDepartmentsAsync(false)).Returns(Task.FromResult(allDepartments));
                studentReferenceRepoMock.Setup(repo => repo.GetAcademicLevelsAsync()).Returns(Task.FromResult(allAcademicLevels));
                studentReferenceRepoMock.Setup(repo => repo.GetCourseTypesAsync(false)).Returns(Task.FromResult(allCourseTypes));

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                allCourses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                courseRepoMock.Setup<Task<Domain.Student.Entities.Course>>(repo => repo.GetAsync(It.IsAny<string>())).Returns<string>((id) => Task.FromResult(allCourses.FirstOrDefault(c => c.Id == id)));
                catalogCourses = allCourses.Where(c => c.IsCurrent == true && catalogSubjectCodes.Contains(c.SubjectCode));

                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                ruleRepoMock = new Mock<IRuleRepository>();
                ruleRepo = ruleRepoMock.Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                allSections = new TestSectionRepository().GetAsync().Result;

                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(allSections));

                configurationRepoMock = new Mock<IStudentConfigurationRepository>();
                configurationRepo = configurationRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                userFactoryMock = new Mock<ICurrentUserFactory>();
                userFactory = userFactoryMock.Object;

                requirementsRepoMock = new Mock<IRequirementRepository>();
                requirementsRepo = requirementsRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                var coursePageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>()).Returns(coursePageAdapter);
                var courseSearch2PageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>()).Returns(courseSearch2PageAdapter);
                var corequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>()).Returns(corequisiteAdapter);
                var requisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>()).Returns(requisiteAdapter);
                var locationCycleAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.LocationCycleRestriction, LocationCycleRestriction>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.LocationCycleRestriction, LocationCycleRestriction>()).Returns(locationCycleAdapter);
                courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                    studentReferenceRepo, requirementsRepo, sectionRepoMock.Object, termRepoMock.Object,
                    ruleRepoMock.Object, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseRepoMock = null;
                courseRepo = null;
                referenceRepoMock = null;
                referenceRepo = null;
                adapterRegistryMock = null;
                adapterRegistry = null;
                courseService = null;
            }

            [TestMethod]
            public async Task SubjectFilterContainsOnlySubjectsIncludedInCatalog()
            {
                // Arrange -- blank criteria to get all courses and sections
                var criteria = new CourseSearchCriteria();

                // Act -- invoke search service. Use max in for page size to get all items at once
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- returns only subjects that are included in course search
                var catalogSubjectCodes = catalogSubjects.Select(s => s.Code);
                foreach (var subjectFilter in coursePage.Subjects)
                {
                    Assert.IsTrue(catalogSubjectCodes.Contains(subjectFilter.Value));
                }
                // Assert -- returned courses are only in subjects included in course search
                foreach (var pageItem in coursePage.CurrentPageItems)
                {
                    Assert.IsTrue(catalogSubjectCodes.Contains(pageItem.SubjectCode));
                }
            }

            [TestMethod]
            public async Task SubjectFilterReturnsAllCoursesForSubject()
            {
                // Arrange -- Set up page size and subject search
                var criteria = new CourseSearchCriteria();
                var subject = "MATH";
                var subjectList = new List<string> { subject };
                criteria.Subjects = subjectList;

                // Act -- Invoke Search service. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- returns all courses with given subject
                var origCount = catalogCourses.Where(c => c.SubjectCode == subject).Count();
                Assert.AreEqual(origCount, coursePage.CurrentPageItems.Count());
                Assert.IsTrue(coursePage.Subjects.Count() == 1);
            }

            [TestMethod]
            public async Task InactiveCoursesExcludedFromFilterResults()
            {
                // Arrange--set up subject search for DENT courses
                var criteria = new CourseSearchCriteria();
                var subject = "DENT";
                var subjectList = new List<string> { subject };
                criteria.Subjects = subjectList;

                // Act -- Invoke Search service. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- inactive courses excluded from results, active courses included
                Assert.AreEqual(0, coursePage.CurrentPageItems.Where(c => c.Id == "7705").Count());
                Assert.AreEqual(0, coursePage.CurrentPageItems.Where(c => c.Id == "7706").Count());
                Assert.AreEqual(1, coursePage.CurrentPageItems.Where(c => c.Id == "7704").Count());
            }

            [TestMethod]
            public async Task SubjectSearchSetsSelectedFlagInFilters()
            {
                // Arrange -- Set up page size and subject search
                var criteria = new CourseSearchCriteria();
                var subject = "MATH";
                var subjectList = new List<string> { subject };
                criteria.Subjects = subjectList;

                // Act -- Invoke Search service. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- returns "Selected" flag for requested subject
                Assert.IsTrue(coursePage.Subjects.Where(s => s.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.Subjects.Where(s => s.Selected == true).Select(x => x.Value).First() == "MATH");
            }

            [TestMethod]
            public async Task EmptySearchReturnsSubjectFilterDetailsForAllCourses()
            {
                // Arrange -- Set up empty search
                var pageSize = 10;
                var pageIndex = 2;
                var criteria = new CourseSearchCriteria();

                // Act -- Invoke Search service
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, pageSize, pageIndex);
#pragma warning restore 618

                // Assert
                // Subject filter details contain many subjects
                Assert.IsTrue(coursePage.Subjects.Count() > 13);
            }

            [TestMethod]
            public async Task SearchNoCoursesFoundReturnsEmptyCoursePageAndEmptyFilters()
            {
                // Arrange -- a subject that will yield zero courses
                var subject = "123*&%";
                var criteria = new CourseSearchCriteria();
                criteria.Subjects = new List<string> { subject };

                // Act -- Invoke search. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);

                // Assert -- no items found, verify item count and CoursePage values
                Assert.AreEqual(0, coursePage.CurrentPageItems.Count());
                Assert.IsTrue(coursePage is CoursePage);
                Assert.IsTrue(coursePage.CurrentPageIndex == 0);
                Assert.IsTrue(coursePage.TotalItems == 0);
                Assert.IsTrue(coursePage.Subjects.Count() >= 0);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task SearchAllowsRepositoryExceptionToFallThrough()
            {
                // Arrange
                courseRepoMock.Setup(repo => repo.GetAsync()).Throws(new Exception("Failure"));
                var subject = "HIST";
                var criteria = new CourseSearchCriteria();
                criteria.Subjects = new List<string>() { subject };

                // Act
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);
#pragma warning restore 618
            }

            [TestMethod]
            public async Task EmptySearchWithSubjectFilterReturnsFilteredCoursesAndAllSubjectFilters()
            {
                // Arrange -- Set up input subject filters for HIST and ENGL
                var historySubject = "HIST";
                var englishSubject = "ENGL";
                var criteria = new CourseSearchCriteria();
                criteria.Subjects = new List<string>() { historySubject, englishSubject };

                // Act -- Invoke search. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- correct number of courses found for specified subjects
                var historyCount = catalogCourses.Where(c => c.SubjectCode == historySubject).Count();
                var englishCount = catalogCourses.Where(c => c.SubjectCode == englishSubject).Count();
                var origCount = historyCount + englishCount;
                Assert.AreEqual(origCount, coursePage.CurrentPageItems.Count());
                // Assert -- Subject filter shows 2 selected subject
                Assert.IsTrue(coursePage.Subjects.Count() == 2);
            }

            [TestMethod]
            public async Task SectionDateRangeFilter_OnlyStartDate()
            {
                // Arrange - Set up course search criteria to get sections that within a date range - where many sections have this start and end date exactly.
                var startingDateRange = new DateTime(2014, 8, 23);
                var criteria = new CourseSearchCriteria() { SectionStartDate = startingDateRange };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert - start and end date filters returned returned

                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                // Assert - sections with corredt date selected and filter created
                var sectionsWithinRange = allSections.Where(s => (!s.FirstMeetingDate.HasValue || DateTime.Compare(s.FirstMeetingDate.Value, startingDateRange) >= 0) && catalogCourseList.Contains(s.CourseId));
                var expectedSections = sectionsWithinRange.Where(sec => sec.IsActive);
                var expectedItemCount = expectedSections.Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(expectedItemCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                Assert.AreEqual(expectedItemCount, coursePage.TotalItems);
                var matchingSections = coursePage.CurrentPageItems.SelectMany(cp => cp.MatchingSectionIds).Distinct().ToList();
                Assert.AreEqual(expectedSections.Count(), matchingSections.Count());
                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    foreach (var section in courseSections)
                    {
                        Assert.IsTrue(!section.FirstMeetingDate.HasValue || DateTime.Compare(section.FirstMeetingDate.Value, startingDateRange) >= 0);
                    }
                }
            }

            [TestMethod]
            public async Task SectionDateRangeFilter_BothStartAndEndDate()
            {
                // Arrange - Set up course search criteria to get sections that within a date range - This is wider than test above and should get 1 more section.

                var startingDateRange = new DateTime(2012, 6, 23);
                var endingDateRange = new DateTime(2012, 12, 12);
                var criteria = new CourseSearchCriteria() { SectionStartDate = startingDateRange, SectionEndDate = endingDateRange };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert - start and end date filters returned returned

                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                // Assert - sections with corredt date selected and filter created
                var sectionsWithinRange = allSections.Where(s => (!s.FirstMeetingDate.HasValue || (DateTime.Compare(s.FirstMeetingDate.Value, startingDateRange) >= 0 && DateTime.Compare(s.FirstMeetingDate.Value, endingDateRange) <= 0)) && (!s.LastMeetingDate.HasValue || DateTime.Compare(s.LastMeetingDate.Value, endingDateRange) <= 0) && catalogCourseList.Contains(s.CourseId));
                var expectedSections = sectionsWithinRange.Where(sec => sec.IsActive && !sec.HideInCatalog);
                var expectedItemCount = expectedSections.Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(expectedItemCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                Assert.AreEqual(expectedItemCount, coursePage.TotalItems);
                var matchingSections = coursePage.CurrentPageItems.SelectMany(cp => cp.MatchingSectionIds).Distinct().ToList();
                Assert.AreEqual(expectedSections.Count(), matchingSections.Count());
                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    foreach (var section in courseSections)
                    {
                        Assert.IsTrue(!section.FirstMeetingDate.HasValue || DateTime.Compare(section.FirstMeetingDate.Value, startingDateRange) >= 0);
                        Assert.IsTrue(!section.LastMeetingDate.HasValue || DateTime.Compare(section.LastMeetingDate.Value, endingDateRange) <= 0);
                    }
                }
            }

            [TestMethod]
            public async Task SectionDateRangeFilter_StartAndEndDate2()
            {
                // Arrange - Set up course search criteria to get sections that within a date range - This is narrower than test above and should only get 7 sections.

                var startingDateRange = new DateTime(2012, 9, 2);
                var endingDateRange = new DateTime(2012, 12, 12);
                var criteria = new CourseSearchCriteria() { SectionStartDate = startingDateRange, SectionEndDate = endingDateRange };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert - start and end date filters returned returned

                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                // Assert - sections with corredt date selected and filter created
                var sectionsWithinRange = allSections.Where(s => (!s.FirstMeetingDate.HasValue || (DateTime.Compare(s.FirstMeetingDate.Value, startingDateRange) >= 0 && DateTime.Compare(s.FirstMeetingDate.Value, endingDateRange) <= 0)) && (!s.LastMeetingDate.HasValue || DateTime.Compare(s.LastMeetingDate.Value, endingDateRange) <= 0) && catalogCourseList.Contains(s.CourseId));
                var expectedSections = sectionsWithinRange.Where(sec => sec.IsActive && !sec.HideInCatalog);
                var expectedItemCount = expectedSections.Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(expectedItemCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                Assert.AreEqual(expectedItemCount, coursePage.TotalItems);
                var matchingSections = coursePage.CurrentPageItems.SelectMany(cp => cp.MatchingSectionIds).Distinct().ToList();
                Assert.AreEqual(expectedSections.Count(), matchingSections.Count());
                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    foreach (var section in courseSections)
                    {
                        Assert.IsTrue(!section.FirstMeetingDate.HasValue || DateTime.Compare(section.FirstMeetingDate.Value, startingDateRange) >= 0);
                        Assert.IsTrue(!section.LastMeetingDate.HasValue || DateTime.Compare(section.LastMeetingDate.Value, endingDateRange) <= 0);
                    }
                }
            }

            [TestMethod]
            public async Task SectionDateRangeFilter_OnlyEndDate()
            {
                // Arrange - Set up course search criteria to get sections that within a date range - where many sections have this start and end date exactly.
                var endingDateRange = new DateTime(2014, 8, 22);
                var criteria = new CourseSearchCriteria() { SectionEndDate = endingDateRange };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert - start and end date filters returned returned

                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                // Assert - sections with corredt date selected and filter created
                var sectionsWithinRange = allSections.Where(s => (!s.LastMeetingDate.HasValue || DateTime.Compare(s.LastMeetingDate.Value, endingDateRange) <= 0) && catalogCourseList.Contains(s.CourseId));
                var expectedSections = sectionsWithinRange.Where(sec => sec.IsActive && !sec.HideInCatalog);
                var expectedItemCount = expectedSections.Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(expectedItemCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                Assert.AreEqual(expectedItemCount, coursePage.TotalItems);
                var matchingSections = coursePage.CurrentPageItems.SelectMany(cp => cp.MatchingSectionIds).Distinct().ToList();
                Assert.AreEqual(expectedSections.Count(), matchingSections.Count());
                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    foreach (var section in courseSections)
                    {
                        Assert.IsTrue(!section.LastMeetingDate.HasValue || DateTime.Compare(section.LastMeetingDate.Value, endingDateRange) <= 0);
                    }
                }
            }

            [TestMethod]
            public async Task SectionDateRangeFilter_SectionStartDateOnly()
            {
                // Arrange - Set up course search criteria to get sections that within a date range
                var startingDateRange = new DateTime(2012, 8, 23);
                var criteria = new CourseSearchCriteria() { SectionStartDate = startingDateRange };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert - start and end date filters returned returned
                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                // Assert - sections with corredt date selected and filter created
                var sectionCount = allSections.Where(s => DateTime.Compare(s.StartDate, startingDateRange) >= 0 && catalogCourseList.Contains(s.CourseId) && s.IsActive && !s.HideInCatalog).Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(sectionCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                Assert.AreEqual(sectionCount, coursePage.TotalItems);
                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    foreach (var section in courseSections)
                    {
                        Assert.IsTrue(DateTime.Compare(section.StartDate, startingDateRange) >= 0);
                    }
                }
            }

            [TestMethod]
            public async Task SectionDateRangeFilter_SectionEndDateOnly()
            {
                // Arrange - Set up course search criteria to get sections that within a date range
                var endingDateRange = new DateTime(2012, 12, 23);
                var criteria = new CourseSearchCriteria() { SectionEndDate = endingDateRange };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert - start and end date filters returned returned
                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                // Assert - sections with corredt date selected and filter created
                var sectionCount = allSections.Where(s => (!s.EndDate.HasValue || DateTime.Compare(s.EndDate.Value, endingDateRange) < 0) && catalogCourseList.Contains(s.CourseId) && s.IsActive).Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(sectionCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                Assert.AreEqual(sectionCount, coursePage.TotalItems);
                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    foreach (var section in courseSections)
                    {
                        Assert.IsTrue(!section.EndDate.HasValue || DateTime.Compare(section.EndDate.Value, endingDateRange) < 0);
                    }
                }
            }

            [TestMethod]
            public async Task AcademicLevelFilterCorrectlyLimitsSearchResults()
            {
                // Arrange -- Build academic level filter request
                var academicLevelCode = "GR";
                var criteria = new CourseSearchCriteria();
                criteria.AcademicLevels = new List<string>() { academicLevelCode };

                // Act -- Invoke Search. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- Verify correct number of items returned
                var courses = catalogCourses.Where(c => c.AcademicLevelCode == academicLevelCode);
                var origCount = courses.Count();
                Assert.AreEqual(origCount, coursePage.CurrentPageItems.Count());
                // Assert -- correct type returned
                Assert.IsTrue(coursePage is CoursePage);
                // Assert -- Correct number of acad level filters found
                // Assert.AreEqual(5, coursePage.AcademicLevels.Count()); //Assert used when filters built against search results
                Assert.AreEqual(1, coursePage.AcademicLevels.Count());
                // Assert -- Check correct value for UG courses
                Assert.AreEqual(courses.Count(), coursePage.AcademicLevels.Where(a => a.Selected == true).First().Count);
                // Assert -- other filters also built
                Assert.IsTrue(coursePage.Subjects.Count() > 1);
                // Assert -- Selected flag is set to true only for requested academic level, filter nonexistent for others
                Assert.IsTrue(coursePage.AcademicLevels.Where(a => a.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.AcademicLevels.Where(a => a.Selected == true).Select(x => x.Value).First() == academicLevelCode);

            }

            [TestMethod]
            public async Task CourseLevelFilterCorrectlyLimitsSearchResults()
            {
                // Arrange -- Set up page size and subject search
                var criteria = new CourseSearchCriteria();
                var courseLevel = "100";
                criteria.CourseLevels = new List<string> { courseLevel };

                // Act -- Invoke Search service. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- returns all courses with given course level
                var origCount = catalogCourses.SelectMany(c => c.CourseLevelCodes).Where(cl => cl == courseLevel).Count();
                Assert.AreEqual(origCount, coursePage.CurrentPageItems.Count());
                // Assert -- correct type returned
                Assert.IsTrue(coursePage is CoursePage);
                // Assert correct number of course level filters returned
                Assert.AreEqual(2, coursePage.CourseLevels.Count()); // One course has multiple course levels
                // Assert correct number of courses counted (at least 18 courses at level 100)
                var courseLevelCourses = catalogCourses.Where(c => c.CourseLevelCodes.Contains(courseLevel));
                Assert.AreEqual(courseLevelCourses.Count(), coursePage.CourseLevels.Where(c => c.Value == courseLevel).First().Count);
                // Assert Selected flag is set properly
                Assert.IsTrue(coursePage.CourseLevels.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.CourseLevels.Where(c => c.Selected == true).Select(x => x.Value).First() == courseLevel);
            }

            [TestMethod]
            public async Task CourseTypeFilterCorrectlyLimitsSearchResults()
            {
                // Arrange -- Set up page size and subject search
                var criteria = new CourseSearchCriteria();
                var courseTypeCode = "STND";
                criteria.CourseTypes = new List<string> { courseTypeCode };

                // Act -- Invoke Search service. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                CoursePage2 coursePage = await courseService.Search2Async(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- correct type returned
                Assert.IsTrue(coursePage is CoursePage2);
                // Assert correct number of course type filters returned
                Assert.AreEqual(2, coursePage.CourseTypes.Count()); // test repository sections have multiple course levels
                // Assert correct number of courses counted-- about half(even numbered) courses have sections with course levels
                Assert.IsTrue(coursePage.CourseTypes.Where(c => c.Value == courseTypeCode).First().Count >= 35);
                // Assert Selected flag is set properly
                Assert.IsTrue(coursePage.CourseTypes.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.CourseTypes.Where(c => c.Selected == true).Select(x => x.Value).First() == courseTypeCode);
            }

            [TestMethod]
            public async Task CourseTypeFilterExcludesType()
            {
                // Arrange -- Set up page size and subject search
                IEnumerable<Colleague.Domain.Student.Entities.CourseType> revisedCourseTypes = new List<Domain.Student.Entities.CourseType>() {
                    new Colleague.Domain.Student.Entities.CourseType("1467b380-68b1-4774-bfab-312de945ee89", "COOP", "Cooperative", true),
                    new Colleague.Domain.Student.Entities.CourseType("6a4db587-a6f6-42f7-acae-a36d49091a20", "STND", "Standard", true),
                    new Colleague.Domain.Student.Entities.CourseType("eb856a9a-cc84-42b7-9342-c21a9520f11a", "WR", "WorkExperience", false),
                    new Colleague.Domain.Student.Entities.CourseType("aa7ed6a6-c63c-4799-934e-52f6a5aa98bc", "VOC", "Vocational", true),
                    new Colleague.Domain.Student.Entities.CourseType("09f37ef0-34b7-41d1-870c-1e5058709af3", "RMED", "Remedial", true)
                };
                studentReferenceRepoMock.Setup(repo => repo.GetCourseTypesAsync(false)).Returns(Task.FromResult(revisedCourseTypes));

                var criteria = new CourseSearchCriteria();
                var courseTypeCode = "STND";
                criteria.CourseTypes = new List<string> { courseTypeCode };

                // Act -- Invoke Search service. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                CoursePage2 coursePage = await courseService.Search2Async(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- correct type returned
                Assert.IsTrue(coursePage is CoursePage2);
                // Assert correct number of course type filters returned
                Assert.AreEqual(1, coursePage.CourseTypes.Count()); // test repository sections have multiple course levels
                // Assert correct number of courses counted-- about half(even numbered) courses have sections with course levels
                Assert.IsTrue(coursePage.CourseTypes.Where(c => c.Value == courseTypeCode).First().Count >= 35);
                // Assert Selected flag is set properly
                Assert.IsTrue(coursePage.CourseTypes.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.CourseTypes.Where(c => c.Selected == true).Select(x => x.Value).First() == courseTypeCode);
            }

            [TestMethod]
            public async Task LocationFilterCorrectlyLimitsBothCoursesAndSections()
            {
                // Arrange - filter sections using location (only sections in the test repo have this location)
                var locations = new List<string>() { "NW" };
                var criteria = new CourseSearchCriteria() { Locations = locations };
                // Mock return of the relevant courses from the repository
                courseRepoMock.Setup(repo => repo.GetAsync("21")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "21").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("333")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "333").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("47")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "47").First()));

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert multiple location filters exist
                Assert.IsTrue(coursePage.Locations.Count() == 2); // Should have NW and SC in the locations
                // Assert Selected flag is set properly 
                Assert.IsTrue(coursePage.Locations.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.Locations.Where(c => c.Selected == true).Select(x => x.Value).First() == "NW");
                // Location code of NW is assigned to sections of courses with location SC (see TestSectionRepository in TestUtil project)
                Assert.IsTrue(coursePage.Locations.Where(c => c.Selected == true).Select(x => x.Count).First() >= 25);
                // Assert course id 21 is found in the results and is carrying its location codes.
                // This course has sections with NW location and the course has SC location, so it should be selected.
                Assert.AreEqual("21", coursePage.CurrentPageItems.Where(c => c.Id == "21").First().Id);
                var crs = coursePage.CurrentPageItems.Where(c => c.Id == "21").First();
                Assert.IsTrue(crs.LocationCodes.Count() > 0);
            }

            [TestMethod]
            public async Task LocationFilterWithSubjectFilterCorrectlyLimitsBothCoursesAndSections()
            {
                // Arrange - filter sections using location and subject. This will remove the BIOL course and sections from the results.
                var locations = new List<string>() { "NW" };
                var subjects = new List<string>() { "MATH" };
                var criteria = new CourseSearchCriteria() { Locations = locations, Subjects = subjects };
                // Mock return of the relevant courses from the repository
                courseRepoMock.Setup(repo => repo.GetAsync("21")).Returns(Task.FromResult(allCourses.Where(c => c.Id == "21").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("333")).Returns(Task.FromResult(allCourses.Where(c => c.Id == "333").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("47")).Returns(Task.FromResult(allCourses.Where(c => c.Id == "47").First()));

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert multiple location filters exist
                Assert.IsTrue(coursePage.Locations.Count() == 2); // Should have NW and SC in the locations
                // Assert Selected flag is set properly 
                Assert.IsTrue(coursePage.Locations.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.Locations.Where(c => c.Selected == true).Select(x => x.Value).First() == "NW");
                // Assert course id 21 is not found in the results
                Assert.AreEqual(null, coursePage.CurrentPageItems.Where(c => c.Id == "21").FirstOrDefault());
                Assert.AreEqual("333", coursePage.CurrentPageItems.Where(c => c.Id == "333").First().Id);
            }

            [TestMethod]
            public async Task FacultyFilterCorrectlyLimitsSearchResults()
            {
                // Arrange - filter sections using faculty
                var faculty = new List<string>() { "0000045", "0000053" };
                var criteria = new CourseSearchCriteria() { Faculty = faculty };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert Selected flag is set properly 
                Assert.IsTrue(coursePage.Faculty.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.Faculty.Where(c => c.Selected == true).Select(x => x.Value).First() == "0000045");
                // Assert a bunch of them found--Faculty Id 45 is assigned to one section per course.
                Assert.IsTrue(coursePage.Faculty.Where(c => c.Value == "0000045").Select(x => x.Count).First() >= 45);
            }


            [TestMethod]
            public async Task DayOfWeekFilterCorrectlyLimitsSearchResults()
            {
                // Arrange - filter sections using day of week
                // Passed in and out of page as string 0 through 6 (Sun - Sat)
                var daysOfWeek = new List<string>() { "1", "3" }; // Mon, Wed
                var criteria = new CourseSearchCriteria() { DaysOfWeek = daysOfWeek };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert multiple DayOfWeek filters exist
                Assert.IsTrue(coursePage.DaysOfWeek.Count() == 4); // some courses have MWTH
                // Assert Selected flag is set properly 
                Assert.IsTrue(coursePage.DaysOfWeek.Where(c => c.Selected == true).Count() == 2);
                Assert.IsTrue(coursePage.DaysOfWeek.Where(c => c.Selected == true).Select(x => x.Value).First() == "1");
                // Check Monday course count
                Assert.IsTrue(coursePage.DaysOfWeek.Where(c => c.Selected == true).Select(x => x.Count).First() >= 25);
                // Check Wednesday course count
                Assert.IsTrue(coursePage.DaysOfWeek.Where(c => c.Value == "3").Select(x => x.Count).First() >= 25);
            }

            [TestMethod]
            public async Task TimeFilterCorrectlyLimitsSearchResults()
            {
                // Arrange - filter sections using time range
                var earliestTimeSpan = new TimeSpan(10, 00, 00);
                var latestTimeSpan = new TimeSpan(12, 00, 00);
                var criteria = new CourseSearchCriteria() { EarliestTime = (int)earliestTimeSpan.TotalMinutes, LatestTime = (int)latestTimeSpan.TotalMinutes };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    var timeCollection = from sec in courseSections
                                         from mtg in sec.Meetings
                                         select new { mtg.StartTime, mtg.EndTime, sec.Id };
                    foreach (var mtgTime in timeCollection)
                    {
                        // If no start/end time given, don't try to compare against the criteria earliest/latest time
                        if (mtgTime.StartTime != null)
                        {
                            DateTimeOffset startTime = mtgTime.StartTime.GetValueOrDefault();
                            Assert.IsTrue(startTime.DateTime.TimeOfDay.TotalMinutes >= criteria.EarliestTime);
                        }
                        if (mtgTime.EndTime != null)
                        {
                            DateTimeOffset endTime = mtgTime.EndTime.GetValueOrDefault();
                            Assert.IsTrue(endTime.DateTime.TimeOfDay.TotalMinutes <= criteria.LatestTime);
                        }
                    }
                }
            }

            [TestMethod]
            public async Task TimeFilterCorrectlyLimitsSearchResults_StartsAtTime_Only()
            {
                // Arrange - filter set time range by starts at time
                var startsAtTime = "06:00 AM";
                var startsAtTimeMinutes = 360;
                var criteria = new CourseSearchCriteria() { StartsAtTime = startsAtTime };

                // Act - call course search
                #pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
                #pragma warning restore 618

                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    var timeCollection = from sec in courseSections
                                         from mtg in sec.Meetings
                                         select new { mtg.StartTime, mtg.EndTime, sec.Id };
                    foreach (var mtgTime in timeCollection)
                    {
                        if (mtgTime.StartTime != null)
                        {
                            DateTimeOffset startTime = mtgTime.StartTime.GetValueOrDefault();
                            Assert.IsTrue(startTime.DateTime.TimeOfDay.TotalMinutes >= startsAtTimeMinutes);
                        }
                    }
                }
            }

            [TestMethod]
            public async Task TimeFilterCorrectlyLimitsSearchResults_EndsByTime_Only()
            {
                // Arrange - filter set time range by ends by time
                var endsByTime = "06:00 PM";
                var endsByTimeMinutes = 1080;
                var criteria = new CourseSearchCriteria() { EndsByTime = endsByTime };

                // Act - call course search
                #pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
                #pragma warning restore 618

                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    var timeCollection = from sec in courseSections
                                         from mtg in sec.Meetings
                                         select new { mtg.StartTime, mtg.EndTime, sec.Id };
                    foreach (var mtgTime in timeCollection)
                    {
                        if (mtgTime.EndTime != null)
                        {
                            DateTimeOffset endTime = mtgTime.EndTime.GetValueOrDefault();
                            Assert.IsTrue(endTime.DateTime.TimeOfDay.TotalMinutes <= endsByTimeMinutes);
                        }
                    }
                }
            }

            [TestMethod]
            public async Task TimeFilterCorrectlyLimitsSearchResults_Both_StartsAtTime_And_EndsByTime()
            {
                // Arrange - filter set time range by starts at and ends by time
                var startsAtTime = "06:00 AM";
                var startsAtTimeMinutes = 360;
                var endsByTime = "06:00 PM";
                var endsByTimeMinutes = 1080;
                var criteria = new CourseSearchCriteria() { StartsAtTime = startsAtTime, EndsByTime = endsByTime };

                // Act - call course search
                #pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
                #pragma warning restore 618

                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    var timeCollection = from sec in courseSections
                                         from mtg in sec.Meetings
                                         select new { mtg.StartTime, mtg.EndTime, sec.Id };
                    foreach (var mtgTime in timeCollection)
                    {
                        if (mtgTime.StartTime != null)
                        {
                            DateTimeOffset startTime = mtgTime.StartTime.GetValueOrDefault();
                            Assert.IsTrue(startTime.DateTime.TimeOfDay.TotalMinutes >= startsAtTimeMinutes);
                        }
                        if (mtgTime.EndTime != null)
                        {
                            DateTimeOffset endTime = mtgTime.EndTime.GetValueOrDefault();
                            Assert.IsTrue(endTime.DateTime.TimeOfDay.TotalMinutes <= endsByTimeMinutes);
                        }
                    }
                }
            }


            [TestMethod]
            public async Task TimeFilterReturnsFewSearchResults()
            {
                // Arrange - filter sections using time range--There are only a few items that will meet this request
                var earliestTimeSpan = new TimeSpan(9, 00, 00);
                var latestTimeSpan = new TimeSpan(9, 30, 00);
                var criteria = new CourseSearchCriteria() { EarliestTime = (int)earliestTimeSpan.TotalMinutes, LatestTime = (int)latestTimeSpan.TotalMinutes };

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Verify that the sections included take place during the specified time span
                foreach (var item in coursePage.CurrentPageItems)
                {
                    // Get each section found for this course
                    var courseSections = (from sectionId in item.MatchingSectionIds
                                          join sec in allSections
                                          on sectionId equals sec.Id into joinSections
                                          from section in joinSections
                                          select section).ToList();
                    var timeCollection = from sec in courseSections
                                         from mtg in sec.Meetings
                                         select new { mtg.StartTime, mtg.EndTime, sec.Id };
                    foreach (var mtgTime in timeCollection)
                    {
                        DateTimeOffset startTime = mtgTime.StartTime.GetValueOrDefault();
                        DateTimeOffset endTime = mtgTime.EndTime.GetValueOrDefault();
                        Assert.IsTrue(endTime.DateTime.TimeOfDay.TotalMinutes <= criteria.LatestTime);
                        Assert.IsTrue(startTime.DateTime.TimeOfDay.TotalMinutes >= criteria.EarliestTime);
                    }
                }
            }

            [TestMethod]
            public async Task OnlineCategoryFilterIsEmpty()
            {
                // Arrange - Set up course search criteria to get all sections without regard for online category
                var criteria = new CourseSearchCriteria() { OnlineCategories = new List<string>() };

                // Act - call course search
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);

                // Assert - all courses included in the result
                Assert.AreEqual(catalogCourses.Count(), coursePage.CurrentPageItems.Count());
                // There are three online category filters
                Assert.AreEqual(3, coursePage.OnlineCategories.Count());
                // There should be a filter for every online category
                var onlineFilter = coursePage.OnlineCategories.Where(f => f.Value == Domain.Student.Entities.OnlineCategory.Online.ToString()).FirstOrDefault();
                Assert.IsNotNull(onlineFilter);
                var notOnlineFilter = coursePage.OnlineCategories.Where(f => f.Value == Domain.Student.Entities.OnlineCategory.NotOnline.ToString()).FirstOrDefault();
                Assert.IsNotNull(notOnlineFilter);
                var hybridFilter = coursePage.OnlineCategories.Where(f => f.Value == Domain.Student.Entities.OnlineCategory.Hybrid.ToString()).FirstOrDefault();
                Assert.IsNotNull(hybridFilter);
            }

            [TestMethod]
            public async Task OnlineCategoryFilterIsOnline()
            {
                // Arrange - Set up course search criteria to get sections that have online instruction only
                var onlineCategory = Domain.Student.Entities.OnlineCategory.Online.ToString();
                var criteria = new CourseSearchCriteria() { OnlineCategories = new List<string>() { onlineCategory } };

                // Act - call course search
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);

                // Assert - One online category filter returned
                Assert.AreEqual(1, coursePage.OnlineCategories.Count());
                // Assert - Online sections were selected
                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                var sectionCount = allSections.Where(s => s.OnlineCategory == Domain.Student.Entities.OnlineCategory.Online && catalogCourseList.Contains(s.CourseId)).Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(sectionCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                var filter = coursePage.OnlineCategories.Where(f => f.Value == onlineCategory).FirstOrDefault();
                Assert.IsNotNull(filter);
                Assert.AreEqual(sectionCount, filter.Count);
                Assert.AreEqual(onlineCategory.ToString(), filter.Value);
                Assert.IsTrue(filter.Selected);
            }

            [TestMethod]
            public async Task OnlineCategoryFilterIsNotOnline()
            {
                // Arrange - Set up course search criteria to get sections that have online instruction only
                var onlineCategory = Domain.Student.Entities.OnlineCategory.NotOnline.ToString();
                var criteria = new CourseSearchCriteria() { OnlineCategories = new List<string>() { onlineCategory } };

                // Act - call course search
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);

                // Assert - One online category filter returned
                Assert.AreEqual(1, coursePage.OnlineCategories.Count());
                // Assert - NotOnline sections were selected
                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                var sectionCount = allSections.Where(s => s.OnlineCategory == Domain.Student.Entities.OnlineCategory.NotOnline && catalogCourseList.Contains(s.CourseId)).Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(sectionCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                var filter = coursePage.OnlineCategories.Where(f => f.Value == onlineCategory).FirstOrDefault();
                Assert.IsNotNull(filter);
                Assert.AreEqual(sectionCount, filter.Count);
                Assert.AreEqual(onlineCategory.ToString(), filter.Value);
                Assert.IsTrue(filter.Selected);
            }

            [TestMethod]
            public async Task OnlineCategoryFilterIsHybrid()
            {
                // Arrange - Set up course search criteria to get sections that have online instruction only
                var onlineCategory = Domain.Student.Entities.OnlineCategory.Hybrid.ToString();
                var criteria = new CourseSearchCriteria() { OnlineCategories = new List<string>() { onlineCategory } };

                // Act - call course search
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);

                // Assert - One online category filter returned
                Assert.AreEqual(1, coursePage.OnlineCategories.Count());
                // Assert - Online sections were selected
                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                var sectionCount = allSections.Where(s => s.OnlineCategory == Domain.Student.Entities.OnlineCategory.Hybrid && catalogCourseList.Contains(s.CourseId)).Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(sectionCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                var filter = coursePage.OnlineCategories.Where(f => f.Value == onlineCategory).FirstOrDefault();
                Assert.IsNotNull(filter);
                Assert.AreEqual(sectionCount, filter.Count);
                Assert.AreEqual(onlineCategory.ToString(), filter.Value);
                Assert.IsTrue(filter.Selected);
            }

            [TestMethod]
            public async Task InvalidOnlineCategoryFilterIgnored()
            {
                // Arrange - Set up course search criteria to get sections that have online instruction only
                var onlineCategory = Domain.Student.Entities.OnlineCategory.Hybrid.ToString();
                var criteria = new CourseSearchCriteria() { OnlineCategories = new List<string>() { onlineCategory, "xyz" } };

                // Act - call course search
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);

                // Assert - One online category filter returned
                Assert.AreEqual(1, coursePage.OnlineCategories.Count());
                // Assert - Online sections were selected
                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                var sectionCount = allSections.Where(s => s.OnlineCategory == Domain.Student.Entities.OnlineCategory.Hybrid && catalogCourseList.Contains(s.CourseId)).Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(sectionCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                var filter = coursePage.OnlineCategories.Where(f => f.Value == onlineCategory).FirstOrDefault();
                Assert.IsNotNull(filter);
                Assert.AreEqual(sectionCount, filter.Count);
                Assert.AreEqual(onlineCategory.ToString(), filter.Value);
                Assert.IsTrue(filter.Selected);
            }

            [TestMethod]
            public async Task OnlineCategoryFilterIsOnlineAndHybrid()
            {
                // Arrange - Set up course search criteria to get sections that have online instruction only
                var hybridOnlineCategory = Domain.Student.Entities.OnlineCategory.Hybrid.ToString();
                var onlineOnlineCategory = Domain.Student.Entities.OnlineCategory.Online.ToString();
                var criteria = new CourseSearchCriteria() { OnlineCategories = new List<string>() { hybridOnlineCategory, onlineOnlineCategory } };

                // Act - call course search
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);

                // Assert - Two online category filters returned returned
                Assert.AreEqual(2, coursePage.OnlineCategories.Count());
                var catalogCourseList = catalogCourses.Select(cc => cc.Id);
                // Assert - Hybrid sections were selected and filter created
                var sectionCount = allSections.Where(s => s.OnlineCategory == Domain.Student.Entities.OnlineCategory.Hybrid && catalogCourseList.Contains(s.CourseId)).Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(sectionCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                var filter = coursePage.OnlineCategories.Where(f => f.Value == hybridOnlineCategory).FirstOrDefault();
                Assert.IsNotNull(filter);
                Assert.AreEqual(sectionCount, filter.Count);
                Assert.AreEqual(hybridOnlineCategory, filter.Value);
                Assert.IsTrue(filter.Selected);

                sectionCount = allSections.Where(s => s.OnlineCategory == Domain.Student.Entities.OnlineCategory.Online && catalogCourseList.Contains(s.CourseId)).Select(s => s.CourseId).Distinct().Count();
                Assert.IsTrue(sectionCount > 0); // just to verify something hasn't gone terribly wrong with the test data repository
                // Assert the filter is built properly, with the name of the online category, the correct section count, and the selected flag
                filter = coursePage.OnlineCategories.Where(f => f.Value == onlineOnlineCategory).FirstOrDefault();
                Assert.IsNotNull(filter);
                Assert.AreEqual(sectionCount, filter.Count);
                Assert.AreEqual(onlineOnlineCategory, filter.Value);
                Assert.IsTrue(filter.Selected);

            }

            [TestMethod]
            public async Task TopicCodeFilterCorrectlyLimitsBothCoursesAndSections()
            {
                // Arrange - filter sections using topic code (only sections in the test repo have this topic code)
                var topicCodes = new List<string>() { "T3" };
                var criteria = new CourseSearchCriteria() { TopicCodes = topicCodes };
                // Mock return of the relevant courses from the repository
                // The ones I need to get are those that do not have a T3 topic code but that has a section that does.  Because these will need to be added back into the list.
                courseRepoMock.Setup(repo => repo.GetAsync("21")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "21").First()));
                // 7701 has a course topic code of T2 but sections have topic code of T3
                courseRepoMock.Setup(repo => repo.GetAsync("7701")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7701").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("7703")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7703").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("7704")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7704").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("186")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "186").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("87")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "87").First()));

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert filters exist - There should be a filter for T3 (with 7 courses) that is selected, and then 1 for T2 with 3 courses, and then 1 with "" with 3 courses
                //                        This is because course's 186, 87 and 21 do not have a topic code on the course but each have sections with that topic code
                //                        (Something that cannot happen with the location filters because sections can only have a location that is on the course.)
                Assert.IsTrue(coursePage.TopicCodes.Count() == 3);
                // Assert Selected flag is set properly 
                Assert.IsTrue(coursePage.TopicCodes.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.TopicCodes.Where(c => c.Selected == true).Select(x => x.Value).First() == "T3");
                // Topic code of T3 is assigned to sections of courses with topic code T2 (see TestSectionRepository in TestUtil project)
                Assert.IsTrue(coursePage.TopicCodes.Where(c => c.Selected == true).Select(x => x.Count).First() == 7);
                // Assert course id 333 (a course that has topic code T3 but no sections with it) is found in the results and is carrying its topic code.
                // Course 333 has the topic code but no sections with T3 topic code; it should be selected.
                Assert.AreEqual("333", coursePage.CurrentPageItems.Where(c => c.Id == "333").First().Id);
                var crs = coursePage.CurrentPageItems.Where(c => c.Id == "333").First();
                // Course 7701 has T2 topic code but has sections with T3 topic code; it should be selected.
                Assert.AreEqual("7701", coursePage.CurrentPageItems.Where(c => c.Id == "7701").First().Id);
                // Course 21 has no topic code but has sections with T3 topic code; it should be selected.
                Assert.AreEqual("21", coursePage.CurrentPageItems.Where(c => c.Id == "21").First().Id);
            }

            [TestMethod]
            public async Task TopicCodeFilterWithSubjectFilterCorrectlyLimitsBothCoursesAndSections()
            {
                // Arrange - filter sections using topic code and subject. Instead of 7 courses we will only end up with 2 courses and their sections.
                var topicCodes = new List<string>() { "T3" };
                var subjects = new List<string>() { "MATH" };
                var criteria = new CourseSearchCriteria() { TopicCodes = topicCodes, Subjects = subjects };
                // Mock return of the relevant courses from the repository
                // The ones I need to get are those that do not have a T3 topic code but that has a section that does.  Because these will need to be added back into the list.
                courseRepoMock.Setup(repo => repo.GetAsync("21")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "21").First()));
                // 7701 has a course topic code of T2 but sections have topic code of T3
                courseRepoMock.Setup(repo => repo.GetAsync("7701")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7701").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("7703")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7703").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("7704")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7704").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("186")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "186").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("87")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "87").First()));

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert filters exist
                Assert.IsTrue(coursePage.TopicCodes.Count() == 2);
                // Assert Selected flag is set properly 
                Assert.IsTrue(coursePage.TopicCodes.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.TopicCodes.Where(c => c.Selected == true).Select(x => x.Value).First() == "T3");
                Assert.IsTrue(coursePage.Subjects.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.Subjects.Where(c => c.Selected == true).Select(x => x.Value).First() == "MATH");
                // Topic code of T3 is assigned to sections of courses with topic code T2 (see TestSectionRepository in TestUtil project)
                Assert.IsTrue(coursePage.TopicCodes.Where(c => c.Selected == true).Select(x => x.Count).First() == 2);
                // Assert course id 333 (a course that has topic code T3 but no sections with it) is found in the results and is carrying its topic code.
                // Course 333 has the topic code but no sections with T3 topic code; it should be selected.
                Assert.AreEqual("333", coursePage.CurrentPageItems.Where(c => c.Id == "333").First().Id);
                var crs = coursePage.CurrentPageItems.Where(c => c.Id == "333").First();
                // Course 7701 should no longer be in the results.
                Assert.AreEqual(0, coursePage.CurrentPageItems.Where(c => c.Id == "7701").Count());
                // Course 186 has no topic code but has sections with T3 topic code; it should be selected.
                Assert.AreEqual("186", coursePage.CurrentPageItems.Where(c => c.Id == "186").First().Id);

                // Verify the results more generically
                // Either the course must have both a matching location and a matching topic code or its sections must.
                // Get all course in the result
                var courseIds = coursePage.CurrentPageItems.Select(c => c.Id).ToList();
                var courses = new TestCourseRepository().GetAsync(courseIds).Result;
                foreach (var course in courses)
                {
                    if (course.TopicCode != "T3")
                    {
                        // If the course doesn't have the right topic make sure it has matching sections in that case and all of those have the right topic code. 
                        var pageItem = coursePage.CurrentPageItems.Where(c => c.Id == course.Id).FirstOrDefault();
                        Assert.IsTrue(pageItem.MatchingSectionIds.Count() > 0);
                        foreach (var sectionId in pageItem.MatchingSectionIds)
                        {
                            var section = new TestSectionRepository().GetSectionAsync(sectionId).Result;
                            Assert.IsTrue(section.TopicCode == "T3");
                        }
                    }
                }
            }

            [TestMethod]
            public async Task TopicCodeFilterWithLocationFilterCorrectlyLimitsBothCoursesAndSections()
            {
                // Arrange - filter sections using topic code and subject. Instead of 7 courses we will only end up with 2 courses and their sections.
                var topicCodes = new List<string>() { "T3" };
                var locations = new List<string>() { "NW" };
                var criteria = new CourseSearchCriteria() { TopicCodes = topicCodes, Locations = locations };
                // Mock return of the relevant courses from the repository
                // The ones I need to get are those that do not have a T3 topic code but that has a section that does.  Because these will need to be added back into the list.
                courseRepoMock.Setup(repo => repo.GetAsync("21")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "21").First()));
                // 7701 has a course topic code of T2 but sections have topic code of T3
                courseRepoMock.Setup(repo => repo.GetAsync("7701")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7701").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("7703")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7703").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("7704")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7704").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("186")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "186").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("87")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "87").First()));

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert filters exist
                Assert.IsTrue(coursePage.TopicCodes.Count() == 3);
                // Assert Selected flag is set properly 
                Assert.IsTrue(coursePage.TopicCodes.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.TopicCodes.Where(c => c.Selected == true).Select(x => x.Value).First() == "T3");
                Assert.IsTrue(coursePage.Locations.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.Locations.Where(c => c.Selected == true).Select(x => x.Value).First() == "NW");
                Assert.IsTrue(coursePage.TopicCodes.Where(c => c.Selected == true).Select(x => x.Count).First() == 6);
                // Assert course id 333 is NOT in the list.
                // In this case course 333 has topic code of T3 but location of SC and it has sections with NW but that don't have a topic of T3.
                // Therefore it does not meet both filters and should not be in the list. 
                Assert.AreEqual(0, coursePage.CurrentPageItems.Where(c => c.Id == "333").Count());
                // Course 7701 should be in the results because while the course has a T2 topic and no location, it has some sections with T3 and location NW.
                Assert.AreEqual(1, coursePage.CurrentPageItems.Where(c => c.Id == "7701").Count());
                // Course 186 has no topic code but has  some sections with T3 topic code and NW location; it should be selected.
                Assert.AreEqual("186", coursePage.CurrentPageItems.Where(c => c.Id == "186").First().Id);

                // Verify the results - Either the course must have both a matching location and a matching topic code or its sections must.
                // Get all course in the result
                var courseIds = coursePage.CurrentPageItems.Select(c => c.Id).ToList();
                var courses = new TestCourseRepository().GetAsync(courseIds).Result;
                foreach (var course in courses)
                {
                    if (course.TopicCode != "T3" || !course.LocationCodes.Contains("NW"))
                    {
                        // If the course doesn't have the right topic and location make sure all its matching sections do.
                        var pageItem = coursePage.CurrentPageItems.Where(c => c.Id == course.Id).FirstOrDefault();
                        Assert.IsTrue(pageItem.MatchingSectionIds.Count() > 0);
                        foreach (var sectionId in pageItem.MatchingSectionIds)
                        {
                            var section = new TestSectionRepository().GetSectionAsync(sectionId).Result;
                            Assert.IsTrue(section.Location == "NW");
                            Assert.IsTrue(section.TopicCode == "T3");
                        }
                    }
                }
            }


            [TestMethod]
            public async Task AcademicLevelFilterCorrectlyLimitsBothCoursesAndSections()
            {
                // Arrange - filter sections using academic level (only sections in the test repo have this code)
                var academicLevels = new List<string>() { "A1" };
                var criteria = new CourseSearchCriteria() { AcademicLevels = academicLevels };
                // Mock return of the relevant courses from the repository
                // The ones I need to get are those that do not have a A1 academic level but that has a section that does.  Because these will need to be added back into the list.
                // course 21 and all its sections have an academic level of A1
                courseRepoMock.Setup(repo => repo.GetAsync("21")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "21").First()));
                // Courses 7701, 7703 and 7704 have a course academic level of UG but sections have academic level of A1
                courseRepoMock.Setup(repo => repo.GetAsync("7701")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7701").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("7703")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7703").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("7704")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "7704").First()));
                // Courses 186 and 87 and their sections have academic level of UG
                courseRepoMock.Setup(repo => repo.GetAsync("186")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "186").First()));
                courseRepoMock.Setup(repo => repo.GetAsync("87")).Returns(Task.FromResult<Domain.Student.Entities.Course>(allCourses.Where(c => c.Id == "87").First()));

                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert filters exist - There should be a filter for A1 (with 4 courses) that is selected and also a UG filter 
                // because the other courses have a level of UG even though sections have A1.
                Assert.IsTrue(coursePage.AcademicLevels.Count() == 2);
                // Assert Selected flag is set properly 
                Assert.IsTrue(coursePage.AcademicLevels.Where(c => c.Selected == true).Count() == 1);
                Assert.IsTrue(coursePage.AcademicLevels.Where(c => c.Selected == true).Select(x => x.Value).First() == "A1");
                // Academic Level A1 is assigned to sections of courses with academic level UG
                var crstest = coursePage.AcademicLevels.Where(c => c.Selected == true).FirstOrDefault();
                Assert.IsTrue(coursePage.AcademicLevels.Where(c => c.Selected == true).Select(x => x.Count).First() == 1);
                // Course 7701 has UG academic level but has sections with A1 academic level; it should be selected.
                Assert.AreEqual("7701", coursePage.CurrentPageItems.Where(c => c.Id == "7701").First().Id);
                // Course 21 has A1 academic level AND sections with A1 academic level code; it should be selected.
                Assert.AreEqual("21", coursePage.CurrentPageItems.Where(c => c.Id == "21").First().Id);
            }

            [TestMethod]
            public async Task OpenFilterCorrectlyLimitsSections()
            {
                // Arrange - filter sections using academic level (only sections in the test repo have this code)
                var criteria = new CourseSearchCriteria() { OpenSections = true };

                // Section 1, with capacity and with some registered student - OPEN
                var openSection1 = new Ellucian.Colleague.Domain.Student.Entities.Section("1", "110", "01", DateTime.Now.AddDays(-100), 3.0m, null, "Section 1 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                openSection1.TermId = "2013/SP";
                var openSeats1 = new Domain.Student.Entities.SectionSeats("1");
                openSeats1.SectionCapacity = 5;
                openSeats1.ActiveStudentIds.Add("student1");
                openSeats1.ActiveStudentIds.Add("student2");
                // Section 2, with unlimited capacity and sme registered students - OPEN
                var openSection2 = new Ellucian.Colleague.Domain.Student.Entities.Section("2", "110", "02", DateTime.Now.AddDays(-100), 3.0m, null, "Section 2 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                openSection2.TermId = "2013/SP";
                var openSeats2 = new Domain.Student.Entities.SectionSeats("2");
                openSeats2.ActiveStudentIds.Add("student1");
                openSeats2.ActiveStudentIds.Add("student2");
                // Section 3, with no student's registered and no capacity - OPEN
                var openSection3 = new Ellucian.Colleague.Domain.Student.Entities.Section("3", "110", "03", DateTime.Now.AddDays(-100), 3.0m, null, "Section 3 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                openSection3.TermId = "2013/SP";
                var openSeats3 = new Domain.Student.Entities.SectionSeats("3");
                // Section 4, with capacity but no student's registered - OPEN
                var openSection4 = new Ellucian.Colleague.Domain.Student.Entities.Section("4", "110", "04", DateTime.Now.AddDays(-100), 3.0m, null, "Section 4 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                openSection4.TermId = "2013/SP";
                var openSeats4 = new Domain.Student.Entities.SectionSeats("4");
                openSeats4.SectionCapacity = 5;
                // Section 5, with capacity and all seats taken - CLOSED
                var closedSection5 = new Ellucian.Colleague.Domain.Student.Entities.Section("5", "110", "05", DateTime.Now.AddDays(-100), 3.0m, null, "Section 5 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                closedSection5.TermId = "2013/SP";
                var openSeats5 = new Domain.Student.Entities.SectionSeats("5");
                openSeats5.SectionCapacity = 5;
                openSeats5.ActiveStudentIds.Add("student1");
                openSeats5.ActiveStudentIds.Add("student2");
                openSeats5.ActiveStudentIds.Add("student3");
                openSeats5.ActiveStudentIds.Add("student4");
                openSeats5.ActiveStudentIds.Add("student5");
                // Section 6, over capacity - CLOSED
                var closedSection6 = new Ellucian.Colleague.Domain.Student.Entities.Section("6", "110", "06", DateTime.Now.AddDays(-100), 3.0m, null, "Section 6 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                closedSection6.TermId = "2013/SP";
                var openSeats6 = new Domain.Student.Entities.SectionSeats("6");
                openSeats6.SectionCapacity = 5;
                openSeats6.ActiveStudentIds.Add("student1");
                openSeats6.ActiveStudentIds.Add("student2");
                openSeats6.ActiveStudentIds.Add("student3");
                openSeats6.ActiveStudentIds.Add("student4");
                openSeats6.ActiveStudentIds.Add("student5");
                openSeats6.ActiveStudentIds.Add("student6");
                // Section 7, seat available but waitlist exists - CLOSED
                var closedSection7 = new Ellucian.Colleague.Domain.Student.Entities.Section("7", "110", "07", DateTime.Now.AddDays(-100), 3.0m, null, "Section 7 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                closedSection7.TermId = "2013/SP";
                var openSeats7 = new Domain.Student.Entities.SectionSeats("7");
                openSeats7.SectionCapacity = 3;
                openSeats7.ActiveStudentIds.Add("student1");
                openSeats7.ActiveStudentIds.Add("student2");
                openSeats7.NumberOnWaitlist = 1;
                // Section 8, between registered students and reserved seats class is closed - CLOSED
                var closedSection8 = new Ellucian.Colleague.Domain.Student.Entities.Section("8", "110", "08", DateTime.Now.AddDays(-100), 3.0m, null, "Section 8 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                closedSection8.TermId = "2013/SP";
                var openSeats8 = new Domain.Student.Entities.SectionSeats("8");
                openSeats8.SectionCapacity = 5;
                openSeats8.ActiveStudentIds.Add("student1");
                openSeats8.ActiveStudentIds.Add("student2");
                openSeats8.ReservedSeats = 3;
                // Section 9 , it is open itself, but its cross listsed sections have reached global capacity so it is really closed
                var closedSection9 = new Ellucian.Colleague.Domain.Student.Entities.Section("9", "110", "09", DateTime.Now.AddDays(-100), 3.0m, null, "Section 9 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                closedSection9.TermId = "2013/SP";
                var openSeats9 = new Domain.Student.Entities.SectionSeats("9");
                openSeats9.SectionCapacity = 5;
                openSeats9.ActiveStudentIds.Add("student1");
                openSeats9.ActiveStudentIds.Add("student2");
                openSeats9.GlobalCapacity = 7;
                openSeats9.CrossListedSections.Add(openSeats5);
                // Section 10 , full with waitlist
                var closedSection10 = new Ellucian.Colleague.Domain.Student.Entities.Section("10", "110", "09", DateTime.Now.AddDays(-100), 3.0m, null, "Section 9 Title", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "Freshman" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Now.AddDays(-10)) }, true, true, true, true, false, false);
                closedSection10.TermId = "2013/SP";
                var openSeats10 = new Domain.Student.Entities.SectionSeats("10");
                openSeats10.SectionCapacity = 2;
                openSeats10.ActiveStudentIds.Add("student1");
                openSeats10.ActiveStudentIds.Add("student2");
                openSeats10.NumberOnWaitlist = 3;
                IEnumerable<Domain.Student.Entities.Section> testSections = new List<Domain.Student.Entities.Section>() { openSection1, openSection2, openSection3, openSection4, closedSection5, closedSection6, closedSection7, closedSection8, closedSection9, closedSection10 };
                List<Domain.Student.Entities.SectionSeats> testSectionSeats = new List<Domain.Student.Entities.SectionSeats>() { openSeats1, openSeats2, openSeats3, openSeats4, openSeats5, openSeats6, openSeats7, openSeats8, openSeats9, openSeats10 };
                var testSectionSeatsDict = testSectionSeats.ToDictionary(x => x.Id, y => y);
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(testSections));
                IEnumerable<string> testSectionIds = testSections.Select(s => s.Id).ToList();
                sectionRepoMock.Setup(repo => repo.GetSectionsSeatsAsync(testSectionIds)).Returns(Task.FromResult(testSectionSeatsDict));
                // Act - call course search
#pragma warning disable 618
                var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                // Assert filters exist - There should be a filter for OpenSections that is selected 
                Assert.IsTrue(coursePage.OpenSections.Count == 4);

            }


        }

        [TestClass]
        public class SearchCourseByKeyword
        {
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IRuleRepository> ruleRepoMock;
            private IRuleRepository ruleRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private CourseService courseService;
            private Mock<IRequirementRepository> requirementsRepoMock;
            private IRequirementRepository requirementsRepo;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
            private IStudentReferenceDataRepository studentReferenceRepo;
            private Mock<IStudentConfigurationRepository> configurationRepoMock;
            private IStudentConfigurationRepository configurationRepo;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> catalogSubjects;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
            private ILogger logger;
            private Mock<ICurrentUserFactory> userFactoryMock;
            private ICurrentUserFactory userFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public async void Initialize()
            {
                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                allSubjects = new TestSubjectRepository().Get();
                catalogSubjects = allSubjects.Where(s => s.ShowInCourseSearch == true);
                allLocations = new TestLocationRepository().Get();
                allDepartments = new TestDepartmentRepository().Get();

                referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
                referenceRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allLocations);
                referenceRepoMock.Setup(repo => repo.GetDepartmentsAsync(false)).Returns(Task.FromResult(allDepartments));

                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                allCourses = await new TestCourseRepository().GetAsync();
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                var catalogSubjectCodes = catalogSubjects.Select(s => s.Code);
                catalogCourses = allCourses.Where(c => c.IsCurrent == true && catalogSubjectCodes.Contains(c.SubjectCode));

                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                ruleRepoMock = new Mock<IRuleRepository>();
                ruleRepo = ruleRepoMock.Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                configurationRepoMock = new Mock<IStudentConfigurationRepository>();
                configurationRepo = configurationRepoMock.Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                allSections = await new TestSectionRepository().GetAsync();
                var course9879Sections = allSections.Where(s => s.CourseId == "9879");
                var course9880Sections = allSections.Where(s => s.CourseId == "9880");
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(allSections));
                sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(It.Is<List<string>>(l => l.Contains("9879")), regTerms)).Returns(Task.FromResult(course9879Sections));
                sectionRepoMock.Setup<Task<IEnumerable<Domain.Student.Entities.Section>>>(repo => repo.GetCourseSectionsCachedAsync(It.IsAny<List<string>>(), regTerms)).Returns(
                    (List<string> courseIds, IEnumerable<Domain.Student.Entities.Term> terms) => Task.FromResult(allSections.Where(a => courseIds.Contains(a.CourseId)))
                    );
                //List<string> course9880List = new List<string>() { "9880" };Where(a=>courseIds.Contains(a.CourseId)
                //sectionRepoMock.Setup(repo => repo.GetCourseSectionsCached(course9880List, regTerms)).Returns(course9880Sections);
                requirementsRepoMock = new Mock<IRequirementRepository>();
                requirementsRepo = requirementsRepoMock.Object;

                studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceRepo = studentReferenceRepoMock.Object;
                studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                userFactoryMock = new Mock<ICurrentUserFactory>();
                userFactory = userFactoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                var coursePageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>()).Returns(coursePageAdapter);
                var corequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>()).Returns(corequisiteAdapter);
                courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                    studentReferenceRepo, null, sectionRepoMock.Object, termRepoMock.Object, ruleRepoMock.Object,
                    configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseRepoMock = null;
                courseRepo = null;
                referenceRepoMock = null;
                referenceRepo = null;
                adapterRegistryMock = null;
                adapterRegistry = null;
                courseService = null;
            }

            [TestMethod]
            public async Task ReturnsResultsAsIEnumerablecoursePage()
            {
                var criteria = new CourseSearchCriteria() { Keyword = "*intro*" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.IsTrue(coursePage.CurrentPageItems.Count() >= 12);
                Assert.IsTrue(coursePage.CurrentPageItems is IEnumerable<Course>);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AllowsRepositoryExceptionToFallThrough()
            {
                courseRepoMock.Setup(repo => repo.GetAsync()).Throws(new Exception("failure"));
                var criteria = new CourseSearchCriteria() { Keyword = "intro" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
            }


            [TestMethod]
            public async Task FindsSubjectCode()
            {
                // Test every subject in the test repository
                foreach (var subj in catalogSubjects)
                {
                    var criteria = new CourseSearchCriteria();
                    criteria.Subjects = new List<string>() { subj.Code };
#pragma warning disable 618
                    var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                    // If there is a course in the test repository that has the current subject, make sure that course is found by the search service
                    foreach (var course in catalogCourses.Where(c => c.SubjectCode == subj.Code))
                    {
                        Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains(course.Id));
                    }
                }
            }

            [TestMethod]
            public async Task FindsSubjectDescription()
            {
                // Test every subject in the test repository
                foreach (var subj in catalogSubjects)
                {
                    var criteria = new CourseSearchCriteria();
                    criteria.Keyword = subj.Description;
#pragma warning disable 618
                    var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                    // If there is a course in the test repository that has the current subject, make sure that course is found by the search service
                    foreach (var course in catalogCourses.Where(c => c.SubjectCode == subj.Code))
                    {
                        Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains(course.Id));
                    }
                }
            }

            [TestMethod]
            public async Task FindsNumber()
            {
                string keyword = "BBB";
                var criteria = new CourseSearchCriteria() { Keyword = keyword };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual(1, coursePage.CurrentPageItems.Count());
                Assert.AreEqual("9879", coursePage.CurrentPageItems.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task FindsNumberWithWildcards()
            {
                string keyword = "*BBB*";
                var criteria = new CourseSearchCriteria() { Keyword = keyword };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual(2, coursePage.CurrentPageItems.Count());
                Assert.AreEqual("9879", coursePage.CurrentPageItems.ElementAt(0).Id);
                Assert.AreEqual("9880", coursePage.CurrentPageItems.ElementAt(1).Id);

            }

            [TestMethod]
            public async Task FindsTitle()
            {
                string keyword = "DDDDD";
                var criteria = new CourseSearchCriteria() { Keyword = keyword };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual(1, coursePage.CurrentPageItems.Count());
                Assert.AreEqual("9879", coursePage.CurrentPageItems.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task FindsTitleWithWildcards()
            {
                string keyword = "*DDDDD*";
                var criteria = new CourseSearchCriteria() { Keyword = keyword };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual(2, coursePage.CurrentPageItems.Count());
                Assert.AreEqual("9879", coursePage.CurrentPageItems.ElementAt(0).Id);
                Assert.AreEqual("9880", coursePage.CurrentPageItems.ElementAt(1).Id);
            }

            [TestMethod]
            public async Task FindsDescription()
            {
                string keyword = "GGGGGGGGGGG";
                var criteria = new CourseSearchCriteria() { Keyword = keyword };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual(1, coursePage.CurrentPageItems.Count());
                Assert.AreEqual("9879", coursePage.CurrentPageItems.ElementAt(0).Id);
                //Assert.AreEqual("9880", coursePage.CurrentPageItems.ElementAt(1).Id);
            }

            [TestMethod]
            public async Task FindsSynonym()
            {
                string keyword = "7966696";
                var criteria = new CourseSearchCriteria() { Keyword = keyword };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual(1, coursePage.CurrentPageItems.Count());
                Assert.AreEqual("7704", coursePage.CurrentPageItems.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task FindsDescriptionWithWildcards()
            {
                string keyword = "*GGGGGGGGGGG*";
                var criteria = new CourseSearchCriteria() { Keyword = keyword };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual(2, coursePage.CurrentPageItems.Count());
                Assert.AreEqual("9879", coursePage.CurrentPageItems.ElementAt(0).Id);
                Assert.AreEqual("9880", coursePage.CurrentPageItems.ElementAt(1).Id);
            }

            [TestMethod]
            public async Task FindsDepartmentCode()
            {
                // Test every department in the test repository
                foreach (var dept in allDepartments)
                {
                    var criteria = new CourseSearchCriteria() { Keyword = dept.Code };
#pragma warning disable 618
                    var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                    // If there is a course in the test repository that has the current location, make sure that course is found by the search service
                    foreach (var course in catalogCourses.Where(c => c.LocationCodes.Any(d => d == dept.Code)))
                    {
                        Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains(course.Id));
                    }
                }
            }

            [TestMethod]
            public async Task FindsDepartmentDescription()
            {
                // Test every department in the test repository
                foreach (var dept in allDepartments)
                {
                    if (dept.Code != "AAPMA") // AAPMA is a junk value in the test repository
                    {
                        var criteria = new CourseSearchCriteria() { Keyword = dept.Description };
#pragma warning disable 618
                        var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                        // If there is a course in the test repository that has the current department, make sure that course is found by the search service
                        foreach (var course in catalogCourses.Where(c => c.LocationCodes.Any(d => d == dept.Code)))
                        {
                            Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains(course.Id));
                        }
                    }
                }
            }

            [TestMethod]
            public async Task FindsLocationCode()
            {
                // Test every location in the test repository
                foreach (var loc in allLocations)
                {
                    var criteria = new CourseSearchCriteria() { Keyword = loc.Code };
#pragma warning disable 618
                    var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                    // If there is a course in the test repository that has the current location, make sure that course is found by the search service
                    foreach (var course in catalogCourses.Where(c => c.LocationCodes.Any(l => l == loc.Code)))
                    {
                        Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains(course.Id));
                    }
                }
            }

            [TestMethod]
            public async Task FindsLocationDescription()
            {
                // Test every location in the test repository
                foreach (var loc in allLocations)
                {
                    var criteria = new CourseSearchCriteria() { Keyword = loc.Description };
#pragma warning disable 618
                    var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                    // If there is a course in the test repository that has the current location, make sure that course is found by the search service
                    foreach (var course in catalogCourses.Where(c => c.LocationCodes.Any(l => l == loc.Code)))
                    {
                        Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains(course.Id));
                    }
                }
            }

            [TestMethod]
            public async Task SubjectAndNumberWithoutSpaceReturnsCourseWithMatchingName()
            {
                var criteria = new CourseSearchCriteria() { Keyword = "MATH502" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual("9878", coursePage.CurrentPageItems.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task LowercaseSubjectAndNumberWithoutSpaceReturnsCourseWithMatchingName()
            {
                var criteria = new CourseSearchCriteria() { Keyword = "math502" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual("9878", coursePage.CurrentPageItems.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task SubjectAndNumberWithSpaceShowsCourseWithMatchingNameFirst()
            {
                var criteria = new CourseSearchCriteria() { Keyword = "MATH 502" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual("9878", coursePage.CurrentPageItems.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task SubjectAndNumberWithDelimiterFindsCourseWithMatchingName()
            {
                var criteria = new CourseSearchCriteria() { Keyword = "MATH" + CourseService.CourseDelimiter + "502" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.AreEqual("9878", coursePage.CurrentPageItems.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task SearchReturnsCourseWithMatchingSubjectFirst()
            {
                // Both strings not found in any one course
                var criteria = new CourseSearchCriteria() { Keyword = "*comp*" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.IsTrue(coursePage.CurrentPageItems.Count() >= 7);
                Assert.AreEqual("117", coursePage.CurrentPageItems.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task SearchFindsAllCoursesWithMultipleWildcardStrings()
            {
                // Strings found in mutually exclusive courses, all found
                var criteria = new CourseSearchCriteria() { Keyword = "*comp* OR *aaa*" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                // This assert allows for items to be added to course repository that also meet criteria. Adjust as needed.
                Assert.IsTrue(coursePage.CurrentPageItems.Count() <= 13);
                // As of the time test was written, these are the 11 that were found
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("117"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("9879"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("188"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("2414"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("9880"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("143"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("148"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("155"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("187"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("78"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("47"));
            }

            [TestMethod]
            public async Task SearchFindsAllCoursesWithMultipleStringsWithOperator()
            {
                // Strings found in mutually exclusive courses, all found
                var criteria = new CourseSearchCriteria() { Keyword = "comp OR aaaa" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                // Assert allows for items to be added to test repository that may meet criteria. Adjust as needed.
                Assert.IsTrue(coursePage.CurrentPageItems.Count() <= 6);
                // When this test was written, four items had either of these absolute strings (subset of previous test)
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("117"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("9879"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("188"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("2414"));
            }


            [TestMethod]
            public async Task SearchFindsAllCoursesNoDups()
            {
                // strings found in multiple courses, some overlapping. But none are duplicated in the list. Two terms are "OR"ed
                var criteria = new CourseSearchCriteria() { Keyword = "*writ* *intro*" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                // When this test was written, fourteen items had either of these wildcard strings (but others may be added. Adjust as needed).
                Assert.IsTrue(coursePage.CurrentPageItems.Count() <= 20);
                // Specifically verify that the ones that had both are not duplicated in the list.
                Assert.IsTrue(coursePage.CurrentPageItems.Where(c => c.Id == "188").Count() == 1);
                Assert.IsTrue(coursePage.CurrentPageItems.Where(c => c.Id == "117").Count() == 1);
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("187"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("130"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("180"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("160"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("306"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("315"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("159"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("155"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("156"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("154"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("213"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("353"));
            }


            [TestMethod]
            public async Task SearchFindsCoursesUsingANDFromTitleAndDescription()
            {
                // strings found in multiple courses, AND works without wildcards
                var criteria = new CourseSearchCriteria() { Keyword = "college AND Learned" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                // Only two as of test writing, but allow for the possibility that additional courses may be added that meet criteria
                Assert.IsTrue(coursePage.CurrentPageItems.Count() <= 4);
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("187"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("188"));
            }

            [TestMethod]
            public async Task SearchFindsCoursesUsingANDFromDepartmentAndTitleAndLocation()
            {
                // AND without wildcards -- department and subject and location
                var criteria = new CourseSearchCriteria() { Keyword = "perf AND dance AND main" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                // Only one as of test writing, but allow for the possibility that additional courses added that meet criteria
                Assert.IsTrue(coursePage.CurrentPageItems.Count() <= 3);
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("28"));
            }

            [TestMethod]
            public async Task SearchFindsCoursesUsingANDFromNumberAndTitleAndDescription()
            {
                // AND without wildcards -- course number and title and description
                var criteria = new CourseSearchCriteria() { Keyword = "207 AND ensemble AND students" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                // Only one as of test writing, but allow for the possibility that additional courses added that meet criteria. Adjust if needed
                Assert.IsTrue(coursePage.CurrentPageItems.Count() <= 3);
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("355"));
            }

            [TestMethod]
            public async Task SearchFindsAllCoursesWithMultipleStringsWithANDOperatorAndBeginningWildcard()
            {
                // Wildcard at beginning and end of both strings with AND
                var criteria = new CourseSearchCriteria() { Keyword = "*comp* AND *engl*" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                // Only three as of test writing, but allow for the possibility that additional courses added that meet criteria. Adjust if needed.
                Assert.IsTrue(coursePage.CurrentPageItems.Count() <= 5);
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("187"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("188"));
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("2414"));
            }

            [TestMethod]
            public async Task SearchFindsCoursesUsingANDInTitle()
            {
                // Wildcard and end of strings with AND.
                var criteria = new CourseSearchCriteria() { Keyword = "danc* AND ball*" };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                // Only one as of test writing, but allow for the possibility that additional courses added that meet criteria. Adjust if needed.
                Assert.IsTrue(coursePage.CurrentPageItems.Count() <= 3);
                Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("122"));
            }

            [TestMethod]
            public async Task InactiveCoursesExcludedFromKeywordResults()
            {
                // Arrange--set up subject search for DENT courses
                var criteria = new CourseSearchCriteria() { Keyword = "DENT" };

                // Act -- Invoke Search service. Use max int for page size to verify correct quantity of items returned
#pragma warning disable 618
                CoursePage coursePage = await courseService.SearchAsync(criteria, Int32.MaxValue, 1);
#pragma warning restore 618

                // Assert -- inactive courses excluded from results, active courses included
                Assert.AreEqual(0, coursePage.CurrentPageItems.Where(c => c.Id == "7705").Count());
                Assert.AreEqual(0, coursePage.CurrentPageItems.Where(c => c.Id == "7706").Count());
                Assert.AreEqual(1, coursePage.CurrentPageItems.Where(c => c.Id == "7704").Count());
            }
        }

        [TestClass]
        public class SearchCourseByID
        {
            private Mock<ICourseRepository> courseRepoMock;
            private ICourseRepository courseRepo;
            private Mock<ITermRepository> termRepoMock;
            private ITermRepository termRepo;
            private Mock<IRuleRepository> ruleRepoMock;
            private IRuleRepository ruleRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private Mock<IReferenceDataRepository> referenceRepoMock;
            private IReferenceDataRepository referenceRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private CourseService courseService;
            private Mock<IRequirementRepository> requirementsRepoMock;
            private IRequirementRepository requirementsRepo;
            private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
            private IStudentReferenceDataRepository studentReferenceRepo;
            private Mock<IStudentConfigurationRepository> configurationRepoMock;
            private IStudentConfigurationRepository configurationRepo;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> catalogSubjects;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
            private ILogger logger;
            private Mock<ICurrentUserFactory> userFactoryMock;
            private ICurrentUserFactory userFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public async void Initialize()
            {
                referenceRepoMock = new Mock<IReferenceDataRepository>();
                referenceRepo = referenceRepoMock.Object;
                allSubjects = new TestSubjectRepository().Get();
                catalogSubjects = allSubjects.Where(s => s.ShowInCourseSearch == true);
                allLocations = new TestLocationRepository().Get();
                allDepartments = new TestDepartmentRepository().Get();

                referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
                referenceRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allLocations);
                referenceRepoMock.Setup(repo => repo.GetDepartmentsAsync(false)).Returns(Task.FromResult(allDepartments));

                courseRepoMock = new Mock<ICourseRepository>();
                courseRepo = courseRepoMock.Object;
                allCourses = new TestCourseRepository().GetAsync().Result;
                courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
                var catalogSubjectCodes = catalogSubjects.Select(s => s.Code);
                catalogCourses = allCourses.Where(c => c.IsCurrent == true && catalogSubjectCodes.Contains(c.SubjectCode));

                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

                ruleRepoMock = new Mock<IRuleRepository>();
                ruleRepo = ruleRepoMock.Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                allSections = new TestSectionRepository().GetAsync().Result;
                sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(allSections));

                requirementsRepoMock = new Mock<IRequirementRepository>();
                requirementsRepo = requirementsRepoMock.Object;

                studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceRepo = studentReferenceRepoMock.Object;
                studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));

                configurationRepoMock = new Mock<IStudentConfigurationRepository>();
                configurationRepo = configurationRepoMock.Object;

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                userFactoryMock = new Mock<ICurrentUserFactory>();
                userFactory = userFactoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                var coursePageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>()).Returns(coursePageAdapter);
                var corequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>()).Returns(corequisiteAdapter);
                courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                    studentReferenceRepo, null, sectionRepoMock.Object, termRepoMock.Object, ruleRepoMock.Object,
                    configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);

            }

            [TestCleanup]
            public void Cleanup()
            {
                courseRepoMock = null;
                courseRepo = null;
                referenceRepoMock = null;
                referenceRepo = null;
                adapterRegistryMock = null;
                adapterRegistry = null;
                courseService = null;
            }

            [TestMethod]
            public async Task FindsOneCourse()
            {
                var criteria = new CourseSearchCriteria() { CourseIds = new List<string>() { "2418" } };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.IsTrue(coursePage.CurrentPageItems.Count() == 1);
                Assert.IsTrue(coursePage.CurrentPageItems is IEnumerable<Course>);
            }
            [TestMethod]
            public async Task FindsTwoCourses()
            {
                var criteria = new CourseSearchCriteria() { CourseIds = new List<string>() { "2418", "2419" } };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.IsTrue(coursePage.CurrentPageItems.Count() == 2);
                Assert.IsTrue(coursePage.CurrentPageItems is IEnumerable<Course>);
            }
            [TestMethod]
            public async Task FindsEquatedCourse()
            {
                var criteria = new CourseSearchCriteria() { CourseIds = new List<string>() { "155" } };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.IsTrue(coursePage.CurrentPageItems.Count() == 2);
                Assert.IsTrue(coursePage.CurrentPageItems is IEnumerable<Course>);
            }
            [TestMethod]
            public async Task FindsEquatedCourses()
            {
                var criteria = new CourseSearchCriteria() { CourseIds = new List<string>() { "155", "2418" } };
#pragma warning disable 618
                var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
                Assert.IsTrue(coursePage.CurrentPageItems.Count() == 3);
                Assert.IsTrue(coursePage.CurrentPageItems is IEnumerable<Course>);
            }

        }
    }

    [TestClass]
    public class CourseService_GetCourses2Async
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;
        List<Dtos.Course2> allDtoCourses = new List<Dtos.Course2>();
        List<Ellucian.Colleague.Dtos.Credit2> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForGetAllAndGetById();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course2>();
            foreach (var course in allEntityCourses)
            {
                Ellucian.Colleague.Dtos.Course2 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course2>(course);
                allDtoCourses.Add(target);
            }

            courseService = new CourseService
                            (
                                adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                            );
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
        }

        private async Task BuildMocksForGetAllAndGetById()
        {
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                    {
                                        new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                    };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(true)).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(false)).ReturnsAsync(creditCategoryCollection);
        }

        [TestMethod]
        public async Task CourseService_GetCourses2Async_GetAll()
        {
            //Arrange
            courseRepositoryMock.Setup(c => c.GetAsync())
                 .ReturnsAsync(allEntityCourses);
            //Act
            var result = await courseService.GetCourses2Async(true);
            //Assert
            Assert.AreEqual(41, allDtoCourses.Count());
        }

        [TestMethod]
        public async Task CourseService_GetCourses2Async_GetCourseByGuid2Async()
        {
            //Arrange
            var entityCourse = allEntityCourses.Take(1).FirstOrDefault();
            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(entityCourse.Guid))
                 .ReturnsAsync(entityCourse);
            //Act
            var result = await courseService.GetCourseByGuid2Async(entityCourse.Guid);
            //Assert
            Assert.AreEqual(result.Id, entityCourse.Guid);
            Assert.AreEqual(result.Description, entityCourse.Description);
            Assert.AreEqual(result.EffectiveEndDate, entityCourse.EndDate);
            Assert.AreEqual(result.EffectiveStartDate, entityCourse.StartDate);
        }
    }

    [TestClass]
    public class CourseService_GetCourses3Async
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;
        List<Dtos.Course3> allDtoCourses = new List<Dtos.Course3>();
        List<Ellucian.Colleague.Dtos.Credit3> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        List<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string acctGuidForFilter = string.Empty;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForGetAllAndGetById();

            allEntityCourses.First().AddInstructionalMethodCode("LG");
            allEntityCourses.First().GradeSchemeCode = "UG";
            BuildCreditCategories(allEntityCourses);


            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course3>();
            foreach (var course in allEntityCourses)
            {
                Ellucian.Colleague.Dtos.Course3 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course3>(course);
                allDtoCourses.Add(target);
            }

            courseService = new CourseService
                            (
                                adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                            );
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
        }

        private async Task BuildMocksForGetAllAndGetById()
        {
            //GetSubjectsAsync
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync()).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(true)).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(false)).ReturnsAsync(allSubjects);

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(true)).ReturnsAsync(allGradeScheme);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(false)).ReturnsAsync(allGradeScheme);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(allDepartments);

            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                    {
                                        new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                    };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "IN", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d7", "E", "Transfer Credit", Domain.Student.Entities.CreditType.Exchange));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d6", "N", "Transfer Credit", Domain.Student.Entities.CreditType.None));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d5", "O", "Transfer Credit", Domain.Student.Entities.CreditType.Other));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(true)).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(false)).ReturnsAsync(creditCategoryCollection);
        }

        private void BuildCreditCategories(IEnumerable<Domain.Student.Entities.Course> allEntityCourses)
        {
            int counter = 0;
            foreach (var course in allEntityCourses)
            {
                if (counter < 6)
                {
                    course.LocalCreditType = creditCategoryCollection[counter].Code;
                    counter++;
                }
                else if (counter == 6)
                {
                    course.LocalCreditType = string.Empty;
                    break;
                }
            }
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetAll_ByPassCache_True()
        {
            string empty = string.Empty;
            List<string> emptylist = null;
            //Arrange
            //courseRepositoryMock.Setup(c => c.GetNonCacheAsync(empty, empty, empty, empty, empty, empty, empty, empty, empty, empty))
            //     .ReturnsAsync(allEntityCourses);
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, emptylist, emptylist, empty, emptylist, empty, empty, empty, empty, false))
               .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses3Async(It.IsAny<int>(), It.IsAny<int>(), true, empty, empty, empty, empty, empty, empty, empty, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item1.Count());
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetAll_ByPassCache_False()
        {
            string empty = string.Empty;
            List<string> emptylist = null; // new List<string>() { "" };
            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, emptylist, emptylist, empty, emptylist, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses3Async(It.IsAny<int>(), It.IsAny<int>(), false, empty, empty, empty, empty, empty, empty, empty, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item1.Count());
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetCourseByGuid3Async()
        {
            //Arrange
            var entityCourse = allEntityCourses.Take(1).FirstOrDefault();
            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(entityCourse.Guid))
                 .ReturnsAsync(entityCourse);
            //Act
            var result = await courseService.GetCourseByGuid3Async(entityCourse.Guid);
            //Assert
            Assert.AreEqual(result.Id, entityCourse.Guid);
            Assert.AreEqual(result.Description, entityCourse.Description);
            Assert.AreEqual(result.EffectiveEndDate, entityCourse.EndDate);
            Assert.AreEqual(result.EffectiveStartDate, entityCourse.StartDate);
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetCourseByGuid3Async_Subject()
        {
            acctGuidForFilter = allSubjects.FirstOrDefault(i => i.Code.Equals("HIST", StringComparison.OrdinalIgnoreCase)).Guid;
            var expected = allEntityCourses.Where(i => i.SubjectCode.Equals("HIST", StringComparison.OrdinalIgnoreCase));
            string empty = string.Empty;
            List<string> emptylist = null;
            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), "HIST", empty, emptylist, emptylist, empty, emptylist, empty, empty, empty, empty, false))
               .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(expected, expected.ToList().Count));

            //Act
            var actuals = await courseService.GetCourses3Async(It.IsAny<int>(), It.IsAny<int>(), false, acctGuidForFilter, empty, empty, empty, empty, empty, empty, empty);
            //Assert
            Assert.AreEqual(expected.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetCourseByGuid3Async_Number()
        {
            var expected = allEntityCourses.Where(i => i.Number.Equals("200", StringComparison.OrdinalIgnoreCase));
            string empty = string.Empty;
            List<string> emptylist = null;
            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, "200", emptylist, emptylist, empty, emptylist, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(expected, expected.ToList().Count));

            //Act
            var actuals = await courseService.GetCourses3Async(It.IsAny<int>(), It.IsAny<int>(), false, empty, "200", empty, empty, empty, empty, empty, empty);
            //Assert
            Assert.AreEqual(expected.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetCourseByGuid3Async_AcademicLevel()
        {
            var expected = allEntityCourses.Where(i => i.AcademicLevelCode.Equals("UG", StringComparison.OrdinalIgnoreCase));
            string acadLevelId = "1df164eb-8178-4321-a9f7-24f12d3991d8";
            string empty = string.Empty;
            List<string> emptylist = null;
            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, new List<string>() { "UG" }, emptylist, empty, emptylist, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(expected, expected.ToList().Count));

            //Act
            var actuals = await courseService.GetCourses3Async(It.IsAny<int>(), It.IsAny<int>(), false, empty, empty, acadLevelId, empty, empty, empty, empty, empty);
            //Assert
            Assert.AreEqual(expected.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetCourseByGuid3Async_OwningInstitutionUnits()
        {
            var expected = allEntityCourses.Where(i => i.Departments.Any(a => a.AcademicDepartmentCode.Equals("HIST", StringComparison.OrdinalIgnoreCase)));
            string owningInstitutionUnitsId = allDepartments.Where(i => i.Code.Equals("HIST", StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Guid;//expected.FirstOrDefault().Guid;
            string empty = string.Empty;
            List<string> emptylist = null;
            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, emptylist, new List<string>() { "HIST" }, empty, emptylist, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(expected, expected.ToList().Count));

            //Act
            var actuals = await courseService.GetCourses3Async(It.IsAny<int>(), It.IsAny<int>(), false, empty, empty, empty, owningInstitutionUnitsId, empty, empty, empty, empty);
            //Assert
            Assert.AreEqual(expected.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetCourseByGuid3Async_Title()
        {
            string title = "World History to WWII";
            var expected = allEntityCourses.Where(i => i.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            string empty = string.Empty;
            List<string> emptylist = null;
            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, emptylist, emptylist, title, emptylist, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(expected, expected.ToList().Count));

            //Act
            var actuals = await courseService.GetCourses3Async(It.IsAny<int>(), It.IsAny<int>(), false, empty, empty, empty, empty, title, empty, empty, empty);
            //Assert
            Assert.AreEqual(expected.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetCourseByGuid3Async_InstructionalMethod()
        {
            string instructionalMethod = "9c3b805d-cfe6-483b-86c3-4c20562f8c15";
            var expected = allEntityCourses.Where(i => i.InstructionalMethodCodes.Any(a => a.Equals("LG", StringComparison.OrdinalIgnoreCase)));
            string empty = string.Empty;
            List<string> emptylist = null;
            //Arrange       
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, emptylist, emptylist, empty, new List<string>() { "LG" }, empty, empty, empty, empty, false))
               .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(expected, expected.ToList().Count));

            //Act
            var actuals = await courseService.GetCourses3Async(It.IsAny<int>(), It.IsAny<int>(), false, empty, empty, empty, empty, empty, instructionalMethod, empty, empty);
            //Assert
            Assert.AreEqual(expected.Count(), actuals.Item1.Count());
        }

        [TestMethod]
        public async Task CourseService_GetCourses3Async_GetCourseByGuid3Async_StartOn()
        {
            var schedulingStartOn = new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Unspecified);
            var expected = allEntityCourses.Where(i => i.StartDate.Equals(schedulingStartOn));
            string empty = string.Empty;
            List<string> emptylist = null;
            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, emptylist, emptylist, empty, emptylist, schedulingStartOn.ToString(), empty, empty, empty, false))
              .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(expected, expected.ToList().Count));

            sectionRepositoryMock.Setup(d => d.GetUnidataFormattedDate("2001-01-01")).ReturnsAsync(schedulingStartOn.ToString());
            //Act
            var actuals = await courseService.GetCourses3Async(It.IsAny<int>(), It.IsAny<int>(), false, empty, empty, empty, empty, empty, empty, "2001-01-01", empty);
            //Assert
            Assert.AreEqual(expected.Count(), actuals.Item1.Count());
        }
    }

    [TestClass]
    public class CourseService_PostCourses2Async : CurrentUserSetup
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;

        List<Dtos.Course2> allDtoCourses = new List<Dtos.Course2>();
        List<Ellucian.Colleague.Dtos.Credit2> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string code;
        string description;
        ExternalMapping externalMapping;
        ExternalMappingItem item1;
        ExternalMappingItem item2;
        ExternalMappingItem item3;
        ExternalMappingItem item4;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForCourseCreateUpdate();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course2>();
            foreach (var course in allEntityCourses)
            {
                Ellucian.Colleague.Dtos.Course2 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course2>(course);
                allDtoCourses.Add(target);
            }

            courseService = new CourseService
                                (
                                    adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                    studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                    termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                    curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                                );
        }

        private async Task BuildMocksForCourseCreateUpdate()
        {
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(true)).Returns(Task.FromResult(allSubjects));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(false)).Returns(Task.FromResult(allSubjects));

            code = "LDM.SUBJECTS";
            description = "Mapping of Subjects to Departments";
            externalMapping = new ExternalMapping(code, description);
            item1 = new ExternalMappingItem("ACCT") { NewCode = "BUSN" };
            item2 = new ExternalMappingItem("AVIA") { NewCode = "AUTO" };
            item3 = new ExternalMappingItem("COMP") { NewCode = "MATH" };
            item4 = new ExternalMappingItem("HIST") { NewCode = "HIST" };

            externalMapping.AddItem(item1);
            externalMapping.AddItem(item2);
            externalMapping.AddItem(item3);
            externalMapping.AddItem(item4);
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(new Domain.Student.Entities.CurriculumConfiguration()
                {
                    SubjectDepartmentMapping = externalMapping,
                    DefaultAcademicLevelCode = "UG",
                    DefaultCourseLevelCode = "UG",
                    ApproverId = "123",
                    ApprovingAgencyId = "123",
                    CourseActiveStatusCode = "A",
                    CourseInactiveStatusCode = "C"
                });


            var creditCategoryOne = new Dtos.CreditIdAndTypeProperty();
            creditCategoryOne.Detail = new GuidObject2() { Id = "9c3b805d-cfe6-483b-86c3-4c20562f8c15" };
            creditCategoryOne.CreditType = CreditCategoryType2.Institutional;

            var creditCategoryTwo = new Dtos.CreditIdAndTypeProperty();
            creditCategoryTwo.Detail = new GuidObject2() { Id = "0736a2d4-7733-4e0d-bfc0-0fe92a165c97" };
            creditCategoryTwo.CreditType = CreditCategoryType2.Institutional;

            credits = new List<Ellucian.Colleague.Dtos.Credit2>()
                            { new Ellucian.Colleague.Dtos.Credit2() { CreditCategory = creditCategoryOne },
                                new Ellucian.Colleague.Dtos.Credit2() { CreditCategory = creditCategoryTwo }
                            };

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(true)).ReturnsAsync(allGradeScheme);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(false)).ReturnsAsync(allGradeScheme);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(true)).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(false)).ReturnsAsync(allDepartments);
            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                        {
                                            new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                        };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(true)).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(false)).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(true)).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(false)).ReturnsAsync(creditCategoryCollection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            code = null;
            description = null;
            externalMapping = null;
            item1 = null;
            item2 = null;
            item3 = null;
            item4 = null;
        }

        [TestMethod]
        public async Task CourseService_CreateCourse2Async()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(newEntityCourseForCreate.Guid))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid2Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionMethods.Add(new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"));

            courseRepositoryMock.Setup(crm => crm.CreateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.CreateCourse2Async(result);
            //Assert
            Assert.AreEqual(result.Id, newEntityCourseForCreate.Guid);
        }

        [TestMethod]
        public async Task CourseService_CreateCourse2Async_GUID_Reqd()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(newEntityCourseForCreate.Guid))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid2Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionMethods.Add(new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"));

            result.Id = Guid.Empty.ToString();

            courseRepositoryMock.Setup(crm => crm.CreateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.CreateCourse2Async(result);
            //Assert
            Assert.AreEqual(newEntityCourseForCreate.Guid, resultCreate.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_Empty_Id()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = string.Empty;
            courseDto.Title = "Some title";
            courseDto.Subject.Id = "234";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_Empty_Title()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = string.Empty;
            courseDto.Subject.Id = "234";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_Empty_SubjectId()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = string.Empty;
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_Empty_Number()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = string.Empty;
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_Default_EffectiveStartDate()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = default(DateTime);

            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_Default_EffectiveEndDate()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveEndDate = null;

            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_AcadLevels()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.AcademicLevels = new List<GuidObject2>() { new GuidObject2("blah") };
            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_CourseLevelsNull()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.CourseLevels = new List<GuidObject2>() { new GuidObject2("blah") };
            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_OwningOrganizations()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningOrganizations = new List<Dtos.OfferingOrganization2>() { new Dtos.OfferingOrganization2() { Organization = new GuidObject2(Guid.NewGuid().ToString()) } };

            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_InstructionalMethodCodes()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.InstructionMethods = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = Guid.NewGuid().ToString() } };
            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_InstructionalMethodCodesNull()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.InstructionMethods = new List<GuidObject2>() { new GuidObject2("blah") };
            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_GradeSchemeCode()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.GradeSchemes = new List<Dtos.GuidObject2>() { new Dtos.GuidObject2() { Id = Guid.NewGuid().ToString() } };
            await courseService.CreateCourse2Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse2Async_ArgumentException_CreditCategory()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course2 courseDto = new Dtos.Course2();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.Credits = null;
            await courseService.CreateCourse2Async(courseDto);
        }
        private Domain.Student.Entities.Course BuildCourseForCreate(Domain.Student.Entities.Course entityCourse)
        {
            Ellucian.Colleague.Domain.Student.Entities.Course course = new Ellucian.Colleague.Domain.Student.Entities.Course
                ("", entityCourse.Title, null, entityCourse.Departments, entityCourse.SubjectCode, entityCourse.Number, entityCourse.AcademicLevelCode,
                entityCourse.CourseLevelCodes, 3.0m, 1.0m, entityCourse.CourseApprovals);
            // GUID
            course.Guid = entityCourse.Guid;

            // DescriptionSearch
            course.Description = entityCourse.Description;


            course.LocationCodes = entityCourse.LocationCodes;
            // Max credits
            course.MaximumCredits = 4.0m;

            // Variable Credit Increment
            course.VariableCreditIncrement = 1.0m;

            // Start/End date
            course.StartDate = entityCourse.StartDate;
            course.EndDate = entityCourse.EndDate;

            // Topic Code
            course.TopicCode = "TopicCode";

            // Add course types
            course.AddType("TYPEA");

            // Add local credit type - the local string value of the credit type.
            course.LocalCreditType = "I";

            return course;
        }
    }

    [TestClass]
    public class CourseService_PostCourses3Async : CurrentUserSetup
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;

        List<Dtos.Course3> allDtoCourses = new List<Dtos.Course3>();
        List<Ellucian.Colleague.Dtos.Credit3> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string code;
        string description;
        ExternalMapping externalMapping;
        ExternalMappingItem item1;
        ExternalMappingItem item2;
        ExternalMappingItem item3;
        ExternalMappingItem item4;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForCourseCreateUpdate();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course3>();
            foreach (var course in allEntityCourses)
            {
                Ellucian.Colleague.Dtos.Course3 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course3>(course);
                allDtoCourses.Add(target);
            }

            courseService = new CourseService
                                (
                                    adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                    studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                    termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                    curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                                );
        }

        private async Task BuildMocksForCourseCreateUpdate()
        {
            //GetSubjectsAsync
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync()).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(true)).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(false)).ReturnsAsync(allSubjects);

            code = "LDM.SUBJECTS";
            description = "Mapping of Subjects to Departments";
            externalMapping = new ExternalMapping(code, description);
            item1 = new ExternalMappingItem("ACCT") { NewCode = "BUSN" };
            item2 = new ExternalMappingItem("AVIA") { NewCode = "AUTO" };
            item3 = new ExternalMappingItem("COMP") { NewCode = "MATH" };
            item4 = new ExternalMappingItem("HIST") { NewCode = "HIST" };

            externalMapping.AddItem(item1);
            externalMapping.AddItem(item2);
            externalMapping.AddItem(item3);
            externalMapping.AddItem(item4);
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(new Domain.Student.Entities.CurriculumConfiguration()
                {
                    SubjectDepartmentMapping = externalMapping,
                    DefaultAcademicLevelCode = "UG",
                    DefaultCourseLevelCode = "UG",
                    ApproverId = "123",
                    ApprovingAgencyId = "123",
                    CourseActiveStatusCode = "A",
                    CourseInactiveStatusCode = "C"
                });


            var creditCategoryOne = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryOne.Detail = new GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15");
            creditCategoryOne.CreditType = CreditCategoryType3.Institutional;

            var creditCategoryTwo = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryTwo.Detail = new GuidObject2("0736a2d4-7733-4e0d-bfc0-0fe92a165c97");
            creditCategoryTwo.CreditType = CreditCategoryType3.Institutional;

            credits = new List<Ellucian.Colleague.Dtos.Credit3>()
                            { new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryOne, Measure = CreditMeasure2.Credit, Minimum = 3.00000m, Maximum = 6.00000m, Increment = 1.00000m },
                                new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryTwo, Measure = CreditMeasure2.CEU, Minimum = 4.00000m }
                            };

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                        {
                                            new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                        };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(true)).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(false)).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(true)).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(false)).ReturnsAsync(creditCategoryCollection);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>())).Returns(Task.FromResult(allDepartments));
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            code = null;
            description = null;
            externalMapping = null;
            item1 = null;
            item2 = null;
            item3 = null;
            item4 = null;
        }

        [TestMethod]
        public async Task CourseService_CreateCourse3Async()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(newEntityCourseForCreate.Guid))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid3Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionMethods.Add(new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"));

            courseRepositoryMock.Setup(crm => crm.CreateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.CreateCourse3Async(result);
            //Assert
            Assert.AreEqual(result.Id, newEntityCourseForCreate.Guid);
        }

        [TestMethod]
        public async Task CourseService_CreateCourse3Async_GUID_Reqd()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(newEntityCourseForCreate.Guid))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid3Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionMethods.Add(new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"));

            result.Id = Guid.Empty.ToString();

            courseRepositoryMock.Setup(crm => crm.CreateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.CreateCourse3Async(result);
            //Assert
            Assert.AreEqual(newEntityCourseForCreate.Guid, resultCreate.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CourseService_CreateCourse3Async_ArgumentNullException_NUll_Course()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            await courseService.CreateCourse3Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_Empty_Title()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = string.Empty;
            courseDto.Subject.Id = "234";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_Subject_Null()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = null;
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_Subject_Id_Empty()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = new GuidObject2();
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_Number_Empty()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = new GuidObject2("200");
            courseDto.Number = "";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_Number_Length_GT_7()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = new GuidObject2("200");
            courseDto.Number = "12345678";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_EffectiveDate_null()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = new GuidObject2("200");
            courseDto.Number = "1234567";

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_CourseLevelsNull()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.CourseLevels = new List<GuidObject2>() { new GuidObject2() };

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_InstructionMethods_Null()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.CourseLevels = new List<GuidObject2>() { new GuidObject2("222") };
            courseDto.InstructionMethods = new List<GuidObject2>() { new GuidObject2() };

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_AcadLevels()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.AcademicLevels = new List<GuidObject2>() { new GuidObject2("") };

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_GradeSchemes()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.GradeSchemes = new List<GuidObject2>() { new GuidObject2("") };

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_OwningInstitutionUnits_Null()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = null } };

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_OwningInstitutionUnits_Id_EmptyString()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = new GuidObject2() } };

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_OwningInstitutionUnits_CreditCategory()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1") } };
            courseDto.Credits = new List<Credit3>() { new Credit3() { CreditCategory = null } };

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_OwningInstitutionUnits_CreditCategory_CreditType()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1") } };
            courseDto.Credits = new List<Credit3>() { new Credit3() { CreditCategory = new CreditIdAndTypeProperty2() { CreditType = null } } };

            await courseService.CreateCourse3Async(courseDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse3Async_ArgumentException_OwningInstitutionUnits_CreditCategory_Detail()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course3 courseDto = new Dtos.Course3();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1") } };
            courseDto.Credits = new List<Credit3>() { new Credit3() { CreditCategory = new CreditIdAndTypeProperty2() { CreditType = CreditCategoryType3.ContinuingEducation, Detail = new GuidObject2() } } };

            await courseService.CreateCourse3Async(courseDto);
        }

        private Domain.Student.Entities.Course BuildCourseForCreate(Domain.Student.Entities.Course entityCourse)
        {
            Ellucian.Colleague.Domain.Student.Entities.Course course = new Ellucian.Colleague.Domain.Student.Entities.Course
                ("", entityCourse.Title, null, entityCourse.Departments, entityCourse.SubjectCode, entityCourse.Number, entityCourse.AcademicLevelCode,
                entityCourse.CourseLevelCodes, 3.0m, 1.0m, entityCourse.CourseApprovals);
            // GUID
            course.Guid = entityCourse.Guid;

            // DescriptionSearch
            course.Description = entityCourse.Description;


            course.LocationCodes = entityCourse.LocationCodes;
            // Max credits
            course.MaximumCredits = 4.0m;

            // Variable Credit Increment
            course.VariableCreditIncrement = 1.0m;

            // Start/End date
            course.StartDate = entityCourse.StartDate;
            course.EndDate = entityCourse.EndDate;

            // Topic Code
            course.TopicCode = "TopicCode";

            // Add course types
            course.AddType("TYPEA");

            // Add local credit type - the local string value of the credit type.
            course.LocalCreditType = "I";

            return course;
        }
    }

    [TestClass]
    public class CourseService_PutCourses2Async : CurrentUserSetup
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;

        List<Dtos.Course2> allDtoCourses = new List<Dtos.Course2>();
        List<Ellucian.Colleague.Dtos.Credit2> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string code;
        string description;
        ExternalMapping externalMapping;
        ExternalMappingItem item1;
        ExternalMappingItem item2;
        ExternalMappingItem item3;
        ExternalMappingItem item4;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForCourseCreateUpdate();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course2>();
            foreach (var course in allEntityCourses)
            {
                Ellucian.Colleague.Dtos.Course2 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course2>(course);
                allDtoCourses.Add(target);
            }

            courseService = new CourseService
                                (
                                    adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                    studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                    termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                    curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                                );
        }

        private async Task BuildMocksForCourseCreateUpdate()
        {
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(true)).Returns(Task.FromResult(allSubjects));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(false)).Returns(Task.FromResult(allSubjects));

            code = "LDM.SUBJECTS";
            description = "Mapping of Subjects to Departments";
            externalMapping = new ExternalMapping(code, description);
            item1 = new ExternalMappingItem("ACCT") { NewCode = "BUSN" };
            item2 = new ExternalMappingItem("AVIA") { NewCode = "AUTO" };
            item3 = new ExternalMappingItem("COMP") { NewCode = "MATH" };
            item4 = new ExternalMappingItem("HIST") { NewCode = "HIST" };

            externalMapping.AddItem(item1);
            externalMapping.AddItem(item2);
            externalMapping.AddItem(item3);
            externalMapping.AddItem(item4);
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(new Domain.Student.Entities.CurriculumConfiguration()
                {
                    SubjectDepartmentMapping = externalMapping,
                    DefaultAcademicLevelCode = "UG",
                    DefaultCourseLevelCode = "UG",
                    ApproverId = "123",
                    ApprovingAgencyId = "123",
                    CourseActiveStatusCode = "A",
                    CourseInactiveStatusCode = "C"
                });

            var creditCategoryOne = new Dtos.CreditIdAndTypeProperty();
            creditCategoryOne.Detail = new GuidObject2() { Id = "9c3b805d-cfe6-483b-86c3-4c20562f8c15" };
            creditCategoryOne.CreditType = CreditCategoryType2.Institutional;

            var creditCategoryTwo = new Dtos.CreditIdAndTypeProperty();
            creditCategoryTwo.Detail = new GuidObject2() { Id = "0736a2d4-7733-4e0d-bfc0-0fe92a165c97" };
            creditCategoryTwo.CreditType = CreditCategoryType2.Institutional;

            credits = new List<Ellucian.Colleague.Dtos.Credit2>()
                            { new Ellucian.Colleague.Dtos.Credit2() { CreditCategory = creditCategoryOne },
                                new Ellucian.Colleague.Dtos.Credit2() { CreditCategory = creditCategoryTwo}
                            };

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(true)).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(false)).ReturnsAsync(allDepartments);
            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                        {
                                            new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                        };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(true)).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(false)).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(true)).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(false)).ReturnsAsync(creditCategoryCollection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            code = null;
            description = null;
            externalMapping = null;
            item1 = null;
            item2 = null;
            item3 = null;
            item4 = null;
        }

        [TestMethod]
        public async Task CourseService_UpdateCourse2Async()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(newEntityCourseForCreate.Guid))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid2Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionMethods.Add(new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"));

            courseRepositoryMock.Setup(crm => crm.UpdateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.UpdateCourse2Async(result);
            //Assert
            Assert.AreEqual(result.Id, newEntityCourseForCreate.Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CourseService_UpdateCourse2Async_ArgumentNullException()
        {
            await courseService.UpdateCourse2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CourseService_UpdateCourse2Async_KeyNotFoundException()
        {
            var result = new Dtos.Course2();
            result.Id = string.Empty;
            //Act
            await courseService.UpdateCourse2Async(result);
        }


        private Domain.Student.Entities.Course BuildCourseForCreate(Domain.Student.Entities.Course entityCourse)
        {
            Ellucian.Colleague.Domain.Student.Entities.Course course = new Ellucian.Colleague.Domain.Student.Entities.Course
                ("", entityCourse.Title, null, entityCourse.Departments, entityCourse.SubjectCode, entityCourse.Number, entityCourse.AcademicLevelCode,
                entityCourse.CourseLevelCodes, 3.0m, 1.0m, entityCourse.CourseApprovals);
            // GUID
            course.Guid = entityCourse.Guid;

            // DescriptionSearch
            course.Description = entityCourse.Description;


            course.LocationCodes = entityCourse.LocationCodes;
            // Max credits
            course.MaximumCredits = 4.0m;

            // Variable Credit Increment
            course.VariableCreditIncrement = 1.0m;

            // Start/End date
            course.StartDate = entityCourse.StartDate;
            course.EndDate = entityCourse.EndDate;

            // Topic Code
            course.TopicCode = "TopicCode";

            // Add course types
            course.AddType("TYPEA");

            // Add local credit type - the local string value of the credit type.
            course.LocalCreditType = "I";

            return course;
        }
    }

    [TestClass]
    public class CourseService_PutCourses3Async : CurrentUserSetup
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;

        List<Dtos.Course3> allDtoCourses = new List<Dtos.Course3>();
        List<Ellucian.Colleague.Dtos.Credit3> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string code;
        string description;
        ExternalMapping externalMapping;
        ExternalMappingItem item1;
        ExternalMappingItem item2;
        ExternalMappingItem item3;
        ExternalMappingItem item4;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForCourseCreateUpdate();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course3>();
            foreach (var course in allEntityCourses)
            {
                Ellucian.Colleague.Dtos.Course3 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course3>(course);
                allDtoCourses.Add(target);
            }

            courseService = new CourseService
                                (
                                    adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                    studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                    termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                    curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                                );
        }

        private async Task BuildMocksForCourseCreateUpdate()
        {
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(It.IsAny<bool>())).Returns(Task.FromResult(allSubjects));

            code = "LDM.SUBJECTS";
            description = "Mapping of Subjects to Departments";
            externalMapping = new ExternalMapping(code, description);
            item1 = new ExternalMappingItem("ACCT") { NewCode = "BUSN" };
            item2 = new ExternalMappingItem("AVIA") { NewCode = "AUTO" };
            item3 = new ExternalMappingItem("COMP") { NewCode = "MATH" };
            item4 = new ExternalMappingItem("HIST") { NewCode = "HIST" };

            externalMapping.AddItem(item1);
            externalMapping.AddItem(item2);
            externalMapping.AddItem(item3);
            externalMapping.AddItem(item4);
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(new Domain.Student.Entities.CurriculumConfiguration()
                {
                    SubjectDepartmentMapping = externalMapping,
                    DefaultAcademicLevelCode = "UG",
                    DefaultCourseLevelCode = "UG",
                    ApproverId = "123",
                    ApprovingAgencyId = "123",
                    CourseActiveStatusCode = "A",
                    CourseInactiveStatusCode = "C"
                });

            var creditCategoryOne = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryOne.Detail = new GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15");
            creditCategoryOne.CreditType = CreditCategoryType3.Institutional;

            var creditCategoryTwo = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryTwo.Detail = new GuidObject2("0736a2d4-7733-4e0d-bfc0-0fe92a165c97");
            creditCategoryTwo.CreditType = CreditCategoryType3.Institutional;

            credits = new List<Ellucian.Colleague.Dtos.Credit3>()
                            { new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryOne },
                                new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryTwo}
                            };

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(true)).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(false)).ReturnsAsync(allDepartments);
            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                        {
                                            new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                        };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(true)).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(false)).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(creditCategoryCollection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            code = null;
            description = null;
            externalMapping = null;
            item1 = null;
            item2 = null;
            item3 = null;
            item4 = null;
        }

        [TestMethod]
        public async Task CourseService_UpdateCourse3Async()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(newEntityCourseForCreate.Guid))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid3Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionMethods.Add(new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"));

            courseRepositoryMock.Setup(crm => crm.UpdateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.UpdateCourse3Async(result);
            //Assert
            Assert.AreEqual(result.Id, newEntityCourseForCreate.Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CourseService_UpdateCourse3Async_ArgumentNullException()
        {
            await courseService.UpdateCourse3Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CourseService_UpdateCourse3Async_KeyNotFoundException()
        {
            var result = new Dtos.Course3();
            result.Id = string.Empty;
            //Act
            await courseService.UpdateCourse3Async(result);
        }


        private Domain.Student.Entities.Course BuildCourseForCreate(Domain.Student.Entities.Course entityCourse)
        {
            Ellucian.Colleague.Domain.Student.Entities.Course course = new Ellucian.Colleague.Domain.Student.Entities.Course
                ("", entityCourse.Title, null, entityCourse.Departments, entityCourse.SubjectCode, entityCourse.Number, entityCourse.AcademicLevelCode,
                entityCourse.CourseLevelCodes, 3.0m, 1.0m, entityCourse.CourseApprovals);
            // GUID
            course.Guid = entityCourse.Guid;

            // DescriptionSearch
            course.Description = entityCourse.Description;


            course.LocationCodes = entityCourse.LocationCodes;
            // Max credits
            course.MaximumCredits = 4.0m;

            // Variable Credit Increment
            course.VariableCreditIncrement = 1.0m;

            // Start/End date
            course.StartDate = entityCourse.StartDate;
            course.EndDate = entityCourse.EndDate;

            // Topic Code
            course.TopicCode = "TopicCode";

            // Add course types
            course.AddType("TYPEA");

            // Add local credit type - the local string value of the credit type.
            course.LocalCreditType = "I";

            return course;
        }
    }

    [TestClass]
    public class CourseService_GetCourses4Async
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;
        List<Dtos.Course4> allDtoCourses = new List<Dtos.Course4>();
        List<Ellucian.Colleague.Dtos.Credit3> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        List<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string acctGuidForFilter = string.Empty;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForGetAllAndGetById();

            allEntityCourses.First().AddInstructionalMethodCode("LG");
            allEntityCourses.First().GradeSchemeCode = "UG";
            BuildCreditCategories(allEntityCourses);


            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course4>();
            foreach (var course in allEntityCourses)
            {
                Ellucian.Colleague.Dtos.Course4 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course4>(course);
                allDtoCourses.Add(target);
            }

            courseService = new CourseService
                            (
                                adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                            );
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            courseRepositoryMock = null;
            studentReferenceDataRepositoryMock = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            academicLevelCollection = null;
        }

        private async Task BuildMocksForGetAllAndGetById()
        {
            //GetSubjectsAsync
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync()).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(true)).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(false)).ReturnsAsync(allSubjects);

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(true)).ReturnsAsync(allGradeScheme);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(false)).ReturnsAsync(allGradeScheme);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(allDepartments);

            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                    {
                                        new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                    };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "IN", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d7", "E", "Transfer Credit", Domain.Student.Entities.CreditType.Exchange));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d6", "N", "Transfer Credit", Domain.Student.Entities.CreditType.None));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d5", "O", "Transfer Credit", Domain.Student.Entities.CreditType.Other));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(true)).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(false)).ReturnsAsync(creditCategoryCollection);
        }

        private void BuildCreditCategories(IEnumerable<Domain.Student.Entities.Course> allEntityCourses)
        {
            int counter = 0;
            foreach (var course in allEntityCourses)
            {
                if (counter < 6)
                {
                    course.LocalCreditType = creditCategoryCollection[counter].Code;
                    counter++;
                }
                else if (counter == 6)
                {
                    course.LocalCreditType = string.Empty;
                    break;
                }
            }
        }

        [TestMethod]
        public async Task CourseService_GetCourses4Async_GetAll()
        {
            string empty = string.Empty;

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, null, null, empty, null, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, null, null, empty, null, empty, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses4Async_GetCourseByGuid3Async()
        {
            //Arrange
            var entityCourse = allEntityCourses.Take(1).FirstOrDefault();
            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(entityCourse.Guid))
                 .ReturnsAsync(entityCourse);
            //Act
            var result = await courseService.GetCourseByGuid3Async(entityCourse.Guid);
            //Assert
            Assert.AreEqual(result.Id, entityCourse.Guid);
            Assert.AreEqual(result.Description, entityCourse.Description);
            Assert.AreEqual(result.EffectiveEndDate, entityCourse.EndDate);
            Assert.AreEqual(result.EffectiveStartDate, entityCourse.StartDate);
        }

        [TestMethod]
        public async Task CourseService_GetCourses4Async_GetCourseByGuid3Async_Subject()
        {
            acctGuidForFilter = allSubjects.FirstOrDefault(i => i.Code.Equals("HIST", StringComparison.OrdinalIgnoreCase)).Guid;
            var expected = allEntityCourses.Where(i => i.SubjectCode.Equals("HIST", StringComparison.OrdinalIgnoreCase));
            string empty = string.Empty;

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), "HIST", empty, null, null, empty, null, empty, empty, empty, empty, false))
              .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), acctGuidForFilter, empty, null, null, empty, null, empty, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);

        }

        [TestMethod]
        public async Task CourseService_GetCourses4Async_GetCourseByGuid3Async_Number()
        {
            var expected = allEntityCourses.Where(i => i.Number.Equals("200", StringComparison.OrdinalIgnoreCase));
            string empty = string.Empty;

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, "200", null, null, empty, null, empty, empty, empty, empty, false))
                 .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, "200", null, null, empty, null, empty, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);

        }

        [TestMethod]
        public async Task CourseService_GetCourses4Async_GetCourseByGuid3Async_AcademicLevel()
        {
            var expected = allEntityCourses.Where(i => i.AcademicLevelCode.Equals("UG", StringComparison.OrdinalIgnoreCase));
            string acadLevelId = "1df164eb-8178-4321-a9f7-24f12d3991d8";
            string empty = string.Empty;

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, new List<string>() { "UG" }, null, empty, null, empty, empty, empty, empty, false))
                 .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, new List<string> { acadLevelId }, null, empty, null, empty, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);

        }

        [TestMethod]
        public async Task CourseService_GetCourses4Async_GetCourseByGuid3Async_OwningInstitutionUnits()
        {
            var expected = allEntityCourses.Where(i => i.Departments.Any(a => a.AcademicDepartmentCode.Equals("HIST", StringComparison.OrdinalIgnoreCase)));
            string owningInstitutionUnitsId = allDepartments.Where(i => i.Code.Equals("HIST", StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Guid;//expected.FirstOrDefault().Guid;
            string empty = string.Empty;

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, null, new List<string>() { "HIST" }, empty, null, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, null, new List<string> { owningInstitutionUnitsId }, empty, null, empty, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses4Async_GetCourseByGuid3Async_Title()
        {
            string title = "World History to WWII";
            var expected = allEntityCourses.Where(i => i.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            string empty = string.Empty;

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, null, null, title, null, empty, empty, empty, empty, false))
             .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, null, null, title, null, empty, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses4Async_GetCourseByGuid3Async_InstructionalMethod()
        {
            string instructionalMethod = "9c3b805d-cfe6-483b-86c3-4c20562f8c15";
            var expected = allEntityCourses.Where(i => i.InstructionalMethodCodes.Any(a => a.Equals("LG", StringComparison.OrdinalIgnoreCase)));
            string empty = string.Empty;

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, null, null, empty, new List<string>() { "LG" }, empty, empty, empty, empty, false))
                 .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, null, null, empty, new List<string> { instructionalMethod }, empty, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses4Async_GetCourseByGuid3Async_StartOn()
        {
            var schedulingStartOn = new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Unspecified);
            var expected = allEntityCourses.Where(i => i.StartDate.Equals(schedulingStartOn));
            string empty = string.Empty;

            //Arrange
            sectionRepositoryMock.Setup(d => d.GetUnidataFormattedDate("2001-01-01")).ReturnsAsync(schedulingStartOn.ToString());
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, null, null, empty, null, schedulingStartOn.ToString(), empty, empty, empty, false))
               .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));
            //Act
            var result = await courseService.GetCourses4Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, null, null, empty, null, "2001-01-01", empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }
    }

    [TestClass]
    public class CourseService_GetCourses5Async
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;
        List<Dtos.Course5> allDtoCourses = new List<Dtos.Course5>();
        List<Ellucian.Colleague.Dtos.Credit3> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CourseType> courseTypes;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TopicCode> topicCodes;
        IEnumerable<Domain.Student.Entities.CourseTitleType> courseTitleTypes = null;
        IEnumerable<Domain.Student.Entities.CourseStatuses> courseStatuses = null;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AdministrativeInstructionalMethod> administrativeInstructionalMethodCollection = new List<Domain.Student.Entities.AdministrativeInstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        List<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string acctGuidForFilter = string.Empty;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForGetAllAndGetById();

            allEntityCourses.First().AddInstructionalMethodCode("LG");
            allEntityCourses.First().GradeSchemeCode = "UG";
            BuildCreditCategories(allEntityCourses);

            courseService = new CourseService
                            (
                                adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                            );
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            courseRepositoryMock = null;
            studentReferenceDataRepositoryMock = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            academicLevelCollection = null;
            courseTypes = null;
            topicCodes = null;
            courseTitleTypes = null;
            courseStatuses = null;
        }

        private async Task BuildMocksForGetAllAndGetById()
        {
            //GetSubjectsAsync
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync()).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(true)).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(false)).ReturnsAsync(allSubjects);
            foreach (var entity in allSubjects)
                studentReferenceDataRepositoryMock.Setup(s => s.GetSubjectGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);
            foreach (var entity in courseLevelCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseLevelGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalMethodCollection);
            foreach (var entity in instructionalMethodCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetInstructionalMethodGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            administrativeInstructionalMethodCollection.Add(new Domain.Student.Entities.AdministrativeInstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", "D8CED21A-F220-4F79-9544-706E13B51972"));
            administrativeInstructionalMethodCollection.Add(new Domain.Student.Entities.AdministrativeInstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", ""));
            administrativeInstructionalMethodCollection.Add(new Domain.Student.Entities.AdministrativeInstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", ""));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAdministrativeInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(administrativeInstructionalMethodCollection);
            foreach (var entity in administrativeInstructionalMethodCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetAdministrativeInstructionalMethodGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);
            foreach (var entity in academicLevelCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(true)).ReturnsAsync(allGradeScheme);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(false)).ReturnsAsync(allGradeScheme);
            foreach (var entity in allGradeScheme)
                studentReferenceDataRepositoryMock.Setup(s => s.GetGradeSchemeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(allDepartments);
            foreach (var entity in allDepartments)
                referenceDataRepositoryMock.Setup(s => s.GetDepartments2GuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                    {
                                        new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                    };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "IN", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d7", "E", "Transfer Credit", Domain.Student.Entities.CreditType.Exchange));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d6", "N", "Transfer Credit", Domain.Student.Entities.CreditType.None));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d5", "O", "Transfer Credit", Domain.Student.Entities.CreditType.Other));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(true)).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(false)).ReturnsAsync(creditCategoryCollection);
            foreach (var entity in creditCategoryCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCreditCategoriesGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            courseTypes = new List<Ellucian.Colleague.Domain.Student.Entities.CourseType>()
            {
                new Domain.Student.Entities.CourseType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "MUS", "Descr MUS", true)
            };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseTypesAsync(It.IsAny<bool>())).ReturnsAsync(courseTypes);
            foreach (var entity in courseTypes)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseTypeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);


            topicCodes = new List<Ellucian.Colleague.Domain.Student.Entities.TopicCode>()
            {
                new Domain.Student.Entities.TopicCode("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "MUS", "Descr MUS")
            };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetTopicCodesAsync(It.IsAny<bool>())).ReturnsAsync(topicCodes);
            foreach (var entity in topicCodes)
                studentReferenceDataRepositoryMock.Setup(s => s.GetTopicCodeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            courseTitleTypes = new List<Domain.Student.Entities.CourseTitleType>()
            {
                new Domain.Student.Entities.CourseTitleType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "SHORT", "Descr"),
                new Domain.Student.Entities.CourseTitleType("8C3B805D-CFE6-483B-86C3-4C20562F8C15", "LONG", "Descr")
            };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseTitleTypesAsync(It.IsAny<bool>())).ReturnsAsync(courseTitleTypes);
            foreach (var entity in courseTitleTypes)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseTitleTypeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            courseStatuses = new List<Domain.Student.Entities.CourseStatuses>()
            {
                new Domain.Student.Entities.CourseStatuses("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CS", "Descr CS")
            };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseStatusesAsync(It.IsAny<bool>())).ReturnsAsync(courseStatuses);
            foreach (var entity in courseStatuses)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseStatusGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);


        }

        private void BuildCreditCategories(IEnumerable<Domain.Student.Entities.Course> allEntityCourses)
        {
            int counter = 0;
            foreach (var course in allEntityCourses)
            {
                if (counter < 6)
                {
                    course.LocalCreditType = creditCategoryCollection[counter].Code;
                    counter++;
                }
                else if (counter == 6)
                {
                    course.LocalCreditType = string.Empty;
                    break;
                }
            }
        }

        [TestMethod]
        public async Task CourseService_GetCourses5Async_GetAll_WrongSubjectId_EmptySet()
        {
            string empty = string.Empty;
            List<string> list = new List<string>() { "1" };

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), "10", empty, list, list, empty, list, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "10", empty, list, list, list, list, empty, empty, empty, list, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses5Async_GetAll_WrongAcadLevelId_EmptySet()
        {
            string empty = string.Empty;
            List<string> list = new List<string>() { "1" };

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, list, list, empty, list, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, list, list, list, list, empty, empty, empty, list, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }


        [TestMethod]
        public async Task CourseService_GetCourses5Async_GetAll_WrongOwningInstitutionUnitsId_EmptySet()
        {
            string empty = string.Empty;
            List<string> list = new List<string>() { "1" };
            List<string> acadLevlList = new List<string>() { "9c3b805d-cfe6-483b-86c3-4c20562f8c15" };

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, acadLevlList, list, empty, list, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, acadLevlList, list, list, list, empty, empty, empty, list, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses5Async_GetAll_WrongInstructionalMethodsId_EmptySet()
        {
            string empty = string.Empty;
            List<string> list = new List<string>() { "1" };
            List<string> instrList = new List<string>() { "-1" };

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, null, null, empty, instrList, empty, empty, empty, empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, null, null, null, instrList, empty, empty, empty, list, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses5Async_GetAll_WrongCategoryId_EmptySet()
        {
            string empty = string.Empty;
            List<string> list = new List<string>() { "1" };
            List<string> catIdList = new List<string>() { "-1" };

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, null, null, empty, null, empty, empty, empty, "-1", false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, null, null, null, null, empty, empty, empty, catIdList, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses5Async_GetAll_WrongTopicId_EmptySet()
        {
            string empty = string.Empty;
            List<string> list = new List<string>() { "1" };
            List<string> catIdList = new List<string>() { "-1" };

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), empty, empty, null, null, empty, null, empty, empty, "-1", empty, false))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses5Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), empty, empty, null, null, null, null, empty, empty, "-1", null, empty);
            //Assert
            Assert.AreEqual(allDtoCourses.Count(), result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses5Async_GetAll()
        {
            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course>, int>(allEntityCourses, allEntityCourses.ToList().Count));

            //Act
            var result = await courseService.GetCourses5Async(0, 1, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>());
            //Assert
            Assert.IsNotNull(result.Item2);
        }

        [TestMethod]
        public async Task CourseService_GetCourses5Async_GetById()
        {
            var entity = allEntityCourses.First();
            //Arrange
            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(entity);

            //Act
            var result = await courseService.GetCourseByGuid5Async(It.IsAny<string>());
            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.Guid, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_GetCourses5Async_GetAll_Exception()
        {
            string empty = string.Empty;

            //Arrange
            courseRepositoryMock.Setup(c => c.GetPagedCoursesAsync(0, 1, empty, empty, null, null, null, null, empty, empty, empty, null, empty, true)).ThrowsAsync(new RepositoryException());

            //Act
            var result = await courseService.GetCourses5Async(0, 1, false, "", "", null, null, null, null, "", "", "", null, "");
        }
    }

    [TestClass]
    public class CourseService_PostCourses4Async : CurrentUserSetup
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;

        List<Dtos.Course4> allDtoCourses = new List<Dtos.Course4>();
        List<Ellucian.Colleague.Dtos.Credit3> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string code;
        string description;
        ExternalMapping externalMapping;
        ExternalMappingItem item1;
        ExternalMappingItem item2;
        ExternalMappingItem item3;
        ExternalMappingItem item4;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            curntUserFactory = new ThirdPartyUserFactory();
            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForCourseCreateUpdate();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course4>();
            foreach (var course in allEntityCourses)
            {
                Ellucian.Colleague.Dtos.Course4 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course4>(course);
                allDtoCourses.Add(target);
            }

            courseService = new CourseService
                                (
                                    adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                    studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                    termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                    curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                                );
        }

        private async Task BuildMocksForCourseCreateUpdate()
        {
            //GetSubjectsAsync
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync()).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(true)).ReturnsAsync(allSubjects);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(false)).ReturnsAsync(allSubjects);
            foreach (var entity in allSubjects)
                studentReferenceDataRepositoryMock.Setup(s => s.GetSubjectGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            code = "LDM.SUBJECTS";
            description = "Mapping of Subjects to Departments";
            externalMapping = new ExternalMapping(code, description);
            item1 = new ExternalMappingItem("ACCT") { NewCode = "BUSN" };
            item2 = new ExternalMappingItem("AVIA") { NewCode = "AUTO" };
            item3 = new ExternalMappingItem("COMP") { NewCode = "MATH" };
            item4 = new ExternalMappingItem("HIST") { NewCode = "HIST" };

            externalMapping.AddItem(item1);
            externalMapping.AddItem(item2);
            externalMapping.AddItem(item3);
            externalMapping.AddItem(item4);
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(new Domain.Student.Entities.CurriculumConfiguration()
                {
                    SubjectDepartmentMapping = externalMapping,
                    DefaultAcademicLevelCode = "UG",
                    DefaultCourseLevelCode = "UG",
                    ApproverId = "123",
                    ApprovingAgencyId = "123",
                    CourseActiveStatusCode = "A",
                    CourseInactiveStatusCode = "C"
                });


            var creditCategoryOne = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryOne.Detail = new GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15");
            creditCategoryOne.CreditType = CreditCategoryType3.Institutional;

            var creditCategoryTwo = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryTwo.Detail = new GuidObject2("0736a2d4-7733-4e0d-bfc0-0fe92a165c97");
            creditCategoryTwo.CreditType = CreditCategoryType3.Institutional;

            credits = new List<Ellucian.Colleague.Dtos.Credit3>()
                            { new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryOne, Measure = CreditMeasure2.Credit, Minimum = 3.00000m, Maximum = 6.00000m, Increment = 1.00000m },
                                new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryTwo, Measure = CreditMeasure2.CEU, Minimum = 4.00000m }
                            };

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);
            foreach (var entity in courseLevelCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseLevelGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);
            foreach (var entity in instructionalMethodCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetInstructionalMethodGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);
            foreach (var entity in academicLevelCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);
            foreach (var entity in allGradeScheme)
                studentReferenceDataRepositoryMock.Setup(s => s.GetGradeSchemeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            foreach (var entity in allDepartments)
                referenceDataRepositoryMock.Setup(s => s.GetDepartments2GuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                        {
                                            new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                        };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(true)).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(false)).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync()).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(true)).ReturnsAsync(creditCategoryCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(false)).ReturnsAsync(creditCategoryCollection);
            foreach (var entity in creditCategoryCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCreditCategoriesGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>())).Returns(Task.FromResult(allDepartments));
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            code = null;
            description = null;
            externalMapping = null;
            item1 = null;
            item2 = null;
            item3 = null;
            item4 = null;
        }

        [TestMethod]
        public async Task CourseService_CreateCourse4Async()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(newEntityCourseForCreate.Guid))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid4Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionMethods.Add(new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"));

            courseRepositoryMock.Setup(crm => crm.CreateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.CreateCourse4Async(result, false);
            //Assert
            Assert.AreEqual(result.Id, newEntityCourseForCreate.Guid);
        }

        [TestMethod]
        public async Task CourseService_CreateCourse4Async_GUID_Reqd()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(newEntityCourseForCreate.Guid))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid4Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionMethods.Add(new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"));

            result.Id = Guid.Empty.ToString();

            courseRepositoryMock.Setup(crm => crm.CreateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.CreateCourse4Async(result, false);
            //Assert
            Assert.AreEqual(newEntityCourseForCreate.Guid, resultCreate.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CourseService_CreateCourse4Async_ArgumentNullException_NUll_Course()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            await courseService.CreateCourse4Async(null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_Empty_Title()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = string.Empty;
            courseDto.Subject.Id = "234";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_Subject_Null()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = null;
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_Subject_Id_Empty()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = new GuidObject2();
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_Number_Empty()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = new GuidObject2("200");
            courseDto.Number = "";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_Number_Length_GT_7()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = new GuidObject2("200");
            courseDto.Number = "12345678";
            courseDto.EffectiveStartDate = DateTime.Now;

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_EffectiveDate_null()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject = new GuidObject2("200");
            courseDto.Number = "1234567";

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_CourseLevelsNull()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.CourseLevels = new List<GuidObject2>() { new GuidObject2() };

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_InstructionMethods_Null()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.CourseLevels = new List<GuidObject2>() { new GuidObject2("222") };
            courseDto.InstructionMethods = new List<GuidObject2>() { new GuidObject2() };

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_AcadLevels()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.AcademicLevels = new List<GuidObject2>() { new GuidObject2("") };

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_GradeSchemes()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.GradeSchemes = new List<GuidObject2>() { new GuidObject2("") };

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_OwningInstitutionUnits_Null()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = null } };

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_OwningInstitutionUnits_Id_EmptyString()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = new GuidObject2() } };

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_OwningInstitutionUnits_CreditCategory()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1") } };
            courseDto.Credits = new List<Credit3>() { new Credit3() { CreditCategory = null } };

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_OwningInstitutionUnits_CreditCategory_CreditType()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1") } };
            courseDto.Credits = new List<Credit3>() { new Credit3() { CreditCategory = new CreditIdAndTypeProperty2() { CreditType = null } } };

            await courseService.CreateCourse4Async(courseDto, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CourseService_CreateCourse4Async_ArgumentException_OwningInstitutionUnits_CreditCategory_Detail()
        {
            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            Dtos.Course4 courseDto = new Dtos.Course4();
            courseDto.Id = "123";
            courseDto.Title = "Some Title";
            courseDto.Subject.Id = "569";
            courseDto.Number = "100";
            courseDto.EffectiveStartDate = DateTime.Now;
            courseDto.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = new GuidObject2("1") } };
            courseDto.Credits = new List<Credit3>() { new Credit3() { CreditCategory = new CreditIdAndTypeProperty2() { CreditType = CreditCategoryType3.ContinuingEducation, Detail = new GuidObject2() } } };

            await courseService.CreateCourse4Async(courseDto, false);
        }

        private Domain.Student.Entities.Course BuildCourseForCreate(Domain.Student.Entities.Course entityCourse)
        {
            Ellucian.Colleague.Domain.Student.Entities.Course course = new Ellucian.Colleague.Domain.Student.Entities.Course
                ("", entityCourse.Title, null, entityCourse.Departments, entityCourse.SubjectCode, entityCourse.Number, entityCourse.AcademicLevelCode,
                entityCourse.CourseLevelCodes, 3.0m, 1.0m, entityCourse.CourseApprovals);
            // GUID
            course.Guid = entityCourse.Guid;

            // DescriptionSearch
            course.Description = entityCourse.Description;


            course.LocationCodes = entityCourse.LocationCodes;
            // Max credits
            course.MaximumCredits = 4.0m;

            // Variable Credit Increment
            course.VariableCreditIncrement = 1.0m;

            // Start/End date
            course.StartDate = entityCourse.StartDate;
            course.EndDate = entityCourse.EndDate;

            // Topic Code
            course.TopicCode = "TopicCode";

            // Add course types
            course.AddType("TYPEA");

            // Add local credit type - the local string value of the credit type.
            course.LocalCreditType = "I";

            return course;
        }
    }

    [TestClass]
    public class CourseService_PutCourses4Async : CurrentUserSetup
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;

        List<Dtos.Course4> allDtoCourses = new List<Dtos.Course4>();
        List<Ellucian.Colleague.Dtos.Credit3> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;

        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string code;
        string description;
        ExternalMapping externalMapping;
        ExternalMappingItem item1;
        ExternalMappingItem item2;
        ExternalMappingItem item3;
        ExternalMappingItem item4;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            curntUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForCourseCreateUpdate();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course4>();
            foreach (var course in allEntityCourses)
            {
                Ellucian.Colleague.Dtos.Course4 target = Mapper.Map<Domain.Student.Entities.Course, Ellucian.Colleague.Dtos.Course4>(course);
                allDtoCourses.Add(target);
            }

            courseService = new CourseService
                                (
                                    adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                    studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                    termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                    curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                                );
        }

        private async Task BuildMocksForCourseCreateUpdate()
        {
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(It.IsAny<bool>())).Returns(Task.FromResult(allSubjects));
            foreach (var entity in allSubjects)
                studentReferenceDataRepositoryMock.Setup(s => s.GetSubjectGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            code = "LDM.SUBJECTS";
            description = "Mapping of Subjects to Departments";
            externalMapping = new ExternalMapping(code, description);
            item1 = new ExternalMappingItem("ACCT") { NewCode = "BUSN" };
            item2 = new ExternalMappingItem("AVIA") { NewCode = "AUTO" };
            item3 = new ExternalMappingItem("COMP") { NewCode = "MATH" };
            item4 = new ExternalMappingItem("HIST") { NewCode = "HIST" };

            externalMapping.AddItem(item1);
            externalMapping.AddItem(item2);
            externalMapping.AddItem(item3);
            externalMapping.AddItem(item4);
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(new Domain.Student.Entities.CurriculumConfiguration()
                {
                    SubjectDepartmentMapping = externalMapping,
                    DefaultAcademicLevelCode = "UG",
                    DefaultCourseLevelCode = "UG",
                    ApproverId = "123",
                    ApprovingAgencyId = "123",
                    CourseActiveStatusCode = "A",
                    CourseInactiveStatusCode = "C"
                });

            var creditCategoryOne = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryOne.Detail = new GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15");
            creditCategoryOne.CreditType = CreditCategoryType3.Institutional;

            var creditCategoryTwo = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryTwo.Detail = new GuidObject2("0736a2d4-7733-4e0d-bfc0-0fe92a165c97");
            creditCategoryTwo.CreditType = CreditCategoryType3.Institutional;

            credits = new List<Ellucian.Colleague.Dtos.Credit3>()
                            { new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryOne },
                                new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryTwo}
                            };

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);
            foreach (var entity in courseLevelCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseLevelGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);
            foreach (var entity in instructionalMethodCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetInstructionalMethodGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);


            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);
            foreach (var entity in academicLevelCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);
            foreach (var entity in allGradeScheme)
                studentReferenceDataRepositoryMock.Setup(s => s.GetGradeSchemeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(true)).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(false)).ReturnsAsync(allDepartments);
            foreach (var entity in allDepartments)
                referenceDataRepositoryMock.Setup(s => s.GetDepartments2GuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                        {
                                            new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                        };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(true)).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(false)).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(creditCategoryCollection);
            foreach (var entity in creditCategoryCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCreditCategoriesGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            code = null;
            description = null;
            externalMapping = null;
            item1 = null;
            item2 = null;
            item3 = null;
            item4 = null;
        }

        [TestMethod]
        public async Task CourseService_UpdateCourse4Async()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuidAsync(newEntityCourseForCreate.Guid))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid4Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionMethods.Add(new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"));

            courseRepositoryMock.Setup(crm => crm.UpdateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.UpdateCourse4Async(result, false);
            //Assert
            Assert.AreEqual(result.Id, newEntityCourseForCreate.Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CourseService_UpdateCourse4Async_ArgumentNullException()
        {
            await courseService.UpdateCourse4Async(null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CourseService_UpdateCourse4Async_KeyNotFoundException()
        {
            var result = new Dtos.Course4();
            result.Id = string.Empty;
            //Act
            await courseService.UpdateCourse4Async(result, false);
        }


        private Domain.Student.Entities.Course BuildCourseForCreate(Domain.Student.Entities.Course entityCourse)
        {
            Ellucian.Colleague.Domain.Student.Entities.Course course = new Ellucian.Colleague.Domain.Student.Entities.Course
                ("", entityCourse.Title, null, entityCourse.Departments, entityCourse.SubjectCode, entityCourse.Number, entityCourse.AcademicLevelCode,
                entityCourse.CourseLevelCodes, 3.0m, 1.0m, entityCourse.CourseApprovals);
            // GUID
            course.Guid = entityCourse.Guid;

            // DescriptionSearch
            course.Description = entityCourse.Description;


            course.LocationCodes = entityCourse.LocationCodes;
            // Max credits
            course.MaximumCredits = 4.0m;

            // Variable Credit Increment
            course.VariableCreditIncrement = 1.0m;

            // Start/End date
            course.StartDate = entityCourse.StartDate;
            course.EndDate = entityCourse.EndDate;

            // Topic Code
            course.TopicCode = "TopicCode";

            // Add course types
            course.AddType("TYPEA");

            // Add local credit type - the local string value of the credit type.
            course.LocalCreditType = "I";

            return course;
        }
    }

    [TestClass]
    public class CourseService_PutPostCourses5Async : Ellucian.Colleague.Coordination.Base.Tests.Services.CurrentUserSetup
    {
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ICourseRepository> courseRepositoryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        Mock<IRequirementRepository> requirementRepositoryMock;
        Mock<ISectionRepository> sectionRepositoryMock;
        Mock<ITermRepository> termRepositoryMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        Mock<IColleagueTransactionFactory> transFactoryMock;

        ICurrentUserFactory curntUserFactory;
        CourseService courseService;

        List<Dtos.Course5> allDtoCourses = new List<Dtos.Course5>();
        List<Ellucian.Colleague.Dtos.Credit3> credits;

        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allEntityCourses;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GradeScheme> allGradeScheme;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment> academicDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CourseType> courseTypes;
        IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TopicCode> topicCodes;
        IEnumerable<Domain.Student.Entities.CourseTitleType> courseTitleTypes = null;
        IEnumerable<Domain.Student.Entities.CourseStatuses> courseStatuses = null;
        IEnumerable<Division> divisions = null;
        IEnumerable<School> schools = null;
        Ellucian.Colleague.Domain.Student.Entities.CurriculumConfiguration currConfiguration;


        ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();
        ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();
        ICollection<Domain.Student.Entities.AdministrativeInstructionalMethod> administrativeInstructionalMethodCollection = new List<Domain.Student.Entities.AdministrativeInstructionalMethod>();
        ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();
        ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        string code;
        string description;
        ExternalMapping externalMapping;
        ExternalMappingItem item1;
        ExternalMappingItem item2;
        ExternalMappingItem item3;
        ExternalMappingItem item4;

        [TestInitialize]
        public async void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            courseRepositoryMock = new Mock<ICourseRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            requirementRepositoryMock = new Mock<IRequirementRepository>();
            sectionRepositoryMock = new Mock<ISectionRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();
            studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            transFactoryMock = new Mock<IColleagueTransactionFactory>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            curntUserFactory = new ThirdPartyUserFactory();
            allEntityCourses = new TestCourseRepository().GetAsync().Result.Take(41);
            allSubjects = new TestSubjectRepository().Get();

            await BuildMocksForCourseCreateUpdate();

            courseService = new CourseService
                                (
                                    adapterRegistryMock.Object, courseRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                    studentReferenceDataRepositoryMock.Object, requirementRepositoryMock.Object, sectionRepositoryMock.Object,
                                    termRepositoryMock.Object, ruleRepositoryMock.Object, studentConfigurationRepositoryMock.Object, baseConfigurationRepository,
                                    curntUserFactory, roleRepositoryMock.Object, loggerMock.Object
                                );
        }

        private async Task BuildMocksForCourseCreateUpdate()
        {
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetSubjectsAsync(It.IsAny<bool>())).Returns(Task.FromResult(allSubjects));
            foreach (var entity in allSubjects)
                studentReferenceDataRepositoryMock.Setup(s => s.GetSubjectGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            code = "LDM.SUBJECTS";
            description = "Mapping of Subjects to Departments";
            externalMapping = new ExternalMapping(code, description);
            item1 = new ExternalMappingItem("ACCT") { NewCode = "BUSN" };
            item2 = new ExternalMappingItem("AVIA") { NewCode = "AUTO" };
            item3 = new ExternalMappingItem("COMP") { NewCode = "MATH" };
            item4 = new ExternalMappingItem("HIST") { NewCode = "HIST" };

            externalMapping.AddItem(item1);
            externalMapping.AddItem(item2);
            externalMapping.AddItem(item3);
            externalMapping.AddItem(item4);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var creditCategoryOne = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryOne.Detail = new GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15");
            creditCategoryOne.CreditType = CreditCategoryType3.Institutional;

            var creditCategoryTwo = new Dtos.DtoProperties.CreditIdAndTypeProperty2();
            creditCategoryTwo.Detail = new GuidObject2("0736a2d4-7733-4e0d-bfc0-0fe92a165c97");
            creditCategoryTwo.CreditType = CreditCategoryType3.Institutional;

            credits = new List<Ellucian.Colleague.Dtos.Credit3>()
                            { new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryOne },
                                new Ellucian.Colleague.Dtos.Credit3() { CreditCategory = creditCategoryTwo}
                            };

            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("4aa187ae-6d22-4b10-a2e2-1304ebc18176", "400", "Third Year"));
            courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("d9f42a0f-39de-44bc-87af-517619141bde", "500", "Third Year"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);
            foreach (var entity in courseLevelCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseLevelGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", false));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", true));
            instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", false));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(instructionalMethodCollection);
            foreach (var entity in instructionalMethodCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetInstructionalMethodGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            administrativeInstructionalMethodCollection.Add(new Domain.Student.Entities.AdministrativeInstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "LG", "D8CED21A-F220-4F79-9544-706E13B51972"));
            administrativeInstructionalMethodCollection.Add(new Domain.Student.Entities.AdministrativeInstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "30", "705F052C-7B63-492D-A7CA-5769CE003274"));
            administrativeInstructionalMethodCollection.Add(new Domain.Student.Entities.AdministrativeInstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "04", "67B0664B-0650-4C88-ACC6-FB0C689CB519"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAdministrativeInstructionalMethodsAsync(It.IsAny<bool>())).ReturnsAsync(administrativeInstructionalMethodCollection);
            foreach (var entity in academicLevelCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
            academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);
            foreach (var entity in academicLevelCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetAcademicLevelsGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            allGradeScheme = await new TestStudentReferenceDataRepository().GetGradeSchemesAsync();
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync()).ReturnsAsync(allGradeScheme);
            foreach (var entity in allGradeScheme)
                studentReferenceDataRepositoryMock.Setup(s => s.GetGradeSchemeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            allDepartments = new TestDepartmentRepository().Get();
            referenceDataRepositoryMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(true)).ReturnsAsync(allDepartments);
            referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(false)).ReturnsAsync(allDepartments);
            foreach (var entity in allDepartments)
                referenceDataRepositoryMock.Setup(s => s.GetDepartments2GuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            academicDepartments = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment>()
                                        {
                                            new Ellucian.Colleague.Domain.Student.Entities.AcademicDepartment("f84e8e9d-1254-4331-a0a5-04d494a6eaa8", "HIST", "History", true)
                                        };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync()).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(true)).ReturnsAsync(academicDepartments);
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicDepartmentsAsync(false)).ReturnsAsync(academicDepartments);

            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
            creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(creditCategoryCollection);
            foreach (var entity in creditCategoryCollection)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCreditCategoriesGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            courseTypes = new List<Ellucian.Colleague.Domain.Student.Entities.CourseType>()
            {
                new Domain.Student.Entities.CourseType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "MUS", "Descr MUS", true)
            };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseTypesAsync(It.IsAny<bool>())).ReturnsAsync(courseTypes);
            foreach (var entity in courseTypes)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseTypeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            topicCodes = new List<Ellucian.Colleague.Domain.Student.Entities.TopicCode>()
            {
                new Domain.Student.Entities.TopicCode("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "MUS", "Descr MUS")
            };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetTopicCodesAsync(It.IsAny<bool>())).ReturnsAsync(topicCodes);
            foreach (var entity in topicCodes)
                studentReferenceDataRepositoryMock.Setup(s => s.GetTopicCodeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            courseTitleTypes = new List<Domain.Student.Entities.CourseTitleType>()
            {
                new Domain.Student.Entities.CourseTitleType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "SHORT", "Descr"),
                new Domain.Student.Entities.CourseTitleType("8C3B805D-CFE6-483B-86C3-4C20562F8C15", "LONG", "Descr")
            };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseTitleTypesAsync(It.IsAny<bool>())).ReturnsAsync(courseTitleTypes);
            foreach (var entity in courseTitleTypes)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseTitleTypeGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);


            courseStatuses = new List<Domain.Student.Entities.CourseStatuses>()
            {
                new Domain.Student.Entities.CourseStatuses("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "A", "Active")
                {
                    Status = Domain.Student.Entities.CourseStatus.Active
                },
                new Domain.Student.Entities.CourseStatuses("9C3B905D-CFE6-483B-86C3-4C20562F8C15", "U", "Unknown")
                {
                    Status = Domain.Student.Entities.CourseStatus.Unknown
                },
                new Domain.Student.Entities.CourseStatuses("9C3B105D-CFE6-483B-86C3-4C20562F8C15", "T", "Terminated")
                {
                    Status = Domain.Student.Entities.CourseStatus.Terminated
                }
            };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseStatusesAsync(It.IsAny<bool>())).ReturnsAsync(courseStatuses);
            foreach (var entity in courseStatuses)
                studentReferenceDataRepositoryMock.Setup(s => s.GetCourseStatusGuidAsync(entity.Code)).ReturnsAsync(entity.Guid);

            divisions = new List<Division>()
            {
                new Division("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional") { InstitutionId = "1" }
            };
            referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>())).ReturnsAsync(divisions);

            schools = new List<School>()
            {
                new School("8C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional") { InstitutionId = "1" }
            };
            referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>())).ReturnsAsync(schools);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseService = null;
            allEntityCourses = null;
            allDtoCourses = null;
            allGradeScheme = null;
            allGradeScheme = null;
            credits = null;
            academicDepartments = null;
            allDepartments = null;
            allSubjects = null;
            courseLevelCollection = null;
            instructionalMethodCollection = null;
            code = null;
            description = null;
            externalMapping = null;
            item1 = null;
            item2 = null;
            item3 = null;
            item4 = null;
        }

        [TestMethod]
        public async Task CourseService_UpdateCourse5Async()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionalMethodDetails.Add(new InstructionalMethodDetail()
            {
                InstructionalMethod = new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"),
                InstructionalDeliveryMethod = new GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15")
            });

            courseRepositoryMock.Setup(crm => crm.UpdateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.UpdateCourse5Async(result, false);
            //Assert
            Assert.AreEqual(result.Id, newEntityCourseForCreate.Guid);
        }

        [TestMethod]
        public async Task CourseService_CreateCourse5Async()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            // result.Credits = credits;
            result.InstructionalMethodDetails.Add(new InstructionalMethodDetail()
            {
                InstructionalMethod = new Dtos.GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15"),
                InstructionalDeliveryMethod = new GuidObject2("9c3b805d-cfe6-483b-86c3-4c20562f8c15")
            });

            courseRepositoryMock.Setup(crm => crm.CreateCourseAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.Course>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(newEntityCourseForCreate);

            //Act
            var resultCreate = await courseService.CreateCourse5Async(result, false);
            //Assert
            Assert.IsNotNull(resultCreate);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_TitleNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Titles = null;

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("At least one course title must be provided.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_TitleNotAny()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Titles = new List<CoursesTitlesDtoProperty>();

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("At least one course title must be provided.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_TitleValueNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Titles = new List<CoursesTitlesDtoProperty>() { new CoursesTitlesDtoProperty() { Type = new GuidObject2(Guid.NewGuid().ToString()), Value = "" } };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("The title value must be provided for the title object.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_SubjectNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Subject = null;

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Subject is required; no Id supplied.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_SubjectIdNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Subject = new GuidObject2();

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Subject is required; no Id supplied.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CourseNumberEmpty()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Number = string.Empty;

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Course number is required.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CourseNumberGTSeven()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Number = "abcdefgh";

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Course number cannot be longer than 7 characters.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_EffectiveStartDateGTEndDate()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.EffectiveStartDate = DateTime.Today.Date.AddDays(1);
            result.EffectiveEndDate = DateTime.Today.Date;

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("schedulingEndOn cannot be earlier than schedulingStartOn.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CourseLevelIdEmpty()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.CourseLevels = new List<GuidObject2>() { new GuidObject2() };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Course Level id is a required field when Course Levels are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CourseAcadLevelIdEmpty()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.AcademicLevels = new List<GuidObject2>() { new GuidObject2() };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Academic Level id is a required field when Academic Levels are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CourseGradeSchemesNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.GradeSchemes = new List<GradeSchemesDtoProperty>() { new GradeSchemesDtoProperty() { } };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Grade Scheme id is a required field when Grade Schemes are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CourseGradeSchemesEmptyId()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.GradeSchemes = new List<GradeSchemesDtoProperty>() { new GradeSchemesDtoProperty() { GradeScheme = new GuidObject2() } };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Grade Scheme id is a required field when Grade Schemes are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_InstitutionUnitNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit() { InstitutionUnit = null } };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Institution Unit id is a required field when Owning Organizations are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_InstitutionUnitIdNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.OwningInstitutionUnits = new List<OwningInstitutionUnit>()
            {
                new OwningInstitutionUnit()
                {
                    InstitutionUnit = new GuidObject2()
                }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Institution Unit id is a required field when Owning Organizations are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_OwnershipPercentageZero()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.OwningInstitutionUnits = new List<OwningInstitutionUnit>() { new OwningInstitutionUnit()
            { InstitutionUnit = new GuidObject2(Guid.NewGuid().ToString()), OwnershipPercentage = 0 } };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Ownership Percentage is a required field when Owning Organizations are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditsCountGTTwo()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3() { },
                new Credit3() { },
                new Credit3() { }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("A maximum of 2 entries are allowed in the Credits array.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditTypeNotSame()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3() { CreditCategory = new CreditIdAndTypeProperty2() { CreditType = CreditCategoryType3.ContinuingEducation } },
                new Credit3() { CreditCategory = new CreditIdAndTypeProperty2() { CreditType = CreditCategoryType3.Exam } }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("The same Credit Type must be used for each entry in the Credits array.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditMeasureNotSame()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3() { Measure = CreditMeasure2.CEU },
                new Credit3() { Measure = CreditMeasure2.CEU }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Invalid combination of measures 'CEU' and 'CEU'.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditCategoryNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3() { CreditCategory = null, Measure = CreditMeasure2.CEU },
                new Credit3() { CreditCategory = new CreditIdAndTypeProperty2() {  Detail = new GuidObject2(Guid.NewGuid().ToString()) }, Measure = CreditMeasure2.Credit }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Credit Category is required if Credits are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditTypeNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3() { CreditCategory = new CreditIdAndTypeProperty2()
                {  CreditType = null, Detail = new GuidObject2(Guid.NewGuid().ToString()) }, Measure = CreditMeasure2.Credit },
                new Credit3() { CreditCategory = null, Measure = CreditMeasure2.CEU }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Credit Type is required for Credit Categories if Credits are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditCategoryDetailIdNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3() { CreditCategory = new CreditIdAndTypeProperty2()
                {
                    CreditType = CreditCategoryType3.ContinuingEducation,
                    Detail = new GuidObject2() },
                    Measure = CreditMeasure2.Credit
                },
                new Credit3() { CreditCategory = null, Measure = CreditMeasure2.CEU }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Credit Category id within the detail object is required if Credit Category is defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditIncrementNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);

            result.Credits = new List<Credit3>()
            {
                new Credit3() { CreditCategory = new CreditIdAndTypeProperty2()
                {
                    CreditType = CreditCategoryType3.ContinuingEducation,
                    Detail = new GuidObject2( Guid.NewGuid().ToString()) },
                    Measure = CreditMeasure2.Credit,
                    Increment = null,
                    Maximum = 1
                },
                new Credit3() { CreditCategory = null, Measure = CreditMeasure2.CEU }
            };
            try
            {
                await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                var message = "Credit Category is required if Credits are defined.";
                Assert.AreEqual(message, ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditMaximumNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3() { CreditCategory = new CreditIdAndTypeProperty2()
                {
                    CreditType = CreditCategoryType3.ContinuingEducation,
                    Detail = new GuidObject2( Guid.NewGuid().ToString()) },
                    Measure = CreditMeasure2.Credit,
                    Increment = 1,
                    Maximum = null
                },
                new Credit3() { CreditCategory = null, Measure = CreditMeasure2.CEU }
            };

            try { await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                var message = "Credit Maximum is required when Credit Increment exists.";
                Assert.AreEqual(message, ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditMaximumWithCEU()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3() { CreditCategory = new CreditIdAndTypeProperty2()
                {
                    CreditType = CreditCategoryType3.ContinuingEducation,
                    Detail = new GuidObject2( Guid.NewGuid().ToString()) },
                    Measure = CreditMeasure2.CEU,
                    Increment = 1,
                    Maximum = 3
                },
                new Credit3() { CreditCategory = null, Measure = CreditMeasure2.Credit }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Credit Maximum/Increment cannot exist when Credit Measure is 'ceu'.", ex.Errors[0].Message);
                throw ex;
            }
        }

        //Dont run
        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditIncrementWithCEU()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3() { CreditCategory = new CreditIdAndTypeProperty2()
                {
                    CreditType = CreditCategoryType3.ContinuingEducation,
                    Detail = new GuidObject2( Guid.NewGuid().ToString()) },
                    Measure = CreditMeasure2.CEU,
                    Increment = 1,
                    Maximum = 2
                },
                new Credit3() { CreditCategory = null, Measure = CreditMeasure2.Credit }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Credit Maximum/Increment cannot exist when Credit Measure is 'ceu'.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_BillingMinimumLTZero()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Billing = new BillingCreditDtoProperty()
            {
                Minimum = -1
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Billing credits minimum cannot be less than zero.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_BillingMinMaxNotEqual()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Billing = new BillingCreditDtoProperty()
            {
                Maximum = 1
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Billing minimum and maximum credits must be equal.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_BillingIncrementNotEqualZero()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Billing = new BillingCreditDtoProperty()
            {
                Increment = 1
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Billing credits increment must be zero.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CourseTopicIdNullOrEmpty()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Topic = new GuidObject2();

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Id must be supplied for a Topic.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_DuplicateInstrMethods()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            var guid = Guid.NewGuid().ToString();
            result.InstructionalMethodDetails = new List<InstructionalMethodDetail>()
            {
                new InstructionalMethodDetail() { InstructionalMethod = new GuidObject2(guid) },
                new InstructionalMethodDetail() { InstructionalMethod = new GuidObject2(guid) }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual(string.Format("Duplicate instructional method '{0}' is not allowed.", guid), ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_Hours()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            var guid = Guid.NewGuid().ToString();
            result.Hours = new List<CoursesHoursDtoProperty>()
            {
                new CoursesHoursDtoProperty() { AdministrativeInstructionalMethod = new GuidObject2(guid), Minimum = 0 },
                new CoursesHoursDtoProperty() { AdministrativeInstructionalMethod = new GuidObject2(guid), Minimum = 0 }

            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual(string.Format("Duplicate instructional method '{0}' is not allowed.", guid), ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_HoursMinimum_Null()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            var guid = Guid.NewGuid().ToString();
            result.Hours = new List<CoursesHoursDtoProperty>()
            {
                new CoursesHoursDtoProperty() { AdministrativeInstructionalMethod = new GuidObject2(guid), Minimum = null },
                new CoursesHoursDtoProperty() { AdministrativeInstructionalMethod = new GuidObject2(Guid.NewGuid().ToString()) }

            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual(string.Format("hours minimum is required for administrativeInstructionalMethod '{0}'.", guid), ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_SubjectCodeNUll()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Subject = new GuidObject2("WrongId");

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Subject for id 'WrongId' was not found. Valid Subject is required.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_OwningInstitutionUnits_DivisionNotNUll()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.OwningInstitutionUnits = new List<OwningInstitutionUnit>()
            {
                new OwningInstitutionUnit()
                {
                    InstitutionUnit = new GuidObject2("9C3B805D-CFE6-483B-86C3-4C20562F8C15"),
                    OwnershipPercentage = 1
                }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Owning institution unit of type 'division' is not supported.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_OwningInstitutionUnits_SchoolNotNUll()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.OwningInstitutionUnits = new List<OwningInstitutionUnit>()
            {
                new OwningInstitutionUnit()
                {
                    InstitutionUnit = new GuidObject2("8C3B805D-CFE6-483B-86C3-4C20562F8C15"),
                    OwnershipPercentage = 1
                }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Owning institution unit of type 'school' is not supported.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_AcademicLevels_InvalidId()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.AcademicLevels = new List<GuidObject2>()
            {
                new GuidObject2("WrongId")
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Invalid Id 'WrongId' supplied for academicLevels.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CourseLevels_InvalidId()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.CourseLevels = new List<GuidObject2>()
            {
                new GuidObject2("WrongId")
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Invalid Id 'WrongId' supplied for courseLevels.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_GradeSchemes_InvalidId()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.GradeSchemes = new List<GradeSchemesDtoProperty>()
            {
                new GradeSchemesDtoProperty() { GradeScheme = new GuidObject2("WrongId") }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Invalid Id 'WrongId' supplied for gradeSchemes.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CreditCategory_Null()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3()
                {
                    CreditCategory = new CreditIdAndTypeProperty2()
                    {
                       Detail = new GuidObject2("WrongId"),
                       CreditType = CreditCategoryType3.ContinuingEducation
                    }
                }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Invalid Id 'WrongId' supplied for creditCategory.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_DefaultCreditTypeCode_Null()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = new List<Credit3>()
            {
                new Credit3()
                {
                    CreditCategory = new CreditIdAndTypeProperty2()
                    {
                        CreditType = null
                    }
                }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Credit Type is required for Credit Categories if Credits are defined.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_TopicId_Empty()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C",
                DefaultCreditTypeCode = "I"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Topic = new GuidObject2("");

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Id must be supplied for a Topic.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_TopicId_WrongId()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C",
                DefaultCreditTypeCode = "I"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Topic = new GuidObject2("WrongId");

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Invalid Id 'WrongId' supplied for topic.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CategoryId_Empty()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C",
                DefaultCreditTypeCode = "I"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Categories = new List<GuidObject2>()
            {
                new GuidObject2("")
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Course category must contain a valid Id.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_CategoryId_WrongId()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C",
                DefaultCreditTypeCode = "I"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Categories = new List<GuidObject2>()
            {
                new GuidObject2("WrongId")
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Invalid Id 'WrongId' supplied for course category.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_AdministrativeInstructionalMethod_Null()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C",
                DefaultCreditTypeCode = "I"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Hours = new List<CoursesHoursDtoProperty>()
            {
                new CoursesHoursDtoProperty()
                {
                    AdministrativeInstructionalMethod = new GuidObject2("")
                }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Administrative Instructional Method detail requires the Id property.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_AdministrativeInstructionalMethod_WrongId()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C",
                DefaultCreditTypeCode = "I"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Hours = new List<CoursesHoursDtoProperty>()
            {
                new CoursesHoursDtoProperty()
                {
                    AdministrativeInstructionalMethod = new GuidObject2("WrongId"),
                    Minimum = 1
                }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Administrative Instructional method GUID 'WrongId' could not be found.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_ContactPeriod_Null()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C",
                DefaultCreditTypeCode = "I"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Hours = new List<CoursesHoursDtoProperty>()
            {
                new CoursesHoursDtoProperty()
                {
                    AdministrativeInstructionalMethod = new GuidObject2("9C3B805D-CFE6-483B-86C3-4C20562F8C15"),
                    Interval = ContactHoursPeriod.Day,
                    Minimum = 1
                }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Hours interval value 'Day' could not be matched to a contact measure.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_InstructionalMethodDetails_WrongId()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C",
                DefaultCreditTypeCode = "I"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.InstructionalMethodDetails = new List<InstructionalMethodDetail>()
            {
                new InstructionalMethodDetail()
                {
                    InstructionalMethod = new GuidObject2("WrongId"),
                    InstructionalDeliveryMethod = new GuidObject2("1")
                }
            };

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Instructional method GUID 'WrongId' could not be found.", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CourseService_UpdateCourse5Async_MinCreditsNull()
        {
            var entityCourse = new TestCourseRepository().Hist100;
            var newEntityCourseForCreate = BuildCourseForCreate(entityCourse);

            createUpdateCourseRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateAndUpdateCourse));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createUpdateCourseRole });

            courseRepositoryMock.Setup(c => c.GetCourseByGuid2Async(newEntityCourseForCreate.Guid, true))
                 .ReturnsAsync(newEntityCourseForCreate);

            currConfiguration = new Domain.Student.Entities.CurriculumConfiguration()
            {
                SubjectDepartmentMapping = externalMapping,
                DefaultAcademicLevelCode = "UG",
                DefaultCourseLevelCode = "UG",
                ApproverId = "123",
                ApprovingAgencyId = "123",
                CourseActiveStatusCode = "A",
                CourseInactiveStatusCode = "C",
                DefaultCreditTypeCode = "I"
            };
            studentConfigurationRepositoryMock.Setup(scr => scr.GetCurriculumConfigurationAsync())
                .ReturnsAsync(currConfiguration);

            var result = await courseService.GetCourseByGuid5Async(newEntityCourseForCreate.Guid);
            result.Credits = null;
            var courseTitleTypesTemp = new List<Domain.Student.Entities.CourseTitleType>()
            {
                new Domain.Student.Entities.CourseTitleType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "SHORT", "Descr"),
                new Domain.Student.Entities.CourseTitleType("8C3B805D-CFE6-483B-86C3-4C20562F8C15", "LONG", "Descr")
            };
            studentReferenceDataRepositoryMock.Setup(repo => repo.GetCourseTitleTypesAsync(It.IsAny<bool>())).ReturnsAsync(courseTitleTypesTemp);

            try
            {
                var resultCreate = await courseService.UpdateCourse5Async(result, false);
            }
            catch (IntegrationApiException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.HttpStatusCode);
                Assert.AreEqual("Validation.Exception", ex.Errors[0].Code);
                Assert.AreEqual("Either Course Min Credits or CEUs is required ", ex.Errors[0].Message);
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CourseService_UpdateCourse5Async_NullInput_ArgumentNullException()
        {
            //Act
            var resultCreate = await courseService.UpdateCourse5Async(null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CourseService_UpdateCourse5Async_NullCourseId_ArgumentNullException()
        {
            //Act
            var resultCreate = await courseService.UpdateCourse5Async(new Course5(), false);
        }

        private Domain.Student.Entities.Course BuildCourseForCreate(Domain.Student.Entities.Course entityCourse)
        {
            Ellucian.Colleague.Domain.Student.Entities.Course course = new Ellucian.Colleague.Domain.Student.Entities.Course
                ("", entityCourse.Title, null, entityCourse.Departments, entityCourse.SubjectCode, entityCourse.Number, entityCourse.AcademicLevelCode,
                entityCourse.CourseLevelCodes, 3.0m, 1.0m, entityCourse.CourseApprovals);
            // GUID
            course.Guid = entityCourse.Guid;

            // DescriptionSearch
            course.Description = entityCourse.Description;


            course.LocationCodes = entityCourse.LocationCodes;
            // Max credits
            course.MaximumCredits = 4.0m;

            // Variable Credit Increment
            course.VariableCreditIncrement = 1.0m;

            // Start/End date
            course.StartDate = entityCourse.StartDate;
            course.EndDate = entityCourse.EndDate;

            // Topic Code
            course.TopicCode = "TopicCode";

            // Add course types
            course.AddType("TYPEA");

            // Add local credit type - the local string value of the credit type.
            course.LocalCreditType = "I";

            return course;
        }
    }

    public abstract class CurrentUserSetup
    {
        protected Ellucian.Colleague.Domain.Entities.Role createUpdateCourseRole = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.UPDATE.COURSE");

        public class StudentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Samwise",
                        PersonId = "STU1",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Samwise",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        // Represents a third party system like ILP
        public class ThirdPartyUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "ILP",
                        PersonId = "ILP",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "ILP",
                        Roles = new List<string>() { "CREATE.UPDATE.COURSE" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }


    }

    [TestClass]
    public class SearchSectionByKeyword
    {
        private Mock<ICourseRepository> courseRepoMock;
        private ICourseRepository courseRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IRuleRepository> ruleRepoMock;
        private IRuleRepository ruleRepo;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepo;
        private Mock<IReferenceDataRepository> referenceRepoMock;
        private IReferenceDataRepository referenceRepo;
        private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
        private IStudentReferenceDataRepository studentReferenceRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private CourseService courseService;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
        private IEnumerable<string> catalogSubjectCodes;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
        private ILogger logger;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            allSubjects = new TestSubjectRepository().Get();
            catalogSubjectCodes = allSubjects.Where(s => s.ShowInCourseSearch == true).Select(s => s.Code);
            allLocations = new TestLocationRepository().Get();
            allDepartments = new TestDepartmentRepository().Get();

            courseRepoMock = new Mock<ICourseRepository>();
            courseRepo = courseRepoMock.Object;
            allCourses = await new TestCourseRepository().GetAsync();
            courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
            courseRepoMock.Setup<Task<Domain.Student.Entities.Course>>(repo => repo.GetAsync(It.IsAny<string>())).Returns<string>((id) => Task.FromResult(allCourses.FirstOrDefault(c => c.Id == id)));
            catalogCourses = allCourses.Where(c => c.IsCurrent && catalogSubjectCodes.Contains(c.SubjectCode));

            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

            ruleRepoMock = new Mock<IRuleRepository>();
            ruleRepo = ruleRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepo = sectionRepoMock.Object;
            allSections = await new TestSectionRepository().GetAsync();
            var allLevel100Sections = allSections.Where(s => s.CourseLevelCodes.Contains("100"));
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allSections));
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(It.IsAny<List<string>>(), regTerms)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(allLevel100Sections));

            referenceRepoMock = new Mock<IReferenceDataRepository>();
            referenceRepo = referenceRepoMock.Object;
            studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
            studentReferenceRepo = studentReferenceRepoMock.Object;
            studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));
            referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
            referenceRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allLocations);
            referenceRepoMock.Setup(repo => repo.DepartmentsAsync()).ReturnsAsync(allDepartments);

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            var coursePageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>()).Returns(coursePageAdapter);
            var corequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>()).Returns(corequisiteAdapter);
            courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                studentReferenceRepo, null, sectionRepoMock.Object, termRepoMock.Object, ruleRepoMock.Object,
                configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseRepoMock = null;
            courseRepo = null;
            referenceRepoMock = null;
            referenceRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }

        [TestMethod]
        public async Task OnlySectionsFoundByKeywordSearch()
        {
            // This search will only find sections, since no courses have the NW campus location.
            // Test that a course was retrieved for each section displayed on the page.
            var criteria = new CourseSearchCriteria() { Keyword = "northwest" };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
            var pageSectionIds = new List<string>();
            // Verify every section is associated with the correct course
            bool sectionIdsFound = false;
            foreach (var item in coursePage.CurrentPageItems)
            {
                foreach (var sectionId in item.MatchingSectionIds)
                {
                    var section = allSections.First(s => s.Id == sectionId);
                    Assert.IsTrue(item.Id == section.CourseId);
                    sectionIdsFound = true;
                }
            }
            // Verify there are section ids listed
            Assert.IsTrue(sectionIdsFound);
        }

        [TestMethod]
        public async Task NoCoursesOrSectionsFound()
        {
            var criteria = new CourseSearchCriteria() { Keyword = "XYZ$98785" };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
            // Assert -- No courses returned by the search
            Assert.IsTrue(coursePage.CurrentPageItems.Count() == 0);
        }

        [TestMethod]
        public async Task GetsSectionsWhenOnlyCourseFoundWithKeywordSearch()
        {
            var criteria = new CourseSearchCriteria() { Keyword = "pre-calculus" };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
            // Assert -- Sections returned even though search keyword not found in them
            Assert.IsTrue(coursePage.CurrentPageItems.First().MatchingSectionIds.Count() > 0);
        }

        [TestMethod]
        public async Task SearchFindsSectionKeywordInTitle()
        {
            // Arrange -- Manipulate a subset of sections to demonstrate that a keyword was found in the section title
            //string testKeyword = "titletest123";
            string testTitle = "Mathematic Exploratives";
            string testKeyword = testTitle;
            var testSections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
            for (int i = 0; i < 3; i++)
            {
                var sec = allSections.ElementAt(i);
                Ellucian.Colleague.Domain.Student.Entities.Section newSec = new Ellucian.Colleague.Domain.Student.Entities.Section(sec.Id, sec.CourseId, sec.Number, sec.StartDate, sec.MaximumCredits, sec.Ceus, testTitle, "IN", sec.Departments.ToList(), sec.CourseLevelCodes.ToList(), sec.AcademicLevelCode, statuses);
                newSec.TermId = sec.TermId;
                testSections.Add(newSec);
            }
            for (int i = 3; i < allSections.Count(); i++)
            {
                testSections.Add(allSections.ElementAt(i));
            }
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(testSections));

            // Act -- call course search
            var criteria = new CourseSearchCriteria() { Keyword = testKeyword };
            CourseService.ClearIndex();
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

            // Assert -- at least 5 sections with selected string were found
            var sectionIds = new List<string>();
            foreach (var item in coursePage.CurrentPageItems)
            {
                sectionIds.AddRange(item.MatchingSectionIds);
            }
            Assert.IsTrue(sectionIds.Count() >= 3);

            // Assert -- course with section with matching title appears first
            Assert.IsTrue(coursePage.CurrentPageItems.ElementAt(0).Id == "139");
        }

        [TestMethod]
        public async Task SearchFindsSectionKeywordInDepartment()
        {
            // Arrange -- Manipulate a subset of sections to demonstrate that a keyword was found in the section department
            string testDepartment = "FIAF";
            var testSections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            for (int i = 0; i < 5; i++)
            {
                var sec = allSections.ElementAt(i);
                var testDepts = sec.Departments.ToList();
                testDepts.Add(new Domain.Student.Entities.OfferingDepartment(testDepartment, 100m / (testDepts.Count + 1)));
                var currentStatus = new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-5));
                var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { currentStatus };
                var newSec = new Ellucian.Colleague.Domain.Student.Entities.Section(sec.Id, sec.CourseId, sec.Number, sec.StartDate, sec.MaximumCredits, sec.Ceus, sec.Title, "IN", testDepts, sec.CourseLevelCodes.ToList(), sec.AcademicLevelCode, statuses);
                newSec.TermId = sec.TermId;
                testSections.Add(newSec);
            }
            for (int i = 5; i < allSections.Count(); i++)
            {
                testSections.Add(allSections.ElementAt(i));
            }
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(testSections));

            // Act -- call course search
            var criteria = new CourseSearchCriteria() { Keyword = "Fiscal" };
            CourseService.ClearIndex();
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

            // Assert -- 5 sections with selected string were found
            var sectionIds = new List<string>();
            foreach (var item in coursePage.CurrentPageItems)
            {
                sectionIds.AddRange(item.MatchingSectionIds);
            }
            Assert.IsTrue(sectionIds.Count() == 5);
        }

        [TestMethod]
        public async Task SearchFindsSectionKeywordInLocation()
        {
            // Arrange -- Manipulate a subset of sections to demonstrate that a keyword was found in the section location
            string testLocation = "SAT";
            var testSections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            var statuses = new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
            for (int i = 0; i < 5; i++)
            {
                var sec = allSections.ElementAt(i);
                var newSec = new Ellucian.Colleague.Domain.Student.Entities.Section(sec.Id, sec.CourseId, sec.Number, sec.StartDate, sec.MaximumCredits, sec.Ceus, sec.Title, "IN", sec.Departments.ToList(), sec.CourseLevelCodes.ToList(), sec.AcademicLevelCode, statuses);
                newSec.TermId = sec.TermId;
                newSec.Location = testLocation;
                testSections.Add(newSec);
            }
            for (int i = 5; i < allSections.Count(); i++)
            {
                testSections.Add(allSections.ElementAt(i));
            }
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(testSections));

            // Act -- call course search
            var criteria = new CourseSearchCriteria() { Keyword = "satellite" };
            CourseService.ClearIndex();
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

            // Assert -- 5 sections with selected string were found
            var sectionIds = new List<string>();
            foreach (var item in coursePage.CurrentPageItems)
            {
                sectionIds.AddRange(item.MatchingSectionIds);
            }
            Assert.IsTrue(sectionIds.Count() == 5);
        }

        [TestMethod]
        public async Task KeywordSearchUsingNoDelimsInSectionNameFindsSection()
        {
            var subject = "MATH";
            var number = "371";
            var sectionNo = "02";
            var criteria = new CourseSearchCriteria() { Keyword = subject + number + sectionNo };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
            // Assert -- Requested section returned
            var sectionId = coursePage.CurrentPageItems.ElementAt(0).MatchingSectionIds.ElementAt(0);
            var section = allSections.Where(s => s.Id == sectionId).FirstOrDefault();
            var course = catalogCourses.Where(c => c.Id == section.CourseId).FirstOrDefault();
            Assert.IsTrue(course.SubjectCode == subject);
            Assert.IsTrue(course.Number == number);
            Assert.IsTrue(section.Number == sectionNo);
        }

        //PASSES WHEN RAN ALONE, BUT FAILS WHEN RAN WITH OTHER UNIT TESTS
        //        [TestMethod]
        //        public async Task KeywordSearchUsingInstDelimsInSectionNameFindsSection()
        //        {
        //            var subject = "MATH";
        //            var number = "371";
        //            var sectionNo = "02";
        //            var criteria = new CourseSearchCriteria() { Keyword = subject + CourseService.CourseDelimiter + number + CourseService.CourseDelimiter + sectionNo };
        //#pragma warning disable 618
        //            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
        //#pragma warning restore 618
        //            // Assert -- Requested section returned
        //            var sectionId = coursePage.CurrentPageItems.ElementAt(0).MatchingSectionIds.ElementAt(0);
        //            var section = allSections.Where(s => s.Id == sectionId).FirstOrDefault();
        //            var course = catalogCourses.Where(c => c.Id == section.CourseId).FirstOrDefault();
        //            Assert.IsTrue(course.SubjectCode == subject);
        //            Assert.IsTrue(course.Number == number);
        //            Assert.IsTrue(section.Number == sectionNo);
        //        }

        [TestMethod]
        public async Task KeywordSearchCombinedWithCourseLevelFilterReturnsCorrectNumberOfCourses()
        {
            // Verify that the number of courses returned matches the quantity of items identified with course level 100.
            var criteria = new CourseSearchCriteria() { Keyword = "English" };
            var courseLevel = "100";
            criteria.CourseLevels = new List<string> { courseLevel };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
            // Assert -- Number of items with that filter returned
            var courseLevelCount = coursePage.CourseLevels.Where(c => c.Value == courseLevel).Select(c => c.Count).First();
            Assert.IsTrue(courseLevelCount > 0);
            Assert.AreEqual(courseLevelCount, coursePage.CurrentPageItems.Count());
        }

        [TestMethod]
        public async Task BlankKeywordReturnsAllCurrentCourses()
        {
            var criteria = new CourseSearchCriteria() { Keyword = " " };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
            Assert.AreEqual(catalogCourses.Count(), coursePage.CurrentPageItems.Count());
            Assert.IsTrue(coursePage.CurrentPageItems is IEnumerable<Course>);
        }

        [TestMethod]
        public async Task SomeSectionsFoundWithoutCoursesFound()
        {
            // This search will find courses using "main", and only sections with
            // Northwest. Verify results have a mixture of courses with all sections
            // and courses with only some sections returned.
            List<string> course47List = new List<string>() { "47" };
            var course47Sections = allSections.Where(s => s.CourseId == "47");
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(course47List, regTerms)).Returns(Task.FromResult(course47Sections));
            var criteria = new CourseSearchCriteria() { Keyword = "northwest OR MAIN" };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
            // Assert that each section is correctly corresponding with course,
            // And that there is at least one section for each course found
            foreach (var item in coursePage.CurrentPageItems)
            {
                foreach (var sectionId in item.MatchingSectionIds)
                {
                    var section = allSections.First(s => s.Id == sectionId);
                    Assert.IsTrue(section.CourseId == item.Id);
                }
                Assert.IsTrue(item.MatchingSectionIds.Count() > 0);
            }
        }

        [TestMethod]
        public async Task ProcessesSuccessfullyWhenNoRegistrationSectionsReturned()
        {
            // Arrange -- Mock section repository so that no registration sections are found
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>>(new List<Ellucian.Colleague.Domain.Student.Entities.Section>()));
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(It.IsAny<List<string>>(), regTerms)).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>>(new List<Ellucian.Colleague.Domain.Student.Entities.Section>()));
            var criteria = new CourseSearchCriteria() { Keyword = "main" };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
            // Assert -- no sections ids associated with the courses
            foreach (var item in coursePage.CurrentPageItems)
            {
                Assert.IsTrue(item.MatchingSectionIds.Count() == 0);
            }
            // Assert -- courses are returned by the search
            Assert.IsTrue(coursePage.CurrentPageItems.Count() >= 4);
        }
    }

    [TestClass]
    public class SearchByRequirement
    {
        private Mock<ICourseRepository> courseRepoMock;
        private ICourseRepository courseRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IRuleRepository> ruleRepoMock;
        private IRuleRepository ruleRepo;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepo;
        private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
        private IStudentReferenceDataRepository studentReferenceRepo;
        private Mock<IReferenceDataRepository> referenceRepoMock;
        private IReferenceDataRepository referenceRepo;
        private Mock<IRequirementRepository> requirementRepoMock;
        private IRequirementRepository requirementRepo;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private ILogger logger;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch> coursePageAdapter;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2> courseSearch2PageAdapter;
        private CourseService courseService;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
        private IEnumerable<string> catalogSubjectCodes;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement> allRequirements;
        private ProgramRequirements programRequirements;
        private IEnumerable<RuleRequest<Ellucian.Colleague.Domain.Student.Entities.Course>> ruleReqList;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


        [TestInitialize]
        public async void Initialize()
        {
            referenceRepoMock = new Mock<IReferenceDataRepository>();
            referenceRepo = referenceRepoMock.Object;
            allSubjects = new TestSubjectRepository().Get();
            catalogSubjectCodes = allSubjects.Where(s => s.ShowInCourseSearch == true).Select(s => s.Code);
            allLocations = new TestLocationRepository().Get();
            allDepartments = new TestDepartmentRepository().Get();
            referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
            referenceRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allLocations);
            referenceRepoMock.Setup(repo => repo.GetDepartmentsAsync(false)).Returns(Task.FromResult(allDepartments));
            studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
            studentReferenceRepo = studentReferenceRepoMock.Object;
            studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));

            courseRepoMock = new Mock<ICourseRepository>();
            courseRepo = courseRepoMock.Object;
            allCourses = new TestCourseRepository().GetAsync().Result;
            courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
            catalogCourses = allCourses.Where(c => c.IsCurrent && catalogSubjectCodes.Contains(c.SubjectCode));

            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

            ruleRepoMock = new Mock<IRuleRepository>();
            ruleRepo = ruleRepoMock.Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepo = sectionRepoMock.Object;
            allSections = new TestSectionRepository().GetAsync().Result;
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(allSections));

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            requirementRepoMock = new Mock<IRequirementRepository>();
            requirementRepo = requirementRepoMock.Object;

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;

            programRequirements = new ProgramRequirements("MATH.BA", "2013");
            allRequirements = await new TestRequirementRepository().GetAsync(new List<string> { "GEN.ED", "BA.HIST", "MA.HIST", "MIN.HIST", "BS.BIO", "MS.BIO", "MIN.BIO" }, programRequirements);
            requirementRepoMock.Setup(repo => repo.GetAsync(new List<string> { "GEN.ED", "BA.HIST", "MA.HIST", "MIN.HIST", "BS.BIO", "MS.BIO", "MIN.BIO" })).Returns(Task.FromResult(allRequirements));
            requirementRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns((string reqCode) => Task.FromResult(allRequirements.First(r => r.Code == reqCode)));
            // Mock rule evaluation request response that validates all courses to true
            var rd = new RuleDescriptor() { Id = "REQRULE1", PrimaryView = "COURSES" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NO", "NE", "\"100\""));
            var rule = (Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)new CourseRuleAdapter().Create(rd);
            var reqRule = new RequirementRule((Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)rule);
            var ruleResults = new List<RuleResult>();
            // Create a "true" rule response for each course so that the response from the rule repository does not cause any courses to be excluded from the results
            foreach (var course in allCourses)
            {
                var ruleReq = new RuleRequest<Domain.Student.Entities.Course>(reqRule.CourseRule, course);
                ruleResults.Add(new RuleResult()
                {
                    RuleId = ruleReq.Rule.Id,
                    Context = ruleReq.Context,
                    Passed = true
                });
            }
            ruleRepoMock.Setup(repo => repo.ExecuteAsync<Ellucian.Colleague.Domain.Student.Entities.Course>(It.IsAny<IEnumerable<RuleRequest<Ellucian.Colleague.Domain.Student.Entities.Course>>>()))
                .ReturnsAsync(ruleResults)
                .Callback<List<RuleRequest<Ellucian.Colleague.Domain.Student.Entities.Course>>>(r => ruleReqList = r);
            // Mock Adapter 
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            coursePageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>()).Returns(coursePageAdapter);
            courseSearch2PageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>()).Returns(courseSearch2PageAdapter);
            var corequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>(adapterRegistry, logger);
            var requisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Dtos.Student.Requisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>()).Returns(corequisiteAdapter);
            courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                studentReferenceRepo, requirementRepo, sectionRepoMock.Object, termRepoMock.Object, ruleRepoMock.Object,
                configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseRepoMock = null;
            courseRepo = null;
            referenceRepoMock = null;
            referenceRepo = null;
            requirementRepoMock = null;
            requirementRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }

        [TestMethod]
        public async Task ReturnsCoursesForRequirementAndSubrequirement()
        {
            foreach (var req in allRequirements)
            {
                foreach (var Subreq in req.SubRequirements)
                {
                    foreach (var group in Subreq.Groups)
                    {
                        RequirementGroup requirementGroup = new RequirementGroup() { RequirementCode = req.Code, SubRequirementId = Subreq.Id, GroupId = group.Id };
                        var criteria = new CourseSearchCriteria() { RequirementGroup = requirementGroup };
#pragma warning disable 618
                        var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

                        var expected = new List<Ellucian.Colleague.Domain.Student.Entities.Course>();

                        var filtered = new List<Ellucian.Colleague.Domain.Student.Entities.Course>(catalogCourses);
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
                        expected = expected.Union(filtered).ToList();

                        foreach (var domainCourse in expected)
                        {
                            // make sure the course is in the result set from the course search service
                            // Except for items that are not current
                            if (domainCourse.IsCurrent)
                            {
                                var course = coursePageAdapter.MapToType(domainCourse);
                                Assert.IsTrue(coursePage.CurrentPageItems.Where(c => c.Id == course.Id).FirstOrDefault() != null);
                            }
                        }
                        // Make sure there are 6 sections for each course found
                        foreach (var item in coursePage.CurrentPageItems)
                        {
                            if (item.Id == "7704" || item.Id == "7701" || item.Id == "7706" || item.Id == "7710" || item.Id == "7711" || item.Id == "7714" || item.Id == "7715" || item.Id == "7272" || item.Id == "333")
                            {
                                // These courses have an extra section defined in the TestSectionRepository, above and beyond the 3 created for each of the two registration terms.
                                Assert.IsTrue(item.MatchingSectionIds.Count() >= 7);
                            }
                            else
                            {
                                Assert.IsTrue(item.MatchingSectionIds.Count() == 9);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AllowsRequirementExceptionToFallThrough()
        {
            requirementRepoMock.Setup(repo => repo.GetAsync("FOO")).Throws(new Exception("Failure"));
            RequirementGroup requirementGroup = new RequirementGroup() { RequirementCode = "FOO", SubRequirementId = "BAR" };
            var criteria = new CourseSearchCriteria() { RequirementGroup = requirementGroup };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AllowsSubrequirementExceptionToFallThrough()
        {
            var testReq = (await new TestRequirementRepository().GetAsync(new List<string> { "00001" })).First();
            requirementRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(testReq));
            requirementRepoMock.Setup(repo => repo.GetAsync("BAR")).Throws(new Exception("Failure"));
            RequirementGroup requirementGroup = new RequirementGroup() { RequirementCode = "00001", SubRequirementId = "BAR" };
            var criteria = new CourseSearchCriteria() { RequirementGroup = requirementGroup };
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618
        }


        [TestMethod]
        public async Task Search2_ReturnsCoursesForRequirementAndSubrequirement()
        {
            foreach (var req in allRequirements)
            {
                foreach (var Subreq in req.SubRequirements)
                {
                    foreach (var group in Subreq.Groups)
                    {
                        RequirementGroup requirementGroup = new RequirementGroup() { RequirementCode = req.Code, SubRequirementId = Subreq.Id, GroupId = group.Id };
                        var criteria = new CourseSearchCriteria() { RequirementGroup = requirementGroup };
                        var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);

                        var expected = new List<Ellucian.Colleague.Domain.Student.Entities.Course>();

                        var filtered = new List<Ellucian.Colleague.Domain.Student.Entities.Course>(catalogCourses);
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
                        expected = expected.Union(filtered).ToList();

                        foreach (var domainCourse in expected)
                        {
                            // make sure the course is in the result set from the course search service
                            // Except for items that are not current
                            if (domainCourse.IsCurrent)
                            {
                                var course = coursePageAdapter.MapToType(domainCourse);
                                Assert.IsTrue(coursePage.CurrentPageItems.Where(c => c.Id == course.Id).FirstOrDefault() != null);
                            }
                        }
                        // Make sure there are 6 sections for each course found
                        foreach (var item in coursePage.CurrentPageItems)
                        {
                            if (item.Id == "7704" || item.Id == "7701" || item.Id == "7706" || item.Id == "7710" || item.Id == "7711" || item.Id == "7714" || item.Id == "7715" || item.Id == "7272" || item.Id == "333")
                            {
                                // These courses have an extra section defined in the TestSectionRepository, above and beyond the 3 created for each of the two registration terms.
                                Assert.IsTrue(item.MatchingSectionIds.Count() >= 7);
                            }
                            else
                            {
                                Assert.IsTrue(item.MatchingSectionIds.Count() == 9);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Search2_AllowsRequirementExceptionToFallThrough()
        {
            requirementRepoMock.Setup(repo => repo.GetAsync("FOO")).Throws(new Exception("Failure"));
            RequirementGroup requirementGroup = new RequirementGroup() { RequirementCode = "FOO", SubRequirementId = "BAR" };
            var criteria = new CourseSearchCriteria() { RequirementGroup = requirementGroup };
            var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Search2_AllowsSubrequirementExceptionToFallThrough()
        {
            var testReq = (await new TestRequirementRepository().GetAsync(new List<string> { "00001" })).First();
            requirementRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(testReq));
            requirementRepoMock.Setup(repo => repo.GetAsync("BAR")).Throws(new Exception("Failure"));
            RequirementGroup requirementGroup = new RequirementGroup() { RequirementCode = "00001", SubRequirementId = "BAR" };
            var criteria = new CourseSearchCriteria() { RequirementGroup = requirementGroup };
            var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
        }

    }

    [TestClass]
    public class SearchByRequirementWithRules
    {
        private Mock<ICourseRepository> courseRepoMock;
        private ICourseRepository courseRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IRuleRepository> ruleRepoMock;
        private IRuleRepository ruleRepo;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepo;
        private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
        private IStudentReferenceDataRepository studentReferenceRepo;
        private Mock<IReferenceDataRepository> referenceRepoMock;
        private IReferenceDataRepository referenceRepo;
        private Mock<IRequirementRepository> requirementRepoMock;
        private IRequirementRepository requirementRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private ILogger logger;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2> coursePageAdapter;
        private CourseService courseService;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
        private IEnumerable<string> catalogSubjectCodes;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Requirement> allRequirements;
        private ProgramRequirements programRequirements;
        private IEnumerable<RuleRequest<Ellucian.Colleague.Domain.Student.Entities.Course>> ruleReqList;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


        [TestInitialize]
        public async void Initialize()
        {
            referenceRepoMock = new Mock<IReferenceDataRepository>();
            referenceRepo = referenceRepoMock.Object;
            allSubjects = new TestSubjectRepository().Get();
            catalogSubjectCodes = allSubjects.Where(s => s.ShowInCourseSearch == true).Select(s => s.Code);
            allLocations = new TestLocationRepository().Get();
            allDepartments = new TestDepartmentRepository().Get();
            referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
            referenceRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allLocations);
            referenceRepoMock.Setup(repo => repo.GetDepartmentsAsync(false)).Returns(Task.FromResult(allDepartments));
            studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
            studentReferenceRepo = studentReferenceRepoMock.Object;
            studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));

            courseRepoMock = new Mock<ICourseRepository>();
            courseRepo = courseRepoMock.Object;
            allCourses = new TestCourseRepository().GetAsync().Result;
            courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
            catalogCourses = allCourses.Where(c => c.IsCurrent && catalogSubjectCodes.Contains(c.SubjectCode));

            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

            ruleRepoMock = new Mock<IRuleRepository>();
            ruleRepo = ruleRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepo = sectionRepoMock.Object;
            allSections = new TestSectionRepository().GetAsync().Result;
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(allSections));

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            requirementRepoMock = new Mock<IRequirementRepository>();
            requirementRepo = requirementRepoMock.Object;
            programRequirements = new ProgramRequirements("MATH.BA", "2013");
            // Mock Adapter 
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;

            coursePageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>()).Returns(coursePageAdapter);
            var corequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>(adapterRegistry, logger);
            var requisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Dtos.Student.Requisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>()).Returns(corequisiteAdapter);
            courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                studentReferenceRepo, requirementRepo, sectionRepoMock.Object, termRepoMock.Object, ruleRepoMock.Object,
                configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseRepoMock = null;
            courseRepo = null;
            referenceRepoMock = null;
            referenceRepo = null;
            requirementRepoMock = null;
            requirementRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }


        // The purpose of this test is to verify that a rule request is created for each course in the result against each rule specified in the associated
        // program requirement, requirement, subrequirement and group. A rule is added to the program requirement here. The TestRequirementRepository has 
        // logic that adds rules to requirement, subrequirement and group respectively, so there is at least one test case where there are rules at every
        // level of the requirement specification, ensuring that rule requests are created no matter at level the rule is specified.
        [TestMethod]
        public async Task RuleRequestCreatedForEachRequirementRule()
        {
            // Add activity eligibility rule to program requirements disallowing courses with a subject of DENT
            var rd = new RuleDescriptor() { Id = "SUBJDENT", PrimaryView = "COURSES" };
            rd.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.SUBJECT", "NE", "\"DENT\""));
            var rule1 = (Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)new CourseRuleAdapter().Create(rd);
            programRequirements.ActivityEligibilityRules.Add(new RequirementRule((Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)rule1));
            allRequirements = await new TestRequirementRepository().GetAsync(new List<string> { "GEN.ED" }, programRequirements);
            requirementRepoMock.Setup(repo => repo.GetAsync(new List<string> { "GEN.ED" })).Returns(Task.FromResult(allRequirements));
            requirementRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns((string reqCode) => Task.FromResult(allRequirements.First(r => r.Code == reqCode)));
            // Mock rule evaluation request response that returns a passing rule request response for every course.
            var ruleDesc = new RuleDescriptor() { Id = "REQRULE1", PrimaryView = "COURSES" };
            ruleDesc.Expressions.Add(new RuleExpressionDescriptor("WITH", "CRS.NO", "NE", "\"100\""));
            var rule = (Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)new CourseRuleAdapter().Create(ruleDesc);
            var reqRule = new RequirementRule((Rule<Ellucian.Colleague.Domain.Student.Entities.Course>)rule);
            var ruleResults = new List<RuleResult>();
            // Create a "passed" rule response for each course
            foreach (var course in allCourses)
            {
                var ruleReq = new RuleRequest<Domain.Student.Entities.Course>(reqRule.CourseRule, course);
                ruleResults.Add(new RuleResult()
                {
                    RuleId = ruleReq.Rule.Id,
                    Context = ruleReq.Context,
                    Passed = true
                });
            }
            ruleRepoMock.Setup(repo => repo.ExecuteAsync<Ellucian.Colleague.Domain.Student.Entities.Course>(It.IsAny<IEnumerable<RuleRequest<Ellucian.Colleague.Domain.Student.Entities.Course>>>()))
                .ReturnsAsync(ruleResults)
                .Callback<List<RuleRequest<Ellucian.Colleague.Domain.Student.Entities.Course>>>(r => ruleReqList = r);

            foreach (var req in allRequirements)
            {
                foreach (var Subreq in req.SubRequirements)
                {
                    foreach (var group in Subreq.Groups)
                    {
                        RequirementGroup requirementGroup = new RequirementGroup() { RequirementCode = req.Code, SubRequirementId = Subreq.Id, GroupId = group.Id };
                        var criteria = new CourseSearchCriteria() { RequirementGroup = requirementGroup };
                        var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);

                        var expected = new List<Ellucian.Colleague.Domain.Student.Entities.Course>();

                        var filtered = new List<Ellucian.Colleague.Domain.Student.Entities.Course>(catalogCourses);
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

                        expected = expected.Union(filtered).ToList();

                        // A rule has been set up at each level; program requirement, requirement, subrequirement and group. 
                        foreach (var domainCourse in expected)
                        {
                            // Check that a rule request was submitted for each rule associated with the program requirement
                            foreach (var rRule in programRequirements.ActivityEligibilityRules)
                            {
                                // Verify that there is a rule request was created for this rule and for each course not yet filtered.
                                Assert.IsTrue(ruleReqList.Where(rr => rr.Rule.Id == rRule.Id && rr.Context.Id == domainCourse.Id).FirstOrDefault() != null);
                            }
                            // Check that a rule request exists for each rule associated with the requirement
                            foreach (var rRule in req.AcademicCreditRules)
                            {
                                // Verify that there is a rule request was created for this rule and for each course not yet filtered.
                                Assert.IsTrue(ruleReqList.Where(rr => rr.Rule.Id == rRule.Id && rr.Context.Id == domainCourse.Id).FirstOrDefault() != null);
                            }
                            // Check that a rule request exists for each rule associated with the requirement
                            foreach (var rRule in Subreq.AcademicCreditRules)
                            {
                                // Verify that there is a rule request was created for this rule and for each course not yet filtered.
                                Assert.IsTrue(ruleReqList.Where(rr => rr.Rule.Id == rRule.Id && rr.Context.Id == domainCourse.Id).FirstOrDefault() != null);
                            }
                            // Check that a rule request exists for each rule associated with the requirement
                            foreach (var rRule in group.AcademicCreditRules)
                            {
                                // Verify that there is a rule request was created for this rule and for each course not yet filtered.
                                Assert.IsTrue(ruleReqList.Where(rr => rr.Rule.Id == rRule.Id && rr.Context.Id == domainCourse.Id).FirstOrDefault() != null);
                            }
                        }
                    }
                }
            }
        }

    }

    [TestClass]
    public class SearchBySectionId
    {
        private Mock<ICourseRepository> courseRepoMock;
        private ICourseRepository courseRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IRuleRepository> ruleRepoMock;
        private IRuleRepository ruleRepo;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepo;
        private Mock<IReferenceDataRepository> referenceRepoMock;
        private IReferenceDataRepository referenceRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private CourseService courseService;
        private Mock<IRequirementRepository> requirementsRepoMock;
        private IRequirementRepository requirementsRepo;
        private Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
        private IStudentReferenceDataRepository studentReferenceRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> catalogSubjects;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
        private ILogger logger;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            referenceRepoMock = new Mock<IReferenceDataRepository>();
            referenceRepo = referenceRepoMock.Object;
            allSubjects = new TestSubjectRepository().Get();
            catalogSubjects = allSubjects.Where(s => s.ShowInCourseSearch == true);
            allLocations = new TestLocationRepository().Get();
            allDepartments = new TestDepartmentRepository().Get();

            referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
            referenceRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allLocations);
            referenceRepoMock.Setup(repo => repo.GetDepartmentsAsync(false)).Returns(Task.FromResult(allDepartments));

            courseRepoMock = new Mock<ICourseRepository>();
            courseRepo = courseRepoMock.Object;
            allCourses = new TestCourseRepository().GetAsync().Result;
            courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
            var catalogSubjectCodes = catalogSubjects.Select(s => s.Code);
            catalogCourses = allCourses.Where(c => c.IsCurrent == true && catalogSubjectCodes.Contains(c.SubjectCode));

            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

            ruleRepoMock = new Mock<IRuleRepository>();
            ruleRepo = ruleRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepo = sectionRepoMock.Object;
            allSections = new TestSectionRepository().GetAsync().Result;
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(allSections));

            requirementsRepoMock = new Mock<IRequirementRepository>();
            requirementsRepo = requirementsRepoMock.Object;

            studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
            studentReferenceRepo = studentReferenceRepoMock.Object;
            studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            var coursePageAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch>()).Returns(coursePageAdapter);
            var corequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>()).Returns(corequisiteAdapter);
            courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                studentReferenceRepo, null, sectionRepoMock.Object, termRepoMock.Object, ruleRepoMock.Object,
                configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseRepoMock = null;
            courseRepo = null;
            referenceRepoMock = null;
            referenceRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }

        [TestMethod]
        public async Task SearchOnOneSection()
        {
            // Get one specific section.
            var section1 = (await sectionRepo.GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == "333" && s.TermId == regTerms.ElementAt(0).Code && s.Number == "02").First();
            // Set up repository response
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(new List<Domain.Student.Entities.Section>() { section1 }));
            // Set up search criteria
            var criteria = new CourseSearchCriteria() { SectionIds = new List<string>() { section1.Id } };

            // Execute the course search using the criteria of one section Id
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

            // Verify that the one expected section and the one expected course are returned
            Assert.AreEqual(1, coursePage.CurrentPageItems.Count());
            Assert.AreEqual(section1.CourseId, coursePage.CurrentPageItems.ElementAt(0).Id);
            Assert.AreEqual(1, coursePage.CurrentPageItems.ElementAt(0).MatchingSectionIds.Count());
            Assert.AreEqual(section1.Id, coursePage.CurrentPageItems.ElementAt(0).MatchingSectionIds.ElementAt(0));
        }

        [TestMethod]
        public async Task SearchOnMultipleSections()
        {
            // Get several sections
            var section1 = (await sectionRepo.GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == "333" && s.TermId == regTerms.ElementAt(0).Code && s.Number == "02").First();
            var section2 = (await sectionRepo.GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == "333" && s.TermId == regTerms.ElementAt(0).Code && s.Number == "01").First(); // second section of same course
            var section3 = (await sectionRepo.GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == "42" && s.TermId == regTerms.ElementAt(0).Code && s.Number == "01").First(); // section of a course with equates
            var sectionCollection = new List<Domain.Student.Entities.Section>() { section1, section2, section3 };
            var courseIdCollection = sectionCollection.Select(s => s.CourseId).Distinct();
            // Set up repository response
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionCollection));
            // Set up search criteria
            var criteria = new CourseSearchCriteria() { SectionIds = new List<string>() { section1.Id, section2.Id, section3.Id } };

            // Execute the course search using the criteria of one section Id
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

            // Verify the three expected sections and the two expected courses are returned
            Assert.AreEqual(courseIdCollection.Count(), coursePage.CurrentPageItems.Count()); // Expected number of courses
            Assert.AreEqual(sectionCollection.Count(), coursePage.CurrentPageItems.SelectMany(cpi => cpi.MatchingSectionIds).Count()); // Expected number of sections
            foreach (var course in coursePage.CurrentPageItems)
            {
                // Each course Id is expected
                Assert.IsTrue(courseIdCollection.Contains(course.Id));
                foreach (var secId in course.MatchingSectionIds)
                {
                    // Each section is in the expected collection and matches the course with which it is listed
                    var sec = sectionCollection.Where(s => s.Id == secId).First();
                    Assert.AreEqual(course.Id, sec.CourseId);
                }
            }
        }

        [TestMethod]
        public async Task SearchOnMultipleSectionsWithFilter()
        {
            // Get several sections
            var section1 = (await sectionRepo.GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == "333" && s.TermId == regTerms.ElementAt(0).Code && s.Number == "02").First();
            var section2 = (await sectionRepo.GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == "333" && s.TermId == regTerms.ElementAt(0).Code && s.Number == "01").First(); // second section of same course
            var section3 = (await sectionRepo.GetRegistrationSectionsAsync(regTerms)).Where(s => s.CourseId == "42" && s.TermId == regTerms.ElementAt(0).Code && s.Number == "01").First(); // section of a course with equates
            var sectionCollection = new List<Domain.Student.Entities.Section>() { section1, section2, section3 };
            var courseIdCollection = sectionCollection.Select(s => s.CourseId).Distinct();

            // Set up repository response
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionCollection));

            // Set up search criteria
            var criteria = new CourseSearchCriteria() { SectionIds = new List<string>() { section1.Id, section2.Id, section3.Id } };
            criteria.Subjects = new List<string>() { "MATH", "ENGL" }; // this filter will remove course 42

            // Execute the course search using the criteria of one section Id
#pragma warning disable 618
            var coursePage = await courseService.SearchAsync(criteria, Int16.MaxValue, 1);
#pragma warning restore 618

            // Verify the two expected sections and the one expected course are returned
            Assert.AreEqual(1, coursePage.CurrentPageItems.Count()); // Expected number of courses
            Assert.AreEqual(2, coursePage.CurrentPageItems.SelectMany(cpi => cpi.MatchingSectionIds).Count()); // Expected number of sections
            var matchingSectionIds = coursePage.CurrentPageItems.ElementAt(0).MatchingSectionIds;
            Assert.AreEqual(2, matchingSectionIds.Count());
            Assert.IsTrue(matchingSectionIds.Contains(section1.Id));
            Assert.IsTrue(matchingSectionIds.Contains(section2.Id));
        }
    }

    [TestClass]
    public class GetSectionsAsync_Tests : CourseServiceTests
    {
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IRuleRepository> ruleRepoMock;
        private IRuleRepository ruleRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private ILogger logger;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section> sectionDtoAdapter;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting> meetingTimeDtoAdapter;
        private CourseService courseService;

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            ruleRepoMock = new Mock<IRuleRepository>();
            ruleRepo = ruleRepoMock.Object;
            regTerms = new TestTermRepository().Get();
            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepo = sectionRepoMock.Object;
            allSections = new TestSectionRepository().GetAsync().Result;
            termRepoMock.Setup(trepo => trepo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = new CurrentUserSetup.FacultytUserFactory();

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            // Mock Adapter 
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            meetingTimeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting>()).Returns(meetingTimeDtoAdapter);
            sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section>()).Returns(sectionDtoAdapter);
            var sectionBookAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionBook, SectionBook>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionBook, SectionBook>()).Returns(sectionBookAdapter);
            courseService = new CourseService(adapterRegistryMock.Object, null, null, null, null, sectionRepoMock.Object,
                termRepoMock.Object, ruleRepoMock.Object, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionRepoMock = null;
            sectionRepo = null;
            termRepoMock = null;
            termRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }

        [TestMethod]
        public async Task ReturnsSectionsForTwoCourses()
        {
            IEnumerable<string> courseIds = new List<string>() { "139", "42" };
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).Returns(Task.FromResult(allSections.Where(s => s.CourseId == "139" || s.CourseId == "42")));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section>> privacyWrapper = await courseService.GetSectionsAsync(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(18, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsNoSectionsForCourses()
        {
            IEnumerable<string> courseIds = new List<string>() { "999", "998" };
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).Returns(Task.FromResult(allSections.Where(s => s.CourseId == "999")));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section>> privacyWrapper = await courseService.GetSectionsAsync(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(0, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsNoSectionsWhenCourseIdsEmpty()
        {
            IEnumerable<string> courseIds = new List<string>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> noSections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).Returns(Task.FromResult(noSections));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section>> privacyWrapper = await courseService.GetSectionsAsync(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(0, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsSectionsForTwoCourses_non_cached()
        {
            IEnumerable<string> courseIds = new List<string>() { "139", "42" };
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).ReturnsAsync(new List<Domain.Student.Entities.Section>());
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsNonCachedAsync(courseIds, regTerms)).ReturnsAsync(allSections.Where(s => s.CourseId == "139" || s.CourseId == "42"));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section>> privacyWrapper = await courseService.GetSectionsAsync(courseIds, false);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(18, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsSectionsForTwoCourses_privacy_wrapping()
        {
            IEnumerable<string> courseIds = new List<string>() { "139", "42" };
            allSections.ElementAt(0).AddFaculty(userFactory.CurrentUser.PersonId);
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).ReturnsAsync(new List<Domain.Student.Entities.Section>()
            {
                allSections.ElementAt(0),
                allSections.ElementAt(1)
            });
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section>> privacyWrapper = await courseService.GetSectionsAsync(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(2, privacyWrapper.Dto.Count());
            Assert.IsNotNull(privacyWrapper.Dto.ElementAt(0).ActiveStudentIds);
            Assert.AreEqual(0, privacyWrapper.Dto.ElementAt(1).ActiveStudentIds.Count());
        }
    }

    [TestClass]
    public class GetSections2Async_Tests : CourseServiceTests
    {
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IRuleRepository> ruleRepoMock;
        private IRuleRepository ruleRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private ILogger logger;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section2> sectionDtoAdapter;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting> meetingTimeDtoAdapter;
        private CourseService courseService;

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            ruleRepoMock = new Mock<IRuleRepository>();
            ruleRepo = ruleRepoMock.Object;
            regTerms = new TestTermRepository().Get();
            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepo = sectionRepoMock.Object;
            allSections = new TestSectionRepository().GetAsync().Result;
            termRepoMock.Setup(trepo => trepo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = new CurrentUserSetup.FacultytUserFactory();

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            // Mock Adapter 
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            meetingTimeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting>()).Returns(meetingTimeDtoAdapter);
            sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Section2>()).Returns(sectionDtoAdapter);
            var sectionBookAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionBook, SectionBook>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionBook, SectionBook>()).Returns(sectionBookAdapter);
            courseService = new CourseService(adapterRegistryMock.Object, null, null, null, null, sectionRepoMock.Object,
                termRepoMock.Object, ruleRepoMock.Object, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionRepoMock = null;
            sectionRepo = null;
            termRepoMock = null;
            termRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }

        [TestMethod]
        public async Task ReturnsSectionsForTwoCourses()
        {
            IEnumerable<string> courseIds = new List<string>() { "139", "42" };
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).Returns(Task.FromResult(allSections.Where(s => s.CourseId == "139" || s.CourseId == "42")));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section2>> privacyWrapper = await courseService.GetSections2Async(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(18, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsNoSectionsForCourses()
        {
            IEnumerable<string> courseIds = new List<string>() { "999", "998" };
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).Returns(Task.FromResult(allSections.Where(s => s.CourseId == "999")));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section2>> privacyWrapper = await courseService.GetSections2Async(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(0, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsNoSectionsWhenCourseIdsEmpty()
        {
            IEnumerable<string> courseIds = new List<string>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> noSections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).Returns(Task.FromResult(noSections));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section2>> privacyWrapper = await courseService.GetSections2Async(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(0, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsSectionsForTwoCourses_non_cached()
        {
            IEnumerable<string> courseIds = new List<string>() { "139", "42" };
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).ReturnsAsync(new List<Domain.Student.Entities.Section>());
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsNonCachedAsync(courseIds, regTerms)).ReturnsAsync(allSections.Where(s => s.CourseId == "139" || s.CourseId == "42"));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section2>> privacyWrapper = await courseService.GetSections2Async(courseIds, false);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(18, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsSectionsForTwoCourses_privacy_wrapping()
        {
            IEnumerable<string> courseIds = new List<string>() { "139", "42" };
            allSections.ElementAt(0).AddFaculty(userFactory.CurrentUser.PersonId);
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).ReturnsAsync(new List<Domain.Student.Entities.Section>()
            {
                allSections.ElementAt(0),
                allSections.ElementAt(1)
            });
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Section2>> privacyWrapper = await courseService.GetSections2Async(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(2, privacyWrapper.Dto.Count());
            Assert.IsNotNull(privacyWrapper.Dto.ElementAt(0).ActiveStudentIds);
            Assert.AreEqual(0, privacyWrapper.Dto.ElementAt(1).ActiveStudentIds.Count());
        }
    }

    [TestClass]
    public class GetSections3Async_Tests : CourseServiceTests
    {
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository sectionRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IRuleRepository> ruleRepoMock;
        private IRuleRepository ruleRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private ILogger logger;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3> sectionDtoAdapter;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting2> meetingTimeDtoAdapter;
        private CourseService courseService;

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            ruleRepoMock = new Mock<IRuleRepository>();
            ruleRepo = ruleRepoMock.Object;
            regTerms = new TestTermRepository().Get();
            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepo = sectionRepoMock.Object;
            allSections = new TestSectionRepository().GetAsync().Result;
            termRepoMock.Setup(trepo => trepo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = new CurrentUserSetup.FacultytUserFactory();

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            // Mock Adapter 
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            meetingTimeDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, SectionMeeting2>()).Returns(meetingTimeDtoAdapter);
            sectionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionDtoAdapter);
            var sectionBookAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionBook, SectionBook>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionBook, SectionBook>()).Returns(sectionBookAdapter);
            courseService = new CourseService(adapterRegistryMock.Object, null, null, null, null, sectionRepoMock.Object,
                termRepoMock.Object, ruleRepoMock.Object, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionRepoMock = null;
            sectionRepo = null;
            termRepoMock = null;
            termRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }

        [TestMethod]
        public async Task ReturnsSectionsForTwoCourses()
        {
            IEnumerable<string> courseIds = new List<string>() { "139", "42" };
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).Returns(Task.FromResult(allSections.Where(s => s.CourseId == "139" || s.CourseId == "42")));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Dtos.Student.Section3>> privacyWrapper = await courseService.GetSections3Async(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(18, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsNoSectionsForCourses()
        {
            IEnumerable<string> courseIds = new List<string>() { "999", "998" };
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).Returns(Task.FromResult(allSections.Where(s => s.CourseId == "999")));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Dtos.Student.Section3>> privacyWrapper = await courseService.GetSections3Async(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(0, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsNoSectionsWhenCourseIdsEmpty()
        {
            IEnumerable<string> courseIds = new List<string>();
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> noSections = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).Returns(Task.FromResult(noSections));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Dtos.Student.Section3>> privacyWrapper = await courseService.GetSections3Async(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(0, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsSectionsForTwoCourses_non_cached()
        {
            IEnumerable<string> courseIds = new List<string>() { "139", "42" };
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).ReturnsAsync(new List<Domain.Student.Entities.Section>());
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsNonCachedAsync(courseIds, regTerms)).ReturnsAsync(allSections.Where(s => s.CourseId == "139" || s.CourseId == "42"));
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Dtos.Student.Section3>> privacyWrapper = await courseService.GetSections3Async(courseIds, false);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(18, privacyWrapper.Dto.Count());
        }

        [TestMethod]
        public async Task ReturnsSectionsForTwoCourses_privacy_wrapping()
        {
            IEnumerable<string> courseIds = new List<string>() { "139", "42" };
            allSections.ElementAt(0).AddFaculty(userFactory.CurrentUser.PersonId);
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(courseIds, regTerms)).ReturnsAsync(new List<Domain.Student.Entities.Section>()
            {
                allSections.ElementAt(0),
                allSections.ElementAt(1)
            });
#pragma warning disable 618
            PrivacyWrapper<IEnumerable<Dtos.Student.Section3>> privacyWrapper = await courseService.GetSections3Async(courseIds);
#pragma warning restore 618
            Assert.IsNotNull(privacyWrapper);
            Assert.IsNotNull(privacyWrapper.Dto);
            Assert.AreEqual(2, privacyWrapper.Dto.Count());
            Assert.IsNotNull(privacyWrapper.Dto.ElementAt(0).ActiveStudentIds);
            Assert.AreEqual(0, privacyWrapper.Dto.ElementAt(1).ActiveStudentIds.Count());
        }
    }

    /// <summary>
    /// Basic tests for properties beyond those non-automapped for the CourseDto
    /// </summary>
    [TestClass]
    public class GetCourseById
    {
        private Mock<ICourseRepository> courseRepoMock;
        private ICourseRepository courseRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private ILogger logger;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course> courseDtoAdapter;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite> corequisiteDtoAdapter;
        private CourseService courseService;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            courseRepoMock = new Mock<ICourseRepository>();
            courseRepo = courseRepoMock.Object;
            allCourses = new TestCourseRepository().GetAsync().Result;

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;

            // Mock Adapter 
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            courseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course>()).Returns(courseDtoAdapter);
            corequisiteDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>()).Returns(corequisiteDtoAdapter);

            courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, null, null, null, null, null, null, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseRepoMock = null;
            courseRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }

        // Test for the properties beyond those "automapped" properties
        [TestMethod]
        public async Task ReturnsCourseWithPrereq()
        {
            courseRepoMock.Setup(repo => repo.GetAsync("186")).Returns(Task.FromResult(allCourses.Where(s => s.Id == "186").FirstOrDefault()));
#pragma warning disable 618
            Course courseDto = await courseService.GetCourseByIdAsync("186");
#pragma warning restore 618
            Assert.AreEqual("PREREQ1", courseDto.Prerequisites);
            Assert.AreEqual(0, courseDto.Corequisites.Count());
        }

    }

    /// <summary>
    /// Basic tests for automapped properties of the Course2Dto
    /// </summary>
    [TestClass]
    public class GetCourseById2
    {
        private Mock<ICourseRepository> courseRepoMock;
        private ICourseRepository courseRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private ILogger logger;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course2> courseDtoAdapter;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite> requisiteDtoAdapter;
        private CourseService courseService;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            courseRepoMock = new Mock<ICourseRepository>();
            courseRepo = courseRepoMock.Object;
            allCourses = new TestCourseRepository().GetAsync().Result;

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;

            // Mock Adapter 
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            courseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>()).Returns(courseDtoAdapter);
            requisiteDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>()).Returns(requisiteDtoAdapter);

            courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, null, null, null, null, null, null, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseRepoMock = null;
            courseRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }

        // Test for some basic the automapped properties 
        [TestMethod]
        public async Task ReturnsCourse2Dto_Basics()
        {
            Ellucian.Colleague.Domain.Student.Entities.Course courseEntity = allCourses.Where(s => s.Id == "186").FirstOrDefault();
            courseRepoMock.Setup(repo => repo.GetAsync("186")).Returns(Task.FromResult(courseEntity));
            Course2 course2Dto = await courseService.GetCourseById2Async("186");
            Assert.AreEqual(courseEntity.Id, course2Dto.Id);
            Assert.AreEqual(courseEntity.SubjectCode, course2Dto.SubjectCode);
            Assert.AreEqual(courseEntity.Title, course2Dto.Title);
            Assert.AreEqual(courseEntity.Number, course2Dto.Number);
            Assert.AreEqual(courseEntity.MinimumCredits, course2Dto.MinimumCredits);
            Assert.AreEqual(courseEntity.MaximumCredits, course2Dto.MaximumCredits);
            Assert.AreEqual(courseEntity.Ceus, course2Dto.Ceus);
            Assert.AreEqual(courseEntity.Description, course2Dto.Description);
            Assert.AreEqual(courseEntity.YearsOffered, course2Dto.YearsOffered);
            Assert.AreEqual(courseEntity.TermsOffered, course2Dto.TermsOffered);
            Assert.AreEqual(courseEntity.TermYearlyCycle, course2Dto.TermYearlyCycle);
            Assert.AreEqual(courseEntity.TermSessionCycle, course2Dto.TermSessionCycle);
            Assert.AreEqual(courseEntity.LocationCodes.Count(), course2Dto.LocationCodes.Count());

        }

        [TestMethod]
        public async Task Course2Dto_Requisite()
        {
            Ellucian.Colleague.Domain.Student.Entities.Course courseEntity = allCourses.Where(s => s.Id == "186").FirstOrDefault();
            courseRepoMock.Setup(repo => repo.GetAsync("186")).Returns(Task.FromResult(courseEntity));
            Course2 course2Dto = await courseService.GetCourseById2Async("186");
            Ellucian.Colleague.Domain.Student.Entities.Requisite requisiteEntity = courseEntity.Requisites.ElementAt(0);
            Requisite requisiteDto = course2Dto.Requisites.ElementAt(0);
            Assert.AreEqual(requisiteEntity.RequirementCode, requisiteDto.RequirementCode);
            Assert.AreEqual(requisiteEntity.CompletionOrder.ToString(), requisiteDto.CompletionOrder.ToString());
            Assert.AreEqual(requisiteEntity.IsRequired, requisiteDto.IsRequired);
        }
    }

    /// <summary>
    /// Basic tests for automapped properties of the Course2Dto
    /// </summary>
    [TestClass]
    public class GetCourses2
    {
        private Mock<ICourseRepository> courseRepoMock;
        private ICourseRepository courseRepo;
        private Mock<IStudentConfigurationRepository> configurationRepoMock;
        private IStudentConfigurationRepository configurationRepo;
        private ILogger logger;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course2> courseDtoAdapter;
        private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite> requisiteDtoAdapter;
        private CourseService courseService;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            courseRepoMock = new Mock<ICourseRepository>();
            courseRepo = courseRepoMock.Object;
            allCourses = new TestCourseRepository().GetAsync().Result;

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;

            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;

            // Mock Adapter 
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            courseDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, Course2>()).Returns(courseDtoAdapter);
            requisiteDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>()).Returns(requisiteDtoAdapter);

            courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, null, null, null, null, null, null, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseRepoMock = null;
            courseRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
        }

        // Test for some basic the automapped properties 
        [TestMethod]
        public async Task ReturnsCourse2Dto()
        {
            // Arrange
            var badCourseIdList = new List<string>() { "2222222" };
            var goodCourseIdList = new List<string>() { "186", "42", "139" };
            var courseIdList = badCourseIdList;
            courseIdList.AddRange(goodCourseIdList);
            var courseEntities = allCourses.Where(c => courseIdList.Contains(c.Id));
            courseRepoMock.Setup(repo => repo.GetAsync(courseIdList)).Returns(Task.FromResult(courseEntities));

            // Act
            var criteria = new CourseQueryCriteria() { CourseIds = courseIdList };
            var courseDtos = await courseService.GetCourses2Async(criteria);

            // Assert
            foreach (var course2Dto in courseDtos)
            {
                var courseEntity = courseEntities.Where(c => c.Id == course2Dto.Id).FirstOrDefault();

                if (courseEntity == null)
                {
                    Assert.IsTrue(badCourseIdList.Contains(course2Dto.Id));
                }
                else
                {
                    Assert.AreEqual(courseEntity.Id, course2Dto.Id);
                    Assert.AreEqual(courseEntity.SubjectCode, course2Dto.SubjectCode);
                    Assert.AreEqual(courseEntity.Title, course2Dto.Title);
                    Assert.AreEqual(courseEntity.Number, course2Dto.Number);
                    Assert.AreEqual(courseEntity.MinimumCredits, course2Dto.MinimumCredits);
                    Assert.AreEqual(courseEntity.MaximumCredits, course2Dto.MaximumCredits);
                    Assert.AreEqual(courseEntity.Ceus, course2Dto.Ceus);
                    Assert.AreEqual(courseEntity.Description, course2Dto.Description);
                    Assert.AreEqual(courseEntity.YearsOffered, course2Dto.YearsOffered);
                    Assert.AreEqual(courseEntity.TermsOffered, course2Dto.TermsOffered);
                    Assert.AreEqual(courseEntity.LocationCodes.Count(), course2Dto.LocationCodes.Count());
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ThrowsExceptionForNullArgument()
        {
            List<string> courseIdList = null;
            var courseEntities = allCourses.Where(c => courseIdList.Contains(c.Id));
            courseRepoMock.Setup(repo => repo.GetAsync(courseIdList)).Returns(Task.FromResult(courseEntities));

            // Act
            var criteria = new CourseQueryCriteria() { CourseIds = courseIdList };
            var courseDtos = await courseService.GetCourses2Async(criteria);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ThrowsExceptionForEmptyListArgument()
        {
            List<string> courseIdList = new List<string>();
            var courseEntities = allCourses.Where(c => courseIdList.Contains(c.Id));
            courseRepoMock.Setup(repo => repo.GetAsync(courseIdList)).Returns(Task.FromResult(courseEntities));

            // Act
            var criteria = new CourseQueryCriteria() { CourseIds = courseIdList };
            var courseDtos = await courseService.GetCourses2Async(criteria);
        }
    }

    [TestClass]
    public class Search3
    {
        protected Mock<ICourseRepository> courseRepoMock;
        protected ICourseRepository courseRepo;
        protected Mock<ITermRepository> termRepoMock;
        protected ITermRepository termRepo;
        protected Mock<IRuleRepository> ruleRepoMock;
        protected IRuleRepository ruleRepo;
        protected Mock<IRoleRepository> roleRepoMock;
        protected IRoleRepository roleRepo;
        protected Mock<ISectionRepository> sectionRepoMock;
        protected ISectionRepository sectionRepo;
        protected Mock<IReferenceDataRepository> referenceRepoMock;
        protected IReferenceDataRepository referenceRepo;
        protected Mock<IRequirementRepository> requirementsRepoMock;
        protected IRequirementRepository requirementsRepo;
        protected Mock<IStudentReferenceDataRepository> studentReferenceRepoMock;
        protected IStudentReferenceDataRepository studentReferenceRepo;
        protected Mock<IStudentConfigurationRepository> configurationRepoMock;
        protected IStudentConfigurationRepository configurationRepo;
        protected Mock<IAdapterRegistry> adapterRegistryMock;
        protected IAdapterRegistry adapterRegistry;
        protected CourseService courseService;
        protected IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> allCourses;
        protected IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Course> catalogCourses;
        protected IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Term> regTerms;
        protected IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section> allSections;
        protected IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjects;
        protected IEnumerable<string> catalogSubjectCodes;
        protected IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartments;
        protected IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Location> allLocations;
        protected IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CourseType> allCourseTypes;
        protected ILogger logger;
        protected Mock<ICurrentUserFactory> userFactoryMock;
        protected ICurrentUserFactory userFactory;
        protected IConfigurationRepository baseConfigurationRepository;
        protected Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {



            courseRepoMock = new Mock<ICourseRepository>();
            courseRepo = courseRepoMock.Object;
            allCourses = new TestCourseRepository().GetAsync().Result;
            courseRepoMock.Setup(repo => repo.GetAsync()).Returns(Task.FromResult(allCourses));
            catalogCourses = allCourses.Where(c => c.IsCurrent == true && catalogSubjectCodes.Contains(c.SubjectCode));

            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            regTerms = await new TestTermRepository().GetRegistrationTermsAsync();
            termRepoMock.Setup(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult(regTerms));

            ruleRepoMock = new Mock<IRuleRepository>();
            ruleRepo = ruleRepoMock.Object;

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            sectionRepoMock = new Mock<ISectionRepository>();
            sectionRepo = sectionRepoMock.Object;
            allSections = new TestSectionRepository().GetAsync().Result;
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(allSections));

            referenceRepoMock = new Mock<IReferenceDataRepository>();
            referenceRepo = referenceRepoMock.Object;
            allSubjects = new TestSubjectRepository().Get();
            allLocations = new TestLocationRepository().Get();
            allDepartments = new TestDepartmentRepository().Get();
            allCourseTypes = new TestCourseTypeRepository().Get();
            referenceRepoMock.Setup(repo => repo.Locations).Returns(allLocations);
            referenceRepoMock.Setup(repo => repo.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(allLocations);
            referenceRepoMock.Setup(repo => repo.GetDepartmentsAsync(false)).Returns(Task.FromResult(allDepartments));

            requirementsRepoMock = new Mock<IRequirementRepository>();
            requirementsRepo = requirementsRepoMock.Object;

            studentReferenceRepoMock = new Mock<IStudentReferenceDataRepository>();
            studentReferenceRepo = studentReferenceRepoMock.Object;
            studentReferenceRepoMock.Setup(repo => repo.GetSubjectsAsync()).Returns(Task.FromResult(allSubjects));
            studentReferenceRepoMock.Setup(repo => repo.GetCourseTypesAsync(It.IsAny<bool>())).ReturnsAsync(allCourseTypes);

            catalogSubjectCodes = allSubjects.Where(s => s.ShowInCourseSearch == true).Select(s => s.Code);

            configurationRepoMock = new Mock<IStudentConfigurationRepository>();
            configurationRepo = configurationRepoMock.Object;
            configurationRepoMock.Setup(repo => repo.GetCourseCatalogConfigurationAsync()).ReturnsAsync(new Domain.Student.Entities.CourseCatalogConfiguration(new DateTime?(new DateTime(2012, 2, 3)), new DateTime?(new DateTime(2014, 2, 3))));


            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            var coursePageAdapter2 = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Course, CourseSearch2>()).Returns(coursePageAdapter2);
            var requisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Requisite, Requisite>()).Returns(requisiteAdapter);
            var locationCycleAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.LocationCycleRestriction, LocationCycleRestriction>(adapterRegistry, logger);
            adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.LocationCycleRestriction, LocationCycleRestriction>()).Returns(locationCycleAdapter);
            courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
                studentReferenceRepo, requirementsRepo, sectionRepoMock.Object, termRepoMock.Object,
                ruleRepoMock.Object, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);

            CourseService.ClearIndex();
        }

        [TestCleanup]
        public void Cleanup()
        {
            courseRepoMock = null;
            courseRepo = null;
            referenceRepoMock = null;
            referenceRepo = null;
            adapterRegistryMock = null;
            adapterRegistry = null;
            courseService = null;
            configurationRepoMock = null;
            configurationRepo = null;
        }

        [TestMethod]
        public async Task Course2DtoContainsAllCourseData()
        {
            // Arrange -- Set up empty search
            var pageSize = Int16.MaxValue;
            var pageIndex = 2;
            var emptySearchCriteria = new CourseSearchCriteria();

            // Act -- Invoke Search service
            CoursePage2 course2Page = await courseService.Search2Async(emptySearchCriteria, pageSize, pageIndex);

            // Assert
            var course2Dto = course2Page.CurrentPageItems.Where(c => c.Id == "42").First();
            var course = catalogCourses.Where(c => c.Id == "42").First();
            Assert.AreEqual(course.Id, course2Dto.Id);
            Assert.AreEqual(course.SubjectCode, course2Dto.SubjectCode);
            Assert.AreEqual(course.Number, course2Dto.Number);
            Assert.AreEqual(course.MinimumCredits, course2Dto.MinimumCredits);
            Assert.AreEqual(course.MaximumCredits, course2Dto.MaximumCredits);
            Assert.AreEqual(course.Ceus, course2Dto.Ceus);
            Assert.AreEqual(course.Title, course2Dto.Title);
            Assert.AreEqual(course.Description, course2Dto.Description);
            Assert.AreEqual(course.YearsOffered, course2Dto.YearsOffered);
            Assert.AreEqual(course.TermsOffered, course2Dto.TermsOffered);
            Assert.AreEqual(course.TermYearlyCycle, course2Dto.TermYearlyCycle);
            Assert.AreEqual(course.TermSessionCycle, course2Dto.TermSessionCycle);
            Assert.AreEqual(course.LocationCodes.Count(), course2Dto.LocationCodes.Count());
        }

        [TestMethod]
        public async Task Course2DtoContainsAllCourseData_HiddenLocationsFromCourseNotIncluded()
        {
            // Data Setup -- course with all location codes, some of which will be filtered out
            var course = allCourses.Where(c => c.Id == "42").First();
            course.LocationCodes = allLocations.Select(loc => loc.Code).Distinct().ToList();

            // Arrange -- Set up empty search
            var pageSize = Int16.MaxValue;
            var pageIndex = 2;
            var emptySearchCriteria = new CourseSearchCriteria();

            // Act -- Invoke Search service
            CoursePage2 course2Page = await courseService.Search2Async(emptySearchCriteria, pageSize, pageIndex);
            var locationCount = allLocations.Where(loc => !loc.HideInSelfServiceCourseSearch).Count();

            // Assert
            var course2Dto = course2Page.CurrentPageItems.Where(c => c.Id == "42").First();
            Assert.AreEqual(course.Id, course2Dto.Id);
            Assert.AreEqual(course.SubjectCode, course2Dto.SubjectCode);
            Assert.AreEqual(course.Number, course2Dto.Number);
            Assert.AreEqual(course.MinimumCredits, course2Dto.MinimumCredits);
            Assert.AreEqual(course.MaximumCredits, course2Dto.MaximumCredits);
            Assert.AreEqual(course.Ceus, course2Dto.Ceus);
            Assert.AreEqual(course.Title, course2Dto.Title);
            Assert.AreEqual(course.Description, course2Dto.Description);
            Assert.AreEqual(course.YearsOffered, course2Dto.YearsOffered);
            Assert.AreEqual(course.TermsOffered, course2Dto.TermsOffered);
            Assert.AreEqual(course.TermYearlyCycle, course2Dto.TermYearlyCycle);
            Assert.AreEqual(course.TermSessionCycle, course2Dto.TermSessionCycle);
            Assert.AreEqual(course.LocationCodes.Count(), course2Dto.LocationCodes.Count());
            Assert.AreEqual(locationCount, course2Page.Locations.Count());
        }

        [TestMethod]
        public async Task Course2DtoContainsRequisiteData()
        {
            // Arrange -- Set up empty search
            var pageSize = Int16.MaxValue;
            var pageIndex = 2;
            var emptySearchCriteria = new CourseSearchCriteria();

            // Act -- Invoke Search service
            CoursePage2 course2Page = await courseService.Search2Async(emptySearchCriteria, pageSize, pageIndex);

            // Assert
            var course2Dto = course2Page.CurrentPageItems.Where(c => c.Id == "186").First();
            var course = catalogCourses.Where(c => c.Id == "186").First();
            Assert.AreEqual(course.Requisites.Count(), course2Dto.Requisites.Count());

        }

        [TestMethod]
        public async Task Course2DtoContainsLocationCycleRestrictionData()
        {
            // Arrange -- Set up empty search
            var pageSize = Int16.MaxValue;
            var pageIndex = 2;
            var emptySearchCriteria = new CourseSearchCriteria();

            // Act -- Invoke Search service
            CoursePage2 course2Page = await courseService.Search2Async(emptySearchCriteria, pageSize, pageIndex);

            // Assert
            var course2Dto = course2Page.CurrentPageItems.Where(c => c.Id == "46").First();
            var course = catalogCourses.Where(c => c.Id == "46").First();
            Assert.AreEqual(course.LocationCycleRestrictions.Count(), course2Dto.LocationCycleRestrictions.Count());

        }

        [TestMethod]
        public async Task DayOfWeekFilterCorrectlyLimitsSearchResults_WithSectionsHavePrimarySectionMeetings()
        {
            //update few sections to have PrimarySectionMeetings
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            var newList = allSections.ToList();
            newList.AddRange(primaryMeetingSections);
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(newList.AsEnumerable()));

            // Arrange - filter sections using day of week
            // Passed in and out of page as string 0 through 6 (Sun - Sat)
            var daysOfWeek = new List<string>() { "1", "3" }; // Mon, Wed
            var criteria = new CourseSearchCriteria() { DaysOfWeek = daysOfWeek };
            var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("997-SEC-WITH-PRIM-MTNGS"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("999-SEC-WITH-PRIM-MTNGS"));

            // Assert multiple DayOfWeek filters exist
            Assert.IsTrue(coursePage.DaysOfWeek.Count() == 4); // some courses have MWTH
                                                               // Assert Selected flag is set properly 
            Assert.IsTrue(coursePage.DaysOfWeek.Where(c => c.Selected == true).Count() == 2);
            Assert.IsTrue(coursePage.DaysOfWeek.Where(c => c.Selected == true).Select(x => x.Value).First() == "1");

            // Check Monday course count
            Assert.IsTrue(coursePage.DaysOfWeek.Where(c => c.Selected == true).Select(x => x.Count).First() >= 25);
            // Check Wednesday course count
            Assert.IsTrue(coursePage.DaysOfWeek.Where(c => c.Value == "3").Select(x => x.Count).First() >= 25);
        }

        [TestMethod]

        public async Task TimeOfDayFilter_WithPrimarySectionMeetings()
        {

            //update few sections to have PrimarySectionMeetings
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            List<Ellucian.Colleague.Domain.Student.Entities.Section> newList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();

            //add few sections that have section.Meetings AND NO PrimaryscetionMeetings
            newList.Add(allSections.ToList()[0]); //no meetings
            newList.Add(allSections.ToList()[1]); //no meetings
            newList.Add(allSections.ToList()[2]);//no meetings
            newList.Add(allSections.ToList()[3]); // 9-9:50 am
            newList.Add(allSections.ToList()[4]);//1am - 1:50am 
            newList.Add(allSections.ToList()[5]); //have 2 meetings- 11-11:50 am  and second with start time & end time  null



            newList.AddRange(primaryMeetingSections);
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(newList.AsEnumerable()));
            // Arrange - filter sections using time range
            var earliestTimeSpan = new TimeSpan(09, 00, 00);
            var latestTimeSpan = new TimeSpan(11, 00, 00);
            var criteria = new CourseSearchCriteria() { EarliestTime = (int)earliestTimeSpan.TotalMinutes, LatestTime = (int)latestTimeSpan.TotalMinutes };
            // Act - call course search
            var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("4"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("999-SEC-WITH-PRIM-MTNGS"));
            Assert.IsFalse(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("997-SEC-WITH-PRIM-MTNGS"));
            Assert.AreEqual(2, coursePage.CurrentPageItems.Count());



            // Verify that the sections included take place during the specified time span
            // Get each section found for this course
            var courseSections = (from sectionId in coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds)
                                  join sec in newList
                                  on sectionId equals sec.Id into joinSections
                                  from section in joinSections
                                  select section).ToList();
            var timeCollection = (from sec in courseSections
                                  from mtg in sec.Meetings
                                  select new { mtg.StartTime, mtg.EndTime, sec.Id }).Union
                                 (from sec in courseSections
                                  from mtg in sec.PrimarySectionMeetings
                                  select new { mtg.StartTime, mtg.EndTime, sec.Id });
            foreach (var mtgTime in timeCollection)
            {
                // If no start/end time given, don't try to compare against the criteria earliest/latest time
                if (mtgTime.StartTime != null)
                {
                    DateTimeOffset startTime = mtgTime.StartTime.GetValueOrDefault();
                    Assert.IsTrue(startTime.DateTime.TimeOfDay.TotalMinutes >= criteria.EarliestTime);
                }
                if (mtgTime.EndTime != null)
                {
                    DateTimeOffset endTime = mtgTime.EndTime.GetValueOrDefault();
                    Assert.IsTrue(endTime.DateTime.TimeOfDay.TotalMinutes <= criteria.LatestTime);
                }
            }
        }

        [TestMethod]
        public async Task TimeOfDayFilter_WithOnlyPrimarySectionMeetings()
        {

            //update few sections to have PrimarySectionMeetings
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(primaryMeetingSections.AsEnumerable()));
            // Arrange - filter sections using time range
            var earliestTimeSpan = new TimeSpan(09, 00, 00);
            var latestTimeSpan = new TimeSpan(12, 00, 00);
            var criteria = new CourseSearchCriteria() { EarliestTime = (int)earliestTimeSpan.TotalMinutes, LatestTime = (int)latestTimeSpan.TotalMinutes };
            // Act - call course search
            var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("999-SEC-WITH-PRIM-MTNGS"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("997-SEC-WITH-PRIM-MTNGS"));


            // Verify that the sections included take place during the specified time span
            // Get each section found for this course
            var courseSections = (from sectionId in coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds)
                                  join sec in primaryMeetingSections
                                  on sectionId equals sec.Id into joinSections
                                  from section in joinSections
                                  select section).ToList();
            var timeCollection = (from sec in courseSections
                                  from mtg in sec.PrimarySectionMeetings
                                  select new { mtg.StartTime, mtg.EndTime, sec.Id });

            foreach (var mtgTime in timeCollection)
            {
                // If no start/end time given, don't try to compare against the criteria earliest/latest time
                if (mtgTime.StartTime != null)
                {
                    DateTimeOffset startTime = mtgTime.StartTime.GetValueOrDefault();
                    Assert.IsTrue(startTime.DateTime.TimeOfDay.TotalMinutes >= criteria.EarliestTime);
                }
                if (mtgTime.EndTime != null)
                {
                    DateTimeOffset endTime = mtgTime.EndTime.GetValueOrDefault();
                    Assert.IsTrue(endTime.DateTime.TimeOfDay.TotalMinutes <= criteria.LatestTime);
                }
            }
        }

        [TestMethod]
        public async Task StartAtTimeEndByTimeFilter_WithPrimarySectionMeetings()
        {

            //update few sections to have PrimarySectionMeetings
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            List<Ellucian.Colleague.Domain.Student.Entities.Section> newList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();

            //add few sections that have section.Meetings AND NO PrimaryscetionMeetings
            newList.Add(allSections.ToList()[0]); //no meetings
            newList.Add(allSections.ToList()[1]); //no meetings
            newList.Add(allSections.ToList()[2]);//no meetings
            newList.Add(allSections.ToList()[3]); // 9-9:50 am
            newList.Add(allSections.ToList()[4]);//1am - 1:50am 
            newList.Add(allSections.ToList()[5]); //have 2 meetings- 11-11:50 am  and second with start time & end time  null



            newList.AddRange(primaryMeetingSections);
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(newList.AsEnumerable()));
         
            var criteria = new CourseSearchCriteria() { StartsAtTime = "09:00 AM", EndsByTime = "11:00 AM" };
            // Act - call course search
            var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("4"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("999-SEC-WITH-PRIM-MTNGS"));
            Assert.IsFalse(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("997-SEC-WITH-PRIM-MTNGS"));
            Assert.AreEqual(2, coursePage.CurrentPageItems.Count());

        }

        [TestMethod]
        public async Task StartAtTimeEndsByTimeFilter_WithOnlyPrimarySectionMeetings()
        {

            //update few sections to have PrimarySectionMeetings
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(primaryMeetingSections.AsEnumerable()));
            // Arrange - filter sections using time range
          
            var criteria = new CourseSearchCriteria() { StartsAtTime = "09:00 AM", EndsByTime = "12:00 PM"};
            // Act - call course search
            var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("999-SEC-WITH-PRIM-MTNGS"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("997-SEC-WITH-PRIM-MTNGS"));
            Assert.AreEqual(2, coursePage.CurrentPageItems.Count());


        }

        [TestMethod]
        public async Task CatalogSearchAsync_WhenTODAndStartAtAndEndAtTimesAreProvided()
        {

            //update few sections to have PrimarySectionMeetings
            //primary section meeting times are:
            /*
             * Section- 999-SEC with 2 meeting times - 9:10am- 10am and  9:10 to 9:50am
             * section - 998-SEC with 1pm -1:50pm
             * section -997-SEC with 11:00am - 11:50am
             */
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            List<Ellucian.Colleague.Domain.Student.Entities.Section> newList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            //modifying a section from the collection to have start time of 8
            Ellucian.Colleague.Domain.Student.Entities.Section sectionStartAt8am = allSections.ToList()[0];
            //set meeting times on section
            string guid = Guid.NewGuid().ToString();
            Ellucian.Colleague.Domain.Student.Entities.SectionMeeting mp3 = new Ellucian.Colleague.Domain.Student.Entities.SectionMeeting("something new", sectionStartAt8am.Id, "LEC", null, null, "W")
            {
                Guid = guid,
                StartTime = new DateTimeOffset(new DateTime(2012, 8, 9, 08, 00, 00)),
                EndTime = new DateTimeOffset(new DateTime(2012, 8, 9, 10, 50, 00)),
                Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday }
            };


            sectionStartAt8am.UpdatePrimarySectionMeetings(new List<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting>() { mp3 });
            sectionStartAt8am.FirstMeetingDate = sectionStartAt8am.StartDate.AddDays(15);
            sectionStartAt8am.LastMeetingDate = sectionStartAt8am.EndDate;
            newList.Add(sectionStartAt8am); 
            newList.Add(allSections.ToList()[1]); //no meetings
            newList.Add(allSections.ToList()[2]);//no meetings
            newList.Add(allSections.ToList()[3]); // 9-9:50 am
            newList.Add(allSections.ToList()[4]);//1am - 1:50am 
            newList.Add(allSections.ToList()[5]); //have 2 meetings- 11-11:50 am  and second with start time & end time  null
            newList.AddRange(primaryMeetingSections);

            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(newList.AsEnumerable()));
            var earliestTimeSpan = new TimeSpan(08, 00, 00);
            var latestTimeSpan = new TimeSpan(12, 00, 00);
            var criteria = new CourseSearchCriteria() { EarliestTime = (int)earliestTimeSpan.TotalMinutes, LatestTime = (int)latestTimeSpan.TotalMinutes };

            criteria.StartsAtTime = "09:00 AM";
            criteria.EndsByTime = "02:00 PM";
            //sections retrieved will be from 8am to 2pm
            // Act - call course search
            var coursePage = await courseService.Search2Async(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("4"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("6"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("1"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("999-SEC-WITH-PRIM-MTNGS"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("997-SEC-WITH-PRIM-MTNGS"));
            Assert.IsTrue(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains("998-SEC-WITH-PRIM-MTNGS"));
            Assert.IsFalse(coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Contains(allSections.ToList()[4].Id));//shuld not have section that starts t 1am
            Assert.AreEqual(2, coursePage.CurrentPageItems.Count());
            Assert.AreEqual(6,coursePage.CurrentPageItems.SelectMany(c => c.MatchingSectionIds).Count());

        }

        [TestClass]
        public class BuildDayOfWeekFilter_PrivateMethod_Tests : Search3
        {
            MethodInfo methodInfo = null;
            List<SectionEntity> sections = new List<SectionEntity>();

            [TestInitialize]
            public async void Initialize()
            {
                base.Initialize();
                methodInfo = typeof(CourseService).GetMethod("BuildDayOfWeekFilter", BindingFlags.NonPublic | BindingFlags.Instance);


                List<Domain.Student.Entities.OfferingDepartment> departments = new List<Domain.Student.Entities.OfferingDepartment>();
                departments.Add(new Domain.Student.Entities.OfferingDepartment("Math"));

                //first section
                SectionEntity sec1 = new SectionEntity("s001", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 1", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting1 = new Domain.Student.Entities.SectionMeeting("meeting-1", "s001", "inst", null, null, "");
                secMeeting1.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday });

                Domain.Student.Entities.SectionMeeting secMeeting2 = new Domain.Student.Entities.SectionMeeting("meeting-2", "s001", "lab", null, null, "");
                secMeeting2.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });

                sec1.AddSectionMeeting(secMeeting1);
                sec1.AddSectionMeeting(secMeeting2);

                //2nd section with same course as section 1
                SectionEntity sec2 = new SectionEntity("s002", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 2", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting3 = new Domain.Student.Entities.SectionMeeting("meeting-3", "s002", "inst", null, null, "");
                secMeeting3.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });

                Domain.Student.Entities.SectionMeeting secMeeting4 = new Domain.Student.Entities.SectionMeeting("meeting-4", "s002", "lab", null, null, "");
                secMeeting4.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });

                sec2.AddSectionMeeting(secMeeting3);
                sec2.AddSectionMeeting(secMeeting4);

                //3rd section with different course
                SectionEntity sec3 = new SectionEntity("s003", "c002", "1", DateTime.Today.AddDays(-2), 2, null, "section 3", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting5 = new Domain.Student.Entities.SectionMeeting("meeting-5", "s003", "inst", null, null, "");
                secMeeting5.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });

                Domain.Student.Entities.SectionMeeting secMeeting6 = new Domain.Student.Entities.SectionMeeting("meeting-6", "s003", "lab", null, null, "");
                secMeeting6.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Friday });

                sec3.AddSectionMeeting(secMeeting5);
                sec3.AddSectionMeeting(secMeeting6);

                //add the sections created above to the list
                sections.Add(sec1);
                sections.Add(sec2);
                sections.Add(sec3);

                /* This should how it should be grouped
                 * M-  s001-c001-inst, s001-c001-lab, s002-c001-inst, s002-c001-lab, s003-c002-inst, s003-c002-lab
                 * Tu- nothing
                 * W-  s001-c001-lab, s002-c001-lab
                 * Th- s002-c001-inst, s003-c002-inst
                 * Fr- s003-c002-lab
                 * Sa- nothing
                 * Su- nothing
                 * 
                 * This should be the count
                 * "1" M- 2
                 * "2" Tu -0
                 * "3" w - 1
                 * "4" Th - 2
                 * "5" Fr- 1
                 * "6" Sa -0
                 * "0" Sun -0
                 */



            }

            [TestCleanup]
            public void Cleanup()
            {
            }


            [TestMethod]
            public async Task BuildDayOfWeekFilter_Validate()
            {
                object[] parameters = { sections, new CourseSearchCriteria() };
                List<Ellucian.Colleague.Dtos.Base.Filter> dayOfFilter = (List<Ellucian.Colleague.Dtos.Base.Filter>)methodInfo.Invoke(courseService, parameters);
                Assert.IsNotNull(dayOfFilter);
                Assert.AreEqual(4, dayOfFilter.Count);
                //monday
                Assert.AreEqual("1", dayOfFilter[0].Value);
                Assert.AreEqual(2, dayOfFilter[0].Count);
                //wednesday
                Assert.AreEqual("3", dayOfFilter[1].Value);
                Assert.AreEqual(1, dayOfFilter[1].Count);
                //thursday
                Assert.AreEqual("4", dayOfFilter[2].Value);
                Assert.AreEqual(2, dayOfFilter[2].Count);
                //friday
                Assert.AreEqual("5", dayOfFilter[3].Value);
                Assert.AreEqual(1, dayOfFilter[3].Count);


            }
            [TestMethod]
            public async Task BuildDayOfWeekFilter_Section_Mtngs_With_NoDays()
            {
                List<Domain.Student.Entities.OfferingDepartment> departments = new List<Domain.Student.Entities.OfferingDepartment>();
                departments.Add(new Domain.Student.Entities.OfferingDepartment("Math"));

                List<SectionEntity> sections = new List<SectionEntity>();
                //first section
                SectionEntity sec1 = new SectionEntity("s001", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 1", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting1 = new Domain.Student.Entities.SectionMeeting("meeting-1", "s001", "inst", null, null, "");

                Domain.Student.Entities.SectionMeeting secMeeting2 = new Domain.Student.Entities.SectionMeeting("meeting-2", "s001", "lab", null, null, "");

                sec1.AddSectionMeeting(secMeeting1);
                sec1.AddSectionMeeting(secMeeting2);
                //2nd section with same course as section 1
                SectionEntity sec2 = new SectionEntity("s002", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 2", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                //3rd section with different course
                SectionEntity sec3 = new SectionEntity("s003", "c002", "1", DateTime.Today.AddDays(-2), 2, null, "section 3", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                //add the sections created above to the list
                sections.Add(sec1);
                sections.Add(sec2);
                sections.Add(sec3);
                object[] parameters = { sections, new CourseSearchCriteria() };
                List<Ellucian.Colleague.Dtos.Base.Filter> dayOfFilter = (List<Ellucian.Colleague.Dtos.Base.Filter>)methodInfo.Invoke(courseService, parameters);
                Assert.IsNotNull(dayOfFilter);
                Assert.AreEqual(0, dayOfFilter.Count);

            }
            [TestMethod]
            public async Task BuildDayOfWeekFilter_Section_Have_Empty_Section_Mtngs()
            {
                List<Domain.Student.Entities.OfferingDepartment> departments = new List<Domain.Student.Entities.OfferingDepartment>();
                departments.Add(new Domain.Student.Entities.OfferingDepartment("Math"));

                List<SectionEntity> sections = new List<SectionEntity>();
                //first section
                SectionEntity sec1 = new SectionEntity("s001", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 1", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                //2nd section with same course as section 1
                SectionEntity sec2 = new SectionEntity("s002", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 2", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                //3rd section with different course
                SectionEntity sec3 = new SectionEntity("s003", "c002", "1", DateTime.Today.AddDays(-2), 2, null, "section 3", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                //add the sections created above to the list
                sections.Add(sec1);
                sections.Add(sec2);
                sections.Add(sec3);
                object[] parameters = { sections, new CourseSearchCriteria() };
                List<Ellucian.Colleague.Dtos.Base.Filter> dayOfFilter = (List<Ellucian.Colleague.Dtos.Base.Filter>)methodInfo.Invoke(courseService, parameters);
                Assert.IsNotNull(dayOfFilter);
                Assert.AreEqual(0, dayOfFilter.Count);

            }
            [TestMethod]
            public async Task BuildDayOfWeekFilter_Section_Have_primary_Mtngs()
            {
                List<Domain.Student.Entities.OfferingDepartment> departments = new List<Domain.Student.Entities.OfferingDepartment>();
                departments.Add(new Domain.Student.Entities.OfferingDepartment("Math"));

                //first section
                SectionEntity sec1 = new SectionEntity("s001", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 1", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting1 = new Domain.Student.Entities.SectionMeeting("meeting-1", "s001", "inst", null, null, "");
                secMeeting1.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday });

                Domain.Student.Entities.SectionMeeting secMeeting2 = new Domain.Student.Entities.SectionMeeting("meeting-2", "s001", "lab", null, null, "");
                secMeeting2.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });

                sec1.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting1, secMeeting2 });

                //2nd section with same course as section 1
                SectionEntity sec2 = new SectionEntity("s002", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 2", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting3 = new Domain.Student.Entities.SectionMeeting("meeting-3", "s002", "inst", null, null, "");
                secMeeting3.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });

                Domain.Student.Entities.SectionMeeting secMeeting4 = new Domain.Student.Entities.SectionMeeting("meeting-4", "s002", "lab", null, null, "");
                secMeeting4.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });

                sec2.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting3, secMeeting4 });

                //3rd section with different course
                SectionEntity sec3 = new SectionEntity("s003", "c002", "1", DateTime.Today.AddDays(-2), 2, null, "section 3", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting5 = new Domain.Student.Entities.SectionMeeting("meeting-5", "s003", "inst", null, null, "");
                secMeeting5.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });

                Domain.Student.Entities.SectionMeeting secMeeting6 = new Domain.Student.Entities.SectionMeeting("meeting-6", "s003", "lab", null, null, "");
                secMeeting6.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Friday });

                sec3.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting5, secMeeting6 });

                //add the sections created above to the list
                sections.Add(sec1);
                sections.Add(sec2);
                sections.Add(sec3);
                object[] parameters = { sections, new CourseSearchCriteria() };
                List<Ellucian.Colleague.Dtos.Base.Filter> dayOfFilter = (List<Ellucian.Colleague.Dtos.Base.Filter>)methodInfo.Invoke(courseService, parameters);
                Assert.IsNotNull(dayOfFilter);
                Assert.AreEqual(4, dayOfFilter.Count);
                //monday
                Assert.AreEqual("1", dayOfFilter[0].Value);
                Assert.AreEqual(2, dayOfFilter[0].Count);
                //wednesday
                Assert.AreEqual("3", dayOfFilter[1].Value);
                Assert.AreEqual(1, dayOfFilter[1].Count);
                //thursday
                Assert.AreEqual("4", dayOfFilter[2].Value);
                Assert.AreEqual(2, dayOfFilter[2].Count);
                //friday
                Assert.AreEqual("5", dayOfFilter[3].Value);
                Assert.AreEqual(1, dayOfFilter[3].Count);
            }
            [TestMethod]
            public async Task BuildDayOfWeekFilter_Section_Have_Meetngs_And_primaryMtngs()
            {
                List<Domain.Student.Entities.OfferingDepartment> departments = new List<Domain.Student.Entities.OfferingDepartment>();
                departments.Add(new Domain.Student.Entities.OfferingDepartment("Math"));

                //first section
                SectionEntity sec1 = new SectionEntity("s001", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 1", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting1 = new Domain.Student.Entities.SectionMeeting("meeting-1", "s001", "inst", null, null, "");
                secMeeting1.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday });

                Domain.Student.Entities.SectionMeeting secMeeting2 = new Domain.Student.Entities.SectionMeeting("meeting-2", "s001", "lab", null, null, "");
                secMeeting2.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });

                sec1.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting1, secMeeting2 });

                //2nd section with same course as section 1
                SectionEntity sec2 = new SectionEntity("s002", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 2", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting3 = new Domain.Student.Entities.SectionMeeting("meeting-3", "s002", "inst", null, null, "");
                secMeeting3.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });

                Domain.Student.Entities.SectionMeeting secMeeting4 = new Domain.Student.Entities.SectionMeeting("meeting-4", "s002", "lab", null, null, "");
                secMeeting4.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });

                sec2.AddSectionMeeting(secMeeting3);
                sec2.AddSectionMeeting(secMeeting4);

                //3rd section with different course
                SectionEntity sec3 = new SectionEntity("s003", "c002", "1", DateTime.Today.AddDays(-2), 2, null, "section 3", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting5 = new Domain.Student.Entities.SectionMeeting("meeting-5", "s003", "inst", null, null, "");
                secMeeting5.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });

                Domain.Student.Entities.SectionMeeting secMeeting6 = new Domain.Student.Entities.SectionMeeting("meeting-6", "s003", "lab", null, null, "");
                secMeeting6.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Friday });

                sec3.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting5, secMeeting6 });

                //add the sections created above to the list
                sections.Add(sec1);
                sections.Add(sec2);
                sections.Add(sec3);
                object[] parameters = { sections, new CourseSearchCriteria() };
                List<Ellucian.Colleague.Dtos.Base.Filter> dayOfFilter = (List<Ellucian.Colleague.Dtos.Base.Filter>)methodInfo.Invoke(courseService, parameters);
                Assert.IsNotNull(dayOfFilter);
                Assert.AreEqual(4, dayOfFilter.Count);
                //monday
                Assert.AreEqual("1", dayOfFilter[0].Value);
                Assert.AreEqual(2, dayOfFilter[0].Count);
                //wednesday
                Assert.AreEqual("3", dayOfFilter[1].Value);
                Assert.AreEqual(1, dayOfFilter[1].Count);
                //thursday
                Assert.AreEqual("4", dayOfFilter[2].Value);
                Assert.AreEqual(2, dayOfFilter[2].Count);
                //friday
                Assert.AreEqual("5", dayOfFilter[3].Value);
                Assert.AreEqual(1, dayOfFilter[3].Count);

            }

            [TestMethod]
            public async Task BuildDayOfWeekFilter_Section_Have_Both_Meetngs_And_primaryMtngs()
            {
                List<Domain.Student.Entities.OfferingDepartment> departments = new List<Domain.Student.Entities.OfferingDepartment>();
                departments.Add(new Domain.Student.Entities.OfferingDepartment("Math"));

                //first section
                SectionEntity sec1 = new SectionEntity("s001", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 1", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting1 = new Domain.Student.Entities.SectionMeeting("meeting-1", "s001", "inst", null, null, "");
                secMeeting1.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday });

                Domain.Student.Entities.SectionMeeting secMeeting2 = new Domain.Student.Entities.SectionMeeting("meeting-2", "s001", "lab", null, null, "");
                secMeeting2.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });
                sec1.AddSectionMeeting(secMeeting1);
                sec1.AddSectionMeeting(secMeeting2);

                sec1.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting1, secMeeting2 });

                //2nd section with same course as section 1
                SectionEntity sec2 = new SectionEntity("s002", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 2", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting3 = new Domain.Student.Entities.SectionMeeting("meeting-3", "s002", "inst", null, null, "");
                secMeeting3.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });

                Domain.Student.Entities.SectionMeeting secMeeting4 = new Domain.Student.Entities.SectionMeeting("meeting-4", "s002", "lab", null, null, "");
                secMeeting4.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });

                sec2.AddSectionMeeting(secMeeting3);
                sec2.AddSectionMeeting(secMeeting4);
                sec2.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting3, secMeeting4 });

                //3rd section with different course
                SectionEntity sec3 = new SectionEntity("s003", "c002", "1", DateTime.Today.AddDays(-2), 2, null, "section 3", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting5 = new Domain.Student.Entities.SectionMeeting("meeting-5", "s003", "inst", null, null, "");
                secMeeting5.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });

                Domain.Student.Entities.SectionMeeting secMeeting6 = new Domain.Student.Entities.SectionMeeting("meeting-6", "s003", "lab", null, null, "");
                secMeeting6.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Friday });

                sec3.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting5, secMeeting6 });
                sec3.AddSectionMeeting(secMeeting5);
                sec3.AddSectionMeeting(secMeeting6);

                //add the sections created above to the list
                sections.Add(sec1);
                sections.Add(sec2);
                sections.Add(sec3);
                object[] parameters = { sections, new CourseSearchCriteria() };
                List<Ellucian.Colleague.Dtos.Base.Filter> dayOfFilter = (List<Ellucian.Colleague.Dtos.Base.Filter>)methodInfo.Invoke(courseService, parameters);
                Assert.IsNotNull(dayOfFilter);
                Assert.AreEqual(4, dayOfFilter.Count);
                //monday
                Assert.AreEqual("1", dayOfFilter[0].Value);
                Assert.AreEqual(2, dayOfFilter[0].Count);
                //wednesday
                Assert.AreEqual("3", dayOfFilter[1].Value);
                Assert.AreEqual(1, dayOfFilter[1].Count);
                //thursday
                Assert.AreEqual("4", dayOfFilter[2].Value);
                Assert.AreEqual(2, dayOfFilter[2].Count);
                //friday
                Assert.AreEqual("5", dayOfFilter[3].Value);
                Assert.AreEqual(1, dayOfFilter[3].Count);

            }

            [TestMethod]
            public async Task BuildDayOfWeekFilter_Section_Meetngs_And_Other_Sections_Have_same_primaryMtngs_Selected_criteria()
            {
                List<Domain.Student.Entities.OfferingDepartment> departments = new List<Domain.Student.Entities.OfferingDepartment>();
                departments.Add(new Domain.Student.Entities.OfferingDepartment("Math"));

                //sections with section meetings
                //first section
                SectionEntity sec1 = new SectionEntity("s001", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 1", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());

                Domain.Student.Entities.SectionMeeting secMeeting1 = new Domain.Student.Entities.SectionMeeting("meeting-1", "s001", "inst", null, null, "");
                secMeeting1.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday });
                Domain.Student.Entities.SectionMeeting secMeeting2 = new Domain.Student.Entities.SectionMeeting("meeting-2", "s001", "lab", null, null, "");
                secMeeting2.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });
                sec1.AddSectionMeeting(secMeeting1);
                sec1.AddSectionMeeting(secMeeting2);
                //2nd section with same course as section 1
                SectionEntity sec2 = new SectionEntity("s002", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 2", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                Domain.Student.Entities.SectionMeeting secMeeting3 = new Domain.Student.Entities.SectionMeeting("meeting-3", "s002", "inst", null, null, "");
                secMeeting3.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });
                Domain.Student.Entities.SectionMeeting secMeeting4 = new Domain.Student.Entities.SectionMeeting("meeting-4", "s002", "lab", null, null, "");
                secMeeting4.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });
                sec2.AddSectionMeeting(secMeeting3);
                sec2.AddSectionMeeting(secMeeting4);
                //3rd section with different course
                SectionEntity sec3 = new SectionEntity("s003", "c002", "1", DateTime.Today.AddDays(-2), 2, null, "section 3", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                Domain.Student.Entities.SectionMeeting secMeeting5 = new Domain.Student.Entities.SectionMeeting("meeting-5", "s003", "inst", null, null, "");
                secMeeting5.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });
                Domain.Student.Entities.SectionMeeting secMeeting6 = new Domain.Student.Entities.SectionMeeting("meeting-6", "s003", "lab", null, null, "");
                secMeeting6.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Friday });
                sec3.AddSectionMeeting(secMeeting5);
                sec3.AddSectionMeeting(secMeeting6);

                //sections with primary  meetings
                //first section
                SectionEntity sec4 = new SectionEntity("s001", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 4", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                Domain.Student.Entities.SectionMeeting secMeeting7 = new Domain.Student.Entities.SectionMeeting("meeting-7", "s001", "inst", null, null, "");
                secMeeting7.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday });
                Domain.Student.Entities.SectionMeeting secMeeting8 = new Domain.Student.Entities.SectionMeeting("meeting-8", "s001", "lab", null, null, "");
                secMeeting8.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });
                sec4.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting7, secMeeting8 });
                //2nd section with same course as section 1
                SectionEntity sec5 = new SectionEntity("s002", "c001", "1", DateTime.Today.AddDays(-2), 2, null, "section 5", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                Domain.Student.Entities.SectionMeeting secMeeting9 = new Domain.Student.Entities.SectionMeeting("meeting-9", "s002", "inst", null, null, "");
                secMeeting9.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });
                Domain.Student.Entities.SectionMeeting secMeeting10 = new Domain.Student.Entities.SectionMeeting("meeting-10", "s002", "lab", null, null, "");
                secMeeting10.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday });
                sec5.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting9, secMeeting10 });

                //3rd section with different course
                SectionEntity sec6 = new SectionEntity("s003", "c002", "1", DateTime.Today.AddDays(-2), 2, null, "section 6", "P", departments, courseLevelCodes: new List<string>() { "100" }, academicLevelCode: "UG", statuses: new List<Domain.Student.Entities.SectionStatusItem>());
                Domain.Student.Entities.SectionMeeting secMeeting11 = new Domain.Student.Entities.SectionMeeting("meeting-11", "s003", "inst", null, null, "");
                secMeeting11.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Thursday });
                Domain.Student.Entities.SectionMeeting secMeeting12 = new Domain.Student.Entities.SectionMeeting("meeting-12", "s003", "lab", null, null, "");
                secMeeting12.Days.AddRange(new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Friday });
                sec6.UpdatePrimarySectionMeetings(new List<Domain.Student.Entities.SectionMeeting>() { secMeeting11, secMeeting12 });

                //add the sections created above to the list
                sections.Add(sec1);
                sections.Add(sec2);
                sections.Add(sec3);
                sections.Add(sec4);
                sections.Add(sec5);
                sections.Add(sec6);
                object[] parameters = { sections, new CourseSearchCriteria() { DaysOfWeek = new List<string>() { "1", "4" } } };
                List<Ellucian.Colleague.Dtos.Base.Filter> dayOfFilter = (List<Ellucian.Colleague.Dtos.Base.Filter>)methodInfo.Invoke(courseService, parameters);
                Assert.IsNotNull(dayOfFilter);
                Assert.AreEqual(4, dayOfFilter.Count);
                //monday
                Assert.AreEqual("1", dayOfFilter[0].Value);
                Assert.AreEqual(2, dayOfFilter[0].Count);
                Assert.IsTrue(dayOfFilter[0].Selected);
                //wednesday
                Assert.AreEqual("3", dayOfFilter[1].Value);
                Assert.AreEqual(1, dayOfFilter[1].Count);
                Assert.IsFalse(dayOfFilter[1].Selected);
                //thursday
                Assert.AreEqual("4", dayOfFilter[2].Value);
                Assert.AreEqual(2, dayOfFilter[2].Count);
                Assert.IsTrue(dayOfFilter[2].Selected);
                //friday
                Assert.AreEqual("5", dayOfFilter[3].Value);
                Assert.AreEqual(1, dayOfFilter[3].Count);
                Assert.IsFalse(dayOfFilter[3].Selected);

            }
        }


    }

    [TestClass]
    public class SectionSearchAsync : Search3
    {
        [TestInitialize]
        public new async void Initialize()
        {
            base.Initialize();

            //mock adapter
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionAdapter);
            //mock adapter
            var sectionMeetingAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Dtos.Student.SectionMeeting2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Dtos.Student.SectionMeeting2>()).Returns(sectionMeetingAdapter);

            //mock adapter
            var sectionRequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>()).Returns(sectionRequisiteAdapter);

        }

        [TestMethod]
        public async Task CriteriaIsNull()
        {
            SectionPage sectionPage = await courseService.SectionSearchAsync(null, int.MaxValue, 0);
            //should return an empty section page
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
            //filters are null
            Assert.IsNull(sectionPage.Locations);
            Assert.IsNull(sectionPage.Faculty);
            Assert.IsNull(sectionPage.DaysOfWeek);
            Assert.IsNull(sectionPage.TopicCodes);
            Assert.IsNull(sectionPage.CourseTypes);
            Assert.IsNull(sectionPage.Terms);
            Assert.IsNull(sectionPage.OnlineCategories);
            Assert.IsNull(sectionPage.OpenSections);
            Assert.IsNull(sectionPage.OpenAndWaitlistSections);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
        }


        //when criteria is not null but has ONLY keyword, courses or section id. 
        [TestMethod]
        public async Task SectionSearch_CriteriaOnlyHasCourseIds()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria() { CourseIds = new List<string>() { "7272", "10003" }, SectionIds = new List<string>() { "166", "167", "168" } };
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            //should return an empty section page
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(21, sectionPage.CurrentPageItems.Count());
            //filters are not null
            Assert.AreEqual(6, sectionPage.Faculty.Count());
            Assert.AreEqual(3, sectionPage.Terms.Count());
        }


        //when criteria provided but repository has no sections 
        [TestMethod]
        public async Task CriteriaIsNotNull_NoSectionsReturnedFromRepository()
        {
            // Arrange -- Mock section repository so that no registration sections are found
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>>(new List<Ellucian.Colleague.Domain.Student.Entities.Section>()));
            sectionRepoMock.Setup(repo => repo.GetCourseSectionsCachedAsync(It.IsAny<List<string>>(), regTerms)).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Section>>(new List<Ellucian.Colleague.Domain.Student.Entities.Section>()));
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.Keyword = "History";
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);

            //should get no sections returned
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
            //filters are empty
            Assert.AreEqual(0, sectionPage.Locations.Count());
            Assert.AreEqual(0, sectionPage.Faculty.Count());//36,48,45,49,46,50
            Assert.AreEqual(0, sectionPage.DaysOfWeek.Count());
            Assert.AreEqual(0, sectionPage.TopicCodes.Count());//T1, null
            Assert.AreEqual(0, sectionPage.Terms.Count());//2012/FA
            Assert.AreEqual(0, sectionPage.OnlineCategories.Count());
            Assert.AreEqual(0, sectionPage.OpenSections.Count);
            Assert.AreEqual(0, sectionPage.OpenAndWaitlistSections.Count);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
        }

        //when sections from repo have sections that matched few sections with kword 
        [TestMethod]
        public async Task CriteriaHasKeywordOnly_ReturnsSections_Filters_Day_Location()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.Keyword = "History";
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from  repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(75, sectionPage.CurrentPageItems.Count());

            //filters are not empty
            Assert.AreEqual(2, sectionPage.Locations.Count());
            Assert.AreEqual(6, sectionPage.Faculty.Count());//36,48,45,49,46,50
            Assert.AreEqual(5, sectionPage.DaysOfWeek.Count());
            Assert.AreEqual(3, sectionPage.TopicCodes.Count());//T1, null
            Assert.AreEqual(3, sectionPage.Terms.Count());//2012/FA
            Assert.AreEqual(3, sectionPage.OnlineCategories.Count());
            Assert.AreEqual(75, sectionPage.OpenSections.Count);
            Assert.AreEqual(75, sectionPage.OpenAndWaitlistSections.Count);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
            // Verify counts and values in some of the filters
            Assert.AreEqual(18, ((Dtos.Base.Filter)sectionPage.DaysOfWeek.ToList()[1]).Count);
            Assert.AreEqual("4", ((Dtos.Base.Filter)sectionPage.DaysOfWeek.ToList()[1]).Value);
            Assert.AreEqual(30, ((Dtos.Base.Filter)sectionPage.DaysOfWeek.ToList()[2]).Count);
            Assert.AreEqual("1", ((Dtos.Base.Filter)sectionPage.DaysOfWeek.ToList()[2]).Value);
            Assert.AreEqual(13, ((Dtos.Base.Filter)sectionPage.Locations.ToList()[0]).Count);
            Assert.AreEqual("NW", ((Dtos.Base.Filter)sectionPage.Locations.ToList()[0]).Value);
            Assert.AreEqual(9, ((Dtos.Base.Filter)sectionPage.Locations.ToList()[1]).Count);
            Assert.AreEqual("MAIN", ((Dtos.Base.Filter)sectionPage.Locations.ToList()[1]).Value);
        }

        //when filter result does not return any sections. sections from repository have sections but keyword doesn't give any sections
        [TestMethod]
        public async Task CriteriaHasKeyword_NoSectionsMatchTheKW()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.Keyword = "xyz";
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned 
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
            //filters are empty
            Assert.AreEqual(0, sectionPage.Locations.Count());
            Assert.AreEqual(0, sectionPage.Faculty.Count());//36,48,45,49,46,50
            Assert.AreEqual(0, sectionPage.DaysOfWeek.Count());
            Assert.AreEqual(0, sectionPage.TopicCodes.Count());//T1, null
            Assert.AreEqual(0, sectionPage.Terms.Count());//2012/FA
            Assert.AreEqual(0, sectionPage.OnlineCategories.Count());
            Assert.AreEqual(0, sectionPage.OpenSections.Count);
            Assert.AreEqual(0, sectionPage.OpenAndWaitlistSections.Count);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
        }

        [TestMethod]
        public async Task SectionSearchAsync_CourseTypeFilters_Validated()
        {
            // Arrange -- Set up search criteria and page info then execute the section search

            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.Keyword = "Hist";

            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);

            // Validate
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(66, sectionPage.CurrentPageItems.Count());

            //Course Type filter is not empty and has correct counts
            Assert.AreEqual(2, sectionPage.CourseTypes.Count());

            // Verify counts and values in some of the filters
            Assert.AreEqual(27, ((Dtos.Base.Filter)sectionPage.CourseTypes.ToList()[0]).Count);
            Assert.AreEqual("STND", ((Dtos.Base.Filter)sectionPage.CourseTypes.ToList()[0]).Value);
            Assert.AreEqual(27, ((Dtos.Base.Filter)sectionPage.CourseTypes.ToList()[1]).Count);
            Assert.AreEqual("WR", ((Dtos.Base.Filter)sectionPage.CourseTypes.ToList()[1]).Value);
        }

        [TestMethod]
        public async Task SectionSearchAsync_CourseLevelFilters_Validated()
        {
            // Arrange -- Set up search criteria and page info then execute the section search

            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.Keyword = "Hist";

            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);

            // Validate
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(66, sectionPage.CurrentPageItems.Count());

            //Course Type filter is not empty and has correct counts
            Assert.AreEqual(4, sectionPage.CourseLevels.Count());

            // Verify counts and values in some of the filters
            Assert.AreEqual(27, ((Dtos.Base.Filter)sectionPage.CourseLevels.ToList()[0]).Count);
            Assert.AreEqual("100", ((Dtos.Base.Filter)sectionPage.CourseLevels.ToList()[0]).Value);
            Assert.AreEqual(18, ((Dtos.Base.Filter)sectionPage.CourseLevels.ToList()[1]).Count);
            Assert.AreEqual("200", ((Dtos.Base.Filter)sectionPage.CourseLevels.ToList()[1]).Value);
            Assert.AreEqual(9, ((Dtos.Base.Filter)sectionPage.CourseLevels.ToList()[2]).Count);
            Assert.AreEqual("300", ((Dtos.Base.Filter)sectionPage.CourseLevels.ToList()[2]).Value);
        }

        [TestMethod]
        public async Task SectionSearchAsync_AcademicLevelFilters_Validated()
        {
            // Arrange -- Set up search criteria and page info then execute the section search

            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.Keyword = "Hist";

            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);

            // Validate
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(66, sectionPage.CurrentPageItems.Count());

            //Course Type filter is not empty and has correct counts
            Assert.AreEqual(1, sectionPage.AcademicLevels.Count());

            // Verify counts and values in some of the filters
            Assert.AreEqual(66, ((Dtos.Base.Filter)sectionPage.AcademicLevels.ToList()[0]).Count);
            Assert.AreEqual("UG", ((Dtos.Base.Filter)sectionPage.AcademicLevels.ToList()[0]).Value);
        }

        [TestMethod]
        public async Task SectionSearchAsync_FacultyFilters_Validated()
        {
            // Arrange -- Set up search criteria and page info then execute the section search

            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.Keyword = "Hist";

            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);

            // Validate
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(66, sectionPage.CurrentPageItems.Count());

            //Faculty filter is not empty and has correct counts
            Assert.AreEqual(6, sectionPage.Faculty.Count());

            // Verify counts and values in some of the filters
            Assert.AreEqual(21, ((Dtos.Base.Filter)sectionPage.Faculty.ToList()[0]).Count);
            Assert.AreEqual("0000036", ((Dtos.Base.Filter)sectionPage.Faculty.ToList()[0]).Value);
            Assert.AreEqual(21, ((Dtos.Base.Filter)sectionPage.Faculty.ToList()[1]).Count);
            Assert.AreEqual("0000048", ((Dtos.Base.Filter)sectionPage.Faculty.ToList()[1]).Value);
        }

        [TestMethod]
        public async Task SectionSearchAsync_SubjectsFilters_Validated()
        {
            // Arrange -- Set up search criteria and page info then execute the section search

            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.Subjects = new List<string>() {"ACCT","ART" };

            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            int acctSections=allSections.Where(s => s.Subject == "ACCT").Count();
            int artSections = allSections.Where(s => s.Subject == "ART").Count();

            // Validate
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(acctSections + artSections, sectionPage.CurrentPageItems.Count());

            //Faculty filter is not empty and has correct counts
            Assert.AreEqual(2, sectionPage.Subjects.Count());

            // Verify counts and values in some of the filters
            Assert.AreEqual(acctSections, ((Dtos.Base.Filter)sectionPage.Subjects.ToList()[0]).Count);
            Assert.AreEqual("ACCT", ((Dtos.Base.Filter)sectionPage.Subjects.ToList()[0]).Value);
            Assert.AreEqual(true, ((Dtos.Base.Filter)sectionPage.Subjects.ToList()[0]).Selected);
            Assert.AreEqual(artSections, ((Dtos.Base.Filter)sectionPage.Subjects.ToList()[1]).Count);
            Assert.AreEqual("ART", ((Dtos.Base.Filter)sectionPage.Subjects.ToList()[1]).Value);
            Assert.AreEqual(true, ((Dtos.Base.Filter)sectionPage.Subjects.ToList()[1]).Selected);
        }

        [TestMethod]
        public async Task SectionSearchAsync_TimeOfDayFilter_WithPrimarySectionMeetings()
        {

            //update few sections to have PrimarySectionMeetings
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            List<Ellucian.Colleague.Domain.Student.Entities.Section> newList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();

            //add few sections that have section.Meetings AND NO PrimaryscetionMeetings
            newList.Add(allSections.ToList()[0]); //no meetings
            newList.Add(allSections.ToList()[1]); //no meetings
            newList.Add(allSections.ToList()[2]);//no meetings
            newList.Add(allSections.ToList()[3]); // 9-9:50 am
            newList.Add(allSections.ToList()[4]);//1am - 1:50am 
            newList.Add(allSections.ToList()[5]); //have 2 meetings- 11-11:50 am  and second with start time & end time  null



            newList.AddRange(primaryMeetingSections);
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(newList.AsEnumerable()));
            // Arrange - filter sections using time range
            var earliestTimeSpan = new TimeSpan(09, 00, 00);
            var latestTimeSpan = new TimeSpan(11, 00, 00);
            var criteria = new SectionSearchCriteria() { EarliestTime = (int)earliestTimeSpan.TotalMinutes, LatestTime = (int)latestTimeSpan.TotalMinutes };
            // Act - call course search
            SectionPage sectionPage = await courseService.SectionSearchAsync(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(sectionPage.CurrentPageItems.Select(c => c.Id).Contains("4"));
            Assert.IsTrue(sectionPage.CurrentPageItems.Select(c => c.Id).Contains("999-SEC-WITH-PRIM-MTNGS"));
            Assert.IsFalse(sectionPage.CurrentPageItems.Select(c => c.Id).Contains("997-SEC-WITH-PRIM-MTNGS"));
            Assert.AreEqual(2, sectionPage.CurrentPageItems.Count());



            // Verify that the sections included take place during the specified time span
            // Get each section found for this course
            var courseSections = (from sectionId in sectionPage.CurrentPageItems.Select(c => c.Id)
                                  join sec in newList
                                  on sectionId equals sec.Id into joinSections
                                  from section in joinSections
                                  select section).ToList();
            var timeCollection = (from sec in courseSections
                                  from mtg in sec.Meetings
                                  select new { mtg.StartTime, mtg.EndTime, sec.Id }).Union
                                 (from sec in courseSections
                                  from mtg in sec.PrimarySectionMeetings
                                  select new { mtg.StartTime, mtg.EndTime, sec.Id });
            foreach (var mtgTime in timeCollection)
            {
                // If no start/end time given, don't try to compare against the criteria earliest/latest time
                if (mtgTime.StartTime != null)
                {
                    DateTimeOffset startTime = mtgTime.StartTime.GetValueOrDefault();
                    Assert.IsTrue(startTime.DateTime.TimeOfDay.TotalMinutes >= criteria.EarliestTime);
                }
                if (mtgTime.EndTime != null)
                {
                    DateTimeOffset endTime = mtgTime.EndTime.GetValueOrDefault();
                    Assert.IsTrue(endTime.DateTime.TimeOfDay.TotalMinutes <= criteria.LatestTime);
                }
            }
        }

        [TestMethod]
        public async Task SectionSearchAsync_TimeOFDayFilter_WithOnlyPrimarySectionMeetings()
        {

            //update few sections to have PrimarySectionMeetings
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(primaryMeetingSections.AsEnumerable()));
            // Arrange - filter sections using time range
            var earliestTimeSpan = new TimeSpan(09, 00, 00);
            var latestTimeSpan = new TimeSpan(12, 00, 00);
            var criteria = new SectionSearchCriteria() { EarliestTime = (int)earliestTimeSpan.TotalMinutes, LatestTime = (int)latestTimeSpan.TotalMinutes };
            // Act - call course search
            SectionPage sectionPage = await courseService.SectionSearchAsync(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(sectionPage.CurrentPageItems.Select(c => c.Id).Contains("999-SEC-WITH-PRIM-MTNGS"));
            Assert.IsTrue(sectionPage.CurrentPageItems.Select(c => c.Id).Contains("997-SEC-WITH-PRIM-MTNGS"));


            // Verify that the sections included take place during the specified time span
            // Get each section found for this course
            var courseSections = (from sectionId in sectionPage.CurrentPageItems.Select(c => c.Id)
                                  join sec in primaryMeetingSections
                                  on sectionId equals sec.Id into joinSections
                                  from section in joinSections
                                  select section).ToList();
            var timeCollection = (from sec in courseSections
                                  from mtg in sec.PrimarySectionMeetings
                                  select new { mtg.StartTime, mtg.EndTime, sec.Id });

            foreach (var mtgTime in timeCollection)
            {
                // If no start/end time given, don't try to compare against the criteria earliest/latest time
                if (mtgTime.StartTime != null)
                {
                    DateTimeOffset startTime = mtgTime.StartTime.GetValueOrDefault();
                    Assert.IsTrue(startTime.DateTime.TimeOfDay.TotalMinutes >= criteria.EarliestTime);
                }
                if (mtgTime.EndTime != null)
                {
                    DateTimeOffset endTime = mtgTime.EndTime.GetValueOrDefault();
                    Assert.IsTrue(endTime.DateTime.TimeOfDay.TotalMinutes <= criteria.LatestTime);
                }
            }
        }

      
        [TestMethod]
        public async Task SectionSearchAsync_StartAtTimeEndsByTimeFilter_WithOnlyPrimarySectionMeetings()
        {

            //update few sections to have PrimarySectionMeetings
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(primaryMeetingSections.AsEnumerable()));
            // Arrange - filter sections using time range

            var criteria = new SectionSearchCriteria() { StartsAtTime = "09:00 AM", EndsByTime = "12:00 PM" };
            // Act - call course search
            var coursePage = await courseService.SectionSearchAsync(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("999-SEC-WITH-PRIM-MTNGS"));
            Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("997-SEC-WITH-PRIM-MTNGS"));
            Assert.AreEqual(2, coursePage.CurrentPageItems.Count());


        }

        [TestMethod]
        public async Task SectionSearchAsync_WhenTODAndStartAtAndEndAtTimesAreProvided()
        {

            //update few sections to have PrimarySectionMeetings
            //primary section meeting times are:
            /*
             * Section- 999-SEC with 2 meeting times - 9:10am- 10am and  9:10 to 9:50am
             * section - 998-SEC with 1pm -1:50pm
             * section -997-SEC with 11:00am - 11:50am
             */
            List<Ellucian.Colleague.Domain.Student.Entities.Section> primaryMeetingSections = (await (new TestSectionRepository().BuildSectionsWithPrimarySectionMeetingsAsync())).ToList();
            List<Ellucian.Colleague.Domain.Student.Entities.Section> newList = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
            //modifying a section from the collection to have start time of 8
            Ellucian.Colleague.Domain.Student.Entities.Section sectionStartAt8am = allSections.ToList()[0];
            //set meeting times on section
            string guid = Guid.NewGuid().ToString();
            Ellucian.Colleague.Domain.Student.Entities.SectionMeeting mp3 = new Ellucian.Colleague.Domain.Student.Entities.SectionMeeting("something new", sectionStartAt8am.Id, "LEC", null, null, "W")
            {
                Guid = guid,
                StartTime = new DateTimeOffset(new DateTime(2012, 8, 9, 08, 00, 00)),
                EndTime = new DateTimeOffset(new DateTime(2012, 8, 9, 10, 50, 00)),
                Days = new List<DayOfWeek>() { DayOfWeek.Tuesday, DayOfWeek.Thursday }
            };


            sectionStartAt8am.UpdatePrimarySectionMeetings(new List<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting>() { mp3 });
            sectionStartAt8am.FirstMeetingDate = sectionStartAt8am.StartDate.AddDays(15);
            sectionStartAt8am.LastMeetingDate = sectionStartAt8am.EndDate;
            newList.Add(sectionStartAt8am); //no meetings
            newList.Add(allSections.ToList()[1]); //no meetings
            newList.Add(allSections.ToList()[2]);//no meetings
            newList.Add(allSections.ToList()[3]); // 9-9:50 am
            newList.Add(allSections.ToList()[4]);//1am - 1:50am 
            newList.Add(allSections.ToList()[5]); //have 2 meetings- 11-11:50 am  and second with start time & end time  null
            newList.AddRange(primaryMeetingSections);

            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(newList.AsEnumerable()));
            var earliestTimeSpan = new TimeSpan(08, 00, 00);
            var latestTimeSpan = new TimeSpan(12, 00, 00);
            var criteria = new SectionSearchCriteria() { EarliestTime = (int)earliestTimeSpan.TotalMinutes, LatestTime = (int)latestTimeSpan.TotalMinutes };

            criteria.StartsAtTime = "09:00 AM";
            criteria.EndsByTime = "02:00 PM";
            //sections retrieved will be from 8am to 2pm
            // Act - call course search
            var coursePage = await courseService.SectionSearchAsync(criteria, Int16.MaxValue, 1);
            Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("4"));
            Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("6"));
            Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("1"));
            Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("999-SEC-WITH-PRIM-MTNGS"));
            Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("997-SEC-WITH-PRIM-MTNGS"));
            Assert.IsTrue(coursePage.CurrentPageItems.Select(c => c.Id).Contains("998-SEC-WITH-PRIM-MTNGS"));
            Assert.AreEqual(6, coursePage.CurrentPageItems.Count());

        }
    }

   
    [TestClass]
    public class SectionsSearchSorting : Search3
    {
        List<Ellucian.Colleague.Domain.Student.Entities.Section> sectionsEntity = new List<Ellucian.Colleague.Domain.Student.Entities.Section>();
        [TestInitialize]
        public async void Initialize()
        {

            base.Initialize();
            //mock adapter
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionAdapter);
            //mock adapter
            var sectionMeetingAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Dtos.Student.SectionMeeting2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Dtos.Student.SectionMeeting2>()).Returns(sectionMeetingAdapter);

            //mock adapter
            var sectionRequisiteAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Dtos.Student.SectionRequisite>()).Returns(sectionRequisiteAdapter);
            SectionEntity section1 = new SectionEntity("100", "84", "01", DateTime.Today, 0, null, "spanish 300", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MDLL") }, new List<string>() { "300" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Ellucian.Colleague.Domain.Student.Entities.SectionStatus.Active, "N", DateTime.Today) });
            SectionEntity section2 = new SectionEntity("101", "110", "01", DateTime.Today, 0, null, "biology 100", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("BIOL") }, new List<string>() { "100" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Ellucian.Colleague.Domain.Student.Entities.SectionStatus.Active, "N", DateTime.Today) });
            SectionEntity section3 = new SectionEntity("102", "333", "01", DateTime.Today, 0, null, "math 152", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") }, new List<string>() { "100" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Ellucian.Colleague.Domain.Student.Entities.SectionStatus.Active, "N", DateTime.Today) });
            SectionEntity section4 = new SectionEntity("103", "155", "01", DateTime.Today, 0, null, "poli 100", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("POLI") }, new List<string>() { "100" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Ellucian.Colleague.Domain.Student.Entities.SectionStatus.Active, "N", DateTime.Today) });
            SectionEntity section5 = new SectionEntity("104", "139", "01", DateTime.Today, 0, null, "history 100", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("HIST") }, new List<string>() { "100" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Ellucian.Colleague.Domain.Student.Entities.SectionStatus.Active, "N", DateTime.Today) });
            SectionEntity section6 = new SectionEntity("105", "156", "01", DateTime.Today, 0, null, "psycology 100", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("PSYC") }, new List<string>() { "100" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Ellucian.Colleague.Domain.Student.Entities.SectionStatus.Active, "N", DateTime.Today) });
            SectionEntity section7 = new SectionEntity("106", "187", "01", DateTime.Today, 0, null, "engl 102", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("ENGL") }, new List<string>() { "100" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Ellucian.Colleague.Domain.Student.Entities.SectionStatus.Active, "N", DateTime.Today) });
            SectionEntity section8 = new SectionEntity("107", "139", "02", DateTime.Today, 0, null, "history 100", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("HIST") }, new List<string>() { "100" }, "UG", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Ellucian.Colleague.Domain.Student.Entities.SectionStatus.Active, "N", DateTime.Today) });

            section1.NumberOnWaitlist = 5;
            section2.NumberOnWaitlist = 0;
            section3.NumberOnWaitlist = 3;
            section4.NumberOnWaitlist = null;
            section5.NumberOnWaitlist = 2;
            section6.NumberOnWaitlist = 0;
            section7.NumberOnWaitlist = 0;
            section8.NumberOnWaitlist = 1;


            section1.SectionCapacity = 20;
            section2.SectionCapacity = 20;
            section3.SectionCapacity = 13;
            section4.SectionCapacity = 15;
            section5.SectionCapacity = 0;
            section6.SectionCapacity = 22;
            section7.SectionCapacity = 30;
            section8.SectionCapacity = null;


            section1.Location = "NW";
            section2.Location = "MAIN";
            section3.Location = "DT";
            section4.Location = "SC";
            section5.Location = null;
            section6.Location = string.Empty;
            section7.Location = "NW";
            section8.Location = "DT";


            section1.CourseName = "SPAN-300";
            section2.CourseName = "BIOL-100";
            section3.CourseName = "MATH-152";
            section4.CourseName = "POLI-100";
            section5.CourseName = "HIST-100";
            section6.CourseName = "PSYC-100";
            section7.CourseName = "ENGL-100";
            section8.CourseName = "HIST-100";


            sectionsEntity.Add(section1);
            sectionsEntity.Add(section2);
            sectionsEntity.Add(section3);
            sectionsEntity.Add(section4);
            sectionsEntity.Add(section5);
            sectionsEntity.Add(section6);
            sectionsEntity.Add(section7);
            sectionsEntity.Add(section8);


            sectionRepoMock.Setup(repo => repo.GetRegistrationSectionsAsync(regTerms)).Returns(Task.FromResult(sectionsEntity.AsEnumerable()));
            courseService = new CourseService(adapterRegistryMock.Object, courseRepoMock.Object, referenceRepo,
           studentReferenceRepo, requirementsRepo, sectionRepoMock.Object, termRepoMock.Object,
           ruleRepoMock.Object, configurationRepo, baseConfigurationRepository, userFactory, roleRepo, logger);

        }

        [TestMethod]
        public async Task NoSortOrder()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);

            List<Dtos.Student.Section3> sections = sectionPage.CurrentPageItems.ToList();
            //normal sorting is on subject and then on number
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(8, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("BIOL-100", sections[0].CourseName);
            Assert.AreEqual("ENGL-100", sections[1].CourseName);
            Assert.AreEqual("HIST-100-01", sections[2].CourseName + "-" + sections[2].Number);
            Assert.AreEqual("HIST-100-02", sections[3].CourseName + "-" + sections[3].Number);
            Assert.AreEqual("MATH-152", sections[4].CourseName);
            Assert.AreEqual("POLI-100", sections[5].CourseName);
            Assert.AreEqual("PSYC-100", sections[6].CourseName);
            Assert.AreEqual("SPAN-300", sections[7].CourseName);
        }


        [TestMethod]
        public async Task StatusAscendingSortOrder()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.SortOn = CatalogSortType.Status;
            searchcriteria.SortDirection = CatalogSortDirection.Ascending;
            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            List<Dtos.Student.Section3> sections = sectionPage.CurrentPageItems.ToList();
            //status sorting is on waitlisted. if waitlested is 0 then status is open otherwise status is waitlistedd
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(8, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("BIOL-100", sections[0].CourseName);
            Assert.AreEqual("ENGL-100", sections[1].CourseName);
            Assert.AreEqual("POLI-100", sections[2].CourseName);
            Assert.AreEqual("PSYC-100", sections[3].CourseName);
            Assert.AreEqual("HIST-100-01", sections[4].CourseName + "-" + sections[4].Number);
            Assert.AreEqual("HIST-100-02", sections[5].CourseName + "-" + sections[5].Number);
            Assert.AreEqual("MATH-152", sections[6].CourseName);
            Assert.AreEqual("SPAN-300", sections[7].CourseName);
        }

        [TestMethod]
        public async Task StatusDescendingSortOrder()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.SortOn = CatalogSortType.Status;
            searchcriteria.SortDirection = CatalogSortDirection.Descending;
            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            List<Dtos.Student.Section3> sections = sectionPage.CurrentPageItems.ToList();
            //status sorting is on waitlisted. if waitlested is 0 then status is open otherwise status is waitlistedd
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(8, sectionPage.CurrentPageItems.Count());

            Assert.AreEqual("HIST-100-01", sections[0].CourseName + "-" + sections[0].Number);
            Assert.AreEqual("HIST-100-02", sections[1].CourseName + "-" + sections[1].Number);
            Assert.AreEqual("MATH-152", sections[2].CourseName);
            Assert.AreEqual("SPAN-300", sections[3].CourseName);
            Assert.AreEqual("BIOL-100", sections[4].CourseName);
            Assert.AreEqual("ENGL-100", sections[5].CourseName);
            Assert.AreEqual("POLI-100", sections[6].CourseName);
            Assert.AreEqual("PSYC-100", sections[7].CourseName);
        }
        [TestMethod]
        public async Task SeatsAvailableAscendingSortOrder()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.SortOn = CatalogSortType.SeatsAvailable;
            searchcriteria.SortDirection = CatalogSortDirection.Ascending;
            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            List<Dtos.Student.Section3> sections = sectionPage.CurrentPageItems.ToList();
            //status sorting is on seats available
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(8, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("HIST-100-02", sections[0].CourseName + "-" + sections[0].Number);
            Assert.AreEqual("HIST-100-01", sections[1].CourseName + "-" + sections[1].Number);
            Assert.AreEqual("MATH-152", sections[2].CourseName);
            Assert.AreEqual("POLI-100", sections[3].CourseName);
            Assert.AreEqual("BIOL-100", sections[4].CourseName);
            Assert.AreEqual("SPAN-300", sections[5].CourseName);
            Assert.AreEqual("PSYC-100", sections[6].CourseName);
            Assert.AreEqual("ENGL-100", sections[7].CourseName);
        }

        [TestMethod]
        public async Task SeatsAvailableDescendingSortOrder()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.SortOn = CatalogSortType.SeatsAvailable;
            searchcriteria.SortDirection = CatalogSortDirection.Descending;
            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            List<Dtos.Student.Section3> sections = sectionPage.CurrentPageItems.ToList();
            //status sorting is on waitlisted. if waitlested is 0 then status is open otherwise status is waitlistedd
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(8, sectionPage.CurrentPageItems.Count());

            Assert.AreEqual("HIST-100-02", sections[7].CourseName + "-" + sections[7].Number);
            Assert.AreEqual("HIST-100-01", sections[6].CourseName + "-" + sections[6].Number);
            Assert.AreEqual("MATH-152", sections[5].CourseName);
            Assert.AreEqual("POLI-100", sections[4].CourseName);
            Assert.AreEqual("BIOL-100", sections[2].CourseName);
            Assert.AreEqual("SPAN-300", sections[3].CourseName);
            Assert.AreEqual("PSYC-100", sections[1].CourseName);
            Assert.AreEqual("ENGL-100", sections[0].CourseName);
        }

        [TestMethod]
        public async Task LocationAscendingSortOrder()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.SortOn = CatalogSortType.Location;
            searchcriteria.SortDirection = CatalogSortDirection.Ascending;
            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            List<Dtos.Student.Section3> sections = sectionPage.CurrentPageItems.ToList();
            //status sorting is on seats available
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(8, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("HIST-100-01", sections[0].CourseName + "-" + sections[0].Number);
            Assert.AreEqual("PSYC-100", sections[1].CourseName);
            Assert.AreEqual("HIST-100-02", sections[2].CourseName + "-" + sections[2].Number);
            Assert.AreEqual("MATH-152", sections[3].CourseName);
            Assert.AreEqual("BIOL-100", sections[4].CourseName);
            Assert.AreEqual("ENGL-100", sections[5].CourseName);
            Assert.AreEqual("SPAN-300", sections[6].CourseName);
            Assert.AreEqual("POLI-100", sections[7].CourseName);
        }

        [TestMethod]
        public async Task LocationDescendingSortOrder()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.SortOn = CatalogSortType.Location;
            searchcriteria.SortDirection = CatalogSortDirection.Descending;
            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            List<Dtos.Student.Section3> sections = sectionPage.CurrentPageItems.ToList();
            //status sorting is on waitlisted. if waitlested is 0 then status is open otherwise status is waitlistedd
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(8, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("HIST-100-01", sections[7].CourseName + "-" + sections[7].Number);
            Assert.AreEqual("PSYC-100", sections[6].CourseName);
            Assert.AreEqual("HIST-100-02", sections[4].CourseName + "-" + sections[4].Number);
            Assert.AreEqual("MATH-152", sections[5].CourseName);
            Assert.AreEqual("BIOL-100", sections[3].CourseName);
            Assert.AreEqual("ENGL-100", sections[1].CourseName);
            Assert.AreEqual("SPAN-300", sections[2].CourseName);
            Assert.AreEqual("POLI-100", sections[0].CourseName);

        }

        [TestMethod]
        public async Task SectionNameAscendingSortOrder()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.SortOn = CatalogSortType.SectionName;
            searchcriteria.SortDirection = CatalogSortDirection.Ascending;
            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            List<Dtos.Student.Section3> sections = sectionPage.CurrentPageItems.ToList();
            //status sorting is on seats available
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(8, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("BIOL-100", sections[0].CourseName);
            Assert.AreEqual("ENGL-100", sections[1].CourseName);
            Assert.AreEqual("HIST-100-01", sections[2].CourseName + "-" + sections[2].Number);
            Assert.AreEqual("HIST-100-02", sections[3].CourseName + "-" + sections[3].Number);
            Assert.AreEqual("MATH-152", sections[4].CourseName);
            Assert.AreEqual("POLI-100", sections[5].CourseName);
            Assert.AreEqual("PSYC-100", sections[6].CourseName);
            Assert.AreEqual("SPAN-300", sections[7].CourseName);

        }

        [TestMethod]
        public async Task SectionNameDescendingSortOrder()
        {
            SectionSearchCriteria searchcriteria = new SectionSearchCriteria();
            searchcriteria.SortOn = CatalogSortType.SectionName;
            searchcriteria.SortDirection = CatalogSortDirection.Descending;
            // Act
            SectionPage sectionPage = await courseService.SectionSearchAsync(searchcriteria, int.MaxValue, 0);
            List<Dtos.Student.Section3> sections = sectionPage.CurrentPageItems.ToList();
            //status sorting is on waitlisted. if waitlested is 0 then status is open otherwise status is waitlistedd
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(8, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("SPAN-300", sections[0].CourseName);
            Assert.AreEqual("PSYC-100", sections[1].CourseName);
            Assert.AreEqual("POLI-100", sections[2].CourseName);
            Assert.AreEqual("MATH-152", sections[3].CourseName);
            Assert.AreEqual("HIST-100-02", sections[4].CourseName + "-" + sections[4].Number);
            Assert.AreEqual("HIST-100-01", sections[5].CourseName + "-" + sections[5].Number);
            Assert.AreEqual("ENGL-100", sections[6].CourseName);
            Assert.AreEqual("BIOL-100", sections[7].CourseName);
        }
    }


    [TestClass]
    public class InstantEnrollmentSearchAsync : Search3
    {
        private Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration ieConfig;

        [TestInitialize]
        public new void Initialize()
        {
            base.Initialize();
            //var section1 = allSections.Where(s => s.Id == "1").First();
            //var section2 = allSections.Where(s => s.Id == "2").First();
            //var section3 = allSections.Where(s => s.Id == "3").First();
            //var sectionCollection = new List<Domain.Student.Entities.Section>() { section1, section2, section3 };

            var demographicFields = new List<DemographicField>()
            {
                null, // Nulls should be handled gracefully
                new DemographicField("FIRST_NAME", "First Name", DemographicFieldRequirement.Required),
                new DemographicField("MIDDLE_NAME","Middle Name",DemographicFieldRequirement.Optional),
                new DemographicField("LAST_NAME","Last Name",DemographicFieldRequirement.Required),
            };

            // Set up section repository response
          //  sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionCollection));

            // Set up student configuration repo response
            var ieConfig = new Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration(Domain.Student.Entities.InstantEnrollment.AddNewStudentProgramBehavior.Any,
                new List<Domain.Student.Entities.InstantEnrollment.AcademicProgramOption>()
                {
                    new Domain.Student.Entities.InstantEnrollment.AcademicProgramOption("CE.SYSTEMASSIGNED", DateTime.Today.Year.ToString())
                },
                "BANK",
                "US",
                true,
                "CEUSER", null, false, demographicFields);
            foreach(var subj in allSubjects)
            {
                ieConfig.AddSubjectCodeToDisplayInCatalog(subj.Code);
            }
            configurationRepoMock.Setup(repo => repo.GetInstantEnrollmentConfigurationAsync()).ReturnsAsync(ieConfig);

            //mock adapter
            var sectionAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Dtos.Student.Section3>()).Returns(sectionAdapter);

            //mock section repo - GetInstantEnrollmentSections
            //139 - HIST course(no location) 122- is  DANC-100 (CD location) course  110- is BIOL-100 course (MAIN location)
            List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() {
                new Domain.Student.Entities.Section("1", "139", "01", DateTime.Today.AddDays(-5), 3m, null, "Sec 1", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("CE") }, new List<string>() {"100" }, "CE", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, Domain.Student.Entities.SectionStatusIntegration.Open, "A", DateTime.Today.AddDays(-3)) }) { TopicCode = "T1" },
                new Domain.Student.Entities.Section("2", "139", "02", DateTime.Today.AddDays(-5), 3m, null, "Sec 2", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("CE") }, new List<string>() {"100" }, "CE", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, Domain.Student.Entities.SectionStatusIntegration.Open, "A", DateTime.Today.AddDays(-3)) }) { Location = "SC" },
                new Domain.Student.Entities.Section("3", "139", "03", DateTime.Today.AddDays(-5), 3m, null, "Sec 3", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("CE") }, new List<string>() {"100" }, "CE", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, Domain.Student.Entities.SectionStatusIntegration.Open, "A", DateTime.Today.AddDays(-3)) }),
                new Domain.Student.Entities.Section("4", "122", "01", DateTime.Today.AddDays(-5), 3m, null, "Sec 1 dance", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("CE") }, new List<string>() {"100" }, "CE", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, Domain.Student.Entities.SectionStatusIntegration.Open, "A", DateTime.Today.AddDays(-3)) }) { Location = "MAIN" },
                new Domain.Student.Entities.Section("5", "110", "01", DateTime.Today.AddDays(-5), 3m, null, "Sec 1 biology", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("CE") }, new List<string>() {"100" }, "CE", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, Domain.Student.Entities.SectionStatusIntegration.Open, "A", DateTime.Today.AddDays(-3)) }),


            };
            sectionRepoMock.Setup(repo => repo.GetInstantEnrollmentSectionsAsync()).Returns(Task.FromResult(sections.AsEnumerable()));
            CourseService.ClearInstantEnrollmentIndex();
        }

        [TestCleanup]
        public new void Cleanup()
        {
            base.Cleanup();
        }

        //when criteria for IE is null
        [TestMethod]
        public async Task CriteriaIsNull()
        {
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(null, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(5, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("1", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[0]).Id);
            Assert.AreEqual("2", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[1]).Id);
            Assert.AreEqual("3", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[2]).Id);
            Assert.AreEqual("4", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[3]).Id);
            Assert.AreEqual("5", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[4]).Id);
            //filters are not empty
            Assert.AreEqual(3, sectionPage.Locations.Count()); // SC, null, MAIN

            Assert.AreEqual(null, sectionPage.Locations.ToList()[0].Value);
            Assert.AreEqual(3, sectionPage.Locations.ToList()[0].Count);
            Assert.AreEqual(false, sectionPage.Locations.ToList()[0].Selected);

            Assert.AreEqual("SC", sectionPage.Locations.ToList()[1].Value);
            Assert.AreEqual(1, sectionPage.Locations.ToList()[1].Count);
            Assert.AreEqual(false, sectionPage.Locations.ToList()[1].Selected);

            Assert.AreEqual("MAIN", sectionPage.Locations.ToList()[2].Value);
            Assert.AreEqual(1, sectionPage.Locations.ToList()[2].Count);
            Assert.AreEqual(false, sectionPage.Locations.ToList()[2].Selected);
          
            Assert.AreEqual(0, sectionPage.Faculty.Count());
            Assert.AreEqual(0, sectionPage.DaysOfWeek.Count());
            Assert.AreEqual(2, sectionPage.TopicCodes.Count()); // T1, null
            Assert.AreEqual(1, sectionPage.Terms.Count());
            Assert.AreEqual(1, sectionPage.OnlineCategories.Count());
            Assert.AreEqual(5, sectionPage.OpenSections.Count);
            Assert.AreEqual(5, sectionPage.OpenAndWaitlistSections.Count);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
        }
        //when criteria is null and repository also have null sections for IE
        [TestMethod]
        public async Task CriteriaIsNull_And_IESectionsAlsoNull()
        {
            IEnumerable<Domain.Student.Entities.Section> sections = null;
            sectionRepoMock.Setup(repo => repo.GetInstantEnrollmentSectionsAsync()).Returns(Task.FromResult(sections.AsEnumerable()));

            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(null, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
            //filters are empty
            Assert.AreEqual(0, sectionPage.Locations.Count());
            Assert.AreEqual(0, sectionPage.Faculty.Count());
            Assert.AreEqual(0, sectionPage.DaysOfWeek.Count());
            Assert.AreEqual(0, sectionPage.TopicCodes.Count());
            Assert.AreEqual(0, sectionPage.Terms.Count());
            Assert.AreEqual(0, sectionPage.OnlineCategories.Count());
            Assert.IsNull(sectionPage.OpenSections.Value);
            Assert.IsNull(sectionPage.OpenAndWaitlistSections.Value);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
            //These filters are null because IE doens' need those filters
            Assert.IsNull(sectionPage.Subjects);
            Assert.IsNull(sectionPage.AcademicLevels);
            Assert.IsNull(sectionPage.CourseLevels);
            Assert.IsNull(sectionPage.CourseTypes);
        }
        //when criteria doesn't have any search criteria and is not null
        [TestMethod]
        public async Task CriteriaIsNotNull_WithNoCriteria()
        {
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(5, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("1", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[0]).Id);
            Assert.AreEqual("2", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[1]).Id);
            Assert.AreEqual("3", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[2]).Id);
            Assert.AreEqual("4", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[3]).Id);
            Assert.AreEqual("5", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[4]).Id);
            //filters are not empty
            Assert.AreEqual(3, sectionPage.Locations.Count()); // SC, null, MAIN
            Assert.AreEqual(0, sectionPage.Faculty.Count());
            Assert.AreEqual(0, sectionPage.DaysOfWeek.Count());
            Assert.AreEqual(2, sectionPage.TopicCodes.Count()); // T1, null
            Assert.AreEqual(1, sectionPage.Terms.Count());//2012/FA
            Assert.AreEqual(1, sectionPage.OnlineCategories.Count());
            Assert.AreEqual(5, sectionPage.OpenSections.Count);
            Assert.AreEqual(5, sectionPage.OpenAndWaitlistSections.Count);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
        }
        //when criteria provided but repository have no sections for IE
        [TestMethod]
        public async Task CriteriaIsNotNull_WithNoCriteria_IESectionsAreNull()
        {
            IEnumerable<Domain.Student.Entities.Section> sections = null;
            sectionRepoMock.Setup(repo => repo.GetInstantEnrollmentSectionsAsync()).Returns(Task.FromResult(sections));

            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
            //filters are empty
            Assert.AreEqual(0, sectionPage.Locations.Count());
            Assert.AreEqual(0, sectionPage.Faculty.Count());
            Assert.AreEqual(0, sectionPage.DaysOfWeek.Count());
            Assert.AreEqual(0, sectionPage.TopicCodes.Count());
            Assert.AreEqual(0, sectionPage.Terms.Count());
            Assert.AreEqual(0, sectionPage.OnlineCategories.Count());
            Assert.IsNull(sectionPage.OpenSections.Value);
            Assert.IsNull(sectionPage.OpenAndWaitlistSections.Value);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
            //These filters are null because IE doens' need those filters
            Assert.IsNull(sectionPage.Subjects);
            Assert.IsNull(sectionPage.AcademicLevels);
            Assert.IsNull(sectionPage.CourseLevels);
            Assert.IsNull(sectionPage.CourseTypes);
        }
        //when sections from repository have sections but keyword doesn't give any sections
        [TestMethod]
        public async Task CriteriaHasKeyword_NoSectionsforTheKW_IEHaveSections()
        {
             InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.Keyword = "xyz";
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);

            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
            //filters are  empty
            Assert.AreEqual(0,sectionPage.Locations.Count());
            Assert.AreEqual(0, sectionPage.Faculty.Count());
            Assert.AreEqual(0, sectionPage.DaysOfWeek.Count());
            Assert.AreEqual(0, sectionPage.TopicCodes.Count());
            Assert.AreEqual(0, sectionPage.Terms.Count());
            Assert.AreEqual(0, sectionPage.OnlineCategories.Count());
            Assert.IsNull(sectionPage.OpenSections.Value);
            Assert.IsNull(sectionPage.OpenAndWaitlistSections.Value);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
            //These filters are null because IE doens' need those filters
            Assert.IsNull(sectionPage.Subjects);
            Assert.IsNull(sectionPage.AcademicLevels);
            Assert.IsNull(sectionPage.CourseLevels);
            Assert.IsNull(sectionPage.CourseTypes);
        }
        //when sections from repo have sections that matched few sections with keyword 
        [TestMethod]
        public async Task CriteriaHasKeyword_MoreSectionsforTheKW_IEHaveSections()
        {
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.Keyword = "Sec";
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(5, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("1", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[0]).Id);
            Assert.AreEqual("2", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[1]).Id);
            Assert.AreEqual("3", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[2]).Id);
            Assert.AreEqual("5", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[3]).Id);
            Assert.AreEqual("4", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[4]).Id);
            //filters are not empty
            Assert.AreEqual(3, sectionPage.Locations.Count()); // SC, null, MAIN
            Assert.AreEqual(0, sectionPage.Faculty.Count());
            Assert.AreEqual(0, sectionPage.DaysOfWeek.Count());
            Assert.AreEqual(2, sectionPage.TopicCodes.Count()); // T1, null
            Assert.AreEqual(1, sectionPage.Terms.Count());//2012/FA
            Assert.AreEqual(1, sectionPage.OnlineCategories.Count());
            Assert.AreEqual(5, sectionPage.OpenSections.Count);
            Assert.AreEqual(5, sectionPage.OpenAndWaitlistSections.Count);
            Assert.AreEqual(0, sectionPage.EarliestTime);
            Assert.AreEqual(0, sectionPage.LatestTime);
        }
        //when sections from repo have more sections than  matched with sections with kword 
        [TestMethod]
        public async Task SectionsforTheKW_IEHaveMoreSections()
        {
            List<Domain.Student.Entities.Section> sections = new List<Domain.Student.Entities.Section>() {
                new Domain.Student.Entities.Section("1", "139", "01", DateTime.Today.AddDays(-5), 3m, null, "Sec 1", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("CE") }, new List<string>() {"100" }, "CE", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, Domain.Student.Entities.SectionStatusIntegration.Open, "A", DateTime.Today.AddDays(-3)) }),
                new Domain.Student.Entities.Section("2", "139", "02", DateTime.Today.AddDays(-5), 3m, null, "Sec 2", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("CE") }, new List<string>() {"100" }, "CE", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, Domain.Student.Entities.SectionStatusIntegration.Open, "A", DateTime.Today.AddDays(-3)) }),
                new Domain.Student.Entities.Section("3", "139", "03", DateTime.Today.AddDays(-5), 3m, null, "Sec 3", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("CE") }, new List<string>() {"100" }, "CE", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, Domain.Student.Entities.SectionStatusIntegration.Open, "A", DateTime.Today.AddDays(-3)) }),
                new Domain.Student.Entities.Section("new-sec-ie-1", "139", "04", DateTime.Today.AddDays(-5), 3m, null, "Crocheting", "IN", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("CE") }, new List<string>() {"100" }, "CE", new List<Domain.Student.Entities.SectionStatusItem>() { new Domain.Student.Entities.SectionStatusItem(Domain.Student.Entities.SectionStatus.Active, Domain.Student.Entities.SectionStatusIntegration.Open, "A", DateTime.Today.AddDays(-3)) }),
            };
            sectionRepoMock.Setup(repo => repo.GetInstantEnrollmentSectionsAsync()).Returns(Task.FromResult(sections.AsEnumerable()));

            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.Keyword = "crocheting";
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(1, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("new-sec-ie-1", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[0]).Id);
        }
        //when sections from repo have sections that doesn't match with kword
        [TestMethod]
        public async Task SectionsforTheKW_NotMatchWithIESections()
        {
            // Set up section repository response
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.Keyword = "crocheting";
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
        }
        //when sections from repo have sections that do not match with section ids in criteria
        [TestMethod]
        public async Task SectionIdsIncriteria_NotInIESections()
        {
            var section1 = allSections.Where(s => s.Id == "1").First();
            var section2 = allSections.Where(s => s.Id == "2").First();
            var section3 = allSections.Where(s => s.Id == "3").First();

            var sectionCollection = new List<Domain.Student.Entities.Section>() { section1, section2, section3 };
            // Set up section repository response
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "1", "2", "3" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionCollection));

            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.SectionIds = new List<string>() { "new-sec-ie-1" };
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());

        }
        //when sections from repo have sections that matched with section ids in criteria
        [TestMethod]
        public async Task SectionIdsInCriteria_AllInIESections()
        {
            var section1 = allSections.Where(s => s.Id == "1").First();
            var section2 = allSections.Where(s => s.Id == "2").First();
            var sectionCollection = new List<Domain.Student.Entities.Section>() { section1, section2 };
            // Set up section repository response
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "1", "2" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionCollection));
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.SectionIds = new List<string>() { "1", "2" };
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            Assert.AreEqual(2, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("1", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[0]).Id);
            Assert.AreEqual("2", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[1]).Id);

        }
        //when sections from repo have few sections that matches with section ids in criteria and few do not
        [TestMethod]
        public async Task SectionIdsInCriteria_FewInIESections()
        {
            var section1 = allSections.Where(s => s.Id == "2").First();
            var sectionCollection = new List<Domain.Student.Entities.Section>() { section1 };
            // Set up section repository response
            sectionRepoMock.Setup(repo => repo.GetCachedSectionsAsync(new List<string>() { "2" }, false)).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.Section>>(sectionCollection));
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.SectionIds = new List<string>() { "2", "new-sec-ie-1" };
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            Assert.AreEqual(1, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("2", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[0]).Id);

        }
        //Test case on filters
        //location filter on section and courses but only sections returned
        //when a location filter is on section as well as course but course and section are not related
        [TestMethod]
        public async Task Location_InSection_InCourse_OnlySectionAppears()
        {
            //data setup - criteira = MAIN. this location is on course 110 BIOL-100 and is on section 4 for DANC-100.  
            //oNLY danc-100 SECTION appears even though course is valid IE course.
            //BIOL-100 COURSE have section 5 which does not have any location
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.Locations = new List<string>() { "MAIN" };
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(1, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("4", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[0]).Id);
            Assert.AreEqual(1, sectionPage.Locations.Count());
            Assert.AreEqual("MAIN", sectionPage.Locations.ToList()[0].Value);
            Assert.AreEqual(true, sectionPage.Locations.ToList()[0].Selected);
            Assert.AreEqual(1, sectionPage.Locations.ToList()[0].Count);
        }

        //when a course have location but section does not
        [TestMethod]
        public async Task Location_InCourse_NotInSection()
        {
            //data setup DANC-100 course have CD location but there the correspoding section does not have CD location as well as no other 
            //section have CD location
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.Locations = new List<string>() { "CD" };
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual(0, sectionPage.Locations.Count());

        }

        //topic codes on section and courses but only sections returned
        [TestMethod]
        public async Task TopicCode_InSection_InCourse_OnlySectionAppears()
        {
            //data setup - criteira = MAIN. this location is on course 110 BIOL-100 and is on section 4 for DANC-100.  
            //oNLY danc-100 SECTION appears even though course is valid IE course.
            //BIOL-100 COURSE have section 5 which does not have any location
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.TopicCodes = new List<string>() { "T1" };
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(1, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual("1", ((Dtos.Student.Section3)sectionPage.CurrentPageItems.ToList()[0]).Id);
            Assert.AreEqual(1, sectionPage.TopicCodes.Count());
            Assert.AreEqual("T1", sectionPage.TopicCodes.ToList()[0].Value);
            Assert.AreEqual(true, sectionPage.TopicCodes.ToList()[0].Selected);
            Assert.AreEqual(1, sectionPage.TopicCodes.ToList()[0].Count);
        }

        //when a course have location but section does not
        [TestMethod]
        public async Task TopicCode_InCourse_NotInSection()
        {
            //data setup DANC-100 course have CD location but there the correspoding section does not have CD location as well as no other 
            //section have CD location
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.TopicCodes = new List<string>() { "T2","T3" };
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
            Assert.AreEqual(0, sectionPage.TopicCodes.Count());

        }
        //SEARCH on coursid when it is not on valid sections
        [TestMethod]
        public async Task CourseId_Valid_ButNoInNijaSections()
        {
            //data setup- DANC-102 with id 7438 is not in any of the sections
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.CourseIds = new List<string>() { "7438" };
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(0, sectionPage.CurrentPageItems.Count());
        }
        //search on coursids where it is vlid course but there is no section with that courseid
        [TestMethod]
        public async Task CourseId_Valid_AlsoInNijaSections()
        {
            //data setup- HIST-100 with id 139 is in 3 valid sections
            InstantEnrollmentCourseSearchCriteria searchcriteria = new InstantEnrollmentCourseSearchCriteria();
            searchcriteria.CourseIds = new List<string>() { "139" };
            SectionPage sectionPage = await courseService.InstantEnrollmentSearchAsync(searchcriteria, int.MaxValue, 0);
            //should get all the sections that are returned from IE repository
            Assert.IsNotNull(sectionPage);
            Assert.AreEqual(3, sectionPage.CurrentPageItems.Count());
        }

    }
}
