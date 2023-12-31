﻿//// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
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
    public class FinancialAidFundsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinancialAidFundsService> financialAidFundServiceMock;
        private Mock<ILogger> loggerMock;
        private FinancialAidFundsController financialAidFundsController;
        private IEnumerable<FinancialAidFund> allFinancialAidFund;
        private List<Dtos.FinancialAidFunds> financialAidFundCollection;
        Tuple<IEnumerable<Dtos.FinancialAidFunds>, int> financialAidFundTuple;
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");
        Web.Http.Models.Paging paging = new Web.Http.Models.Paging(100, 0);

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            financialAidFundServiceMock = new Mock<IFinancialAidFundsService>();
            loggerMock = new Mock<ILogger>();
            financialAidFundCollection = new List<Dtos.FinancialAidFunds>();

            allFinancialAidFund = (await new TestStudentReferenceDataRepository().GetFinancialAidFundsAsync(true)).ToList();

            foreach (var source in allFinancialAidFund)
            {
                var financialAidFund = new Ellucian.Colleague.Dtos.FinancialAidFunds
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,
                };
                financialAidFundCollection.Add(financialAidFund);
            }

            financialAidFundTuple = new Tuple<IEnumerable<Dtos.FinancialAidFunds>, int>(financialAidFundCollection, financialAidFundCollection.Count());

            financialAidFundsController = new FinancialAidFundsController(financialAidFundServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            financialAidFundsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            financialAidFundsController = null;
            allFinancialAidFund = null;
            financialAidFundCollection = null;
            loggerMock = null;
            financialAidFundServiceMock = null;
        }

        [TestMethod]
        public async Task FinancialAidFundController_GetFinancialAidFund_ValidateFields_Nocache()
        {
            financialAidFundsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.FinancialAidFundsFilter>(), false)).ReturnsAsync(financialAidFundTuple);

            var results = await financialAidFundsController.GetFinancialAidFundsAsync(paging, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.FinancialAidFunds> sourceContexts = ((ObjectContent<IEnumerable<Dtos.FinancialAidFunds>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.FinancialAidFunds>;

            Assert.AreEqual(financialAidFundCollection.Count, sourceContexts.Count());
            for (var i = 0; i < sourceContexts.Count(); i++)
            {
                var expected = financialAidFundCollection[i];
                var actual = sourceContexts.ElementAt(i);
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidFundController_GetFinancialAidFund_ValidateFields_Cache()
        {
            financialAidFundsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.FinancialAidFundsFilter>(), true)).ReturnsAsync(financialAidFundTuple);

            var results = await financialAidFundsController.GetFinancialAidFundsAsync(paging, criteriaFilter);
            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.FinancialAidFunds> sourceContexts = ((ObjectContent<IEnumerable<Dtos.FinancialAidFunds>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.FinancialAidFunds>;

            Assert.AreEqual(financialAidFundCollection.Count, sourceContexts.Count());
            for (var i = 0; i < sourceContexts.Count(); i++)
            {
                var expected = financialAidFundCollection[i];
                var actual = sourceContexts.ElementAt(i);
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidFundController_GetFinancialAidFundsByIdAsync_ValidateFields()
        {
            var expected = financialAidFundCollection.FirstOrDefault();
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await financialAidFundsController.GetFinancialAidFundsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFund_PermissionsException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.FinancialAidFundsFilter>(), false)).Throws<PermissionsException>();
            await financialAidFundsController.GetFinancialAidFundsAsync(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFund_KeyNotFoundException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.FinancialAidFundsFilter>(), false)).Throws<KeyNotFoundException>();
            await financialAidFundsController.GetFinancialAidFundsAsync(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFund_ArgumentNullException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.FinancialAidFundsFilter>(), false)).Throws<ArgumentNullException>();
            await financialAidFundsController.GetFinancialAidFundsAsync(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFund_RepositoryException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.FinancialAidFundsFilter>(), false)).Throws<RepositoryException>();
            await financialAidFundsController.GetFinancialAidFundsAsync(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFund_IntgApiException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.FinancialAidFundsFilter>(), false)).Throws<IntegrationApiException>();
            await financialAidFundsController.GetFinancialAidFundsAsync(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFund_Exception()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.FinancialAidFundsFilter>(), false)).Throws<Exception>();
            await financialAidFundsController.GetFinancialAidFundsAsync(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFundsByIdAsync_PermissionsException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await financialAidFundsController.GetFinancialAidFundsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFundsByIdAsync_KeyNotFoundException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await financialAidFundsController.GetFinancialAidFundsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFundsByIdAsync_ArgumentNullException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsByGuidAsync(It.IsAny<string>())).Throws<ArgumentNullException>();
            await financialAidFundsController.GetFinancialAidFundsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFundsByIdAsync_RepositoryException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await financialAidFundsController.GetFinancialAidFundsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFundsByIdAsync_IntgApiException()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await financialAidFundsController.GetFinancialAidFundsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_GetFinancialAidFundsByIdAsync_Exception()
        {
            financialAidFundServiceMock.Setup(x => x.GetFinancialAidFundsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await financialAidFundsController.GetFinancialAidFundsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_PostFinancialAidFundsAsync_Exception()
        {
            await financialAidFundsController.PostFinancialAidFundsAsync(financialAidFundCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_PutFinancialAidFundsAsync_Exception()
        {
            var sourceContext = financialAidFundCollection.FirstOrDefault();
            await financialAidFundsController.PutFinancialAidFundsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundController_DeleteFinancialAidFundsAsync_Exception()
        {
            await financialAidFundsController.DeleteFinancialAidFundsAsync(financialAidFundCollection.FirstOrDefault().Id);
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomesController_GetAll_CodeFilter()
        {
            financialAidFundsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var filterGroupName = "criteria";
            financialAidFundsController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new Dtos.Filters.FinancialAidFundsFilter() { Code = "CODE1" });

            var collection = financialAidFundCollection.Where(e => e.Code.Equals("CODE1", StringComparison.OrdinalIgnoreCase));
            var collectionCount = collection.Count();

            var tuple = new Tuple<IEnumerable<Dtos.FinancialAidFunds>, int>(collection, collectionCount);
            financialAidFundServiceMock.Setup(ci => ci.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.Filters.FinancialAidFundsFilter>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var financialAidFunds = await financialAidFundsController.GetFinancialAidFundsAsync(new Paging(3, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await financialAidFunds.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.FinancialAidFunds> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidFunds>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.FinancialAidFunds>;


            Assert.AreEqual(collectionCount, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = financialAidFundCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected); 
                Assert.AreEqual(expected.Id, actual.Id, "Id");
                Assert.AreEqual(expected.Title, actual.Title, "Title");
                Assert.AreEqual(expected.Code, actual.Code, "Code");
            }
        }
    }
}