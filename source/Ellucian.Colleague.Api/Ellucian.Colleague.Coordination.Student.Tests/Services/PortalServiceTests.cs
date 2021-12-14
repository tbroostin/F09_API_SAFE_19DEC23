// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.Portal;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class PortalServiceTests
    {

        public class StudentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "administrator",
                        PersonId = "0000001",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "admin",
                        Roles = new List<string>() { "portalRole" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public IAdapterRegistry adapterRegistry;
        public Mock<IRoleRepository> roleRepositoryMock;
        public IRoleRepository roleRepository;
        public Mock<ILogger> loggerMock;
        public ILogger logger;
        public ICurrentUserFactory currentUserFactory;
        private Mock<IPortalRepository> portalRepositoryMock;
        private IPortalRepository portalRepository;

        private PortalService service;
        Domain.Student.Entities.Portal.PortalDeletedCoursesResult deletedCoursesEntity;
        Domain.Student.Entities.Portal.PortalUpdatedSectionsResult updatedSectionsResultEntity;
        Domain.Student.Entities.Portal.PortalDeletedSectionsResult deletedSectionsEntity;
        Domain.Student.Entities.Portal.PortalUpdatedCoursesResult updatedCoursesResultEntity;


        [TestInitialize]
        public void Initialize()
        {
            //mock
            loggerMock = new Mock<ILogger>();
            loggerMock.Setup(lgr => lgr.IsDebugEnabled).Returns(true);
            logger = loggerMock.Object;

            currentUserFactory = new StudentUserFactory();
            //adaptors
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            var portalDeletedCoursesDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Portal.PortalDeletedCoursesResult, Dtos.Student.Portal.PortalDeletedCoursesResult>(adapterRegistry, logger);
            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.Student.Entities.Portal.PortalDeletedCoursesResult, Dtos.Student.Portal.PortalDeletedCoursesResult>()).Returns(portalDeletedCoursesDtoAdapter);

            var portalUpdatedSectionsResultDtoAdapter = new PortalUpdatedSectionsResultEntityToDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.Student.Entities.Portal.PortalUpdatedSectionsResult, Dtos.Student.Portal.PortalUpdatedSectionsResult>()).Returns(portalUpdatedSectionsResultDtoAdapter);

            var portalDeletedSectionsDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Portal.PortalDeletedSectionsResult, Dtos.Student.Portal.PortalDeletedSectionsResult>(adapterRegistry, logger);
            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.Student.Entities.Portal.PortalDeletedSectionsResult, Dtos.Student.Portal.PortalDeletedSectionsResult>()).Returns(portalDeletedSectionsDtoAdapter);

            var portalUpdatedCoursesResultDtoAdapter = new PortalUpdatedCoursesResultEntityToDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.Student.Entities.Portal.PortalUpdatedCoursesResult, Dtos.Student.Portal.PortalUpdatedCoursesResult>()).Returns(portalUpdatedCoursesResultDtoAdapter);

            //repos
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepository = roleRepositoryMock.Object;
            var portalPermission = new Domain.Entities.Permission(StudentPermissionCodes.PortalCatalogAdmin);
            var portalRole = new Role(1, currentUserFactory.CurrentUser.Roles.FirstOrDefault());
            portalRole.AddPermission(portalPermission);
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { portalRole });

            deletedCoursesEntity = new Domain.Student.Entities.Portal.PortalDeletedCoursesResult(2, new List<string>() { "course-1", "course-2" });
            updatedSectionsResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedSectionsResult("MDY", 0, new List<Domain.Student.Entities.Portal.PortalSection>());
            deletedSectionsEntity = new Domain.Student.Entities.Portal.PortalDeletedSectionsResult(2, new List<string>() { "section-1", "section-2" });
            updatedCoursesResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedCoursesResult(0, new List<Domain.Student.Entities.Portal.PortalCourse>());

            portalRepositoryMock = new Mock<IPortalRepository>();
            portalRepositoryMock.Setup(repo => repo.GetCoursesForDeletionAsync()).ReturnsAsync(deletedCoursesEntity);
            portalRepositoryMock.Setup(repo => repo.GetSectionsForUpdateAsync()).ReturnsAsync(updatedSectionsResultEntity);
            portalRepositoryMock.Setup(repo => repo.GetSectionsForDeletionAsync()).ReturnsAsync(deletedSectionsEntity);
            portalRepositoryMock.Setup(repo => repo.GetCoursesForUpdateAsync()).ReturnsAsync(updatedCoursesResultEntity);
            portalRepository = portalRepositoryMock.Object;

            service = new PortalService(adapterRegistry, portalRepository, currentUserFactory, roleRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            currentUserFactory = null;
        }

        [TestClass]
        public class PortalService_GetCoursesForDeletionAsync : PortalServiceTests
        {
            [TestInitialize]
            public void PortalService_GetCoursesForDeletionAsync_Initialize()
            {

            }
            //When repo returns proper result
            [TestMethod]
            public async Task PortalService_Repo_Returns_Proper_Courses()
            {
                PortalDeletedCoursesResult deletedCoursesDto = await service.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesDto);
                Assert.AreEqual(2, deletedCoursesDto.TotalCourses);
                Assert.AreEqual(2, deletedCoursesDto.CourseIds.Count);
                Assert.AreEqual("course-1", deletedCoursesDto.CourseIds[0]);
                Assert.AreEqual("course-2", deletedCoursesDto.CourseIds[1]);
            }
            //when repo returns null result
            [TestMethod]
            public async Task PortalService_Repo_Returns_Null()
            {
                portalRepositoryMock.Setup(repo => repo.GetCoursesForDeletionAsync()).ReturnsAsync(() => null);
                PortalDeletedCoursesResult deletedCoursesDto = await service.GetCoursesForDeletionAsync();
                Assert.IsNull(deletedCoursesDto);
                loggerMock.Verify(l => l.Warn("Portal call to retrieve courses for deletion from repository returned null entity"));
            }
            //when repo throws repo exception
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalService_Repo_throws_RepoException()
            {
                portalRepositoryMock.Setup(repo => repo.GetCoursesForDeletionAsync()).Throws(new RepositoryException());
                PortalDeletedCoursesResult deletedCoursesDto = await service.GetCoursesForDeletionAsync();
            }
            //when repo throws  exception
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PortalService_Repo_throws_Exception()
            {
                portalRepositoryMock.Setup(repo => repo.GetCoursesForDeletionAsync()).Throws(new Exception());
                PortalDeletedCoursesResult deletedCoursesDto = await service.GetCoursesForDeletionAsync();
            }
            //when repo returns 0 total courses
            [TestMethod]
            public async Task PortalService_With_Zero_TotalCourses()
            {
                deletedCoursesEntity = new Domain.Student.Entities.Portal.PortalDeletedCoursesResult(0, new List<string>() { "course-1", "course-2" });
                portalRepositoryMock.Setup(repo => repo.GetCoursesForDeletionAsync()).ReturnsAsync(deletedCoursesEntity);
                PortalDeletedCoursesResult deletedCoursesDto = await service.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesDto);
                Assert.AreEqual(0, deletedCoursesDto.TotalCourses);
                Assert.AreEqual(2, deletedCoursesDto.CourseIds.Count);
                Assert.AreEqual("course-1", deletedCoursesDto.CourseIds[0]);
                Assert.AreEqual("course-2", deletedCoursesDto.CourseIds[1]);
            }
            //when repo returns empty course ids
            [TestMethod]
            public async Task PortalService_Empty_Courses()
            {
                deletedCoursesEntity = new Domain.Student.Entities.Portal.PortalDeletedCoursesResult(2, new List<string>());
                portalRepositoryMock.Setup(repo => repo.GetCoursesForDeletionAsync()).ReturnsAsync(deletedCoursesEntity);
                PortalDeletedCoursesResult deletedCoursesDto = await service.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesDto);
                Assert.AreEqual(2, deletedCoursesDto.TotalCourses);
                Assert.AreEqual(0, deletedCoursesDto.CourseIds.Count);

            }
            //when repo returns null course ids
            [TestMethod]
            public async Task PortalService_Null_Courses()
            {
                deletedCoursesEntity = new Domain.Student.Entities.Portal.PortalDeletedCoursesResult(-1, null);
                portalRepositoryMock.Setup(repo => repo.GetCoursesForDeletionAsync()).ReturnsAsync(deletedCoursesEntity);
                PortalDeletedCoursesResult deletedCoursesDto = await service.GetCoursesForDeletionAsync();
                Assert.IsNotNull(deletedCoursesDto);
                Assert.AreEqual(-1, deletedCoursesDto.TotalCourses);
                Assert.IsNotNull(deletedCoursesDto.CourseIds);
                Assert.AreEqual(0, deletedCoursesDto.CourseIds.Count);

            }
            //when user does not have permissions
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PortalService_Wrong_permssions()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Role>());
                //  service = new PortalService(adapterRegistry, portalRepository, currentUserFactory, roleRepository, logger);
                PortalDeletedCoursesResult deletedCoursesDto = await service.GetCoursesForDeletionAsync();
            }
        }

        [TestClass]
        public class PortalService_GetSectionsForUpdateAsync : PortalServiceTests
        {
            [TestInitialize]
            public void PortalService_GetSectionsForUpdateAsync_Initialize()
            {
            }

            //When repo returns valid result
            [TestMethod]
            public async Task PortalService_Repo_Returns_Valid_Updated_Sections_Result()
            {
                updatedSectionsResultEntity = SetPortalUpupdatedSectionsResultEntityData();
                portalRepositoryMock.Setup(repo => repo.GetSectionsForUpdateAsync()).ReturnsAsync(updatedSectionsResultEntity);
                PortalUpdatedSectionsResult updatedSectionsResultDto = await service.GetSectionsForUpdateAsync();

                Assert.IsNotNull(updatedSectionsResultDto);
                Assert.AreEqual(expected: updatedSectionsResultEntity.ShortDateFormat, actual: updatedSectionsResultDto.ShortDateFormat);
                Assert.AreEqual(expected: updatedSectionsResultEntity.TotalSections, actual: updatedSectionsResultDto.TotalSections);


                Domain.Student.Entities.Portal.PortalSection entitySection = null;
                PortalSection dtoSection = null;

                for (var i = 0; i < updatedSectionsResultEntity.Sections.Count; i++)
                {
                    entitySection = (updatedSectionsResultEntity.Sections[i]);
                    dtoSection = (updatedSectionsResultDto.Sections[i]);

                    Assert.AreEqual(entitySection.SectionId, dtoSection.SectionId);
                    Assert.AreEqual(entitySection.ShortTitle, dtoSection.ShortTitle);
                    Assert.AreEqual(entitySection.Description, dtoSection.Description);
                    Assert.AreEqual(entitySection.Location, dtoSection.Location);
                    Assert.AreEqual(entitySection.Term, dtoSection.Term);
                    Assert.AreEqual(entitySection.StartDate, dtoSection.StartDate);
                    Assert.AreEqual(entitySection.EndDate, dtoSection.EndDate);
                    Assert.AreEqual(entitySection.Capacity, dtoSection.Capacity);
                    Assert.AreEqual(entitySection.Subject, dtoSection.Subject);
                    Assert.AreEqual(entitySection.CourseNumber, dtoSection.CourseNumber);
                    Assert.AreEqual(entitySection.SectionNumber, dtoSection.SectionNumber);
                    Assert.AreEqual(entitySection.AcademicLevel, dtoSection.AcademicLevel);
                    Assert.AreEqual(entitySection.Synonym, dtoSection.Synonym);
                    Assert.AreEqual(entitySection.MinimumCredits, dtoSection.MinimumCredits);
                    Assert.AreEqual(entitySection.MaximumCredits, dtoSection.MaximumCredits);
                    Assert.AreEqual(entitySection.SectionName, dtoSection.SectionName);
                    Assert.AreEqual(entitySection.CourseId, dtoSection.CourseId);
                    Assert.AreEqual(entitySection.PrerequisiteText, dtoSection.PrerequisiteText);
                    Assert.AreEqual(entitySection.ContinuingEducationUnits, dtoSection.ContinuingEducationUnits);
                    Assert.AreEqual(entitySection.PrintedComments, dtoSection.PrintedComments);
                    Assert.AreEqual(entitySection.TotalBookCost, dtoSection.TotalBookCost);

                    Assert.AreEqual(entitySection.Departments.Count, dtoSection.Departments.Count);
                    for (var j = 0; j < entitySection.Departments.Count; j++)
                    {
                        Assert.AreEqual(entitySection.Departments[j], dtoSection.Departments[j]);
                    }

                    Assert.AreEqual(entitySection.Faculty.Count, dtoSection.Faculty.Count);
                    for (var j = 0; j < entitySection.Faculty.Count; j++)
                    {
                        Assert.AreEqual(entitySection.Faculty[j], dtoSection.Faculty[j]);
                    }

                    Assert.AreEqual(entitySection.CourseTypes.Count, dtoSection.CourseTypes.Count);
                    for (var j = 0; j < entitySection.CourseTypes.Count; j++)
                    {
                        Assert.AreEqual(entitySection.CourseTypes[j], dtoSection.CourseTypes[j]);
                    }

                    Assert.AreEqual(entitySection.MeetingInformation.Count, dtoSection.MeetingInformation.Count);
                    for (var j = 0; j < entitySection.MeetingInformation.Count; j++)
                    {
                        Assert.AreEqual(entitySection.MeetingInformation[j].Building, dtoSection.MeetingInformation[j].Building);
                        Assert.AreEqual(entitySection.MeetingInformation[j].Room, dtoSection.MeetingInformation[j].Room);
                        Assert.AreEqual(entitySection.MeetingInformation[j].InstructionalMethod, dtoSection.MeetingInformation[j].InstructionalMethod);
                        Assert.AreEqual(entitySection.MeetingInformation[j].StartTime, dtoSection.MeetingInformation[j].StartTime);
                        Assert.AreEqual(entitySection.MeetingInformation[j].EndTime, dtoSection.MeetingInformation[j].EndTime);

                        Assert.AreEqual(entitySection.MeetingInformation[j].DaysOfWeek.Count, dtoSection.MeetingInformation[j].DaysOfWeek.Count);
                        for (int k = 0; k < entitySection.MeetingInformation[j].DaysOfWeek.Count; k++)
                        {
                            Assert.AreEqual(entitySection.MeetingInformation[j].DaysOfWeek[k], dtoSection.MeetingInformation[j].DaysOfWeek[k]);
                        }
                    }

                    Assert.AreEqual(entitySection.BookInformation.Count, dtoSection.BookInformation.Count);
                    for (var j = 0; j < entitySection.BookInformation.Count; j++)
                    {
                        Assert.AreEqual(entitySection.BookInformation[j].Information, dtoSection.BookInformation[j].Information);
                        Assert.AreEqual(entitySection.BookInformation[j].Cost, dtoSection.BookInformation[j].Cost);
                    }

                }
            }

            //when repo returns null result
            [TestMethod]
            public async Task PortalService_Repo_Returns_Null()
            {
                portalRepositoryMock.Setup(repo => repo.GetSectionsForUpdateAsync()).ReturnsAsync(() => null);
                PortalUpdatedSectionsResult portalUpdatedSectionsResultDto = await service.GetSectionsForUpdateAsync();
                Assert.IsNull(portalUpdatedSectionsResultDto);
                loggerMock.Verify(l => l.Warn("Portal call to retrieve updated sections result from repository returned null entity"));
            }

            //when repo throws repo exception
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalService_Repo_throws_RepoException()
            {
                portalRepositoryMock.Setup(repo => repo.GetSectionsForUpdateAsync()).Throws(new RepositoryException());
                PortalUpdatedSectionsResult portalUpdatedSectionsResultDto = await service.GetSectionsForUpdateAsync();
            }

            //when repo throws  exception
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PortalService_Repo_throws_Exception()
            {
                portalRepositoryMock.Setup(repo => repo.GetSectionsForUpdateAsync()).Throws(new Exception());
                PortalUpdatedSectionsResult portalUpdatedSectionsResultDto = await service.GetSectionsForUpdateAsync();
            }

            //when repo returns 0 total sections
            [TestMethod]
            public async Task PortalService_With_Zero_TotalSections()
            {
                updatedSectionsResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedSectionsResult("MDY", 0, new List<Domain.Student.Entities.Portal.PortalSection>());
                portalRepositoryMock.Setup(repo => repo.GetSectionsForUpdateAsync()).ReturnsAsync(updatedSectionsResultEntity);
                PortalUpdatedSectionsResult portalUpdatedSectionsResultDto = await service.GetSectionsForUpdateAsync();
                Assert.IsNotNull(portalUpdatedSectionsResultDto);
                Assert.AreEqual(0, portalUpdatedSectionsResultDto.TotalSections);
                Assert.AreEqual(0, portalUpdatedSectionsResultDto.Sections.Count);
            }

            //when repo returns null total sections
            [TestMethod]
            public async Task PortalService_With_Null_TotalSections()
            {
                updatedSectionsResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedSectionsResult("MDY", null, new List<Domain.Student.Entities.Portal.PortalSection>());
                portalRepositoryMock.Setup(repo => repo.GetSectionsForUpdateAsync()).ReturnsAsync(updatedSectionsResultEntity);
                PortalUpdatedSectionsResult portalUpdatedSectionsResultDto = await service.GetSectionsForUpdateAsync();
                Assert.IsNotNull(portalUpdatedSectionsResultDto);
                Assert.IsNull(portalUpdatedSectionsResultDto.TotalSections);
                Assert.AreEqual(0, portalUpdatedSectionsResultDto.Sections.Count);
            }

            //when repo returns empty portal sections
            [TestMethod]
            public async Task PortalService_Empty_Sections()
            {
                updatedSectionsResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedSectionsResult("MDY", 2, new List<Domain.Student.Entities.Portal.PortalSection>());
                portalRepositoryMock.Setup(repo => repo.GetSectionsForUpdateAsync()).ReturnsAsync(updatedSectionsResultEntity);
                PortalUpdatedSectionsResult portalUpdatedSectionsResultDto = await service.GetSectionsForUpdateAsync();
                Assert.IsNotNull(portalUpdatedSectionsResultDto);
                Assert.AreEqual(2, portalUpdatedSectionsResultDto.TotalSections);
                Assert.AreEqual(0, portalUpdatedSectionsResultDto.Sections.Count);
            }

            //when repo returns null portal sections
            [TestMethod]
            public async Task PortalService_Null_Sections()
            {
                updatedSectionsResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedSectionsResult("MDY", 0, null);
                portalRepositoryMock.Setup(repo => repo.GetSectionsForUpdateAsync()).ReturnsAsync(updatedSectionsResultEntity);
                PortalUpdatedSectionsResult portalUpdatedSectionsResultDto = await service.GetSectionsForUpdateAsync();
                Assert.IsNotNull(portalUpdatedSectionsResultDto);
                Assert.AreEqual(0, portalUpdatedSectionsResultDto.TotalSections);
                Assert.AreEqual(0, portalUpdatedSectionsResultDto.Sections.Count);
            }

            //when user does not have permissions
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PortalService_Wrong_permssions()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Role>());
                PortalUpdatedSectionsResult portalUpdatedSectionsResultDto = await service.GetSectionsForUpdateAsync();
            }
        }

        [TestClass]
        public class PortalService_GetSectionsForDeletionAsync : PortalServiceTests
        {
            [TestInitialize]
            public void PortalService_GetSectionsForDeletionAsync_Initialize()
            {
            }

            //When repo returns proper result
            [TestMethod]
            public async Task PortalService_Repo_Returns_Proper_Sections()
            {
                PortalDeletedSectionsResult deletedSectionsDto = await service.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsDto);
                Assert.AreEqual(2, deletedSectionsDto.TotalSections);
                Assert.AreEqual(2, deletedSectionsDto.SectionIds.Count);
                Assert.AreEqual("section-1", deletedSectionsDto.SectionIds[0]);
                Assert.AreEqual("section-2", deletedSectionsDto.SectionIds[1]);
            }

            //when repo returns null result
            [TestMethod]
            public async Task PortalService_Repo_Returns_Null()
            {
                portalRepositoryMock.Setup(repo => repo.GetSectionsForDeletionAsync()).ReturnsAsync(() => null);
                PortalDeletedSectionsResult deletedSectionsDto = await service.GetSectionsForDeletionAsync();
                Assert.IsNull(deletedSectionsDto);
                loggerMock.Verify(l => l.Warn("Portal call to retrieve sections for deletion from repository returned null entity"));
            }

            //when repo throws repo exception
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalService_Repo_throws_RepoException()
            {
                portalRepositoryMock.Setup(repo => repo.GetSectionsForDeletionAsync()).Throws(new RepositoryException());
                PortalDeletedSectionsResult deletedSectionsDto = await service.GetSectionsForDeletionAsync();
            }

            //when repo throws exception
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PortalService_Repo_throws_Exception()
            {
                portalRepositoryMock.Setup(repo => repo.GetSectionsForDeletionAsync()).Throws(new Exception());
                PortalDeletedSectionsResult deletedSectionsDto = await service.GetSectionsForDeletionAsync();
            }

            //when repo returns 0 total sections
            [TestMethod]
            public async Task PortalService_With_Zero_TotalSections()
            {
                deletedSectionsEntity = new Domain.Student.Entities.Portal.PortalDeletedSectionsResult(0, new List<string>() { "section-1", "section-2" });
                portalRepositoryMock.Setup(repo => repo.GetSectionsForDeletionAsync()).ReturnsAsync(deletedSectionsEntity);
                PortalDeletedSectionsResult deletedSectionsDto = await service.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsDto);
                Assert.AreEqual(0, deletedSectionsDto.TotalSections);
                Assert.AreEqual(2, deletedSectionsDto.SectionIds.Count);
                Assert.AreEqual("section-1", deletedSectionsDto.SectionIds[0]);
                Assert.AreEqual("section-2", deletedSectionsDto.SectionIds[1]);
            }

            //when repo returns empty section ids
            [TestMethod]
            public async Task PortalService_Empty_Sections()
            {
                deletedSectionsEntity = new Domain.Student.Entities.Portal.PortalDeletedSectionsResult(2, new List<string>());
                portalRepositoryMock.Setup(repo => repo.GetSectionsForDeletionAsync()).ReturnsAsync(deletedSectionsEntity);
                PortalDeletedSectionsResult deletedSectionsDto = await service.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsDto);
                Assert.AreEqual(2, deletedSectionsDto.TotalSections);
                Assert.AreEqual(0, deletedSectionsDto.SectionIds.Count);
            }

            //when repo returns null section ids
            [TestMethod]
            public async Task PortalService_Null_Sections()
            {
                deletedSectionsEntity = new Domain.Student.Entities.Portal.PortalDeletedSectionsResult(-1, null);
                portalRepositoryMock.Setup(repo => repo.GetSectionsForDeletionAsync()).ReturnsAsync(deletedSectionsEntity);
                PortalDeletedSectionsResult deletedSectionsDto = await service.GetSectionsForDeletionAsync();
                Assert.IsNotNull(deletedSectionsDto);
                Assert.AreEqual(-1, deletedSectionsDto.TotalSections);
                Assert.IsNotNull(deletedSectionsDto.SectionIds);
                Assert.AreEqual(0, deletedSectionsDto.SectionIds.Count);
            }

            //when user does not have permissions
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PortalService_Wrong_permssions()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Role>());
                PortalDeletedSectionsResult deletedSectionsDto = await service.GetSectionsForDeletionAsync();
            }
        }

        [TestClass]
        public class PortalService_GetCoursesForUpdateAsync : PortalServiceTests
        {
            [TestInitialize]
            public void PortalService_GetCoursesForUpdateAsync_Initialize()
            {
            }

            //When repo returns valid result
            [TestMethod]
            public async Task PortalService_Repo_Returns_Valid_Updated_Courses_Result()
            {
                updatedCoursesResultEntity = SetPortalUpupdatedCoursesResultEntityData();
                portalRepositoryMock.Setup(repo => repo.GetCoursesForUpdateAsync()).ReturnsAsync(updatedCoursesResultEntity);
                PortalUpdatedCoursesResult updatedCoursesResultDto = await service.GetCoursesForUpdateAsync();

                Assert.IsNotNull(updatedCoursesResultDto);
                Assert.AreEqual(expected: updatedCoursesResultEntity.TotalCourses, actual: updatedCoursesResultDto.TotalCourses);


                Domain.Student.Entities.Portal.PortalCourse entityCourse = null;
                PortalCourse dtoCourse = null;

                for (var i = 0; i < updatedCoursesResultEntity.Courses.Count; i++)
                {
                    entityCourse = (updatedCoursesResultEntity.Courses[i]);
                    dtoCourse = (updatedCoursesResultDto.Courses[i]);

                    Assert.AreEqual(entityCourse.CourseId, dtoCourse.CourseId);
                    Assert.AreEqual(entityCourse.ShortTitle, dtoCourse.ShortTitle);
                    Assert.AreEqual(entityCourse.Title, dtoCourse.Title);
                    Assert.AreEqual(entityCourse.Description, dtoCourse.Description);
                    Assert.AreEqual(entityCourse.Subject, dtoCourse.Subject);
                    Assert.AreEqual(entityCourse.CourseNumber, dtoCourse.CourseNumber);
                    Assert.AreEqual(entityCourse.AcademicLevel, dtoCourse.AcademicLevel);
                    Assert.AreEqual(entityCourse.CourseName, dtoCourse.CourseName);
                    Assert.AreEqual(entityCourse.PrerequisiteText, dtoCourse.PrerequisiteText);

                    if (entityCourse.Departments != null)
                    {
                        Assert.AreEqual(entityCourse.Departments.Count, dtoCourse.Departments.Count);
                        for (var j = 0; j < entityCourse.Departments.Count; j++)
                        {
                            Assert.AreEqual(entityCourse.Departments[j], dtoCourse.Departments[j]);
                        }
                    }
                    else 
                    {
                        Assert.IsTrue(dtoCourse.Departments.Count == 0);
                    }

                    if (entityCourse.CourseTypes != null)
                    {
                        Assert.AreEqual(entityCourse.CourseTypes.Count, dtoCourse.CourseTypes.Count);
                        for (var j = 0; j < entityCourse.CourseTypes.Count; j++)
                        {
                            Assert.AreEqual(entityCourse.CourseTypes[j], dtoCourse.CourseTypes[j]);
                        }
                    }
                    else
                    {
                        Assert.IsTrue(dtoCourse.CourseTypes.Count == 0);
                    }

                    if (entityCourse.Locations != null)
                    {
                        Assert.AreEqual(entityCourse.Locations.Count, dtoCourse.Locations.Count);
                        for (var j = 0; j < entityCourse.Locations.Count; j++)
                        {
                            Assert.AreEqual(entityCourse.Locations[j], dtoCourse.Locations[j]);
                        }
                    }
                    else
                    {
                        Assert.IsTrue(dtoCourse.Locations.Count == 0);
                    }
                }
            }

            //when repo returns null result
            [TestMethod]
            public async Task PortalService_Repo_Returns_Null()
            {
                portalRepositoryMock.Setup(repo => repo.GetCoursesForUpdateAsync()).ReturnsAsync(() => null);
                PortalUpdatedCoursesResult portalUpdatedCoursesResultDto = await service.GetCoursesForUpdateAsync();
                Assert.IsNull(portalUpdatedCoursesResultDto);
                loggerMock.Verify(l => l.Warn("Portal call to retrieve updated courses result from repository returned null entity"));
            }

            //when repo throws repo exception
            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalService_Repo_throws_RepoException()
            {
                portalRepositoryMock.Setup(repo => repo.GetCoursesForUpdateAsync()).Throws(new RepositoryException());
                PortalUpdatedCoursesResult portalUpdatedCoursesResultDto = await service.GetCoursesForUpdateAsync();
            }

            //when repo throws exception
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PortalService_Repo_throws_Exception()
            {
                portalRepositoryMock.Setup(repo => repo.GetCoursesForUpdateAsync()).Throws(new Exception());
                PortalUpdatedCoursesResult portalUpdatedCoursesResultDto = await service.GetCoursesForUpdateAsync();
            }

            //when repo returns 0 total courses
            [TestMethod]
            public async Task PortalService_With_Zero_TotalCourses()
            {
                updatedCoursesResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedCoursesResult(0, new List<Domain.Student.Entities.Portal.PortalCourse>());
                portalRepositoryMock.Setup(repo => repo.GetCoursesForUpdateAsync()).ReturnsAsync(updatedCoursesResultEntity);
                PortalUpdatedCoursesResult portalUpdatedCoursesResultDto = await service.GetCoursesForUpdateAsync();
                Assert.IsNotNull(portalUpdatedCoursesResultDto);
                Assert.AreEqual(0, portalUpdatedCoursesResultDto.TotalCourses);
                Assert.AreEqual(0, portalUpdatedCoursesResultDto.Courses.Count);
            }

            //when repo returns null total courses
            [TestMethod]
            public async Task PortalService_With_Null_TotalCourses()
            {
                updatedCoursesResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedCoursesResult(null, new List<Domain.Student.Entities.Portal.PortalCourse>());
                portalRepositoryMock.Setup(repo => repo.GetCoursesForUpdateAsync()).ReturnsAsync(updatedCoursesResultEntity);
                PortalUpdatedCoursesResult portalUpdatedCoursesResultDto = await service.GetCoursesForUpdateAsync();
                Assert.IsNotNull(portalUpdatedCoursesResultDto);
                Assert.IsNull(portalUpdatedCoursesResultDto.TotalCourses);
                Assert.AreEqual(0, portalUpdatedCoursesResultDto.Courses.Count);
            }

            //when repo returns empty portal courses
            [TestMethod]
            public async Task PortalService_Empty_Courses()
            {
                updatedCoursesResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedCoursesResult(2, new List<Domain.Student.Entities.Portal.PortalCourse>());
                portalRepositoryMock.Setup(repo => repo.GetCoursesForUpdateAsync()).ReturnsAsync(updatedCoursesResultEntity);
                PortalUpdatedCoursesResult portalUpdatedCoursesResultDto = await service.GetCoursesForUpdateAsync();
                Assert.IsNotNull(portalUpdatedCoursesResultDto);
                Assert.AreEqual(2, portalUpdatedCoursesResultDto.TotalCourses);
                Assert.AreEqual(0, portalUpdatedCoursesResultDto.Courses.Count);
            }

            //when repo returns null portal courses
            [TestMethod]
            public async Task PortalService_Null_Courses()
            {
                updatedCoursesResultEntity = new Domain.Student.Entities.Portal.PortalUpdatedCoursesResult(0, null);
                portalRepositoryMock.Setup(repo => repo.GetCoursesForUpdateAsync()).ReturnsAsync(updatedCoursesResultEntity);
                PortalUpdatedCoursesResult portalUpdatedCoursesResultDto = await service.GetCoursesForUpdateAsync();
                Assert.IsNotNull(portalUpdatedCoursesResultDto);
                Assert.AreEqual(0, portalUpdatedCoursesResultDto.TotalCourses);
                Assert.AreEqual(0, portalUpdatedCoursesResultDto.Courses.Count);
            }

            //when user does not have permissions
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PortalService_Wrong_permssions()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Role>());
                PortalUpdatedCoursesResult portalUpdatedCoursesResultDto = await service.GetCoursesForUpdateAsync();
            }
        }

        [TestClass]
        public class PortalService_GetEventsAndRemindersAsync : PortalServiceTests
        {
            [TestInitialize]
            public void PortalService_GetEventsAndRemindersAsync_Initialize()
            {
            }

            [TestMethod]
            public async Task PortalService_GetEventsAndRemindersAsync_Repo_returns_null()
            {
                portalRepositoryMock.Setup(repo => repo.GetEventsAndRemindersAsync(It.IsAny<string>(), It.IsAny<Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria>())).ReturnsAsync(() => null);
                PortalEventsAndReminders dto = await service.GetEventsAndRemindersAsync(new PortalEventsAndRemindersQueryCriteria());
                Assert.IsNotNull(dto);
                Assert.IsNull(dto.HostShortDateFormat);
                Assert.IsNull(dto.Events);
                Assert.IsNull(dto.Reminders);
            }

            [TestMethod]
            public async Task PortalService_GetEventsAndRemindersAsync_Repo_returns_PortalEventsAndReminders()
            {
                Domain.Student.Entities.Portal.PortalEventsAndReminders entity = new Domain.Student.Entities.Portal.PortalEventsAndReminders("MDY");
                entity.AddPortalEvent(new Domain.Student.Entities.Portal.PortalEvent("1", "2", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today.AddHours(10), "Description", "Building", "Room", "CS Course Section Meeting", "SUBJ", "SUBJ-100", "100", "Participants"));
                entity.AddPortalReminder(new Domain.Student.Entities.Portal.PortalReminder("1", DateTime.Today, DateTime.Today.AddHours(12), DateTime.Today.AddDays(1), DateTime.Today.AddDays(1).AddHours(12), "City", "Region", "HO Holiday", "Short Text", "Participants"));
                portalRepositoryMock.Setup(repo => repo.GetEventsAndRemindersAsync(It.IsAny<string>(), It.IsAny<Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria>())).ReturnsAsync(entity);
                PortalEventsAndReminders dto = await service.GetEventsAndRemindersAsync(new PortalEventsAndRemindersQueryCriteria());
                Assert.IsNotNull(dto);
                Assert.AreEqual(entity.HostShortDateFormat, dto.HostShortDateFormat);
                Assert.AreEqual(entity.Events.Count, dto.Events.Count());
                Assert.AreEqual(entity.Reminders.Count, dto.Reminders.Count());
            }

            [TestMethod]
            public async Task PortalService_GetEventsAndRemindersAsync_Repo_returns_PortalEventsAndReminders_null_criteria()
            {
                Domain.Student.Entities.Portal.PortalEventsAndReminders entity = new Domain.Student.Entities.Portal.PortalEventsAndReminders("MDY");
                entity.AddPortalEvent(new Domain.Student.Entities.Portal.PortalEvent("1", "2", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today.AddHours(10), "Description", "Building", "Room", "CS Course Section Meeting", "SUBJ", "SUBJ-100", "100", "Participants"));
                entity.AddPortalReminder(new Domain.Student.Entities.Portal.PortalReminder("1", DateTime.Today, DateTime.Today.AddHours(12), DateTime.Today.AddDays(1), DateTime.Today.AddDays(1).AddHours(12), "City", "Region", "HO Holiday", "Short Text", "Participants"));
                portalRepositoryMock.Setup(repo => repo.GetEventsAndRemindersAsync(It.IsAny<string>(), It.IsAny<Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria>())).ReturnsAsync(entity);
                PortalEventsAndReminders dto = await service.GetEventsAndRemindersAsync(null);
                Assert.IsNotNull(dto);
                Assert.AreEqual(entity.HostShortDateFormat, dto.HostShortDateFormat);
                Assert.AreEqual(entity.Events.Count, dto.Events.Count());
                Assert.AreEqual(entity.Reminders.Count, dto.Reminders.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalService_GetEventsAndRemindersAsync_Repo_throws_RepositoryException()
            {
                portalRepositoryMock.Setup(repo => repo.GetEventsAndRemindersAsync(It.IsAny<string>(), It.IsAny<Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria>())).ThrowsAsync(new RepositoryException());
                PortalEventsAndReminders dto = await service.GetEventsAndRemindersAsync(new PortalEventsAndRemindersQueryCriteria());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task PortalService_GetEventsAndRemindersAsync_Repo_throws_Exception()
            {
                portalRepositoryMock.Setup(repo => repo.GetEventsAndRemindersAsync(It.IsAny<string>(), It.IsAny<Domain.Student.Entities.Portal.PortalEventsAndRemindersQueryCriteria>())).ThrowsAsync(new Exception());
                PortalEventsAndReminders dto = await service.GetEventsAndRemindersAsync(new PortalEventsAndRemindersQueryCriteria());
            }

        }

        [TestClass]
        public class PortalService_UpdateStudentPreferredCourseSectionsAsync : PortalServiceTests
        {
            [TestInitialize]
            public void PortalService_UpdateStudentPreferredCourseSectionsAsync_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PortalService_UpdateStudentPreferredCourseSectionsAsync_null_StudentId()
            {
                var result = await service.UpdateStudentPreferredCourseSectionsAsync(null, new List<string>() { "12345" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PortalService_UpdateStudentPreferredCourseSectionsAsync_null_CourseSectionIds()
            {
                var result = await service.UpdateStudentPreferredCourseSectionsAsync(currentUserFactory.CurrentUser.PersonId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PortalService_UpdateStudentPreferredCourseSectionsAsync_empty_CourseSectionIds()
            {
                var result = await service.UpdateStudentPreferredCourseSectionsAsync(currentUserFactory.CurrentUser.PersonId, new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PortalService_UpdateStudentPreferredCourseSectionsAsync_CourseSectionIds_all_whitespace()
            {
                var result = await service.UpdateStudentPreferredCourseSectionsAsync(currentUserFactory.CurrentUser.PersonId, new List<string>() { null, string.Empty });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PortalService_UpdateStudentPreferredCourseSectionsAsync_user_is_not_requesting_For_self()
            {
                var result = await service.UpdateStudentPreferredCourseSectionsAsync("NOT_ME", new List<string>() { "123" });
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PortalService_UpdateStudentPreferredCourseSectionsAsync_repo_throws_RepositoryException()
            {
                portalRepositoryMock.Setup(repo => repo.UpdateStudentPreferredCourseSectionsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ThrowsAsync(new RepositoryException());
                var result = await service.UpdateStudentPreferredCourseSectionsAsync(currentUserFactory.CurrentUser.PersonId, new List<string>() { "123" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PortalService_UpdateStudentPreferredCourseSectionsAsync_repo_throws_Exception()
            {
                portalRepositoryMock.Setup(repo => repo.UpdateStudentPreferredCourseSectionsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ThrowsAsync(new ArgumentException());
                var result = await service.UpdateStudentPreferredCourseSectionsAsync(currentUserFactory.CurrentUser.PersonId, new List<string>() { "123" });
            }

            [TestMethod]
            public async Task PortalService_UpdateStudentPreferredCourseSectionsAsync_valid()
            {
                portalRepositoryMock.Setup(repo => repo.UpdateStudentPreferredCourseSectionsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Domain.Student.Entities.Portal.PortalStudentPreferredCourseSectionUpdateResult>()
                {
                    new Domain.Student.Entities.Portal.PortalStudentPreferredCourseSectionUpdateResult("123", Domain.Student.Entities.Portal.PortalStudentPreferredCourseSectionUpdateStatus.Ok),
                    new Domain.Student.Entities.Portal.PortalStudentPreferredCourseSectionUpdateResult("456", Domain.Student.Entities.Portal.PortalStudentPreferredCourseSectionUpdateStatus.Error, "Unable to add 456.")
                });
                var result = await service.UpdateStudentPreferredCourseSectionsAsync(currentUserFactory.CurrentUser.PersonId, new List<string>() { "123", "456" });
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("123", result.ElementAt(0).CourseSectionId);
                Assert.AreEqual(PortalStudentPreferredCourseSectionUpdateStatus.Ok, result.ElementAt(0).Status);
                Assert.AreEqual(null, result.ElementAt(0).Message);
                Assert.AreEqual("456", result.ElementAt(1).CourseSectionId);
                Assert.AreEqual(PortalStudentPreferredCourseSectionUpdateStatus.Error, result.ElementAt(1).Status);
                Assert.AreEqual("Unable to add 456.", result.ElementAt(1).Message);
            }
        }

        private Domain.Student.Entities.Portal.PortalUpdatedSectionsResult SetPortalUpupdatedSectionsResultEntityData()
        {

            var portalSectionEntities = new List<Domain.Student.Entities.Portal.PortalSection>();
            var portalSectionMeetingEntities = new List<Domain.Student.Entities.Portal.PortalSectionMeeting>();
            var portalBookInformationEntities = new List<Domain.Student.Entities.Portal.PortalBookInformation>();

            var sectionDepartments = new List<string>() { "dept-1", "dept-2" };
            var sectionFaculty = new List<string>() { "faculty name1", "faculty name2" };
            var courseTypes = new List<string>() { "type-1", "type-2" };

            portalSectionMeetingEntities.Add(new Domain.Student.Entities.Portal.PortalSectionMeeting(
                building: "building 1",
                room: "room 1",
                instructionalMethod: "online",
               daysOfWeek: new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday },
               startTime: new DateTimeOffset(),
               endTime: new DateTimeOffset()
                ));

            portalSectionMeetingEntities.Add(new Domain.Student.Entities.Portal.PortalSectionMeeting(
                building: "building 2",
                room: "room 2",
                instructionalMethod: "online",
               daysOfWeek: new List<DayOfWeek>(),
               startTime: new DateTimeOffset(),
               endTime: new DateTimeOffset()
                ));

            portalBookInformationEntities.Add(new Domain.Student.Entities.Portal.PortalBookInformation(
                information: "book information 1",
                cost: 100.00m
                ));

            portalBookInformationEntities.Add(new Domain.Student.Entities.Portal.PortalBookInformation(
                information: "book information 2",
                cost: 25.25m
                ));

            portalSectionEntities.Add(new Domain.Student.Entities.Portal.PortalSection(
                sectionId: null,
                shortTitle: null,
                description: null,
                location: null,
                term: null,
                startDate: null,
                endDate: null,
                meetingInformation: new List<Domain.Student.Entities.Portal.PortalSectionMeeting>(),
                capacity: null,
                subject: null,
                departments: new List<string>(),
                courseNumber: null,
                sectionNumber: null,
                academicLevel: null,
                synonym: null,
                faculty: new List<string>(),
                minimumCredits: null,
                maximumCredits: null,
                sectionName: null,
                courseId: null,
                prerequisiteText: null,
                courseTypes: new List<string>(),
                continuingEducationUnits: null,
                printedComments: null,
                bookInformation: new List<Domain.Student.Entities.Portal.PortalBookInformation>(),
                totalBookCost: 0
            ));

            portalSectionEntities.Add(new Domain.Student.Entities.Portal.PortalSection(
                sectionId: string.Empty,
                shortTitle: string.Empty,
                description: string.Empty,
                location: string.Empty,
                term: string.Empty,
                startDate: DateTime.MinValue,
                endDate: DateTime.MaxValue,
                meetingInformation: new List<Domain.Student.Entities.Portal.PortalSectionMeeting>(),
                capacity: 0,
                subject: string.Empty,
                departments: new List<string>(),
                courseNumber: string.Empty,
                sectionNumber: string.Empty,
                academicLevel: string.Empty,
                synonym: string.Empty,
                faculty: new List<string>(),
                minimumCredits: 0,
                maximumCredits: 0,
                sectionName: string.Empty,
                courseId: string.Empty,
                prerequisiteText: string.Empty,
                courseTypes: new List<string>(),
                continuingEducationUnits: 0,
                printedComments: string.Empty,
                bookInformation: new List<Domain.Student.Entities.Portal.PortalBookInformation>(),
                totalBookCost: 0
            ));

            portalSectionEntities.Add(new Domain.Student.Entities.Portal.PortalSection(
                sectionId: "section-1",
                shortTitle: "short title",
                description: "course-1 description",
                location: "section location",
                term: "term",
                startDate: new DateTime(2020, 01, 05),
                endDate: new DateTime(2020, 05, 21),
                meetingInformation: portalSectionMeetingEntities,
                capacity: 20,
                subject: "course subject",
                departments: sectionDepartments,
                courseNumber: "course-1",
                sectionNumber: "section no",
                academicLevel: "acad level",
                synonym: "12345",
                faculty: sectionFaculty,
                minimumCredits: 1,
                maximumCredits: 12,
                sectionName: "section name",
                courseId: "course id",
                prerequisiteText: "couese prerequisite text",
                courseTypes: courseTypes,
                continuingEducationUnits: 1.5m,
                printedComments: "course printed comments",
                bookInformation: portalBookInformationEntities,
                totalBookCost: 125.25m
            ));

            portalSectionEntities.Add(new Domain.Student.Entities.Portal.PortalSection(
                sectionId: "section-2",
                shortTitle: "short title",
                description: "course-2 description",
                location: "section location",
                term: "term",
                startDate: new DateTime(2020, 01, 05),
                endDate: new DateTime(2020, 05, 21),
                meetingInformation: new List<Domain.Student.Entities.Portal.PortalSectionMeeting>(),
                capacity: 20,
                subject: "course subject",
                departments: sectionDepartments,
                courseNumber: "course-2",
                sectionNumber: "section 2 no",
                academicLevel: "acad level",
                synonym: "67890",
                faculty: sectionFaculty,
                minimumCredits: 2,
                maximumCredits: 10,
                sectionName: "section 2 name",
                courseId: "course 2 id",
                prerequisiteText: "couese 2 prerequisite text",
                courseTypes: courseTypes,
                continuingEducationUnits: 3.25m,
                printedComments: "course 2 printed comments",
                bookInformation: new List<Domain.Student.Entities.Portal.PortalBookInformation>(),
                totalBookCost: 0m
            ));

            return new Domain.Student.Entities.Portal.PortalUpdatedSectionsResult(shortDateFormat: "MDY", totalSections: 154, sections: portalSectionEntities);
        }

        private Domain.Student.Entities.Portal.PortalUpdatedCoursesResult SetPortalUpupdatedCoursesResultEntityData()
        {
            var portalCourseEntities = new List<Domain.Student.Entities.Portal.PortalCourse>();

            portalCourseEntities.Add(new Domain.Student.Entities.Portal.PortalCourse(
                    courseId: null,
                    shortTitle: null,
                    title: null,
                    description: null,
                    subject: null,
                    departments: null,
                    courseNumber: null,
                    academicLevel: null,
                    courseName: null,
                    courseTypes: null,
                    prerequisiteText: null,
                    locations: null
            ));

            portalCourseEntities.Add(new Domain.Student.Entities.Portal.PortalCourse(
                    courseId: string.Empty,
                    shortTitle: string.Empty,
                    title: string.Empty,
                    description: string.Empty,
                    subject: string.Empty,
                    departments: new List<string>(),
                    courseNumber: string.Empty,
                    academicLevel: string.Empty,
                    courseName: string.Empty,
                    courseTypes: new List<string>(),
                    prerequisiteText: string.Empty,
                    locations: new List<string>()
            ));

            portalCourseEntities.Add(new Domain.Student.Entities.Portal.PortalCourse(
                    courseId: "10",
                    shortTitle: "Studies in French",
                    title: "Intermediate Studies in French",
                    description: "Intermediate French Language and Literature",
                    subject: "French",
                    departments: new List<string>() { "Modern Language and Literature" },
                    courseNumber: "500",
                    academicLevel: "Graduate",
                    courseName: "FREN-500",
                    courseTypes: new List<string>() { "Standard" },
                    prerequisiteText: "Take FREN-400",
                    locations: new List<string>() { "Main Campus" }
            ));

            portalCourseEntities.Add(new Domain.Student.Entities.Portal.PortalCourse(
                    courseId: "100",
                    shortTitle: "Talmudic Studies",
                    title: "Beginning Talmudic Studies",
                    description: "Study of the Talmud",
                    subject: "Religious Studies",
                    departments: new List<string>() { "Religious Studies" },
                    courseNumber: "CE400",
                    academicLevel: "Continuing Education Level",
                    courseName: "RELG-CE400",
                    courseTypes: new List<string>() { "Standard" },
                    prerequisiteText: "Take RELG-100 AND RELG-200",
                    locations: new List<string>()
            ));

            var courseLocations = new List<string>() { "Herndon College of Music", "Central Campus South", "Fred Robinson College", "Central District Office", "Main Campus" };
            portalCourseEntities.Add(new Domain.Student.Entities.Portal.PortalCourse(
                    courseId: "980",
                    shortTitle: "Comm * 1321",
                    title: "The Art of Effective Communication",
                    description: "The Art of Effective Communication Description",
                    subject: "Communications",
                    departments: new List<string>() { "Communications", "Humanities" },
                    courseNumber: "1321",
                    academicLevel: "Undergraduate",
                    courseName: "COMM-1321",
                    courseTypes: new List<string>() { "Standard", "Basic Skills", "Humanities" },
                    prerequisiteText: "Take RELG-100 AND RELG-200",
                    locations: courseLocations

            ));

            return new Domain.Student.Entities.Portal.PortalUpdatedCoursesResult(totalCourses: 8, courses: portalCourseEntities);
        }
    }
}
