//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{


    [TestClass]
    public class BulkLoadRequestServiceTests
    {

        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Student");
            protected Ellucian.Colleague.Domain.Entities.Role viewPersonRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.PERSON");
            protected Ellucian.Colleague.Domain.Entities.Role createPersonRole = new Ellucian.Colleague.Domain.Entities.Role(2, "CREATE.PERSON");


            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }

            // Represents a third party system like ILP
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ILP",
                            PersonId = "ILP",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ILP",
                            Roles = new List<string>() { "VIEW.PERSON", "UPDATE.PERSON" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class BulkLoadRequestServiceTest_GET: CurrentUserSetup
        {
            // private const string configurationSettingsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            //private const string configurationSettingsCode = "1";
            //private ICollection<Domain.Base.Entities.ConfigurationSettings> _configurationSettingsCollection;

            private BulkRequestDetails _bulkRequestDetails = null;

            private BulkLoadRequestService _bulkLoadRequestService;
            private BulkLoadRequest _bulkLoadRequestDto = null;
            private BulkRequest _bulkRequest = null;

            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<IBulkRequestRepository> _bulkRequestRepoMock;

            private ICurrentUserFactory _currentUserFactory;

            [TestInitialize]
            public void Initialize()
            {
                _bulkRequestRepoMock = new Mock<IBulkRequestRepository>();

                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();


                // Mock permissions
                var permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });


                _bulkRequestDetails = new BulkRequestDetails()
                {
                    ApplicationId = Guid.NewGuid().ToString(),
                    Errors = new List<BulkRequestGetError>(),
                    JobNumber = "1",
                    ProcessingSteps = new List<BulkRequestProcessingStep>()
                {
                    new  BulkRequestProcessingStep()
                    {
                        Count = "100",
                        ElapsedTime = "100",
                        JobNumber = "A123",
                        Seq = "1",
                        StartTime = DateTime.Now.ToShortDateString(),
                        Status = "InProgress",
                        Step = "Start"
                    }
                },
                    Representation = "headerVersion",
                    RequestorTrackingId = Guid.NewGuid().ToString(),
                    ResourceName = "person",
                    Status = "InProgress",
                    TenantId = Guid.NewGuid().ToString(),
                    XTotalCount = "121"
                };

                _bulkRequest = new BulkRequest()
                {
                    ApplicationId = _bulkRequestDetails.ApplicationId,
                    JobNumber = _bulkRequestDetails.JobNumber,
                    Message = "test message",
                    Representation = _bulkRequestDetails.Representation,
                    RequestorTrackingId = _bulkRequestDetails.RequestorTrackingId,
                    ResourceName = _bulkRequestDetails.ResourceName,
                    Status = BulkRequestStatus.InProgress
                };


                _bulkRequestRepoMock.Setup(repo => repo.GetBulkRequestDetails(It.IsAny<string>()))
                    .ReturnsAsync(_bulkRequestDetails);

                _bulkRequestRepoMock.Setup(repo => repo.CreateBulkLoadRequestAsync(It.IsAny<BulkRequest>(), It.IsAny<List<string>>()))
                        .ReturnsAsync(_bulkRequest);


                _bulkLoadRequestDto = new BulkLoadRequest()
                {
                    ApplicationId = _bulkRequestDetails.ApplicationId,
                    JobNumber = _bulkRequestDetails.JobNumber,
                    Message = "test message",
                    Representation = _bulkRequestDetails.Representation,
                    RequestorTrackingId = _bulkRequestDetails.RequestorTrackingId,
                    ResourceName = _bulkRequestDetails.ResourceName,
                    Status = Dtos.EnumProperties.BulkLoadRequestStatus.InProgress
                };

                // Set up current user
                _currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                _bulkLoadRequestService = new BulkLoadRequestService(_bulkRequestRepoMock.Object,
                    _adapterRegistryMock.Object, _currentUserFactory,
                    _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
            }
            
            [TestCleanup]
            public void Cleanup()
            {
                _bulkLoadRequestService = null;

                _loggerMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(System.ArgumentNullException))]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_NoPermissions()
            {
                await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, "");
               
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_InvalidPermissions()
            {
                await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, BasePermissionCodes.DeleteComment.ToString());

            }


            [TestMethod]
            [ExpectedException(typeof(System.ArgumentNullException))]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_EmptyBody()
            {
                await _bulkLoadRequestService.CreateBulkLoadRequestAsync(null, BasePermissionCodes.ViewAnyPerson.ToString());
                
            }

            [TestMethod]
            [ExpectedException(typeof(System.ArgumentNullException))]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_EmptyRequestTrackerId()
            {
                _bulkLoadRequestDto.RequestorTrackingId = null;
                await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, BasePermissionCodes.ViewAnyPerson.ToString());

            }



            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_RepositoryException()
            {
                _bulkRequestRepoMock.Setup(repo => repo.CreateBulkLoadRequestAsync(It.IsAny<BulkRequest>(), It.IsAny<List<string>>()))
                      .ThrowsAsync(new RepositoryException());

                await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, BasePermissionCodes.ViewAnyPerson.ToString());

            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_Exception()
            {
                _bulkRequestRepoMock.Setup(repo => repo.CreateBulkLoadRequestAsync(It.IsAny<BulkRequest>(), It.IsAny<List<string>>()))
                      .ThrowsAsync(new Exception());

                await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, BasePermissionCodes.ViewAnyPerson.ToString());

            }

            [TestMethod]          
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_InProgress()
            {
                var results = await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, BasePermissionCodes.ViewAnyPerson.ToString());
                Assert.IsTrue(results is BulkLoadRequest);
                Assert.IsNotNull(results);


                Assert.AreEqual(_bulkLoadRequestDto.ApplicationId, results.ApplicationId, "ApplicationID");
                Assert.AreEqual(_bulkLoadRequestDto.JobNumber, results.JobNumber, "JobNumber");
                Assert.AreEqual(_bulkLoadRequestDto.Message, results.Message, "Message");
                Assert.AreEqual(_bulkLoadRequestDto.Representation, results.Representation, "Representation");
                Assert.AreEqual(_bulkLoadRequestDto.RequestorTrackingId, results.RequestorTrackingId, "RequesterTrackingId");
                Assert.AreEqual(_bulkLoadRequestDto.ResourceName, results.ResourceName, "ResoureName");
                Assert.AreEqual(BulkLoadRequestStatus.InProgress, results.Status, "Status");
            }

            [TestMethod]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_Completed()
            {
                _bulkRequest = new BulkRequest()
                {
                    ApplicationId = _bulkRequestDetails.ApplicationId,
                    JobNumber = _bulkRequestDetails.JobNumber,
                    Message = "test message",
                    Representation = _bulkRequestDetails.Representation,
                    RequestorTrackingId = _bulkRequestDetails.RequestorTrackingId,
                    ResourceName = _bulkRequestDetails.ResourceName,
                    Status = BulkRequestStatus.Completed
                };


                _bulkRequestRepoMock.Setup(repo => repo.CreateBulkLoadRequestAsync(It.IsAny<BulkRequest>(), It.IsAny<List<string>>()))
                        .ReturnsAsync(_bulkRequest);


                var results = await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, BasePermissionCodes.ViewAnyPerson.ToString());
                Assert.IsTrue(results is BulkLoadRequest);
                Assert.IsNotNull(results);


                Assert.AreEqual(_bulkLoadRequestDto.ApplicationId, results.ApplicationId, "ApplicationID");
                Assert.AreEqual(_bulkLoadRequestDto.JobNumber, results.JobNumber, "JobNumber");
                Assert.AreEqual(_bulkLoadRequestDto.Message, results.Message, "Message");
                Assert.AreEqual(_bulkLoadRequestDto.Representation, results.Representation, "Representation");
                Assert.AreEqual(_bulkLoadRequestDto.RequestorTrackingId, results.RequestorTrackingId, "RequesterTrackingId");
                Assert.AreEqual(_bulkLoadRequestDto.ResourceName, results.ResourceName, "ResoureName");
                Assert.AreEqual(BulkLoadRequestStatus.Completed, results.Status, "Status");
            }

            [TestMethod]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_InProgressDefault()
            {
                _bulkRequest = new BulkRequest()
                {
                    ApplicationId = _bulkRequestDetails.ApplicationId,
                    JobNumber = _bulkRequestDetails.JobNumber,
                    Message = "test message",
                    Representation = _bulkRequestDetails.Representation,
                    RequestorTrackingId = _bulkRequestDetails.RequestorTrackingId,
                    ResourceName = _bulkRequestDetails.ResourceName
                    
                };


                _bulkRequestRepoMock.Setup(repo => repo.CreateBulkLoadRequestAsync(It.IsAny<BulkRequest>(), It.IsAny<List<string>>()))
                        .ReturnsAsync(_bulkRequest);


                var results = await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, BasePermissionCodes.ViewAnyPerson.ToString());
                Assert.IsTrue(results is BulkLoadRequest);
                Assert.IsNotNull(results);


                Assert.AreEqual(_bulkLoadRequestDto.ApplicationId, results.ApplicationId, "ApplicationID");
                Assert.AreEqual(_bulkLoadRequestDto.JobNumber, results.JobNumber, "JobNumber");
                Assert.AreEqual(_bulkLoadRequestDto.Message, results.Message, "Message");
                Assert.AreEqual(_bulkLoadRequestDto.Representation, results.Representation, "Representation");
                Assert.AreEqual(_bulkLoadRequestDto.RequestorTrackingId, results.RequestorTrackingId, "RequesterTrackingId");
                Assert.AreEqual(_bulkLoadRequestDto.ResourceName, results.ResourceName, "ResoureName");
                Assert.AreEqual(BulkLoadRequestStatus.InProgress, results.Status, "Status");
            }

            [TestMethod]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_ErrorStatus()
            {
                _bulkRequest = new BulkRequest()
                {
                    ApplicationId = _bulkRequestDetails.ApplicationId,
                    JobNumber = _bulkRequestDetails.JobNumber,
                    Message = "test message",
                    Representation = _bulkRequestDetails.Representation,
                    RequestorTrackingId = _bulkRequestDetails.RequestorTrackingId,
                    ResourceName = _bulkRequestDetails.ResourceName,
                    Status = BulkRequestStatus.Error
                };


                _bulkRequestRepoMock.Setup(repo => repo.CreateBulkLoadRequestAsync(It.IsAny<BulkRequest>(), It.IsAny<List<string>>()))
                        .ReturnsAsync(_bulkRequest);


                var results = await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, BasePermissionCodes.ViewAnyPerson.ToString());
                Assert.IsTrue(results is BulkLoadRequest);
                Assert.IsNotNull(results);


                Assert.AreEqual(_bulkLoadRequestDto.ApplicationId, results.ApplicationId, "ApplicationID");
                Assert.AreEqual(_bulkLoadRequestDto.JobNumber, results.JobNumber, "JobNumber");
                Assert.AreEqual(_bulkLoadRequestDto.Message, results.Message, "Message");
                Assert.AreEqual(_bulkLoadRequestDto.Representation, results.Representation, "Representation");
                Assert.AreEqual(_bulkLoadRequestDto.RequestorTrackingId, results.RequestorTrackingId, "RequesterTrackingId");
                Assert.AreEqual(_bulkLoadRequestDto.ResourceName, results.ResourceName, "ResoureName");
                Assert.AreEqual(BulkLoadRequestStatus.Error, results.Status, "Status");
            }

            [TestMethod]
            public async Task BulkLoadRequestService_CreateBulkLoadRequestAsync_PackagingCompleted()
            {
                _bulkRequest = new BulkRequest()
                {
                    ApplicationId = _bulkRequestDetails.ApplicationId,
                    JobNumber = _bulkRequestDetails.JobNumber,
                    Message = "test message",
                    Representation = _bulkRequestDetails.Representation,
                    RequestorTrackingId = _bulkRequestDetails.RequestorTrackingId,
                    ResourceName = _bulkRequestDetails.ResourceName,
                    Status = BulkRequestStatus.PackagingCompleted
                };


                _bulkRequestRepoMock.Setup(repo => repo.CreateBulkLoadRequestAsync(It.IsAny<BulkRequest>(), It.IsAny<List<string>>()))
                        .ReturnsAsync(_bulkRequest);


                var results = await _bulkLoadRequestService.CreateBulkLoadRequestAsync(_bulkLoadRequestDto, BasePermissionCodes.ViewAnyPerson.ToString());
                Assert.IsTrue(results is BulkLoadRequest);
                Assert.IsNotNull(results);


                Assert.AreEqual(_bulkLoadRequestDto.ApplicationId, results.ApplicationId, "ApplicationID");
                Assert.AreEqual(_bulkLoadRequestDto.JobNumber, results.JobNumber, "JobNumber");
                Assert.AreEqual(_bulkLoadRequestDto.Message, results.Message, "Message");
                Assert.AreEqual(_bulkLoadRequestDto.Representation, results.Representation, "Representation");
                Assert.AreEqual(_bulkLoadRequestDto.RequestorTrackingId, results.RequestorTrackingId, "RequesterTrackingId");
                Assert.AreEqual(_bulkLoadRequestDto.ResourceName, results.ResourceName, "ResoureName");
                Assert.AreEqual(BulkLoadRequestStatus.PackagingCompleted, results.Status, "Status");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task BulkLoadRequestService_GetBulkLoadRequestStatus_EmptyId()
            {
                await _bulkLoadRequestService.GetBulkLoadRequestStatus("");
               
            }


            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task BulkLoadRequestService_GetBulkLoadRequestStatus_InvalidId()
            {
                _bulkRequestRepoMock.Setup(repo => repo.GetBulkRequestDetails("99"))
                     .ThrowsAsync(new RepositoryException());

                await _bulkLoadRequestService.GetBulkLoadRequestStatus("99");

            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task BulkLoadRequestService_GetBulkLoadRequestStatus_Exception()
            {
                _bulkRequestRepoMock.Setup(repo => repo.GetBulkRequestDetails(_bulkRequestDetails.RequestorTrackingId))
                     .ThrowsAsync(new Exception());

                await _bulkLoadRequestService.GetBulkLoadRequestStatus(_bulkRequestDetails.RequestorTrackingId);

            }


            [TestMethod]
            public async Task BulkLoadRequestService_GetBulkLoadRequestStatus()
            {
                var results = await _bulkLoadRequestService.GetBulkLoadRequestStatus(_bulkRequestDetails.RequestorTrackingId);
                Assert.IsTrue(results is BulkLoadGet);
                Assert.IsNotNull(results);
            }


            [TestMethod]
            public void BulkLoadRequestService_IsBulkLoadSupported()
            {
                _bulkRequestRepoMock.Setup(repo => repo.IsBulkLoadSupported()).Returns(true);

                var results = _bulkLoadRequestService.IsBulkLoadSupported();
                Assert.IsTrue(results);
            
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void BulkLoadRequestService_IsBulkLoadSupported_Exception()
            {
                _bulkRequestRepoMock.Setup(repo => repo.IsBulkLoadSupported()).Throws(new Exception());

                _bulkLoadRequestService.IsBulkLoadSupported();               

            }



            //[TestMethod]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsAsync_Count()
            //{
            //    var results = await _configurationSettingsService.GetConfigurationSettingsAsync(new List<string>(), true);
            //    Assert.AreEqual(8, results.Count());
            //}

            //[TestMethod]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsAsync_Properties()
            //{
            //    var result =
            //        (await _configurationSettingsService.GetConfigurationSettingsAsync(new List<string>(), true)).FirstOrDefault(x => x.Id == configurationSettingsGuid);
            //    Assert.IsNotNull(result.Id);
            //    Assert.IsNotNull(result.Title);
            //    Assert.IsNotNull(result.Description);
            //}

            //[TestMethod]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsAsync_Expected()
            //{
            //    var expectedResults = _configurationSettingsCollection.FirstOrDefault(c => c.Guid == configurationSettingsGuid);
            //    var actualResult =
            //        (await _configurationSettingsService.GetConfigurationSettingsAsync(new List<string>(), true)).FirstOrDefault(x => x.Id == configurationSettingsGuid);
            //    Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            //    Assert.AreEqual(expectedResults.Description, actualResult.Title);

            //}

            //[TestMethod]
            //[ExpectedException(typeof(KeyNotFoundException))]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_Empty()
            //{
            //    _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
            //        .Throws<KeyNotFoundException>();

            //    await _configurationSettingsService.GetConfigurationSettingsByGuidAsync("");
            //}

            //[TestMethod]
            //[ExpectedException(typeof(KeyNotFoundException))]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_Null()
            //{
            //    _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
            //        .Throws<KeyNotFoundException>();

            //    await _configurationSettingsService.GetConfigurationSettingsByGuidAsync(null);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(KeyNotFoundException))]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_InvalidId()
            //{
            //    _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
            //        .Throws<KeyNotFoundException>();

            //    await _configurationSettingsService.GetConfigurationSettingsByGuidAsync("99");
            //}

            //[TestMethod]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_Expected()
            //{
            //    var expectedResults =
            //        _configurationSettingsCollection.First(c => c.Guid == configurationSettingsGuid);
            //    var actualResult =
            //        await _configurationSettingsService.GetConfigurationSettingsByGuidAsync(configurationSettingsGuid);
            //    Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            //    Assert.AreEqual(expectedResults.Description, actualResult.Title);

            //}

            //[TestMethod]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsByGuidAsync_Properties()
            //{
            //    var result =
            //        await _configurationSettingsService.GetConfigurationSettingsByGuidAsync(configurationSettingsGuid);
            //    Assert.IsNotNull(result.Id);
            //    Assert.IsNotNull(result.Description);
            //    Assert.IsNotNull(result.Title);

            //}

            //[TestMethod]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsOptionsAsync_Properties()
            //{
            //    var resultCollection =
            //        (await _configurationSettingsService.GetConfigurationSettingsOptionsAsync(new List<string>(), true));

            //    foreach (var result in resultCollection)
            //    {
            //        Assert.IsNotNull(result.Id, "Guid");
            //        Assert.IsNotNull(result.Ethos, "Ethos Resources");
            //        Assert.IsNotNull(result.SourceOptions, "Source Options");
            //    }
            //}

            //[TestMethod]
            //public async Task ConfigurationSettingsService_GetConfigurationSettingsOptionsByGuidAsync_Properties()
            //{
            //    foreach (var setting in _configurationSettingsCollection)
            //    {
            //        _configurationSettingsRepoMock.Setup(repo => repo.GetConfigurationSettingsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
            //          .ReturnsAsync(_configurationSettingsCollection.FirstOrDefault(cs => cs.Guid == setting.Guid));

            //        var result =
            //            (await _configurationSettingsService.GetConfigurationSettingsOptionsByGuidAsync(setting.Guid, true));

            //        Assert.IsNotNull(result.Id, "Guid");
            //        Assert.IsNotNull(result.Ethos, "Ethos Resources");
            //        Assert.IsNotNull(result.SourceOptions, "Source Options");
            //    }
            //}
        }
    }
}