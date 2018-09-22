// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;


namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class BookOptionsControllerTests
    {
        [TestClass]
        public class BookOptionsControllerTestsGetAsync
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

            private BookOptionsController bookOptionsController;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private IStudentReferenceDataRepository studentReferenceDataRepository;
            private IAdapterRegistry adapterRegistry;

            private IEnumerable<Domain.Student.Entities.BookOption> allBookOptionsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistry = new Mock<IAdapterRegistry>().Object;

                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepository = studentReferenceDataRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.BookOption, BookOption>(adapterRegistry, logger);
                adapterRegistry.AddAdapter(testAdapter);

                allBookOptionsDtos = new TestBookOptionRepository().Get();
                var BookOptionsList = new List<BookOption>();

                bookOptionsController = new BookOptionsController(adapterRegistry, studentReferenceDataRepository);
                Mapper.CreateMap<Ellucian.Colleague.Domain.Student.Entities.BookOption, BookOption>();
                foreach (var BookOption in allBookOptionsDtos)
                {
                    BookOption target = Mapper.Map<Ellucian.Colleague.Domain.Student.Entities.BookOption, BookOption>(BookOption);
                    BookOptionsList.Add(target);
                }
                studentReferenceDataRepositoryMock.Setup(x => x.GetBookOptionsAsync()).Returns(Task.FromResult((allBookOptionsDtos)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                bookOptionsController = null;
                studentReferenceDataRepository = null;
            }


            [TestMethod]
            public async Task ReturnsAllBookOptions()
            {
                var BookOptions = await bookOptionsController.GetAsync();
                Assert.AreEqual(BookOptions.Count(), allBookOptionsDtos.Count());
                foreach(var option in allBookOptionsDtos)
                {
                    Assert.IsNotNull(option.Code);
                    Assert.IsNotNull(option.Description);
                    Assert.IsNotNull(option.IsRequired);
                    var bookOption = BookOptions.Where(c => c.Code == option.Code).FirstOrDefault();
                    Assert.AreEqual(bookOption.Code, option.Code);
                    Assert.AreEqual(bookOption.Description, option.Description);
                    Assert.AreEqual(bookOption.IsRequired, option.IsRequired);
                }
            }
        }
    }
}