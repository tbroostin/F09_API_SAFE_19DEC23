//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class FinancialAidApplicationOutcomesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinancialAidApplicationOutcomeService> financialAidApplicationOutcomesServiceMock;
        private Mock<ILogger> loggerMock;
        public TestFafsaRepository expectedFafsaRepository;
        private FinancialAidApplicationOutcomesController financialAidApplicationOutcomesController;      
        private IEnumerable<Fafsa> allFafsa;
        private List<Dtos.FinancialAidApplicationOutcome> financialAidApplicationOutcomesCollection;
        private Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int> financialAidApplicationOutcomesTuple;

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            financialAidApplicationOutcomesServiceMock = new Mock<IFinancialAidApplicationOutcomeService>();
            loggerMock = new Mock<ILogger>();
            financialAidApplicationOutcomesCollection = new List<Dtos.FinancialAidApplicationOutcome>();

            expectedFafsaRepository = new TestFafsaRepository();

            allFafsa = expectedFafsaRepository.GetFafsasAsync(new List<string>() { "0003914" }, new List<string>() { "2013" }).Result;
            
            foreach (var source in allFafsa)
            {
                var financialAidApplicationOutcomes = new Ellucian.Colleague.Dtos.FinancialAidApplicationOutcome
                {
                    Id = Guid.NewGuid().ToString(),
                    Applicant = new Dtos.DtoProperties.FinancialAidApplicationApplicant() { Person = new GuidObject2(Guid.NewGuid().ToString()) },
                    AidYear = new GuidObject2(Guid.NewGuid().ToString()),
                    ExpectedFamilyContribution = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = source.FamilyContribution },
                    TotalIncome = new Dtos.DtoProperties.AmountDtoProperty() {  Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = source.StudentAdjustedGrossIncome }
                };
                financialAidApplicationOutcomesCollection.Add(financialAidApplicationOutcomes);
            }
            financialAidApplicationOutcomesTuple = new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(financialAidApplicationOutcomesCollection, financialAidApplicationOutcomesCollection.Count);

            financialAidApplicationOutcomesController = new FinancialAidApplicationOutcomesController(financialAidApplicationOutcomesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            financialAidApplicationOutcomesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            financialAidApplicationOutcomesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            financialAidApplicationOutcomesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            financialAidApplicationOutcomesController = null;
            allFafsa = null;
            financialAidApplicationOutcomesCollection = null;
            loggerMock = null;
            financialAidApplicationOutcomesServiceMock = null;
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomes_ValidateFields_Nocache()
        {
            financialAidApplicationOutcomesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(financialAidApplicationOutcomesTuple);
       
            var outcomes = await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesAsync(new Paging(3,0));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await outcomes.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.FinancialAidApplicationOutcome> sourceContexts = ((ObjectContent<IEnumerable<Dtos.FinancialAidApplicationOutcome>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.FinancialAidApplicationOutcome>;

            Assert.AreEqual(financialAidApplicationOutcomesCollection.Count, sourceContexts.Count());
            for (var i = 0; i < sourceContexts.Count(); i++)
            {
                var expected = financialAidApplicationOutcomesCollection[i];
                var actual = sourceContexts.ElementAt(i);
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomes_ValidateFields_Cache()
        {
            financialAidApplicationOutcomesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(financialAidApplicationOutcomesTuple);

            var outcomes = (await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesAsync(new Web.Http.Models.Paging(3, 0)));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await outcomes.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.FinancialAidApplicationOutcome> sourceContexts = ((ObjectContent<IEnumerable<Dtos.FinancialAidApplicationOutcome>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.FinancialAidApplicationOutcome>;

            Assert.AreEqual(financialAidApplicationOutcomesCollection.Count, sourceContexts.Count());
            for (var i = 0; i < sourceContexts.Count(); i++)
            {
                var expected = financialAidApplicationOutcomesCollection[i];
                var actual = sourceContexts.ElementAt(i);
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomesByGuidAsync_ValidateFields()
        {
            var expected = financialAidApplicationOutcomesCollection.FirstOrDefault();
            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomes_Exception()
        {
            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<Exception>();
            await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesAsync(new Web.Http.Models.Paging(3, 0));       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomesByGuidAsync_Exception()
        {
            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidApplicationOutcomesController_PostFinancialAidApplicationOutcomesAsync_Exception()
        {
            await financialAidApplicationOutcomesController.CreateAsync(financialAidApplicationOutcomesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidApplicationOutcomesController_PutFinancialAidApplicationOutcomesAsync_Exception()
        {
            var sourceContext = financialAidApplicationOutcomesCollection.FirstOrDefault();
            await financialAidApplicationOutcomesController.UpdateAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidApplicationOutcomesController_DeleteFinancialAidApplicationOutcomesAsync_Exception()
        {
            await financialAidApplicationOutcomesController.DeleteAsync(financialAidApplicationOutcomesCollection.FirstOrDefault().Id);
        }
    }
}