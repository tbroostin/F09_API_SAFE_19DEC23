using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PersonEmploymentProficienciesServiceTests_V10
    {
        [TestClass]
        public class PersonEmploymentProficienciesService_GET_GET_BY_ID : CurrentUserSetup
        {
            #region DECLARATIONS

            private Mock<IPersonEmploymentProficienciesRepository> repositoryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private ICurrentUserFactory currentUserFactory;

            private PersonEmploymentProficienciesService service;

            private IEnumerable<Domain.HumanResources.Entities.PersonEmploymentProficiency> collection;
            private Tuple<IEnumerable<Domain.HumanResources.Entities.PersonEmploymentProficiency>, int> tupleResult;

            protected Ellucian.Colleague.Domain.Entities.Role viewPersonEmploymentProf = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.PERSON.EMPL.PROFICIENCIES");

            private string guid = "5a1a02c4-21da-4cbb-98f1-bfd47cba87cd";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                repositoryMock = new Mock<IPersonEmploymentProficienciesRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.PersonEmploymentProficienciesUserFactory();

                InitializeTestData();

                viewPersonEmploymentProf.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.HumanResources.HumanResourcesPermissionCodes.ViewPersonEmpProficiencies));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPersonEmploymentProf });

                service = new PersonEmploymentProficienciesService(repositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                repositoryMock = null;
                adapterRegistryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                service = null;
            }

            private void InitializeTestData()
            {
                collection = new List<Domain.HumanResources.Entities.PersonEmploymentProficiency>()
                {
                    new Domain.HumanResources.Entities.PersonEmploymentProficiency()
                    {
                        Guid = "5a1a02c4-21da-4cbb-98f1-bfd47cba87cd",
                        PersonId = "1",
                        StartOn = DateTime.Today,
                        EndOn = DateTime.Today.AddDays(10)
                    },
                    new Domain.HumanResources.Entities.PersonEmploymentProficiency()
                    {
                        Guid = "6a1a02c4-21da-4cbb-98f1-bfd47cba87cd",
                        PersonId = "1"
                    }
                };

                tupleResult = new Tuple<IEnumerable<Domain.HumanResources.Entities.PersonEmploymentProficiency>, int>(collection, collection.Count());
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(MissingFieldException))]
            public async Task GetPersonEmploymentProficienciesAsync_MissingFieldException_Guid_Null()
            {
                tupleResult.Item1.FirstOrDefault().Guid = null;

                repositoryMock.Setup(r => r.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(tupleResult);

                await service.GetPersonEmploymentProficienciesAsync(0, 10, true);
            }

            [TestMethod]
            [ExpectedException(typeof(MissingFieldException))]
            public async Task GetPersonEmploymentProficienciesAsync_MissingFieldException_PersonId_Null()
            {
                tupleResult.Item1.FirstOrDefault().PersonId = null;

                repositoryMock.Setup(r => r.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(tupleResult);

                await service.GetPersonEmploymentProficienciesAsync(0, 10, true);
            }

            [TestMethod]
            public async Task GetPersonEmploymentProficienciesAsync()
            {
                repositoryMock.Setup(r => r.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(tupleResult);
                repositoryMock.Setup(r => r.GetGuidFromID(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(guid);

                var result = await service.GetPersonEmploymentProficienciesAsync(0, 10, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(collection.Count(), result.Item1.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_KeyNotFoundException()
            {
                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                await service.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_InvalidOperationException()
            {
                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

                await service.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(MissingFieldException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_MissingFieldException_Guid_Null()
            {
                collection.FirstOrDefault().Guid = null;

                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ReturnsAsync(collection.FirstOrDefault());

                await service.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(MissingFieldException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_MissingFieldException_PersonId_Null()
            {
                collection.FirstOrDefault().PersonId = null;

                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ReturnsAsync(collection.FirstOrDefault());

                await service.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            public async Task GetPersonEmploymentProficienciesByGuidAsync()
            {
                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ReturnsAsync(collection.FirstOrDefault());
                repositoryMock.Setup(r => r.GetGuidFromID(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(guid);

                var result = await service.GetPersonEmploymentProficienciesByGuidAsync(guid);

                Assert.IsNotNull(result);
            }
        }
    }
}
