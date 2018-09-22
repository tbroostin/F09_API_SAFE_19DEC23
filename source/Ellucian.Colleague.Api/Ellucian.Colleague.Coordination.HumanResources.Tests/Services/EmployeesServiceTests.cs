//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
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
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Coordination.Base.Tests.Services;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class EmployeeServiceTests : CurrentUserSetup
    {
        [TestClass]
        public class EmployeeServiceTests_Tests : CurrentUserSetup
        {
            Mock<IEmployeeRepository> employeeRepositoryMock;
            Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            Mock<IPositionRepository> positionRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            EmployeeService employeesService;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            IEnumerable<Domain.Base.Entities.PersonBase> personEntities;
            IEnumerable<string> employeeKeysList;

            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                employeeRepositoryMock = new Mock<IEmployeeRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                positionRepositoryMock = new Mock<IPositionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewEmployeeData);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                employeesService = new EmployeeService(personRepositoryMock.Object, personBaseRepositoryMock.Object, employeeRepositoryMock.Object, referenceDataRepositoryMock.Object, hrReferenceDataRepositoryMock.Object,
                                               positionRepositoryMock.Object, baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personEntities = null;
                employeeKeysList = null;
                employeeRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
                referenceDataRepositoryMock = null;
            }

            [TestMethod]
            public async Task Employees_QueryEmployeeNameByPostAsync()
            {

                Dtos.Base.EmployeeNameQueryCriteria criteria = new Dtos.Base.EmployeeNameQueryCriteria { QueryKeyword = "Jack Black" };

                var employees = await employeesService.QueryEmployeeNamesByPostAsync(criteria);

                Assert.IsNotNull(employees);

                int count = employees.Count();

                Assert.IsTrue(count > 0);

                for (int i = 0; i < count; i++)
                {
                    var expected = personEntities.ToList()[i];
                    var actual = employees.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.FirstName, actual.FirstName);
                }

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Employees_QueryEmployeeNameByPostAsync_ThrowsArgumentNullException()
            {
                 await employeesService.QueryEmployeeNamesByPostAsync(null);
            }

            private void BuildData()
            {
                personEntities = new List<Domain.Base.Entities.PersonBase>()
                {
                    new Domain.Base.Entities.PersonBase("12345", "Black")
                    {
                        Guid = "54321",
                        FirstName = "Jack"
                    }

                };

                employeeKeysList = new List<string>() { "12345", "98765", "23145", "14725", "87104" };

                List<string> ids = null;
                bool? hasOnlineConsent = null;
                bool active = false;
                bool includeNonEmp = false;

                employeeRepositoryMock.Setup(i => i.GetEmployeeKeysAsync(ids, hasOnlineConsent, active, includeNonEmp)).ReturnsAsync(employeeKeysList);
                personBaseRepositoryMock.Setup(i => i.SearchByIdsOrNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personEntities);

            }
        }

    }

    [TestClass]
    public class EmployeeServiceTests_V8 : CurrentUserSetup
    {
        [TestClass]
        public class EmployeeServiceTests_GET_V8 : CurrentUserSetup
        {
            Mock<IEmployeeRepository> employeeRepositoryMock;
            Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            Mock<IPositionRepository> positionRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            EmployeeService employeesService;
            IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> employeesEntities;
            Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int> employeesEntityTuple;

            //IEnumerable<Domain.Base.Entities.Person> personEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType> rehireTypeEntities;
            IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason> employmentStatusEndingReasonEntities;
            //IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> acctPaySourceEntities;
            //IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionEntities;
            //IEnumerable<Domain.Base.Entities.Institution> institutionsEntities;
            //IEnumerable<Domain.HumanResources.Entities.employeePay> employeePayEntities;

            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                employeeRepositoryMock = new Mock<IEmployeeRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                positionRepositoryMock = new Mock<IPositionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewEmployeeData);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Ellucian.Colleague.Domain.Entities.Role>() { personRole });

                employeesService = new EmployeeService(personRepositoryMock.Object, personBaseRepositoryMock.Object, employeeRepositoryMock.Object, referenceDataRepositoryMock.Object, hrReferenceDataRepositoryMock.Object,
                                               positionRepositoryMock.Object, baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                employeesEntityTuple = null;
                employeesEntities = null;
                rehireTypeEntities = null;
                employmentStatusEndingReasonEntities = null;
                employeeRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
                referenceDataRepositoryMock = null;
            }

            [TestMethod]
            public async Task Employees_GETAllAsync()
            {
                var actualsTuple =
                    await
                        employeesService.GetEmployeesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = employeesEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

           
            [TestMethod]
            public async Task Employees_GETAllFilterAsync()
            {
                string personId = "0000011";
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                string employeeId = "x";
                employeeRepositoryMock.Setup(i => i.GetEmployeeIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(employeeId);

                var actualsTuple =
                    await
                        employeesService.GetEmployeesAsync(offset, limit, It.IsAny<bool>(), "cd385d31-75ed-4d93-9a1b-4776a951396d",
                        "", "", "2000-01-01 00:00:00.000",
                        "2020-12-31 00:00:00.000", "", "");

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = employeesEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }
           
            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployeesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidPersonFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployeesAsync(offset, limit, It.IsAny<bool>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidEmployerFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployeesAsync(offset, limit, It.IsAny<bool>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

           

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidStartDateFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployeesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidEndDateFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployeesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }


            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidStatusFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployeesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_StatusLeaveFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {
                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployeesAsync(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), "leave", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GET_ById()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = employeesEntities.ToList()[0];
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(i => i.GetGuidLookupResultFromGuidAsync(id)).ReturnsAsync(new GuidLookupResult() { Entity = "HRPER", PrimaryKey = id });
                var actual = await employeesService.GetEmployeeByGuidAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_NullId_KeyNotFoundException()
            {
                await employeesService.GetEmployeeByGuidAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).Throws<KeyNotFoundException>();
                await employeesService.GetEmployeeByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).Throws<InvalidOperationException>();
                await employeesService.GetEmployeeByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).Throws<RepositoryException>();
                await employeesService.GetEmployeeByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).Throws<Exception>();
                await employeesService.GetEmployeeByGuidAsync(id);
            }

            private void BuildData()
            {
                employmentStatusEndingReasonEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason>()
                {
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentStatusEndingReasonsAsync(It.IsAny<bool>())).ReturnsAsync(employmentStatusEndingReasonEntities);

                rehireTypeEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType>()
                    {
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty", "i"),
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support", "o"),
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test", "i"),
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support", "o"),
                    };
                hrReferenceDataRepositoryMock.Setup(i => i.GetRehireTypesAsync(It.IsAny<bool>())).ReturnsAsync(rehireTypeEntities);

                //var perposwgItems = new List<Domain.HumanResources.Entities.PersonemployeeWageItem>()
                //{
                //    new Domain.HumanResources.Entities.PersonemployeeWageItem()
                //    {
                //        GlNumber = "11-00-02-67-60000-53011",
                //        PpwgProjectsId = "12345",
                //        GlPercentDistribution = 100,
                //        StartDate = new DateTime(2017,7,17)
                //    }
                //};

                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                    {
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("ce4d68f6-257d-4052-92c8-17eed0f088fa", "e9e6837f-2c51-431b-9069-4ac4c0da3041")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)

                        },
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "9ae3a175-1dfd-4937-b97b-3c9ad596e023")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)
                        },
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("7ea5142f-12f1-4ac9-b9f3-73e4205dfc11", "e9e6837f-2c51-431b-9069-4ac4c0da3041")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)
                        },
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("db8f690b-071f-4d98-8da8-d4312511a4c1", "bfea651b-8e27-4fcd-abe3-04573443c04c")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)
                        }
                    };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, employeesEntities.Count());
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(It.IsAny<string>())).ReturnsAsync(employeesEntities.ToList()[0]);
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
                // employeeRepositoryMock.Setup(i => i..GetemployeeGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");


            }
        }
    }

    public class EmployeeServiceTests_V11 : CurrentUserSetup
    {
        [TestClass]
        public class EmployeeServiceTests_GET_V11 : CurrentUserSetup
        {
            Mock<IEmployeeRepository> employeeRepositoryMock;
            Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            Mock<IPositionRepository> positionRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            EmployeeService employeesService;
            IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> employeesEntities;
            Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int> employeesEntityTuple;

            //IEnumerable<Domain.Base.Entities.Person> personEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType> rehireTypeEntities;
            IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason> employmentStatusEndingReasonEntities;
            //IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> acctPaySourceEntities;
            //IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionEntities;
            //IEnumerable<Domain.Base.Entities.Institution> institutionsEntities;
            //IEnumerable<Domain.HumanResources.Entities.employeePay> employeePayEntities;

            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                employeeRepositoryMock = new Mock<IEmployeeRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                positionRepositoryMock = new Mock<IPositionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewEmployeeData);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Ellucian.Colleague.Domain.Entities.Role>() { personRole });

                employeesService = new EmployeeService(personRepositoryMock.Object, personBaseRepositoryMock.Object, employeeRepositoryMock.Object, referenceDataRepositoryMock.Object, hrReferenceDataRepositoryMock.Object,
                                               positionRepositoryMock.Object, baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                employeesEntityTuple = null;
                employeesEntities = null;
                rehireTypeEntities = null;
                employmentStatusEndingReasonEntities = null;
                employeeRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
                referenceDataRepositoryMock = null;
            }

            [TestMethod]
            public async Task Employees_GETAllAsync()
            {
                var actualsTuple =
                    await
                        employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = employeesEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            
            [TestMethod]
            public async Task Employees_GETAllFilterAsync()
            {
                string personId = "0000011";
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                string employeeId = "x";
                employeeRepositoryMock.Setup(i => i.GetEmployeeIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(employeeId);

                var actualsTuple =
                    await
                        employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), "cd385d31-75ed-4d93-9a1b-4776a951396d",
                        "", It.IsAny<string>(), "2000-01-01 00:00:00.000",
                        "2020-12-31 00:00:00.000", "", "");

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = employeesEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }
            

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidPersonFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidEmployerFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

          

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidStartDateFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidEndDateFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }


            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidStatusFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_StatusLeaveFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {
                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), "leave", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GET_ById()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = employeesEntities.ToList()[0];
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(i => i.GetGuidLookupResultFromGuidAsync(id)).ReturnsAsync(new GuidLookupResult() { Entity = "HRPER", PrimaryKey = id });
                var actual = await employeesService.GetEmployee2ByIdAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_NullId_KeyNotFoundException()
            {
                await employeesService.GetEmployee2ByIdAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).Throws<KeyNotFoundException>();
                await employeesService.GetEmployee2ByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).Throws<InvalidOperationException>();
                await employeesService.GetEmployee2ByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).Throws<RepositoryException>();
                await employeesService.GetEmployee2ByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(id)).Throws<Exception>();
                await employeesService.GetEmployee2ByIdAsync(id);
            }

            private void BuildData()
            {
                employmentStatusEndingReasonEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason>()
                {
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentStatusEndingReasonsAsync(It.IsAny<bool>())).ReturnsAsync(employmentStatusEndingReasonEntities);

                rehireTypeEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType>()
                    {
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty", "i"),
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support", "o"),
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test", "i"),
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support", "o"),
                    };
                hrReferenceDataRepositoryMock.Setup(i => i.GetRehireTypesAsync(It.IsAny<bool>())).ReturnsAsync(rehireTypeEntities);

                //var perposwgItems = new List<Domain.HumanResources.Entities.PersonemployeeWageItem>()
                //{
                //    new Domain.HumanResources.Entities.PersonemployeeWageItem()
                //    {
                //        GlNumber = "11-00-02-67-60000-53011",
                //        PpwgProjectsId = "12345",
                //        GlPercentDistribution = 100,
                //        StartDate = new DateTime(2017,7,17)
                //    }
                //};

                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                    {
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("ce4d68f6-257d-4052-92c8-17eed0f088fa", "e9e6837f-2c51-431b-9069-4ac4c0da3041")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)

                        },
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "9ae3a175-1dfd-4937-b97b-3c9ad596e023")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)
                        },
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("7ea5142f-12f1-4ac9-b9f3-73e4205dfc11", "e9e6837f-2c51-431b-9069-4ac4c0da3041")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)
                        },
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("db8f690b-071f-4d98-8da8-d4312511a4c1", "bfea651b-8e27-4fcd-abe3-04573443c04c")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)
                        }
                    };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, employeesEntities.Count());
                employeeRepositoryMock.Setup(i => i.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                employeeRepositoryMock.Setup(i => i.GetEmployeeByGuidAsync(It.IsAny<string>())).ReturnsAsync(employeesEntities.ToList()[0]);
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
                // employeeRepositoryMock.Setup(i => i..GetemployeeGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");


            }
        }
    }

    [TestClass]
        public class EmployeeServiceTests_POST_PUT_V12 : CurrentUserSetup
        {
            #region DECLARATION

            protected Domain.Entities.Role updateEmployee = new Domain.Entities.Role(1, "UPDATE.EMPLOYEE");

            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IPersonBaseRepository> personBaseRepositoryMock;
            private Mock<IEmployeeRepository> employeeRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            private Mock<IPositionRepository> positionRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;

            private ICurrentUserFactory currentUserFactory;

            private EmployeeService employeeService;

            private Dtos.Employee2 employee;
            private List<Domain.Base.Entities.Location> locations;
            private List<Domain.HumanResources.Entities.HrStatuses> hrStatuses;
            private List<Domain.HumanResources.Entities.EmploymentStatusEndingReason> endingReasons;
            private List<Domain.HumanResources.Entities.RehireType> rehireTypes;
            private List<Domain.Base.Entities.Person> persons;

            private Domain.HumanResources.Entities.Employee domainEmployee;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                employeeRepositoryMock = new Mock<IEmployeeRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                positionRepositoryMock = new Mock<IPositionRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.EmployeeUserFactory();

                InitializeTestData();

                InitializeMock();

                employeeService = new EmployeeService(personRepositoryMock.Object, personBaseRepositoryMock.Object, employeeRepositoryMock.Object, referenceDataRepositoryMock.Object, hrReferenceDataRepositoryMock.Object,
                    positionRepositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personRepositoryMock = null;
                adapterRegistryMock = null;
                referenceDataRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                employeeRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;
            }

            private void InitializeTestData()
            {
                locations = new List<Domain.Base.Entities.Location>()
                {
                    new Domain.Base.Entities.Location(guid, "1", "description")
                };

                hrStatuses = new List<Domain.HumanResources.Entities.HrStatuses>()
                {
                    new Domain.HumanResources.Entities.HrStatuses(guid, "1", "description") { Category = Domain.HumanResources.Entities.ContractType.FullTime },
                    new Domain.HumanResources.Entities.HrStatuses("1a59eed8-5fe7-4120-b1cf-f23266b9e875", "2", "description") { Category = Domain.HumanResources.Entities.ContractType.PartTime },
                    new Domain.HumanResources.Entities.HrStatuses("1a59eed8-5fe7-4120-b1cf-f23266b9e876", "3", "description") { Category = Domain.HumanResources.Entities.ContractType.Contractual }
                };

                endingReasons = new List<Domain.HumanResources.Entities.EmploymentStatusEndingReason>()
                {
                    new Domain.HumanResources.Entities.EmploymentStatusEndingReason(guid, "1", "description")
                };

                rehireTypes = new List<Domain.HumanResources.Entities.RehireType>()
                {
                    new Domain.HumanResources.Entities.RehireType(guid, "1", "description", "eligibility")
                };

                persons = new List<Domain.Base.Entities.Person>()
                {
                    new Domain.Base.Entities.Person(guid, "last name")
                };

                domainEmployee = new Domain.HumanResources.Entities.Employee(guid, "1")
                {
                    Location = "1",
                    StatusCode = "1",
                    PayStatus = Domain.HumanResources.Entities.PayStatus.PartialPay,
                    BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                    PayPeriodHours = new List<decimal?>() { 2 },
                    EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Terminated,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(100),
                    StatusEndReasonCode = "1",
                    RehireEligibilityCode = "1"
                };

                employee = new Dtos.Employee2()
                {
                    Id = guid,
                    Person = new Dtos.GuidObject2(guid),
                    Status = Dtos.EnumProperties.EmployeeStatus.Active,
                    Contract = new Dtos.DtoProperties.ContractTypeDtoProperty() { Detail = new Dtos.GuidObject2(guid), Type = Dtos.EnumProperties.ContractType.FullTime },
                    StartOn = DateTime.Today,
                    Campus = new Dtos.GuidObject2(guid),
                    PayStatus = Dtos.EnumProperties.PayStatus.PartialPay,
                    BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithBenefits,
                    EndOn = DateTime.Today.AddDays(100),
                    TerminationReason = new Dtos.GuidObject2(guid),
                    RehireableStatus = new Dtos.DtoProperties.RehireableStatusDtoProperty() { Type = new Dtos.GuidObject2(guid), Eligibility = Dtos.EnumProperties.RehireEligibility.Eligible },
                    HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                    {
                        new Dtos.DtoProperties.HoursPerPeriodDtoProperty() { Hours = 2, Period = Dtos.EnumProperties.PayPeriods.Day}
                    }
                };
            }

            private void InitializeMock()
            {
                updateEmployee.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.UpdateEmployee));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updateEmployee });

                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
                personRepositoryMock.Setup(p => p.GetPersonByGuidNonCachedAsync(It.IsAny<string>())).ReturnsAsync(persons.FirstOrDefault());
                employeeRepositoryMock.Setup(e => e.GetEmployeeIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
                referenceDataRepositoryMock.Setup(r => r.GetLocationsAsync(true)).ReturnsAsync(locations);
                hrReferenceDataRepositoryMock.Setup(h => h.GetHrStatusesAsync(true)).ReturnsAsync(hrStatuses);
                hrReferenceDataRepositoryMock.Setup(h => h.GetEmploymentStatusEndingReasonsAsync(true)).ReturnsAsync(endingReasons);
                hrReferenceDataRepositoryMock.Setup(h => h.GetRehireTypesAsync(true)).ReturnsAsync(rehireTypes);

                employeeRepositoryMock.Setup(e => e.CreateEmployee2Async(It.IsAny<Domain.HumanResources.Entities.Employee>())).ReturnsAsync(domainEmployee);
                employeeRepositoryMock.Setup(e => e.UpdateEmployee2Async(It.IsAny<Domain.HumanResources.Entities.Employee>())).ReturnsAsync(domainEmployee);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmployeeService_PostEmployeeAsync_Dto_Null()
            {
                await employeeService.PostEmployee2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmployeeService_PostEmployeeAsync_Dto_Id_Null()
            {
                await employeeService.PostEmployee2Async(new Dtos.Employee2() { Id = null });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmployeeService_PostEmployeeAsync_Validate_Person_Null()
            {
                employee.Person = null;
                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmployeeService_PostEmployeeAsync_Validate_Person_Id_Null()
            {
                employee.Person.Id = null;
                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostEmployeeAsync_Person_Id_Null_From_Repository()
            {
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);
                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostEmployeeAsync_Validate_PersonId_And_EmployeeId_NotSame_From_Repository()
            {
                personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("2");
                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostEmployeeAsync_Invalid_Employee_Status()
            {
                employee.Status = Dtos.EnumProperties.EmployeeStatus.NotSet;
                employeeRepositoryMock.Setup(e => e.GetEmployeeIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(null);

                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostEmployeeAsync_Invalid_Employee_Status_And_EmployeeId_Empty()
            {
                employee.Id = Guid.Empty.ToString();
                employee.Status = Dtos.EnumProperties.EmployeeStatus.NotSet;

                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostEmployeeAsync_Employee_Contract_Null()
            {
                employee.Contract = null;

                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostEmployeeAsync_Invalid_Contract_Type_And_Contract_Detail_Null()
            {
                employee.Contract.Detail = null;
                employee.Contract.Type = Dtos.EnumProperties.ContractType.NotSet;

                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostEmployeeAsync_Invalid_Contract_Type_And_Contract_DetailId_Null()
            {
                employee.Contract.Detail.Id = null;
                employee.Contract.Type = Dtos.EnumProperties.ContractType.NotSet;

                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PostEmployeeAsync_Employee_StartOn_Null()
            {
                employee.StartOn = null;

                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task EmployeeService_PostEmployeeAsync_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EmployeeService_PostEmployeeAsync_RepositoryException()
            {
                employeeRepositoryMock.Setup(e => e.CreateEmployee2Async(It.IsAny<Domain.HumanResources.Entities.Employee>())).ThrowsAsync(new RepositoryException());
                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PostEmployeeAsync_Exception()
            {
                employeeRepositoryMock.Setup(e => e.CreateEmployee2Async(It.IsAny<Domain.HumanResources.Entities.Employee>())).ThrowsAsync(new Exception());
                await employeeService.PostEmployee2Async(employee);
            }

            [TestMethod]
            public async Task EmployeeService_PostEmployeeAsync()
            {
                var result2 = await employeeService.PostEmployee2Async(employee);

                Assert.IsNotNull(result2);
                Assert.AreEqual(result2.Id, guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmployeeService_PutEmployeeAsync_Dto_Null()
            {
                await employeeService.PutEmployee2Async(guid, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmployeeService_PutEmployeeAsync_Dto_Id_Null()
            {
                await employeeService.PutEmployee2Async(guid, new Dtos.Employee2() { Id = null }, null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_Contract_Detail_Id_Null()
            {
                employee.Contract.Detail.Id = null;
                await employeeService.PutEmployee2Async(guid, employee, null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EmployeeService_PutEmployeeAsync_RepositoryException()
            {
                employeeRepositoryMock.Setup(e => e.UpdateEmployee2Async(It.IsAny<Domain.HumanResources.Entities.Employee>())).ThrowsAsync(new RepositoryException());
                await employeeService.PutEmployee2Async(guid, employee, null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_Exception()
            {
                employeeRepositoryMock.Setup(e => e.UpdateEmployee2Async(It.IsAny<Domain.HumanResources.Entities.Employee>())).ThrowsAsync(new Exception());
                await employeeService.PutEmployee2Async(guid, employee, null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_Status()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Active;

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_Person()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2("123");

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_location()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2("123");

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_contract()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_payclass()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayClass = new Dtos.GuidObject2("123");
                employee.PayClass = new Dtos.GuidObject2("345");

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_payclass_null()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayClass = null;
                employee.PayClass = new Dtos.GuidObject2("345");

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_paystatus()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithPay;

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_benefitstatus()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                origDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithBenefits;

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_endDate()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                origDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                origDto.EndOn = new DateTime();

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_term()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                origDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                origDto.EndOn = employee.EndOn;
                origDto.TerminationReason = new Dtos.GuidObject2("123");

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_term_null()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                origDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                origDto.EndOn = employee.EndOn;
                origDto.TerminationReason = null;

                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_rehire()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                origDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                origDto.EndOn = employee.EndOn;
                origDto.TerminationReason = employee.TerminationReason;
                origDto.RehireableStatus = null;
                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_rehireType()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = new Dtos.GuidObject2("1234");

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                origDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                origDto.EndOn = employee.EndOn;
                origDto.TerminationReason = employee.TerminationReason;
                origDto.RehireableStatus = new Dtos.DtoProperties.RehireableStatusDtoProperty();
                origDto.RehireableStatus.Type = new Dtos.GuidObject2("123");
                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_rehireId()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = new Dtos.GuidObject2("123");
                employee.RehireableStatus.Eligibility = Dtos.EnumProperties.RehireEligibility.Ineligible;
                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                origDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                origDto.EndOn = employee.EndOn;
                origDto.TerminationReason = employee.TerminationReason;
                origDto.RehireableStatus = new Dtos.DtoProperties.RehireableStatusDtoProperty();
                origDto.RehireableStatus.Type = new Dtos.GuidObject2("123");
                origDto.RehireableStatus.Eligibility = Dtos.EnumProperties.RehireEligibility.Eligible;
                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_hoursPerPeriod()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = new Dtos.GuidObject2("123");
                employee.RehireableStatus.Eligibility = Dtos.EnumProperties.RehireEligibility.Ineligible;
                var emphour = new Dtos.DtoProperties.HoursPerPeriodDtoProperty();
                emphour.Hours = 80;
                employee.HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>() { emphour };


                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                origDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                origDto.EndOn = employee.EndOn;
                origDto.TerminationReason = employee.TerminationReason;
                origDto.RehireableStatus = new Dtos.DtoProperties.RehireableStatusDtoProperty();
                origDto.RehireableStatus.Type = new Dtos.GuidObject2("123");
                origDto.RehireableStatus.Eligibility = Dtos.EnumProperties.RehireEligibility.Ineligible;
                var hour = new Dtos.DtoProperties.HoursPerPeriodDtoProperty();
                hour.Hours = 100;
                origDto.HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>() { hour };
                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EmployeeService_PutEmployeeAsync_ArgumentException_hoursPerPeriod2()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = new Dtos.GuidObject2("123");
                employee.RehireableStatus.Eligibility = Dtos.EnumProperties.RehireEligibility.Ineligible;
                
                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var origDto = new Dtos.Employee2();
                origDto.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                origDto.Person = new Dtos.GuidObject2(employee.Id);
                origDto.Campus = new Dtos.GuidObject2(employee.Campus.Id);
                origDto.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                origDto.Contract.Type = employee.Contract.Type;
                origDto.Contract.Detail = employee.Contract.Detail;
                origDto.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                origDto.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                origDto.EndOn = employee.EndOn;
                origDto.TerminationReason = employee.TerminationReason;
                origDto.RehireableStatus = new Dtos.DtoProperties.RehireableStatusDtoProperty();
                origDto.RehireableStatus.Type = new Dtos.GuidObject2("123");
                origDto.RehireableStatus.Eligibility = Dtos.EnumProperties.RehireEligibility.Ineligible;
                var hour = new Dtos.DtoProperties.HoursPerPeriodDtoProperty();
                hour.Hours = 100;
                origDto.HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>() { hour };
                var result = await employeeService.PutEmployee2Async(guid, employee, origDto);
            }


            [TestMethod]
            public async Task EmployeeService_PutEmployeeAsync()
            {
                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithoutPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.WithoutBenefits;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Leave;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "2";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithoutBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Leave;

                var result = await employeeService.PutEmployee2Async(guid, employee, employee);

                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task EmployeeService_PutEmployeeAsync_Create_Employee_With_Put_Request()
            {
                employeeRepositoryMock.SetupSequence(e => e.GetEmployeeIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>(null)).Returns(Task.FromResult<string>("1"));

                employee.PayStatus = Dtos.EnumProperties.PayStatus.WithPay;
                employee.BenefitsStatus = Dtos.EnumProperties.BenefitsStatus.NotSet;
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Terminated;
                employee.RehireableStatus.Type = null;

                domainEmployee.StatusCode = "3";
                domainEmployee.PayStatus = Domain.HumanResources.Entities.PayStatus.WithoutPay;
                domainEmployee.BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithBenefits;
                domainEmployee.EmploymentStatus = Domain.HumanResources.Entities.EmployeeStatus.Active;

                var result = await employeeService.PutEmployee2Async(guid, employee, null);

                Assert.IsNotNull(result);
            }
        }
    }

    [TestClass]
    public class EmployeeServiceTests_V12 : CurrentUserSetup
    {
        [TestClass]
        public class EmployeeServiceTests_GET_V12 : CurrentUserSetup
        {
            Mock<IEmployeeRepository> employeeRepositoryMock;
            Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            Mock<IPositionRepository> positionRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            EmployeeService employeesService;
            IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> employeesEntities;
            Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int> employeesEntityTuple;

            //IEnumerable<Domain.Base.Entities.Person> personEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType> rehireTypeEntities;
            IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason> employmentStatusEndingReasonEntities;
            //IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> acctPaySourceEntities;
            //IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionEntities;
            //IEnumerable<Domain.Base.Entities.Institution> institutionsEntities;
            //IEnumerable<Domain.HumanResources.Entities.employeePay> employeePayEntities;

            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                employeeRepositoryMock = new Mock<IEmployeeRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                positionRepositoryMock = new Mock<IPositionRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewEmployeeData);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Ellucian.Colleague.Domain.Entities.Role>() { personRole });

                employeesService = new EmployeeService(personRepositoryMock.Object, personBaseRepositoryMock.Object, employeeRepositoryMock.Object, referenceDataRepositoryMock.Object, hrReferenceDataRepositoryMock.Object,
                                               positionRepositoryMock.Object, baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                employeesEntityTuple = null;
                employeesEntities = null;
                rehireTypeEntities = null;
                employmentStatusEndingReasonEntities = null;
                employeeRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
                referenceDataRepositoryMock = null;
            }

            [TestMethod]
            public async Task Employees_GETAllAsync()
            {
                var actualsTuple =
                    await
                        employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = employeesEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

        
            [TestMethod]
            public async Task Employees_GETAllFilterAsync()
            {
                string personId = "0000011";
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                string employeeId = "x";
                employeeRepositoryMock.Setup(i => i.GetEmployeeIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(employeeId);

                var actualsTuple =
                    await
                        employeesService.GetEmployees3Async(offset, limit, It.IsAny<bool>(), "cd385d31-75ed-4d93-9a1b-4776a951396d",
                        "", "", "2000-01-01 00:00:00.000",
                        "2020-12-31 00:00:00.000", "", "");

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = employeesEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees3Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidPersonFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees3Async(offset, limit, It.IsAny<bool>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidEmployerFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees3Async(offset, limit, It.IsAny<bool>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }
        

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidStartDateFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees3Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidEndDateFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees3Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }


            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_InvalidStatusFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {

                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees3Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GETAllAsync_EmptyTuple_StatusLeaveFilter()
            {
                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                {
                };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, 0);
                employeeRepositoryMock.Setup(i => i.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                var actualsTuple = await employeesService.GetEmployees2Async(offset, limit, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), "leave", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task Employees_GET_ById()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = employeesEntities.ToList()[0];
                employeeRepositoryMock.Setup(i => i.GetEmployee2ByGuidAsync(id)).ReturnsAsync(expected);
                referenceDataRepositoryMock.Setup(i => i.GetGuidLookupResultFromGuidAsync(id)).ReturnsAsync(new GuidLookupResult() { Entity = "HRPER", PrimaryKey = id });
                var actual = await employeesService.GetEmployee3ByIdAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Employees_GET_ById_NullId_ArgumentNullException()
            {
                await employeesService.GetEmployee3ByIdAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployee2ByGuidAsync(id)).Throws<KeyNotFoundException>();
                await employeesService.GetEmployee3ByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployee2ByGuidAsync(id)).Throws<InvalidOperationException>();
                await employeesService.GetEmployee3ByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployee2ByGuidAsync(id)).Throws<RepositoryException>();
                await employeesService.GetEmployee3ByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Employees_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                employeeRepositoryMock.Setup(i => i.GetEmployee2ByGuidAsync(id)).Throws<Exception>();
                await employeesService.GetEmployee3ByIdAsync(id);
            }

            private void BuildData()
            {
                employmentStatusEndingReasonEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason>()
                {
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentStatusEndingReason("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
                hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentStatusEndingReasonsAsync(It.IsAny<bool>())).ReturnsAsync(employmentStatusEndingReasonEntities);

                rehireTypeEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.RehireType>()
                    {
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty", "i"),
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support", "o"),
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test", "i"),
                        new Ellucian.Colleague.Domain.HumanResources.Entities.RehireType("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support", "o"),
                    };
                hrReferenceDataRepositoryMock.Setup(i => i.GetRehireTypesAsync(It.IsAny<bool>())).ReturnsAsync(rehireTypeEntities);

                //var perposwgItems = new List<Domain.HumanResources.Entities.PersonemployeeWageItem>()
                //{
                //    new Domain.HumanResources.Entities.PersonemployeeWageItem()
                //    {
                //        GlNumber = "11-00-02-67-60000-53011",
                //        PpwgProjectsId = "12345",
                //        GlPercentDistribution = 100,
                //        StartDate = new DateTime(2017,7,17)
                //    }
                //};

                employeesEntities = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>()
                    {
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("ce4d68f6-257d-4052-92c8-17eed0f088fa", "e9e6837f-2c51-431b-9069-4ac4c0da3041")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)

                        },
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "9ae3a175-1dfd-4937-b97b-3c9ad596e023")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)
                        },
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("7ea5142f-12f1-4ac9-b9f3-73e4205dfc11", "e9e6837f-2c51-431b-9069-4ac4c0da3041")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)
                        },
                        new Ellucian.Colleague.Domain.HumanResources.Entities.Employee("db8f690b-071f-4d98-8da8-d4312511a4c1", "bfea651b-8e27-4fcd-abe3-04573443c04c")
                        {
                            StartDate = DateTime.Now,
                            //PayClass = "5b05410c-c94c-464a-98ee-684198bde60b",
                            EndDate = DateTime.Now,
                            PayStatus = Ellucian.Colleague.Domain.HumanResources.Entities.PayStatus.WithPay,
                            BenefitsStatus = Ellucian.Colleague.Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                            PpwgCycleWorkTimeAmt = new decimal(40.0),
                            PpwgYearWorkTimeAmt = new decimal(1600.0)
                        }
                    };
                employeesEntityTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>, int>(employeesEntities, employeesEntities.Count());
                employeeRepositoryMock.Setup(i => i.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).ReturnsAsync(employeesEntityTuple);
                employeeRepositoryMock.Setup(i => i.GetEmployee2ByGuidAsync(It.IsAny<string>())).ReturnsAsync(employeesEntities.ToList()[0]);
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
                // employeeRepositoryMock.Setup(i => i..GetemployeeGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");


            }
        }
    }
