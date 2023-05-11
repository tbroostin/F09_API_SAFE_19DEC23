// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Data.Colleague.Exceptions;
using System;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class ProgramsControllerTests
    {
        [TestClass]
        public class ProgramControllerGet
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

            private ProgramsController ProgramController;

            private Mock<IProgramRepository> ProgramRepositoryMock;
            private IProgramRepository ProgramRepository;

            private Mock<IProgramRequirementsRepository> ProgramRequirementsRepositoryMock;
            private IProgramRequirementsRepository ProgramRequirmentsRepository;

            private IAdapterRegistry AdapterRegistry;

            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program> allPrograms;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ProgramRepositoryMock = new Mock<IProgramRepository>();
                ProgramRepository = ProgramRepositoryMock.Object;

                ProgramRequirementsRepositoryMock = new Mock<IProgramRequirementsRepository>();
                ProgramRequirmentsRepository = ProgramRequirementsRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                allPrograms = await new TestProgramRepository().GetAsync();
                var ProgramsList = new List<Program>();

                ProgramController = new ProgramsController(AdapterRegistry, ProgramRepository, ProgramRequirmentsRepository, logger);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>();
                foreach (var Program in allPrograms)
                {
                    Program target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>(Program);
                    ProgramsList.Add(target);
                }
                ProgramRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(allPrograms));
            }

            [TestCleanup]
            public void Cleanup()
            {
                ProgramController = null;
                ProgramRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    ProgramRepositoryMock.Setup(x => x.GetAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                    await ProgramController.GetAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    ProgramRepositoryMock.Setup(x => x.GetAsync()).Throws(new ApplicationException());
                    //termRepoMock.Setup(x => x.GetAsync()).Throws(new ColleagueSessionExpiredException("session expired"));
                    await ProgramController.GetAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task ReturnsAllPrograms()
            {
                var Programs = await ProgramController.GetAsync();
                Assert.IsTrue(Programs is IEnumerable<Program>);
                Assert.AreEqual(Programs.Count(), allPrograms.Count());
            }

            [TestMethod]
            public async Task ReturnedDtoContainsAllDataFields()
            {
                var programs = await ProgramController.GetAsync();
                var program = programs.ElementAt(0);
                var repoProgram = allPrograms.ElementAt(0);
                Assert.AreEqual(repoProgram.Code, program.Code);
                Assert.AreEqual(repoProgram.Description, program.Description);
                Assert.AreEqual(repoProgram.Title, program.Title);
                Assert.AreEqual(repoProgram.Departments.ElementAt(0), program.Departments.ElementAt(0));
                Assert.AreEqual(repoProgram.Departments.Count(), program.Departments.Count());
                Assert.AreEqual(repoProgram.Catalogs.ElementAt(0), program.Catalogs.ElementAt(0));
                Assert.AreEqual(repoProgram.Catalogs.Count(), program.Catalogs.Count());
            }
        }


        [TestClass]
        public class ProgramControllerGetActivePrograms
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

            private ProgramsController ProgramController;

            private Mock<IProgramRepository> ProgramRepositoryMock;
            private IProgramRepository ProgramRepository;

            private Mock<IProgramRequirementsRepository> ProgramRequirementsRepositoryMock;
            private IProgramRequirementsRepository ProgramRequirmentsRepository;

            private IAdapterRegistry AdapterRegistry;

            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program> allPrograms;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ProgramRepositoryMock = new Mock<IProgramRepository>();
                ProgramRepository = ProgramRepositoryMock.Object;

                ProgramRequirementsRepositoryMock = new Mock<IProgramRequirementsRepository>();
                ProgramRequirmentsRepository = ProgramRequirementsRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                allPrograms = await new TestProgramRepository().GetAsync();
                var ProgramsList = new List<Program>();

                ProgramController = new ProgramsController(AdapterRegistry, ProgramRepository, ProgramRequirmentsRepository, logger);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>();
                foreach (var Program in allPrograms)
                {
                    Program target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program, Program>(Program);
                    ProgramsList.Add(target);
                }
                ProgramRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult(allPrograms));
            }

            [TestCleanup]
            public void Cleanup()
            {
                ProgramController = null;
                ProgramRepository = null;
            }

            [TestMethod]
            public async Task ReturnsAllActivePrograms()
            {
                var Programs = await ProgramController.GetActivePrograms2Async();
                Assert.IsTrue(Programs is IEnumerable<Program>);
                var inactivePrograms = allPrograms.Where(p => p.IsActive == false || p.IsSelectable == false);
                Assert.AreEqual(Programs.Count(), (allPrograms.Count() - inactivePrograms.Count()));
            }

            [TestMethod]
            public async Task ReturnedDtoContainsAllDataFields()
            {
                var programs = await ProgramController.GetActivePrograms2Async();
                var program = programs.ElementAt(0);
                var repoProgram = allPrograms.ElementAt(0);
                Assert.AreEqual(repoProgram.Code, program.Code);
                Assert.AreEqual(repoProgram.Description, program.Description);
                Assert.AreEqual(repoProgram.Title, program.Title);
                Assert.AreEqual(repoProgram.Departments.ElementAt(0), program.Departments.ElementAt(0));
                Assert.AreEqual(repoProgram.Departments.Count(), program.Departments.Count());
                Assert.AreEqual(repoProgram.Catalogs.ElementAt(0), program.Catalogs.ElementAt(0));
                Assert.AreEqual(repoProgram.Catalogs.Count(), program.Catalogs.Count());

            }
        }
    }
}
