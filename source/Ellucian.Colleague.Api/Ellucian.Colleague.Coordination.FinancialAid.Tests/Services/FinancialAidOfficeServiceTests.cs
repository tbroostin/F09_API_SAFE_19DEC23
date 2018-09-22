/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class FinancialAidOfficeServiceTests
    {
        [TestClass]
        public class GetFinancialAidOfficeTests : FinancialAidServiceTestsSetup
        {
            private string studentId;

            private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;

            private IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> inputOfficeEntities;
            private AutoMapperAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice> officeDtoAdapter;

            private List<Dtos.FinancialAid.FinancialAidOffice> expectedOffices;
            private IEnumerable<Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice> actualOffices;

            private Mock<IFinancialAidOfficeRepository> officeRepositoryMock;

            private FinancialAidOfficeService financialAidOfficeService;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                studentId = currentUserFactory.CurrentUser.PersonId;

                testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                inputOfficeEntities = testFinancialAidOfficeRepository.GetFinancialAidOffices();

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync()).ReturnsAsync(testFinancialAidOfficeRepository.GetFinancialAidOffices());

                officeDtoAdapter = new FinancialAidOfficeEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedOffices = new List<Dtos.FinancialAid.FinancialAidOffice>();
                foreach (var inputOffice in inputOfficeEntities)
                {
                    expectedOffices.Add(officeDtoAdapter.MapToType(inputOffice));
                }

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice>()
                    ).Returns(officeDtoAdapter);

                financialAidOfficeService = new FinancialAidOfficeService(
                    adapterRegistryMock.Object,
                    officeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object
                    );

                actualOffices = financialAidOfficeService.GetFinancialAidOffices();
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedOffices);
                Assert.IsNotNull(actualOffices);
            }

            [TestMethod]
            public void NumOfficesAreEqualTest()
            {
                Assert.IsTrue(expectedOffices.Count() > 0);
                Assert.IsTrue(actualOffices.Count() > 0);
                Assert.AreEqual(expectedOffices.Count(), actualOffices.Count());
            }

            [TestMethod]
            public void ObjectsAreEqualTest()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Id == expectedOffice.Id);
                    Assert.IsNotNull(actualOffice);

                    Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                    Assert.AreEqual(expectedOffice.PhoneNumber, actualOffice.PhoneNumber);
                    Assert.AreEqual(expectedOffice.DirectorName, actualOffice.DirectorName);
                    Assert.AreEqual(expectedOffice.EmailAddress, actualOffice.EmailAddress);
                    Assert.AreEqual(expectedOffice.AddressLabel.Count(), actualOffice.AddressLabel.Count());
                    for (int i = 0; i < expectedOffice.AddressLabel.Count(); i++)
                    {
                        Assert.AreEqual(expectedOffice.AddressLabel[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public void EmptyOfficeListTest()
            {
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(new List<Domain.FinancialAid.Entities.FinancialAidOffice>());

                actualOffices = financialAidOfficeService.GetFinancialAidOffices();
                Assert.IsNotNull(actualOffices);
                Assert.IsTrue(actualOffices.Count() == 0);
            }

            [TestMethod]
            public void NullOffices_ThrowsException_LogsMessageTest()
            {
                inputOfficeEntities = null;
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(inputOfficeEntities);

                var exceptionCaught = false;
                try
                {
                    actualOffices = financialAidOfficeService.GetFinancialAidOffices();
                }
                catch (KeyNotFoundException)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error("Null FinancialAidOffice object returned by repository"));


            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testFinancialAidOfficeRepository = null;
                inputOfficeEntities = null;
                officeDtoAdapter = null;
                expectedOffices = null;
                actualOffices = null;
                officeRepositoryMock = null;
                financialAidOfficeService = null;
            }
        }

        [TestClass]
        public class GetFinancialAidOfficeAsyncTests : FinancialAidServiceTestsSetup
        {
            private string studentId;

            private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;

            private IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> inputOfficeEntities;
            private AutoMapperAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice> officeDtoAdapter;

            private List<Dtos.FinancialAid.FinancialAidOffice> expectedOffices;
            private IEnumerable<Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice> actualOffices;

            private Mock<IFinancialAidOfficeRepository> officeRepositoryMock;

            private FinancialAidOfficeService financialAidOfficeService;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                studentId = currentUserFactory.CurrentUser.PersonId;

                testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                inputOfficeEntities = await testFinancialAidOfficeRepository.GetFinancialAidOfficesAsync();

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync()).Returns(testFinancialAidOfficeRepository.GetFinancialAidOfficesAsync());

                officeDtoAdapter = new FinancialAidOfficeEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedOffices = new List<Dtos.FinancialAid.FinancialAidOffice>();
                foreach (var inputOffice in inputOfficeEntities)
                {
                    expectedOffices.Add(officeDtoAdapter.MapToType(inputOffice));
                }

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice>()
                    ).Returns(officeDtoAdapter);

                financialAidOfficeService = new FinancialAidOfficeService(
                    adapterRegistryMock.Object,
                    officeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object
                    );

                actualOffices = await financialAidOfficeService.GetFinancialAidOfficesAsync();
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedOffices);
                Assert.IsNotNull(actualOffices);
            }

            [TestMethod]
            public void NumOfficesAreEqualTest()
            {
                Assert.IsTrue(expectedOffices.Count() > 0);
                Assert.IsTrue(actualOffices.Count() > 0);
                Assert.AreEqual(expectedOffices.Count(), actualOffices.Count());
            }

            [TestMethod]
            public void ObjectsAreEqualTest()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Id == expectedOffice.Id);
                    Assert.IsNotNull(actualOffice);

                    Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                    Assert.AreEqual(expectedOffice.PhoneNumber, actualOffice.PhoneNumber);
                    Assert.AreEqual(expectedOffice.DirectorName, actualOffice.DirectorName);
                    Assert.AreEqual(expectedOffice.EmailAddress, actualOffice.EmailAddress);
                    Assert.AreEqual(expectedOffice.AddressLabel.Count(), actualOffice.AddressLabel.Count());
                    for (int i = 0; i < expectedOffice.AddressLabel.Count(); i++)
                    {
                        Assert.AreEqual(expectedOffice.AddressLabel[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public async Task EmptyOfficeListTest()
            {
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(new List<Domain.FinancialAid.Entities.FinancialAidOffice>());

                actualOffices = await financialAidOfficeService.GetFinancialAidOfficesAsync();
                Assert.IsNotNull(actualOffices);
                Assert.IsTrue(actualOffices.Count() == 0);
            }

            [TestMethod]
            public async Task NullOffices_ThrowsException_LogsMessageTest()
            {
                inputOfficeEntities = null;
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(inputOfficeEntities);

                var exceptionCaught = false;
                try
                {
                    actualOffices = await financialAidOfficeService.GetFinancialAidOfficesAsync();
                }
                catch (KeyNotFoundException)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error("Null FinancialAidOffice object returned by repository"));


            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testFinancialAidOfficeRepository = null;
                inputOfficeEntities = null;
                officeDtoAdapter = null;
                expectedOffices = null;
                actualOffices = null;
                officeRepositoryMock = null;
                financialAidOfficeService = null;
            }
        }

        [TestClass]
        public class GetFinancialAidOffice2Tests : FinancialAidServiceTestsSetup
        {
            private string studentId;

            private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;

            private IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> inputOfficeEntities;
            private AutoMapperAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice2> officeDtoAdapter;

            private List<Dtos.FinancialAid.FinancialAidOffice2> expectedOffices;
            private IEnumerable<Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice2> actualOffices;

            private Mock<IFinancialAidOfficeRepository> officeRepositoryMock;

            private FinancialAidOfficeService financialAidOfficeService;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                studentId = currentUserFactory.CurrentUser.PersonId;

                testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                inputOfficeEntities = testFinancialAidOfficeRepository.GetFinancialAidOffices();

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync()).ReturnsAsync(testFinancialAidOfficeRepository.GetFinancialAidOffices());

                officeDtoAdapter = new FinancialAidOffice2EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedOffices = new List<Dtos.FinancialAid.FinancialAidOffice2>();
                foreach (var inputOffice in inputOfficeEntities)
                {
                    expectedOffices.Add(officeDtoAdapter.MapToType(inputOffice));
                }

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice2>()
                    ).Returns(officeDtoAdapter);

                financialAidOfficeService = new FinancialAidOfficeService(
                    adapterRegistryMock.Object,
                    officeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object
                    );

                actualOffices = financialAidOfficeService.GetFinancialAidOffices2();
            }

            [TestMethod]
            public void ObjectsHaveValueTest2()
            {
                Assert.IsNotNull(expectedOffices);
                Assert.IsNotNull(actualOffices);
            }

            [TestMethod]
            public void NumOfficesAreEqualTest2()
            {
                Assert.IsTrue(expectedOffices.Count() > 0);
                Assert.IsTrue(actualOffices.Count() > 0);
                Assert.AreEqual(expectedOffices.Count(), actualOffices.Count());
            }

            [TestMethod]
            public void ObjectsAreEqualTest2()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Id == expectedOffice.Id);
                    Assert.IsNotNull(actualOffice);

                    Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                    Assert.AreEqual(expectedOffice.PhoneNumber, actualOffice.PhoneNumber);
                    Assert.AreEqual(expectedOffice.DirectorName, actualOffice.DirectorName);
                    Assert.AreEqual(expectedOffice.EmailAddress, actualOffice.EmailAddress);
                    Assert.AreEqual(expectedOffice.AddressLabel.Count(), actualOffice.AddressLabel.Count());
                    for (int i = 0; i < expectedOffice.AddressLabel.Count(); i++)
                    {
                        Assert.AreEqual(expectedOffice.AddressLabel[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public void EmptyOfficeListTest2()
            {
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(new List<Domain.FinancialAid.Entities.FinancialAidOffice>());

                actualOffices = financialAidOfficeService.GetFinancialAidOffices2();
                Assert.IsNotNull(actualOffices);
                Assert.IsTrue(actualOffices.Count() == 0);
            }

            [TestMethod]
            public void NullOffices_ThrowsException_LogsMessageTest2()
            {
                inputOfficeEntities = null;
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(inputOfficeEntities);

                var exceptionCaught = false;
                try
                {
                    actualOffices = financialAidOfficeService.GetFinancialAidOffices2();
                }
                catch (KeyNotFoundException)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error("Null FinancialAidOffice object returned by repository"));


            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testFinancialAidOfficeRepository = null;
                inputOfficeEntities = null;
                officeDtoAdapter = null;
                expectedOffices = null;
                actualOffices = null;
                officeRepositoryMock = null;
                financialAidOfficeService = null;
            }
        }

        [TestClass]
        public class GetFinancialAidOffice2AsyncTests : FinancialAidServiceTestsSetup
        {
            private string studentId;

            private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;

            private IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> inputOfficeEntities;
            private AutoMapperAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice2> officeDtoAdapter;

            private List<Dtos.FinancialAid.FinancialAidOffice2> expectedOffices;
            private IEnumerable<Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice2> actualOffices;

            private Mock<IFinancialAidOfficeRepository> officeRepositoryMock;

            private FinancialAidOfficeService financialAidOfficeService;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                studentId = currentUserFactory.CurrentUser.PersonId;

                testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                inputOfficeEntities = await testFinancialAidOfficeRepository.GetFinancialAidOfficesAsync();

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync()).Returns(testFinancialAidOfficeRepository.GetFinancialAidOfficesAsync());

                officeDtoAdapter = new FinancialAidOffice2EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedOffices = new List<Dtos.FinancialAid.FinancialAidOffice2>();
                foreach (var inputOffice in inputOfficeEntities)
                {
                    expectedOffices.Add(officeDtoAdapter.MapToType(inputOffice));
                }

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice2>()
                    ).Returns(officeDtoAdapter);

                financialAidOfficeService = new FinancialAidOfficeService(
                    adapterRegistryMock.Object,
                    officeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object
                    );

                actualOffices = await financialAidOfficeService.GetFinancialAidOffices2Async();
            }

            [TestMethod]
            public void ObjectsHaveValueTest2()
            {
                Assert.IsNotNull(expectedOffices);
                Assert.IsNotNull(actualOffices);
            }

            [TestMethod]
            public void NumOfficesAreEqualTest2()
            {
                Assert.IsTrue(expectedOffices.Count() > 0);
                Assert.IsTrue(actualOffices.Count() > 0);
                Assert.AreEqual(expectedOffices.Count(), actualOffices.Count());
            }

            [TestMethod]
            public void ObjectsAreEqualTest2()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Id == expectedOffice.Id);
                    Assert.IsNotNull(actualOffice);

                    Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                    Assert.AreEqual(expectedOffice.PhoneNumber, actualOffice.PhoneNumber);
                    Assert.AreEqual(expectedOffice.DirectorName, actualOffice.DirectorName);
                    Assert.AreEqual(expectedOffice.EmailAddress, actualOffice.EmailAddress);
                    Assert.AreEqual(expectedOffice.AddressLabel.Count(), actualOffice.AddressLabel.Count());
                    for (int i = 0; i < expectedOffice.AddressLabel.Count(); i++)
                    {
                        Assert.AreEqual(expectedOffice.AddressLabel[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public async Task EmptyOfficeListTest2()
            {
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(new List<Domain.FinancialAid.Entities.FinancialAidOffice>());

                actualOffices = await financialAidOfficeService.GetFinancialAidOffices2Async();
                Assert.IsNotNull(actualOffices);
                Assert.IsTrue(actualOffices.Count() == 0);
            }

            [TestMethod]
            public async Task NullOffices_ThrowsException_LogsMessageTest2()
            {
                inputOfficeEntities = null;
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(inputOfficeEntities);

                var exceptionCaught = false;
                try
                {
                    actualOffices = await financialAidOfficeService.GetFinancialAidOffices2Async();
                }
                catch (KeyNotFoundException)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error("Null FinancialAidOffice object returned by repository"));


            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testFinancialAidOfficeRepository = null;
                inputOfficeEntities = null;
                officeDtoAdapter = null;
                expectedOffices = null;
                actualOffices = null;
                officeRepositoryMock = null;
                financialAidOfficeService = null;
            }
        }
    
        [TestClass]
        public class GetFinancialAidOffice3AsyncTests : FinancialAidServiceTestsSetup
        {
            private string studentId;

            private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;

            private IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> inputOfficeEntities;
            private AutoMapperAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice3> officeDtoAdapter;

            private List<Dtos.FinancialAid.FinancialAidOffice3> expectedOffices;
            private IEnumerable<Dtos.FinancialAid.FinancialAidOffice3> actualOffices;

            private Mock<IFinancialAidOfficeRepository> officeRepositoryMock;

            private FinancialAidOfficeService financialAidOfficeService;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                studentId = currentUserFactory.CurrentUser.PersonId;

                testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                inputOfficeEntities = await testFinancialAidOfficeRepository.GetFinancialAidOfficesAsync();

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync()).Returns(testFinancialAidOfficeRepository.GetFinancialAidOfficesAsync());

                officeDtoAdapter = new FinancialAidOffice3EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedOffices = new List<Dtos.FinancialAid.FinancialAidOffice3>();
                foreach (var inputOffice in inputOfficeEntities)
                {
                    expectedOffices.Add(officeDtoAdapter.MapToType(inputOffice));
                }

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice3>()
                    ).Returns(officeDtoAdapter);

                financialAidOfficeService = new FinancialAidOfficeService(
                    adapterRegistryMock.Object,
                    officeRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object
                    );

                actualOffices = await financialAidOfficeService.GetFinancialAidOffices3Async();
            }

            [TestMethod]
            public void ObjectsHaveValueTest2()
            {
                Assert.IsNotNull(expectedOffices);
                Assert.IsNotNull(actualOffices);
            }

            [TestMethod]
            public void NumOfficesAreEqualTest2()
            {
                Assert.IsTrue(expectedOffices.Count() > 0);
                Assert.IsTrue(actualOffices.Count() > 0);
                Assert.AreEqual(expectedOffices.Count(), actualOffices.Count());
            }

            [TestMethod]
            public void ObjectsAreEqualTest2()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Id == expectedOffice.Id);
                    Assert.IsNotNull(actualOffice);

                    Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                    Assert.AreEqual(expectedOffice.PhoneNumber, actualOffice.PhoneNumber);
                    Assert.AreEqual(expectedOffice.DirectorName, actualOffice.DirectorName);
                    Assert.AreEqual(expectedOffice.EmailAddress, actualOffice.EmailAddress);
                    Assert.AreEqual(expectedOffice.AddressLabel.Count(), actualOffice.AddressLabel.Count());
                    for (int i = 0; i < expectedOffice.AddressLabel.Count(); i++)
                    {
                        Assert.AreEqual(expectedOffice.AddressLabel[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public async Task EmptyOfficeListTest2()
            {
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(new List<Domain.FinancialAid.Entities.FinancialAidOffice>());

                actualOffices = await financialAidOfficeService.GetFinancialAidOffices3Async();
                Assert.IsNotNull(actualOffices);
                Assert.IsTrue(actualOffices.Count() == 0);
            }

            [TestMethod]
            public async Task NullOffices_ThrowsException_LogsMessageTest2()
            {
                inputOfficeEntities = null;
                officeRepositoryMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(inputOfficeEntities);

                var exceptionCaught = false;
                try
                {
                    actualOffices = await financialAidOfficeService.GetFinancialAidOffices3Async();
                }
                catch (KeyNotFoundException)
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error("Null FinancialAidOffice object returned by repository"));


            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testFinancialAidOfficeRepository = null;
                inputOfficeEntities = null;
                officeDtoAdapter = null;
                expectedOffices = null;
                actualOffices = null;
                officeRepositoryMock = null;
                financialAidOfficeService = null;
            }
        }
    }
}
