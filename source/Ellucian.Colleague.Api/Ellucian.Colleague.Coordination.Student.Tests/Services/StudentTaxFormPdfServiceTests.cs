﻿// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PdfSharp.Pdf;
using slf4net;
using Microsoft.Reporting.WebForms;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentTaxFormPdfServiceTests
    {
        #region Initialize and Cleanup
        private StudentTaxFormPdfService service = null;
        private TestStudentTaxFormPdfDataRepository TestPdfDataRepository;
        private Mock<IStudentTaxFormPdfDataRepository> mockTaxFormPdfDataRepository;
        private Mock<IPersonRepository> mockPersonRepository;
        private ICurrentUserFactory currentUserFactory;
        private string personId = "000001";
        private string fakePdfPath = "fakePath";
        private string exceptionString = "exception";
        private List<string> institutionAddressLines;

        [TestInitialize]
        public void Initialize()
        {
            this.TestPdfDataRepository = new TestStudentTaxFormPdfDataRepository();

            mockTaxFormPdfDataRepository = new Mock<IStudentTaxFormPdfDataRepository>();
            mockTaxFormPdfDataRepository.Setup<Task<Form1098PdfData>>(rep => rep.Get1098PdfAsync(It.IsAny<string>(), It.IsAny<string>(), false)).Returns<string, string, bool>((personId, recordId, suppressNotification) =>
            {
                return Task.FromResult(TestPdfDataRepository.Get1098PdfAsync(personId, recordId));
            });

            mockTaxFormPdfDataRepository.Setup<Task<Form1098PdfData>>(rep => rep.Get1098PdfAsync("000002", "2015", false)).Returns<string, string, bool>((personId, recordId, suppressNotification) =>
            {
                return Task.FromResult(new Form1098PdfData("2015", "98-7654321") { StudentId = "0000003" });
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup<Task<Form1098PdfData>>(rep => rep.Get1098PdfAsync(It.IsAny<string>(), exceptionString, false)).Returns<string, string, bool>((personId, recordId, suppressNotification) =>
            {
                throw new Exception("An exception occurred.");
            });

            // Mock T2202A
            mockTaxFormPdfDataRepository.Setup<Task<FormT2202aPdfData>>(rep => rep.GetT2202aPdfAsync(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(TestPdfDataRepository.GetFormT2202aPdfDataAsync(personId, recordId));
            });

            mockTaxFormPdfDataRepository.Setup<Task<FormT2202aPdfData>>(rep => rep.GetT2202aPdfAsync("000002", "2")).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(new FormT2202aPdfData("2015", "0000003"));
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup<Task<FormT2202aPdfData>>(rep => rep.GetT2202aPdfAsync(It.IsAny<string>(), exceptionString)).Returns<string, string>((personId, recordId) =>
            {
                throw new Exception("An exception occurred.");
            });

            institutionAddressLines = new List<string>();
            institutionAddressLines.Add("Addr1");
            institutionAddressLines.Add("Addr2");
            institutionAddressLines.Add("Addr3");
            institutionAddressLines.Add("Addr4");

            mockPersonRepository = new Mock<IPersonRepository>();
            mockPersonRepository.Setup<Task<List<string>>>(pr => pr.Get1098HierarchyAddressAsync(It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(institutionAddressLines);
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

        #region Get1098TaxFormData Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1098TaxFormData_NullPersonId()
        {
            var pdfData = await service.Get1098TaxFormData(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1098TaxFormData_EmptyPersonId()
        {
            var pdfData = await service.Get1098TaxFormData("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1098TaxFormData_NullRecordId()
        {
            var pdfData = await service.Get1098TaxFormData(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Get1098TaxFormData_EmptyRecordId()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task Get1098TaxFormData_PersonId_MissingPermission()
        {
            BuildTaxFormPdfService(false);
            var pdfData = await service.Get1098TaxFormData("2", "1");
        }
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task Get1098TaxFormData_PersonId_DoesNotMatch_CurrentUser()
        {
            var pdfData = await service.Get1098TaxFormData("000002", "2015");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task Get1098TaxFormData_RepositoryThrowsException()
        {
            var pdfData = await service.Get1098TaxFormData(personId, exceptionString);
        }

        [TestMethod]
        public async Task Get1098TaxFormData_Success_2018()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "2018");
            Assert.IsTrue(pdfData is Form1098PdfData);
        }

        [TestMethod]
        public async Task Get1098TaxFormData_Success_2017()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "2017");
            Assert.IsTrue(pdfData is Form1098PdfData);
        }

        [TestMethod]
        public async Task Get1098TaxFormData_Success_2016()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "2016");
            Assert.IsTrue(pdfData is Form1098PdfData);
        }

        [TestMethod]
        public async Task Get1098TaxFormData_Success_2015()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "2015");
            Assert.IsTrue(pdfData is Form1098PdfData);
        }

        [TestMethod]
        public async Task Get1098TaxFormData_Success_2014()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "2014");
            Assert.IsTrue(pdfData is Form1098PdfData);
        }

        [TestMethod]
        public async Task Get1098TaxFormData_Success_2013()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "2013");
            Assert.IsTrue(pdfData is Form1098PdfData);
        }

        [TestMethod]
        public async Task Get1098TaxFormData_Success_2012()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "2012");
            Assert.IsTrue(pdfData is Form1098PdfData);
        }

        [TestMethod]
        public async Task Get1098TaxFormData_Success_2011()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "2011");
            Assert.IsTrue(pdfData is Form1098PdfData);
        }

        [TestMethod]
        public async Task Get1098TaxFormData_Success_2010()
        {
            var pdfData = await service.Get1098TaxFormData(personId, "2010");
            Assert.IsTrue(pdfData is Form1098PdfData);
        }
        #endregion

        #region Populate1098Pdf Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1098Pdf_Null_PdfData()
        {
            var pdfBytes = service.Populate1098Report(null, fakePdfPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1098Pdf_Null_Path()
        {
            var pdfData = TestPdfDataRepository.Form1098PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.Populate1098Report(pdfData, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1098Pdf_Empty_Path()
        {
            var pdfData = TestPdfDataRepository.Form1098PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.Populate1098Report(pdfData, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1098Pdf_Null_PdfData_and_Path()
        {
            var pdfData = TestPdfDataRepository.Form1098PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.Populate1098Report(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1098PdfReport_Null_PdfData()
        {
            var pdfBytes = service.Populate1098Report(null, fakePdfPath);
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        [ExpectedException(typeof(LocalProcessingException))]
        public void Populate1098Report_Success()
        {
            // There is no way to mock the local report used by rdlc. This test is here for reference.
            var pdfData = TestPdfDataRepository.Form1098PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.Populate1098Report(pdfData, fakePdfPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1098PdfReport_Null_Path()
        {
            var pdfData = TestPdfDataRepository.Form1098PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.Populate1098Report(pdfData, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1098PdfReport_Empty_Path()
        {
            var pdfData = TestPdfDataRepository.Form1098PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.Populate1098Report(pdfData, "");
            Assert.IsTrue(pdfBytes is byte[]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Populate1098PdfReport_Null_PdfData_and_Path()
        {
            var pdfData = TestPdfDataRepository.Form1098PdfDataObjects.Where(x => x.TaxYear == "2015").FirstOrDefault();
            var pdfBytes = service.Populate1098Report(null, null);
            Assert.IsTrue(pdfBytes is byte[]);
        }
        #endregion

        #region GetT2202aTaxFormData tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetT2202aTaxFormData_NullPersonId()
        {
            var pdfData = await service.GetT2202aTaxFormData(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetT2202aTaxFormData_EmptyPersonId()
        {
            var pdfData = await service.GetT2202aTaxFormData("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetT2202aTaxFormData_NullRecordId()
        {
            var pdfData = await service.GetT2202aTaxFormData(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetT2202aTaxFormData_EmptyRecordId()
        {
            var pdfData = await service.GetT2202aTaxFormData(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetT2202aTaxFormData_PersonId_MissingPermission()
        {
            BuildTaxFormPdfService(false);
            var pdfData = await service.GetT2202aTaxFormData("2", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetT2202aTaxFormData_PersonId_DoesNotMatch_CurrentUser()
        {
            var pdfData = await service.GetT2202aTaxFormData("000002", "2");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetT2202aTaxFormData_RepositoryThrowsException()
        {
            var pdfData = await service.GetT2202aTaxFormData(personId, exceptionString);
        }

        [TestMethod]
        public async Task GetT2202aTaxFormData_Success_2015()
        {
            var pdfData = await service.GetT2202aTaxFormData(personId, "2015");
            Assert.IsTrue(pdfData is FormT2202aPdfData);
        }

        [TestMethod]
        public async Task GetT2202aTaxFormData_Success_2014()
        {
            var pdfData = await service.GetT2202aTaxFormData(personId, "2014");
            Assert.IsTrue(pdfData is FormT2202aPdfData);
        }

        [TestMethod]
        public async Task GetT2202aTaxFormData_Success_2013()
        {
            var pdfData = await service.GetT2202aTaxFormData(personId, "2013");
            Assert.IsTrue(pdfData is FormT2202aPdfData);
        }

        [TestMethod]
        public async Task GetT2202aTaxFormData_Success_2012()
        {
            var pdfData = await service.GetT2202aTaxFormData(personId, "2012");
            Assert.IsTrue(pdfData is FormT2202aPdfData);
        }

        [TestMethod]
        public async Task GetT2202aTaxFormData_Success_2011()
        {
            var pdfData = await service.GetT2202aTaxFormData(personId, "2011");
            Assert.IsTrue(pdfData is FormT2202aPdfData);
        }

        [TestMethod]
        public async Task GetT2202aTaxFormData_Success_2010()
        {
            var pdfData = await service.GetT2202aTaxFormData(personId, "2010");
            Assert.IsTrue(pdfData is FormT2202aPdfData);
        }

        #endregion

        #region Private methods and helper classes

        private void BuildTaxFormPdfService(bool isPermissionsRequired = true)
        {
            // Set up the current user
            currentUserFactory = new GenericUserFactory.TaxInformationUserFactory();

            var roles = new List<Domain.Entities.Role>();

            var role = new Domain.Entities.Role(1, "VIEW.1098");
            if (isPermissionsRequired)
            {
                role.AddPermission(new Domain.Entities.Permission("VIEW.1098"));
                role.AddPermission(new Domain.Entities.Permission("VIEW.STUDENT.1098"));
            }

            roles.Add(role);

            role = new Domain.Entities.Role(2, "VIEW.T2202A");
            if (isPermissionsRequired)
            {
                role.AddPermission(new Domain.Entities.Permission("VIEW.T2202A"));
                role.AddPermission(new Domain.Entities.Permission("VIEW.STUDENT.T2202A"));
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
            service = new StudentTaxFormPdfService(this.mockTaxFormPdfDataRepository.Object,
                this.mockPersonRepository.Object,
                adapterRegistry.Object,
                currentUserFactory,
                roleRepository.Object,
                loggerObject);
        }

        #endregion
    }
}
