// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.Reporting.WebForms;
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
            t4aPdfData = new FormT4aPdfData(DateTime.Now.Year.ToString(), payerName) { RecipientId = "000001" };
            form1099MiPdfData = new Form1099MIPdfData(DateTime.Now.Year.ToString(), payerName) { RecipientId = "000001" };
            TestPdfDataRepository = new TestColleagueFinanceTaxFormPdfDataRepository();


            mockTaxFormPdfDataRepository = new Mock<IColleagueFinanceTaxFormPdfDataRepository>();
            mockTaxFormPdfDataRepository.Setup(rep => rep.GetFormT4aPdfDataAsync(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(t4aPdfData);
            });

            mockTaxFormPdfDataRepository.Setup(rep => rep.GetFormT4aPdfDataAsync("000002", "2015")).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(new FormT4aPdfData(DateTime.Now.Year.ToString(), payerName) { RecipientId = "000003" });
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup(rep => rep.GetFormT4aPdfDataAsync(It.IsAny<string>(), exceptionString)).Returns<string, string>((personId, recordId) =>
            {
                throw new ColleagueWebApiException("An exception occurred.");
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
                throw new ColleagueWebApiException("An exception occurred.");
            });

            BuildTaxFormPdfService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            mockTaxFormPdfDataRepository = null;
            TestPdfDataRepository = null;
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
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetFormT4aPdfDataAsyncc_PersonIdNotMatchingException()
        {
            var pdfData = await service.GetFormT4aPdfDataAsync("000002", "2015");
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
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
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task Get1099MiscPdfDataAsync_RepositoryThrowsException()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, exceptionString);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task Get1099MiscPdfDataAsync_PersonIdNotMatchingException()
        {
            form1099MiPdfData.RecipientId = "000003";
            mockTaxFormPdfDataRepository.Setup(rep => rep.GetForm1099MiPdfDataAsync(personId, "1")).ReturnsAsync(form1099MiPdfData);
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "1");
        }

        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success_2019()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2019");
            Assert.IsTrue(pdfData is Form1099MIPdfData);
        }

        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success_2017()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2017");
            Assert.IsTrue(pdfData is Form1099MIPdfData);
        }

        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success_2016()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2016");
            Assert.IsTrue(pdfData is Form1099MIPdfData);
        }

        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success_2015()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2015");
            Assert.IsTrue(pdfData is Form1099MIPdfData);
        }

        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success_2011()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2011");
            Assert.IsTrue(pdfData is Form1099MIPdfData);
        }

        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success_2009()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2009");
            Assert.IsTrue(pdfData is Form1099MIPdfData);
        }


        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success_2010()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2010");
            Assert.IsTrue(pdfData is Form1099MIPdfData);
        }

        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success_2012()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2012");
            Assert.IsTrue(pdfData is Form1099MIPdfData);
        }

        [TestMethod]
        public async Task Get1099MiscPdfDataAsync_Success_2013()
        {
            var pdfData = await service.Get1099MiscPdfDataAsync(personId, "2013");
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

        [TestMethod]
        [ExpectedException(typeof(LocalProcessingException))]
        public void Populate1099MiscPdfReport_Success()
        {
            // There is no way to mock the local report used by rdlc. This test is here for reference.
            var pdfData = TestPdfDataRepository.Form1099MiPdfDataObjects.Where(x => x.TaxYear == "2017").FirstOrDefault();
            var pdfBytes = service.Populate1099MiscPdf(pdfData, fakePdfPath);
        }
        #endregion

        #region Private methods and helper classes

        private void BuildTaxFormPdfService(bool isPermissionsRequired = true)
        {
            // Set up the current user
            currentUserFactory = new GenericUserFactory.TaxInformationUserFactory();

            var roles = new List<Domain.Entities.Role>();

            var role = new Domain.Entities.Role(2, "VIEW.T4A");
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