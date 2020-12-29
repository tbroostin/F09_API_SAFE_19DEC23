//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class ProspectOpportunitiesControllerTests
    {
        public TestContext TestContext { get; set; }
        private Mock<IProspectOpportunitiesService> prospectOpportunitiesServiceMock;
        private Mock<ILogger> loggerMock;
        private ProspectOpportunitiesController prospectOpportunitiesController;
        private List<Dtos.ProspectOpportunities> prospectOpportunitiesCollection;
        private List<Dtos.ProspectOpportunitiesSubmissions> prospectOpportunitiesSubmissionsCollection;
        int offset = 0;
        int limit = 100;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
        private Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");
        private ProspectOpportunities criteria= new ProspectOpportunities();
        private string person = "";
        private string expectedGuid = "prospect-opportunity 1 guid";


        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            prospectOpportunitiesServiceMock = new Mock<IProspectOpportunitiesService>();
            loggerMock = new Mock<ILogger>();

            BuildData();

            prospectOpportunitiesController = new ProspectOpportunitiesController(prospectOpportunitiesServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            prospectOpportunitiesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            prospectOpportunitiesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
        }

        private void BuildData()
        {
            prospectOpportunitiesCollection = new List<Dtos.ProspectOpportunities>();
            prospectOpportunitiesSubmissionsCollection = new List<Dtos.ProspectOpportunitiesSubmissions>();
 
            var prospectOpportunity1 = new Ellucian.Colleague.Dtos.ProspectOpportunities
            {
                Id = "prospect-opportunity 1 guid",
                AdmissionPopulation = new GuidObject2("POP1 guid"),
                EntryAcademicPeriod = new GuidObject2("TERM1 guid"),
                Prospect = new GuidObject2("PERSON1 guid"),
                RecruitAcademicPrograms = new List<GuidObject2>() { new GuidObject2("PROGRAM1 guid"), new GuidObject2("PROGRAM2 guid") },
                Site = new GuidObject2("SITE1 guid")
            };

            var prospectOpportunity2 = new Ellucian.Colleague.Dtos.ProspectOpportunities
            {
                Id = "prospect-opportunity 2 guid",
                AdmissionPopulation = new GuidObject2("POP2 guid"),
                EntryAcademicPeriod = new GuidObject2("TERM2 guid"),
                Prospect = new GuidObject2("PERSON2 guid"),
                RecruitAcademicPrograms = new List<GuidObject2>() { new GuidObject2("PROGRAM3 guid"), new GuidObject2("PROGRAM4 guid") },
                Site = new GuidObject2("SITE2 guid")
            };

            prospectOpportunitiesCollection.Add(prospectOpportunity1);
            prospectOpportunitiesCollection.Add(prospectOpportunity2);

            var recruitAcademicProgram = new RecruitAcademicProgram()
            {
                Program = new GuidObject2("PROGRAM3 guid"),
                AcademicLevel = new GuidObject2("ACAD LEVEL guid")
            };
            var prospectOpportunitySubmission1 = new Ellucian.Colleague.Dtos.ProspectOpportunitiesSubmissions
            {
                Id = "prospect-opportunity-submission 1 guid",
                AdmissionPopulation = new GuidObject2("POP1 guid"),
                EntryAcademicPeriod = new GuidObject2("TERM1 guid"),
                Prospect = new GuidObject2("PERSON1 guid"),
                RecruitAcademicPrograms = new List<RecruitAcademicProgram>() { recruitAcademicProgram },
                Site = new GuidObject2("SITE1 guid")
            };                       

            var prospectOpportunitySubmission2 = new Ellucian.Colleague.Dtos.ProspectOpportunitiesSubmissions
            {
                Id = "prospect-opportunity-submission 2 guid",
                AdmissionPopulation = new GuidObject2("POP2 guid"),
                EntryAcademicPeriod = new GuidObject2("TERM2 guid"),
                Prospect = new GuidObject2("PERSON2 guid"),
                RecruitAcademicPrograms = new List<RecruitAcademicProgram>() { recruitAcademicProgram },
                Site = new GuidObject2("SITE2 guid")
            };
            prospectOpportunitiesSubmissionsCollection.Add(prospectOpportunitySubmission1);
            prospectOpportunitiesSubmissionsCollection.Add(prospectOpportunitySubmission2);

        }

        [TestCleanup]
        public void Cleanup()
        {
            prospectOpportunitiesController = null;
            prospectOpportunitiesCollection = null;
            loggerMock = null;
            prospectOpportunitiesServiceMock = null;
        }
        [TestMethod]
        public async Task ProspectOpportunitiesController_GetProspectOpportunities_ValidateFields_Nocache()
        {
            prospectOpportunitiesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true, Public  = true};
            var tuple = new Tuple<IEnumerable<Dtos.ProspectOpportunities>, int>(prospectOpportunitiesCollection, 2);
            prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProspectOpportunities>(), person, It.IsAny<bool>())).ReturnsAsync(tuple);

            var prospectOpportunities = await prospectOpportunitiesController.GetProspectOpportunitiesAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await prospectOpportunities.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.ProspectOpportunities> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ProspectOpportunities>>)httpResponseMessage.Content)
                                                .Value as IEnumerable<Dtos.ProspectOpportunities>;

            Assert.IsNotNull(actuals);
            Assert.AreEqual(prospectOpportunitiesCollection.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = prospectOpportunitiesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AdmissionPopulation, actual.AdmissionPopulation);
                Assert.AreEqual(expected.EntryAcademicPeriod, actual.EntryAcademicPeriod);
                Assert.AreEqual(expected.Prospect, actual.Prospect);
                Assert.AreEqual(expected.Site, actual.Site);

                Assert.IsNotNull(expected.RecruitAcademicPrograms);
                foreach (var prog in expected.RecruitAcademicPrograms)
                {
                    Assert.IsTrue(actual.RecruitAcademicPrograms.Contains(prog));
                }
            }

    }
        [TestMethod]
        public async Task ProspectOpportunitiesController_GetProspectOpportunities_ValidateFields_Cache()
        {
            prospectOpportunitiesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false, Public = true };
            var tuple = new Tuple<IEnumerable<Dtos.ProspectOpportunities>, int>(prospectOpportunitiesCollection, 2);
            prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProspectOpportunities>(), person, It.IsAny<bool>())).ReturnsAsync(tuple);

            var prospectOpportunities = await prospectOpportunitiesController.GetProspectOpportunitiesAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await prospectOpportunities.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.ProspectOpportunities> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ProspectOpportunities>>)httpResponseMessage.Content)
                                                .Value as IEnumerable<Dtos.ProspectOpportunities>;

            Assert.IsNotNull(actuals);
            Assert.AreEqual(prospectOpportunitiesCollection.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = prospectOpportunitiesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AdmissionPopulation, actual.AdmissionPopulation);
                Assert.AreEqual(expected.EntryAcademicPeriod, actual.EntryAcademicPeriod);
                Assert.AreEqual(expected.Prospect, actual.Prospect);
                Assert.AreEqual(expected.Site, actual.Site);

                Assert.IsNotNull(expected.RecruitAcademicPrograms);
                foreach (var prog in expected.RecruitAcademicPrograms)
                {
                    Assert.IsTrue(actual.RecruitAcademicPrograms.Contains(prog));
                }
            }

        }

       
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunities_KeyNotFoundException()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesByGuidAsync("BADGUID", It.IsAny<bool>()))
                        .Throws<KeyNotFoundException>();
                    await prospectOpportunitiesController.GetProspectOpportunitiesByGuidAsync("BADGUID");
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunities_PermissionsException()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProspectOpportunities>(), person, It.IsAny<bool>()))
                         .Throws<PermissionsException>();
                    var prospectOpportunities = await prospectOpportunitiesController.GetProspectOpportunitiesAsync(new Paging(limit, offset), criteriaFilter, personFilter);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunities_ArgumentException()
                {

                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProspectOpportunities>(), person, It.IsAny<bool>()))
                        .Throws < ArgumentException >();
                    var prospectOpportunities = await prospectOpportunitiesController.GetProspectOpportunitiesAsync(new Paging(limit, offset), criteriaFilter, personFilter);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunities_RepositoryException()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProspectOpportunities>(), person, It.IsAny<bool>()))
                       .Throws<RepositoryException>();
                    var prospectOpportunities = await prospectOpportunitiesController.GetProspectOpportunitiesAsync(new Paging(limit, offset), criteriaFilter, personFilter);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunities_IntegrationApiException()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProspectOpportunities>(), person, It.IsAny<bool>()))
                       .Throws<IntegrationApiException>();
                    var prospectOpportunities = await prospectOpportunitiesController.GetProspectOpportunitiesAsync(new Paging(limit, offset), criteriaFilter, personFilter);
                }
                [TestMethod]
                public async Task ProspectOpportunitiesController_GetProspectOpportunitiesByGuidAsync_ValidateFields()
                {
                    var expected = prospectOpportunitiesCollection.FirstOrDefault();
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
                    var actual = await prospectOpportunitiesController.GetProspectOpportunitiesByGuidAsync(expected.Id);
                    Assert.AreEqual(expected.Id, actual.Id, "Id");
                    Assert.AreEqual(expected.AdmissionPopulation, actual.AdmissionPopulation);
                    Assert.AreEqual(expected.EntryAcademicPeriod, actual.EntryAcademicPeriod);
                    Assert.AreEqual(expected.Prospect, actual.Prospect);
                    Assert.AreEqual(expected.Site, actual.Site);

                    Assert.IsNotNull(expected.RecruitAcademicPrograms);
                    foreach (var prog in expected.RecruitAcademicPrograms)
                    {
                        Assert.IsTrue(actual.RecruitAcademicPrograms.Contains(prog));
                    }
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunities_Exception()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProspectOpportunities>(), person, It.IsAny<bool>()))
                        .Throws<Exception>();
                    var prospectOpportunities = await prospectOpportunitiesController.GetProspectOpportunitiesAsync(new Paging(limit, offset), criteriaFilter, personFilter);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunitiesByGuidAsync_Exception()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
                    await prospectOpportunitiesController.GetProspectOpportunitiesByGuidAsync(string.Empty);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunitiesByGuid_KeyNotFoundException()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws<KeyNotFoundException>();
                    await prospectOpportunitiesController.GetProspectOpportunitiesByGuidAsync(expectedGuid);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunitiesByGuid_PermissionsException()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws<PermissionsException>();
                    await prospectOpportunitiesController.GetProspectOpportunitiesByGuidAsync(expectedGuid);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunitiesByGuid_ArgumentException()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws<ArgumentException>();
                    await prospectOpportunitiesController.GetProspectOpportunitiesByGuidAsync(expectedGuid);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunitiesByGuid_RepositoryException()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws<RepositoryException>();
                    await prospectOpportunitiesController.GetProspectOpportunitiesByGuidAsync(expectedGuid);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunitiesByGuid_IntegrationApiException()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws<IntegrationApiException>();
                    await prospectOpportunitiesController.GetProspectOpportunitiesByGuidAsync(expectedGuid);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_GetProspectOpportunitiesByGuid_Exception()
                {
                    prospectOpportunitiesServiceMock.Setup(x => x.GetProspectOpportunitiesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                        .Throws<Exception>();
                    await prospectOpportunitiesController.GetProspectOpportunitiesByGuidAsync(expectedGuid);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_PostProspectOpportunitiesAsync_Exception()
                {
                    await prospectOpportunitiesController.PostProspectOpportunitiesAsync(prospectOpportunitiesCollection.FirstOrDefault());
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_PutProspectOpportunitiesAsync_Exception()
                {
                    var sourceContext = prospectOpportunitiesCollection.FirstOrDefault();
                    await prospectOpportunitiesController.PutProspectOpportunitiesAsync(sourceContext.Id, sourceContext);
                }
                [TestMethod]
                [ExpectedException(typeof(HttpResponseException))]
                public async Task ProspectOpportunitiesController_DeleteProspectOpportunitiesAsync_Exception()
                {
                    await prospectOpportunitiesController.DeleteProspectOpportunitiesAsync(prospectOpportunitiesCollection.FirstOrDefault().Id);
                }

        #region prospect-opportunities-submissions
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProspectOpportunitiesController_PostProspectOpportunitiesSubmissionsAsync_Exception()
        {
            await prospectOpportunitiesController.PostProspectOpportunitiesSubmissionsAsync(prospectOpportunitiesSubmissionsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProspectOpportunitiesController_PutProspectOpportunitiesSubmissions_PermissionsException()
        {
            prospectOpportunitiesServiceMock.Setup(x => x.UpdateProspectOpportunitiesSubmissionsAsync(prospectOpportunitiesSubmissionsCollection.FirstOrDefault(), false))
                .Throws<PermissionsException>();
            await prospectOpportunitiesController.PutProspectOpportunitiesSubmissionsAsync(expectedGuid, prospectOpportunitiesSubmissionsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProspectOpportunitiesController_PutProspectOpportunitiesSubmissionsAsync_Exception()
        {
            prospectOpportunitiesServiceMock.Setup(x => x.UpdateProspectOpportunitiesSubmissionsAsync(prospectOpportunitiesSubmissionsCollection.FirstOrDefault(), false))
                .Throws<Exception>();
            var sourceContext = prospectOpportunitiesSubmissionsCollection.FirstOrDefault();
            await prospectOpportunitiesController.PutProspectOpportunitiesSubmissionsAsync(sourceContext.Id, sourceContext);
        }
        #endregion
    }
}
