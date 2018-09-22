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

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    class RestrictionServiceTests
    {
        [TestClass]
        public class GetRestrictionTypes
        {
            private Mock<IReferenceDataRepository> referenceRepositoryMock;
            private IReferenceDataRepository referenceRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonRestrictionRepository> personRestrictionRepoMock;
            private IPersonRestrictionRepository personRestrictionRepo;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ILogger logger;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private ICurrentUserFactory currentUserFactory;
            private RestrictionTypeService restrictionService;
            private ICollection<Domain.Base.Entities.Restriction> restrictionTypeCollection = new List<Domain.Base.Entities.Restriction>();

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personRestrictionRepoMock = new Mock<IPersonRestrictionRepository>();
                personRestrictionRepo = personRestrictionRepoMock.Object;
                referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                currentUserFactory = currentUserFactoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("73244057-d1ec-4094-a0b7-de602533e3a6", "AA", "Advisor Approval", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Disciplinary });
                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("1df164eb-8178-4321-a9f7-24f12d3991d8", "ALV1", "Test", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Financial });
                restrictionTypeCollection.Add(new Domain.Base.Entities.Restriction("4af374ab-8908-4091-a7k7-24i02d9931d8", "BH", "Business Office Hold", null, null, null, null, null, null, null, null, null) { RestIntgCategory = Domain.Base.Entities.RestrictionCategoryType.Administrative }); 

                referenceRepositoryMock.Setup(repo => repo.RestrictionsAsync()).ReturnsAsync(restrictionTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsAsync(false)).ReturnsAsync(restrictionTypeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetRestrictionsAsync(true)).ReturnsAsync(restrictionTypeCollection);

                restrictionService = new RestrictionTypeService(adapterRegistry, referenceRepository, personRepo, personRestrictionRepo, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                restrictionTypeCollection = null;
                adapterRegistry = null;
                referenceRepository = null;
                personRepo = null;
                personRestrictionRepo = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
                restrictionService = null;
            }

            [TestMethod]
            public async Task RestrictionService__RestrictionType()
            {
                var results = await restrictionService.GetRestrictionTypesAsync();
                Assert.IsTrue(results is IEnumerable<RestrictionType>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task RestrictionService_RestrictionType_Count()
            {
                var results = await restrictionService.GetRestrictionTypesAsync(); 
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task RestrictionService_RestrictionType_Properties()
            {
                var results = await restrictionService.GetRestrictionTypesAsync();
                var restrictionServiceItem = results.Where(x => x.Abbreviation == "AA").FirstOrDefault();
                Assert.IsNotNull(restrictionServiceItem.Guid);
                Assert.IsNotNull(restrictionServiceItem.Abbreviation);
                Assert.IsNotNull(restrictionServiceItem.Title);
            }

            [TestMethod]
            public async Task RestrictionService_RestrictionType_Expected()
            {
                var expectedResults = restrictionTypeCollection.Where(c => c.Code == "AA").FirstOrDefault();
                var results = await restrictionService.GetRestrictionTypesAsync(false);
                var restrictionTypeItem = results.Where(s => s.Abbreviation == "AA").FirstOrDefault();

                Assert.AreEqual(expectedResults.Guid, restrictionTypeItem.Guid);
                Assert.AreEqual(expectedResults.Code, restrictionTypeItem.Abbreviation);
                Assert.AreEqual(expectedResults.Title, restrictionTypeItem.Title);
            }


            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RestrictionService_GetRestrictionTypeByGuid_Empty()
            {
                await restrictionService.GetRestrictionTypeByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RestrictionService_GetRestrictionTypeByGuid_Null()
            {
                await restrictionService.GetRestrictionTypeByGuidAsync(null);
            }

            [TestMethod]
            public async Task RestrictionService_GetRestrictionTypeByGuid_Expected()
            {
                var expectedResults = restrictionTypeCollection.Where(c => c.Guid == "73244057-d1ec-4094-a0b7-de602533e3a6").FirstOrDefault();
                var restrictionType = await restrictionService.GetRestrictionTypeByGuidAsync("73244057-d1ec-4094-a0b7-de602533e3a6");
                Assert.AreEqual(expectedResults.Guid, restrictionType.Guid);
                Assert.AreEqual(expectedResults.Code, restrictionType.Abbreviation);
                Assert.AreEqual(expectedResults.Title, restrictionType.Title);
            }

            [TestMethod]
            public async Task RestrictionService_GetRestrictionTypeByGuid_Properties()
            {
                var expectedResults = restrictionTypeCollection.Where(c => c.Guid == "73244057-d1ec-4094-a0b7-de602533e3a6").FirstOrDefault();
                var restrictionType = await restrictionService.GetRestrictionTypeByGuidAsync("73244057-d1ec-4094-a0b7-de602533e3a6");
                Assert.IsNotNull(restrictionType.Guid);
                Assert.IsNotNull(restrictionType.Abbreviation);
                Assert.IsNotNull(restrictionType.Title);
            }

            [TestMethod]
            public async Task RestrictionService__RestrictionType2()
            {
                var results = await restrictionService.GetRestrictionTypes2Async();
                Assert.IsTrue(results is IEnumerable<RestrictionType2>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task RestrictionService_RestrictionType2_Count()
            {
                var results = await restrictionService.GetRestrictionTypes2Async();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task RestrictionService_RestrictionType2_Properties()
            {
                var results = await restrictionService.GetRestrictionTypes2Async();
                var restrictionType = results.Where(x => x.Code == "AA").FirstOrDefault();
                Assert.IsNotNull(restrictionType.Id);
                Assert.IsNotNull(restrictionType.Code);
                Assert.IsNotNull(restrictionType.Title);
            }

            [TestMethod]
            public async Task RestrictionService_RestrictionType2_Expected()
            {
                var expectedResults = restrictionTypeCollection.Where(c => c.Code == "AA").FirstOrDefault();
                var results = await restrictionService.GetRestrictionTypes2Async(false);
                var restrictionType = results.Where(s => s.Code == "AA").FirstOrDefault();

                Assert.AreEqual(expectedResults.Guid, restrictionType.Id);
                Assert.AreEqual(expectedResults.Code, restrictionType.Code);
                Assert.AreEqual(expectedResults.Title, restrictionType.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RestrictionService_GetRestrictionTypeById2_Empty()
            {
                await restrictionService.GetRestrictionTypeByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RestrictionService_GetRestrictionTypeById2_Null()
            {
                await restrictionService.GetRestrictionTypeByGuid2Async(null);
            }

            [TestMethod]
            public async Task RestrictionService_GetRestrictionTypeById2_Expected()
            {
                var expectedResults = restrictionTypeCollection.Where(c => c.Guid == "73244057-d1ec-4094-a0b7-de602533e3a6").FirstOrDefault();
                var restrictionType = await restrictionService.GetRestrictionTypeByGuid2Async("73244057-d1ec-4094-a0b7-de602533e3a6");
                Assert.AreEqual(expectedResults.Guid, restrictionType.Id);
                Assert.AreEqual(expectedResults.Code, restrictionType.Code);
                Assert.AreEqual(expectedResults.Title, restrictionType.Title);
            }

            [TestMethod]
            public async Task RestrictionService_GetRestrictionTypeById2_Properties()
            {
                var expectedResults = restrictionTypeCollection.Where(c => c.Guid == "73244057-d1ec-4094-a0b7-de602533e3a6").FirstOrDefault();
                var restrictionType = await restrictionService.GetRestrictionTypeByGuid2Async("73244057-d1ec-4094-a0b7-de602533e3a6");
                Assert.IsNotNull(restrictionType.Id);
                Assert.IsNotNull(restrictionType.Code);
                Assert.IsNotNull(restrictionType.Title);
            }
        }
    }
}
