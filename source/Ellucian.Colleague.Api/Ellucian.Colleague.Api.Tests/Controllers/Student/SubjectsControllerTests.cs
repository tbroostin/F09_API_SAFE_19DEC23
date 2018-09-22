// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SubjectsControllerTests
    {
        [TestClass]
        public class SubjectsControllerGetHEDM4
        {
            #region Test Context

            private TestContext testContextInstance;

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

            private SubjectsController SubjectsController;
            private Mock<IStudentReferenceDataRepository> SubjectRepositoryMock;
            private IStudentReferenceDataRepository SubjectRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Subject> allSubjectEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<ICurriculumService> curriculumServiceMock;
            private ICurriculumService curriculumService;
            List<Subject2> SubjectList;
            private string subjectsGuid;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                SubjectRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                SubjectRepository = SubjectRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Subject, Subject2>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                curriculumServiceMock = new Mock<ICurriculumService>();
                curriculumService = curriculumServiceMock.Object;

                allSubjectEntities = new TestSubjectRepository().Get();
                SubjectList = new List<Subject2>();
                subjectsGuid = allSubjectEntities.FirstOrDefault().Guid;

                SubjectsController = new SubjectsController(AdapterRegistry, SubjectRepository, curriculumService, logger);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Subject, Subject2>();
                SubjectsController.Request = new HttpRequestMessage();
                SubjectsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var subjects in allSubjectEntities)
                {
                    Subject2 target = ConvertSubjectEntitytoSubjectDto(subjects);
                    SubjectList.Add(target);
                }
                SubjectRepositoryMock.Setup(x => x.GetSubjectsAsync()).ReturnsAsync(allSubjectEntities);
                SubjectRepositoryMock.Setup(x => x.GetSubjectsAsync(It.IsAny<bool>())).ReturnsAsync(allSubjectEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                SubjectsController = null;
                SubjectRepository = null;
            }

            [TestMethod]
            public async Task ReturnsAllSubjectsAsync()
            {
                var Subjects = await SubjectsController.GetAsync();
                Assert.AreEqual(Subjects.Count(), allSubjectEntities.Count());
            }

            [TestMethod]
            public async Task GetSubjectsByGuidAsync_Validate()
            {
                var thisSubject = SubjectList.Where(m => m.Id == subjectsGuid).FirstOrDefault();

                curriculumServiceMock.Setup(x => x.GetSubjectByGuid2Async(It.IsAny<string>())).ReturnsAsync(thisSubject);

                var subject = await SubjectsController.GetSubjectByGuid2Async(subjectsGuid);
                Assert.AreEqual(thisSubject.Id, subject.Id);
                Assert.AreEqual(thisSubject.Abbreviation, subject.Abbreviation);
                Assert.AreEqual(thisSubject.Description, subject.Description);
            }

            [TestMethod]
            public async Task SubjectsController_GetHedmAsync()
            {
                curriculumServiceMock.Setup(gc => gc.GetSubjects2Async(It.IsAny<bool>())).ReturnsAsync(SubjectList);

                var result = await SubjectsController.GetSubjects2Async();
                Assert.AreEqual(result.Count(), allSubjectEntities.Count());

                int count = allSubjectEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = SubjectList[i];
                    var actual = allSubjectEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Abbreviation, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task SubjectsController_GetHedmAsync_CacheControlNotNull()
            {
                SubjectsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                curriculumServiceMock.Setup(gc => gc.GetSubjects2Async(It.IsAny<bool>())).ReturnsAsync(SubjectList);

                var result = await SubjectsController.GetSubjects2Async();
                Assert.AreEqual(result.Count(), allSubjectEntities.Count());

                int count = allSubjectEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = SubjectList[i];
                    var actual = allSubjectEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Abbreviation, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task SubjectsController_GetHedmAsync_NoCache()
            {
                SubjectsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                SubjectsController.Request.Headers.CacheControl.NoCache = true;

                curriculumServiceMock.Setup(gc => gc.GetSubjects2Async(It.IsAny<bool>())).ReturnsAsync(SubjectList);

                var result = await SubjectsController.GetSubjects2Async();
                Assert.AreEqual(result.Count(), allSubjectEntities.Count());

                int count = allSubjectEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = SubjectList[i];
                    var actual = allSubjectEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Abbreviation, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task SubjectsController_GetHedmAsync_Cache()
            {
                SubjectsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                SubjectsController.Request.Headers.CacheControl.NoCache = false;

                curriculumServiceMock.Setup(gc => gc.GetSubjects2Async(It.IsAny<bool>())).ReturnsAsync(SubjectList);

                var result = await SubjectsController.GetSubjects2Async();
                Assert.AreEqual(result.Count(), allSubjectEntities.Count());

                int count = allSubjectEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = SubjectList[i];
                    var actual = allSubjectEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Abbreviation, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task SubjectsController_GetByIdHedmAsync()
            {
                var thisSubject = SubjectList.Where(m => m.Id == subjectsGuid).FirstOrDefault();

                curriculumServiceMock.Setup(x => x.GetSubjectByGuid2Async(It.IsAny<string>())).ReturnsAsync(thisSubject);

                var subject = await SubjectsController.GetSubjectByGuid2Async(subjectsGuid);
                Assert.AreEqual(thisSubject.Id, subject.Id);
                Assert.AreEqual(thisSubject.Abbreviation, subject.Abbreviation);
                Assert.AreEqual(thisSubject.Description, subject.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SubjectsController_GetThrowsIntAppiExc()
            {
                curriculumServiceMock.Setup(gc => gc.GetSubjects2Async(It.IsAny<bool>())).Throws<Exception>();

                await SubjectsController.GetSubjects2Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SubjectsController_GetByIdThrowsIntAppiExc()
            {
                curriculumServiceMock.Setup(gc => gc.GetSubjectByGuid2Async(It.IsAny<string>())).Throws<Exception>();

                await SubjectsController.GetSubjectByGuid2Async("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void SubjectsController_PostThrowsIntAppiExc()
            {
                SubjectsController.PostSubject(SubjectList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void SubjectsController_PutThrowsIntAppiExc()
            {
                var result = SubjectsController.PutSubject("9ae3a175-1dfd-4937-b97b-3c9ad596e023", SubjectList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void SubjectsController_DeleteThrowsIntAppiExc()
            {
                SubjectsController.DeleteSubject("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
            /// <summary>
            /// Converts a Subject domain entity to its corresponding Subject DTO
            /// </summary>
            /// <param name="source">Subject domain entity</param>
            /// <returns>Subject2 DTO</returns>
            private Dtos.Subject2 ConvertSubjectEntitytoSubjectDto(Domain.Student.Entities.Subject source)
            {
                var subject = new Dtos.Subject2();
                subject.Id = source.Guid;
                subject.Abbreviation = source.Code;
                subject.Title = source.Description;
                subject.Description = null;
                return subject;
            }
        }
            
        }
}
