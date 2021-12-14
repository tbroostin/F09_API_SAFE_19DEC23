// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class FixedAssetServiceTests_V12 : GeneralLedgerCurrentUser
    {
        [TestClass]
        public class FixedAssetServiceTests_GET
        {
            #region DECLARATION

            protected Domain.Entities.Role getFixedAssets = new Domain.Entities.Role(1, "VIEW.FIXED.ASSETS");

            private Mock<IFixedAssetsRepository> fixedAssetRepositoryMock;           
            private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IRoomRepository> roomRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;            
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
            private Mock<IGeneralLedgerUserRepository> generalLedgerUserRepositoryMock;
            private FixedAssetsUser currentUserFactory;
            private IEnumerable<Domain.Entities.Role> roles;

            private FixedAssetsService fixedAssetService;

            private FixedAssets fixedAsset;            
            private IEnumerable<AssetCategories> assetCategories;
            private IEnumerable<AssetTypes> assetTypes;
            private IEnumerable<FixedAssetsFlag> fixedAssetsFlagEntities;
            private IEnumerable<Domain.Base.Entities.Room> rooms;
            private IEnumerable<Domain.Base.Entities.Building> buildings;
            private IEnumerable<Domain.Base.Entities.ItemCondition> itemConditions;
            private IEnumerable<Domain.Base.Entities.AcquisitionMethod> acquisitionMethods;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                fixedAssetRepositoryMock = new Mock<IFixedAssetsRepository>();
                colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                roomRepositoryMock = new Mock<IRoomRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
                generalLedgerUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
                currentUserFactory = new GeneralLedgerCurrentUser.FixedAssetsUser();

                fixedAssetService = new FixedAssetsService(fixedAssetRepositoryMock.Object, colleagueFinanceReferenceDataRepositoryMock.Object, 
                    referenceDataRepositoryMock.Object, roomRepositoryMock.Object, personRepositoryMock.Object, adapterRegistryMock.Object,
                    currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
              
                InitializeTestData();

                InitializeMock();
            }

            [TestCleanup]
            public void Cleanup()
            {
                fixedAssetRepositoryMock = null;
                generalLedgerConfigurationRepositoryMock = null;
                generalLedgerUserRepositoryMock = null;
                colleagueFinanceReferenceDataRepositoryMock = null;
                roomRepositoryMock = null;
                referenceDataRepositoryMock = null;
                personRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;

                fixedAssetService = null;
            }

            private void InitializeTestData()
            {
                assetCategories = new List<AssetCategories>()
                {
                    new AssetCategories("2e507d85-a051-4edd-951f-ebb106b4cfff", "AC", "Academic building"),
                    new AssetCategories("9debab97-f600-46b8-95a9-aee37cc7a01c", "AE", "Athletic equipment"),
                    new AssetCategories("6a17e419-0e99-4111-bef2-3b73a136fcfe", "CE", "Computer equipment")
                };
                assetTypes = new List<AssetTypes>() 
                {
                    new AssetTypes("4cee0ba2-83bc-482c-9048-3b561660b06f", "EQ", "Equipment"),
                    new AssetTypes("6c686f7f-34ec-43eb-afae-2e7f03add19a", "LAND", "Landing"),
                    new AssetTypes("76a2d4d5-8426-4733-9905-a08130092fc8", "BLDG", "Building")
                };
                rooms = new List<Domain.Base.Entities.Room>()
                {
                    new Domain.Base.Entities.Room("97a04029-31f3-4100-839f-c7f0959dc81c", "LEE*100", "Lee Hall 101"),
                    new Domain.Base.Entities.Room("4e2f391c-85ef-42f4-a502-a63c06300143", "MCB*101", "McBryde 101"),
                    new Domain.Base.Entities.Room("14877171-69a3-4c20-8391-3ccdc4a9f8b3", "PAMP*200", "Pamplin 200")
                };
                buildings = new List<Domain.Base.Entities.Building>()
                {
                    new Domain.Base.Entities.Building("320e7dd4-b4eb-41e6-9f9c-4a4516df1fc6", "LEE", "Lee Hall"),
                    new Domain.Base.Entities.Building("e5d17241-15f8-4f91-a462-cb71f24cd350", "MCB", "McBryde Hall"),
                    new Domain.Base.Entities.Building("b0a4dc70-e992-4349-a82e-56ea86f02203", "PAMP", "Pamplin Hall")
                };
                itemConditions = new List<Domain.Base.Entities.ItemCondition>()
                {
                    new Domain.Base.Entities.ItemCondition("cef26680-4e2c-404c-ae4a-362b2a7d604f", "BR", "Broken"),
                    new Domain.Base.Entities.ItemCondition("adf60b65-fded-49e8-8720-476db1c28a6b", "GD", "Good"),
                    new Domain.Base.Entities.ItemCondition("5795b906-6b91-4d45-9024-9fb3cfc428eb", "FR", "Fair")
                };
                acquisitionMethods = new List<Domain.Base.Entities.AcquisitionMethod>()
                {
                    new Domain.Base.Entities.AcquisitionMethod("dc77ffbe-6d33-4827-891e-322ee36f995f", "DO", "D"),
                    new Domain.Base.Entities.AcquisitionMethod("12a6f8a3-145b-4399-92b7-9e1dcf71a1b4", "LE", "L"),
                    new Domain.Base.Entities.AcquisitionMethod("cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "PR", "P")
                };

                fixedAsset = getFixedAsset();

                roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"VIEW.FIXED.ASSETS")
                    };

                roles.FirstOrDefault().AddPermission(new Permission(ColleagueFinancePermissionCodes.ViewFixedAssets));

                fixedAssetsFlagEntities = new List<FixedAssetsFlag>
                {
                    new FixedAssetsFlag("S", "Single"), new FixedAssetsFlag("M", "Multi-Valued")
                };
        }

            //private FixedAssets getFixedAsset(FixedAssetStatus status = FixedAssetStatus.Outstanding)
            private FixedAssets getFixedAsset()
            {
                //public FixedAssets(string guid, string id, string description, string capitalizationStatus, string acquisitionMethod, string fixPropertyTag)
                return new FixedAssets(guid, "BLDG1", "Building one", "C", "cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "01")
                {
                    FixAssetType = "LAND",
                    FixAssetCategory = "AC",
                    FixInvoiceCondition = "FR",
                    FixBuilding = "MCB",
                    FixRoom = "101"
                };
            }

            private void InitializeMock()
            {
                //getFixedAssets.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewFixedAssets));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { getFixedAssets });
                //roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(getFixedAssets);
                //roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewGrants });
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);

                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ReturnsAsync(fixedAsset);
                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", guid);
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personGuidCollection);
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<String>())).ReturnsAsync(guid);
                roomRepositoryMock.Setup( m => m.GetRoomsAsync(It.IsAny<bool>())).ReturnsAsync(rooms);
                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetAssetCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(assetCategories);
                foreach (var a in assetCategories)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetAssetCategoriesGuidAsync(a.Code)).ReturnsAsync(a.Guid);
                }

                colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetAssetTypesAsync(It.IsAny<bool>())).ReturnsAsync(assetTypes);
                foreach (var a in assetTypes)
                {
                    colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetAssetTypesGuidAsync(a.Code)).ReturnsAsync(a.Guid);
                }
                referenceDataRepositoryMock.Setup(f => f.GetBuildings2Async(It.IsAny<bool>())).ReturnsAsync(buildings);
                foreach (var a in buildings)
                {
                    referenceDataRepositoryMock.Setup(f => f.GetBuildingGuidAsync(a.Code)).ReturnsAsync(a.Guid);
                }

                referenceDataRepositoryMock.Setup(r => r.GetItemConditionsAsync(It.IsAny<bool>())).ReturnsAsync(itemConditions);
                referenceDataRepositoryMock.Setup(r => r.GetAcquisitionMethodsAsync(It.IsAny<bool>())).ReturnsAsync(acquisitionMethods);

                // await _referenceDataRepository.GetHostCountry()
                referenceDataRepositoryMock.Setup(r => r.GetHostCountryAsync()).ReturnsAsync("USA");

                colleagueFinanceReferenceDataRepositoryMock.Setup(r => r.GetFixedAssetTransferFlagsAsync()).ReturnsAsync(fixedAssetsFlagEntities);
                var fixedAssetsFlagAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.FixedAssetsFlag, Dtos.ColleagueFinance.FixedAssetsFlag>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.ColleagueFinance.Entities.FixedAssetsFlag, Dtos.ColleagueFinance.FixedAssetsFlag>()).Returns(fixedAssetsFlagAdapter);

            }

            #endregion

            #region GETBYID

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ArgumentNullException_When_Guid_Null()
            {
                await fixedAssetService.GetFixedAssetsByGuidAsync(null);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_KeyNotFoundException_From_Repository()
            {
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ThrowsAsync(new KeyNotFoundException());
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_InvalidOperationException_From_Repository()
            {
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ThrowsAsync(new InvalidOperationException());
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_RepositoryException_From_Repository()
            {
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ThrowsAsync(new RepositoryException());
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }
            

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ArgumentException_From_Repository()
            {
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ThrowsAsync(new ArgumentException());
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }
            
            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ConvertEntityToAcquisiotionMethod_KeyNotFoundException()
            {
                var fixedAsset = new FixedAssets(guid, "BLDG1", "Building one", "C", "invalidGuid", "01");
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ReturnsAsync(fixedAsset);
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ConvertEntityToCapitalizationStatus_InvalidOperationException()
            {
                var fixedAsset = new FixedAssets(guid, "BLDG1", "Building one", "invalidCapitalizationStatus", "cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "01");
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ReturnsAsync(fixedAsset);
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ConvertEntityToAssetTypeGuidObjectAsync_KeyNotFoundException()
            {
                var fixedAsset = new FixedAssets(guid, "BLDG1", "Building one", "C", "cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "01"){ FixAssetType = "InvalidAssetType"};
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ReturnsAsync(fixedAsset);
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ConvertEntityToAssetCategoryGuidObjectAsync_KeyNotFoundException()
            {
                var fixedAsset = new FixedAssets(guid, "BLDG1", "Building one", "C", "cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "01") { FixAssetCategory = "InvalidAssetCategory" };
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ReturnsAsync(fixedAsset);
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ConvertEntityToBuildingAsync_KeyNotFoundException()
            {
                var fixedAsset = new FixedAssets(guid, "BLDG1", "Building one", "C", "cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "01") { FixBuilding = "InvalidBuilding" };
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ReturnsAsync(fixedAsset);
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ConvertEntityToItemConditionAsync_KeyNotFoundException()
            {
                var fixedAsset = new FixedAssets(guid, "BLDG1", "Building one", "C", "cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "01") { FixInvoiceCondition = "InvalidCondition" };
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ReturnsAsync(fixedAsset);
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ConvertEntityToRoomGuidObjectAsync_KeyNotFoundException()
            {
                var fixedAsset = new FixedAssets(guid, "BLDG1", "Building one", "C", "cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "01") { FixBuilding = "MCB", FixRoom = "InvalidRoom" };
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ReturnsAsync(fixedAsset);
                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync_ConvertEntityToAcquisiotionMethod_InvalidOperationException()
            {
                acquisitionMethods = new List<Domain.Base.Entities.AcquisitionMethod>()
                {
                    new Domain.Base.Entities.AcquisitionMethod("cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "PR", "invalidType")
                };
                referenceDataRepositoryMock.Setup(r => r.GetAcquisitionMethodsAsync(It.IsAny<bool>())).ReturnsAsync(acquisitionMethods);

                var fixedAsset = new FixedAssets(guid, "BLDG1", "Building one", "C", "cfb29bc7-5468-4075-a9ab-7d265b0a04f3", "01");
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetByIdAsync(It.IsAny<String>())).ReturnsAsync(fixedAsset);

                await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
            }

            [TestMethod]
            public async Task FixedAssetService_GetFixedAssetsByGuidAsync()
            {
                var result = await fixedAssetService.GetFixedAssetsByGuidAsync(guid);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, guid);
            }

            #endregion

            #region GETALL

            [TestMethod]
            public async Task FixedAssetService_GetFixedAssetsAsync()
            {
                var fixedAssets = new List<FixedAssets>() { fixedAsset };
                var tupleFixedAssets = new Tuple<IEnumerable<FixedAssets>, int>(fixedAssets, 1);
                fixedAssetRepositoryMock.Setup(r => r.GetFixedAssetsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tupleFixedAssets);
                var result = await fixedAssetService.GetFixedAssetsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item1.FirstOrDefault().Id, guid);
            }

            [TestMethod]
            public async Task FixedAssetService_GetFixedAssetTransferFlagsAsync()
            {
                var fixedAssetflagDtos = await fixedAssetService.GetFixedAssetTransferFlagsAsync();
                Assert.AreEqual(fixedAssetflagDtos.ToList().Count, fixedAssetsFlagEntities.ToList().Count);
                Assert.AreEqual(fixedAssetflagDtos.ToList()[0].Code, fixedAssetsFlagEntities.ToList()[0].Code);
                Assert.AreEqual(fixedAssetflagDtos.ToList()[0].Description, fixedAssetsFlagEntities.ToList()[0].Description);
                Assert.AreEqual(fixedAssetflagDtos.ToList()[1].Code, fixedAssetsFlagEntities.ToList()[1].Code);
                Assert.AreEqual(fixedAssetflagDtos.ToList()[1].Description, fixedAssetsFlagEntities.ToList()[1].Description);
            }
            #endregion
        }
    }
}
