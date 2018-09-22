// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Dtos;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.Exceptions;
using AcademicPeriodEnrollmentStatus = Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus;
using AutoMapper;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class CurriculumServiceTests
    {

        [TestClass]
        public class GetAcademicLevels
        {
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private ILogger logger;
            private CurriculumService curriculumService;
            private ICollection<Domain.Student.Entities.AcademicLevel> academicLevelCollection = new List<Domain.Student.Entities.AcademicLevel>();

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;

                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CE", "Continuing Education"));
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "GR", "Graduate"));
                academicLevelCollection.Add(new Domain.Student.Entities.AcademicLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "UG", "Undergraduate"));
                referenceRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync()).ReturnsAsync(academicLevelCollection);
                referenceRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicLevelCollection);
                referenceRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicLevelCollection);

                curriculumService = new CurriculumService(referenceRepository, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                academicLevelCollection = null;
                referenceRepository = null;
                curriculumService = null;
            }

            [TestMethod]
            public async Task CurriculumService__AcademicLevels2()
            {
                var results = await curriculumService.GetAcademicLevels2Async();
                Assert.IsTrue(results is IEnumerable<AcademicLevel2>);
                Assert.IsNotNull(results);
            }

            public async Task CurriculumService_AcademicLevels2_Count()
            {
                var results = await curriculumService.GetAcademicLevels2Async();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CurriculumService_AcademicLevels2_Properties()
            {
                var results = await curriculumService.GetAcademicLevels2Async();
                var academicLevel = results.Where(x => x.Code == "CE").FirstOrDefault();
                Assert.IsNotNull(academicLevel.Id);
                Assert.IsNotNull(academicLevel.Code);
                //Assert.IsNotNull(instructionalMethod.CreditType);
            }

            [TestMethod]
            public async Task CurriculumService_AcademicLevels2_Expected()
            {
                var expectedResults = academicLevelCollection.Where(c => c.Code == "CE").FirstOrDefault();
                var results = await curriculumService.GetAcademicLevels2Async();
                var academicLevel = results.Where(s => s.Code == "CE").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, academicLevel.Id);
                Assert.AreEqual(expectedResults.Code, academicLevel.Code);
                //Assert.AreEqual(expectedResults.CreditType.ToString(), creditCategory.CreditType.ToString());
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetAcademicLevelById2_Empty()
            {
                await curriculumService.GetAcademicLevelById2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetAcademicLevelById2_Null()
            {
                await curriculumService.GetAcademicLevelById2Async(null);
            }

            [TestMethod]
            public async Task CurriculumService_GetAcademicLevelById2_Expected()
            {
                var expectedResults = academicLevelCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var academicLevel = await curriculumService.GetAcademicLevelById2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, academicLevel.Id);
                Assert.AreEqual(expectedResults.Code, academicLevel.Code);
                //Assert.AreEqual(expectedResults.CreditType.ToString(), instructionalMethod.CreditType.ToString());
            }

            [TestMethod]
            public async Task CurriculumService_GetAcademicLevelById2_Properties()
            {
                var expectedResults = academicLevelCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var academicLevel = await curriculumService.GetAcademicLevelById2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(academicLevel.Id);
                Assert.IsNotNull(academicLevel.Code);
                //Assert.IsNotNull(instructionalMethod.CreditType.ToString());
            }
        }

        [TestClass]
        public class AcademicPeriodEnrollmentStatus_GET
        {
            private Mock<IStudentReferenceDataRepository> studentReferenceRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ILogger logger;
            private CurriculumService curriculumService;

            List<Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus> enrollmentStatusesEntities = new List<Domain.Student.Entities.EnrollmentStatus>();
            List<AcademicPeriodEnrollmentStatus> enrollmentStatusCollection = new List<AcademicPeriodEnrollmentStatus>();

            [TestInitialize]
            public void Initialize()
            {
                studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                logger = new Mock<ILogger>().Object;

                enrollmentStatusCollection = BuildStatuses();
                enrollmentStatusesEntities = BuildStatusesEntites();

                curriculumService = new CurriculumService(studentReferenceRepositoryMock.Object, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            private List<Domain.Student.Entities.EnrollmentStatus> BuildStatusesEntites()
            {
 	            List<Domain.Student.Entities.EnrollmentStatus> statusesEntities = new List<Domain.Student.Entities.EnrollmentStatus>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus("891ab399-259f-4597-aca7-57b1a7f31626", "A", "Active", Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatusType.active),
                    new Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus("e37f3d90-622e-4636-9110-bdb18ad538e5", "P", "Potential", Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatusType.active),
                    new Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus("0d383982-fa39-4c3d-9a50-28223014c3c6", "W", "Withdrawn", Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatusType.active),
                    new Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus("3a8500a2-9d16-4be4-8d08-3a86199cf9a9", "G", "Graduated", Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatusType.active),
                    new Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatus("3c6d72b0-1915-42a7-abe4-cac21d3166b2", "C", "Changed Program", Ellucian.Colleague.Domain.Student.Entities.EnrollmentStatusType.active),
                };
                return statusesEntities;
            }

            private List<AcademicPeriodEnrollmentStatus> BuildStatuses()
            {
                List<AcademicPeriodEnrollmentStatus> statuses = new List<AcademicPeriodEnrollmentStatus>() 
            { 
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "A",
                    Title = "Active",
                    Description = "Active",
                    Id = "891ab399-259f-4597-aca7-57b1a7f31626"
                },
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "P",
                    Title = "Potential",
                    Description = "Potential",
                    Id = "e37f3d90-622e-4636-9110-bdb18ad538e5"
                },
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "W",
                    Title = "Withdrawn",
                    Description = "Withdrawn",
                    Id = "0d383982-fa39-4c3d-9a50-28223014c3c6"
                },
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "G",
                    Title = "Graduated",
                    Description = "Graduated",
                    Id = "3a8500a2-9d16-4be4-8d08-3a86199cf9a9"
                },
                new AcademicPeriodEnrollmentStatus()
                {
                    Code = "c",
                    Title = "Changed Program",
                    Description = "Changed Program",
                    Id = "3c6d72b0-1915-42a7-abe4-cac21d3166b2"
                },
            };
                return statuses;
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentReferenceRepositoryMock = null;
                logger = null;
                curriculumService = null;
                enrollmentStatusesEntities = null;
                enrollmentStatusCollection = null;
            }

            [TestMethod]
            public async Task CurriculumService__AcademicPeriodEnrollmentStatus()
            {
                studentReferenceRepositoryMock.Setup(i => i.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatusesEntities);
                var actuals = await curriculumService.GetAcademicPeriodEnrollmentStatusesAsync(It.IsAny<bool>());

                Assert.AreEqual(actuals.Count(), enrollmentStatusCollection.Count());

                foreach (var actual in actuals)
                {
                    var expected = enrollmentStatusesEntities.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Guid, actual.Id);

                }
            }

            [TestMethod]
            public async Task CurriculumService__AcademicPeriodEnrollmentStatus_ById()
            {
                string id = "e37f3d90-622e-4636-9110-bdb18ad538e5";
                var expected = enrollmentStatusesEntities.FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));

                studentReferenceRepositoryMock.Setup(i => i.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatusesEntities);
                var actual = await curriculumService.GetAcademicPeriodEnrollmentStatusByGuidAsync(id);

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.Guid, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService__AcademicPeriodEnrollmentStatus_ById_KeyNotFoundException()
            {
                var actual = await curriculumService.GetAcademicPeriodEnrollmentStatusByGuidAsync(It.IsAny<string>());
            }
        }

        [TestClass]
        public class GetAccountReceivableTypes
        {
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<ILogger> loggerMock;

            CurriculumService curriculumService;
            List<Ellucian.Colleague.Domain.Student.Entities.AccountReceivableType> accountReceivableTypes;

            [TestInitialize]
            public void Initialize()
            {
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                loggerMock = new Mock<ILogger>();

                accountReceivableTypes = new TestAccountReceivableTypeRepository().Get();

                curriculumService = new CurriculumService(studentReferenceDataRepositoryMock.Object, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                curriculumService = null;
                accountReceivableTypes = null;
            }

            [TestMethod]
            public async Task CurriculumService__GetAllAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountReceivableTypesAsync(It.IsAny<bool>())).ReturnsAsync(accountReceivableTypes);

                var results = await curriculumService.GetAccountReceivableTypesAsync(It.IsAny<bool>());
                Assert.AreEqual(accountReceivableTypes.Count, (results.Count()));

                foreach (var accountReceivableType in accountReceivableTypes)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountReceivableType.Guid);

                    Assert.AreEqual(accountReceivableType.Code, result.Code);
                    Assert.AreEqual(accountReceivableType.Description, result.Title);
                    Assert.AreEqual(accountReceivableType.Guid, result.Id);
                }
            }

            [TestMethod]
            public async Task CurriculumService__GetByIdAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountReceivableTypesAsync(It.IsAny<bool>())).ReturnsAsync(accountReceivableTypes);

                string id = "a142d78a-b472-45de-8a4b-953258976a0b";
                var accountReceivableType = accountReceivableTypes.FirstOrDefault(i => i.Guid == id);

                var result = await curriculumService.GetAccountReceivableTypeByIdAsync(id);

                Assert.AreEqual(accountReceivableType.Code, result.Code);
                Assert.AreEqual(accountReceivableType.Description, result.Title);
                Assert.AreEqual(accountReceivableType.Guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService__GetByIdAsync_KeyNotFoundException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountReceivableTypesAsync(true)).ReturnsAsync(accountReceivableTypes);
                var result = await curriculumService.GetAccountReceivableTypeByIdAsync("123");
            }
        }

        [TestClass]
        public class GetAssessmentSpecialCircumstances
        {
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ILogger logger;
            private CurriculumService curriculumService;
            private ICollection<Domain.Student.Entities.AssessmentSpecialCircumstance> assessmentSpecialCircumstanceCollection = new List<Domain.Student.Entities.AssessmentSpecialCircumstance>();

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                logger = new Mock<ILogger>().Object;

                assessmentSpecialCircumstanceCollection.Add(new Domain.Student.Entities.AssessmentSpecialCircumstance("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CODE1", "DESC1"));
                assessmentSpecialCircumstanceCollection.Add(new Domain.Student.Entities.AssessmentSpecialCircumstance("73244057-D1EC-4094-A0B7-DE602533E3A6", "CODE2", "DESC2"));
                assessmentSpecialCircumstanceCollection.Add(new Domain.Student.Entities.AssessmentSpecialCircumstance("1df164eb-8178-4321-a9f7-24f12d3991d8", "CODE3", "DESC3"));
                referenceRepositoryMock.Setup(repo => repo.GetAssessmentSpecialCircumstancesAsync(true)).ReturnsAsync(assessmentSpecialCircumstanceCollection);
                referenceRepositoryMock.Setup(repo => repo.GetAssessmentSpecialCircumstancesAsync(false)).ReturnsAsync(assessmentSpecialCircumstanceCollection);

                curriculumService = new CurriculumService(referenceRepository, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                assessmentSpecialCircumstanceCollection = null;
                referenceRepository = null;
                curriculumService = null;
            }

            [TestMethod]
            public async Task CurriculumService__AssessmentSpecialCircumstances()
            {
                var results = await curriculumService.GetAssessmentSpecialCircumstancesAsync();
                Assert.IsTrue(results is IEnumerable<AssessmentSpecialCircumstance>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task CurriculumService_AssessmentSpecialCircumstances_Count()
            {
                var results = await curriculumService.GetAssessmentSpecialCircumstancesAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CurriculumService_AssessmentSpecialCircumstances_Properties()
            {
                var results = await curriculumService.GetAssessmentSpecialCircumstancesAsync();
                var assessmentSpecialCircumstance = results.Where(x => x.Code == "CODE1").FirstOrDefault();
                Assert.IsNotNull(assessmentSpecialCircumstance.Id);
                Assert.IsNotNull(assessmentSpecialCircumstance.Code);
            }

            [TestMethod]
            public async Task CurriculumService_AssessmentSpecialCircumstances_Expected()
            {
                var expectedResults = assessmentSpecialCircumstanceCollection.Where(c => c.Code == "CODE2").FirstOrDefault();
                var results = await curriculumService.GetAssessmentSpecialCircumstancesAsync();
                var assessmentSpecialCircumstance = results.Where(s => s.Code == "CODE2").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, assessmentSpecialCircumstance.Id);
                Assert.AreEqual(expectedResults.Code, assessmentSpecialCircumstance.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetAssessmentSpecialCircumstancedByGuid_Empty()
            {
                await curriculumService.GetAssessmentSpecialCircumstanceByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetAssessmentSpecialCircumstanceByGuid_Null()
            {
                await curriculumService.GetAssessmentSpecialCircumstanceByGuidAsync(null);
            }

            [TestMethod]
            public async Task CurriculumService_GetAssessmentSpecialCircumstanceByGuid_Expected()
            {
                var expectedResults = assessmentSpecialCircumstanceCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var assessmentSpecialCircumstance = await curriculumService.GetAssessmentSpecialCircumstanceByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, assessmentSpecialCircumstance.Id);
                Assert.AreEqual(expectedResults.Code, assessmentSpecialCircumstance.Code);
            }

            [TestMethod]
            public async Task CurriculumService_GetAssessmentSpecialCircumstanceByGuid_Properties()
            {
                var expectedResults = assessmentSpecialCircumstanceCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var assessmentSpecialCircumstance = await curriculumService.GetAssessmentSpecialCircumstanceByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(assessmentSpecialCircumstance.Id);
                Assert.IsNotNull(assessmentSpecialCircumstance.Code);
            }
        }


        [TestClass]
        public class GetCourseLevels
        {
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ILogger logger;
            private CurriculumService curriculumService;
            private ICollection<Domain.Student.Entities.CourseLevel> courseLevelCollection = new List<Domain.Student.Entities.CourseLevel>();

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                logger = new Mock<ILogger>().Object;

                courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "100", "First Yr"));
                courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("73244057-D1EC-4094-A0B7-DE602533E3A6", "200", "Second Year"));
                courseLevelCollection.Add(new Domain.Student.Entities.CourseLevel("1df164eb-8178-4321-a9f7-24f12d3991d8", "300", "Third Year"));
                referenceRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync()).ReturnsAsync(courseLevelCollection);
                referenceRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(true)).ReturnsAsync(courseLevelCollection);
                referenceRepositoryMock.Setup(repo => repo.GetCourseLevelsAsync(false)).ReturnsAsync(courseLevelCollection);

                curriculumService = new CurriculumService(referenceRepository, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                courseLevelCollection = null;
                referenceRepository = null;
                curriculumService = null;
            }

            [TestMethod]
            public async Task CurriculumService__CourseLevels2()
            {
                var results = await curriculumService.GetCourseLevels2Async();
                Assert.IsTrue(results is IEnumerable<CourseLevel2>);
                Assert.IsNotNull(results);
            }

            public async Task CurriculumService_CourseLevels2_Count()
            {
                var results = await curriculumService.GetCourseLevels2Async();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CurriculumService_CourseLevels2_Properties()
            {
                var results = await curriculumService.GetCourseLevels2Async();
                var courseLevel = results.Where(x => x.Code == "100").FirstOrDefault();
                Assert.IsNotNull(courseLevel.Id);
                Assert.IsNotNull(courseLevel.Code);
                Assert.IsNotNull(courseLevel.Title);
            }

            [TestMethod]
            public async Task CurriculumService_CourseLevels2_Expected()
            {
                var expectedResults = courseLevelCollection.Where(c => c.Code == "100").FirstOrDefault();
                var results = await curriculumService.GetCourseLevels2Async();
                var courseLevel = results.Where(s => s.Code == "100").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, courseLevel.Id);
                Assert.AreEqual(expectedResults.Code, courseLevel.Code);
                Assert.AreEqual(expectedResults.Description, courseLevel.Title);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetCourseLevelById2_Empty()
            {
                await curriculumService.GetCourseLevelById2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetCourseLevelById2_Null()
            {
                await curriculumService.GetCourseLevelById2Async(null);
            }

            [TestMethod]
            public async Task CurriculumService_GetCourseLevelById2_Expected()
            {
                var expectedResults = courseLevelCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var courseLevel = await curriculumService.GetCourseLevelById2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, courseLevel.Id);
                Assert.AreEqual(expectedResults.Code, courseLevel.Code);
                Assert.AreEqual(expectedResults.Description, courseLevel.Title);
            }

            [TestMethod]
            public async Task CurriculumService_GetCourseLevelById2_Properties()
            {
                var expectedResults = courseLevelCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var courseLevel = await curriculumService.GetCourseLevelById2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(courseLevel.Id);
                Assert.IsNotNull(courseLevel.Code);
                Assert.IsNotNull(courseLevel.Title);
            }
        }

        [TestClass]
        public class GetCreditCategoriesV6
        {
            private Mock<IStudentReferenceDataRepository> studentReferenceRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ILogger logger;
            private CurriculumService curriculumService;
            private ICollection<Domain.Student.Entities.CreditCategory> creditCategoryCollection = new List<Domain.Student.Entities.CreditCategory>();
            private List<Dtos.CreditCategory3> creditCategoryDtos = new List<Dtos.CreditCategory3>();

            [TestInitialize]
            public void Initialize()
            {
                studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                logger = new Mock<ILogger>().Object;

                BuildData();
                curriculumService = new CurriculumService(studentReferenceRepositoryMock.Object, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            private void BuildData()
            {
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "I", "Institutional", Domain.Student.Entities.CreditType.Institutional));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("d1643b94-bfff-445e-9c28-516999d4e9da", "O", "Transfer Credit", Domain.Student.Entities.CreditType.Other));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("3ff53d46-9cee-4975-86fe-3a4250998342", "E", "Transfer Credit", Domain.Student.Entities.CreditType.Exchange));
                creditCategoryCollection.Add(new Domain.Student.Entities.CreditCategory("11f0059c-b04c-467c-acf1-3db0ba955054", "N", "Transfer Credit", Domain.Student.Entities.CreditType.None));

                foreach (var creditCategory in creditCategoryCollection)
                {
                    Dtos.CreditCategory3 target = new CreditCategory3();
                    target.Id = creditCategory.Guid;
                    target.Code = creditCategory.Code;
                    target.Title = creditCategory.Description;
                    target.CreditType = (creditCategory.CreditType == Domain.Student.Entities.CreditType.None) ? Dtos.EnumProperties.CreditCategoryType3.NoCredit :
                                                                    (Dtos.EnumProperties.CreditCategoryType3)Enum.Parse(typeof(Domain.Student.Entities.CreditType), creditCategory.CreditType.ToString());
                    creditCategoryDtos.Add(target);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                creditCategoryCollection = null;
                curriculumService = null;
                creditCategoryDtos = null;
            }
           
            [TestMethod]
            public async Task CurriculumService_CreditCategories3_GetAll()
            {
                studentReferenceRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(creditCategoryCollection);
                var results = await curriculumService.GetCreditCategories3Async(It.IsAny<bool>());

                Assert.IsNotNull(results);

                foreach (var result in results)
                {
                    var expected = creditCategoryCollection.FirstOrDefault(i => i.Guid.Equals(result.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Code, result.Code);
                    if (expected.CreditType == Domain.Student.Entities.CreditType.None) 
                    {
                        Assert.AreEqual(result.CreditType, Dtos.EnumProperties.CreditCategoryType3.NoCredit);
                    }
                    else
                    {
                        Assert.AreEqual(expected.CreditType.ToString(), result.CreditType.Value.ToString());
                    }
                    Assert.AreEqual(expected.Guid, result.Id);
                    Assert.AreEqual(expected.Description, result.Title);
                }
            }


            [TestMethod]
            public async Task CurriculumService_CreditCategories3_GetById()
            {
                string id = "11f0059c-b04c-467c-acf1-3db0ba955054";
                var expected = creditCategoryCollection.FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));

                studentReferenceRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(creditCategoryCollection);
                var result = await curriculumService.GetCreditCategoryByGuid3Async(id);

                Assert.IsNotNull(result);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Code, result.Code);
                if (expected.CreditType == Domain.Student.Entities.CreditType.None)
                {
                    Assert.AreEqual(result.CreditType, Dtos.EnumProperties.CreditCategoryType3.NoCredit);
                }
                else
                {
                    Assert.AreEqual(expected.CreditType.ToString(), result.CreditType.Value.ToString());
                }
                Assert.AreEqual(expected.Guid, result.Id);
                Assert.AreEqual(expected.Description, result.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CurriculumService_CreditCategories3_GetById_Null_Id()
            {
                var result = await curriculumService.GetCreditCategoryByGuid3Async(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_CreditCategories3_GetById_InvalidOperationException()
            {
                var result = await curriculumService.GetCreditCategoryByGuid3Async("1234");
            }
        }

        [TestClass]
        public class GetEnrollmentStatuses
        {
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ILogger logger;
            private CurriculumService curriculumService;
            private ICollection<Domain.Student.Entities.EnrollmentStatus> enrollmentStatusCollection = new List<Domain.Student.Entities.EnrollmentStatus>();

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                logger = new Mock<ILogger>().Object;

                enrollmentStatusCollection.Add(new Domain.Student.Entities.EnrollmentStatus("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "A", "Active", Domain.Student.Entities.EnrollmentStatusType.active));
                enrollmentStatusCollection.Add(new Domain.Student.Entities.EnrollmentStatus("73244057-D1EC-4094-A0B7-DE602533E3A6", "W", "Withdrawn", Domain.Student.Entities.EnrollmentStatusType.inactive));
                enrollmentStatusCollection.Add(new Domain.Student.Entities.EnrollmentStatus("1df164eb-8178-4321-a9f7-24f12d3991d8", "G", "Graduated", Domain.Student.Entities.EnrollmentStatusType.complete));
                referenceRepositoryMock.Setup(repo => repo.GetEnrollmentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(enrollmentStatusCollection);

                curriculumService = new CurriculumService(referenceRepository, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                enrollmentStatusCollection = null;
                referenceRepository = null;
                curriculumService = null;
            }

            [TestMethod]
            public async Task CurriculumService__EnrollmentStatuses()
            {
                var results = await curriculumService.GetEnrollmentStatusesAsync();
                Assert.IsTrue(results is IEnumerable<EnrollmentStatus>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task CurriculumService_EnrollmentStatuses_Count()
            {
                var results = await curriculumService.GetEnrollmentStatusesAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CurriculumService_EnrollmentStatuses_Properties()
            {
                var results = await curriculumService.GetEnrollmentStatusesAsync();
                var enrollmentStatus = results.Where(x => x.Code == "A").First();
                Assert.IsNotNull(enrollmentStatus.Id);
                Assert.IsNotNull(enrollmentStatus.Code);
                Assert.IsNotNull(enrollmentStatus.enrollmentStatusType);
            }

            [TestMethod]
            public async Task CurriculumService_EnrollmentStatuses_Expected()
            {
                var expectedResults = enrollmentStatusCollection.Where(c => c.Code == "A").First();
                var results = await curriculumService.GetEnrollmentStatusesAsync();
                var enrollmentStatus = results.Where(s => s.Code == "A").First();
                Assert.AreEqual(expectedResults.Guid, enrollmentStatus.Id);
                Assert.AreEqual(expectedResults.Code, enrollmentStatus.Code);
                Assert.AreEqual(expectedResults.EnrollmentStatusType.ToString().ToUpper(), enrollmentStatus.enrollmentStatusType.ToString().ToUpper());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetEnrollmentStatusByGuid_Empty()
            {
                await curriculumService.GetEnrollmentStatusByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetEnrollmentStatusByGuid_Null()
            {
                await curriculumService.GetEnrollmentStatusByGuidAsync(null);
            }

            [TestMethod]
            public async Task CurriculumService_GetEnrollmentStatusByGuid_Expected()
            {
                var expectedResults = enrollmentStatusCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").First();
                var enrollmentStatus = await curriculumService.GetEnrollmentStatusByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, enrollmentStatus.Id);
                Assert.AreEqual(expectedResults.Code, enrollmentStatus.Code);
                Assert.AreEqual(expectedResults.EnrollmentStatusType.ToString().ToUpper(), enrollmentStatus.enrollmentStatusType.ToString().ToUpper());
            }

            [TestMethod]
            public async Task CurriculumService_GetEnrollmentStatusByGuid_Properties()
            {
                var expectedResults = enrollmentStatusCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").First();
                var enrollmentStatus = await curriculumService.GetEnrollmentStatusByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(enrollmentStatus.Id);
                Assert.IsNotNull(enrollmentStatus.Code);
                Assert.IsNotNull(enrollmentStatus.enrollmentStatusType.ToString());
            }
        }

        //[TestClass]
        //public class GetFinancialAidAwardPeriods
        //{
        //    private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
        //    private IStudentReferenceDataRepository referenceRepository;
        //    private ILogger logger;
        //    private CurriculumService curriculumService;
        //    private ICollection<Domain.Student.Entities.FinancialAidAwardPeriod> financialAidAwardPeriodCollection = new List<Domain.Student.Entities.FinancialAidAwardPeriod>();

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
        //        referenceRepository = referenceRepositoryMock.Object;
        //        logger = new Mock<ILogger>().Object;

        //        financialAidAwardPeriodCollection.Add(new Domain.Student.Entities.FinancialAidAwardPeriod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CODE1", "DESC1", "STATUS1"));
        //        financialAidAwardPeriodCollection.Add(new Domain.Student.Entities.FinancialAidAwardPeriod("73244057-D1EC-4094-A0B7-DE602533E3A6", "CODE2", "DESC2", "STATUS2"));
        //        financialAidAwardPeriodCollection.Add(new Domain.Student.Entities.FinancialAidAwardPeriod("1df164eb-8178-4321-a9f7-24f12d3991d8", "CODE3", "DESC3", "STATUS3"));
        //        referenceRepositoryMock.Setup(repo => repo.GetFinancialAidAwardPeriodsAsync(true)).ReturnsAsync(financialAidAwardPeriodCollection);
        //        referenceRepositoryMock.Setup(repo => repo.GetFinancialAidAwardPeriodsAsync(false)).ReturnsAsync(financialAidAwardPeriodCollection);

        //        curriculumService = new CurriculumService(referenceRepository, logger);
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        financialAidAwardPeriodCollection = null;
        //        referenceRepository = null;
        //        curriculumService = null;
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService__FinancialAidAwardPeriods()
        //    {
        //        var results = await curriculumService.GetFinancialAidAwardPeriodsAsync();
        //        Assert.IsTrue(results is IEnumerable<FinancialAidAwardPeriod>);
        //        Assert.IsNotNull(results);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_FinancialAidAwardPeriods_Count()
        //    {
        //        var results = await curriculumService.GetFinancialAidAwardPeriodsAsync();
        //        Assert.AreEqual(3, results.Count());
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_FinancialAidAwardPeriods_Properties()
        //    {
        //        var results = await curriculumService.GetFinancialAidAwardPeriodsAsync();
        //        var financialAidAwardPeriod = results.Where(x => x.Code == "CODE1").FirstOrDefault();
        //        Assert.IsNotNull(financialAidAwardPeriod.Id);
        //        Assert.IsNotNull(financialAidAwardPeriod.Code);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_FinancialAidAwardPeriods_Expected()
        //    {
        //        var expectedResults = financialAidAwardPeriodCollection.Where(c => c.Code == "CODE2").FirstOrDefault();
        //        var results = await curriculumService.GetFinancialAidAwardPeriodsAsync();
        //        var financialAidAwardPeriod = results.Where(s => s.Code == "CODE2").FirstOrDefault();
        //        Assert.AreEqual(expectedResults.Guid, financialAidAwardPeriod.Id);
        //        Assert.AreEqual(expectedResults.Code, financialAidAwardPeriod.Code);
        //    }


        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task CurriculumService_GetFinancialAidAwardPerioddByGuid_Empty()
        //    {
        //        await curriculumService.GetFinancialAidAwardPeriodByGuidAsync("");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task CurriculumService_GetFinancialAidAwardPeriodByGuid_Null()
        //    {
        //        await curriculumService.GetFinancialAidAwardPeriodByGuidAsync(null);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_GetFinancialAidAwardPeriodByGuid_Expected()
        //    {
        //        var expectedResults = financialAidAwardPeriodCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
        //        var financialAidAwardPeriod = await curriculumService.GetFinancialAidAwardPeriodByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
        //        Assert.AreEqual(expectedResults.Guid, financialAidAwardPeriod.Id);
        //        Assert.AreEqual(expectedResults.Code, financialAidAwardPeriod.Code);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_GetFinancialAidAwardPeriodByGuid_Properties()
        //    {
        //        var expectedResults = financialAidAwardPeriodCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
        //        var financialAidAwardPeriod = await curriculumService.GetFinancialAidAwardPeriodByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
        //        Assert.IsNotNull(financialAidAwardPeriod.Id);
        //        Assert.IsNotNull(financialAidAwardPeriod.Code);
        //    }

        //    //[TestMethod]
        //    //public async Task CurriculumService__FinancialAidAwardPeriods2()
        //    //{
        //    //    var results = await curriculumService.GetFinancialAidAwardPeriods2Async();
        //    //    Assert.IsTrue(results is IEnumerable<AcademicLevel2>);
        //    //    Assert.IsNotNull(results);
        //    //}

        //    //public async Task CurriculumService_AcademicLevels2_Count()
        //    //{
        //    //    var results = await curriculumService.GetAcademicLevels2Async();
        //    //    Assert.AreEqual(3, results.Count());
        //    //}

        //    //[TestMethod]
        //    //public async Task CurriculumService_AcademicLevels2_Properties()
        //    //{
        //    //    var results = await curriculumService.GetAcademicLevels2Async();
        //    //    var academicLevel = results.Where(x => x.Code == "CE").FirstOrDefault();
        //    //    Assert.IsNotNull(academicLevel.Id);
        //    //    Assert.IsNotNull(academicLevel.Code);
        //    //    //Assert.IsNotNull(instructionalMethod.CreditType);
        //    //}

        //    //[TestMethod]
        //    //public async Task CurriculumService_AcademicLevels2_Expected()
        //    //{
        //    //    var expectedResults = financialAidAwardPeriodCollection.Where(c => c.Code == "CE").FirstOrDefault();
        //    //    var results = await curriculumService.GetAcademicLevels2Async();
        //    //    var academicLevel = results.Where(s => s.Code == "CE").FirstOrDefault();
        //    //    Assert.AreEqual(expectedResults.Guid, academicLevel.Id);
        //    //    Assert.AreEqual(expectedResults.Code, academicLevel.Code);
        //    //    //Assert.AreEqual(expectedResults.CreditType.ToString(), creditCategory.CreditType.ToString());
        //    //}


        //    //[TestMethod]
        //    //[ExpectedException(typeof(KeyNotFoundException))]
        //    //public async Task CurriculumService_GetAcademicLevelById2_Empty()
        //    //{
        //    //    await curriculumService.GetAcademicLevelById2Async("");
        //    //}

        //    //[TestMethod]
        //    //[ExpectedException(typeof(KeyNotFoundException))]
        //    //public async Task CurriculumService_GetAcademicLevelById2_Null()
        //    //{
        //    //    await curriculumService.GetAcademicLevelById2Async(null);
        //    //}

        //    //[TestMethod]
        //    //public async Task CurriculumService_GetAcademicLevelById2_Expected()
        //    //{
        //    //    var expectedResults = financialAidAwardPeriodCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
        //    //    var academicLevel = await curriculumService.GetAcademicLevelById2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
        //    //    Assert.AreEqual(expectedResults.Guid, academicLevel.Id);
        //    //    Assert.AreEqual(expectedResults.Code, academicLevel.Code);
        //    //    //Assert.AreEqual(expectedResults.CreditType.ToString(), instructionalMethod.CreditType.ToString());
        //    //}

        //    //[TestMethod]
        //    //public async Task CurriculumService_GetAcademicLevelById2_Properties()
        //    //{
        //    //    var expectedResults = financialAidAwardPeriodCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
        //    //    var academicLevel = await curriculumService.GetAcademicLevelById2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
        //    //    Assert.IsNotNull(academicLevel.Id);
        //    //    Assert.IsNotNull(academicLevel.Code);
        //    //    //Assert.IsNotNull(instructionalMethod.CreditType.ToString());
        //    //}
        //}

        //[TestClass]
        //public class GetFinancialAidFundCategories
        //{
        //    private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
        //    private IStudentReferenceDataRepository referenceRepository;
        //    private ILogger logger;
        //    private CurriculumService curriculumService;
        //    private ICollection<Domain.Student.Entities.FinancialAidFundCategory> financialAidFundCategoryCollection = new List<Domain.Student.Entities.FinancialAidFundCategory>();

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
        //        referenceRepository = referenceRepositoryMock.Object;
        //        logger = new Mock<ILogger>().Object;

        //        financialAidFundCategoryCollection.Add(new Domain.Student.Entities.FinancialAidFundCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CODE1", "DESC1"));
        //        financialAidFundCategoryCollection.Add(new Domain.Student.Entities.FinancialAidFundCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "CODE2", "DESC2"));
        //        financialAidFundCategoryCollection.Add(new Domain.Student.Entities.FinancialAidFundCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "CODE3", "DESC3"));
        //        referenceRepositoryMock.Setup(repo => repo.GetFinancialAidFundCategoriesAsync(true)).ReturnsAsync(financialAidFundCategoryCollection);
        //        referenceRepositoryMock.Setup(repo => repo.GetFinancialAidFundCategoriesAsync(false)).ReturnsAsync(financialAidFundCategoryCollection);

        //        curriculumService = new CurriculumService(referenceRepository, logger);
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        financialAidFundCategoryCollection = null;
        //        referenceRepository = null;
        //        curriculumService = null;
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService__FinancialAidFundCategories()
        //    {
        //        var results = await curriculumService.GetFinancialAidFundCategoriesAsync();
        //        Assert.IsTrue(results is IEnumerable<FinancialAidFundCategory>);
        //        Assert.IsNotNull(results);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_FinancialAidFundCategories_Count()
        //    {
        //        var results = await curriculumService.GetFinancialAidFundCategoriesAsync();
        //        Assert.AreEqual(3, results.Count());
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_FinancialAidFundCategories_Properties()
        //    {
        //        var results = await curriculumService.GetFinancialAidFundCategoriesAsync();
        //        var financialAidFundCategory = results.Where(x => x.Code == "CODE1").FirstOrDefault();
        //        Assert.IsNotNull(financialAidFundCategory.Id);
        //        Assert.IsNotNull(financialAidFundCategory.Code);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_FinancialAidFundCategories_Expected()
        //    {
        //        var expectedResults = financialAidFundCategoryCollection.Where(c => c.Code == "CODE2").FirstOrDefault();
        //        var results = await curriculumService.GetFinancialAidFundCategoriesAsync();
        //        var financialAidFundCategory = results.Where(s => s.Code == "CODE2").FirstOrDefault();
        //        Assert.AreEqual(expectedResults.Guid, financialAidFundCategory.Id);
        //        Assert.AreEqual(expectedResults.Code, financialAidFundCategory.Code);
        //    }


        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task CurriculumService_GetFinancialAidFundCategoryByGuid_Empty()
        //    {
        //        await curriculumService.GetFinancialAidFundCategoryByGuidAsync("");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task CurriculumService_GetFinancialAidFundCategoryByGuid_Null()
        //    {
        //        await curriculumService.GetFinancialAidFundCategoryByGuidAsync(null);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_GetFinancialAidFundCategoryByGuid_Expected()
        //    {
        //        var expectedResults = financialAidFundCategoryCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
        //        var financialAidFundCategory = await curriculumService.GetFinancialAidFundCategoryByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
        //        Assert.AreEqual(expectedResults.Guid, financialAidFundCategory.Id);
        //        Assert.AreEqual(expectedResults.Code, financialAidFundCategory.Code);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_GetFinancialAidFundCategoryByGuid_Properties()
        //    {
        //        var expectedResults = financialAidFundCategoryCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
        //        var financialAidFundCategory = await curriculumService.GetFinancialAidFundCategoryByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
        //        Assert.IsNotNull(financialAidFundCategory.Id);
        //        Assert.IsNotNull(financialAidFundCategory.Code);
        //    }
        //}

        //[TestClass]
        //public class GetFinancialAidYears
        //{
        //    private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
        //    private IStudentReferenceDataRepository referenceRepository;
        //    private ILogger logger;
        //    private CurriculumService curriculumService;
        //    private ICollection<Domain.Student.Entities.FinancialAidYear> financialAidYearCollection = new List<Domain.Student.Entities.FinancialAidYear>();

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
        //        referenceRepository = referenceRepositoryMock.Object;
        //        logger = new Mock<ILogger>().Object;

        //        financialAidYearCollection.Add(new Domain.Student.Entities.FinancialAidYear("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2001", "CODE1", "STATUS1") { HostCountry = "USA" } );
        //        financialAidYearCollection.Add(new Domain.Student.Entities.FinancialAidYear("73244057-D1EC-4094-A0B7-DE602533E3A6", "2002", "CODE2", "STATUS2") { HostCountry = "CAN", status = "D" } );
        //        financialAidYearCollection.Add(new Domain.Student.Entities.FinancialAidYear("1df164eb-8178-4321-a9f7-24f12d3991d8", "2003", "CODE3", "STATUS3") { HostCountry = "USA" } );
        //        referenceRepositoryMock.Setup(repo => repo.GetFinancialAidYearsAsync(true)).ReturnsAsync(financialAidYearCollection);
        //        referenceRepositoryMock.Setup(repo => repo.GetFinancialAidYearsAsync(false)).ReturnsAsync(financialAidYearCollection);

        //        curriculumService = new CurriculumService(referenceRepository, logger);
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        financialAidYearCollection = null;
        //        referenceRepository = null;
        //        curriculumService = null;
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService__FinancialAidYears()
        //    {
        //        var results = await curriculumService.GetFinancialAidYearsAsync();
        //        Assert.IsTrue(results is IEnumerable<FinancialAidYear>);
        //        Assert.IsNotNull(results);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_FinancialAidYears_Count()
        //    {
        //        var results = await curriculumService.GetFinancialAidYearsAsync();
        //        Assert.AreEqual(3, results.Count());
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_FinancialAidYears_Properties()
        //    {
        //        var results = await curriculumService.GetFinancialAidYearsAsync();
        //        var financialAidYear = results.Where(x => x.Code == "2001").FirstOrDefault();
        //        Assert.IsNotNull(financialAidYear.Id);
        //        Assert.IsNotNull(financialAidYear.Code);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_FinancialAidYears_Expected()
        //    {
        //        var expectedResults = financialAidYearCollection.Where(c => c.Code == "2002").FirstOrDefault();
        //        var results = await curriculumService.GetFinancialAidYearsAsync();
        //        var financialAidYear = results.Where(s => s.Code == "2002").FirstOrDefault();
        //        Assert.AreEqual(expectedResults.Guid, financialAidYear.Id);
        //        Assert.AreEqual(expectedResults.Code, financialAidYear.Code);
        //    }


        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task CurriculumService_GetFinancialAidYeardByGuid_Empty()
        //    {
        //        await curriculumService.GetFinancialAidYearByGuidAsync("");
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(KeyNotFoundException))]
        //    public async Task CurriculumService_GetFinancialAidYearByGuid_Null()
        //    {
        //        await curriculumService.GetFinancialAidYearByGuidAsync(null);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_GetFinancialAidYearByGuid_Expected()
        //    {
        //        var expectedResults = financialAidYearCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
        //        var financialAidYear = await curriculumService.GetFinancialAidYearByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
        //        Assert.AreEqual(expectedResults.Guid, financialAidYear.Id);
        //        Assert.AreEqual(expectedResults.Code, financialAidYear.Code);
        //    }

        //    [TestMethod]
        //    public async Task CurriculumService_GetFinancialAidYearByGuid_Properties()
        //    {
        //        var expectedResults = financialAidYearCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
        //        var financialAidYear = await curriculumService.GetFinancialAidYearByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
        //        Assert.IsNotNull(financialAidYear.Id);
        //        Assert.IsNotNull(financialAidYear.Code);
        //    }
        //}

        [TestClass]
        public class GetInstructionalMethods
        {
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ILogger logger;
            private CurriculumService curriculumService;
            private ICollection<Domain.Student.Entities.InstructionalMethod> instructionalMethodCollection = new List<Domain.Student.Entities.InstructionalMethod>();

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                logger = new Mock<ILogger>().Object;

                instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "LG", "null", false));
                instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("73244057-D1EC-4094-A0B7-DE602533E3A6", "30", "null", true));
                instructionalMethodCollection.Add(new Domain.Student.Entities.InstructionalMethod("1df164eb-8178-4321-a9f7-24f12d3991d8", "04", "null", false));
                referenceRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(instructionalMethodCollection);
                referenceRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(true)).ReturnsAsync(instructionalMethodCollection);
                referenceRepositoryMock.Setup(repo => repo.GetInstructionalMethodsAsync(false)).ReturnsAsync(instructionalMethodCollection);

                curriculumService = new CurriculumService(referenceRepository, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalMethodCollection = null;
                referenceRepository = null;
                curriculumService = null;
            }

            [TestMethod]
            public async Task CurriculumService__InstructionalMethods2()
            {
                var results = await curriculumService.GetInstructionalMethods2Async();
                Assert.IsTrue(results is IEnumerable<InstructionalMethod2>);
                Assert.IsNotNull(results);
            }

            public async Task CurriculumService_InstructionalMethods2_Count()
            {
                var results = await curriculumService.GetInstructionalMethods2Async();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CurriculumService_InstructionalMethods2_Properties()
            {
                var results = await curriculumService.GetInstructionalMethods2Async();
                var instructionalMethod = results.Where(x => x.Code == "LG").FirstOrDefault();
                Assert.IsNotNull(instructionalMethod.Id);
                Assert.IsNotNull(instructionalMethod.Code);
                //Assert.IsNotNull(instructionalMethod.CreditType);
            }

            [TestMethod]
            public async Task CurriculumService_InstructionalMethods2_Expected()
            {
                var expectedResults = instructionalMethodCollection.Where(c => c.Code == "LG").FirstOrDefault();
                var results = await curriculumService.GetInstructionalMethods2Async();
                var instructionalMethod = results.Where(s => s.Code == "LG").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, instructionalMethod.Id);
                Assert.AreEqual(expectedResults.Code, instructionalMethod.Code);
                //Assert.AreEqual(expectedResults.CreditType.ToString(), creditCategory.CreditType.ToString());
            }


            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CurriculumService_GetInstructionalMethodById2_Empty()
            {
                await curriculumService.GetInstructionalMethodById2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task CurriculumService_GetInstructionalMethodById2_Null()
            {
                await curriculumService.GetInstructionalMethodById2Async(null);
            }

            [TestMethod]
            public async Task CurriculumService_GetInstructionalMethodById2_Expected()
            {
                var expectedResults = instructionalMethodCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var instructionalMethod = await curriculumService.GetInstructionalMethodById2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, instructionalMethod.Id);
                Assert.AreEqual(expectedResults.Code, instructionalMethod.Code);
                //Assert.AreEqual(expectedResults.CreditType.ToString(), instructionalMethod.CreditType.ToString());
            }

            [TestMethod]
            public async Task CurriculumService_GetInstructionalMethodById2_Properties()
            {
                var expectedResults = instructionalMethodCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var instructionalMethod = await curriculumService.GetInstructionalMethodById2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(instructionalMethod.Id);
                Assert.IsNotNull(instructionalMethod.Code);
                //Assert.IsNotNull(instructionalMethod.CreditType.ToString());
            }
        }

        [TestClass]
        public class GetSectionGradeTypes
        {
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ILogger logger;
            private CurriculumService curriculumService;
            private ICollection<Domain.Student.Entities.SectionGradeType> sectionGradeTypeCollection = new List<Domain.Student.Entities.SectionGradeType>();

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                logger = new Mock<ILogger>().Object;

                sectionGradeTypeCollection.Add(new Domain.Student.Entities.SectionGradeType("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "MID1", "Midterm Grade 1"));
                sectionGradeTypeCollection.Add(new Domain.Student.Entities.SectionGradeType("73244057-D1EC-4094-A0B7-DE602533E3A6", "MID2", "Midterm Grade 2"));
                sectionGradeTypeCollection.Add(new Domain.Student.Entities.SectionGradeType("1df164eb-8178-4321-a9f7-24f12d3991d8", "FINAL", "Final Grade"));
                referenceRepositoryMock.Setup(repo => repo.GetSectionGradeTypesAsync()).ReturnsAsync(sectionGradeTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetSectionGradeTypesAsync(true)).ReturnsAsync(sectionGradeTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetSectionGradeTypesAsync(false)).ReturnsAsync(sectionGradeTypeCollection);

                curriculumService = new CurriculumService(referenceRepository, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionGradeTypeCollection = null;
                referenceRepository = null;
                curriculumService = null;
            }

            [TestMethod]
            public async Task CurriculumService__SectionGradeTypes()
            {
                var results = await curriculumService.GetSectionGradeTypesAsync();
                Assert.IsTrue(results is IEnumerable<SectionGradeType>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task CurriculumService_SectionGradeTypes_Count()
            {
                var results = await curriculumService.GetSectionGradeTypesAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CurriculumService_SectionGradeTypes_Properties()
            {
                var results = await curriculumService.GetSectionGradeTypesAsync();
                var sectionGradeType = results.Where(x => x.Code == "MID1").FirstOrDefault();
                Assert.IsNotNull(sectionGradeType.Id);
                Assert.IsNotNull(sectionGradeType.Code);
            }

            [TestMethod]
            public async Task CurriculumService_SectionGradeTypes_Expected()
            {
                var expectedResults = sectionGradeTypeCollection.Where(c => c.Code == "MID1").FirstOrDefault();
                var results = await curriculumService.GetSectionGradeTypesAsync();
                var sectionGradeType = results.Where(s => s.Code == "MID1").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, sectionGradeType.Id);
                Assert.AreEqual(expectedResults.Code, sectionGradeType.Code);
                //Assert.AreEqual(expectedResults.CreditType.ToString(), instructionalMethod.CreditType.ToString());
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetSectionGradeTypeByGuid_Empty()
            {
                await curriculumService.GetSectionGradeTypeByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetSectionGradeTypeByGuid_Null()
            {
                await curriculumService.GetSectionGradeTypeByGuidAsync(null);
            }

            [TestMethod]
            public async Task CurriculumService_GetSectionGradeTypeByGuid_Expected()
            {
                var expectedResults = sectionGradeTypeCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var sectionGradeType = await curriculumService.GetSectionGradeTypeByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, sectionGradeType.Id);
                Assert.AreEqual(expectedResults.Code, sectionGradeType.Code);
                //Assert.AreEqual(expectedResults.CreditType.ToString(), instructionalMethod.CreditType.ToString());
            }

            [TestMethod]
            public async Task CurriculumService_GetSectionGradeTypeByGuid_Properties()
            {
                var expectedResults = sectionGradeTypeCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var sectionGradeType = await curriculumService.GetSectionGradeTypeByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(sectionGradeType.Id);
                Assert.IsNotNull(sectionGradeType.Code);
                //Assert.IsNotNull(creditCategory.CreditType.ToString());
            }
        }

        [TestClass]
        public class GetSectionRegistrationStatuses
        {
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ILogger logger;
            private CurriculumService curriculumService;
            private ICollection<Domain.Student.Entities.SectionRegistrationStatus> sectionRegistrationStatusCollection = new List<Domain.Student.Entities.SectionRegistrationStatus>();
            private ICollection<Domain.Student.Entities.SectionRegistrationStatusItem> sectionRegistrationStatusItemCollection = new List<Domain.Student.Entities.SectionRegistrationStatusItem>();

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                logger = new Mock<ILogger>().Object;
                /*
                var sectionRegistrationStatus1 = new SectionRegistrationStatus()
                {
                    RegistrationStatus = RegistrationStatus.NotRegistered,
                    SectionRegistrationStatusReason = RegistrationStatusReason.Dropped
                };*/
                /*
                sectionRegistrationStatusCollection.Add(new Domain.Student.Entities.SectionRegistrationStatus() { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Dropped });
                sectionRegistrationStatusCollection.Add(new Domain.Student.Entities.SectionRegistrationStatus() { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Withdrawn });
                sectionRegistrationStatusCollection.Add(new Domain.Student.Entities.SectionRegistrationStatus() { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.Registered, SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Registered });
                */
                sectionRegistrationStatusItemCollection.Add(new Domain.Student.Entities.SectionRegistrationStatusItem("73244057-d1ec-4094-a0b7-de602533e3a6", "D", "Dropped") { Status = new Domain.Student.Entities.SectionRegistrationStatus() { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Dropped } });
                sectionRegistrationStatusItemCollection.Add(new Domain.Student.Entities.SectionRegistrationStatusItem("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer") { Status = new Domain.Student.Entities.SectionRegistrationStatus() { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.Registered, SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Registered } });
                sectionRegistrationStatusItemCollection.Add(new Domain.Student.Entities.SectionRegistrationStatusItem("4af374ab-8908-4091-a7k7-24i02d9931d8", "W", "Withdraw") { Status = new Domain.Student.Entities.SectionRegistrationStatus() { RegistrationStatus = Domain.Student.Entities.RegistrationStatus.NotRegistered, SectionRegistrationStatusReason = Domain.Student.Entities.RegistrationStatusReason.Withdrawn } });

                /*
                sectionRegistrationStatusCollection.Add(new Domain.Student.Entities.CreditCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "C", "Continuing Education", Domain.Student.Entities.CreditType.ContinuingEducation));
                sectionRegistrationStatusCollection.Add(new Domain.Student.Entities.CreditCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "T", "Transfer Credit", Domain.Student.Entities.CreditType.Transfer));
               */
                //var sectionRegistrationStatusEntities = await _studentReferenceDataRepository.GetSectionRegistrationStatusesAsync(bypassCache);
                referenceRepositoryMock.Setup(repo => repo.SectionRegistrationStatusesAsync()).ReturnsAsync(sectionRegistrationStatusItemCollection);                
                referenceRepositoryMock.Setup(repo => repo.GetStudentAcademicCreditStatusesAsync(false)).ReturnsAsync(sectionRegistrationStatusItemCollection);
                referenceRepositoryMock.Setup(repo => repo.GetStudentAcademicCreditStatusesAsync(true)).ReturnsAsync(sectionRegistrationStatusItemCollection);

                /*
                referenceRepositoryMock.Setup(repo => repo.GetSectionRegistrationStatusesAsync(false)).ReturnsAsync(sectionRegistrationStatusCollection);
                referenceRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(true)).ReturnsAsync(sectionRegistrationStatusCollection);
                referenceRepositoryMock.Setup(repo => repo.GetCreditCategoriesAsync(false)).ReturnsAsync(sectionRegistrationStatusCollection);
                */
                curriculumService = new CurriculumService(referenceRepository, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationStatusCollection = null;
                referenceRepository = null;
                curriculumService = null;
            }

            [TestMethod]
            public async Task CurriculumService_GetSectionRegistrationStatusByID_Properties()
            {
                var expectedResults = sectionRegistrationStatusItemCollection.Where(c => c.Guid == "73244057-d1ec-4094-a0b7-de602533e3a6").FirstOrDefault();
                var sectionRegistrationStatus = await curriculumService.GetSectionRegistrationStatusById2Async("73244057-d1ec-4094-a0b7-de602533e3a6");
                Assert.IsNotNull(sectionRegistrationStatus.Id);
                Assert.IsNotNull(sectionRegistrationStatus.Code);
                Assert.IsNotNull(sectionRegistrationStatus.Status.RegistrationStatus.ToString());
                Assert.IsNotNull(sectionRegistrationStatus.Status.SectionRegistrationStatusReason.ToString());
            }
            
            [TestMethod]
            public async Task CurriculumService__SectionRegistrationStatus2()
            {
                var results = await curriculumService.GetSectionRegistrationStatuses2Async();
                Assert.IsTrue(results is IEnumerable<SectionRegistrationStatusItem2>);
                Assert.IsNotNull(results);
            }
            
            public async Task CurriculumService_SectionRegistrationStatus2_Count()
            {
                var results = await curriculumService.GetSectionRegistrationStatuses2Async();
                Assert.AreEqual(3, results.Count());
            }
            
            [TestMethod]
            public async Task CurriculumService_SectionRegistrationStatus2_Properties()
            {
                var results = await curriculumService.GetSectionRegistrationStatuses2Async();
                var sectionRegistrationStatus = results.Where(x => x.Code == "D").FirstOrDefault();
                Assert.IsNotNull(sectionRegistrationStatus.Id);
                Assert.IsNotNull(sectionRegistrationStatus.Code);
                Assert.IsNotNull(sectionRegistrationStatus.Status);
            }
            
            [TestMethod]
            public async Task CurriculumService_SectionRegistrationStatus2_Expected()
            {
                var expectedResults = sectionRegistrationStatusItemCollection.Where(c => c.Code == "D").FirstOrDefault();
                var results = await curriculumService.GetSectionRegistrationStatuses2Async(false);
                var sectionRegistrationStatus = results.Where(s => s.Code == "D").FirstOrDefault();

                Assert.AreEqual(expectedResults.Guid, sectionRegistrationStatus.Id);
                Assert.AreEqual(expectedResults.Code, sectionRegistrationStatus.Code);
                Assert.AreEqual(expectedResults.Status.RegistrationStatus.ToString(), sectionRegistrationStatus.Status.RegistrationStatus.ToString());
                Assert.AreEqual(expectedResults.Status.SectionRegistrationStatusReason.ToString(), sectionRegistrationStatus.Status.SectionRegistrationStatusReason.ToString());
            }

            [TestMethod]
            public async Task CurriculumService_SectionRegistrationStatus3()
            {
                //var expectedResults = sectionRegistrationStatusItemCollection.Where(c => c.Code == "D").FirstOrDefault();
                var actuals = await curriculumService.GetSectionRegistrationStatuses3Async(false);

                foreach (var actual in actuals)
                {
                    var expected = sectionRegistrationStatusItemCollection.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Guid, actual.Id);
                    if (expected.Status.RegistrationStatus == Domain.Student.Entities.RegistrationStatus.Registered)
                    {
                        Assert.AreEqual(HeadCountStatus.Include, actual.HeadCountStatus.Value);
                    }
                    else
                    {
                        Assert.AreEqual(HeadCountStatus.Exclude, actual.HeadCountStatus.Value);
                    }
                    Assert.AreEqual((RegistrationStatusReason2) Enum.Parse(typeof(RegistrationStatusReason2), expected.Status.SectionRegistrationStatusReason.ToString()), actual.Status.SectionRegistrationStatusReason);
                }
            }

            
            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetSectionRegistrationStatusById2_Empty()
            {
                await curriculumService.GetSectionRegistrationStatusById2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetSectionRegistrationStatusById2_Null()
            {
                await curriculumService.GetSectionRegistrationStatusById2Async(null);
            }

            [TestMethod]
            public async Task CurriculumService_GetSectionRegistrationStatusById2_Expected()
            {
                var expectedResults = sectionRegistrationStatusItemCollection.Where(c => c.Guid == "73244057-d1ec-4094-a0b7-de602533e3a6").FirstOrDefault();
                var sectionRegistrationStatus = await curriculumService.GetSectionRegistrationStatusById2Async("73244057-d1ec-4094-a0b7-de602533e3a6");
                Assert.AreEqual(expectedResults.Guid, sectionRegistrationStatus.Id);
                Assert.AreEqual(expectedResults.Code, sectionRegistrationStatus.Code);
                Assert.AreEqual(expectedResults.Status.SectionRegistrationStatusReason.ToString(), sectionRegistrationStatus.Status.SectionRegistrationStatusReason.ToString());
            }

            [TestMethod]
            public async Task CurriculumService_GetSectionRegistrationStatusById3_Expected()
            {
                var expected = sectionRegistrationStatusItemCollection.Where(c => c.Guid == "73244057-d1ec-4094-a0b7-de602533e3a6").FirstOrDefault();
                var actual = await curriculumService.GetSectionRegistrationStatusById3Async("73244057-d1ec-4094-a0b7-de602533e3a6");
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Status.SectionRegistrationStatusReason.ToString(), actual.Status.SectionRegistrationStatusReason.ToString()); 
                if (expected.Status.RegistrationStatus == Domain.Student.Entities.RegistrationStatus.Registered)
                {
                    Assert.AreEqual(HeadCountStatus.Include, actual.HeadCountStatus.Value);
                }
                else
                {
                    Assert.AreEqual(HeadCountStatus.Exclude, actual.HeadCountStatus.Value);
                }
                Assert.AreEqual((RegistrationStatusReason2)Enum.Parse(typeof(RegistrationStatusReason2), expected.Status.SectionRegistrationStatusReason.ToString()), actual.Status.SectionRegistrationStatusReason);
                
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetSectionRegistrationStatusById3_Null()
            {
                await curriculumService.GetSectionRegistrationStatusById3Async(null);
            }
            
            [TestMethod]
            public async Task CurriculumService_GetSectionRegistrationStatusById2_Properties()
            {
                var expectedResults = sectionRegistrationStatusItemCollection.Where(c => c.Guid == "73244057-d1ec-4094-a0b7-de602533e3a6").FirstOrDefault();
                var sectionRegistrationStatus = await curriculumService.GetSectionRegistrationStatusById2Async("73244057-d1ec-4094-a0b7-de602533e3a6");
                Assert.IsNotNull(sectionRegistrationStatus.Id);
                Assert.IsNotNull(sectionRegistrationStatus.Code);
                Assert.IsNotNull(sectionRegistrationStatus.Status.RegistrationStatus.ToString());
                Assert.IsNotNull(sectionRegistrationStatus.Status.SectionRegistrationStatusReason.ToString());
            }
        }

        [TestClass]
        public class GetStudentStatuses
        {
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<ILogger> loggerMock;

            CurriculumService curriculumService;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentStatus> studentStatuses;

            [TestInitialize]
            public void Initialize()
            {
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                loggerMock = new Mock<ILogger>();

                studentStatuses = new TestStudentReferenceDataRepository().GetStudentStatusesAsync(false).Result;

                curriculumService = new CurriculumService(studentReferenceDataRepositoryMock.Object, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                curriculumService = null;
                studentStatuses = null;
            }

            [TestMethod]
            public async Task CurriculumService__GetAllAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(studentStatuses);

                var results = await curriculumService.GetStudentStatusesAsync(It.IsAny<bool>());
                Assert.AreEqual(studentStatuses.ToList().Count, (results.Count()));

                foreach (var studentStatus in studentStatuses)
                {
                    var result = results.FirstOrDefault(i => i.Id == studentStatus.Guid);

                    Assert.AreEqual(studentStatus.Code, result.Code);
                    Assert.AreEqual(studentStatus.Description, result.Title);
                    Assert.AreEqual(studentStatus.Guid, result.Id);
                }
            }

            [TestMethod]
            public async Task CurriculumService__GetByIdAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentStatusesAsync(It.IsAny<bool>())).ReturnsAsync(studentStatuses);

                string id = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
                var studentStatus = studentStatuses.FirstOrDefault(i => i.Guid == id);

                var result = await curriculumService.GetStudentStatusByIdAsync(id);

                Assert.AreEqual(studentStatus.Code, result.Code);
                Assert.AreEqual(studentStatus.Description, result.Title);
                Assert.AreEqual(studentStatus.Guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService__GetByIdAsync_KeyNotFoundException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentStatusesAsync(true)).ReturnsAsync(studentStatuses);
                var result = await curriculumService.GetStudentStatusByIdAsync("123");
            }
        }

        [TestClass]
        public class GetStudentTypes
        {
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<ILogger> loggerMock;

            CurriculumService curriculumService;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentType> studentTypes;

            [TestInitialize]
            public void Initialize()
            {
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                loggerMock = new Mock<ILogger>();

                studentTypes = new TestStudentReferenceDataRepository().GetStudentTypesAsync(false).Result;

                curriculumService = new CurriculumService(studentReferenceDataRepositoryMock.Object, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                curriculumService = null;
                studentTypes = null;
            }

            [TestMethod]
            public async Task CurriculumService__GetAllAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentTypesAsync(It.IsAny<bool>())).ReturnsAsync(studentTypes);

                var results = await curriculumService.GetStudentTypesAsync(It.IsAny<bool>());
                Assert.AreEqual(studentTypes.ToList().Count, (results.Count()));

                foreach (var studentType in studentTypes)
                {
                    var result = results.FirstOrDefault(i => i.Id == studentType.Guid);

                    Assert.AreEqual(studentType.Code, result.Code);
                    Assert.AreEqual(studentType.Description, result.Title);
                    Assert.AreEqual(studentType.Guid, result.Id);
                }
            }

            [TestMethod]
            public async Task CurriculumService__GetByIdAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentTypesAsync(It.IsAny<bool>())).ReturnsAsync(studentTypes);

                string id = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
                var studentType = studentTypes.FirstOrDefault(i => i.Guid == id);

                var result = await curriculumService.GetStudentTypeByIdAsync(id);

                Assert.AreEqual(studentType.Code, result.Code);
                Assert.AreEqual(studentType.Description, result.Title);
                Assert.AreEqual(studentType.Guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService__GetByIdAsync_KeyNotFoundException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetStudentTypesAsync(true)).ReturnsAsync(studentTypes);
                var result = await curriculumService.GetStudentTypeByIdAsync("123");
            }
        }

        [TestClass]
        public class GetSubjects
        {
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private ILogger logger;
            private CurriculumService curriculumService;
            private ICollection<Domain.Student.Entities.Subject> subjectCollection = new List<Domain.Student.Entities.Subject>();

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                logger = new Mock<ILogger>().Object;

                subjectCollection.Add(new Domain.Student.Entities.Subject("19f6e2cd-1e5f-4485-9b27-60d4f4e4b1ff", "AAA", "Hello World", true));
                subjectCollection.Add(new Domain.Student.Entities.Subject("73244057-D1EC-4094-A0B7-DE602533E3A6", "ACCT", "Accounting", false));
                subjectCollection.Add(new Domain.Student.Entities.Subject("1df164eb-8178-4321-a9f7-24f12d3991d8", "AGBU", "Agriculture Business", true));
                referenceRepositoryMock.Setup(repo => repo.GetSubjectsAsync()).ReturnsAsync(subjectCollection);
                referenceRepositoryMock.Setup(repo => repo.GetSubjectsAsync(true)).ReturnsAsync(subjectCollection);
                referenceRepositoryMock.Setup(repo => repo.GetSubjectsAsync(false)).ReturnsAsync(subjectCollection);

                curriculumService = new CurriculumService(referenceRepository, configurationRepositoryMock.Object, new Mock<IAdapterRegistry>().Object, new Mock<ICurrentUserFactory>().Object, new Mock<IRoleRepository>().Object, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                subjectCollection = null;
                referenceRepository = null;
                curriculumService = null;
            }

            [TestMethod]
            public async Task CurriculumService__Subjects2()
            {
                var results = await curriculumService.GetSubjects2Async(false);
                Assert.IsTrue(results is IEnumerable<Subject2>);
                Assert.IsNotNull(results);
            }

            public async Task CurriculumService_Subjects2_Count()
            {
                var results = await curriculumService.GetSubjects2Async(false);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task CurriculumService_Subjects2_Properties()
            {
                var results = await curriculumService.GetSubjects2Async(false);
                var subject = results.Where(x => x.Abbreviation == "AAA").FirstOrDefault();
                Assert.IsNotNull(subject.Id);
                Assert.IsNotNull(subject.Abbreviation);
                Assert.IsNotNull(subject.Title);
            }

            [TestMethod]
            public async Task CurriculumService_Subjects2_Expected()
            {
                var expectedResults = subjectCollection.Where(c => c.Code == "AAA").FirstOrDefault();
                var results = await curriculumService.GetSubjects2Async(false);
                var subject = results.Where(s => s.Abbreviation == "AAA").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, subject.Id);
                Assert.AreEqual(expectedResults.Code, subject.Abbreviation);
                Assert.AreEqual(expectedResults.Description, subject.Title);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetSubjectById2_Empty()
            {
                await curriculumService.GetSubjectByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CurriculumService_GetSubjectById2_Null()
            {
                await curriculumService.GetSubjectByGuid2Async(null);
            }

            [TestMethod]
            public async Task CurriculumService_GetSubjectById2_Expected()
            {
                var expectedResults = subjectCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var subject = await curriculumService.GetSubjectByGuid2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, subject.Id);
                Assert.AreEqual(expectedResults.Code, subject.Abbreviation);
                Assert.AreEqual(expectedResults.Description, subject.Title);
            }

            [TestMethod]
            public async Task CurriculumService_GetSubjectById2_Properties()
            {
                var expectedResults = subjectCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var subject = await curriculumService.GetSubjectByGuid2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(subject.Id);
                Assert.IsNotNull(subject.Abbreviation);
                Assert.IsNotNull(subject.Title);
            }
        }
    }
}
