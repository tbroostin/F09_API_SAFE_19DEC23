// Copyright 2017 Ellucian Company L.P. and its affiliates.

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
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class HumanResourcesTaxFormPdfServiceTests
    {
        #region Initialize and Cleanup

        private HumanResourcesTaxFormPdfService service = null;
        private TestHumanResourcesTaxFormPdfDataRepository TestPdfDataRepository;
        private Mock<IHumanResourcesTaxFormPdfDataRepository> mockTaxFormPdfDataRepository;
        private Mock<IPersonRepository> mockPersonRepository;
        private ICurrentUserFactory currentUserFactory;
        private string personId = "0000001";
        private string fakePdfPath = "fakePath";
        private string exceptionString = "exception";
        private List<string> institutionAddressLines;
        private Mock<IPdfSharpRepository> mockPdfSharpRepository;

        [TestInitialize]
        public void Initialize()
        {
            this.TestPdfDataRepository = new TestHumanResourcesTaxFormPdfDataRepository();

            mockTaxFormPdfDataRepository = new Mock<IHumanResourcesTaxFormPdfDataRepository>();
            mockTaxFormPdfDataRepository.Setup<Task<FormT4PdfData>>(rep => rep.GetT4PdfAsync(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((personId, recordId) =>
            {
                return Task.FromResult(TestPdfDataRepository.GetT4PdfAsync(personId, recordId));
            });

            // Mock to throw exception
            mockTaxFormPdfDataRepository.Setup<Task<FormT4PdfData>>(rep => rep.GetT4PdfAsync(It.IsAny<string>(), exceptionString)).Returns<string, string>((personId, recordId) =>
            {
                throw new Exception("An exception occurred.");
            });

            mockPdfSharpRepository = new Mock<IPdfSharpRepository>();
            BuildTaxFormPdfService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            TestPdfDataRepository = null;
            mockTaxFormPdfDataRepository = null;
        }

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
            service = new HumanResourcesTaxFormPdfService(this.mockTaxFormPdfDataRepository.Object,
                this.mockPdfSharpRepository.Object,
                adapterRegistry.Object,
                currentUserFactory,
                roleRepository.Object,
                loggerObject);
        }

        #endregion

        #region GetT4TaxFormData tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetT4TaxFormData_NullPersonId()
        {
            var pdfData = await service.GetT4TaxFormData(null, "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetT4TaxFormData_EmptyPersonId()
        {
            var pdfData = await service.GetT4TaxFormData("", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetT4TaxFormData_NullRecordId()
        {
            var pdfData = await service.GetT4TaxFormData(personId, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetT4TaxFormData_EmptyRecordId()
        {
            var pdfData = await service.GetT4TaxFormData(personId, "");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetT4TaxFormData_PersonId_PermissionMissing()
        {
            BuildTaxFormPdfService(false);
            var pdfData = await service.GetT4TaxFormData("2", "1");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetT4TaxFormData_RepositoryThrowsException()
        {
            var pdfData = await service.GetT4TaxFormData(personId, exceptionString);
        }

        [TestMethod]
        public async Task GetT4TaxFormData_Success_2015()
        {
            var pdfData = await service.GetT4TaxFormData(personId, "2015");
            Assert.IsTrue(pdfData is FormT4PdfData);
        }

        [TestMethod]
        public async Task GetT4TaxFormData_Success_2014()
        {
            var pdfData = await service.GetT4TaxFormData(personId, "2014");
            Assert.IsTrue(pdfData is FormT4PdfData);
        }

        [TestMethod]
        public async Task GetT4TaxFormData_Success_2013()
        {
            var pdfData = await service.GetT4TaxFormData(personId, "2013");
            Assert.IsTrue(pdfData is FormT4PdfData);
        }

        [TestMethod]
        public async Task GetT4TaxFormData_Success_2012()
        {
            var pdfData = await service.GetT4TaxFormData(personId, "2012");
            Assert.IsTrue(pdfData is FormT4PdfData);
        }

        [TestMethod]
        public async Task GetT4TaxFormData_Success_2011()
        {
            var pdfData = await service.GetT4TaxFormData(personId, "2011");
            Assert.IsTrue(pdfData is FormT4PdfData);
        }

        [TestMethod]
        public async Task GetT4TaxFormData_Success_2010()
        {
            var pdfData = await service.GetT4TaxFormData(personId, "2010");
            Assert.IsTrue(pdfData is FormT4PdfData);
        }

        #endregion
    }
}
