// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PhotoServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetPersonPhoto : CurrentUserSetup
        {
            private IPhotoRepository _photoRepository;
            private Mock<IPhotoRepository> _photoRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private ICurrentUserFactory _currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private IRoleRepository _roleRepository;
            private Mock<IProxyRepository> _proxyRepositoryMock;
            private IProxyRepository _proxyRepository;
            private ILogger _logger;

            private PhotoService _photoService;

            private string personId;
            private Photograph photoEntity;


            //private Photograph photoEntity;

            [TestInitialize]
            public void Initialize()
            {
                _logger = new Mock<ILogger>().Object;
                _photoRepositoryMock = new Mock<IPhotoRepository>();
                _photoRepository = _photoRepositoryMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _roleRepository = _roleRepositoryMock.Object;
                _proxyRepositoryMock = new Mock<IProxyRepository>();
                _proxyRepository = _proxyRepositoryMock.Object;
                _currentUserFactory = new CurrentUserSetup.FacultyUserFactory();


                personId = _currentUserFactory.CurrentUser.PersonId;

                var memoryStream = new System.IO.MemoryStream();
                photoEntity = new Photograph(memoryStream);
                memoryStream.Close();
                var emptysubjectsList = new List<Domain.Base.Entities.ProxySubject>();
                _proxyRepositoryMock.Setup(prepo => prepo.GetUserProxySubjectsAsync(It.IsAny<string>())).ReturnsAsync(emptysubjectsList);
                _photoRepositoryMock.Setup(repo => repo.GetPersonPhoto(It.IsAny<string>())).Returns(photoEntity);
                _photoService = new PhotoService(_photoRepository, _currentUserFactory, _roleRepository, _proxyRepository, _adapterRegistry, _logger);

            }

            [TestCleanup]
            public void Cleanup()
            {
                _logger = null;
                _photoRepositoryMock = null;
                _photoRepository = null;
                _adapterRegistryMock = null;
                _adapterRegistry = null;
                _roleRepositoryMock = null;
                _roleRepository = null;

                _photoService = null;

                
                personId = null;

            }

            #region GetPersonPhoto

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PhotoService_GetPersonPhoto_NullId()
            {
                await _photoService.GetPersonPhotoAsync(null);
            }



            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PhotoService_GetAnotherPersonPhoto_DoesNotHavePermission()
            {
                facultyRole.RemovePermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.CanViewPersonPhotos));
                _photoService = new PhotoService(_photoRepository, _currentUserFactory, _roleRepository, _proxyRepository, _adapterRegistry, _logger);
                await _photoService.GetPersonPhotoAsync("0000022");
            }

            [TestMethod]
            public async Task PhotoService_GetPersonPhoto_CanGetMyPhoto()
            {
                // Even without the special permission a person can get their own photo
                var testPhoto = await _photoService.GetPersonPhotoAsync(personId);
            }

            [TestMethod]
            public async Task PhotoService_GetAnotherPersonPhoto_UserHasViewPhotoPermission()
            {
                // Mock permissions               
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.CanViewPersonPhotos));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });
                var memoryStream = new System.IO.MemoryStream();
                photoEntity = new Photograph(memoryStream);
                memoryStream.Close();
                _photoRepositoryMock.Setup(repo => repo.GetPersonPhoto(It.IsAny<string>())).Returns(photoEntity);
                _photoService = new PhotoService(_photoRepository, _currentUserFactory, _roleRepository, _proxyRepository, _adapterRegistry, _logger);
                var photoResult = await _photoService.GetPersonPhotoAsync("0000022");

                // Assert that it returned a photograph object
                Assert.IsTrue(photoResult is Photograph);
            }


            #endregion GetTextPhotoAsync
        }
    }
}