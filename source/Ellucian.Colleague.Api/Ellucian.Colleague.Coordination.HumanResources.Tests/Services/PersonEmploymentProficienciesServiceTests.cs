using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
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
            GuidLookupResult guidLookUpResult = new GuidLookupResult() { Entity = "HR.IND.SKILL", PrimaryKey = "ABC" };

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
                        ProficiencyId = "CAP",
                        PersonId = "1",
                        StartOn = DateTime.Today,
                        EndOn = DateTime.Today.AddDays(10),
                        RecordKey = "ABC"
                    },
                    new Domain.HumanResources.Entities.PersonEmploymentProficiency()
                    {
                        Guid = "6a1a02c4-21da-4cbb-98f1-bfd47cba87cd",
                        ProficiencyId = "CAP",
                        PersonId = "1",
                        RecordKey = "123"
                    }
                };

                tupleResult = new Tuple<IEnumerable<Domain.HumanResources.Entities.PersonEmploymentProficiency>, int>(collection, collection.Count());
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPersonEmploymentProficienciesAsync_MissingFieldException_Guid_Null()
            {
                tupleResult.Item1.FirstOrDefault().Guid = null;

                repositoryMock.Setup(r => r.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(tupleResult);

                try
                {
                    await service.GetPersonEmploymentProficienciesAsync(0, 10, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsTrue(ex.Errors.Count > 0, "Error Count");
                    bool messageFound = false;
                    foreach (var error in ex.Errors)
                    {
                        if (error.Message == "Record is missing GUID, Entity: ‘HR.IND.SKILL’, Record ID: ‘ABC’" && error.Code == "Bad.Data")
                        {
                            messageFound = true;
                        }
                    }
                    Assert.IsTrue(messageFound, "Appropriate Error Message found in error collection.");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPersonEmploymentProficienciesAsync_MissingFieldException_PersonId_Null()
            {
                tupleResult.Item1.FirstOrDefault().PersonId = null;

                repositoryMock.Setup(r => r.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(tupleResult);

                try
                {
                    await service.GetPersonEmploymentProficienciesAsync(0, 10, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsTrue(ex.Errors.Count > 0, "Error Count");
                    bool messageFound = false;
                    foreach (var error in ex.Errors)
                    {
                        if (error.Message == "Record is missing Person ID, Entity: ‘HR.IND.SKILL’, Record ID: ‘" + tupleResult.Item1.FirstOrDefault().RecordKey + "’" && error.Code == "Bad.Data")
                        {
                            messageFound = true;
                        }
                    }
                    Assert.IsTrue(messageFound, "Appropriate Error Message found in error collection.");
                    throw ex;
                }
            }

            [TestMethod]
            public async Task GetPersonEmploymentProficienciesAsync()
            {
                repositoryMock.Setup(r => r.GetPersonEmploymentProficienciesAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(tupleResult);
                repositoryMock.Setup(r => r.GetGuidFromID(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(guid);
                repositoryMock.Setup(r => r.GetInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guidLookUpResult);

                var result = await service.GetPersonEmploymentProficienciesAsync(0, 10, true);

                Assert.IsNotNull(result);
                Assert.AreEqual(collection.Count(), result.Item1.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_KeyNotFoundException()
            {
                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                try
                {
                    await service.GetPersonEmploymentProficienciesByGuidAsync(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsTrue(ex.Errors.Count > 0, "Error Count");
                    bool messageFound = false;
                    foreach (var error in ex.Errors)
                    {
                        if (error.Message == "person-employment-proficiencies not found for GUID " + guid && error.Code == "GUID.Not.Found")
                        {
                            messageFound = true;
                        }
                    }
                    Assert.IsTrue(messageFound, "Appropriate Error Message found in error collection.");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_InvalidOperationException()
            {
                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());

                await service.GetPersonEmploymentProficienciesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_MissingFieldException_Guid_Null()
            {
                collection.FirstOrDefault().Guid = null;

                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ReturnsAsync(collection.FirstOrDefault());

                try
                {
                    await service.GetPersonEmploymentProficienciesByGuidAsync(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsTrue(ex.Errors.Count > 0, "Error Count");
                    bool messageFound = false;
                    foreach (var error in ex.Errors)
                    {
                        if (error.Message == "Record is missing GUID, Entity: ‘HR.IND.SKILL’, Record ID: ‘ABC’" && error.Code == "Bad.Data")
                        {
                            messageFound = true;
                        }
                    }
                    Assert.IsTrue(messageFound, "Appropriate Error Message found in error collection.");
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPersonEmploymentProficienciesByGuidAsync_MissingFieldException_PersonId_Null()
            {
                collection.FirstOrDefault().PersonId = null;

                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ReturnsAsync(collection.FirstOrDefault());

                try
                {
                    await service.GetPersonEmploymentProficienciesByGuidAsync(guid);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsTrue(ex.Errors.Count > 0, "Error Count");
                    bool messageFound = false;
                    foreach (var error in ex.Errors)
                    {
                        if (error.Message == "Record is missing Person ID, Entity: ‘HR.IND.SKILL’, Record ID: ‘" + tupleResult.Item1.FirstOrDefault().RecordKey + "’" && error.Code == "Bad.Data")
                        {
                            messageFound = true;
                        }
                    }
                    Assert.IsTrue(messageFound, "Appropriate Error Message found in error collection.");
                    throw ex;
                }
            }

            [TestMethod]
            public async Task GetPersonEmploymentProficienciesByGuidAsync()
            {
                repositoryMock.Setup(r => r.GetPersonEmploymentProficiency(It.IsAny<string>())).ReturnsAsync(collection.FirstOrDefault());
                repositoryMock.Setup(r => r.GetGuidFromID(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(guid);
                repositoryMock.Setup(r => r.GetInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guidLookUpResult);

                var result = await service.GetPersonEmploymentProficienciesByGuidAsync(guid);

                Assert.IsNotNull(result);
            }
        }
    }
}
