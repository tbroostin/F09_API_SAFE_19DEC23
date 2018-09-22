// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmployeesControllerTests
    {

        [TestClass]
        public class EmployeeControllerQueryEmployeeNames
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

            private EmployeesController EmployeesController;
            private Mock<IEmployeeRepository> employeeRepositoryMock;
            private IEmployeeRepository employeeRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> allEmployeeEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IEmployeeService> employeeServiceMock;
            private IEmployeeService employeeService;
            List<Ellucian.Colleague.Dtos.Employee> EmployeeList;
            IEnumerable<Dtos.Base.Person> PersonDtos;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                employeeRepositoryMock = new Mock<IEmployeeRepository>();
                employeeRepository = employeeRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.Employee, Dtos.Employee>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                employeeServiceMock = new Mock<IEmployeeService>();
                employeeService = employeeServiceMock.Object;

                allEmployeeEntities = new TestEmployeeRepository().GetEmployees();
                EmployeeList = new List<Dtos.Employee>();

                EmployeesController = new EmployeesController(logger, employeeService);
                EmployeesController.Request = new HttpRequestMessage();
                EmployeesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                BuildData();

            }

            [TestCleanup]
            public void Cleanup()
            {
                EmployeesController = null;
                employeeRepository = null;
            }

            [TestMethod]
            public async Task EmployeesController_QueryEmployeesByPostAsync()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var people = PersonDtos;

                employeeServiceMock.Setup(s => s.QueryEmployeeNamesByPostAsync(It.IsAny<Dtos.Base.EmployeeNameQueryCriteria>())).ReturnsAsync(people);
                var employees = await EmployeesController.QueryEmployeeNamesByPostAsync(new Dtos.Base.EmployeeNameQueryCriteria() { QueryKeyword = "Jack Black"});

                Assert.IsTrue(employees is IEnumerable<Dtos.Base.Person>);

                foreach (var personDto in PersonDtos)
                {
                    var emp = employees.FirstOrDefault(i => i.Id == personDto.Id);

                    Assert.AreEqual(personDto.Id, emp.Id);
                    Assert.AreEqual(personDto.FirstName, emp.FirstName);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_QueryEmployeesByPostAsync_ThrowsPermissionsException()
            {

                employeeServiceMock.Setup(s => s.QueryEmployeeNamesByPostAsync(It.IsAny<Dtos.Base.EmployeeNameQueryCriteria>())).Throws<PermissionsException>();
                await EmployeesController.QueryEmployeeNamesByPostAsync(new Dtos.Base.EmployeeNameQueryCriteria() { QueryKeyword = "Jack Black" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_QueryEmployeesByPostAsync_ThrowsException()
            {

                employeeServiceMock.Setup(s => s.QueryEmployeeNamesByPostAsync(It.IsAny<Dtos.Base.EmployeeNameQueryCriteria>())).Throws<Exception>();
                await EmployeesController.QueryEmployeeNamesByPostAsync(new Dtos.Base.EmployeeNameQueryCriteria() { QueryKeyword = "Jack Black" });
            }

            private void BuildData()
            {
                PersonDtos = new List<Dtos.Base.Person>()
                {
                    new Dtos.Base.Person()
                    {
                        Id = "12345",
                        FirstName = "Jack",
                        LastName = "Black"
                    }
                };
            }
        }
        [TestClass]
        public class EmployeesControllerGet
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

            private EmployeesController EmployeesController;
            private Mock<IEmployeeRepository> employeeRepositoryMock;
            private IEmployeeRepository employeeRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> allEmployeeEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IEmployeeService> employeeServiceMock;
            private IEmployeeService employeeService;
            List<Ellucian.Colleague.Dtos.Employee> EmployeeList;
            private string employeesGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                employeeRepositoryMock = new Mock<IEmployeeRepository>();
                employeeRepository = employeeRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.Employee, Dtos.Employee>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                employeeServiceMock = new Mock<IEmployeeService>();
                employeeService = employeeServiceMock.Object;

                allEmployeeEntities = new TestEmployeeRepository().GetEmployees();
                EmployeeList = new List<Dtos.Employee>();

                EmployeesController = new EmployeesController(logger, employeeService);
                EmployeesController.Request = new HttpRequestMessage();
                EmployeesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var employee in allEmployeeEntities)
                {
                    Dtos.Employee target = ConvertEmployeeEntityToDto(employee);
                    EmployeeList.Add(target);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                EmployeesController = null;
                employeeRepository = null;
            }

            [TestMethod]
            public async Task EmployeesController_GetEmployeesAsync()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var tuple = new Tuple<IEnumerable<Dtos.Employee>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployeesAsync(new Paging(10, 0), "", "");

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task GetEmployeesByGuidAsync_Validate()
            {
                var thisEmployee = EmployeeList.Where(m => m.Id == employeesGuid).FirstOrDefault();

                employeeServiceMock.Setup(x => x.GetEmployeeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisEmployee);

                var employee = await EmployeesController.GetEmployeeByIdAsync(employeesGuid);
                Assert.AreEqual(thisEmployee.Id, employee.Id);
                Assert.AreEqual(thisEmployee.Person, employee.Person);
            }

            [TestMethod]
            public async Task EmployeesController_GetHedmAsync_CacheControlNotNull()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                EmployeesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

                var tuple = new Tuple<IEnumerable<Dtos.Employee>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployeesAsync(new Paging(10, 0), "", "");

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task EmployeesController_GetHedmAsync_NoCache()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                EmployeesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmployeesController.Request.Headers.CacheControl.NoCache = true;

                var tuple = new Tuple<IEnumerable<Dtos.Employee>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployeesAsync(new Paging(10, 0), "", "");

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task EmployeesController_GetHedmAsync_Cache()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                EmployeesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmployeesController.Request.Headers.CacheControl.NoCache = false;

                var tuple = new Tuple<IEnumerable<Dtos.Employee>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployeesAsync(new Paging(10, 0), "", "");

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task EmployeesController_GetByIdHedmAsync()
            {
                var thisEmployee = EmployeeList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                employeeServiceMock.Setup(x => x.GetEmployeeByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisEmployee);

                var employee = await EmployeesController.GetEmployeeByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisEmployee.Id, employee.Id);
                Assert.AreEqual(thisEmployee.Person, employee.Person);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();

                await EmployeesController.GetEmployeesAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiArgumentExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<ArgumentException>();

                await EmployeesController.GetEmployeesAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiRepositoryExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<RepositoryException>();

                await EmployeesController.GetEmployeesAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiPermissionExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployeesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<PermissionsException>();

                await EmployeesController.GetEmployeesAsync(new Paging(100, 0));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployeeByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await EmployeesController.GetEmployeeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiPermissionExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployeeByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();

                await EmployeesController.GetEmployeeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiArgumentExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployeeByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();

                await EmployeesController.GetEmployeeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiRepositoryExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployeeByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();

                await EmployeesController.GetEmployeeByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_PostThrowsIntAppiExc()
            {
                await EmployeesController.PostEmployeeAsync(EmployeeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_PutThrowsIntAppiExc()
            {
                var result = await EmployeesController.PutEmployeeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023", EmployeeList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_DeleteThrowsIntAppiExc()
            {
                await EmployeesController.DeleteEmployeeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a Employee domain entity to its corresponding Employee DTO
            /// </summary>
            /// <param name="source">Employee domain entity</param>
            /// <returns>Employee DTO</returns>
            private Ellucian.Colleague.Dtos.Employee ConvertEmployeeEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.Employee source)
            {
                var employee = new Ellucian.Colleague.Dtos.Employee();
                employee.Id = source.Guid;
                employee.Person = new Dtos.GuidObject2(source.PersonId);

                return employee;
            }
        }

        [TestClass]
        public class EmployeesControllerGet2
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

            private EmployeesController EmployeesController;
            private Mock<IEmployeeRepository> employeeRepositoryMock;
            private IEmployeeRepository employeeRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> allEmployeeEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IEmployeeService> employeeServiceMock;
            private IEmployeeService employeeService;
            List<Ellucian.Colleague.Dtos.Employee2> EmployeeList;
            private string employeesGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
           
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                employeeRepositoryMock = new Mock<IEmployeeRepository>();
                employeeRepository = employeeRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.Employee, Dtos.Employee>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                employeeServiceMock = new Mock<IEmployeeService>();
                employeeService = employeeServiceMock.Object;

                allEmployeeEntities = new TestEmployeeRepository().GetEmployees();
                EmployeeList = new List<Dtos.Employee2>();

                foreach (var employee in allEmployeeEntities)
                {
                    Dtos.Employee2 target = ConvertEmployeeEntityToDto(employee);
                    EmployeeList.Add(target);
                }

                EmployeesController = new EmployeesController(logger, employeeService);
                EmployeesController.Request = new HttpRequestMessage();
                EmployeesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                EmployeesController.Request.Properties.Add("PartialInputJsonObject",
                    JObject.FromObject(EmployeeList.Where(m => m.Id == employeesGuid).FirstOrDefault()));
            }

            [TestCleanup]
            public void Cleanup()
            {
                EmployeesController = null;
                employeeRepository = null;
            }

            [TestMethod]
            public async Task EmployeesController_GetEmployeesAsync()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var tuple = new Tuple<IEnumerable<Dtos.Employee2>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployees2Async(new Paging(10, 0), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task GetEmployeesByGuidAsync_Validate()
            {
                var thisEmployee = EmployeeList.Where(m => m.Id == employeesGuid).FirstOrDefault();

                employeeServiceMock.Setup(x => x.GetEmployee2ByIdAsync(It.IsAny<string>())).ReturnsAsync(thisEmployee);

                var employee = await EmployeesController.GetEmployee2ByIdAsync(employeesGuid);
                Assert.AreEqual(thisEmployee.Id, employee.Id);
                Assert.AreEqual(thisEmployee.Person, employee.Person);
            }

            [TestMethod]
            public async Task EmployeesController_GetHedmAsync_CacheControlNotNull()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                EmployeesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

                var tuple = new Tuple<IEnumerable<Dtos.Employee2>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployees2Async(new Paging(10, 0), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task EmployeesController_GetHedmAsync_NoCache()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                EmployeesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmployeesController.Request.Headers.CacheControl.NoCache = true;

                var tuple = new Tuple<IEnumerable<Dtos.Employee2>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployees2Async(new Paging(10, 0), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task EmployeesController_GetHedmAsync_Cache()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                EmployeesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmployeesController.Request.Headers.CacheControl.NoCache = false;

                var tuple = new Tuple<IEnumerable<Dtos.Employee2>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployees2Async(new Paging(10, 0), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task EmployeesController_GetByIdHedmAsync()
            {
                var thisEmployee = EmployeeList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                employeeServiceMock.Setup(x => x.GetEmployee2ByIdAsync(It.IsAny<string>())).ReturnsAsync(thisEmployee);

                var employee = await EmployeesController.GetEmployee2ByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisEmployee.Id, employee.Id);
                Assert.AreEqual(thisEmployee.Person, employee.Person);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();

                await EmployeesController.GetEmployees2Async(new Paging(100,0), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiArgumentExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<ArgumentException>();

                await EmployeesController.GetEmployees2Async(new Paging(100, 0), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiRepositoryExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<RepositoryException>();

                await EmployeesController.GetEmployees2Async(new Paging(100, 0), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiPermissionExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployees2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<PermissionsException>();

                await EmployeesController.GetEmployees2Async(new Paging(100, 0), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployee2ByIdAsync(It.IsAny<string>())).Throws<Exception>();

                await EmployeesController.GetEmployee2ByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiPermissionExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployee2ByIdAsync(It.IsAny<string>())).Throws<PermissionsException>();

                await EmployeesController.GetEmployee2ByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiArgumentExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployee2ByIdAsync(It.IsAny<string>())).Throws<ArgumentException>();

                await EmployeesController.GetEmployee2ByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiRepositoryExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployee2ByIdAsync(It.IsAny<string>())).Throws<RepositoryException>();

                await EmployeesController.GetEmployee2ByIdAsync("sdjfh");
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_DeleteThrowsIntAppiExc()
            {
                await EmployeesController.DeleteEmployeeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a Employee domain entity to its corresponding Employee DTO
            /// </summary>
            /// <param name="source">Employee domain entity</param>
            /// <returns>Employee DTO</returns>
            private Ellucian.Colleague.Dtos.Employee2 ConvertEmployeeEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.Employee source)
            {
                var employee = new Ellucian.Colleague.Dtos.Employee2();
                employee.Id = source.Guid;
                employee.Person = new Dtos.GuidObject2(source.PersonId);
                employee.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty()
                {
                    Type = Dtos.EnumProperties.ContractType.FullTime,
                    Detail = new Dtos.GuidObject2("9ae3a175-1dfd-4937-b97b-3c9ad596e023")
                };
                employee.StartOn = new DateTime(2017, 05, 15);
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Active;

                return employee;
            }
        }

        [TestClass]
        public class EmployeesControllerGet3
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

            private EmployeesController EmployeesController;
            private Mock<IEmployeeRepository> employeeRepositoryMock;
            private IEmployeeRepository employeeRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> allEmployeeEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IEmployeeService> employeeServiceMock;
            private IEmployeeService employeeService;
            List<Ellucian.Colleague.Dtos.Employee2> EmployeeList;
            private string employeesGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                employeeRepositoryMock = new Mock<IEmployeeRepository>();
                employeeRepository = employeeRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.Employee, Dtos.Employee>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                employeeServiceMock = new Mock<IEmployeeService>();
                employeeService = employeeServiceMock.Object;

                allEmployeeEntities = new TestEmployeeRepository().GetEmployees();
                EmployeeList = new List<Dtos.Employee2>();

                foreach (var employee in allEmployeeEntities)
                {
                    Dtos.Employee2 target = ConvertEmployeeEntityToDto(employee);
                    EmployeeList.Add(target);
                }

                EmployeesController = new EmployeesController(logger, employeeService);
                EmployeesController.Request = new HttpRequestMessage();
                EmployeesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                EmployeesController.Request.Properties.Add("PartialInputJsonObject",
                    JObject.FromObject(EmployeeList.Where(m => m.Id == employeesGuid).FirstOrDefault()));
            }

            [TestCleanup]
            public void Cleanup()
            {
                EmployeesController = null;
                employeeRepository = null;
            }

            
            [TestMethod]
            public async Task EmployeesController_GetEmployeesAsync()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var tuple = new Tuple<IEnumerable<Dtos.Employee2>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployees3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployees3Async(new Paging(10, 0), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task GetEmployeesByGuidAsync_Validate()
            {
                var thisEmployee = EmployeeList.Where(m => m.Id == employeesGuid).FirstOrDefault();

                employeeServiceMock.Setup(x => x.GetEmployee3ByIdAsync(It.IsAny<string>())).ReturnsAsync(thisEmployee);

                var employee = await EmployeesController.GetEmployee3ByIdAsync(employeesGuid);
                Assert.AreEqual(thisEmployee.Id, employee.Id);
                Assert.AreEqual(thisEmployee.Person, employee.Person);
            }

            [TestMethod]
            public async Task EmployeesController_GetHedmAsync_CacheControlNotNull()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                EmployeesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

                var tuple = new Tuple<IEnumerable<Dtos.Employee2>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployees3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployees3Async(new Paging(10, 0), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task EmployeesController_GetHedmAsync_NoCache()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                EmployeesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmployeesController.Request.Headers.CacheControl.NoCache = true;

                var tuple = new Tuple<IEnumerable<Dtos.Employee2>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployees3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployees3Async(new Paging(10, 0), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }

            [TestMethod]
            public async Task EmployeesController_GetHedmAsync_Cache()
            {
                EmployeesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                EmployeesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmployeesController.Request.Headers.CacheControl.NoCache = false;

                var tuple = new Tuple<IEnumerable<Dtos.Employee2>, int>(EmployeeList, 5);

                employeeServiceMock.Setup(s => s.GetEmployees3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var employees = await EmployeesController.GetEmployees3Async(new Paging(10, 0), criteriaFilter);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await employees.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.Employee2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.Employee2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.Employee2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(employees is IHttpActionResult);

                foreach (var employeesDto in EmployeeList)
                {
                    var emp = results.FirstOrDefault(i => i.Id == employeesDto.Id);

                    Assert.AreEqual(employeesDto.Id, emp.Id);
                    Assert.AreEqual(employeesDto.Person, emp.Person);
                }
            }
            
            [TestMethod]
            public async Task EmployeesController_GetByIdHedmAsync()
            {
                var thisEmployee = EmployeeList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                employeeServiceMock.Setup(x => x.GetEmployee3ByIdAsync(It.IsAny<string>())).ReturnsAsync(thisEmployee);

                var employee = await EmployeesController.GetEmployee3ByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisEmployee.Id, employee.Id);
                Assert.AreEqual(thisEmployee.Person, employee.Person);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployees3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();

                await EmployeesController.GetEmployees3Async(new Paging(100, 0), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiArgumentExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployees3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<ArgumentException>();

                await EmployeesController.GetEmployees3Async(new Paging(100, 0), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiRepositoryExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployees3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<RepositoryException>();

                await EmployeesController.GetEmployees3Async(new Paging(100, 0), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetThrowsIntAppiPermissionExc()
            {
                employeeServiceMock.Setup(s => s.GetEmployees3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<PermissionsException>();

                await EmployeesController.GetEmployees3Async(new Paging(100, 0), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployee3ByIdAsync(It.IsAny<string>())).Throws<Exception>();

                await EmployeesController.GetEmployee3ByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiPermissionExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployee3ByIdAsync(It.IsAny<string>())).Throws<PermissionsException>();

                await EmployeesController.GetEmployee3ByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiArgumentExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployee3ByIdAsync(It.IsAny<string>())).Throws<ArgumentException>();

                await EmployeesController.GetEmployee3ByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_GetByIdThrowsIntAppiRepositoryExc()
            {
                employeeServiceMock.Setup(gc => gc.GetEmployee3ByIdAsync(It.IsAny<string>())).Throws<RepositoryException>();

                await EmployeesController.GetEmployee3ByIdAsync("sdjfh");
            }

            [TestMethod]
            public async Task EmployeesController_Post()
            {
                var thisEmployee = EmployeeList.Where(m => m.Id == "00000000-0000-0000-0000-000000000000").FirstOrDefault();

                employeeServiceMock.Setup(gc => gc.PostEmployee2Async(thisEmployee)).ReturnsAsync(thisEmployee);
                var employee = await EmployeesController.PostEmployee3Async(thisEmployee);

                Assert.AreEqual(thisEmployee.Id, employee.Id);
                Assert.AreEqual(thisEmployee.Person, employee.Person);
            }
            

            [TestMethod]
            public async Task EmployeesController_Put()
            {
                var thisEmployee = EmployeeList.Where(m => m.Id == employeesGuid).FirstOrDefault();

                employeeServiceMock.Setup(gc => gc.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ReturnsAsync(thisEmployee);

                var employee = await EmployeesController.PutEmployee3Async(employeesGuid, thisEmployee);

                Assert.AreEqual(thisEmployee.Id, employee.Id);
                Assert.AreEqual(thisEmployee.Person, employee.Person);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeesController_DeleteThrowsIntAppiExc()
            {
                await EmployeesController.DeleteEmployeeAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a Employee domain entity to its corresponding Employee DTO
            /// </summary>
            /// <param name="source">Employee domain entity</param>
            /// <returns>Employee DTO</returns>
            private Ellucian.Colleague.Dtos.Employee2 ConvertEmployeeEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.Employee source)
            {
                var employee = new Ellucian.Colleague.Dtos.Employee2();
                employee.Id = source.Guid;
                employee.Person = new Dtos.GuidObject2(source.PersonId);
                employee.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty()
                {
                    Type = Dtos.EnumProperties.ContractType.FullTime,
                    Detail = new Dtos.GuidObject2("9ae3a175-1dfd-4937-b97b-3c9ad596e023")
                };
                employee.StartOn = new DateTime(2017, 05, 15);
                employee.Status = Dtos.EnumProperties.EmployeeStatus.Active;

                return employee;
            }
        }

        [TestClass]
        public class EmployeeControllerPost2
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }
            private Mock<IEmployeeService> employeeServiceMock;
            private Mock<ILogger> loggerMock;
            private EmployeesController employeesController;

            private Dtos.Employee2 employee;

            private string guid = "00000000-0000-0000-0000-000000000000";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                employeeServiceMock = new Mock<IEmployeeService>();

                InitializeTestData();

                employeeServiceMock.Setup(e => e.PostEmployee2Async(It.IsAny<Dtos.Employee2>())).ReturnsAsync(employee);

                employeesController = new EmployeesController(loggerMock.Object, employeeServiceMock.Object) { Request = new HttpRequestMessage() };
                employeesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                employeeServiceMock = null;
                employeesController = null;
            }

            private void InitializeTestData()
            {
                employee = new Dtos.Employee2() { Id = guid };
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PostEmployee3Async_Dto_Null()
            {
                await employeesController.PostEmployee3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PostEmployee3Async_KeyNotFoundException()
            {
                employeeServiceMock.Setup(e => e.PostEmployee2Async(It.IsAny<Dtos.Employee2>())).ThrowsAsync(new KeyNotFoundException());
                await employeesController.PostEmployee3Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PostEmployee3Async_PermissionsException()
            {
                employeeServiceMock.Setup(e => e.PostEmployee2Async(It.IsAny<Dtos.Employee2>())).ThrowsAsync(new PermissionsException());
                await employeesController.PostEmployee3Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PostEmployee3Async_ArgumentException()
            {
                employeeServiceMock.Setup(e => e.PostEmployee2Async(It.IsAny<Dtos.Employee2>())).ThrowsAsync(new ArgumentException());
                await employeesController.PostEmployee3Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PostEmployee3Async_RepositoryException()
            {
                employeeServiceMock.Setup(e => e.PostEmployee2Async(It.IsAny<Dtos.Employee2>())).ThrowsAsync(new RepositoryException());
                await employeesController.PostEmployee3Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PostEmployee3Async_RepositoryException_With_Errors()
            {
                var exception = new RepositoryException() { };

                exception.AddErrors(new List<RepositoryError>() { new RepositoryError("Error") { } });

                employeeServiceMock.Setup(e => e.PostEmployee2Async(It.IsAny<Dtos.Employee2>())).ThrowsAsync(exception);
                await employeesController.PostEmployee3Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PostEmployee3Async_IntegrationApiException()
            {
                employeeServiceMock.Setup(e => e.PostEmployee2Async(It.IsAny<Dtos.Employee2>())).ThrowsAsync(new IntegrationApiException());
                await employeesController.PostEmployee3Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PostEmployee3Async_ConfigurationException()
            {
                employeeServiceMock.Setup(e => e.PostEmployee2Async(It.IsAny<Dtos.Employee2>())).ThrowsAsync(new ConfigurationException());
                await employeesController.PostEmployee3Async(employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PostEmployee3Async_Exception()
            {
                employeeServiceMock.Setup(e => e.PostEmployee2Async(It.IsAny<Dtos.Employee2>())).ThrowsAsync(new Exception());
                await employeesController.PostEmployee3Async(employee);
            }

            [TestMethod]
            public async Task EmployeeController_PostEmployee3Async()
            {
                var result = await employeesController.PostEmployee3Async(employee);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }
        }

        [TestClass]
        public class EmployeeControllerPut2
        {
            #region DECLARATIONS

            public TestContext TestContext { get; set; }
            private Mock<IEmployeeService> employeeServiceMock;
            private Mock<ILogger> loggerMock;
            private EmployeesController employeesController;

            private Dtos.Employee2 employee;

            private string guid = "02dc2629-e8a7-410e-b4df-572d02822f8b";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                employeeServiceMock = new Mock<IEmployeeService>();

                InitializeTestData();

                employeeServiceMock.Setup(e => e.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ReturnsAsync(employee);
                //employeeServiceMock.Setup(e => e.DoesUpdateViolateDataPrivacySettings(It.IsAny<string>(), It.IsAny<object>(), true)).ReturnsAsync(true);
                employeeServiceMock.Setup(e => e.GetEmployee2ByIdAsync(It.IsAny<string>())).ReturnsAsync(employee);

                employeesController = new EmployeesController(loggerMock.Object, employeeServiceMock.Object) { Request = new HttpRequestMessage() };
                employeesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                employeesController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(employee));
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                employeeServiceMock = null;
                employeesController = null;
            }

            private void InitializeTestData()
            {
                employee = new Dtos.Employee2() { Id = guid };
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee3Async_Id_Null()
            {
                await employeesController.PutEmployee3Async(null, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee3Async_Dto_Null()
            {
                await employeesController.PutEmployee3Async(guid, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee3Async_Id_As_EmptyGuid()
            {
                await employeesController.PutEmployee3Async(Guid.Empty.ToString(), employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee3Async_Dto_Id_As_EmptyGuid()
            {
                employee.Id = Guid.Empty.ToString();
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee3Async_Dto_Id_NotSameAs_Guid()
            {
                employee.Id = Guid.NewGuid().ToString();
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee2Async_PermissionsException()
            {
                employee.Id = null;
                employeeServiceMock.Setup(e => e.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ThrowsAsync(new PermissionsException());
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee3Async_KeyNotFoundException()
            {
                employeeServiceMock.Setup(e => e.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ThrowsAsync(new KeyNotFoundException());
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee3Async_ArgumentException()
            {
                employeeServiceMock.Setup(e => e.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ThrowsAsync(new ArgumentException());
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee2Async_RepositoryException()
            {
                employeeServiceMock.Setup(e => e.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ThrowsAsync(new RepositoryException());
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee3Async_RepositoryException_With_Errors()
            {
                var exception = new RepositoryException() { };

                exception.AddErrors(new List<RepositoryError>() { new RepositoryError("Error") { } });

                employeeServiceMock.Setup(e => e.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ThrowsAsync(exception);
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee2Async_IntegrationApiException()
            {
                employeeServiceMock.Setup(e => e.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ThrowsAsync(new IntegrationApiException());
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee2Async_ConfigurationException()
            {
                employeeServiceMock.Setup(e => e.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ThrowsAsync(new ConfigurationException());
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmployeeController_PutEmployee2Async_Exception()
            {
                employeeServiceMock.Setup(e => e.PutEmployee2Async(It.IsAny<string>(), It.IsAny<Dtos.Employee2>(), It.IsAny<Dtos.Employee2>())).ThrowsAsync(new Exception());
                await employeesController.PutEmployee3Async(guid, employee);
            }

            [TestMethod]
            public async Task EmployeeController_PutEmployee2Async()
            {
                employee.Contract = new Dtos.DtoProperties.ContractTypeDtoProperty();
                employee.Contract.Detail = new Dtos.GuidObject2();
                employee.Contract.Detail.Id = "DetailGuid1";
                employee.Contract.Type = Dtos.EnumProperties.ContractType.PartTime;
                var result = await employeesController.PutEmployee3Async(guid, employee);

                Assert.IsNotNull(result);
            }
        }
    }

}