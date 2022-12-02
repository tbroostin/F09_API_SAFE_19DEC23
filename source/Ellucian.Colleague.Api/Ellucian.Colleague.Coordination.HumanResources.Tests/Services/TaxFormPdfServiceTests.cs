// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Reports;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
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
        private ICurrentUserFactory currentUserFactory;
        private Mock<ILocalReportService> reportRenderServiceMock;

        private string personId = "000001";
        private string fakeRdlcPath = "fakePath";
        private string exceptionString = "exception";

        [TestInitialize]
        public void Initialize()
        {
            this.TestPdfDataRepository = new TestTaxFormPdfDataRepository();

            reportRenderServiceMock = new Mock<ILocalReportService>();
            mockTaxFormPdfDataRepository = new Mock<IHumanResourcesTaxFormPdfDataRepository>();
            mockTaxFormPdfDataRepository.Setup<Task<FormW2PdfData>>(rep => rep.GetW2PdfAsync(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(TestPdfDataRepository.GetW2PdfAsync(personId, recordId));
            });

            mockTaxFormPdfDataRepository.Setup<Task<FormW2PdfData>>(rep => rep.GetW2PdfAsync("000002", "2015")).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(new FormW2PdfData("2015", "12-345678", "000-00-0001") { EmployeeId = "000003" });
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup<Task<FormW2PdfData>>(rep => rep.GetW2PdfAsync(It.IsAny<string>(), exceptionString)).Returns<string, string>((personId, recordId) =>
            {
                throw new ColleagueWebApiException("An exception occurred.");
            });

            mockTaxFormPdfDataRepository.Setup<Task<Form1095cPdfData>>(rep => rep.Get1095cPdfAsync(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(TestPdfDataRepository.Get1095cPdfAsync(personId, recordId));
            });

            mockTaxFormPdfDataRepository.Setup<Task<Form1095cPdfData>>(rep => rep.Get1095cPdfAsync("000002", "2015")).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(new Form1095cPdfData("2015", "12-345678", "000-00-0001") { EmployeeId = "000003" });
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup<Task<Form1095cPdfData>>(rep => rep.Get1095cPdfAsync(It.IsAny<string>(), exceptionString)).Returns<string, string>((personId, recordId) =>
            {
                throw new ColleagueWebApiException("An exception occurred.");
            });

            BuildTaxFormPdfService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            TestPdfDataRepository = null;
            mockTaxFormPdfDataRepository = null;
        }
        #endregion

        #region GetW2TaxFormDataAsync Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2TaxFormDataAsync_NullPersonId()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2TaxFormDataAsync_EmptyPersonId()
        {
            var pdfData = await service.GetW2TaxFormDataAsync("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2TaxFormDataAsync_NullRecordId()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetW2TaxFormDataAsync_EmptyRecordId()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetW2TaxFormDataAsync_PersonId_MissingPermission()
        {
            BuildTaxFormPdfService(false, true, true);
            var pdfData = await service.GetW2TaxFormDataAsync("2", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetW2TaxFormDataAsync_PersonId_DoesNotMatch_CurrentUser()
        {
            var pdfData = await service.GetW2TaxFormDataAsync("000002", "2015");
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task GetW2TaxFormDataAsync_RepositoryThrowsException()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(personId, exceptionString);
        }

        [TestMethod]
        public async Task GetW2TaxFormDataAsync_Success_2015()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(personId, "2015");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormDataAsync_Success_2014()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(personId, "2014");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormDataAsync_Success_2013()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(personId, "2013");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormDataAsync_Success_2012()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(personId, "2012");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormDataAsync_Success_2011()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(personId, "2011");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }

        [TestMethod]
        public async Task GetW2TaxFormDataAsync_Success_2010()
        {
            var pdfData = await service.GetW2TaxFormDataAsync(personId, "2010");
            Assert.IsTrue(pdfData is FormW2PdfData);
        }
        #endregion

        #region PopulateW2Pdf Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateW2Pdf_Null_PdfData()
        {
            var pdfBytes = service.PopulateW2PdfReport(null, fakeRdlcPath);
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateW2Pdf_Null_Path()
        {
            var pdfData = TestPdfDataRepository.FormW2PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.PopulateW2PdfReport(pdfData, null);
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateW2Pdf_Empty_Path()
        {
            var pdfData = TestPdfDataRepository.FormW2PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.PopulateW2PdfReport(pdfData, "");
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PopulateW2Pdf_Null_PdfData_and_Path()
        {
            var pdfData = TestPdfDataRepository.FormW2PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.PopulateW2PdfReport(null, null);
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        [ExpectedException(typeof(LocalProcessingException))]
        [Ignore]
        public void PopulateW2Pdf_Success()
        {
            // There is no way to mock the local report used by rdlc. This test is here for reference.
            var pdfData = TestPdfDataRepository.FormW2PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.PopulateW2PdfReport(pdfData, fakeRdlcPath);
        }
        #endregion

        #region Get1095cTaxFormDataAsync Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cTaxFormDataAsync_NullPersonId()
        {
            var pdfdata = await service.Get1095cTaxFormDataAsync(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cTaxFormDataAsync_EmptyPersonId()
        {
            var pdfdata = await service.Get1095cTaxFormDataAsync("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cTaxFormDataAsync_NullRecordId()
        {
            var pdfdata = await service.Get1095cTaxFormDataAsync(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1095cTaxFormDataAsync_EmptyRecordId()
        {
            var pdfdata = await service.Get1095cTaxFormDataAsync(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task Get1095cTaxFormDataAsync_PersonId_MissingPermission()
        {
            BuildTaxFormPdfService(true, false, true);
            var pdfData = await service.Get1095cTaxFormDataAsync(personId, "2015");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task Get1095cTaxFormDataAsync_PersonId_DoesNotMatch_CurrentUser()
        {
            var pdfdata = await service.Get1095cTaxFormDataAsync("000002", "2015");
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task Get1095cTaxFormDataAsync_RepositoryThrowsException()
        {
            var pdfData = await service.Get1095cTaxFormDataAsync(personId, exceptionString);
        }

        [TestMethod]
        public async Task Get1095cTaxFormDataAsync_Success_2015()
        {
            var pdfdata = await service.Get1095cTaxFormDataAsync(personId, "2015");
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

        private void BuildTaxFormPdfService(bool isW2RoleRequired = true, bool is1095CRoleRequired = true, bool isT4RoleRequired = true)
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces.
            var roleRepositoryMock = new Mock<IRoleRepository>();
            var loggerObject = new Mock<ILogger>().Object;

            // Set up the current user
            currentUserFactory = new GenericUserFactory.TaxInformationUserFactory();

            var roles = new List<Domain.Entities.Role>();

            if (isW2RoleRequired)
            {
                var role = new Domain.Entities.Role(1, "VIEW.W2");
                role.AddPermission(new Domain.Entities.Permission("VIEW.W2"));
                role.AddPermission(new Domain.Entities.Permission("VIEW.EMPLOYEE.W2"));
                roles.Add(role);
            }

            if (is1095CRoleRequired)
            {
                var role = new Domain.Entities.Role(2, "VIEW.1095C");
                role.AddPermission(new Domain.Entities.Permission("VIEW.1095C"));
                role.AddPermission(new Domain.Entities.Permission("VIEW.EMPLOYEE.1095C"));
                roles.Add(role);
            }

            if (isT4RoleRequired)
            {
                var role = new Domain.Entities.Role(3, "VIEW.T4");
                role.AddPermission(new Domain.Entities.Permission("VIEW.T4"));
                role.AddPermission(new Domain.Entities.Permission("VIEW.EMPLOYEE.T4"));
                roles.Add(role);
            }

            roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var taxFormStatementDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.TaxFormStatement, Dtos.HumanResources.TaxFormStatement>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.HumanResources.Entities.TaxFormStatement, Dtos.HumanResources.TaxFormStatement>()).Returns(taxFormStatementDtoAdapter);

            // Set up the current user with a subset of tax form statements and set up the service.
            service = new HumanResourcesTaxFormPdfService(reportRenderServiceMock.Object, this.mockTaxFormPdfDataRepository.Object,
                adapterRegistry.Object,
                currentUserFactory,
                roleRepositoryMock.Object,
                loggerObject);
        }
        #endregion
    }
}
