// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Data.Colleague;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AdmissionApplicationSupportingItemsServiceTests_V12
    {
        [TestClass]
        public class AdmissionApplicationSupportingItemsServiceTests_GETALL_GETBYID : StudentUserFactory
        {
            #region DECLCARATIONS

            protected Domain.Entities.Role viewAdmissionApplSupportingItems = new Domain.Entities.Role(1, "VIEW.APPL.SUPPORTING.ITEMS");

            private Mock<IAdmissionApplicationSupportingItemsRepository> admissionApplicationSupportingItemsRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;

            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;

            private Mock<IConfigurationRepository> configurationRepositoryMock;

            private AdmissionApplicationSupportingItemsUser currentUserFactory;

            private AdmissionApplicationSupportingItemsService admissionApplicationSupportingItemsService;

            private IEnumerable<AdmissionApplicationSupportingItem> admissionApplicationSupportingItemsCollection;

            private Tuple<IEnumerable<AdmissionApplicationSupportingItem>, int> domainAdmissionSupportingItmesTuple;

            private IEnumerable<CommunicationCode> communicationCodes;

            private string guid = "157d73eb-7194-4988-872d-0d425df8dfd3";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                admissionApplicationSupportingItemsRepositoryMock = new Mock<IAdmissionApplicationSupportingItemsRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();

                currentUserFactory = new AdmissionApplicationSupportingItemsUser();

                InitializeTestData();

                InitializeTestMock();

                admissionApplicationSupportingItemsService = new AdmissionApplicationSupportingItemsService(admissionApplicationSupportingItemsRepositoryMock.Object,
                    referenceDataRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
            }

            private void InitializeTestData()
            {
                admissionApplicationSupportingItemsCollection = new List<AdmissionApplicationSupportingItem>()
                {
                    new AdmissionApplicationSupportingItem("157d73eb-7194-4988-872d-0d425df8dfd0", "1", "1", "1", "instance", DateTime.Today, "1")
                    {
                        ReceivedDate = DateTime.Today,
                        ActionDate = DateTime.Today,
                        StatusAction = "1"
                    },
                    new AdmissionApplicationSupportingItem("157d73eb-7194-4988-872d-0d425df8dfd1", "2", "2", "2", "instance", DateTime.Today, "1")
                    {
                        StatusAction = "0"
                    },
                    new AdmissionApplicationSupportingItem("157d73eb-7194-4988-872d-0d425df8dfd2", "2", "2", "2", "instance", DateTime.Today, "")
                    {
                        StatusAction = "2",
                        Required = true
                    },
                };

                domainAdmissionSupportingItmesTuple = new Tuple<IEnumerable<AdmissionApplicationSupportingItem>, int>(admissionApplicationSupportingItemsCollection, admissionApplicationSupportingItemsCollection.Count());

                communicationCodes = new List<CommunicationCode>()
                {
                    new CommunicationCode(Guid.NewGuid().ToString(), "1", "accepted"),
                    new CommunicationCode(Guid.NewGuid().ToString(), "2", "incomplete"),
                    new CommunicationCode(Guid.NewGuid().ToString(), "3", "waived")
                };
            }

            private void InitializeTestMock()
            {
                viewAdmissionApplSupportingItems.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewApplicationSupportingItems));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAdmissionApplSupportingItems });

                admissionApplicationSupportingItemsRepositoryMock.Setup(a => a.GetAdmissionApplicationSupportingItemsAsync(It.IsAny<int>(), It.IsAny<int>(), false))
                    .ReturnsAsync(domainAdmissionSupportingItmesTuple);

                referenceDataRepositoryMock.Setup(rr => rr.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>())).ReturnsAsync(communicationCodes);
            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationSupportingItemsRepositoryMock = null;
                referenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                configurationRepositoryMock = null;
                currentUserFactory = null;
                admissionApplicationSupportingItemsService = null;
            }

            #endregion

            #region GETALL & GET BY ID

            

            [TestMethod]
            public async Task AdmsnApplnSupptngItemsService_GetAdmissionApplicationSupportingItemsAsync()
            {
                var results = await admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());

                Assert.IsNotNull(results);
                Assert.AreEqual(results.Item2, admissionApplicationSupportingItemsCollection.Count());

                foreach (var actual in results.Item1)
                {
                    var expected = admissionApplicationSupportingItemsCollection.FirstOrDefault(a => a.Guid.Equals(actual.Id));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_KeyNotFoundException()
            {
                admissionApplicationSupportingItemsRepositoryMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetAdmissionApplicationSupportingItemsByGuidAsync_InvalidOperationException()
            {
                admissionApplicationSupportingItemsRepositoryMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                await admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);
            }

            [TestMethod]
            public async Task AdmsnApplnSupptngItemsService_GetAdmissionApplicationSupportingItemsByGuidAsync()
            {
                AdmissionApplicationSupportingItem expected = new AdmissionApplicationSupportingItem(guid, "1", "1", "1", "instance", DateTime.Today, "1") { };

                admissionApplicationSupportingItemsRepositoryMock.Setup(a => a.GetAdmissionApplicationSupportingItemsByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);

                var result = await admissionApplicationSupportingItemsService.GetAdmissionApplicationSupportingItemsByGuidAsync(guid);

                Assert.IsNotNull(result);

                Assert.AreEqual(expected.Guid, result.Id);
            }

            #endregion
        }

        [TestClass]
        public class AdmissionApplicationSupportingItemsServiceTests_POST_PUT : StudentUserFactory {

            #region DECLARATIONS

            protected Domain.Entities.Role updateAdmissionApplSupportingItems = new Domain.Entities.Role(1, "UPDATE.APPL.SUPPORTING.ITEMS");

            private Mock<IAdmissionApplicationSupportingItemsRepository> admissionApplicationSupportingItemsRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;

            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;

            private Mock<IConfigurationRepository> configurationRepositoryMock;

            private AdmissionApplicationSupportingItemsUser currentUserFactory;

            private AdmissionApplicationSupportingItemsService admissionApplicationSupportingItemsService;
            private IEnumerable<AdmissionApplicationSupportingItem> admissionApplicationSupportingItemsCollection;

            private IEnumerable<CorrStatus> corrStatus;

            private IEnumerable<CommunicationCode> communicationCodes;

            private AdmissionApplicationSupportingItems admissionApplicationSupportingItems;

            private KeyValuePair<string, GuidLookupResult> entityIds;
            
            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                admissionApplicationSupportingItemsRepositoryMock = new Mock<IAdmissionApplicationSupportingItemsRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();

                currentUserFactory = new AdmissionApplicationSupportingItemsUser();

                InitializeTestData();

                InitializeTestMock();

                admissionApplicationSupportingItemsService = new AdmissionApplicationSupportingItemsService(admissionApplicationSupportingItemsRepositoryMock.Object,
                    referenceDataRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
            }

            private void InitializeTestData()
            {
                admissionApplicationSupportingItems = new AdmissionApplicationSupportingItems() { Type = new GuidObject2("333d73eb-7194-4988-872d-0d425df8dfd0"), Application = new GuidObject2("157d73eb-7194-4988-872d-0d425df8dfd0"), AssignedOn = DateTime.Now, ExternalReference = "external_Ref_001", Id = "157d73eb-7194-4988-872d-0d425df8dfd0", ReceivedOn = DateTime.Now, Required = new Dtos.EnumProperties.AdmissionApplicationSupportingItemsRequired(), RequiredByDate = DateTime.Now, Status = new Dtos.DtoProperties.AdmissionApplicationSupportingItemsStatus() { Detail = new GuidObject2("444d73eb-7194-4988-872d-0d425df8dfd1"), Type = AdmissionApplicationSupportingItemsType.Received } };

                admissionApplicationSupportingItemsCollection = new List<AdmissionApplicationSupportingItem>()
                {
                    new AdmissionApplicationSupportingItem("157d73eb-7194-4988-872d-0d425df8dfd0", "1", "1", "1", "instance", DateTime.Today, "1")
                    {
                        ReceivedDate = DateTime.Today,
                        ActionDate = DateTime.Today,
                        StatusAction = "1"
                    },
                    new AdmissionApplicationSupportingItem("157d73eb-7194-4988-872d-0d425df8dfd1", "2", "2", "2", "instance", DateTime.Today, "1")
                    {
                        StatusAction = "0"
                    },
                    new AdmissionApplicationSupportingItem("157d73eb-7194-4988-872d-0d425df8dfd2", "2", "2", "2", "instance", DateTime.Today, "")
                    {
                        StatusAction = "2",
                        Required = true
                    },
                };

                corrStatus = new List<CorrStatus>() { new CorrStatus("444d73eb-7194-4988-872d-0d425df8dfd1", "1", "desc") { Action="1" }, new CorrStatus("444d73eb-7194-4988-872d-0d425df8dfd2", "0", "desc") { Action="0" }, new CorrStatus("444d73eb-7194-4988-872d-0d425df8dfd2", "2", "desc") { Action = "2" }, new CorrStatus("444d73eb-7194-4988-872d-0d425df8dfd3", "status_003", "desc")  };
                
                entityIds = new KeyValuePair<string, GuidLookupResult>( "1", new GuidLookupResult() { Entity = "APPL.SUPPORTING.ITEMS", PrimaryKey = "1", SecondaryKey = "1*123*12343*678" });

                communicationCodes = new List<CommunicationCode>()
                {
                    new CommunicationCode(Guid.NewGuid().ToString(), "1", "accepted"),
                    new CommunicationCode(Guid.NewGuid().ToString(), "2", "incomplete"),
                    new CommunicationCode(Guid.NewGuid().ToString(), "3", "waived")
                };
            }

            private void InitializeTestMock()
            {
                updateAdmissionApplSupportingItems.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.UpdateApplicationSupportingItems));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updateAdmissionApplSupportingItems });

                admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.GetApplicationIdFromGuidAsync("157d73eb-7194-4988-872d-0d425df8dfd0")).ReturnsAsync("1");

                admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.GetIdFromGuidAsync("333d73eb-7194-4988-872d-0d425df8dfd0", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("2");

                admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.GetPersonIdFromApplicationIdAsync(It.IsAny<string>())).ReturnsAsync("1");

                admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItem>())).ReturnsAsync(admissionApplicationSupportingItemsCollection.FirstOrDefault());

                referenceDataRepositoryMock.Setup(x => x.GetCorrStatusesAsync(It.IsAny<bool>())).ReturnsAsync(corrStatus);

                admissionApplicationSupportingItemsRepositoryMock.Setup( x =>x.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(entityIds);

                admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItem>())).ReturnsAsync(admissionApplicationSupportingItemsCollection.FirstOrDefault());

                referenceDataRepositoryMock.Setup(rr => rr.GetAdmissionApplicationSupportingItemTypesAsync(It.IsAny<bool>())).ReturnsAsync(communicationCodes);

            }

            [TestCleanup]
            public void Cleanup()
            {
                admissionApplicationSupportingItemsRepositoryMock = null;
                referenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                configurationRepositoryMock = null;
                currentUserFactory = null;
                admissionApplicationSupportingItemsService = null;
            }

            #endregion

            #region POST

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Dto_Null()
            {
                await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Dto_Id_Null()
            {
                admissionApplicationSupportingItems.Id = null;
                await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Throws_RepositoryException()
            {
                try
                {
                    admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItem>())).Throws(new RepositoryException());
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "Repository exception", "Throws RepositoryException");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Throws_Exception()
            {
                try
                {
                    admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItem>())).Throws(new Exception());
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "Exception of type 'System.Exception' was thrown.", "Throws Exception");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Application_As_Null()
            {
                try
                {
                    admissionApplicationSupportingItems.Application = null;
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "The Application Id is required for a PUT or POST request.", "Application as Null");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Application_Id_As_Null()
            {
                try
                { 
                    admissionApplicationSupportingItems.Application.Id = null;
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "The Application Id is required for a PUT or POST request.", "Application Id as Null");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Type_As_Null()
            {
                try
                {
                    admissionApplicationSupportingItems.Type = null;
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "The Type Id is required for a PUT or POST request.", "Type as Null");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Type_Id_As_Null()
            {
                try
                {
                    admissionApplicationSupportingItems.Type.Id = null;
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "The Type Id is required for a PUT or POST request.", "Type Id as Null");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Status_As_Null()
            {
                try
                {
                    admissionApplicationSupportingItems.Status = null;
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "The Status Type or Status Detail ID is required for a PUT or POST request.", "Status as Null");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Status_Type_As_NotSet()
            {
                try
                {
                    admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.NotSet;
                    admissionApplicationSupportingItems.Status.Detail = null;
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "The Status Type or Status Detail ID is required for a PUT or POST request.", "Status Type as NotSet");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Status_Detail_Id_As_Null()
            {
                try
                {
                    admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.NotSet;
                    admissionApplicationSupportingItems.Status.Detail.Id = null;
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "The Status Type or Status Detail ID is required for a PUT or POST request.", "Status Detail Id as Null");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_GetIdFromGuidAsync_Returns_Null()
            {
                try
                {
                    admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.GetIdFromGuidAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "No type was found for guid '333d73eb-7194-4988-872d-0d425df8dfd0'.", "GetIdFromGuidAsync Returns Null");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_GetIdFromGuidAsync_Type_Returns_Null()
            {
                try
                {
                    admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.GetIdFromGuidAsync("333d73eb-7194-4988-872d-0d425df8dfd0", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);
                    await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "No type was found for guid '333d73eb-7194-4988-872d-0d425df8dfd0'.", "GetIdFromGuidAsync Type Returns Null");
                    throw ex;
                }
            }

            [TestMethod]
            public async Task CreateAdmissionApplicationSupportingItemsAsync()
            {

                var result =  await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplicationSupportingItems.Id);

            }

            [TestMethod]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_StatusDetail_As_Null()
            {
                admissionApplicationSupportingItems.Status.Detail = null;
                
                var result = await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplicationSupportingItems.Id);

            }

            [TestMethod]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Status_Type_As_Incomplete()
            {
                admissionApplicationSupportingItems.Status.Detail = null;
                admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.Incomplete;

                var result = await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplicationSupportingItems.Id);

            }

            [TestMethod]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Status_Type_As_Waived()
            {
                admissionApplicationSupportingItems.Status.Detail = null;
                admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.Waived;

                var result = await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplicationSupportingItems.Id);

            }

            [TestMethod]
            public async Task CreateAdmissionApplicationSupportingItemsAsync_Status_Type_As_NotRecieved()
            {
                admissionApplicationSupportingItems.Status.Detail = null;
                admissionApplicationSupportingItems.Status.Type = AdmissionApplicationSupportingItemsType.Notreceived;


                admissionApplicationSupportingItemsCollection.FirstOrDefault().StatusAction = "3";

                admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.CreateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItem>())).ReturnsAsync(admissionApplicationSupportingItemsCollection.FirstOrDefault());

                var result = await admissionApplicationSupportingItemsService.CreateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplicationSupportingItems.Id);

            }

            #endregion

            #region PUT

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_Dto_Null()
            {
                await admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_Dto_Id_Null()
            {
                admissionApplicationSupportingItems.Id = null;
                await admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_Throws_RepositoryException()
            {
                try
                {
                    admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItem>())).ThrowsAsync(new RepositoryException());
                    await admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "Repository exception", "Throws Repository Exception");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_Throws_KeyNotFoundException()
            {
                admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItem>())).ThrowsAsync(new KeyNotFoundException());
                await admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_Throws_ArgumentException()
            {
                try
                {
                    admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItem>())).ThrowsAsync(new ArgumentException());
                    await admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "Value does not fall within the expected range.", "Throws ArgumentException");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_Throws_Exception()
            {
                try
                {
                    admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.UpdateAdmissionApplicationSupportingItemsAsync(It.IsAny<AdmissionApplicationSupportingItem>())).ThrowsAsync(new Exception());
                    await admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.IsTrue(ex.Errors.Any());
                    Assert.AreEqual(ex.Errors[0].Message, "Exception of type 'System.Exception' was thrown.", "Throws Exception");
                    throw ex;
                }
            }

            [TestMethod]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync()
            {

                var result = await admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplicationSupportingItems.Id);

            }

            [TestMethod]
            public async Task UpdateAdmissionApplicationSupportingItemsAsync_When_Primary_And_SecondaryKeys_Null()
            {

                entityIds = new KeyValuePair<string, GuidLookupResult>("1", new GuidLookupResult() { Entity = "APPL.SUPPORTING.ITEMS", PrimaryKey = null, SecondaryKey = null });

                admissionApplicationSupportingItemsRepositoryMock.Setup(x => x.GetAdmissionApplicationSupportingItemsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(entityIds);

                var result = await admissionApplicationSupportingItemsService.UpdateAdmissionApplicationSupportingItemsAsync(admissionApplicationSupportingItems);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, admissionApplicationSupportingItems.Id);

            }

            #endregion
        }
    }
}
