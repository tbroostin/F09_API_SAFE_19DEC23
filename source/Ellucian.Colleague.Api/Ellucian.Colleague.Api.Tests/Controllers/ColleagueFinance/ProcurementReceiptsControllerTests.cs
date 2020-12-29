//Copyright 2018-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class ProcurementReceiptsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IProcurementReceiptsService> procurementReceiptsServiceMock;
        private Mock<ILogger> loggerMock;
        private ProcurementReceiptsController procurementReceiptsController;      
        private IEnumerable<Domain.ColleagueFinance.Entities.PurchaseOrderReceipt> allPurchaseorderreceipt;
        private List<Dtos.ProcurementReceipts> procurementReceiptsCollection;
        private Tuple<IEnumerable<Dtos.ProcurementReceipts>, int> procurementTuple;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private int offset = 0, limit = 3;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter;
        private Dtos.ProcurementReceipts procurementReceiptsFilter;

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            procurementReceiptsFilter = new ProcurementReceipts();

            procurementReceiptsServiceMock = new Mock<IProcurementReceiptsService>();
            loggerMock = new Mock<ILogger>();
            procurementReceiptsCollection = new List<Dtos.ProcurementReceipts>();

            allPurchaseorderreceipt  = new List<Domain.ColleagueFinance.Entities.PurchaseOrderReceipt>()
                {
                    new Domain.ColleagueFinance.Entities.PurchaseOrderReceipt("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                    new Domain.ColleagueFinance.Entities.PurchaseOrderReceipt("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d"),
                    new Domain.ColleagueFinance.Entities.PurchaseOrderReceipt("d2253ac7-9931-4560-b42f-1fccd43c952e")
                };
            
            foreach (var source in allPurchaseorderreceipt)
            {
                var procurementReceipts = new Ellucian.Colleague.Dtos.ProcurementReceipts
                {
                    Id = source.Guid
                };
                procurementReceiptsCollection.Add(procurementReceipts);
            }
            procurementTuple = new Tuple<IEnumerable<ProcurementReceipts>, int>(procurementReceiptsCollection, procurementReceiptsCollection.Count());
            procurementReceiptsController = new ProcurementReceiptsController(procurementReceiptsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            procurementReceiptsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            procurementReceiptsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

        }

        [TestCleanup]
        public void Cleanup()
        {
            procurementReceiptsController = null;
            allPurchaseorderreceipt = null;
            procurementReceiptsCollection = null;
            loggerMock = null;
            procurementReceiptsServiceMock = null;
            procurementReceiptsFilter = null;
        }

        [TestMethod]
        public async Task ProcurementReceiptsController_GetProcurementReceipts_ValidateFields_Nocache()
        {
            procurementReceiptsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsAsync(0, 100, It.IsAny<ProcurementReceipts>(), true)).ReturnsAsync(procurementTuple);
       
            var sourceContexts = await procurementReceiptsController.GetProcurementReceiptsAsync(null, criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            List<ProcurementReceipts> ActualsAPI = ((ObjectContent<IEnumerable<ProcurementReceipts>>)httpResponseMessage.Content).Value as List<ProcurementReceipts>;

            Assert.AreEqual(procurementReceiptsCollection.Count, ActualsAPI.Count);

            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = procurementReceiptsCollection[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());                
            }
        }

        [TestMethod]
        public async Task ProcurementReceiptsController_GetProcurementReceipts_ValidateFields_Cache()
        {
            procurementReceiptsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsAsync(offset, limit, It.IsAny<ProcurementReceipts>(), false)).ReturnsAsync(procurementTuple);

            var sourceContexts = await procurementReceiptsController.GetProcurementReceiptsAsync(new Web.Http.Models.Paging(limit, offset), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            List<ProcurementReceipts> ActualsAPI = ((ObjectContent<IEnumerable<ProcurementReceipts>>)httpResponseMessage.Content).Value as List<ProcurementReceipts>;

            Assert.AreEqual(procurementReceiptsCollection.Count, ActualsAPI.Count);

            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = procurementReceiptsCollection[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceipts_KeyNotFoundException()
        {
            //
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            await procurementReceiptsController.GetProcurementReceiptsAsync(It.IsAny<Paging>(), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceipts_PermissionsException()
        {

            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            await procurementReceiptsController.GetProcurementReceiptsAsync(It.IsAny<Paging>(), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceipts_ArgumentException()
        {

            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            await procurementReceiptsController.GetProcurementReceiptsAsync(It.IsAny<Paging>(), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceipts_RepositoryException()
        {

            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            await procurementReceiptsController.GetProcurementReceiptsAsync(It.IsAny<Paging>(), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceipts_IntegrationApiException()
        {

            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            await procurementReceiptsController.GetProcurementReceiptsAsync(It.IsAny<Paging>(), criteriaFilter);
        }

        [TestMethod]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsByGuidAsync_ValidateFields()
        {
            procurementReceiptsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            var expected = procurementReceiptsCollection.FirstOrDefault();
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await procurementReceiptsController.GetProcurementReceiptsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceipts_Exception()
        {
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(),  It.IsAny<bool>())).ThrowsAsync(new Exception());
            await procurementReceiptsController.GetProcurementReceiptsAsync(It.IsAny<Paging>(), criteriaFilter);
        }


        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsAsync_InvalidFilter()
        {
            var contextSuffix = "criteria";
            var queryStringCriteria = new QueryStringFilter(contextSuffix, "{'packingSlipNumber':\'" + Guid.NewGuid().ToString() + "\'}");
            try
            {
                var queryStringFilter = new QueryStringFilterFilter(contextSuffix, typeof(Dtos.ProcurementReceipts));

                var controllerContext = procurementReceiptsController.ControllerContext;
                var actionDescriptor = procurementReceiptsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);

                _context.ActionArguments.Add(contextSuffix, queryStringCriteria);

                await queryStringFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            }
            catch (Exception ex)
            {
                Assert.AreEqual("'packingSlipNumber' is an invalid property on the schema.", ex.Message.Trim());
                throw ex;
            }
        }

        [TestMethod]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsAsync_ValidFilter()
        {
            var contextPrefix = "FilterObject";
            var contextSuffix = "criteria";

            var queryStringCriteria = new QueryStringFilter(contextSuffix, "{'purchaseOrder':{'id':\'" + Guid.NewGuid().ToString() + "\'}}");

            var contextPropertyName = string.Format("{0}{1}", contextPrefix, contextSuffix);

            var queryStringFilter = new QueryStringFilterFilter(contextSuffix, typeof(Dtos.ProcurementReceipts));

            var controllerContext = procurementReceiptsController.ControllerContext;
            var actionDescriptor = procurementReceiptsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);

            _context.ActionArguments.Add(contextSuffix, queryStringCriteria);

            await queryStringFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.ProcurementReceipts>(), It.IsAny<bool>()))
                .ReturnsAsync(procurementTuple);


            var sourceContexts = await procurementReceiptsController.GetProcurementReceiptsAsync(new Paging(0, 1), queryStringCriteria);


            Object filterObject;
            procurementReceiptsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);

            Assert.IsNotNull(filterObject);


            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);
            var actualsAPI = ((ObjectContent<IEnumerable<ProcurementReceipts>>)httpResponseMessage.Content).Value as List<ProcurementReceipts>;

            Assert.AreEqual(procurementReceiptsCollection.Count, actualsAPI.Count);
        }
        

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsByGuidAsync_Exception()
        {
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
            await procurementReceiptsController.GetProcurementReceiptsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsByGuid_KeyNotFoundException()
        {
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            await procurementReceiptsController.GetProcurementReceiptsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsByGuid_PermissionsException()
        {
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            await procurementReceiptsController.GetProcurementReceiptsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsByGuid_ArgumentException()
        {
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            await procurementReceiptsController.GetProcurementReceiptsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsByGuid_RepositoryException()
        {
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            await procurementReceiptsController.GetProcurementReceiptsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsByGuid_IntegrationApiException()
        {
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            await procurementReceiptsController.GetProcurementReceiptsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_GetProcurementReceiptsByGuid_Exception()
        {
            procurementReceiptsServiceMock.Setup(x => x.GetProcurementReceiptsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            await procurementReceiptsController.GetProcurementReceiptsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_Exception()
        {
            await procurementReceiptsController.PostProcurementReceiptsAsync(procurementReceiptsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_Null_Input_Exception()
        {
            await procurementReceiptsController.PostProcurementReceiptsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_Null_InputId_Exception()
        {
            await procurementReceiptsController.PostProcurementReceiptsAsync(new Dtos.ProcurementReceipts());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_KeyNotFoundException()
        {
            procurementReceiptsServiceMock.Setup(x => x.CreateProcurementReceiptsAsync(It.IsAny<Dtos.ProcurementReceipts>())).ThrowsAsync(new KeyNotFoundException());
            await procurementReceiptsController.PostProcurementReceiptsAsync(new Dtos.ProcurementReceipts() { Id = Guid.Empty.ToString()});
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_PermissionsException()
        {
            procurementReceiptsServiceMock.Setup(x => x.CreateProcurementReceiptsAsync(It.IsAny<Dtos.ProcurementReceipts>())).ThrowsAsync(new PermissionsException());
            await procurementReceiptsController.PostProcurementReceiptsAsync(new Dtos.ProcurementReceipts() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_ArgumentException()
        {
            procurementReceiptsServiceMock.Setup(x => x.CreateProcurementReceiptsAsync(It.IsAny<Dtos.ProcurementReceipts>())).ThrowsAsync(new ArgumentException());
            await procurementReceiptsController.PostProcurementReceiptsAsync(new Dtos.ProcurementReceipts() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_RepositoryException()
        {
            procurementReceiptsServiceMock.Setup(x => x.CreateProcurementReceiptsAsync(It.IsAny<Dtos.ProcurementReceipts>())).ThrowsAsync(new RepositoryException());

            await procurementReceiptsController.PostProcurementReceiptsAsync(new Dtos.ProcurementReceipts() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_IntegrationApiException()
        {
            procurementReceiptsServiceMock.Setup(x => x.CreateProcurementReceiptsAsync(It.IsAny<Dtos.ProcurementReceipts>())).ThrowsAsync(new IntegrationApiException());

            await procurementReceiptsController.PostProcurementReceiptsAsync(new Dtos.ProcurementReceipts() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_ConfigurationException()
        {
            procurementReceiptsServiceMock.Setup(x => x.CreateProcurementReceiptsAsync(It.IsAny<Dtos.ProcurementReceipts>())).ThrowsAsync(new ConfigurationException());

            await procurementReceiptsController.PostProcurementReceiptsAsync(new Dtos.ProcurementReceipts() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PostProcurementReceiptsAsync_Exception1()
        {
            procurementReceiptsServiceMock.Setup(x => x.CreateProcurementReceiptsAsync(It.IsAny<Dtos.ProcurementReceipts>())).ThrowsAsync(new Exception());

            await procurementReceiptsController.PostProcurementReceiptsAsync(new Dtos.ProcurementReceipts() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_PutProcurementReceiptsAsync_Exception()
        {
            var sourceContext = procurementReceiptsCollection.FirstOrDefault();
            await procurementReceiptsController.PutProcurementReceiptsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReceiptsController_DeleteProcurementReceiptsAsync_Exception()
        {
            await procurementReceiptsController.DeleteProcurementReceiptsAsync(procurementReceiptsCollection.FirstOrDefault().Id);
        }
    }
}