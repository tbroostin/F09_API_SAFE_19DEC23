// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AcademicHistoryControllerTests
    {
        /// <summary>
        /// Set up class to use for each faculty controller test class
        /// </summary>
        public abstract class AcademicHistoryControllerTestSetup
        {
            #region Test Context

            protected TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            public AcademicHistoryController academicHistoryController;
            public Mock<IAcademicHistoryService> academicHistoryServiceMock;
            public IAcademicHistoryService academicHistoryService;
            public Mock<ILogger> loggerMock;
            public IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit> academicCredits;

            public async Task InitializeAcademicHistoryController()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                academicCredits = await new TestAcademicCreditRepository().GetAsync();

                academicHistoryController = new AcademicHistoryController(academicHistoryService, loggerMock.Object);

                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, AcademicCredit2>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, AcademicCredit3>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, MidTermGrade2>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.GradingType, GradingType>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.ReplacementStatus, ReplacementStatus>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.ReplacedStatus, ReplacedStatus>();
            }

        }

        [TestClass]
        public class AcademicHistoryController_QueryAcademicCreditsAsync : AcademicHistoryControllerTestSetup
        {
            private List<string> sectionIds;
            private List<AcademicCredit2> academicCreditDtos;
            AcademicCreditQueryCriteria criteria;

            [TestInitialize]
            public void Initialize()
            {
                InitializeAcademicHistoryController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionIds = new List<string>() { "8001", "8005" };
                List<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit> sectionAcademicCreditEntities = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit>();
                foreach (var sectionId in sectionIds)
                {
                    sectionAcademicCreditEntities.AddRange(academicCredits.Where(ac => ac.SectionId == sectionId).ToList());
                }
                
                academicHistoryController = new AcademicHistoryController(academicHistoryService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                academicCreditDtos = new List<AcademicCredit2>();
                foreach (var credit in sectionAcademicCreditEntities)
                {
                    AcademicCredit2 target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, AcademicCredit2>(credit);
                    academicCreditDtos.Add(target);
                }
                
                criteria = new AcademicCreditQueryCriteria() { SectionIds = sectionIds };
                academicHistoryServiceMock.Setup(x => x.QueryAcademicCreditsAsync(criteria)).Returns(Task.FromResult<IEnumerable<AcademicCredit2>>(academicCreditDtos));
            }

            [TestCleanup]
            public void Cleanup()
            {
                academicHistoryController = null;
                academicHistoryService = null;
            }

            [TestMethod]
            public async Task ReturnsAcademicCreditDtos()
            {
                // act
                var response = await academicHistoryController.QueryAcademicCreditsAsync(criteria);

                // assert
                Assert.IsTrue(response is IEnumerable<AcademicCredit2>);
                Assert.AreEqual(2, response.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResponseWhenArgumentExceptionOccurs()
            {
                // arrange
                criteria.SectionIds = null;
                academicHistoryServiceMock.Setup(x => x.QueryAcademicCreditsAsync(criteria)).Throws(new ArgumentException());

                // act
                var response = await academicHistoryController.QueryAcademicCreditsAsync(criteria);
            }
        }

        [TestClass]
        public class AcademicHistoryController_QueryAcademicCredits2Async : AcademicHistoryControllerTestSetup
        {
            private List<string> sectionIds;
            private List<AcademicCredit3> academicCreditDtos;
            AcademicCreditQueryCriteria criteria;

            [TestInitialize]
            public void Initialize()
            {
                InitializeAcademicHistoryController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionIds = new List<string>() { "8001", "8005" };
                List<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit> sectionAcademicCreditEntities = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit>();
                foreach (var sectionId in sectionIds)
                {
                    sectionAcademicCreditEntities.AddRange(academicCredits.Where(ac => ac.SectionId == sectionId).ToList());
                }

                academicHistoryController = new AcademicHistoryController(academicHistoryService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                academicCreditDtos = new List<AcademicCredit3>();
                foreach (var credit in sectionAcademicCreditEntities)
                {
                    AcademicCredit3 target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, AcademicCredit3>(credit);
                    academicCreditDtos.Add(target);
                }

                criteria = new AcademicCreditQueryCriteria() { SectionIds = sectionIds };
                academicHistoryServiceMock.Setup(x => x.QueryAcademicCredits2Async(criteria)).Returns(Task.FromResult<IEnumerable<AcademicCredit3>>(academicCreditDtos));
            }

            [TestCleanup]
            public void Cleanup()
            {
                academicHistoryController = null;
                academicHistoryService = null;
            }

            [TestMethod]
            public async Task ReturnsAcademicCreditDtos()
            {
                // act
                var response = await academicHistoryController.QueryAcademicCredits2Async(criteria);

                // assert
                Assert.IsTrue(response is IEnumerable<AcademicCredit3>);
                Assert.AreEqual(2, response.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResponseWhenArgumentNullExceptionOccurs()
            {
                // arrange
                criteria.SectionIds = null;
                academicHistoryServiceMock.Setup(x => x.QueryAcademicCredits2Async(criteria)).Throws(new ArgumentNullException());

                // act
                var response = await academicHistoryController.QueryAcademicCredits2Async(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResponseWhenArgumentExceptionOccurs()
            {
                // arrange
                criteria.SectionIds = null;
                academicHistoryServiceMock.Setup(x => x.QueryAcademicCredits2Async(criteria)).Throws(new ArgumentException());

                // act
                var response = await academicHistoryController.QueryAcademicCredits2Async(criteria);
            }
        }

        [TestClass]
        public class AcademicHistoryController_QueryAcademicCredits3Async : AcademicHistoryControllerTestSetup
        {
            private List<string> sectionIds;
            private List<AcademicCredit3> academicCreditDtos;
            AcademicCreditQueryCriteria criteria;

            [TestInitialize]
            public void Initialize()
            {
                InitializeAcademicHistoryController();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionIds = new List<string>() { "8001", "8005" };
                List<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit> sectionAcademicCreditEntities = new List<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit>();
                foreach (var sectionId in sectionIds)
                {
                    sectionAcademicCreditEntities.AddRange(academicCredits.Where(ac => ac.SectionId == sectionId).ToList());
                }

                academicHistoryController = new AcademicHistoryController(academicHistoryService, loggerMock.Object);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Faculty, Faculty>();
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Corequisite, Corequisite>();
                academicCreditDtos = new List<AcademicCredit3>();
                foreach (var credit in sectionAcademicCreditEntities)
                {
                    AcademicCredit3 target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, AcademicCredit3>(credit);
                    academicCreditDtos.Add(target);
                }

                criteria = new AcademicCreditQueryCriteria() { SectionIds = sectionIds };
                AcademicCreditsWithInvalidKeys creditsWithInvalidKeys = new AcademicCreditsWithInvalidKeys(academicCreditDtos, new List<string>() { "8002" });
                academicHistoryServiceMock.Setup(x => x.QueryAcademicCreditsWithInvalidKeysAsync(criteria)).Returns(Task.FromResult<AcademicCreditsWithInvalidKeys>(creditsWithInvalidKeys));
            }

            [TestCleanup]
            public void Cleanup()
            {
                academicHistoryController = null;
                academicHistoryService = null;
            }

            [TestMethod]
            public async Task ReturnsAcademicCreditWithInvalidKeysDtos()
            {
                // act
                var response = await academicHistoryController.QueryAcademicCreditsWithInvalidKeysAsync(criteria);

                // assert
                Assert.IsTrue(response.AcademicCredits is IEnumerable<AcademicCredit3>);
                Assert.AreEqual(2, response.AcademicCredits.Count());
                Assert.AreEqual(1, response.InvalidAcademicCreditIds.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResponseWhenArgumentNullExceptionOccurs()
            {
                // arrange
                criteria.SectionIds = null;
                academicHistoryServiceMock.Setup(x => x.QueryAcademicCreditsWithInvalidKeysAsync(criteria)).Throws(new ArgumentNullException());

                // act
                var response = await academicHistoryController.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ResponseWhenArgumentExceptionOccurs()
            {
                // arrange
                criteria.SectionIds = null;
                academicHistoryServiceMock.Setup(x => x.QueryAcademicCreditsWithInvalidKeysAsync(criteria)).Throws(new ArgumentException());

                // act
                var response = await academicHistoryController.QueryAcademicCreditsWithInvalidKeysAsync(criteria);
            }
        }
    }
}
