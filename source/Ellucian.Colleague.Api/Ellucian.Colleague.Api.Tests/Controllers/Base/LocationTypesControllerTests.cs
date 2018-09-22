// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class LocationTypesControllerTests
    {
        [TestClass]
        public class LocationTypesControllerGet
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private LocationTypesController locationTypesController;
            private Mock<IReferenceDataRepository> refRepositoryMock;
            private IReferenceDataRepository refRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;

            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.LocationTypeItem> allLocationTypes;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<ILocationTypeService> locationTypesServiceMock;
            private ILocationTypeService locationTypesService;
            private string locationTypesGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";

            List<Dtos.LocationTypeItem> allLocationTypesList;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                refRepositoryMock = new Mock<IReferenceDataRepository>();
                refRepository = refRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                locationTypesServiceMock = new Mock<ILocationTypeService>();
                locationTypesService = locationTypesServiceMock.Object;

                allLocationTypes = new TestLocationTypeRepository().Get();
                allLocationTypesList = new List<Dtos.LocationTypeItem>();

                locationTypesController = new LocationTypesController(adapterRegistry, locationTypesService, logger);
                locationTypesController.Request = new HttpRequestMessage();
                locationTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var locationType in allLocationTypes)
                {
                    Dtos.LocationTypeItem target = ConvertLocationTypeEntityToLocationTypeDto(locationType);
                    //target.Id = locationType.Guid;

                    allLocationTypesList.Add(target);
                }

                refRepositoryMock.Setup(repo => repo.GetLocationTypesAsync(It.IsAny<bool>())).ReturnsAsync(allLocationTypes);
                //refRepositoryMock.Setup(repo => repo.RoomTypesAsync()).ReturnsAsync(allLocationTypes);
            }

            [TestCleanup]
            public void Cleanup()
            {
                locationTypesController = null;
                refRepository = null;
                locationTypesService = null;
                allLocationTypes = null;
                allLocationTypesList = null;
            }

            [TestMethod]
            public async Task GetLocationTypesByGuidAsync_Validate()
            {
                var thisLocationType = allLocationTypesList.Where(m => m.Id == locationTypesGuid).FirstOrDefault();

                locationTypesServiceMock.Setup(x => x.GetLocationTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisLocationType);

                var locationType = await locationTypesController.GetLocationTypeByGuidAsync(locationTypesGuid);
                Assert.AreEqual(thisLocationType.Id, locationType.Id);
                Assert.AreEqual(thisLocationType.Code, locationType.Code);
                Assert.AreEqual(thisLocationType.Description, locationType.Description);
                Assert.AreEqual(thisLocationType.Type, locationType.Type);
            }

            [TestMethod]
            public async Task LocationTypesController_GetHedmAsync()
            {
                locationTypesServiceMock.Setup(gc => gc.GetLocationTypesAsync(It.IsAny<bool>())).ReturnsAsync(allLocationTypesList);

                var result = await locationTypesController.GetLocationTypesAsync();
                Assert.AreEqual(result.Count(), allLocationTypes.Count());

                int count = allLocationTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allLocationTypesList[i];
                    var actual = allLocationTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task LocationTypesController_GetHedmAsync_CacheControlNotNull()
            {
                locationTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                locationTypesServiceMock.Setup(gc => gc.GetLocationTypesAsync(It.IsAny<bool>())).ReturnsAsync(allLocationTypesList);

                var result = await locationTypesController.GetLocationTypesAsync();
                Assert.AreEqual(result.Count(), allLocationTypes.Count());

                int count = allLocationTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allLocationTypesList[i];
                    var actual = allLocationTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task LocationTypesController_GetHedmAsync_NoCache()
            {
                locationTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                locationTypesController.Request.Headers.CacheControl.NoCache = true;

                locationTypesServiceMock.Setup(gc => gc.GetLocationTypesAsync(It.IsAny<bool>())).ReturnsAsync(allLocationTypesList);

                var result = await locationTypesController.GetLocationTypesAsync();
                Assert.AreEqual(result.Count(), allLocationTypes.Count());

                int count = allLocationTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allLocationTypesList[i];
                    var actual = allLocationTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task LocationTypesController_GetHedmAsync_Cache()
            {
                locationTypesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                locationTypesController.Request.Headers.CacheControl.NoCache = false;

                locationTypesServiceMock.Setup(gc => gc.GetLocationTypesAsync(It.IsAny<bool>())).ReturnsAsync(allLocationTypesList);

                var result = await locationTypesController.GetLocationTypesAsync();
                Assert.AreEqual(result.Count(), allLocationTypes.Count());

                int count = allLocationTypes.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = allLocationTypesList[i];
                    var actual = allLocationTypes.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task LocationTypesController_GetByIdHedmAsync()
            {
                var thisLocationType = allLocationTypesList.Where(m => m.Id == "9ae3a175-1dfd-4937-b97b-3c9ad596e023").FirstOrDefault();

                locationTypesServiceMock.Setup(x => x.GetLocationTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisLocationType);

                var roomType = await locationTypesController.GetLocationTypeByGuidAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
                Assert.AreEqual(thisLocationType.Id, roomType.Id);
                Assert.AreEqual(thisLocationType.Code, roomType.Code);
                Assert.AreEqual(thisLocationType.Description, roomType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LocationTypeController_GetThrowsIntAppiExc()
            {
                locationTypesServiceMock.Setup(gc => gc.GetLocationTypesAsync(It.IsAny<bool>())).Throws<Exception>();

                await locationTypesController.GetLocationTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task LocationTypeController_GetByIdThrowsIntAppiExc()
            {
                locationTypesServiceMock.Setup(gc => gc.GetLocationTypeByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await locationTypesController.GetLocationTypeByGuidAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void LocationTypeController_PostThrowsIntAppiExc()
            {
                locationTypesController.PostLocationTypes(allLocationTypesList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void LocationTypeController_PutThrowsIntAppiExc()
            {
                var result = locationTypesController.PutLocationTypes(allLocationTypesList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void LocationTypeController_DeleteThrowsIntAppiExc()
            {
                locationTypesController.DeleteLocationTypes("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
            /// <summary>
            /// Converts a LocationType domain entity to its corresponding LocationType DTO
            /// </summary>
            /// <param name="source">LocationType domain entity</param>
            /// <returns>LocationType DTO</returns>
            private Dtos.LocationTypeItem ConvertLocationTypeEntityToLocationTypeDto(Ellucian.Colleague.Domain.Base.Entities.LocationTypeItem source)
            {
                var locationType = new Dtos.LocationTypeItem();
                locationType.Id = source.Guid;
                locationType.Code = source.Code;
                locationType.Title = source.Description;
                locationType.Description = null;
                locationType.Type = new Dtos.LocationType();

                if (source.Type.EntityType == Ellucian.Colleague.Domain.Base.Entities.EntityType.Person)
                {

                    locationType.Type.PersonLocationType = ConvertPersonLocationTypeDomainEnumToPersonLocationTypeDtoEnum(source.Type.PersonLocationType);
                }
                else
                {
                    locationType.Type.OrganizationLocationType = ConvertOrganizationLocationTypeDomainEnumToOrganizationLocationTypeDtoEnum(source.Type.OrganizationLocationType);
                }
                return locationType;
            }



            /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
            /// <summary>
            /// Converts a PersonLocationType domain enumeration value to its corresponding PersonLocationType DTO enumeration value
            /// </summary>
            /// <param name="source">PersonLocationType domain enumeration value</param>
            /// <returns>PersonLocationType DTO enumeration value</returns>
            private Dtos.PersonLocationType ConvertPersonLocationTypeDomainEnumToPersonLocationTypeDtoEnum(Ellucian.Colleague.Domain.Base.Entities.PersonLocationType source)
            {
                switch (source)
                {
                    case Domain.Base.Entities.PersonLocationType.Billing:
                        return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Billing };
                    case Domain.Base.Entities.PersonLocationType.Home:
                        return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Home };
                    case Domain.Base.Entities.PersonLocationType.Business:
                        return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Business };
                    case Domain.Base.Entities.PersonLocationType.Mailing:
                        return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Mailing };
                    case Domain.Base.Entities.PersonLocationType.School:
                        return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.School };
                    case Domain.Base.Entities.PersonLocationType.Shipping:
                        return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Shipping };
                    case Domain.Base.Entities.PersonLocationType.Vacation:
                        return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Vacation };
                    default:
                        return new Dtos.PersonLocationType { PersonLocationTypeList = Dtos.PersonLocationTypeList.Other };

                }
            }

            /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
            /// <summary>
            /// Converts a OrganizationLocationType domain enumeration value to its corresponding OrganizationLocationType DTO enumeration value
            /// </summary>
            /// <param name="source">OrganizationLocationType domain enumeration value</param>
            /// <returns>OrganizationLocationType DTO enumeration value</returns>
            private Dtos.OrganizationLocationType ConvertOrganizationLocationTypeDomainEnumToOrganizationLocationTypeDtoEnum(Ellucian.Colleague.Domain.Base.Entities.OrganizationLocationType source)
            {
                switch (source)
                {
                    case Domain.Base.Entities.OrganizationLocationType.Branch:
                        return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Branch };
                    case Domain.Base.Entities.OrganizationLocationType.Business:
                        return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Business };
                    case Domain.Base.Entities.OrganizationLocationType.Main:
                        return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Main };
                    case Domain.Base.Entities.OrganizationLocationType.Pobox:
                        return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Pobox };
                    case Domain.Base.Entities.OrganizationLocationType.Region:
                        return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Region };
                    case Domain.Base.Entities.OrganizationLocationType.Support:
                        return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Support };
                    default:
                        return new Dtos.OrganizationLocationType { OrganizationLocationTypeList = Dtos.OrganizationLocationTypeList.Other };
                }
            }
        }
    }
}