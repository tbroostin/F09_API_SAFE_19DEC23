using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Filters;
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

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    #region vendors v8
    [TestClass]
    public class VendorServiceTests
    {
        [TestClass]
        public class VendorServiceTests_GET : CurrentUserSetup
        {
            //Mock<IPositionRepository> positionRepositoryMock;
            Mock<IVendorsRepository> vendorRepositoryMock;
            Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IInstitutionRepository> institutionRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            VendorsService vendorService;
            IEnumerable<Domain.ColleagueFinance.Entities.Vendors> vendorEntities;
            Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int> vendorEntityTuple;

            IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> vendorTermEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.VendorType> vendorTypeEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> acctPaySourceEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionEntities;
            IEnumerable<Domain.Base.Entities.Institution> institutionsEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            //IEnumerable<Domain.HumanResources.Entities.PositionPay> positionPayEntities;

            private Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                //positionRepositoryMock = new Mock<IPositionRepository>();
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                institutionRepositoryMock = new Mock<IInstitutionRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewVendors);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                vendorEntityTuple = null;
                vendorEntities = null;
                vendorTermEntities = null;
                vendorTypeEntities = null;
                acctPaySourceEntities = null;
                currencyConversionEntities = null;
                vendorRepositoryMock = null;
                referenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
                institutionRepositoryMock = null;
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync()
            {
                VendorFilter filter = new VendorFilter();
                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync(offset, limit, filter, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = vendorEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_EmptyTuple()
            {
                VendorFilter filter = new VendorFilter();
                vendorEntities = new List<Domain.ColleagueFinance.Entities.Vendors>()
                {

                };
                vendorEntityTuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int>(vendorEntities, 0);
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", null, null, null,null)).ReturnsAsync(vendorEntityTuple);
                var actualsTuple = await vendorService.GetVendorsAsync(offset, limit, filter, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_VenderFilter()
            {
                //For some reason need to reset repo's and service to truly run the tests
                string VendorGuid = "VenderGUID123", VenderID = "VenderID123";
                VendorFilter vF = new VendorFilter()
                {
                    vendorDetail = VendorGuid
                };
                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null)).ReturnsAsync(vendorEntityTuple);
                personRepositoryMock = null;
                personRepositoryMock = new Mock<IPersonRepository>();
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(VenderID);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(new Guid().ToString());

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                              baseConfigurationRepository,  adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync(offset, limit, vF, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = vendorEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_VenderFilter_NullorEmpty()
            {
                //For some reason need to reset repo's and service to truly run the tests
                VendorFilter vendorFilter = new VendorFilter() { vendorDetail = "VenderGUID123" };
                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null)).ReturnsAsync(vendorEntityTuple);
                personRepositoryMock = null;
                personRepositoryMock = new Mock<IPersonRepository>();
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync(offset, limit, vendorFilter, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());

            }
            [TestMethod]
            public async Task Vendors_GETAllAsync_ClassificationFilter()
            {
                VendorFilter ClassficationGuid = new VendorFilter() { classifications = "d4ff9cf9-3300-4dca-b52e-59c905021893" };

                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", It.IsAny<List<string>>(), null, null, null)).ReturnsAsync(vendorEntityTuple);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository,  adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                   await
                       vendorService.GetVendorsAsync(offset, limit, ClassficationGuid, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = vendorEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_ClassificationFilter_ClassificationMissing()
            {
                VendorFilter ClassficationGuid = new VendorFilter() { classifications = "BadGuid" };

                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", It.IsAny<List<string>>(), null, null, It.IsAny<List<string>>())).ReturnsAsync(vendorEntityTuple);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                   await
                       vendorService.GetVendorsAsync(offset, limit, ClassficationGuid, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_StatusFilter()
            {
                List<string> statuses = new List<string>() { "active" };
                VendorFilter vF = new VendorFilter() { statuses = statuses };
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", null, statuses, null, It.IsAny<List<string>>())).ReturnsAsync(vendorEntityTuple);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync(offset, limit, vF, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = vendorEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task Vendors_GET_ById()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).ReturnsAsync(expected);
                var actual = await vendorService.GetVendorsByGuidAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }

            [TestMethod]
            public async Task Vendors_CurrencyCodes()
            {
                List<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionItems = new List<Domain.ColleagueFinance.Entities.CurrencyConversion>();
                var currenyCodeItems = Enum.GetValues(typeof(Ellucian.Colleague.Domain.ColleagueFinance.Entities.CurrencyCodes));
                foreach (var item in currenyCodeItems)
                {
                    Domain.ColleagueFinance.Entities.CurrencyConversion currencyConv = new Domain.ColleagueFinance.Entities.CurrencyConversion(item.ToString(), item.ToString()) { CurrencyCode = (Ellucian.Colleague.Domain.ColleagueFinance.Entities.CurrencyCodes)item };
                    currencyConversionItems.Add(currencyConv);
                }
                currencyConversionItems.Add(new Domain.ColleagueFinance.Entities.CurrencyConversion("_", "_") { CurrencyCode = null });

                referenceDataRepositoryMock.Setup(i => i.GetCurrencyConversionAsync()).ReturnsAsync(currencyConversionItems);
                foreach (var item in currencyConversionItems)
                {
                    var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                    var expected = vendorEntities.ToList()[0];
                    expected.CurrencyCode = item.CurrencyCode.HasValue ? item.CurrencyCode.Value.ToString() : "_";
                    vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).ReturnsAsync(expected);
                    var actual = await vendorService.GetVendorsByGuidAsync(id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_GET_ById_NullId_ArgumentNullException()
            {
                var actual = await vendorService.GetVendorsByGuidAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).Throws<KeyNotFoundException>();
                var actual = await vendorService.GetVendorsByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).Throws<InvalidOperationException>();
                var actual = await vendorService.GetVendorsByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).Throws<RepositoryException>();
                var actual = await vendorService.GetVendorsByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).Throws<Exception>();
                var actual = await vendorService.GetVendorsByGuidAsync(id);
            }

            private void BuildData()
            {
                acctPaySourceEntities = new List<Domain.ColleagueFinance.Entities.AccountsPayableSources>() 
                {
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("e43e7195-6eca-451d-b6c3-1e52fe540083", "BMA", "BMA Test"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("95df0303-9b7f-4686-908f-1640b4881e23", "CD", "Central District Office"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("ec49b053-7acc-411a-a766-8a7fc2f24ee3", "COE", "Colonial Ohio-East (coe) Campus"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("eded9894-ea62-44f4-be8e-b141dfc00dba", "COEEA", "Coe-east"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("17ff700b-8d20-43d7-be31-c34933baca75", "CVIL", "Loc Description"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(acctPaySourceEntities);

                vendorTypeEntities = new List<Domain.ColleagueFinance.Entities.VendorType>() 
                {
                    new Domain.ColleagueFinance.Entities.VendorType("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.ColleagueFinance.Entities.VendorType("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.ColleagueFinance.Entities.VendorType("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.ColleagueFinance.Entities.VendorType("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.ColleagueFinance.Entities.VendorType("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypeEntities);

                vendorTermEntities = new List<Domain.ColleagueFinance.Entities.VendorTerm>() 
                {
                    new Domain.ColleagueFinance.Entities.VendorTerm("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTermEntities);

                currencyConversionEntities = new List<Domain.ColleagueFinance.Entities.CurrencyConversion>() 
                {
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("ALU", "American Labor Union"),
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("NEA", "National Education Association")
                };
                referenceDataRepositoryMock.Setup(i => i.GetCurrencyConversionAsync()).ReturnsAsync(currencyConversionEntities);

                vendorEntities = new List<Domain.ColleagueFinance.Entities.Vendors>() 
                {
                    new Domain.ColleagueFinance.Entities.Vendors("ce4d68f6-257d-4052-92c8-17eed0f088fa")
                    { 
                        Id = "0000231",
                        IsOrganization = true,
                        StopPaymentFlag = "Y",
                        ApprovalFlag = "Y",
                        ActiveFlag = "Y",
                        CurrencyCode = "ALU",
                        Comments = "comments",
                        AddDate = DateTime.Now,
                        ApTypes = new List<string>() 
                        {
                            "BMA" 
                        },
                        Misc = new List<string>()
                        {
                            "misc"
                        },
                        Terms = new List<string>() 
                        {
                            "ADJ"
                        },
                        Types = new List<string>()
                        {
                            "Admissions"
                        }
                    },
                    new Domain.ColleagueFinance.Entities.Vendors("5bc2d86c-6a0c-46b1-824d-485ccb27dc67"){IsOrganization = false, Id = "5bc2d86c-6a0c-46b1-824d-485ccb27dc67"},
                    new Domain.ColleagueFinance.Entities.Vendors("7ea5142f-12f1-4ac9-b9f3-73e4205dfc11"),
                    new Domain.ColleagueFinance.Entities.Vendors("db8f690b-071f-4d98-8da8-d4312511a4c1")
                };
                vendorEntityTuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int>(vendorEntities, vendorEntities.Count());
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", null, null, null, It.IsAny<List<string>>())).ReturnsAsync(vendorEntityTuple);
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(vendorEntities.ToList()[0]);
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");

                institutionsEntities = new List<Domain.Base.Entities.Institution>() 
                {
                    new Domain.Base.Entities.Institution("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", Domain.Base.Entities.InstType.College),
                    new Domain.Base.Entities.Institution("61f1f719-cb8e-4827-b314-1e7861bc6e09", Domain.Base.Entities.InstType.College)
                };
                institutionRepositoryMock.Setup(i => i.Get()).Returns(institutionsEntities);
            }
        }

        [TestClass]
        public class VendorServiceTests_PUT_POST : CurrentUserSetup
        {
            Mock<IVendorsRepository> vendorRepositoryMock;
            Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IInstitutionRepository> institutionRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            VendorsService vendorService;
            private Dtos.Vendors vendorDto;
            Domain.ColleagueFinance.Entities.Vendors vendorEntity;

            IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> vendorTermEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.VendorType> vendorTypeEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> acctPaySourceEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionEntities;
            IEnumerable<Domain.Base.Entities.Institution> institutionsEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;
            private const string vendorGuid = "a830e686-7692-4012-8da5-b1b5d44389b4";

            [TestInitialize]
            public void Initialize()
            {
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                institutionRepositoryMock = new Mock<IInstitutionRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdateVendors);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                vendorEntity = null;
                vendorTermEntities = null;
                vendorTypeEntities = null;
                acctPaySourceEntities = null;
                currencyConversionEntities = null;
                vendorRepositoryMock = null;
                referenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
                institutionRepositoryMock = null;
            }

            [TestMethod]
            public async Task Vendors_PUT()
            {
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(vendorDto.Id, result.Id);
            }

            [TestMethod]
            public async Task Vendors_POST()
            {
                var result = await vendorService.PostVendorAsync(vendorDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(vendorDto.Id, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_PUT_DtoNull_ArgumentNullException()
            {
                var result = await vendorService.PutVendorAsync(vendorGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_PUT_DtoIdNull_ArgumentNullException()
            {
                vendorDto.Id = "";
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_StartDateChange_ArgumentException()
            {
                vendorEntity.AddDate = DateTime.Today.AddDays(-1);
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(vendorEntity);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_NotContainActive_ArgumentException()
            {
                vendorDto.Statuses = new List<VendorsStatuses?>() { VendorsStatuses.Active };
                vendorDto.VendorHoldReasons = null;
                vendorEntity.AddDate = null;
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(vendorEntity);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task Vendors_PUT_RepositoryException()
            {
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Exception()
            {
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_CurrencyCodes_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetCurrencyConversionAsync())
                    .ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_CurrencyCode_NotSet_KeyNotFoundException()
            {
                vendorDto.DefaultCurrency = CurrencyIsoCode.NotSet;
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Institution_Null_KeyNotFoundException()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>())).ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_PersonCorpIndicator_Null_ArgumentException()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "" });
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_PersonCorpIndicator_Is_N_ArgumentException()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Invalid_InstitutionId_ArgumentException()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("ZZZ", "LastName") { PersonCorpIndicator = "N" });
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Organization_Null_KeyNotFoundException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(null);

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_OrganizationId_Invalid_KeyNotFoundException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_PersonCorpIndicator_Y_ArgumentException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("ABCD", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorDetail_Person_Null_KeyNotFoundException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = null;
                vendorDto.VendorDetail.Person = new GuidObject2("ABCD");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(null);

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorDetail_Person_Not_Null_ArgumentException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = null;
                vendorDto.VendorDetail.Person = new GuidObject2("ABCD");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorDetail_PersonCorpIndicator_Y_ArgumentException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = null;
                vendorDto.VendorDetail.Person = new GuidObject2("ABCD");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("zzz", "LastName") { PersonCorpIndicator = "Y" });

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_AccountsPayableSources_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_AccountsPayableSource_Null_KeyNotFoundException()
            {
                vendorDto.PaymentSources.First().Id = "";
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorTerms_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_PaymentTerm_Null_KeyNotFoundException()
            {
                vendorDto.PaymentTerms = new List<GuidObject2>() { new GuidObject2("")};
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorTypes_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Classifications_Null_KeyNotFoundException()
            {
                vendorDto.Classifications = new List<GuidObject2>() { new GuidObject2("") };
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorHoldReasons_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorHoldReason_Null_KeyNotFoundException()
            {
                vendorDto.VendorHoldReasons = new List<GuidObject2>() { new GuidObject2("") };
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_POST_VendorNull_ArgumentNullException()
            {
                var result = await vendorService.PostVendorAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_POST_VendorIdNull_ArgumentNullException()
            {
                vendorDto.Id = null;
                var result = await vendorService.PostVendorAsync(vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task Vendors_POST_RepositoryException()
            {
                vendorRepositoryMock.Setup(i => i.CreateVendorsAsync(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>()))
                    .ThrowsAsync(new RepositoryException());
                var result = await vendorService.PostVendorAsync(vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_POST_Exception()
            {
                vendorRepositoryMock.Setup(i => i.CreateVendorsAsync(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>()))
                    .ThrowsAsync(new Exception());
                var result = await vendorService.PostVendorAsync(vendorDto);
            }

            private void BuildData()
            {
                vendorDto = new Ellucian.Colleague.Dtos.Vendors
                {
                    Id = vendorGuid,
                    Comment = "Some comment",
                    DefaultCurrency = CurrencyIsoCode.USD,
                    PaymentSources = new List<GuidObject2>() 
                    {
                        new GuidObject2("e43e7195-6eca-451d-b6c3-1e52fe540083")
                    },
                    RelatedVendor = new List<RelatedVendorDtoProperty>()
                    {
                       new RelatedVendorDtoProperty()
                       {
                           Type = Dtos.EnumProperties.VendorType.ParentVendor,
                           Vendor = new GuidObject2("4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46")
                       }
                    },
                    StartOn = DateTime.Today,
                    Classifications = new List<GuidObject2>() 
                    {
                        new GuidObject2("d4ff9cf9-3300-4dca-b52e-59c905021893")
                    },
                    VendorDetail = new VendorDetailsDtoProperty()
                    {
                        Institution = new GuidObject2("b42ca98d-edee-42da-8ddf-2a9e915221e7")
                    },
                    PaymentTerms = new List<GuidObject2>() 
                    {
                        new GuidObject2("c1b91008-ba77-4b5b-8b77-84f5a7ae1632")
                    },
                    VendorHoldReasons = new List<GuidObject2>() 
                    {
                        new GuidObject2("c8263488-bf7d-45a7-9190-39b9587561a1")
                    },
                    Statuses = new List<Dtos.EnumProperties.VendorsStatuses?>() 
                    {
                        Dtos.EnumProperties.VendorsStatuses.Holdpayment
                    }

                };
                vendorRepositoryMock.Setup(repo => repo.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0000231");
                acctPaySourceEntities = new List<Domain.ColleagueFinance.Entities.AccountsPayableSources>() 
                {
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("e43e7195-6eca-451d-b6c3-1e52fe540083", "BMA", "BMA Test"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("95df0303-9b7f-4686-908f-1640b4881e23", "CD", "Central District Office"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("ec49b053-7acc-411a-a766-8a7fc2f24ee3", "COE", "Colonial Ohio-East (coe) Campus"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("eded9894-ea62-44f4-be8e-b141dfc00dba", "COEEA", "Coe-east"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("17ff700b-8d20-43d7-be31-c34933baca75", "CVIL", "Loc Description"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(acctPaySourceEntities);

                vendorTypeEntities = new List<Domain.ColleagueFinance.Entities.VendorType>() 
                {
                    new Domain.ColleagueFinance.Entities.VendorType("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.ColleagueFinance.Entities.VendorType("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.ColleagueFinance.Entities.VendorType("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.ColleagueFinance.Entities.VendorType("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.ColleagueFinance.Entities.VendorType("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypeEntities);

                vendorTermEntities = new List<Domain.ColleagueFinance.Entities.VendorTerm>() 
                {
                    new Domain.ColleagueFinance.Entities.VendorTerm("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTermEntities);

                currencyConversionEntities = new List<Domain.ColleagueFinance.Entities.CurrencyConversion>() 
                {
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("USD", "United States"){ CurrencyCode = Domain.ColleagueFinance.Entities.CurrencyCodes.USD },
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("CAN", "Canada"){ CurrencyCode = Domain.ColleagueFinance.Entities.CurrencyCodes.CAD }
                };
                referenceDataRepositoryMock.Setup(i => i.GetCurrencyConversionAsync()).ReturnsAsync(currencyConversionEntities);

                vendorEntity = new Domain.ColleagueFinance.Entities.Vendors(vendorGuid)
                {
                    Id = "0000231",
                    AddDate = DateTime.Today.AddDays(-5),
                    IsOrganization = true,
                    StopPaymentFlag = "Y",
                    ApprovalFlag = "Y",
                    ActiveFlag = "Y",
                    CurrencyCode = "ALU",
                    Comments = "comments",
                    ApTypes = new List<string>() 
                        {
                            "BMA" 
                        },
                    Misc = new List<string>()
                        {
                            "misc"
                        },
                    Terms = new List<string>() 
                        {
                            "ADJ"
                        },
                    Types = new List<string>()
                        {
                            "Admissions"
                        }
                };
                vendorRepositoryMock.Setup(i => i.UpdateVendorsAsync(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>())).ReturnsAsync(vendorEntity);
                vendorRepositoryMock.Setup(i => i.CreateVendorsAsync(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>())).ReturnsAsync(vendorEntity);

                List<Domain.ColleagueFinance.Entities.VendorHoldReasons> vendorHoldReasons = new List<Domain.ColleagueFinance.Entities.VendorHoldReasons>() 
                {
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("aa06e48b-cfb9-4341-83f8-4dc4b2d0fad1", "OB", "Out of Business"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("f8c04467-9b72-46f8-ba1c-ba6aeb0bb6a6", "DISC", "Vendor Discontinued"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("c8263488-bf7d-45a7-9190-39b9587561a1", "QUAL", "Quality Hold"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("f9865084-2c02-484e-8286-b98afa5909cc", "DISP", "Disputed Transaction desc"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ReturnsAsync(vendorHoldReasons);

                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "Y" });

                institutionsEntities = new List<Domain.Base.Entities.Institution>() 
                {
                    new Domain.Base.Entities.Institution("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", Domain.Base.Entities.InstType.College),
                    new Domain.Base.Entities.Institution("61f1f719-cb8e-4827-b314-1e7861bc6e09", Domain.Base.Entities.InstType.College)
                };
                institutionRepositoryMock.Setup(i => i.GetInstitutionsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(institutionsEntities);
            }
        }
    }
    #endregion

    #region vendors v11
    [TestClass]
    public class VendorServiceTests_v11
    {
        [TestClass]
        public class VendorServiceTests_GET : CurrentUserSetup
        {
            //Mock<IPositionRepository> positionRepositoryMock;
            Mock<IVendorsRepository> vendorRepositoryMock;
            Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IInstitutionRepository> institutionRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            VendorsService vendorService;
            IEnumerable<Domain.ColleagueFinance.Entities.Vendors> vendorEntities;
            Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int> vendorEntityTuple;

            IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> vendorTermEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.VendorType> vendorTypeEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> acctPaySourceEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionEntities;
            IEnumerable<Domain.Base.Entities.Institution> institutionsEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            //IEnumerable<Domain.HumanResources.Entities.PositionPay> positionPayEntities;

            private Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                //positionRepositoryMock = new Mock<IPositionRepository>();
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                institutionRepositoryMock = new Mock<IInstitutionRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewVendors);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                vendorEntityTuple = null;
                vendorEntities = null;
                vendorTermEntities = null;
                vendorTypeEntities = null;
                acctPaySourceEntities = null;
                currencyConversionEntities = null;
                vendorRepositoryMock = null;
                referenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
                institutionRepositoryMock = null;
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync2()
            {
                VendorFilter filter = new VendorFilter();
                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, "", null, null, null, null, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = vendorEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_EmptyTuple2()
            {
                VendorFilter filter = new VendorFilter();
                vendorEntities = new List<Domain.ColleagueFinance.Entities.Vendors>()
                {

                };
                vendorEntityTuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int>(vendorEntities, 0);
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", null, null, null, null)).ReturnsAsync(vendorEntityTuple);
                var actualsTuple = await vendorService.GetVendorsAsync2(offset, limit, "", null, null, null, null, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_VenderFilter2()
            {
                //For some reason need to reset repo's and service to truly run the tests
                string VendorGuid = "VenderGUID123", VenderID = "VenderID123";
                
                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null)).ReturnsAsync(vendorEntityTuple);
                personRepositoryMock = null;
                personRepositoryMock = new Mock<IPersonRepository>();
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(VenderID);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(new Guid().ToString());

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                              baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, VendorGuid, null, null, null, null, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = vendorEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_VenderFilter_NullorEmpty2()
            {
                //For some reason need to reset repo's and service to truly run the tests
                 vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null)).ReturnsAsync(vendorEntityTuple);
                personRepositoryMock = null;
                personRepositoryMock = new Mock<IPersonRepository>();
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, "VenderGUID123", null, null, null, null, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());

            }
            [TestMethod]
            public async Task Vendors_GETAllAsync_ClassificationFilter2()
            {
                var classficationGuid =  new List<string>() { "d4ff9cf9-3300-4dca-b52e-59c905021893" };

                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", It.IsAny<List<string>>(), null, null, null)).ReturnsAsync(vendorEntityTuple);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                   await
                       vendorService.GetVendorsAsync2(offset, limit, "", classficationGuid, null, null, null, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = vendorEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_ClassificationFilter_ClassificationMissing2()
            {
                var classficationGuid = new List<string> { "BadGuid" };

                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", It.IsAny<List<string>>(), null, null, It.IsAny<List<string>>())).ReturnsAsync(vendorEntityTuple);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                   await
                       vendorService.GetVendorsAsync2(offset, limit, "", classficationGuid, null, null, null, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_StatusFilter2()
            {
                var status =  "active" ;
                var statuses = new List<string>();
                statuses.Add(status);
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", null, statuses, null, It.IsAny<List<string>>())).ReturnsAsync(vendorEntityTuple);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, "", null, null, null, null, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = vendorEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task Vendors_GET_ById2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).ReturnsAsync(expected);
                var actual = await vendorService.GetVendorsByGuidAsync2(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }
            
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_GET_ById_NullId_ArgumentNullException2()
            {
                var actual = await vendorService.GetVendorsByGuidAsync2(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_KeyNotFoundException2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).Throws<KeyNotFoundException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_InvalidOperationException2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).Throws<InvalidOperationException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_RepositoryException2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).Throws<RepositoryException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_Exception2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(id)).Throws<Exception>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            private void BuildData()
            {
                acctPaySourceEntities = new List<Domain.ColleagueFinance.Entities.AccountsPayableSources>()
                {
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("e43e7195-6eca-451d-b6c3-1e52fe540083", "BMA", "BMA Test"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("95df0303-9b7f-4686-908f-1640b4881e23", "CD", "Central District Office"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("ec49b053-7acc-411a-a766-8a7fc2f24ee3", "COE", "Colonial Ohio-East (coe) Campus"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("eded9894-ea62-44f4-be8e-b141dfc00dba", "COEEA", "Coe-east"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("17ff700b-8d20-43d7-be31-c34933baca75", "CVIL", "Loc Description"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(acctPaySourceEntities);

                vendorTypeEntities = new List<Domain.ColleagueFinance.Entities.VendorType>()
                {
                    new Domain.ColleagueFinance.Entities.VendorType("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.ColleagueFinance.Entities.VendorType("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.ColleagueFinance.Entities.VendorType("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.ColleagueFinance.Entities.VendorType("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.ColleagueFinance.Entities.VendorType("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypeEntities);

                vendorTermEntities = new List<Domain.ColleagueFinance.Entities.VendorTerm>()
                {
                    new Domain.ColleagueFinance.Entities.VendorTerm("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTermEntities);

                currencyConversionEntities = new List<Domain.ColleagueFinance.Entities.CurrencyConversion>()
                {
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("ALU", "American Labor Union"),
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("NEA", "National Education Association")
                };
                referenceDataRepositoryMock.Setup(i => i.GetCurrencyConversionAsync()).ReturnsAsync(currencyConversionEntities);

                vendorEntities = new List<Domain.ColleagueFinance.Entities.Vendors>()
                {
                    new Domain.ColleagueFinance.Entities.Vendors("ce4d68f6-257d-4052-92c8-17eed0f088fa")
                    {
                        Id = "0000231",
                        IsOrganization = true,
                        StopPaymentFlag = "Y",
                        ApprovalFlag = "Y",
                        ActiveFlag = "Y",
                        CurrencyCode = "ALU",
                        Comments = "comments",
                        AddDate = DateTime.Now,
                        ApTypes = new List<string>()
                        {
                            "BMA"
                        },
                        Misc = new List<string>()
                        {
                            "misc"
                        },
                        Terms = new List<string>()
                        {
                            "ADJ"
                        },
                        Types = new List<string>()
                        {
                            "Admissions"
                        },
                        Categories = new List<string>() { "EP", "TR" }
                    },
                    new Domain.ColleagueFinance.Entities.Vendors("5bc2d86c-6a0c-46b1-824d-485ccb27dc67"){IsOrganization = false, Id = "5bc2d86c-6a0c-46b1-824d-485ccb27dc67"},
                    new Domain.ColleagueFinance.Entities.Vendors("7ea5142f-12f1-4ac9-b9f3-73e4205dfc11"),
                    new Domain.ColleagueFinance.Entities.Vendors("db8f690b-071f-4d98-8da8-d4312511a4c1")
                };
                vendorEntityTuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int>(vendorEntities, vendorEntities.Count());
                vendorRepositoryMock.Setup(i => i.GetVendorsAsync(It.IsAny<int>(), It.IsAny<int>(), "", null, null, null, It.IsAny<List<string>>())).ReturnsAsync(vendorEntityTuple);
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(vendorEntities.ToList()[0]);
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");

                institutionsEntities = new List<Domain.Base.Entities.Institution>()
                {
                    new Domain.Base.Entities.Institution("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", Domain.Base.Entities.InstType.College),
                    new Domain.Base.Entities.Institution("61f1f719-cb8e-4827-b314-1e7861bc6e09", Domain.Base.Entities.InstType.College)
                };
                institutionRepositoryMock.Setup(i => i.Get()).Returns(institutionsEntities);
            }
        }

        [TestClass]
        public class VendorServiceTests_PUT_POST2 : CurrentUserSetup
        {
            Mock<IVendorsRepository> vendorRepositoryMock;
            Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IInstitutionRepository> institutionRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            VendorsService vendorService;
            private Dtos.Vendors2 vendorDto;
            Domain.ColleagueFinance.Entities.Vendors vendorEntity;

            IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> vendorTermEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.VendorType> vendorTypeEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> acctPaySourceEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionEntities;
            IEnumerable<Domain.Base.Entities.Institution> institutionsEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;
            private const string vendorGuid = "a830e686-7692-4012-8da5-b1b5d44389b4";

            [TestInitialize]
            public void Initialize()
            {
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                institutionRepositoryMock = new Mock<IInstitutionRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.UpdateVendors);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                vendorEntity = null;
                vendorTermEntities = null;
                vendorTypeEntities = null;
                acctPaySourceEntities = null;
                currencyConversionEntities = null;
                vendorRepositoryMock = null;
                referenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
                institutionRepositoryMock = null;
            }

            [TestMethod]
            public async Task Vendors_PUT2()
            {
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(vendorDto.Id, result.Id);
            }

            [TestMethod]
            public async Task Vendors_POST2()
            {
                var result = await vendorService.PostVendorAsync2(vendorDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(vendorDto.Id, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_PUT_DtoNull_ArgumentNullException2()
            {
                var result = await vendorService.PutVendorAsync2(vendorGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_PUT_DtoIdNull_ArgumentNullException2()
            {
                vendorDto.Id = "";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_StartDateChange_ArgumentException2()
            {
                vendorEntity.AddDate = DateTime.Today.AddDays(-1);
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(vendorEntity);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_NotContainActive_ArgumentException2()
            {
                vendorDto.Statuses = new List<VendorsStatuses?>() { VendorsStatuses.Active };
                vendorDto.VendorHoldReasons = null;
                vendorEntity.AddDate = null;
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(vendorEntity);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task Vendors_PUT_RepositoryException2()
            {
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Exception2()
            {
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_CurrencyCodes_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetCurrencyConversionAsync())
                    .ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_CurrencyCode_NotSet_KeyNotFoundException2()
            {
                vendorDto.DefaultCurrency = CurrencyIsoCode.NotSet;
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Institution_Null_KeyNotFoundException2()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>())).ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_PersonCorpIndicator_Null_ArgumentException2()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "" });
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_PersonCorpIndicator_Is_N_ArgumentException2()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Invalid_InstitutionId_ArgumentException2()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("ZZZ", "LastName") { PersonCorpIndicator = "N" });
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Organization_Null_KeyNotFoundException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(null);

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_OrganizationId_Invalid_KeyNotFoundException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_PersonCorpIndicator_Y_ArgumentException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("ABCD", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorDetail_Person_Null_KeyNotFoundException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = null;
                vendorDto.VendorDetail.Person = new GuidObject2("ABCD");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(null);

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorDetail_Person_Not_Null_ArgumentException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = null;
                vendorDto.VendorDetail.Person = new GuidObject2("ABCD");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorDetail_PersonCorpIndicator_Y_ArgumentException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = null;
                vendorDto.VendorDetail.Person = new GuidObject2("ABCD");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("zzz", "LastName") { PersonCorpIndicator = "Y" });

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_AccountsPayableSources_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_AccountsPayableSource_Null_KeyNotFoundException2()
            {
                vendorDto.PaymentSources.First().Id = "";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorTerms_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_PaymentTerm_Null_KeyNotFoundException2()
            {
                vendorDto.PaymentTerms = new List<GuidObject2>() { new GuidObject2("") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorTypes_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_Classifications_Null_KeyNotFoundException2()
            {
                vendorDto.Classifications = new List<GuidObject2>() { new GuidObject2("") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorHoldReasons_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_PUT_VendorHoldReason_Null_KeyNotFoundException2()
            {
                vendorDto.VendorHoldReasons = new List<GuidObject2>() { new GuidObject2("") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_POST_VendorNull_ArgumentNullException2()
            {
                var result = await vendorService.PostVendorAsync2(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_POST_VendorIdNull_ArgumentNullException2()
            {
                vendorDto.Id = null;
                var result = await vendorService.PostVendorAsync2(vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task Vendors_POST_RepositoryException2()
            {
                vendorRepositoryMock.Setup(i => i.CreateVendorsAsync(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>()))
                    .ThrowsAsync(new RepositoryException());
                var result = await vendorService.PostVendorAsync2(vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task Vendors_POST_Exception2()
            {
                vendorRepositoryMock.Setup(i => i.CreateVendorsAsync(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>()))
                    .ThrowsAsync(new Exception());
                var result = await vendorService.PostVendorAsync2(vendorDto);
            }

            private void BuildData()
            {
                vendorDto = new Ellucian.Colleague.Dtos.Vendors2
                {
                    Id = vendorGuid,
                    Comment = "Some comment",
                    DefaultCurrency = CurrencyIsoCode.USD,
                    PaymentSources = new List<GuidObject2>()
                    {
                        new GuidObject2("e43e7195-6eca-451d-b6c3-1e52fe540083")
                    },
                    RelatedVendor = new List<RelatedVendorDtoProperty>()
                    {
                       new RelatedVendorDtoProperty()
                       {
                           Type = Dtos.EnumProperties.VendorType.ParentVendor,
                           Vendor = new GuidObject2("4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46")
                       }
                    },
                    StartOn = DateTime.Today,
                    Classifications = new List<GuidObject2>()
                    {
                        new GuidObject2("d4ff9cf9-3300-4dca-b52e-59c905021893")
                    },
                    VendorDetail = new VendorDetailsDtoProperty()
                    {
                        Institution = new GuidObject2("b42ca98d-edee-42da-8ddf-2a9e915221e7")
                    },
                    PaymentTerms = new List<GuidObject2>()
                    {
                        new GuidObject2("c1b91008-ba77-4b5b-8b77-84f5a7ae1632")
                    },
                    VendorHoldReasons = new List<GuidObject2>()
                    {
                        new GuidObject2("c8263488-bf7d-45a7-9190-39b9587561a1")
                    },
                    Statuses = new List<Dtos.EnumProperties.VendorsStatuses?>()
                    {
                        Dtos.EnumProperties.VendorsStatuses.Holdpayment
                    },
                    Types = new List<VendorTypes>() {  VendorTypes.Travel}
                };
                vendorRepositoryMock.Setup(repo => repo.GetVendorIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0000231");
                acctPaySourceEntities = new List<Domain.ColleagueFinance.Entities.AccountsPayableSources>()
                {
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("e43e7195-6eca-451d-b6c3-1e52fe540083", "BMA", "BMA Test"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("95df0303-9b7f-4686-908f-1640b4881e23", "CD", "Central District Office"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("ec49b053-7acc-411a-a766-8a7fc2f24ee3", "COE", "Colonial Ohio-East (coe) Campus"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("eded9894-ea62-44f4-be8e-b141dfc00dba", "COEEA", "Coe-east"),
                    new Domain.ColleagueFinance.Entities.AccountsPayableSources("17ff700b-8d20-43d7-be31-c34933baca75", "CVIL", "Loc Description"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(acctPaySourceEntities);

                vendorTypeEntities = new List<Domain.ColleagueFinance.Entities.VendorType>()
                {
                    new Domain.ColleagueFinance.Entities.VendorType("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.ColleagueFinance.Entities.VendorType("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.ColleagueFinance.Entities.VendorType("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.ColleagueFinance.Entities.VendorType("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.ColleagueFinance.Entities.VendorType("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypeEntities);

                vendorTermEntities = new List<Domain.ColleagueFinance.Entities.VendorTerm>()
                {
                    new Domain.ColleagueFinance.Entities.VendorTerm("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTermEntities);

                currencyConversionEntities = new List<Domain.ColleagueFinance.Entities.CurrencyConversion>()
                {
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("USD", "United States"){ CurrencyCode = Domain.ColleagueFinance.Entities.CurrencyCodes.USD },
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("CAN", "Canada"){ CurrencyCode = Domain.ColleagueFinance.Entities.CurrencyCodes.CAD }
                };
                referenceDataRepositoryMock.Setup(i => i.GetCurrencyConversionAsync()).ReturnsAsync(currencyConversionEntities);

                vendorEntity = new Domain.ColleagueFinance.Entities.Vendors(vendorGuid)
                {
                    Id = "0000231",
                    AddDate = DateTime.Today.AddDays(-5),
                    IsOrganization = true,
                    StopPaymentFlag = "Y",
                    ApprovalFlag = "Y",
                    ActiveFlag = "Y",
                    CurrencyCode = "ALU",
                    Comments = "comments",
                    ApTypes = new List<string>()
                        {
                            "BMA"
                        },
                    Misc = new List<string>()
                        {
                            "misc"
                        },
                    Terms = new List<string>()
                        {
                            "ADJ"
                        },
                    Types = new List<string>()
                        {
                            "Admissions"
                        }
                };
                vendorRepositoryMock.Setup(i => i.UpdateVendorsAsync(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>())).ReturnsAsync(vendorEntity);
                vendorRepositoryMock.Setup(i => i.CreateVendorsAsync(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>())).ReturnsAsync(vendorEntity);

                List<Domain.ColleagueFinance.Entities.VendorHoldReasons> vendorHoldReasons = new List<Domain.ColleagueFinance.Entities.VendorHoldReasons>()
                {
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("aa06e48b-cfb9-4341-83f8-4dc4b2d0fad1", "OB", "Out of Business"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("f8c04467-9b72-46f8-ba1c-ba6aeb0bb6a6", "DISC", "Vendor Discontinued"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("c8263488-bf7d-45a7-9190-39b9587561a1", "QUAL", "Quality Hold"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("f9865084-2c02-484e-8286-b98afa5909cc", "DISP", "Disputed Transaction desc"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ReturnsAsync(vendorHoldReasons);

                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "Y" });

                institutionsEntities = new List<Domain.Base.Entities.Institution>()
                {
                    new Domain.Base.Entities.Institution("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", Domain.Base.Entities.InstType.College),
                    new Domain.Base.Entities.Institution("61f1f719-cb8e-4827-b314-1e7861bc6e09", Domain.Base.Entities.InstType.College)
                };
                institutionRepositoryMock.Setup(i => i.GetInstitutionsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(institutionsEntities);
            }
        }
    }
    #endregion
}
