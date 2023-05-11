/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.Student.Services;
//using Ellucian.Colleague.Domain.Base.;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
//using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AdmissionApplicationTypesServiceTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory CurrentUserFactory;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public Mock<IStudentReferenceDataRepository> referenceDataRepository;
        AdmissionApplicationTypeService admissionApplicationTypeService;
        List<Dtos.AdmissionApplicationTypes> admissionApplicationTypeDtoList = new List<Dtos.AdmissionApplicationTypes>();
        List<Domain.Student.Entities.AdmissionApplicationType> admissionApplicationTypeEntityList = new List<Domain.Student.Entities.AdmissionApplicationType>();
        string id = "03ef76f3-61be-4990-8a9d-9a80282fc420";

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            BuildData();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
            referenceDataRepository = new Mock<IStudentReferenceDataRepository>();
            admissionApplicationTypeService = new AdmissionApplicationTypeService(referenceDataRepository.Object,  adapterRegistryMock.Object,
                CurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            referenceDataRepository = null;
            admissionApplicationTypeService = null;
            admissionApplicationTypeDtoList = null;
            admissionApplicationTypeEntityList = null;
        }

        [TestMethod]
        public async Task AdmissionApplicationTypes_GetAll()
        {
            referenceDataRepository.Setup(i => i.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionApplicationTypeEntityList);

            var actuals = await admissionApplicationTypeService.GetAdmissionApplicationTypesAsync(It.IsAny<bool>());

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = admissionApplicationTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationTypes_GetAll_True()
        {
            referenceDataRepository.Setup(i => i.GetAdmissionApplicationTypesAsync(true)).ReturnsAsync(admissionApplicationTypeEntityList);

            var actuals = await admissionApplicationTypeService.GetAdmissionApplicationTypesAsync(true);

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = admissionApplicationTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task AdmissionApplicationTypes_GetById()
        {
            var expected = admissionApplicationTypeDtoList.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            referenceDataRepository.Setup(i => i.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionApplicationTypeEntityList);

            var actual = await admissionApplicationTypeService.GetAdmissionApplicationTypesByGuidAsync(id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.IsNull(actual.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AdmissionApplicationTypes_GetById_InvalidOperationException()
        {
            referenceDataRepository.Setup(i => i.GetAdmissionApplicationTypesAsync(It.IsAny<bool>())).ReturnsAsync(admissionApplicationTypeEntityList);
            var actual = await admissionApplicationTypeService.GetAdmissionApplicationTypesByGuidAsync("abc");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AdmissionApplicationTypes_GetById_Exception()
        {
            referenceDataRepository.Setup(i => i.GetAdmissionApplicationTypesAsync(true)).ThrowsAsync(new Exception());
            var actual = await admissionApplicationTypeService.GetAdmissionApplicationTypesByGuidAsync(It.IsAny<string>());
        }

        private void BuildData()
        {
            admissionApplicationTypeEntityList = new List<Domain.Student.Entities.AdmissionApplicationType>() 
            {
                new Domain.Student.Entities.AdmissionApplicationType("03ef76f3-61be-4990-8a9d-9a80282fc420", "ST", "Standard")
            };
            foreach (var entity in admissionApplicationTypeEntityList)
            {
                admissionApplicationTypeDtoList.Add(new Dtos.AdmissionApplicationTypes()
                {
                    Id = entity.Guid,
                    Code = entity.Code,
                    Title = entity.Description,
                    Description = null,
                });
            }
        }
        public void MockInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            CurrentUserFactory = new UserFactorySubset();
        }
        public class UserFactorySubset : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000001",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
