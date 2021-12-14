//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    //[TestClass]
    //public class ExternalEmploymentsControllerTests
    //{
    //    /// <summary>
    //    ///Gets or sets the test context which provides
    //    ///information about and functionality for the current test run.
    //    ///</summary>
    //    public TestContext TestContext { get; set; }

    //    private Mock<IExternalEmploymentsService> externalEmploymentsServiceMock;
    //    private Mock<ILogger> loggerMock;
    //    private ExternalEmploymentsController externalEmploymentsController;
    //    private IEnumerable<Domain.Base.Entities.ExternalEmployments> allEmploymt;
    //    private List<Dtos.ExternalEmployments> externalEmploymentsCollection;
    //    private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
    //    int offset = 0;
    //    int limit = 200;

    //    [TestInitialize]
    //    public void Initialize()
    //    {
    //        LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
    //        EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

    //        externalEmploymentsServiceMock = new Mock<IExternalEmploymentsService>();
    //        loggerMock = new Mock<ILogger>();
    //        externalEmploymentsCollection = new List<Dtos.ExternalEmployments>();

    //        allEmploymt = new List<Domain.Base.Entities.ExternalEmployments>()
    //            {
    //                new Domain.Base.Entities.ExternalEmployments("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "1", "AT", "Athletic"),
    //                new Domain.Base.Entities.ExternalEmployments("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2", "2", "AC", "Academic"),
    //                new Domain.Base.Entities.ExternalEmployments("d2253ac7-9931-4560-b42f-1fccd43c952e", "3", "3", "CU", "Cultural")
    //            };

    //        foreach (var source in allEmploymt)
    //        {
    //            var externalEmployments = new Ellucian.Colleague.Dtos.ExternalEmployments
    //            {
    //                Id = source.Guid,
    //                //Code = source.Code,
    //                //Title = source.Description,
    //                //Description = null
    //            };
    //            externalEmploymentsCollection.Add(externalEmployments);
    //        }

    //        externalEmploymentsController = new ExternalEmploymentsController(externalEmploymentsServiceMock.Object, loggerMock.Object)
    //        {
    //            Request = new HttpRequestMessage()
    //        };
    //        externalEmploymentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    //    }

    //    [TestCleanup]
    //    public void Cleanup()
    //    {
    //        externalEmploymentsController = null;
    //        allEmploymt = null;
    //        externalEmploymentsCollection = null;
    //        loggerMock = null;
    //        externalEmploymentsServiceMock = null;
    //    }

    //    [TestMethod]
    //    public async Task ExternalEmploymentsController_GetExternalEmployments_ValidateFields_Nocache()
    //    {
    //        externalEmploymentsController.Request.Headers.CacheControl =
    //             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

    //        var tuple = new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentsCollection, 3);
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsAsync(offset, limit, false)).ReturnsAsync(tuple);

    //        var sourceContexts = (await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset)));
    //        Assert.AreEqual(externalEmploymentsCollection.Count, sourceContexts.Count);
    //        for (var i = 0; i < sourceContexts.Count; i++)
    //        {
    //            var expected = externalEmploymentsCollection[i];
    //            var actual = sourceContexts[i];
    //            Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
    //            Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
    //            Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
    //        }
    //    }

    //    [TestMethod]
    //    public async Task ExternalEmploymentsController_GetExternalEmployments_ValidateFields_Cache()
    //    {
    //        externalEmploymentsController.Request.Headers.CacheControl =
    //            new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsAsync(true)).ReturnsAsync(externalEmploymentsCollection);

    //        var sourceContexts = (await externalEmploymentsController.GetExternalEmploymentsAsync()).ToList();
    //        Assert.AreEqual(externalEmploymentsCollection.Count, sourceContexts.Count);
    //        for (var i = 0; i < sourceContexts.Count; i++)
    //        {
    //            var expected = externalEmploymentsCollection[i];
    //            var actual = sourceContexts[i];
    //            Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
    //            Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
    //            Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
    //        }
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmployments_KeyNotFoundException()
    //    {
    //        //
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsAsync(false))
    //            .Throws<KeyNotFoundException>();
    //        await externalEmploymentsController.GetExternalEmploymentsAsync();
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmployments_PermissionsException()
    //    {

    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsAsync(false))
    //            .Throws<PermissionsException>();
    //        await externalEmploymentsController.GetExternalEmploymentsAsync();
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmployments_ArgumentException()
    //    {

    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsAsync(false))
    //            .Throws<ArgumentException>();
    //        await externalEmploymentsController.GetExternalEmploymentsAsync();
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmployments_RepositoryException()
    //    {

    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsAsync(false))
    //            .Throws<RepositoryException>();
    //        await externalEmploymentsController.GetExternalEmploymentsAsync();
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmployments_IntegrationApiException()
    //    {

    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsAsync(false))
    //            .Throws<IntegrationApiException>();
    //        await externalEmploymentsController.GetExternalEmploymentsAsync();
    //    }

    //    [TestMethod]
    //    public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_ValidateFields()
    //    {
    //        var expected = externalEmploymentsCollection.FirstOrDefault();
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(expected.Id)).ReturnsAsync(expected);

    //        var actual = await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(expected.Id);

    //        Assert.AreEqual(expected.Id, actual.Id, "Id");
    //        Assert.AreEqual(expected.Title, actual.Title, "Title");
    //        Assert.AreEqual(expected.Code, actual.Code, "Code");
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmployments_Exception()
    //    {
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsAsync(false)).Throws<Exception>();
    //        await externalEmploymentsController.GetExternalEmploymentsAsync();
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_Exception()
    //    {
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
    //        await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(string.Empty);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuid_KeyNotFoundException()
    //    {
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>()))
    //            .Throws<KeyNotFoundException>();
    //        await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(expectedGuid);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuid_PermissionsException()
    //    {
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>()))
    //            .Throws<PermissionsException>();
    //        await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(expectedGuid);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuid_ArgumentException()
    //    {
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>()))
    //            .Throws<ArgumentException>();
    //        await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(expectedGuid);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuid_RepositoryException()
    //    {
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>()))
    //            .Throws<RepositoryException>();
    //        await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(expectedGuid);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuid_IntegrationApiException()
    //    {
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>()))
    //            .Throws<IntegrationApiException>();
    //        await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(expectedGuid);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuid_Exception()
    //    {
    //        externalEmploymentsServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>()))
    //            .Throws<Exception>();
    //        await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(expectedGuid);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_PostExternalEmploymentsAsync_Exception()
    //    {
    //        await externalEmploymentsController.PostExternalEmploymentsAsync(externalEmploymentsCollection.FirstOrDefault());
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_PutExternalEmploymentsAsync_Exception()
    //    {
    //        var sourceContext = externalEmploymentsCollection.FirstOrDefault();
    //        await externalEmploymentsController.PutExternalEmploymentsAsync(sourceContext.Id, sourceContext);
    //    }

    //    [TestMethod]
    //    [ExpectedException(typeof(HttpResponseException))]
    //    public async Task ExternalEmploymentsController_DeleteExternalEmploymentsAsync_Exception()
    //    {
    //        await externalEmploymentsController.DeleteExternalEmploymentsAsync(externalEmploymentsCollection.FirstOrDefault().Id);
    //    }
    //}

    [TestClass]
    public class ExternalEmploymentsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IExternalEmploymentsService> externalEmploymentServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            ExternalEmploymentsController externalEmploymentsController;
            List<Dtos.ExternalEmployments> externalEmploymentDtos;
            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                externalEmploymentServiceMock = new Mock<IExternalEmploymentsService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                externalEmploymentDtos = BuildData();

                externalEmploymentsController = new ExternalEmploymentsController(externalEmploymentServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                externalEmploymentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                externalEmploymentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private List<Dtos.ExternalEmployments> BuildData()
            {
                List<Dtos.ExternalEmployments> externalEmployments = new List<Dtos.ExternalEmployments>() 
                {
                    new Dtos.ExternalEmployments()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", 
                        Person = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"),
                        StartOn = new DateDtoProperty(),
                        EndOn = new DateDtoProperty(),
                        Status = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        //Owner = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.ExternalEmployments()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b", 
                        Person = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff"),
                        StartOn = new DateDtoProperty(),
                        EndOn = new DateDtoProperty(),
                        Status = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        //Owner = new Dtos.GuidObject2("0cva17h3-er23-5796-cb9a-32f5tdh065yf")
                    },
                    new Dtos.ExternalEmployments()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Person = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        StartOn = new DateDtoProperty(),
                        EndOn = new DateDtoProperty(),
                        Status = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"),
                        //Owner = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.ExternalEmployments()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136", 
                        Person = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"),
                        StartOn = new DateDtoProperty(),
                        EndOn = new DateDtoProperty(),
                        Status = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        //Owner = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
                return externalEmployments;
            }

            [TestCleanup]
            public void Cleanup()
            {
                externalEmploymentsController = null;
                externalEmploymentDtos = null;
                externalEmploymentServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task ExternalEmploymentsController_GetAll_NoCache_True()
            {
                externalEmploymentsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentDtos, 4);
                externalEmploymentServiceMock.Setup(ci => ci.GetExternalEmploymentsAsync(offset, limit, true)).ReturnsAsync(tuple);
                var externalEmployments = await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await externalEmployments.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.ExternalEmployments> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmployments>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.ExternalEmployments>;


                Assert.AreEqual(externalEmploymentDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = externalEmploymentDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Person, actual.Person);
                    //Assert.AreEqual(expected.Type, actual.Type);
                    //Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task ExternalEmploymentsController_GetAll_NoCache_False()
            {
                externalEmploymentsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentDtos, 4);
                externalEmploymentServiceMock.Setup(ci => ci.GetExternalEmploymentsAsync(offset, limit, false)).ReturnsAsync(tuple);
                var externalEmployments = await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await externalEmployments.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.ExternalEmployments> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.ExternalEmployments>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.ExternalEmployments>;


                Assert.AreEqual(externalEmploymentDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = externalEmploymentDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.Person, actual.Person);
                    //Assert.AreEqual(expected.Type, actual.Type);
                    //Assert.AreEqual(expected.Owner, actual.Owner);
                }
            }

            [TestMethod]
            public async Task ExternalEmploymentsController_GetById()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var externalEmployment = externalEmploymentDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                externalEmploymentServiceMock.Setup(ci => ci.GetExternalEmploymentsByGuidAsync(id)).ReturnsAsync(externalEmployment);

                var actual = await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(id);

                var expected = externalEmploymentDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Person, actual.Person);
                //Assert.AreEqual(expected.Type, actual.Type);
                //Assert.AreEqual(expected.Owner, actual.Owner);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetAll_Exception()
            {
                externalEmploymentsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentDtos, 4);
                externalEmploymentServiceMock.Setup(ci => ci.GetExternalEmploymentsAsync(offset, limit, false)).ThrowsAsync(new Exception());
                var externalEmployments = await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetById_Exception()
            {
                externalEmploymentServiceMock.Setup(ci => ci.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actual = await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetById_KeyNotFoundException()
            {
                externalEmploymentServiceMock.Setup(ci => ci.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_PUT_Not_Supported()
            {
                var actual = await externalEmploymentsController.PutExternalEmploymentsAsync(It.IsAny<string>(), It.IsAny<Dtos.ExternalEmployments>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_POST_Not_Supported()
            {
                var actual = await externalEmploymentsController.PostExternalEmploymentsAsync(It.IsAny<Dtos.ExternalEmployments>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_DELETE_Not_Supported()
            {
                await externalEmploymentsController.DeleteExternalEmploymentsAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmployments_KeyNotFoundException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsAsync(offset, limit, It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmployments_PermissionsException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsAsync(offset, limit, It.IsAny<bool>())).Throws<PermissionsException>();
                await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmployments_ArgumentException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsAsync(offset, limit, It.IsAny<bool>())).Throws<ArgumentException>();
                await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmployments_RepositoryException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsAsync(offset, limit, It.IsAny<bool>())).Throws<RepositoryException>();
                await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmployments_IntegrationApiException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsAsync(offset, limit, It.IsAny<bool>())).Throws<IntegrationApiException>();
                await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_KeyNotFoundException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
                await externalEmploymentsController.GetExternalEmploymentsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_PermissionsException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
                await externalEmploymentsController.GetExternalEmploymentsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_ArgumentException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
                await externalEmploymentsController.GetExternalEmploymentsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_RepositoryException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
                await externalEmploymentsController.GetExternalEmploymentsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_IntegrationApiException()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
                await externalEmploymentsController.GetExternalEmploymentsByGuidAsync("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_NoId_Exception()
            {
                externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
                await externalEmploymentsController.GetExternalEmploymentsByGuidAsync("");
            }

            //Permissions

            //Success
            // Get 10
            //GetExternalEmploymentsAsync

            [TestMethod]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "ExternalEmployments" },
                    { "action", "GetExternalEmploymentsAsync" }
                };
                HttpRoute route = new HttpRoute("external-employments", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                externalEmploymentsController.Request.SetRouteData(data);
                externalEmploymentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(BasePermissionCodes.ViewAnyExternalEmployments);

                var controllerContext = externalEmploymentsController.ControllerContext;
                var actionDescriptor = externalEmploymentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentDtos, 4);
                externalEmploymentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                externalEmploymentServiceMock.Setup(ci => ci.GetExternalEmploymentsAsync(offset, limit, false)).ReturnsAsync(tuple);
                var externalEmployments = await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));

                Object filterObject;
                externalEmploymentsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAnyExternalEmployments));
            }

            //Exception
            //Get 10
            //GetExternalEmploymentsAsync

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "ExternalEmployments" },
                    { "action", "GetExternalEmploymentsAsync" }
                };
                HttpRoute route = new HttpRoute("external-employments", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                externalEmploymentsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = externalEmploymentsController.ControllerContext;
                var actionDescriptor = externalEmploymentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentDtos, 5);

                    externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsAsync(offset, limit, It.IsAny<bool>())).Throws<PermissionsException>();
                    externalEmploymentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view external-employments."));
                    await externalEmploymentsController.GetExternalEmploymentsAsync(new Paging(limit, offset));
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Success
            // Get By Id 10
            //GetExternalEmploymentsByGuidAsync

            [TestMethod]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_Permissions()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var externalEmployment = externalEmploymentDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "ExternalEmployments" },
                    { "action", "GetExternalEmploymentsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("external-employments", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                externalEmploymentsController.Request.SetRouteData(data);
                externalEmploymentsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(BasePermissionCodes.ViewAnyExternalEmployments);

                var controllerContext = externalEmploymentsController.ControllerContext;
                var actionDescriptor = externalEmploymentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentDtos, 5);
                externalEmploymentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                externalEmploymentServiceMock.Setup(ci => ci.GetExternalEmploymentsByGuidAsync(id)).ReturnsAsync(externalEmployment);
                var actual = await externalEmploymentsController.GetExternalEmploymentsByGuidAsync(id);

                Object filterObject;
                externalEmploymentsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(BasePermissionCodes.ViewAnyExternalEmployments));
            }

            //Exception
            //Get By Id 10
            //GetExternalEmploymentsByGuidAsync

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExternalEmploymentsController_GetExternalEmploymentsByGuidAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "ExternalEmployments" },
                    { "action", "GetExternalEmploymentsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("external-employments", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                externalEmploymentsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = externalEmploymentsController.ControllerContext;
                var actionDescriptor = externalEmploymentsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.ExternalEmployments>, int>(externalEmploymentDtos, 5);

                    externalEmploymentServiceMock.Setup(x => x.GetExternalEmploymentsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
                    externalEmploymentServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view external-employments."));
                    await externalEmploymentsController.GetExternalEmploymentsByGuidAsync("1234");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }
    }
}