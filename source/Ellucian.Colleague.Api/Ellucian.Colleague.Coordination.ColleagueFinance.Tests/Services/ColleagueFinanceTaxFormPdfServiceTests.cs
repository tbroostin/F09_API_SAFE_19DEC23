// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PdfSharp.Pdf;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class ColleagueFinanceTaxFormPdfServiceTests
    {
        #region Initialize and Cleanup
        private ColleagueFinanceTaxFormPdfService service = null;
        private Mock<IColleagueFinanceTaxFormPdfDataRepository> mockTaxFormPdfDataRepository;
        private TestColleagueFinanceTaxFormPdfDataRepository TestPdfDataRepository;
        private Mock<IPdfSharpRepository> mockPdfSharpRepository;
        private ICurrentUserFactory currentUserFactory;
        private FormT4aPdfData t4aPdfData = null;
        private Form1099MIPdfData form1099MiPdfData = null;
        private string personId = "000001";
        private string exceptionString = "exception";
        private string payerName = "Ellucian University";
        private string fakePdfPath = "fakePath";


        [TestInitialize]
        public void Initialize()
        {
            t4aPdfData = new FormT4aPdfData(DateTime.Now.Year.ToString(), payerName);
            form1099MiPdfData = new Form1099MIPdfData(DateTime.Now.Year.ToString(), payerName);
            TestPdfDataRepository = new TestColleagueFinanceTaxFormPdfDataRepository();


            mockTaxFormPdfDataRepository = new Mock<IColleagueFinanceTaxFormPdfDataRepository>();
            mockTaxFormPdfDataRepository.Setup(rep => rep.GetFormT4aPdfDataAsync(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(t4aPdfData);
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup(rep => rep.GetFormT4aPdfDataAsync(It.IsAny<string>(), exceptionString)).Returns<string, string>((personId, recordId) =>
            {
                throw new Exception("An exception occurred.");
            });

            mockTaxFormPdfDataRepository.Setup(rep => rep.GetForm1099MiPdfDataAsync(personId, "1")).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(form1099MiPdfData);
            });

            mockTaxFormPdfDataRepository.Setup(rep => rep.GetForm1099MiPdfDataAsync(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(TestPdfDataRepository.Get1099MiPdfDataAsync(personId, recordId));
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup(rep => rep.GetForm1099MiPdfDataAsync(It.IsAny<string>(), exceptionString)).Returns<string, string>((personId, recordId) =>
            {
                throw new Exception("An exception occurred.");
            });
            mockPdfSharpRepository = new Mock<IPdfSharpRepository>();

            // Mock for a populated path
            mockPdfSharpRepository.Setup<PdfDocument>(pdfu => pdfu.OpenDocument(fakePdfPath)).Returns(() =>
            {
                return new PdfDocument();
            });

            // Mock for a null path
            mockPdfSharpRepository.Setup<PdfDocument>(pdfu => pdfu.OpenDocument(null)).Returns(() =>
            {
                throw new ApplicationException("Path cannot be null.");
            });

            // Mock for an empty path
            mockPdfSharpRepository.Setup<PdfDocument>(pdfu => pdfu.OpenDocument("")).Returns(() =>
            {
                throw new ApplicationException("Path cannot be empty.");
            });

            // Mock for a thrown exception from OpenDocument
            mockPdfSharpRepository.Setup<PdfDocument>(pdfu => pdfu.OpenDocument(exceptionString)).Returns(() =>
            {
                throw new ApplicationException("Path cannot be empty.");
            });

            mockPdfSharpRepository.Setup<MemoryStream>(pdfu => pdfu.FinalizePdfDocument(It.IsAny<PdfDocument>())).Returns(() =>
            {
                return new MemoryStream();
            });
            BuildTaxFormPdfService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            mockTaxFormPdfDataRepository = null;
            TestPdfDataRepository = null;
            mockPdfSharpRepository = null;
        }
        #endregion

        #region GetFormT4aPdfDataAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetFormT4aPdfDataAsync_NullPersonId()
        {
            var pdfData = await service.GetFormT4aPdfDataAsync(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetFormT4aPdfDataAsync_EmptyPersonId()
        {
            var pdfData = await service.GetFormT4aPdfDataAsync("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetFormT4aPdfDataAsync_NullRecordId()
        {
            var pdfData = await service.GetFormT4aPdfDataAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetFormT4aPdfDataAsync_EmptyRecordId()
        {
            var pdfData = await service.GetFormT4aPdfDataAsync(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetFormT4aPdfDataAsync_PersonId_MissingPermission()
        {
            BuildTaxFormPdfService(false);
            var pdfData = await service.GetFormT4aPdfDataAsync("2", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetFormT4aPdfDataAsync_RepositoryThrowsException()
        {
            var pdfData = await service.GetFormT4aPdfDataAsync(personId, exceptionString);
        }

        [TestMethod]
        public async Task GetFormT4aPdfDataAsync_Success()
        {
            var pdfData = await service.GetFormT4aPdfDataAsync(personId, "1");
            Assert.IsTrue(pdfData is FormT4aPdfData);
        }
        #endregion
         
        #region GetForm1099MiscPdfDataAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1099MiscPdfDataAsync_NullPersonId()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1099MiscPdfDataAsync_EmptyPersonId()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1099MiscPdfDataAsync_NullRecordId()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1099MiscPdfDataAsync_EmptyRecordId()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task Get1099MiscPdfDataAsync_PersonId_MissingPermission()
        {
            BuildTaxFormPdfService(false);
            var pdfData = await service.Get1099MiscPdfDataAsync("2", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task Get1099MiscPdfDataAsync_RepositoryThrowsException()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, exceptionString);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task Get1099MiscPdfDataAsync_PersonIdNotMatchingException()
        {
            form1099MiPdfData.RecipientAccountNumber = "2";
            mockTaxFormPdfDataRepository.Setup(rep => rep.GetForm1099MiPdfDataAsync(personId, "1")).ReturnsAsync(form1099MiPdfData);
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "1");
        }

        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2017");
            Assert.IsTrue(pdfData is Form1099MIPdfData);
        }
        #endregion

        #region Populate1099MiscPdf Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1099MiscPdf_Null_PdfData()
        {
            var pdfBytes = service.Populate1099MiscPdf(null, fakePdfPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1099MiscPdf_Null_Path()
        {
            var pdfData = TestPdfDataRepository.Form1099MiPdfDataObjects.Where(x => x.TaxYear == "2017").FirstOrDefault();
            var pdfBytes = service.Populate1099MiscPdf(pdfData, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1099MiscPdf_Empty_Path()
        {
            var pdfData = TestPdfDataRepository.Form1099MiPdfDataObjects.Where(x => x.TaxYear == "2017").FirstOrDefault();
            var pdfBytes = service.Populate1099MiscPdf(pdfData, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1099MiscPdf_Null_PdfData_and_Path()
        {
            var pdfData = TestPdfDataRepository.Form1099MiPdfDataObjects.Where(x => x.TaxYear == "2017").FirstOrDefault();
            var pdfBytes = service.Populate1099MiscPdf(null, null);
        }      
               
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1099MiscPdfReport_Null_PdfData()
        {
            var pdfBytes = service.Populate1099MiscPdf(null, fakePdfPath);
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1099MiscPdfReport_Null_Path()
        {
            var pdfData = TestPdfDataRepository.Form1099MiPdfDataObjects.Where(x => x.TaxYear == "2017").FirstOrDefault();
            var pdfBytes = service.Populate1099MiscPdf(pdfData, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1099MiscPdfReport_Empty_Path()
        {
            var pdfData = TestPdfDataRepository.Form1099MiPdfDataObjects.Where(x => x.TaxYear == "2017").FirstOrDefault();
            var pdfBytes = service.Populate1099MiscPdf(pdfData, "");
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1099MiscPdfReport_Null_PdfData_and_Path()
        {
            var pdfData = TestPdfDataRepository.Form1099MiPdfDataObjects.Where(x => x.TaxYear == "2017").FirstOrDefault();
            var pdfBytes = service.Populate1099MiscPdf(null, null);
            Assert.IsTrue(pdfBytes is byte[]);
        }
        #endregion

        #region Private methods and helper classes
        private void BuildTaxFormPdfService(bool isPermissionsRequired = true)
        {
            // Set up the current user
            currentUserFactory = new GenericUserFactory.TaxInformationUserFactory();

            var roles = new List<Domain.Entities.Role>();

            var role = new Domain.Entities.Role(1, "VIEW.T4");
            if (isPermissionsRequired)
            {
                role.AddPermission(new Domain.Entities.Permission("VIEW.T4"));
                role.AddPermission(new Domain.Entities.Permission("VIEW.EMPLOYEE.T4"));
            }

            roles.Add(role);

            role = new Domain.Entities.Role(2, "VIEW.T4A");
            if (isPermissionsRequired)
            {
                role.AddPermission(new Domain.Entities.Permission("VIEW.T4A"));
                role.AddPermission(new Domain.Entities.Permission("VIEW.RECIPIENT.T4A"));
            }

            roles.Add(role);

            role = new Domain.Entities.Role(3, "VIEW.1099MISC");
            if (isPermissionsRequired)
            {
                role.AddPermission(new Domain.Entities.Permission("VIEW.1099MISC"));
            }

            roles.Add(role);

            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces.
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(r => r.Roles).Returns(roles);

            var loggerObject = new Mock<ILogger>().Object;

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var taxFormStatementDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>()).Returns(taxFormStatementDtoAdapter);

            // Set up the current user with a subset of tax form statements and set up the service.
            service = new ColleagueFinanceTaxFormPdfService(this.mockTaxFormPdfDataRepository.Object,
                adapterRegistry.Object,
                currentUserFactory,
                roleRepository.Object,
                loggerObject);
        }
        #endregion
    }
}