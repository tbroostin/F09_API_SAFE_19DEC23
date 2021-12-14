//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

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
using System.Net.Http.Headers;
using System.Web.Http.Routing;
using System.Web.Http.Controllers;
using System.Collections;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Student;

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
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
        List<Dtos.FinancialAidApplicationOutcome> financialAidApplicationOutcomeDtos;
        int offset = 0;
        int limit = 200;
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

            financialAidApplicationOutcomeDtos = BuildData();
        }

        private List<Dtos.FinancialAidApplicationOutcome> BuildData()
        {
            List<Dtos.FinancialAidApplicationOutcome> financialAidApplications = new List<Dtos.FinancialAidApplicationOutcome>()
                {
                    new Dtos.FinancialAidApplicationOutcome()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        AidYear = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.FinancialAidApplicationOutcome()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        AidYear = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff")
                    },
                    new Dtos.FinancialAidApplicationOutcome()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        AidYear = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.FinancialAidApplicationOutcome()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        AidYear = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
            return financialAidApplications;
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

            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FinancialAidApplicationOutcome>(), It.IsAny<bool>())).ReturnsAsync(financialAidApplicationOutcomesTuple);
            
            var outcomes = await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesAsync(new Paging(3, 0), criteriaFilter);

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
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FinancialAidApplicationOutcome>(), It.IsAny<bool>())).ReturnsAsync(financialAidApplicationOutcomesTuple);

            var outcomes = (await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesAsync(new Web.Http.Models.Paging(3, 0), criteriaFilter));

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
            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetByIdAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomes_Exception()
        {
            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FinancialAidApplicationOutcome>(), It.IsAny<bool>())).Throws<Exception>();
            await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesAsync(new Web.Http.Models.Paging(3, 0), criteriaFilter);       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomesByGuidAsync_Exception()
        {
            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>(), false)).Throws<Exception>();
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

        [TestMethod]
        public async Task FinancialAidApplicationOutcomesController_GetAll_AidYearFilter()
        {
            financialAidApplicationOutcomesController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            var filterGroupName = "criteria";
            financialAidApplicationOutcomesController.Request.Properties.Add(
                  string.Format("FilterObject{0}", filterGroupName),
                  new FinancialAidApplicationOutcome() { AidYear = new GuidObject2("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a") });
            var tuple = new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(financialAidApplicationOutcomeDtos, 4);
            financialAidApplicationOutcomesServiceMock.Setup(ci => ci.GetAsync(offset, limit, It.IsAny<FinancialAidApplicationOutcome>(), true)).ReturnsAsync(tuple);
            var financialAidApplicationOutcomes = await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesAsync(new Paging(limit, offset), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await financialAidApplicationOutcomes.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.FinancialAidApplicationOutcome> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplicationOutcome>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.FinancialAidApplicationOutcome>;


            Assert.AreEqual(financialAidApplicationOutcomeDtos.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = financialAidApplicationOutcomeDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AidYear, actual.AidYear);
            }
        }


        //Get
        //Version 9.0.0 / 9.1.0
        //GetFinancialAidApplicationOutcomesAsync

        //Example success 
        [TestMethod]
        public async Task financialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomesAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetFinancialAidApplicationOutcomesAsync" }
                };
            HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            financialAidApplicationOutcomesController.Request.SetRouteData(data);
            financialAidApplicationOutcomesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewFinancialAidApplicationOutcomes);

            var controllerContext = financialAidApplicationOutcomesController.ControllerContext;
            var actionDescriptor = financialAidApplicationOutcomesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(financialAidApplicationOutcomeDtos, 5);
            financialAidApplicationOutcomesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FinancialAidApplicationOutcome>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var resp = await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesAsync(new Web.Http.Models.Paging(3, 0), criteriaFilter);

            Object filterObject;
            financialAidApplicationOutcomesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewFinancialAidApplicationOutcomes));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task financialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomesAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetFinancialAidApplicationOutcomesAsync" }
                };
            HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            financialAidApplicationOutcomesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = financialAidApplicationOutcomesController.ControllerContext;
            var actionDescriptor = financialAidApplicationOutcomesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(financialAidApplicationOutcomeDtos, 5);

                financialAidApplicationOutcomesServiceMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FinancialAidApplicationOutcome>(), It.IsAny<bool>())).Throws<PermissionsException>();
                financialAidApplicationOutcomesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view financial-aid-application-outcomes."));
                var resp = await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesAsync(new Web.Http.Models.Paging(3, 0), criteriaFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }



        //Get by Id
        //Version 9.0.0 / 9.1.0
        //GetFinancialAidApplicationOutcomesByGuidAsync

        //Example success 
        [TestMethod]
        public async Task financialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomesByGuidAsync_Permissions()
        {
            var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
            var financialAidApplicationOutcome = financialAidApplicationOutcomeDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetFinancialAidApplicationOutcomesByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            financialAidApplicationOutcomesController.Request.SetRouteData(data);
            financialAidApplicationOutcomesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.ViewFinancialAidApplicationOutcomes);

            var controllerContext = financialAidApplicationOutcomesController.ControllerContext;
            var actionDescriptor = financialAidApplicationOutcomesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(financialAidApplicationOutcomeDtos, 5);
            financialAidApplicationOutcomesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            financialAidApplicationOutcomesServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(financialAidApplicationOutcome);
            var resp = await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesByGuidAsync(id);

            Object filterObject;
            financialAidApplicationOutcomesController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewFinancialAidApplicationOutcomes));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task financialAidApplicationOutcomesController_GetFinancialAidApplicationOutcomesByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionApplications" },
                    { "action", "GetFinancialAidApplicationOutcomesByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            financialAidApplicationOutcomesController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = financialAidApplicationOutcomesController.ControllerContext;
            var actionDescriptor = financialAidApplicationOutcomesController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int>(financialAidApplicationOutcomeDtos, 5);

                financialAidApplicationOutcomesServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                financialAidApplicationOutcomesServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view financial-aid-application-outcomes."));
                var resp = await financialAidApplicationOutcomesController.GetFinancialAidApplicationOutcomesByGuidAsync(string.Empty);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

    }
}