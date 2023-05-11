using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Filters;
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
            Mock<IReferenceDataRepository> refDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IVendorContactsRepository> vendorContactsRepositoryMock;
            Mock<IAddressRepository> addressRepositoryMock;
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
                refDataRepositoryMock = new Mock<IReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                addressRepositoryMock = new Mock<IAddressRepository>();
                vendorContactsRepositoryMock = new Mock<IVendorContactsRepository>();
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

                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
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
                addressRepositoryMock = new Mock<IAddressRepository>();

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(VenderID);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(new Guid().ToString());
                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", "db8f690b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object, 
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
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
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
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
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
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
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
            [ExpectedException(typeof(ColleagueWebApiException))]
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
                foreach (var record in acctPaySourceEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetAccountsPayableSourceGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                vendorTypeEntities = new List<Domain.ColleagueFinance.Entities.VendorType>() 
                {
                    new Domain.ColleagueFinance.Entities.VendorType("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.ColleagueFinance.Entities.VendorType("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.ColleagueFinance.Entities.VendorType("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.ColleagueFinance.Entities.VendorType("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.ColleagueFinance.Entities.VendorType("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypeEntities);
                foreach (var record in vendorTypeEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorTypesGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                vendorTermEntities = new List<Domain.ColleagueFinance.Entities.VendorTerm>() 
                {
                    new Domain.ColleagueFinance.Entities.VendorTerm("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTermEntities);

                foreach (var record in vendorTermEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorTermGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

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
                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", "db8f690b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);

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
            Mock<IReferenceDataRepository> refDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IAddressRepository> addressRepositoryMock;
            Mock<IVendorContactsRepository> vendorContactsRepositoryMock;
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
                refDataRepositoryMock = new Mock<IReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                addressRepositoryMock = new Mock<IAddressRepository>();
                vendorContactsRepositoryMock = new Mock<IVendorContactsRepository>();
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

                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
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
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_StartDateChange_ArgumentException()
            {
                vendorEntity.AddDate = DateTime.Today.AddDays(-1);
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ReturnsAsync(vendorEntity);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
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
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_Exception()
            {
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_CurrencyCodes_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetCurrencyConversionAsync())
                    .ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_CurrencyCode_NotSet_KeyNotFoundException()
            {
                vendorDto.DefaultCurrency = CurrencyIsoCode.NotSet;
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_Institution_Null_KeyNotFoundException()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_PersonCorpIndicator_Null_ArgumentException()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "" });
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_PersonCorpIndicator_Is_N_ArgumentException()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_Invalid_InstitutionId_ArgumentException()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("ZZZ", "LastName") { PersonCorpIndicator = "N" });
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_Organization_Null_KeyNotFoundException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(() => null);

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_OrganizationId_Invalid_KeyNotFoundException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_PersonCorpIndicator_Y_ArgumentException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("ABCD", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_VendorDetail_Person_Null_KeyNotFoundException()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = null;
                vendorDto.VendorDetail.Person = new GuidObject2("ABCD");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(() => null);

                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
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
            [ExpectedException(typeof(ColleagueWebApiException))]
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
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_AccountsPayableSources_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_AccountsPayableSource_Null_KeyNotFoundException()
            {
                vendorDto.PaymentSources.First().Id = "";
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_VendorTerms_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_PaymentTerm_Null_KeyNotFoundException()
            {
                vendorDto.PaymentTerms = new List<GuidObject2>() { new GuidObject2("")};
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_VendorTypes_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_Classifications_Null_KeyNotFoundException()
            {
                vendorDto.Classifications = new List<GuidObject2>() { new GuidObject2("") };
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_PUT_VendorHoldReasons_Null_KeyNotFoundException()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
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
            [ExpectedException(typeof(ColleagueWebApiException))]
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
                foreach (var record in acctPaySourceEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetAccountsPayableSourceGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                vendorTypeEntities = new List<Domain.ColleagueFinance.Entities.VendorType>() 
                {
                    new Domain.ColleagueFinance.Entities.VendorType("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.ColleagueFinance.Entities.VendorType("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.ColleagueFinance.Entities.VendorType("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.ColleagueFinance.Entities.VendorType("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.ColleagueFinance.Entities.VendorType("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypeEntities);
                foreach (var record in vendorTypeEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorTypesGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                vendorTermEntities = new List<Domain.ColleagueFinance.Entities.VendorTerm>() 
                {
                    new Domain.ColleagueFinance.Entities.VendorTerm("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTermEntities);
                foreach (var record in vendorTermEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorTermGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

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
                foreach (var record in vendorHoldReasons)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorHoldReasonsGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "Y" });
                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("1", "db8f690b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);

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
            Mock<IReferenceDataRepository> refDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IAddressRepository> addressRepositoryMock;
            Mock<IVendorContactsRepository> vendorContactsRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IInstitutionRepository> institutionRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            VendorsService vendorService;
            IEnumerable<Domain.ColleagueFinance.Entities.Vendors> vendorEntities;
            IEnumerable<Domain.ColleagueFinance.Entities.VendorHoldReasons> VendorHoldReasonsEntities;
            Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int> vendorEntityTuple;

            IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> vendorTermEntities;
            IEnumerable<Domain.Base.Entities.TaxForms2> taxFormEntities;
            IEnumerable<BoxCodes> taxBoxEntities;
            IEnumerable<IntgVendorAddressUsages> addressUsages;
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
                refDataRepositoryMock = new Mock<IReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                addressRepositoryMock = new Mock<IAddressRepository>();
                vendorContactsRepositoryMock = new Mock<IVendorContactsRepository>();
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

                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                vendorEntityTuple = null;
                vendorEntities = null;
                vendorTermEntities = null;
                taxFormEntities = null;
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
                        vendorService.GetVendorsAsync2(offset, limit, "", null, null, null, null, null, It.IsAny<bool>());

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
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), "", null, null, null, null, null)).ReturnsAsync(vendorEntityTuple);
                var actualsTuple = await vendorService.GetVendorsAsync2(offset, limit, "", null, null, null, null, null,It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_VenderFilter2()
            {
                //For some reason need to reset repo's and service to truly run the tests
                string VendorGuid = "VenderGUID123", VenderID = "VenderID123";
                
                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null, null)).ReturnsAsync(vendorEntityTuple);
                personRepositoryMock = null;
                personRepositoryMock = new Mock<IPersonRepository>();
                addressRepositoryMock = new Mock<IAddressRepository>();
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(VenderID);
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(new Guid().ToString());
                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("0000231", "db8f690b-071f-4d98-8da8-d4312511a4c2");
                personGuidCollection.Add("0000232", "db8f690b-071f-4d98-8da8-d4312511a4c3");
                personGuidCollection.Add("0000233", "db8f690b-071f-4d98-8da8-d4312511a4c4");
                personGuidCollection.Add("0000234", "db8f690b-071f-4d98-8da8-d4312511a4c5");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
                                              baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, VendorGuid, null, null, null, null, null,It.IsAny<bool>());

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
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null, null)).ReturnsAsync(vendorEntityTuple);
                personRepositoryMock = null;
                personRepositoryMock = new Mock<IPersonRepository>();
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, "VenderGUID123", null, null, null, null, null, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_VenderFilter_PersonGuidLookupException()
            {
                //For some reason need to reset repo's and service to truly run the tests
                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null, null)).ReturnsAsync(vendorEntityTuple);
                personRepositoryMock = null;
                personRepositoryMock = new Mock<IPersonRepository>();
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, "VenderGUID123", null, null, null, null, null, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }


            [TestMethod]
            public async Task Vendors_GETAllAsync_ClassificationFilter2()
            {
                var classficationGuid =  new List<string>() { "d4ff9cf9-3300-4dca-b52e-59c905021893" };

                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), "", It.IsAny<List<string>>(), null, null, null, null)).ReturnsAsync(vendorEntityTuple);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                   await
                       vendorService.GetVendorsAsync2(offset, limit, "", classficationGuid, null, null, null, null, It.IsAny<bool>());

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
            public async Task Vendors_GETAllAsync_relatedReferences_PaymentVendor()
            {
               
                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), "", It.IsAny<List<string>>(), null, null, null, null)).ReturnsAsync(vendorEntityTuple);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                   await
                       vendorService.GetVendorsAsync2(offset, limit, "", null, null, new List<string>(){ "paymentvendor"} , null, null, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);
                Assert.AreEqual(actualsTuple.Item2, 0);
                Assert.AreEqual(actualsTuple.Item1.Count(), 0);
            }



            [TestMethod]
            public async Task Vendors_GETAllAsync_ClassificationFilter_ClassificationMissing2()
            {
                var classficationGuid = new List<string> { "BadGuid" };

                vendorRepositoryMock = null;
                vendorRepositoryMock = new Mock<IVendorsRepository>();
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), "", It.IsAny<List<string>>(), null, null, It.IsAny<List<string>>(), null)).ReturnsAsync(vendorEntityTuple);

                vendorService = null;
                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
                                               baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                var actualsTuple =
                   await
                       vendorService.GetVendorsAsync2(offset, limit, "", classficationGuid, null, null, null, null, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Vendors_GETAllAsync_StatusFilter2()
            {
                var status =  "active" ;
                var statuses = new List<string>();
                statuses.Add(status);
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), "", null, statuses, null, It.IsAny<List<string>>(), null)).ReturnsAsync(vendorEntityTuple);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, "", null, statuses, null, null, null, It.IsAny<bool>());

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
            public async Task Vendors_GETAllAsync_StatusFilter2_Invalid()
            {
                var status = "actively";
                var statuses = new List<string>();
                statuses.Add(status);
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), "", null, statuses, null, It.IsAny<List<string>>(), null)).ReturnsAsync(vendorEntityTuple);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, "", null, statuses, null, null, null, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                Assert.IsNotNull(actualsTuple);
                Assert.AreEqual(actualsTuple.Item2, 0);
                Assert.AreEqual(actualsTuple.Item1.Count(), 0);
            }

            [TestMethod]
            
            public async Task Vendors_GETAllAsync_Types_Invalid()
            {
                var type = "actively";
                var types = new List<string>();
                types.Add(type);
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), "", null, types, null, It.IsAny<List<string>>(), null)).ReturnsAsync(vendorEntityTuple);

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, "", null, null, null, types,  null, It.IsAny<bool>());
                Assert.IsNotNull(actualsTuple);

                Assert.IsNotNull(actualsTuple);
                Assert.AreEqual(actualsTuple.Item2, 0);
                Assert.AreEqual(actualsTuple.Item1.Count(), 0);
            }


            [TestMethod]
            public async Task Vendors_GET_ById2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                var actual = await vendorService.GetVendorsByGuidAsync2(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }

            [TestMethod]
            public async Task Vendors_GET_ById2_Institution()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                institutionsEntities = new List<Domain.Base.Entities.Institution>()
                {
                    new Domain.Base.Entities.Institution("0000231", Domain.Base.Entities.InstType.College),
                };
                institutionRepositoryMock.Setup(i => i.GetInstitutionsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(institutionsEntities);
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                var actual = await vendorService.GetVendorsByGuidAsync2(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }


            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task VendorService_GET_ById2_PermissionException()
            //{
            //    personRole.RemovePermission(permissionViewAnyPerson); //Removing the VIEW.VOUCHER Permission
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });
            //    var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            //    var expected = vendorEntities.ToList()[0];
            //    vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
            //    var actual = await vendorService.GetVendorsByGuidAsync2(id);
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Vendors_GET_ById2_noGUID()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(new Domain.ColleagueFinance.Entities.Vendors(string.Empty));
                var actual = await vendorService.GetVendorsByGuidAsync2(id);                
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_Invalid_VendorHoldReasons()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(loc => loc.GetVendorHoldReasonsGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_Invalid_TaxForm()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                expected.TaxForm = "Form546";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            public async Task Vendors_GET_ById_NotDefault_TaxBox()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                expected.TaxForm = "Form128";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(actual.DefaultTaxFormComponent, null); ; ;
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_Invalid_AccountsPayableSource()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(loc => loc.GetAccountsPayableSourceGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_Invalid_VendorTerm()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(loc => loc.GetVendorTermGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_Invalid_VendorTypes()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(loc => loc.GetVendorTypesGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);               
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_Invalid_IntgVendorAddressUsages()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(loc => loc.GetIntgVendorAddressUsagesGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_Invalid_AddressId()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                var addressGuidCollection = new Dictionary<string, string>();
                addressGuidCollection.Add("address3", "db8f690b-071f-4d98-8da8-d4312591a4c2");
                personRepositoryMock.Setup(p => p.GetAddressGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(addressGuidCollection);
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_Invalid_PersonGuidCollection_null()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                var personGuidCollection = new Dictionary<string, string>();
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(() => null);
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_Invalid_PersonGuidCollection_empty()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = vendorEntities.ToList()[0];
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).ReturnsAsync(expected);
                var personGuidCollection = new Dictionary<string, string>();
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
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
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).Throws<KeyNotFoundException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_IntegrationApiException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).Throws<IntegrationApiException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_GET_RepoException_IntegrationApiException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), "", null, null, null, It.IsAny<List<string>>(), null)).Throws<RepositoryException>();

                var actualsTuple =
                    await
                        vendorService.GetVendorsAsync2(offset, limit, "", null, null, null, null, null, It.IsAny<bool>());
            }


            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_InvalidOperationException2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).Throws<InvalidOperationException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_RepositoryException2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).Throws<RepositoryException>();
                var actual = await vendorService.GetVendorsByGuidAsync2(id);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task Vendors_GET_ById_ReturnsNullEntity_Exception2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(id)).Throws<Exception>();
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
                foreach (var record in acctPaySourceEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetAccountsPayableSourceGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                vendorTypeEntities = new List<Domain.ColleagueFinance.Entities.VendorType>()
                {
                    new Domain.ColleagueFinance.Entities.VendorType("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.ColleagueFinance.Entities.VendorType("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.ColleagueFinance.Entities.VendorType("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.ColleagueFinance.Entities.VendorType("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.ColleagueFinance.Entities.VendorType("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypeEntities);
                foreach (var record in vendorTypeEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorTypesGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                vendorTermEntities = new List<Domain.ColleagueFinance.Entities.VendorTerm>()
                {
                    new Domain.ColleagueFinance.Entities.VendorTerm("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTermEntities);
                foreach (var record in vendorTermEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorTermGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                taxFormEntities = new List<TaxForms2>()
                {
                    new TaxForms2("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "Form123", "Form123", "box123" ),
                    new TaxForms2("874dee09-8662-47e6-af0d-504c257493a3", "Form124", "Form124","box124"),
                    new TaxForms2("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "Form123", "Form125","box125"),
                    new TaxForms2("5b05410c-c94c-464a-98ee-684198bde60b", "Form128", "Form126","box127"),
                };
                refDataRepositoryMock.Setup(i => i.GetTaxFormsBaseAsync(It.IsAny<bool>())).ReturnsAsync(taxFormEntities);
                foreach (var record in taxFormEntities)
                {
                    refDataRepositoryMock.Setup(loc => loc.GetTaxFormsGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                taxBoxEntities = new List<BoxCodes>()
                {
                    new BoxCodes("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "box123", "box123",  "Form123"),
                    new BoxCodes("874dee09-8662-47e6-af0d-504c257493a3", "box124", "box124", "Form124"),
                    new BoxCodes("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "box125", "box125", "Form125"),
                    new BoxCodes("5b05410c-c94c-464a-98ee-684198bde60b", "box126", "box126", "Form126"),
                };
                refDataRepositoryMock.Setup(i => i.GetAllBoxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxBoxEntities);
                foreach (var record in taxBoxEntities)
                {
                    refDataRepositoryMock.Setup(loc => loc.GetBoxCodesGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                addressUsages = new List<IntgVendorAddressUsages>()
                {
                    new IntgVendorAddressUsages("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "PO", "Purchase Order Address"),
                    new IntgVendorAddressUsages("874dee09-8662-47e6-af0d-504c257493a3",  "CHECK", "AP Check Address"),
                    
                };
                referenceDataRepositoryMock.Setup(i => i.GetIntgVendorAddressUsagesAsync(It.IsAny<bool>())).ReturnsAsync(addressUsages);
                foreach (var record in addressUsages)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetIntgVendorAddressUsagesGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                VendorHoldReasonsEntities = new List<Domain.ColleagueFinance.Entities.VendorHoldReasons>()
                {
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ReturnsAsync(VendorHoldReasonsEntities);
                foreach (var record in VendorHoldReasonsEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorHoldReasonsGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

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
                        CurrencyCode = "USD",
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
                        IntgHoldReasons = new List<string>(){"AS" },
                        Categories = new List<string>() { "EP", "TR", "PR" },
                        CorpParent =  new List<string>(){"0000231" },
                        TaxId = "EIN123",
                        TaxForm = "Form123"
                        
                    },
                    new Domain.ColleagueFinance.Entities.Vendors("5bc2d86c-6a0c-46b1-824d-485ccb27dc67"){IsOrganization = false, Id = "0000232"},
                    new Domain.ColleagueFinance.Entities.Vendors("7ea5142f-12f1-4ac9-b9f3-73e4205dfc11"){Id = "0000233"},
                    new Domain.ColleagueFinance.Entities.Vendors("db8f690b-071f-4d98-8da8-d4312511a4c1"){Id = "0000234"}
                };
                vendorEntityTuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int>(vendorEntities, vendorEntities.Count());
                vendorRepositoryMock.Setup(i => i.GetVendors2Async(It.IsAny<int>(), It.IsAny<int>(), "", null, null, null, It.IsAny<List<string>>(), null)).ReturnsAsync(vendorEntityTuple);
                vendorRepositoryMock.Setup(i => i.GetVendorsByGuid2Async(It.IsAny<string>())).ReturnsAsync(vendorEntities.ToList()[0]);
                vendorRepositoryMock.Setup(i => i.GetVendorGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("123");
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("0000231", "db8f690b-071f-4d98-8da8-d4312511a4c2");
                personGuidCollection.Add("0000232", "db8f690b-071f-4d98-8da8-d4312511a4c3");
                personGuidCollection.Add("0000233", "db8f690b-071f-4d98-8da8-d4312511a4c4");
                personGuidCollection.Add("0000234", "db8f690b-071f-4d98-8da8-d4312511a4c5");
                var personAddressCollection = new Dictionary<string, string>();
                personAddressCollection.Add("0000231", "address1");
                personAddressCollection.Add("0000232", "address2");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);
                personRepositoryMock.Setup(p => p.GetHierarchyAddressIdsAsync(It.IsAny<List<string>>(), It.IsAny<string>(), DateTime.Today)).ReturnsAsync(personAddressCollection);
                var addressGuidCollection = new Dictionary<string, string>();
                addressGuidCollection.Add( "address1", "db8f690b-071f-4d98-8da8-d4312591a4c2");
                addressGuidCollection.Add( "address2", "db8f698b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(p => p.GetAddressGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(addressGuidCollection);

                
            }
        }

        [TestClass]
        public class VendorServiceTests_PUT_POST2 : CurrentUserSetup
        {
            Mock<IVendorsRepository> vendorRepositoryMock;
            Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IReferenceDataRepository> refDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IAddressRepository> addressRepositoryMock;
            Mock<IVendorContactsRepository> vendorContactsRepositoryMock;
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
            IEnumerable<Domain.Base.Entities.TaxForms2> taxFormEntities;
            IEnumerable<BoxCodes> taxBoxEntities;
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
                refDataRepositoryMock = new Mock<IReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                addressRepositoryMock = new Mock<IAddressRepository>();
                vendorContactsRepositoryMock = new Mock<IVendorContactsRepository>();
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

                vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
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
            public async Task Vendors_POST2_VendorDetail_Person()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "" });
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2("5bc2d86c-6a0c-46b1-824d-485ccb27dc67") };
                vendorDto.TaxId = null;
                institutionRepositoryMock.Setup(i => i.GetInstitutionsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(new List<Domain.Base.Entities.Institution>());
                var result = await vendorService.PostVendorAsync2(vendorDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(vendorDto.Id, result.Id);
            }


            [TestMethod]
            public async Task Vendors_POST2_VendorDetail_Person_Inst()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "" });
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2("5bc2d86c-6a0c-46b1-824d-485ccb27dc67") };
                vendorDto.TaxId = null;
                institutionRepositoryMock.Setup(i => i.GetInstitutionsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(new List<Domain.Base.Entities.Institution>());
                var result = await vendorService.PostVendorAsync2(vendorDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(vendorDto.Id, result.Id);
            }

            //[TestMethod]
            //[ExpectedException(typeof(IntegrationApiException))]
            //public async Task Vendors_POST2_Permission()
            //{
            //    personRole.RemovePermission(permissionViewAnyPerson); //Removing the VIEW.VOUCHER Permission
            //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });
            //    var result = await vendorService.PostVendorAsync2(vendorDto);
                
            //}


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_DtoNull_ArgumentNullException2()
            {
                var result = await vendorService.PutVendorAsync2(vendorGuid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_DtoIdNull_ArgumentNullException2()
            {
                vendorDto.Id = "";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }
            
            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_CurrencyCodes_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetCurrencyConversionAsync())
                    .ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_CurrencyCode_NotSet_KeyNotFoundException2()
            {
                vendorDto.DefaultCurrency = CurrencyIsoCode.NotSet;
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_Institution_Null_KeyNotFoundException2()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_Institution_Null_Record()
            {
                institutionRepositoryMock.Setup(i => i.GetInstitutionsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_PersonCorpIndicator_Null_ArgumentException2()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "" });
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_Person_PersonCorpIndicator_()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "Y" });
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2("5bc2d86c-6a0c-46b1-824d-485ccb27dc67") };
                vendorDto.TaxId = null;
                institutionRepositoryMock.Setup(i => i.GetInstitutionsFromListAsync(It.IsAny<string[]>())).ReturnsAsync(institutionsEntities);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_Person_Person_taxId()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "Y" });
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2("5bc2d86c-6a0c-46b1-824d-485ccb27dc67") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_PersonCorpIndicator_Is_N_ArgumentException2()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_EndOn()
            {
                vendorDto.EndOn = DateTime.Today;
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Null()
            {
                vendorDto.VendorDetail = null;
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Empty()
            {
                vendorDto.VendorDetail = new VendorDetailsDtoProperty();
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_All()
            {
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Institution = new GuidObject2("1"), Organization = new GuidObject2("2"), Person = new GuidObject2("3") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Per_Org()
            {
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Organization = new GuidObject2("2"), Person = new GuidObject2("3") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Per_Inst()
            {
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Institution = new GuidObject2("1"), Person = new GuidObject2("3") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Org_Inst()
            {
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Institution = new GuidObject2("1"), Organization = new GuidObject2("2") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Org_Per()
            {
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Organization = new GuidObject2("2"), Person = new GuidObject2("3") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Org_Null_id()
            {
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Organization = new GuidObject2("") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Person_Null_id()
            {
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2("") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Inst_Null_id()
            {
                vendorDto.VendorDetail = new VendorDetailsDtoProperty() { Institution = new GuidObject2("") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }



            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_Invalid_InstitutionId_ArgumentException2()
            {
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                   .ReturnsAsync(new Domain.Base.Entities.Person("ZZZ", "LastName") { PersonCorpIndicator = "N" });
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }



            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_Organization_Null_KeyNotFoundException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(() => null);

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_OrganizationId_Invalid_KeyNotFoundException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_PersonCorpIndicator_Y_ArgumentException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = new GuidObject2("ABC");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("ABCD", "LastName") { PersonCorpIndicator = "N" });

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorDetail_Person_Null_KeyNotFoundException2()
            {
                vendorDto.VendorDetail.Institution = null;
                vendorDto.VendorDetail.Organization = null;
                vendorDto.VendorDetail.Person = new GuidObject2("ABCD");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(() => null);

                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
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
            [ExpectedException(typeof(IntegrationApiException))]
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
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_AccountsPayableSources_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_AccountsPayableSource_Null_KeyNotFoundException2()
            {
                vendorDto.PaymentSources.First().Id = "";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_AccountsPayableSource_Invalid()
            {
                vendorDto.PaymentSources.First().Id = "123";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_PaymentTerms_Invalid()
            {
                vendorDto.PaymentTerms.First().Id = "123";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_Classifications_Invalid()
            {
                vendorDto.Classifications.First().Id = "123";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorHoldReasons_Invalid()
            {
                vendorDto.VendorHoldReasons.First().Id = "123";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_DefaultTaxFormComponent_Invalid()
            {
                vendorDto.DefaultTaxFormComponent.Id = "123";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_DefaultTaxFormComponent_No_Default()
            {
                vendorDto.DefaultTaxFormComponent.Id = "5b05410c-c94c-464a-98ee-684198bde60b";
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_BoxCodes_Null()
            {
                refDataRepositoryMock.Setup(i => i.GetAllBoxCodesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_TaxForms_Null()
            {
                refDataRepositoryMock.Setup(i => i.GetTaxFormsBaseAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorTerms_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_PaymentTerm_Null_KeyNotFoundException2()
            {
                vendorDto.PaymentTerms = new List<GuidObject2>() { new GuidObject2("") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorTypes_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_RepoExceptopn()
            {
                vendorRepositoryMock.Setup(i => i.UpdateVendors2Async(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>())).ThrowsAsync(new RepositoryException());
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_Exceptopn()
            {
                vendorRepositoryMock.Setup(i => i.UpdateVendors2Async(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>())).ThrowsAsync(new Exception());
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_Classifications_Null_KeyNotFoundException2()
            {
                vendorDto.Classifications = new List<GuidObject2>() { new GuidObject2("") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorHoldReasons_Null_KeyNotFoundException2()
            {
                referenceDataRepositoryMock.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(() => null);
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_PUT_VendorHoldReason_Null_KeyNotFoundException2()
            {
                vendorDto.VendorHoldReasons = new List<GuidObject2>() { new GuidObject2("") };
                var result = await vendorService.PutVendorAsync2(vendorGuid, vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_POST_VendorNull_ArgumentNullException2()
            {
                var result = await vendorService.PostVendorAsync2(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_POST_VendorIdNull_ArgumentNullException2()
            {
                vendorDto.Id = null;
                var result = await vendorService.PostVendorAsync2(vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_POST_RepositoryException2()
            {
                vendorRepositoryMock.Setup(i => i.CreateVendors2Async(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>()))
                    .ThrowsAsync(new RepositoryException());
                var result = await vendorService.PostVendorAsync2(vendorDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task Vendors_POST_Exception2()
            {
                vendorRepositoryMock.Setup(i => i.CreateVendors2Async(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>()))
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
                        Dtos.EnumProperties.VendorsStatuses.Holdpayment,
                        Dtos.EnumProperties.VendorsStatuses.Active,
                        Dtos.EnumProperties.VendorsStatuses.Approved
                    },
                    Types = new List<VendorTypes>() {  VendorTypes.Travel, VendorTypes.EProcurement, VendorTypes.Procurement},
                    TaxId = "tax123",
                    DefaultTaxFormComponent =  new GuidObject2("c1b91008-ba77-4b5b-8b77-84f5a7ae1632")
                   
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
                foreach (var record in acctPaySourceEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetAccountsPayableSourceGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                vendorTypeEntities = new List<Domain.ColleagueFinance.Entities.VendorType>()
                {
                    new Domain.ColleagueFinance.Entities.VendorType("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.ColleagueFinance.Entities.VendorType("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.ColleagueFinance.Entities.VendorType("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.ColleagueFinance.Entities.VendorType("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.ColleagueFinance.Entities.VendorType("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTypesAsync(It.IsAny<bool>())).ReturnsAsync(vendorTypeEntities);
                foreach (var record in vendorTypeEntities)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorTypesGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                vendorTermEntities = new List<Domain.ColleagueFinance.Entities.VendorTerm>()
                {
                    new Domain.ColleagueFinance.Entities.VendorTerm("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorTermsAsync(It.IsAny<bool>())).ReturnsAsync(vendorTermEntities);

                taxFormEntities = new List<TaxForms2>()
                {
                    new TaxForms2("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "Form123", "Form123", "box123" ),
                    new TaxForms2("874dee09-8662-47e6-af0d-504c257493a3", "Form124", "Form124","box124"),
                    new TaxForms2("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "Form125", "Form125","box125"),
                    new TaxForms2("5b05410c-c94c-464a-98ee-684198bde60b", "Form126", "Form126",""),
                };
                refDataRepositoryMock.Setup(i => i.GetTaxFormsBaseAsync(It.IsAny<bool>())).ReturnsAsync(taxFormEntities);
                foreach (var record in taxFormEntities)
                {
                    refDataRepositoryMock.Setup(loc => loc.GetTaxFormsGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                taxBoxEntities = new List<BoxCodes>()
                {
                    new BoxCodes("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "box123", "box123",  "Form123"),
                    new BoxCodes("874dee09-8662-47e6-af0d-504c257493a3", "box124", "box124", "Form124"),
                    new BoxCodes("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "box125", "box125", "Form125"),
                    new BoxCodes("5b05410c-c94c-464a-98ee-684198bde60b", "box126", "box126", "Form126"),
                };
                refDataRepositoryMock.Setup(i => i.GetAllBoxCodesAsync(It.IsAny<bool>())).ReturnsAsync(taxBoxEntities);
                foreach (var record in taxFormEntities)
                {
                    refDataRepositoryMock.Setup(loc => loc.GetBoxCodesGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

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
                vendorRepositoryMock.Setup(i => i.UpdateVendors2Async(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>())).ReturnsAsync(vendorEntity);
                vendorRepositoryMock.Setup(i => i.CreateVendors2Async(It.IsAny<Domain.ColleagueFinance.Entities.Vendors>())).ReturnsAsync(vendorEntity);

                List<Domain.ColleagueFinance.Entities.VendorHoldReasons> vendorHoldReasons = new List<Domain.ColleagueFinance.Entities.VendorHoldReasons>()
                {
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("aa06e48b-cfb9-4341-83f8-4dc4b2d0fad1", "OB", "Out of Business"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("f8c04467-9b72-46f8-ba1c-ba6aeb0bb6a6", "DISC", "Vendor Discontinued"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("c8263488-bf7d-45a7-9190-39b9587561a1", "QUAL", "Quality Hold"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("f9865084-2c02-484e-8286-b98afa5909cc", "DISP", "Disputed Transaction desc"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetVendorHoldReasonsAsync(It.IsAny<bool>())).ReturnsAsync(vendorHoldReasons);
                foreach (var record in vendorHoldReasons)
                {
                    referenceDataRepositoryMock.Setup(loc => loc.GetVendorHoldReasonsGuidAsync(record.Code)).ReturnsAsync(record.Guid);
                }

                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(i => i.GetPersonByGuidNonCachedAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.Person("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "LastName") { PersonCorpIndicator = "Y" });
                var personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("0000231", "db8f690b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);

                var personAddressCollection = new Dictionary<string, string>();
                personAddressCollection.Add("0000231", "address1");
                personAddressCollection.Add("0000232", "address2");
                personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(personGuidCollection);
                personRepositoryMock.Setup(p => p.GetHierarchyAddressIdsAsync(It.IsAny<List<string>>(), It.IsAny<string>(), DateTime.Today)).ReturnsAsync(personAddressCollection);
                var addressGuidCollection = new Dictionary<string, string>();
                addressGuidCollection.Add("address1", "db8f690b-071f-4d98-8da8-d4312591a4c2");
                addressGuidCollection.Add("address2", "db8f698b-071f-4d98-8da8-d4312511a4c2");
                personRepositoryMock.Setup(p => p.GetAddressGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(addressGuidCollection);

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

    #region vendors-maximum

    [TestClass]
    public class VendorsMaximumServiceTests_GET: CurrentUserSetup
    {
        //Mock<IPositionRepository> positionRepositoryMock;
        Mock<IVendorsRepository> vendorRepositoryMock;
        Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IReferenceDataRepository> refDataRepositoryMock;
        Mock<IPersonRepository> personRepositoryMock;
        Mock<IAddressRepository> addressRepositoryMock;
        Mock<IVendorContactsRepository> vendorContactsRepositoryMock;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<IInstitutionRepository> institutionRepositoryMock;
        ICurrentUserFactory currentUserFactory;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;

        VendorsService vendorService;
        IEnumerable<Domain.ColleagueFinance.Entities.Vendors> vendorEntities;
        IEnumerable<Domain.ColleagueFinance.Entities.VendorHoldReasons> VendorHoldReasonsEntities;
        Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int> vendorEntityTuple;

        IEnumerable<Domain.ColleagueFinance.Entities.VendorTerm> vendorTermEntities;
        IEnumerable<Domain.Base.Entities.TaxForms2> taxFormEntities;
        IEnumerable<BoxCodes> taxBoxEntities;
        IEnumerable<IntgVendorAddressUsages> addressUsages;
        IEnumerable<Domain.ColleagueFinance.Entities.VendorType> vendorTypeEntities;
        IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> acctPaySourceEntities;
        IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionEntities;
        IEnumerable<Domain.Base.Entities.AddressType2> addressTypes;
        IEnumerable<State> states;
        IEnumerable<Domain.Base.Entities.Country> countries;
        IEnumerable<Domain.Base.Entities.PhoneType> phoneTypes;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private Domain.Entities.Permission permissionViewAnyPerson;
        string guid = "ab8f690b-071f-4d98-8da8-d4312511a4c1";

        int offset = 0;
        int limit = 4;

        [TestInitialize]
        public void Initialize()
        {
            //positionRepositoryMock = new Mock<IPositionRepository>();
            vendorRepositoryMock = new Mock<IVendorsRepository>();
            referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            refDataRepositoryMock = new Mock<IReferenceDataRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            addressRepositoryMock = new Mock<IAddressRepository>();
            vendorContactsRepositoryMock = new Mock<IVendorContactsRepository>();
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
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission( ColleagueFinancePermissionCodes.ViewVendors );
            personRole.AddPermission( permissionViewAnyPerson );
            roleRepositoryMock.Setup( rpm => rpm.Roles ).Returns( new List<Domain.Entities.Role>() { personRole } );

            vendorService = new VendorsService( referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object, institutionRepositoryMock.Object,
                                           baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object );
        }

        [TestCleanup]
        public void Cleanup()
        {
            vendorEntityTuple = null;
            vendorEntities = null;
            vendorTermEntities = null;
            taxFormEntities = null;
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

        //[TestMethod]
        //[ExpectedException(typeof(IntegrationApiException))]
        //public async Task Vendors_GetVendorsMaximumAsync()
        //{
        //    personRole.RemovePermission( permissionViewAnyPerson ); //Removing the VIEW.VOUCHER Permission
        //    roleRepositoryMock.Setup( rpm => rpm.Roles ).Returns( new List<Domain.Entities.Role>() { personRole } );
        //    var actualsTuple =
        //        await
        //            vendorService.GetVendorsMaximumAsync( offset, limit, null, null, It.IsAny<bool>() );
        //}

        [TestMethod]
        public async Task GetVendorsMaximumAsync_VendorDetails_GetPersonIdFromGuidAsync_EmptySet()
        {
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ThrowsAsync( new Exception() );
            var actualsTuple =
                await
                    vendorService.GetVendorsMaximumAsync( offset, limit, null, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_VendorDetails_GetPersonIdFromGuidAsync_Returns_Null_EmptySet()
        {
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync(() => null);
            var actualsTuple =
                await
                    vendorService.GetVendorsMaximumAsync( offset, limit, null, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_GetPersonByGuidNonCachedAsync_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2("GUID") } };
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ThrowsAsync( new Exception() );
            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, It.IsAny<string>(), It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_GetPersonByGuidNonCachedAsync_Returns_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync(() => null);
            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, It.IsAny<string>(), It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_GetInstitutionsFromListAsync_Returns_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync( new List<Institution>() { new Institution() } );
            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, It.IsAny<string>(), It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_PersonCorpIndicator_Y__EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "Y" } );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync(() => null);

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, It.IsAny<string>(), It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_VendorId_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Person = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync(() => null);

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Organization_Exception_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Organization = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ThrowsAsync( new Exception() );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Organization_Person_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Organization = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync(() => null);

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Organization_Institution_Not_Empty_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Organization = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync( new List<Institution>() { new Institution() } );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Organization_PersonCorpIndicator_Not_Y_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Organization = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "N"} );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync( new List<Institution>() );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Organization_PersonCorpIndicator_Equal_Y_VendorId_Not_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Organization = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "Y" } );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync( new List<Institution>() );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Institution_Exception_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Institution = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ThrowsAsync( new Exception() );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Institution_Person_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Institution = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync(() => null);

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Institution_PersonCorpIndicator_N_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Institution = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "N" } );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Institution_Institution_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Institution = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "Y" } );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Institution_VendorId_Not_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { VendorDetail = new VendorDetailsDtoProperty() { Institution = new GuidObject2( "GUID" ) } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "Y" } );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync( new List<Institution>() { new Institution() } );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Statuses_Value_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { Statuses = new List<VendorsStatuses?>() { null, null  } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "Y" } );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync( new List<Institution>() { new Institution() } );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Addresses_DetailId_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { Addresses = new List<VendorsMaximumAddresses>() { new VendorsMaximumAddresses() { Detail = new GuidObject2("") } } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "Y" } );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync( new List<Institution>() { new Institution() } );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Addresses_addressId_Null_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { Addresses = new List<VendorsMaximumAddresses>() { new VendorsMaximumAddresses() { Detail = new GuidObject2( "GUID" ) } } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "Y" } );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync( new List<Institution>() { new Institution() } );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync_CiteriaObj_Addresses_Exception_EmptySet()
        {
            VendorsMaximum maximum = new VendorsMaximum() { Addresses = new List<VendorsMaximumAddresses>() { new VendorsMaximumAddresses() { Detail = new GuidObject2( "GUID" ) } } };
            personRepositoryMock.Setup( repo => repo.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
            personRepositoryMock.Setup( repo => repo.GetPersonByGuidNonCachedAsync( It.IsAny<string>() ) ).ReturnsAsync( new Person( "1", "Last_Name" ) { PersonCorpIndicator = "Y" } );
            institutionRepositoryMock.Setup( repo => repo.GetInstitutionsFromListAsync( It.IsAny<string[]>() ) ).ReturnsAsync( new List<Institution>() { new Institution() } );
            addressRepositoryMock.Setup( repo => repo.GetAddressFromGuidAsync( It.IsAny<string>() ) ).ThrowsAsync( new Exception() );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, maximum, "vendorDetails", It.IsAny<bool>() );

            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( 0, actualsTuple.Item2 );
        }

        [TestMethod]
        [ExpectedException(typeof( IntegrationApiException ) )]
        public async Task GetVendorsMaximumAsync_RepositoryException_IntegrationApiException_Thrown()
        {
            vendorRepositoryMock.Setup( repo => repo.GetVendorsMaximumAsync( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                 It.IsAny<string>(), It.IsAny<List<string>>() ) ).ThrowsAsync( new RepositoryException() );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, null, "", It.IsAny<bool>() );
        }

        [TestMethod]
        [ExpectedException( typeof( IntegrationApiException ) )]
        public async Task GetVendorsMaximumAsync_IntegrationApiException()
        {
            var vendors = new List<Domain.ColleagueFinance.Entities.Vendors>()
            {
                new Domain.ColleagueFinance.Entities.Vendors( guid )
                {
                    Id = "1",                    
                }
            };
            var tuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int>( vendors, 1 );
            vendorRepositoryMock.Setup( repo => repo.GetVendorsMaximumAsync( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                 It.IsAny<string>(), It.IsAny<List<string>>() ) ).ReturnsAsync( tuple );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, It.IsAny<VendorsMaximum>(), It.IsAny<string>(), It.IsAny<bool>() );
        }

        [TestMethod]
        public async Task GetVendorsMaximumAsync()
        {
            var vendors = new List<Domain.ColleagueFinance.Entities.Vendors>()
            {
                new Domain.ColleagueFinance.Entities.Vendors( guid )
                {
                    Id = "1",
                }
            };
            var tuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int>( vendors, 1 );
            vendorRepositoryMock.Setup( repo => repo.GetVendorsMaximumAsync( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                 It.IsAny<string>(), It.IsAny<List<string>>() ) ).ReturnsAsync( tuple );
            Dictionary<string, string> personDict = new Dictionary<string, string>();
            personDict.Add( "1", guid );
            personRepositoryMock.Setup( repo => repo.GetPersonGuidsCollectionAsync( It.IsAny<string[]>() ) ).ReturnsAsync( personDict );

            var actualsTuple = await vendorService.GetVendorsMaximumAsync( offset, limit, It.IsAny<VendorsMaximum>(), It.IsAny<string>(), It.IsAny<bool>() );
            Assert.IsNotNull( actualsTuple );
            Assert.AreEqual( tuple.Item2, actualsTuple.Item2 );
        }

        [TestMethod]
        public async Task GetVendorsMaximumByGuidAsync()
        {
            var vendor = new Domain.ColleagueFinance.Entities.Vendors( guid ) { Id = "1", };
            vendorRepositoryMock.Setup( repo => repo.GetVendorsMaximumByGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( vendor );
            Dictionary<string, string> personDict = new Dictionary<string, string>();
            personDict.Add( "1", guid );
            personRepositoryMock.Setup( repo => repo.GetPersonGuidsCollectionAsync( It.IsAny<string[]>() ) ).ReturnsAsync( personDict );


            var actual = await vendorService.GetVendorsMaximumByGuidAsync( guid );
            Assert.IsNotNull( actual );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public async Task GetVendorsMaximumByGuidAsync_ArgumentNullException()
        {
            var actualsTuple = await vendorService.GetVendorsMaximumByGuidAsync( "" );
        }

        [TestMethod]
        [ExpectedException( typeof(ColleagueWebApiException) )]
        public async Task GetVendorsMaximumByGuidAsync_RepositoryException()
        {
            vendorContactsRepositoryMock.Setup( repo => repo.GetVendorContactsForVendorsAsync( It.IsAny<string[]>() ) ).ThrowsAsync( new RepositoryException() );
            var actualsTuple = await vendorService.GetVendorsMaximumByGuidAsync( "GUID" );
        }

        [TestMethod]
        [ExpectedException( typeof( IntegrationApiException ) )]
        public async Task GetVendorsMaximumByGuidAsync_IntegrationApiException()
        {
            var vendor = new Domain.ColleagueFinance.Entities.Vendors( guid ) { Id = "1" };
            vendorRepositoryMock.Setup( repo => repo.GetVendorsMaximumByGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( vendor );

            var actualsTuple = await vendorService.GetVendorsMaximumByGuidAsync( guid );
        }

        [TestMethod]
        [ExpectedException( typeof( IntegrationApiException ) )]
        public async Task GetVendorsMaximumByGuidAsync_IntegrationApiException_All()
        {
            var vendor = new Domain.ColleagueFinance.Entities.Vendors( Guid.NewGuid().ToString() ) 
            { 
                Id = "Bad_Id",
                TaxId = "1",
                Categories = new List<string>()
                {
                    "EP",
                    "TR",
                    "PR"
                },
                ActiveFlag = "Y",
                AddDate = DateTime.Now,
                TaxForm = "TF"
            };
            vendorRepositoryMock.Setup( repo => repo.GetVendorsMaximumByGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( vendor );

            var actualsTuple = await vendorService.GetVendorsMaximumByGuidAsync( guid );
        }

        [TestMethod]
        public async Task GetVendorsMaximumByGuidAsync_All()
        {
            var vendor = new Domain.ColleagueFinance.Entities.Vendors( Guid.NewGuid().ToString() )
            {
                Id = "0000231",
                TaxId = "1",
                Categories = new List<string>()
                {
                    "EP",
                    "TR",
                    "PR"
                },
                ActiveFlag = "Y",
                AddDate = DateTime.Now,
                TaxForm = "Form123",
                Addresses = new List<Domain.Base.Entities.Address>()
                {
                    new Domain.Base.Entities.Address()
                    {
                        Guid = "f43e7195-6eca-451d-b6c3-1e52fe540084",
                        TypeCode = "AT1",
                        AddressLines = new List<string>(){"Addr Line 1"},
                        City = "New York",
                        State = "MN",
                        PostalCode = "55430",
                        Country = "USA"                       
                    }
                },
                Phones = new List<Domain.Base.Entities.Phone>()
                {
                    new Domain.Base.Entities.Phone("8005551212", "Home")
                }
            };
            vendorRepositoryMock.Setup( repo => repo.GetVendorsMaximumByGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( vendor );

            var actualsTuple = await vendorService.GetVendorsMaximumByGuidAsync( guid );
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
            referenceDataRepositoryMock.Setup( i => i.GetAccountsPayableSourcesAsync( It.IsAny<bool>() ) ).ReturnsAsync( acctPaySourceEntities );
            foreach( var record in acctPaySourceEntities )
            {
                referenceDataRepositoryMock.Setup( loc => loc.GetAccountsPayableSourceGuidAsync( record.Code ) ).ReturnsAsync( record.Guid );
            }

            vendorTypeEntities = new List<Domain.ColleagueFinance.Entities.VendorType>()
                {
                    new Domain.ColleagueFinance.Entities.VendorType("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.ColleagueFinance.Entities.VendorType("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.ColleagueFinance.Entities.VendorType("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.ColleagueFinance.Entities.VendorType("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.ColleagueFinance.Entities.VendorType("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
            referenceDataRepositoryMock.Setup( i => i.GetVendorTypesAsync( It.IsAny<bool>() ) ).ReturnsAsync( vendorTypeEntities );
            foreach( var record in vendorTypeEntities )
            {
                referenceDataRepositoryMock.Setup( loc => loc.GetVendorTypesGuidAsync( record.Code ) ).ReturnsAsync( record.Guid );
            }

            vendorTermEntities = new List<Domain.ColleagueFinance.Entities.VendorTerm>()
                {
                    new Domain.ColleagueFinance.Entities.VendorTerm("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorTerm("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
            referenceDataRepositoryMock.Setup( i => i.GetVendorTermsAsync( It.IsAny<bool>() ) ).ReturnsAsync( vendorTermEntities );
            foreach( var record in vendorTermEntities )
            {
                referenceDataRepositoryMock.Setup( loc => loc.GetVendorTermGuidAsync( record.Code ) ).ReturnsAsync( record.Guid );
            }

            taxFormEntities = new List<TaxForms2>()
                {
                    new TaxForms2("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "Form123", "Form123", "box123" ),
                    new TaxForms2("874dee09-8662-47e6-af0d-504c257493a3", "Form124", "Form124","box124"),
                    new TaxForms2("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "Form123", "Form125","box125"),
                    new TaxForms2("5b05410c-c94c-464a-98ee-684198bde60b", "Form128", "Form126","box127"),
                };
            refDataRepositoryMock.Setup( i => i.GetTaxFormsBaseAsync( It.IsAny<bool>() ) ).ReturnsAsync( taxFormEntities );
            foreach( var record in taxFormEntities )
            {
                refDataRepositoryMock.Setup( loc => loc.GetTaxFormsGuidAsync( record.Code ) ).ReturnsAsync( record.Guid );
            }

            taxBoxEntities = new List<BoxCodes>()
                {
                    new BoxCodes("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "box123", "box123",  "Form123"),
                    new BoxCodes("874dee09-8662-47e6-af0d-504c257493a3", "box124", "box124", "Form124"),
                    new BoxCodes("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "box125", "box125", "Form125"),
                    new BoxCodes("5b05410c-c94c-464a-98ee-684198bde60b", "box126", "box126", "Form126"),
                };
            refDataRepositoryMock.Setup( i => i.GetAllBoxCodesAsync( It.IsAny<bool>() ) ).ReturnsAsync( taxBoxEntities );
            foreach( var record in taxBoxEntities )
            {
                refDataRepositoryMock.Setup( loc => loc.GetBoxCodesGuidAsync( record.Code ) ).ReturnsAsync( record.Guid );
            }

            addressUsages = new List<IntgVendorAddressUsages>()
                {
                    new IntgVendorAddressUsages("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "PO", "Purchase Order Address"),
                    new IntgVendorAddressUsages("874dee09-8662-47e6-af0d-504c257493a3",  "CHECK", "AP Check Address"),

                };
            referenceDataRepositoryMock.Setup( i => i.GetIntgVendorAddressUsagesAsync( It.IsAny<bool>() ) ).ReturnsAsync( addressUsages );
            foreach( var record in addressUsages )
            {
                referenceDataRepositoryMock.Setup( loc => loc.GetIntgVendorAddressUsagesGuidAsync( record.Code ) ).ReturnsAsync( record.Guid );
            }

            VendorHoldReasonsEntities = new List<Domain.ColleagueFinance.Entities.VendorHoldReasons>()
                {
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test"),
                    new Domain.ColleagueFinance.Entities.VendorHoldReasons("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support"),
                };
            referenceDataRepositoryMock.Setup( i => i.GetVendorHoldReasonsAsync( It.IsAny<bool>() ) ).ReturnsAsync( VendorHoldReasonsEntities );
            foreach( var record in VendorHoldReasonsEntities )
            {
                referenceDataRepositoryMock.Setup( loc => loc.GetVendorHoldReasonsGuidAsync( record.Code ) ).ReturnsAsync( record.Guid );
            }

            currencyConversionEntities = new List<Domain.ColleagueFinance.Entities.CurrencyConversion>()
                {
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("ALU", "American Labor Union"),
                    new Domain.ColleagueFinance.Entities.CurrencyConversion("NEA", "National Education Association")
                };
            referenceDataRepositoryMock.Setup( i => i.GetCurrencyConversionAsync() ).ReturnsAsync( currencyConversionEntities );

            vendorEntities = new List<Domain.ColleagueFinance.Entities.Vendors>()
                {
                    new Domain.ColleagueFinance.Entities.Vendors("ce4d68f6-257d-4052-92c8-17eed0f088fa")
                    {
                        Id = "0000231",
                        IsOrganization = true,
                        StopPaymentFlag = "Y",
                        ApprovalFlag = "Y",
                        ActiveFlag = "Y",
                        CurrencyCode = "USD",
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
                        IntgHoldReasons = new List<string>(){"AS" },
                        Categories = new List<string>() { "EP", "TR", "PR" },
                        CorpParent =  new List<string>(){"0000231" },
                        TaxId = "EIN123",
                        TaxForm = "Form123"

                    },
                    new Domain.ColleagueFinance.Entities.Vendors("5bc2d86c-6a0c-46b1-824d-485ccb27dc67"){IsOrganization = false, Id = "0000232"},
                    new Domain.ColleagueFinance.Entities.Vendors("7ea5142f-12f1-4ac9-b9f3-73e4205dfc11"){Id = "0000233"},
                    new Domain.ColleagueFinance.Entities.Vendors("db8f690b-071f-4d98-8da8-d4312511a4c1"){Id = "0000234"}
                };
            vendorEntityTuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.Vendors>, int>( vendorEntities, vendorEntities.Count() );
            vendorRepositoryMock.Setup( i => i.GetVendors2Async( It.IsAny<int>(), It.IsAny<int>(), "", null, null, null, It.IsAny<List<string>>(), null ) ).ReturnsAsync( vendorEntityTuple );
            vendorRepositoryMock.Setup( i => i.GetVendorsByGuid2Async( It.IsAny<string>() ) ).ReturnsAsync( vendorEntities.ToList()[ 0 ] );
            vendorRepositoryMock.Setup( i => i.GetVendorGuidFromIdAsync( It.IsAny<string>() ) ).ReturnsAsync( "123" );
            personRepositoryMock.Setup( i => i.GetPersonGuidFromIdAsync( It.IsAny<string>() ) ).ReturnsAsync( "db8f690b-071f-4d98-8da8-d4312511a4c2" );
            var personGuidCollection = new Dictionary<string, string>();
            personGuidCollection.Add( "0000231", "db8f690b-071f-4d98-8da8-d4312511a4c2" );
            personGuidCollection.Add( "0000232", "db8f690b-071f-4d98-8da8-d4312511a4c3" );
            personGuidCollection.Add( "0000233", "db8f690b-071f-4d98-8da8-d4312511a4c4" );
            personGuidCollection.Add( "0000234", "db8f690b-071f-4d98-8da8-d4312511a4c5" );
            var personAddressCollection = new Dictionary<string, string>();
            personAddressCollection.Add( "0000231", "address1" );
            personAddressCollection.Add( "0000232", "address2" );
            personRepositoryMock.Setup( p => p.GetPersonGuidsCollectionAsync( It.IsAny<string[]>() ) ).ReturnsAsync( personGuidCollection );
            personRepositoryMock.Setup( p => p.GetHierarchyAddressIdsAsync( It.IsAny<List<string>>(), It.IsAny<string>(), DateTime.Today ) ).ReturnsAsync( personAddressCollection );
            var addressGuidCollection = new Dictionary<string, string>();
            addressGuidCollection.Add( "address1", "db8f690b-071f-4d98-8da8-d4312591a4c2" );
            addressGuidCollection.Add( "address2", "db8f698b-071f-4d98-8da8-d4312511a4c2" );
            personRepositoryMock.Setup( p => p.GetAddressGuidsCollectionAsync( It.IsAny<List<string>>() ) ).ReturnsAsync( addressGuidCollection );

            addressTypes = new List<Domain.Base.Entities.AddressType2>()
            {
                new Domain.Base.Entities.AddressType2("db8f690b-071f-4d98-8da8-d4312511a4c1", "AT1", "Desc1", AddressTypeCategory.Billing),
                new Domain.Base.Entities.AddressType2("db8f690b-071f-4d98-8da8-d4312511a4c2", "AT2", "Desc2", AddressTypeCategory.Home),
                new Domain.Base.Entities.AddressType2("db8f690b-071f-4d98-8da8-d4312511a4c3", "AT3", "Desc3", AddressTypeCategory.Business),
                new Domain.Base.Entities.AddressType2("db8f690b-071f-4d98-8da8-d4312511a4c4", "AT4", "Desc4", AddressTypeCategory.Main),
            };
            refDataRepositoryMock.Setup( repo => repo.GetAddressTypes2Async( It.IsAny<bool>() ) ).ReturnsAsync( addressTypes );

            states = new List<State>()
            {
                new State("MN", "MN", "USA"),
                new State("MA", "MA", "USA"),
                new State("NY", "NY", "USA"),

            };
            refDataRepositoryMock.Setup( repo => repo.GetStateCodesAsync( It.IsAny<bool>() ) ).ReturnsAsync( states );

            countries = new List<Domain.Base.Entities.Country>()
            {
                new Domain.Base.Entities.Country("USA", "USA", "USA"){ IsoAlpha3Code = "USA" }
            };
            refDataRepositoryMock.Setup( repo => repo.GetCountryCodesAsync( It.IsAny<bool>() ) ).ReturnsAsync( countries );

            phoneTypes = new List<Domain.Base.Entities.PhoneType>()
            {
                new Domain.Base.Entities.PhoneType("gb8f690b-071f-4d98-8da8-d4312511a4c4", "Home", "Home", PhoneTypeCategory.Home, false)
            };
            refDataRepositoryMock.Setup( repo => repo.GetPhoneTypesAsync( It.IsAny<bool>() ) ).ReturnsAsync( phoneTypes );
        }
    }

    #endregion

    #region VendorServiceTests_SS

    [TestClass]
    public class VendorServiceTests_SS : CurrentUserSetup
    {
        #region DECLARATION
        Mock<IVendorsRepository> vendorRepositoryMock;
        Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IReferenceDataRepository> refDataRepositoryMock;
        Mock<IPersonRepository> personRepositoryMock;
        Mock<IAddressRepository> addressRepositoryMock;
        Mock<IVendorContactsRepository> vendorContactsRepositoryMock;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<IInstitutionRepository> institutionRepositoryMock;
        ICurrentUserFactory currentUserFactory;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;
        private IEnumerable<Domain.Entities.Role> roles;

        VendorsService vendorService;
        Dtos.ColleagueFinance.VendorSearchCriteria vendorSearchCriteria; 
        IEnumerable<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult> vendorEntities;
        Tuple<IEnumerable<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult>, int> vendorEntityTuple;

        IEnumerable<Domain.ColleagueFinance.Entities.VendorSearchResult> vendorSearchResultEntities;        
        Domain.ColleagueFinance.Entities.VendorDefaultTaxFormInfo vendorDefaultTaxFormInfoEntity;
        private string vendorId;
        private string apType;


        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private Domain.Entities.Permission permissionViewAnyPerson;

        #endregion

        #region SET UP 

        [TestInitialize]
        public void Initialize()
        {
            vendorRepositoryMock = new Mock<IVendorsRepository>();
            referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            refDataRepositoryMock = new Mock<IReferenceDataRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            addressRepositoryMock = new Mock<IAddressRepository>();
            vendorContactsRepositoryMock = new Mock<IVendorContactsRepository>();
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
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewVendor);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            vendorService = new VendorsService(referenceDataRepositoryMock.Object, vendorRepositoryMock.Object, personRepositoryMock.Object, addressRepositoryMock.Object, vendorContactsRepositoryMock.Object, refDataRepositoryMock.Object,
                                           institutionRepositoryMock.Object, baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

             // Set up and mock the adapter, and setup the GetAdapter method.
            var vendorDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult, Dtos.ColleagueFinance.VendorsVoucherSearchResult>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult, Dtos.ColleagueFinance.VendorsVoucherSearchResult>()).Returns(vendorDtoAdapter);

            var vendorSearchResultDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.VendorSearchResult, Dtos.ColleagueFinance.VendorSearchResult>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.VendorSearchResult, Dtos.ColleagueFinance.VendorSearchResult>()).Returns(vendorSearchResultDtoAdapter);

            var vendorDefaultTaxFormInfoDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.VendorDefaultTaxFormInfo, Dtos.ColleagueFinance.VendorDefaultTaxFormInfo>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.VendorDefaultTaxFormInfo, Dtos.ColleagueFinance.VendorDefaultTaxFormInfo>()).Returns(vendorDefaultTaxFormInfoDtoAdapter);

        }


        private void BuildData()
        {
            vendorSearchCriteria = new Dtos.ColleagueFinance.VendorSearchCriteria();

            vendorEntities = new List<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult>()
                {
                    new Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult()
                    {
                        VendorId = "0000192",
                        VendorNameLines = new List<string>{"Blue Cross Office supply" },
                        VendorMiscName = null,
                        AddressLines =  new List<string>{ "PO Box 69845" },
                        City = "Minneapolis",
                        State = "MN",
                        Zip = "55430",
                        Country = "",
                        FormattedAddress = "PO Box 69845 Minneapolis MN 55430",
                        AddressId = "143"
                    },
                    new Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult()
                    {
                        VendorId = "0000193",
                        VendorNameLines = new List<string> {"Logistic Office supply" },
                        VendorMiscName = null,
                        AddressLines = new List<string> { "PO Box 7777" },
                        City = "New York",
                        State = "MN",
                        Zip = "55430",
                        Country = "USA",
                        FormattedAddress = "PO Box 7777 New York MN 55430",
                        AddressId = "144"
                    }
                };
            vendorEntityTuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult>, int>(vendorEntities, vendorEntities.Count());
            vendorRepositoryMock.Setup(i => i.VendorSearchForVoucherAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(vendorEntities);

            vendorSearchResultEntities = new List<Domain.ColleagueFinance.Entities.VendorSearchResult>()
                {

                    new Domain.ColleagueFinance.Entities.VendorSearchResult("0000192")
                    {                        
                        VendorName = "Blue Cross Office supply",
                        VendorAddress = "Blue Cross Office supply Address",
                        TaxForm = "1098",
                        TaxFormCode = "MTG",
                        TaxFormLocation = "FL"
                    },
                    new Domain.ColleagueFinance.Entities.VendorSearchResult("0000193")
                    {
                        VendorName = "Logistic Office supply",
                        VendorAddress = "Logistic Office supply Address",
                        TaxForm = "1099",
                        TaxFormCode = "NEC",
                        TaxFormLocation = "MN"
                    }                    
                };            
            vendorRepositoryMock.Setup(i => i.SearchByKeywordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(vendorSearchResultEntities);

            vendorId = "0000192";
            apType = "AP";
            vendorDefaultTaxFormInfoEntity = new VendorDefaultTaxFormInfo(vendorId)
            {
                TaxForm = "1098",
                TaxFormBoxCode = "MTG",
                TaxFormState = "FL"
            };
            vendorRepositoryMock.Setup(i => i.GetVendorDefaultTaxFormInfoAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(vendorDefaultTaxFormInfoEntity);

            roles = new List<Domain.Entities.Role>() { new Domain.Entities.Role(1, "VIEW.VENDOR") };

            roles.FirstOrDefault().AddPermission(new Permission(ColleagueFinancePermissionCodes.ViewVendor));
        }

        #endregion

        #region Clean Up
        [TestCleanup]
        public void Cleanup()
        {
            vendorEntityTuple = null;
            vendorEntities = null;
            vendorDefaultTaxFormInfoEntity = null;
            vendorSearchResultEntities = null;
            vendorRepositoryMock = null;
            referenceDataRepositoryMock = null;
            adapterRegistryMock = null;
            currentUserFactory = null;
            roleRepositoryMock = null;
            loggerMock = null;
            institutionRepositoryMock = null;
        }
        #endregion

        #region TEST METHODS

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorService_QueryVendorForVoucherAsync_SearchCriteria_Null()
        {
            await vendorService.QueryVendorForVoucherAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorService_QueryVendorForVoucherAsync_SearchCriteria_Empty()
        {
            vendorSearchCriteria.QueryKeyword = "";   
            await vendorService.QueryVendorForVoucherAsync(vendorSearchCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VendorService_QueryVendorForVoucherAsync_PermissionException()
        {
            personRole.RemovePermission(permissionViewAnyPerson); //Removing the VIEW.VOUCHER Permission
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });
            vendorSearchCriteria.QueryKeyword = "Office";
            await vendorService.QueryVendorForVoucherAsync(vendorSearchCriteria);
        }

        [TestMethod]
        public async Task VendorService_VendorSearchForVoucherAsync_Repository_ReturnsNull()
        {
            vendorSearchCriteria.QueryKeyword = "Office";
            vendorRepositoryMock.Setup(i => i.VendorSearchForVoucherAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);
            var resultDto = await vendorService.QueryVendorForVoucherAsync(vendorSearchCriteria);
            Assert.AreEqual(resultDto.Count(), 0);
        }

        [TestMethod]
        public async Task VendorService_VendorSearchForVoucherAsync_Repository_ReturnsVendorSearchResults()
        {
            vendorSearchCriteria.QueryKeyword = "Office";
            vendorRepositoryMock.Setup(i => i.VendorSearchForVoucherAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(vendorEntities);
            var vendorSearchResultDtos = await vendorService.QueryVendorForVoucherAsync(vendorSearchCriteria);

            var vendorSearchResultDto = vendorSearchResultDtos.Where(x => x.VendorId == x.VendorId).FirstOrDefault();
            var vendorDomainEntity = vendorEntities.Where(x => x.VendorId == x.VendorId).FirstOrDefault();

            Assert.AreEqual(vendorSearchResultDtos.Count(), 2);
            Assert.AreEqual(vendorSearchResultDto.VendorId, vendorDomainEntity.VendorId);
            Assert.AreEqual(vendorSearchResultDto.Zip, vendorDomainEntity.Zip);
            Assert.AreEqual(vendorSearchResultDto.Country, vendorDomainEntity.Country);
            Assert.AreEqual(vendorSearchResultDto.FormattedAddress, vendorDomainEntity.FormattedAddress);
            Assert.AreEqual(vendorSearchResultDto.AddressId, vendorDomainEntity.AddressId);
            Assert.AreEqual(vendorSearchResultDto.TaxForm, vendorDomainEntity.TaxForm);
            Assert.AreEqual(vendorSearchResultDto.TaxFormCode, vendorDomainEntity.TaxFormCode);
            Assert.AreEqual(vendorSearchResultDto.TaxFormLocation, vendorDomainEntity.TaxFormLocation);
        }
        #endregion

        #region QueryVendorsByPostAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorService_QueryVendorsByPostAsync_SearchCriteria_Null()
        {
            await vendorService.QueryVendorsByPostAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorService_QueryVendorsByPostAsync_SearchCriteria_Empty()
        {
            vendorSearchCriteria.QueryKeyword = "";
            await vendorService.QueryVendorsByPostAsync(vendorSearchCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VendorService_QueryVendorsByPostAsync_PermissionException()
        {
            personRole.RemovePermission(permissionViewAnyPerson); //Removing the VIEW.VOUCHER Permission
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });
            vendorSearchCriteria.QueryKeyword = "Office";
            await vendorService.QueryVendorsByPostAsync(vendorSearchCriteria);
        }

        [TestMethod]
        public async Task VendorService_QueryVendorsByPostAsync_Repository_ReturnsNull()
        {
            vendorSearchCriteria.QueryKeyword = "Office";
            vendorRepositoryMock.Setup(i => i.SearchByKeywordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);
            var resultDto = await vendorService.QueryVendorsByPostAsync(vendorSearchCriteria);
            Assert.AreEqual(resultDto.Count(), 0);
        }

        [TestMethod]
        public async Task VendorService_QueryVendorsByPostAsync_Repository_ReturnsVendorSearchResults()
        {
            vendorSearchCriteria.QueryKeyword = "Office";
            vendorRepositoryMock.Setup(i => i.SearchByKeywordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(vendorSearchResultEntities);
            var vendorSearchResultDtos = await vendorService.QueryVendorsByPostAsync(vendorSearchCriteria);

            var vendorSearchResultDto = vendorSearchResultDtos.Where(x => x.VendorId == x.VendorId).FirstOrDefault();
            var vendorDomainEntity = vendorSearchResultEntities.Where(x => x.VendorId == x.VendorId).FirstOrDefault();

            Assert.AreEqual(vendorSearchResultDtos.Count(), 2);
            Assert.AreEqual(vendorSearchResultDto.VendorId, vendorDomainEntity.VendorId);
            Assert.AreEqual(vendorSearchResultDto.VendorName, vendorDomainEntity.VendorName);
            Assert.AreEqual(vendorSearchResultDto.VendorAddress, vendorDomainEntity.VendorAddress);
            Assert.AreEqual(vendorSearchResultDto.TaxForm, vendorDomainEntity.TaxForm);
            Assert.AreEqual(vendorSearchResultDto.TaxFormCode, vendorDomainEntity.TaxFormCode);
            Assert.AreEqual(vendorSearchResultDto.TaxFormLocation, vendorDomainEntity.TaxFormLocation);
        }
        #endregion

        #region GetVendorDefaultTaxFormInfoAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorService_GetVendorDefaultTaxFormInfoAsync_SearchCriteria_Null()
        {
            await vendorService.GetVendorDefaultTaxFormInfoAsync(null, apType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorService_GetVendorDefaultTaxFormInfoAsync_SearchCriteria_Empty()
        {
            await vendorService.GetVendorDefaultTaxFormInfoAsync("", apType);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VendorService_GetVendorDefaultTaxFormInfoAsync_PermissionException()
        {
            personRole.RemovePermission(permissionViewAnyPerson); //Removing the VIEW.VOUCHER Permission
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });
            
            await vendorService.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);
        }

        [TestMethod]
        public async Task VendorService_GetVendorDefaultTaxFormInfoAsync_Repository_ReturnsNull()
        {             
            vendorRepositoryMock.Setup(i => i.GetVendorDefaultTaxFormInfoAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);
            var resultDto = await vendorService.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);
            Assert.IsNotNull(resultDto);
            Assert.IsNull(resultDto.TaxForm);
            Assert.IsNull(resultDto.TaxFormBoxCode);
            Assert.IsNull(resultDto.TaxFormState);
        }

        [TestMethod]
        public async Task VendorService_GetVendorDefaultTaxFormInfoAsync_Repository_ReturnsResult()
        {
            vendorRepositoryMock.Setup(i => i.GetVendorDefaultTaxFormInfoAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(this.vendorDefaultTaxFormInfoEntity);
            var resultDto = await vendorService.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);
                        
            Assert.AreEqual(resultDto.VendorId, vendorDefaultTaxFormInfoEntity.VendorId);
            Assert.AreEqual(resultDto.TaxForm, vendorDefaultTaxFormInfoEntity.TaxForm);
            Assert.AreEqual(resultDto.TaxFormBoxCode, vendorDefaultTaxFormInfoEntity.TaxFormBoxCode);
            Assert.AreEqual(resultDto.TaxFormState, vendorDefaultTaxFormInfoEntity.TaxFormState);
        }
        #endregion
    }
    #endregion
}
