// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class DepartmentsControllerTests
    {
        [TestClass]
        public class DepartmentControllerGet
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

            private DepartmentsController DepartmentController;

            private Mock<IReferenceDataRepository> DepartmentRepositoryMock;
            private IReferenceDataRepository DepartmentRepository;

            private IAdapterRegistry AdapterRegistry;

            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartmentDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                DepartmentRepositoryMock = new Mock<IReferenceDataRepository>();
                DepartmentRepository = DepartmentRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Department, Department>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                allDepartmentDtos = new TestDepartmentRepository().Get();
                var DepartmentsList = new List<Department>();

                DepartmentController = new DepartmentsController(AdapterRegistry, DepartmentRepository, logger);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.Department, Department>();
                foreach (var Department in allDepartmentDtos)
                {
                    Department target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.Department, Department>(Department);
                    DepartmentsList.Add(target);
                }
                DepartmentRepositoryMock.Setup(x => x.DepartmentsAsync()).Returns(Task.FromResult(allDepartmentDtos));
            }

            [TestCleanup]
            public void Cleanup()
            {
                DepartmentController = null;
                DepartmentRepository = null;
            }

            [TestMethod]
            public async Task ReturnsAllDepartments()
            {
                var Departments = await DepartmentController.GetDepartmentsAsync();
                Assert.IsTrue(Departments is IEnumerable<Department>);
                Assert.AreEqual(Departments.Count(), allDepartmentDtos.Count());
            }
        }

        [TestClass]
        public class DepartmentControllerGetActiveDepartments
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

            private DepartmentsController DepartmentController;

            private Mock<IReferenceDataRepository> DepartmentRepositoryMock;
            private IReferenceDataRepository DepartmentRepository;

            private IAdapterRegistry AdapterRegistry;

            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Department> allDepartmentDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                DepartmentRepositoryMock = new Mock<IReferenceDataRepository>();
                DepartmentRepository = DepartmentRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Department, Department>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                allDepartmentDtos = new TestDepartmentRepository().Get().Where(d=>d.IsActive==true);
                var DepartmentsList = new List<Department>();

                DepartmentController = new DepartmentsController(AdapterRegistry, DepartmentRepository, logger);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.Department, Department>();
                foreach (var Department in allDepartmentDtos)
                {
                    Department target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.Department, Department>(Department);
                    DepartmentsList.Add(target);
                }
                DepartmentRepositoryMock.Setup(x => x.DepartmentsAsync()).Returns(Task.FromResult(allDepartmentDtos));
            }

            [TestCleanup]
            public void Cleanup()
            {
                DepartmentController = null;
                DepartmentRepository = null;
            }

            [TestMethod]
            public async Task ReturnsActiveDepartments()
            {
                var Departments = await DepartmentController.GetActiveDepartmentsAsync();
                Assert.IsTrue(Departments is IEnumerable<Department>);
                Assert.AreEqual(Departments.Count(), allDepartmentDtos.Count());
            }
        }
    }
}
