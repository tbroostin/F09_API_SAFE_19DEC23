using Ellucian.Colleague.Domain.Base.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    class PersonHoldTypeServiceTests
    {
        [TestClass]
        public class GetRestrictionTypes
        {
            private Mock<IReferenceDataRepository> referenceRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IPersonRestrictionRepository> personRestrictionRepoMock;
            private Mock<IRoleRepository> roleRepoMock;
            private Mock<ILogger> loggerMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private PersonHoldTypeService personHoldTypeService;
            private ICollection<Domain.Base.Entities.Restriction> restrictionTypeCollection = new List<Domain.Base.Entities.Restriction>();
            private Mock<IConfigurationRepository> _configurationRepository;

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                _configurationRepository = new Mock<IConfigurationRepository>();

                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("73244057-d1ec-4094-a0b7-de602533e3a6", "AA", "Advisor Approval", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Disciplinary });
                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("1df164eb-8178-4321-a9f7-24f12d3991d8", "ALV1", "Test", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Financial });
                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("4af374ab-8908-4091-a7k7-24i02d9931d8", "BH", "Business Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Administrative });
                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("8e09a1e0-4d22-462c-95fe-b8be971bed33", "CH", "Academic Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Academic });
                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("0f291189-4d3b-48f2-aa62-11c94c32f815", "DH", "Health Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Health });
                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("2d7d91d6-a9a7-4291-b49b-4a42e1528be2", "EH", "Academic2 Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Academic }); 

                referenceRepositoryMock.Setup(repo => repo.RestrictionsAsync()).ReturnsAsync(restrictionTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsAsync(false)).ReturnsAsync(restrictionTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsAsync(true)).ReturnsAsync(restrictionTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsWithCategoryAsync(false)).ReturnsAsync(restrictionTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsWithCategoryAsync(true)).ReturnsAsync(restrictionTypeCollection);

                personHoldTypeService = new PersonHoldTypeService(adapterRegistryMock.Object, referenceRepositoryMock.Object, currentUserFactoryMock.Object, _configurationRepository.Object, roleRepoMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                restrictionTypeCollection = null;
                personHoldTypeService = null;
                _configurationRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonHoldTypeByGuid2Async_ArgumentNullException()
            {
                var results = await personHoldTypeService.GetPersonHoldTypeByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetPersonHoldTypeByGuid2Async_InvalidOperationException()
            {
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException());
                var results = await personHoldTypeService.GetPersonHoldTypeByGuid2Async("1234");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetPersonHoldTypeByGuid2Async_Exception()
            {
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsWithCategoryAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var results = await personHoldTypeService.GetPersonHoldTypeByGuid2Async("1234");
            }

            [TestMethod]
            public async Task RestrictionService_PersonHoldType_GetPersonHoldTypeByGuid2Async()
            {

                var expectedResults = restrictionTypeCollection.Where(c => c.Guid == "73244057-d1ec-4094-a0b7-de602533e3a6").FirstOrDefault();
                var results = await personHoldTypeService.GetPersonHoldTypeByGuid2Async("73244057-d1ec-4094-a0b7-de602533e3a6");

                Assert.AreEqual(expectedResults.Guid, results.Id);
                Assert.AreEqual(expectedResults.Code, results.Code);
                Assert.AreEqual(expectedResults.Title, results.Title);
                Assert.AreEqual((int)expectedResults.RestIntgCategory, (int)results.Category);
            }

            [TestMethod]
            public async Task RestrictionService_PersonHoldType_GetPersonHoldTypeByGuid2Async_DeafultEnum()
            {
                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("21f51325-90a6-4f4b-a2e3-b895b56d03b3", "EH", "Academic2 Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = (Domain.Base.Entities.RestrictionCategoryType)(-1) });
                var expectedResults = restrictionTypeCollection.Where(c => c.Guid == "21f51325-90a6-4f4b-a2e3-b895b56d03b3").FirstOrDefault();
                var results = await personHoldTypeService.GetPersonHoldTypeByGuid2Async("21f51325-90a6-4f4b-a2e3-b895b56d03b3");

                Assert.AreEqual(expectedResults.Guid, results.Id);
                Assert.AreEqual(expectedResults.Code, results.Code);
                Assert.AreEqual(expectedResults.Title, results.Title);
                Assert.AreEqual(0, (int)results.Category);
            }

            [TestMethod]
            public async Task RestrictionService_PersonHoldType_GetPersonHoldTypesAsync()
            {
                var results = await personHoldTypeService.GetPersonHoldTypesAsync(false);
                foreach (var personHoldType in restrictionTypeCollection)
                {
                    var result = results.Where(s => s.Id == personHoldType.Guid).FirstOrDefault();
                    Assert.AreEqual(personHoldType.Guid, result.Id);
                    Assert.AreEqual(personHoldType.Code, result.Code);
                    Assert.AreEqual(personHoldType.Title, result.Title);
                    Assert.AreEqual((int)personHoldType.RestIntgCategory, (int)result.Category);
                }
            }
        }
    }
}
