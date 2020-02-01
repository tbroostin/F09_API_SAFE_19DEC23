// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
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
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class RelationshipTypesControllerTests
    {
        //[TestClass]
        //public class RelationshipTypesControllerGet
        //{
        //    #region Test Context

        //    private TestContext testContextInstance;

        //    /// <summary>
        //    ///Gets or sets the test context which provides
        //    ///information about and functionality for the current test run.
        //    ///</summary>
        //    public TestContext TestContext
        //    {
        //        get
        //        {
        //            return testContextInstance;
        //        }
        //        set
        //        {
        //            testContextInstance = value;
        //        }
        //    }

        //    #endregion

        //    private RelationshipTypesController relationshipTypesController;

        //    private Mock<IReferenceDataRepository> refDataRepoMock;
        //    private IReferenceDataRepository refDataRepo;

        //    private IAdapterRegistry AdapterRegistry;

        //    private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RelationshipType> allRelTypes;

        //    ILogger logger = new Mock<ILogger>().Object;

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
        //        EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

        //        refDataRepoMock = new Mock<IReferenceDataRepository>();
        //        refDataRepo = refDataRepoMock.Object;

        //        HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
        //        AdapterRegistry = new AdapterRegistry(adapters, logger);
        //        var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RelationshipType, RelationshipType>(AdapterRegistry, logger);
        //        AdapterRegistry.AddAdapter(testAdapter);

        //        allRelTypes = new TestRelationshipTypeRepository().Get();
        //        var relTypeList = new List<RelationshipType>();

        //        relationshipTypesController = new RelationshipTypesController(AdapterRegistry, refDataRepo);
        //        Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.RelationshipType, RelationshipType>();
        //        foreach (var RelationshipType in allRelTypes)
        //        {
        //            RelationshipType target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.RelationshipType, RelationshipType>(RelationshipType);
        //            relTypeList.Add(target);
        //        }
        //        refDataRepoMock.Setup(x => x.GetRelationshipTypesAsync()).ReturnsAsync(allRelTypes);
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        relationshipTypesController = null;
        //        refDataRepo = null;
        //    }


        //    [TestMethod]
        //    public async Task ReturnsAllRelationshipTypesAsync()
        //    {
        //        var relationshipTypes = await relationshipTypesController.GetAsync();
        //        Assert.AreEqual(relationshipTypes.Count(), allRelTypes.Count());
        //    }

        //}

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private IReferenceDataRepository referenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        
        private Mock<ILogger> loggerMock;
        private RelationshipTypesController relationshipTypesController;
        private IEnumerable<Domain.Base.Entities.RelationType> allRelationTypes;
        private List<Dtos.RelationshipTypes> relationshipTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        private IRelationshipTypesService relationshipTypesService;
        private Mock<IRelationshipTypesService> relationshipTypesServiceMock;

        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            relationshipTypesServiceMock = new Mock<IRelationshipTypesService>();
            loggerMock = new Mock<ILogger>();
            relationshipTypesCollection = new List<Dtos.RelationshipTypes>();
            
            relationshipTypesService = relationshipTypesServiceMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Dtos.Base.Country>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Dtos.Base.Country>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            allRelationTypes = new List<Domain.Base.Entities.RelationType>()
                {
                    new Domain.Base.Entities.RelationType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "C", "Child", "", "P"),
                    new Domain.Base.Entities.RelationType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CO", "Cousin", "", "CO"),
                    new Domain.Base.Entities.RelationType("d2253ac7-9931-4560-b42f-1fccd43c952e", "GC", "Grandchild", "N", "GP"),
                    new Domain.Base.Entities.RelationType("f2253ac7-9931-4560-b42f-1fccd43c952e", "GP", "Grandparent", "N", "GC")
            };
            

            foreach (var source in allRelationTypes)
            {
                var relationshipTypes = new Ellucian.Colleague.Dtos.RelationshipTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                relationshipTypesCollection.Add(relationshipTypes);
            }

            relationshipTypesController = new RelationshipTypesController(adapterRegistry, referenceDataRepository, relationshipTypesService, logger)
            {
                Request = new HttpRequestMessage()
            };
            relationshipTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            
        }

        [TestCleanup]
        public void Cleanup()
        {
            relationshipTypesController = null;
            allRelationTypes = null;
            relationshipTypesCollection = null;
            loggerMock = null;
            relationshipTypesServiceMock = null;
        }

        [TestMethod]
        public async Task RelationshipTypesController_GetRelationshipTypes_ValidateFields_Nocache()
        {
            relationshipTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesAsync(false)).ReturnsAsync(relationshipTypesCollection);

            var sourceContexts = (await relationshipTypesController.GetRelationshipTypesAsync()).ToList();
            Assert.AreEqual(relationshipTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = relationshipTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task RelationshipTypesController_GetRelationshipTypes_ValidateFields_Cache()
        {
            relationshipTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesAsync(true)).ReturnsAsync(relationshipTypesCollection);

            var sourceContexts = (await relationshipTypesController.GetRelationshipTypesAsync()).ToList();
            Assert.AreEqual(relationshipTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = relationshipTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypes_KeyNotFoundException()
        {
            //
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await relationshipTypesController.GetRelationshipTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypes_PermissionsException()
        {

            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesAsync(false))
                .Throws<PermissionsException>();
            await relationshipTypesController.GetRelationshipTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypes_ArgumentException()
        {

            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesAsync(false))
                .Throws<ArgumentException>();
            await relationshipTypesController.GetRelationshipTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypes_RepositoryException()
        {

            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesAsync(false))
                .Throws<RepositoryException>();
            await relationshipTypesController.GetRelationshipTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypes_IntegrationApiException()
        {

            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesAsync(false))
                .Throws<IntegrationApiException>();
            await relationshipTypesController.GetRelationshipTypesAsync();
        }

        [TestMethod]
        public async Task RelationshipTypesController_GetRelationshipTypesByGuidAsync_ValidateFields()
        {
            var expected = relationshipTypesCollection.FirstOrDefault();
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await relationshipTypesController.GetRelationshipTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypes_Exception()
        {
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesAsync(false)).Throws<Exception>();
            await relationshipTypesController.GetRelationshipTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypesByGuidAsync_Exception()
        {
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await relationshipTypesController.GetRelationshipTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypesByGuid_KeyNotFoundException()
        {
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await relationshipTypesController.GetRelationshipTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypesByGuid_PermissionsException()
        {
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await relationshipTypesController.GetRelationshipTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypesByGuid_ArgumentException()
        {
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await relationshipTypesController.GetRelationshipTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypesByGuid_RepositoryException()
        {
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await relationshipTypesController.GetRelationshipTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypesByGuid_IntegrationApiException()
        {
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await relationshipTypesController.GetRelationshipTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_GetRelationshipTypesByGuid_Exception()
        {
            relationshipTypesServiceMock.Setup(x => x.GetRelationshipTypesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await relationshipTypesController.GetRelationshipTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_PostRelationshipTypesAsync_Exception()
        {
            await relationshipTypesController.PostRelationshipTypesAsync(relationshipTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_PutRelationshipTypesAsync_Exception()
        {
            var sourceContext = relationshipTypesCollection.FirstOrDefault();
            await relationshipTypesController.PutRelationshipTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task RelationshipTypesController_DeleteRelationshipTypesAsync_Exception()
        {
            await relationshipTypesController.DeleteRelationshipTypesAsync(relationshipTypesCollection.FirstOrDefault().Id);
        }
    }
}