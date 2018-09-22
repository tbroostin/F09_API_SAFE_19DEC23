// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PdfSharp.Pdf;
using slf4net;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class TaxFormPdfServiceTests
    {
        #region Initialize and Cleanup
        private HumanResourcesTaxFormPdfService service = null;
        private TestTaxFormPdfDataRepository TestPdfDataRepository;
        private Mock<IHumanResourcesTaxFormPdfDataRepository> mockTaxFormPdfDataRepository;
        private Mock<IPdfSharpRepository> mockPdfSharpRepository;
        private ICurrentUserFactory currentUserFactory;
        private string personId = "0000001";
        private string fakeRdlcPath = "fakePath";
        private string exceptionString = "exception";

        [TestInitialize]
        public void Initialize()
        {
            this.TestPdfDataRepository = new TestTaxFormPdfDataRepository();

            mockTaxFormPdfDataRepository = new Mock<IHumanResourcesTaxFormPdfDataRepository>();
            mockTaxFormPdfDataRepository.Setup<Task<FormW2PdfData>>(rep => rep.GetW2PdfAsync(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(TestPdfDataRepository.GetW2PdfAsync(personId, recordId));
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup<Task<FormW2PdfData>>(rep => rep.GetW2PdfAsync(It.IsAny<string>(), exceptionString)).Returns<string, string>((personId, recordId) =>
            {
                throw new Exception("An exception occurred.");
            });

            mockTaxFormPdfDataRepository.Setup<Task<Form1095cPdfData>>(rep => rep.Get1095cPdfAsync(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(TestPdfDataRepository.Get1095cPdfAsync(personId, recordId));
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup<Task<Form1095cPdfData>>(rep => rep.Get1095cPdfAsync(It.IsAny<string>(), exceptionString)).Returns<string, string>((personId, recordId) =>
            {
                throw new Exception("An exception occurred.");
            });

            mockPdfSharpRepository = new Mock<IPdfSharpRepository>();

            // Mock for a populated path
            mockPdfSharpRepository.Setup<PdfDocument>(pdfu => pdfu.OpenDocument(fakeRdlcPath)).Returns(() =>
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
            TestPdfDataRepository = null;
            mockTaxFormPdfDataRepository = null;
            mockPdfSharpRepository = null;
        }
        #endregion

        #region GetW2TaxFormData Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2TaxFormData_NullPersonId()
        {
            var pdfData = await service.GetW2TaxFormData(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2TaxFormData_EmptyPersonId()
        {
            var pdfData = await service.GetW2TaxFormData("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2TaxFormData_NullRecordId()
        {
            var pdfData = await service.GetW2TaxFormData(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2TaxFormData_EmptyRecordId()
        {
            var pdfData = await service.GetW2TaxFormData(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetW2TaxFormData_PersonId_DoesNotMatch_CurrentUser()
        {
            var pdfData = await service.GetW2TaxFormData("2", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetW2TaxFormData_RepositoryThrowsException()
        {
            var pdfData = await service.GetW2TaxFormData(personId, exceptionString);
        }

        [TestMethod]
        public async Task GetW2TaxFormData_Success_2015()
        {
            var pdfData = await service.GetW2TaxFormData(personId, "2015");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormData_Success_2014()
        {
            var pdfData = await service.GetW2TaxFormData(personId, "2014");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormData_Success_2013()
        {
            var pdfData = await service.GetW2TaxFormData(personId, "2013");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormData_Success_2012()
        {
            var pdfData = await service.GetW2TaxFormData(personId, "2012");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormData_Success_2011()
        {
            var pdfData = await service.GetW2TaxFormData(personId, "2011");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormData_Success_2010()
        {
            var pdfData = await service.GetW2TaxFormData(personId, "2010");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }
        #endregion

        #region PopulateW2Pdf Tests
        [TestMethod]
        public void PopulateW2Pdf_Null_PdfData()
        {
            var pdfBytes = service.PopulateW2Pdf(null, fakeRdlcPath);
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        public void PopulateW2Pdf_Null_Path()
        {
            var pdfData = TestPdfDataRepository.FormW2PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.PopulateW2Pdf(pdfData, null);
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        public void PopulateW2Pdf_Empty_Path()
        {
            var pdfData = TestPdfDataRepository.FormW2PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.PopulateW2Pdf(pdfData, "");
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        public void PopulateW2Pdf_Null_PdfData_and_Path()
        {
            var pdfData = TestPdfDataRepository.FormW2PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.PopulateW2Pdf(null, null);
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void PopulateW2Pdf_OpenDocumentThrowsApplicationException()
        {
            var pdfData = TestPdfDataRepository.FormW2PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.PopulateW2Pdf(pdfData, exceptionString);
        }

        [TestMethod]
        public void PopulateW2Pdf_Success()
        {
            var pdfData = TestPdfDataRepository.FormW2PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.PopulateW2Pdf(pdfData, fakeRdlcPath);
            Assert.IsTrue(pdfBytes is byte[]);
        }
        #endregion

        #region Get1095cTaxFormData Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cTaxFormData_NullPersonId()
        {
            var pdfdata = await service.Get1095cTaxFormData(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cTaxFormData_EmptyPersonId()
        {
            var pdfdata = await service.Get1095cTaxFormData("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cTaxFormData_NullRecordId()
        {
            var pdfdata = await service.Get1095cTaxFormData(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cTaxFormData_EmptyRecordId()
        {
            var pdfdata = await service.Get1095cTaxFormData(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task Get1095cTaxFormData_PersonId_DoesNotMatch_CurrentUser()
        {
            var pdfdata = await service.Get1095cTaxFormData("2", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task Get1095cTaxFormData_RepositoryThrowsException()
        {
            var pdfData = await service.Get1095cTaxFormData(personId, exceptionString);
        }

        [TestMethod]
        public async Task Get1095cTaxFormData_Success_2015()
        {
            var pdfdata = await service.Get1095cTaxFormData(personId, "2015");
            Assert.IsTrue(pdfdata is Form1095cPdfData);
        }
        #endregion

        #region Populate1095tReport Tests
        [TestMethod]
        public void Populate1095tReport_Null_PdfData()
        {
            var expectedParam = "pdfData";
            var actualParam = "";
            try
            {
                var pdfBytes = service.Populate1095tReport(null, fakeRdlcPath);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void Populate1095tReport_Null_Path()
        {
            var expectedParam = "pathToReport";
            var actualParam = "";
            try
            {
                var pdfData = TestPdfDataRepository.Form1095CPdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
                var pdfBytes = service.Populate1095tReport(pdfData, null);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public void Populate1095tReport_Empty_Path()
        {
            var expectedParam = "pathToReport";
            var actualParam = "";
            try
            {
                var pdfData = TestPdfDataRepository.Form1095CPdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
                var pdfBytes = service.Populate1095tReport(pdfData, "");
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName;
            }
            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        #region Private methods and helper classes
        private void BuildTaxFormPdfService()
        {
            // Set up the current user
            currentUserFactory = new GenericUserFactory.UserFactory();

            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces.
            var roleRepository = new Mock<IRoleRepository>().Object;
            var loggerObject = new Mock<ILogger>().Object;

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var taxFormStatementDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.TaxFormStatement, Dtos.HumanResources.TaxFormStatement>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.HumanResources.Entities.TaxFormStatement, Dtos.HumanResources.TaxFormStatement>()).Returns(taxFormStatementDtoAdapter);

            // Set up the current user with a subset of tax form statements and set up the service.
            service = new HumanResourcesTaxFormPdfService(this.mockTaxFormPdfDataRepository.Object,
                this.mockPdfSharpRepository.Object,
                adapterRegistry.Object,
                currentUserFactory,
                roleRepository,
                loggerObject);
        }
        #endregion
    }
}
