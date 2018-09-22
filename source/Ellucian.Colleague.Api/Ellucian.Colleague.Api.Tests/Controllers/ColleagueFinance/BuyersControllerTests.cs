//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class BuyersControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IBuyersService> buyersServiceMock;
        private Mock<ILogger> loggerMock;
        private BuyersController buyersController;
        private IEnumerable<Dtos.Buyers> buyersDtoCollection;
        private Tuple<IEnumerable<Dtos.Buyers>, int> buyersTuple;
        private string expectedGuid = "ef11bd15-ebeb-4e9e-9b8a-d14c5ff2b097";
        int offset = 0;
        int limit = 3;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            buyersServiceMock = new Mock<IBuyersService>();
            loggerMock = new Mock<ILogger>();
            
            BuildData();

            buyersController = new BuyersController(buyersServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            buyersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            buyersController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
        }

        private void BuildData()
        {
            buyersDtoCollection = new List<Buyers>() 
            {
                new Buyers()
                {
                    Id = "ef11bd15-ebeb-4e9e-9b8a-d14c5ff2b097",
                    EndOn = DateTime.Today.AddDays(30),
                    Buyer = new Dtos.DtoProperties.NamePersonDtoProperty()
                    {
                    Name = "First1, Last1",
                    Person = new GuidObject2("ae80b166-a42a-48eb-b058-b648d7dbbb1f")
                    },
                        Status = Dtos.EnumProperties.BuyerStatus.Active,
                    StartOn = DateTime.Today                    
                },
                new Buyers()
                {
                    Id = "fd9b4a83-b13a-47a2-9a09-65e1b9149678",
                    EndOn = DateTime.Today.AddDays(30),
                     Buyer = new Dtos.DtoProperties.NamePersonDtoProperty()
                    { Name = "First2, Last2",
                    Person = new GuidObject2("f727117b-726b-4e4a-9e92-9630f58fd38a")
                     },
                         Status = Dtos.EnumProperties.BuyerStatus.Active,
                    StartOn = DateTime.Today.AddDays(1)
                },
                new Buyers()
                {
                    Id = "9a9859bd-9b04-4973-b413-4bb492fbfb87",
                    EndOn = DateTime.Today.AddDays(30),
                     Buyer = new Dtos.DtoProperties.NamePersonDtoProperty()
                    {
                         Name = "First3, Last3",
                    Person = new GuidObject2("3b90722e-5052-4b72-b0fa-c86592ae1096")
                     },
                         Status = Dtos.EnumProperties.BuyerStatus.Active,
                    StartOn = DateTime.Today.AddDays(2)
                }
            };
            buyersTuple = new Tuple<IEnumerable<Buyers>, int>(buyersDtoCollection, buyersDtoCollection.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            buyersController = null;
            buyersDtoCollection = null;
            loggerMock = null;
            buyersServiceMock = null;
        }

        [TestMethod]
        public async Task BuyersController_GetBuyers_ValidateFields_Nocache()
        {
            buyersController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            buyersServiceMock.Setup(x => x.GetBuyersAsync(offset, limit, false)).ReturnsAsync(buyersTuple);

            var results = await buyersController.GetBuyersAsync(new Paging(limit, offset));
            Assert.IsNotNull(results);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.Buyers> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Buyers>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.Buyers>;

            foreach (var actual in actuals)
            {
                var expected = buyersDtoCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                if (actual.Buyer != null)
                {
                    if (actual.Buyer.Person != null)
                    {
                        Assert.AreEqual(expected.Buyer.Person.Id, actual.Buyer.Person.Id);
                    }
                    else
                    {
                        Assert.AreEqual(expected.Buyer.Name, actual.Buyer.Name);
                    }
                }
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
            }
        }

        [TestMethod]
        public async Task BuyersController_GetBuyers_ValidateFields_cache()
        {
            buyersController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            buyersServiceMock.Setup(x => x.GetBuyersAsync(offset, limit, true)).ReturnsAsync(buyersTuple);

            var results = await buyersController.GetBuyersAsync(new Paging(limit, offset));
            Assert.IsNotNull(results);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.Buyers> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Buyers>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.Buyers>;

            foreach (var actual in actuals)
            {
                var expected = buyersDtoCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                if (actual.Buyer != null)
                {
                    if (actual.Buyer.Person != null)
                    {
                        Assert.AreEqual(expected.Buyer.Person.Id, actual.Buyer.Person.Id);
                    }
                    else
                    {
                        Assert.AreEqual(expected.Buyer.Name, actual.Buyer.Name);
                    }
                }
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
            }
        }

        [TestMethod]
        public async Task BuyersController_GetBuyers_ValidateFields_NullPage()
        {
            buyersController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            buyersServiceMock.Setup(x => x.GetBuyersAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(buyersTuple);

            var results = await buyersController.GetBuyersAsync(null);
            Assert.IsNotNull(results);

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.Buyers> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Buyers>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.Buyers>;

            foreach (var actual in actuals)
            {
                var expected = buyersDtoCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                if (actual.Buyer != null)
                {
                    if (actual.Buyer.Person != null)
                    {
                        Assert.AreEqual(expected.Buyer.Person.Id, actual.Buyer.Person.Id);
                    }
                    else
                    {
                        Assert.AreEqual(expected.Buyer.Name, actual.Buyer.Name);
                    }
                }
                Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
            }
        }

        [TestMethod]
        public async Task BuyersController_GetBuyersById()
        {
            buyersController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = buyersDtoCollection.FirstOrDefault(i => i.Id.Equals(expectedGuid, StringComparison.OrdinalIgnoreCase));

            buyersServiceMock.Setup(x => x.GetBuyersByGuidAsync(expectedGuid)).ReturnsAsync(expected);

            var actual = await buyersController.GetBuyersByGuidAsync(expectedGuid);
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Id, actual.Id);
            if (actual.Buyer != null)
            {
                if (actual.Buyer.Person != null)
                {
                    Assert.AreEqual(expected.Buyer.Person.Id, actual.Buyer.Person.Id);
                }
                else
                {
                    Assert.AreEqual(expected.Buyer.Name, actual.Buyer.Name);
                }
            }
            Assert.AreEqual(expected.EndOn.Value, actual.EndOn.Value);
            Assert.AreEqual(expected.StartOn, actual.StartOn);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyers_KeyNotFoundException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await buyersController.GetBuyersAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyers_PermissionsException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await buyersController.GetBuyersAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyers_ArgumentException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await buyersController.GetBuyersAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyers_RepositoryException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await buyersController.GetBuyersAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyers_IntegrationApiException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await buyersController.GetBuyersAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyers_Exception()
        {
            buyersServiceMock.Setup(x => x.GetBuyersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<Exception>();
            await buyersController.GetBuyersAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyersByGuidAsync_NoId()
        {
            buyersServiceMock.Setup(x => x.GetBuyersByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await buyersController.GetBuyersByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyersByGuidAsync_KeyNotFoundException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await buyersController.GetBuyersByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyersByGuidAsync_PermissionsException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await buyersController.GetBuyersByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyersByGuidAsync_ArgumentException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await buyersController.GetBuyersByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyersByGuidAsync_RepositoryException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await buyersController.GetBuyersByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyersByGuidAsync_IntegrationApiException()
        {
            buyersServiceMock.Setup(x => x.GetBuyersByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await buyersController.GetBuyersByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_GetBuyersByGuidAsync_Exception()
        {
            buyersServiceMock.Setup(x => x.GetBuyersByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await buyersController.GetBuyersByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_PostBuyersAsync_Exception()
        {
            await buyersController.PostBuyersAsync(buyersDtoCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_PutBuyersAsync_Exception()
        {
            var sourceContext = buyersDtoCollection.FirstOrDefault();
            await buyersController.PutBuyersAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BuyersController_DeleteBuyersAsync_Exception()
        {
            await buyersController.DeleteBuyersAsync(buyersDtoCollection.FirstOrDefault().Id);
        }
    }
}
