// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentTaxFormStatementServiceTests
    {
        #region Initialize and Cleanup
        private StudentTaxFormStatementService service;
        private StudentTaxFormStatementService t2202aService;

        [TestInitialize]
        public void Initialize()
        {
            taxForm1098Configuration = new TaxFormConfiguration(TaxForms.Form1098);
            taxForm1098Configuration.AddAvailability(new TaxFormAvailability("2016", true));
            taxForm1098Configuration.AddAvailability(new TaxFormAvailability("2015", true));
            Build1098Service();

            taxFormT2202aConfiguration = new TaxFormConfiguration(TaxForms.FormT2202A);
            taxFormT2202aConfiguration.AddAvailability(new TaxFormAvailability("2016", true));
            taxFormT2202aConfiguration.AddAvailability(new TaxFormAvailability("2015", true));
            BuildT2202aService();

            expectedTaxFormStatements = new List<TaxFormStatement2>();
            expectedTaxFormStatements.Add(new TaxFormStatement2("1", "2016", TaxForms.Form1098, "1234"));
            expectedTaxFormStatements.Add(new TaxFormStatement2("1", "2015", TaxForms.Form1098, "3452"));

            expectedTaxFormT2202aStatements = new List<TaxFormStatement2>();
            expectedTaxFormT2202aStatements.Add(new TaxFormStatement2("1", "2016", TaxForms.FormT2202A, "3456"));
            expectedTaxFormT2202aStatements.Add(new TaxFormStatement2("1", "2015", TaxForms.FormT2202A, "4567"));

            expectedTaxFormPdf = new Form1098PdfData("2020", "0000043");
        }

        [TestCleanup]
        public void Cleanup()
        {
            taxForm1098Configuration = null;
            taxFormT2202aConfiguration = null;
            service = null;
            t2202aService = null;
        }
        #endregion

        #region Invalid tax form
        [TestMethod]
        public async Task GetAsync_InvalidTaxFormId()
        {
            var expectedParam = "taxform";
            var actualParam = "";
            try
            {
                await service.GetAsync("1", Dtos.Base.TaxForms.FormW2);
            }
            catch (ArgumentException aex)
            {
                actualParam = aex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }
        #endregion

        #region GetAsync 1098

        [TestMethod]
        public async Task GetAsync_1098_NullPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await service.GetAsync(null, Dtos.Base.TaxForms.Form1098);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_1098_EmptyPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await service.GetAsync("", Dtos.Base.TaxForms.Form1098);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetAsync_Missing1098Permission()
        {
            Build1098Service(false);
            var actualTaxFormStatements = await service.GetAsync("1", Dtos.Base.TaxForms.Form1098);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetAsync_PersonId_DoesNotMatch_CurrentUser_1098Exception()
        {
            var actualTaxFormStatements = await service.GetAsync("3", Dtos.Base.TaxForms.Form1098);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAsync_1098_NullConfiguration()
        {
            taxForm1098Configuration = null;
            var actualTaxFormStatements = await service.GetAsync("1", Dtos.Base.TaxForms.Form1098);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetAsync_1098_StatementsPersonIdNotMatching()
        {
            expectedTaxFormStatements = new List<TaxFormStatement2>();
            expectedTaxFormStatements.Add(new TaxFormStatement2("9", "2016", TaxForms.Form1098, "1234"));
            expectedTaxFormStatements.Add(new TaxFormStatement2("9", "2015", TaxForms.Form1098, "3452"));
            var actualTaxFormStatements = await service.GetAsync("1", Dtos.Base.TaxForms.Form1098);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAsync_1098_NullStatements()
        {
            expectedTaxFormStatements = null;
            var actualTaxFormStatements = await service.GetAsync("1", Dtos.Base.TaxForms.Form1098);
        }

        [TestMethod]
        public async Task GetAsync_1098_NullConfigurationAvailabilities()
        {
            taxForm1098Configuration.RemoveAvailability("2016");
            taxForm1098Configuration.RemoveAvailability("2015");
            var actualTaxFormStatements = await service.GetAsync("1", Dtos.Base.TaxForms.Form1098);

            foreach (var expectedStatement in expectedTaxFormStatements)
            {
                var actualStatements = actualTaxFormStatements.Where(x =>
                    x.Notation == Dtos.Base.TaxFormNotations2.NotAvailable
                    && x.PdfRecordId == string.Empty
                    && x.PersonId == expectedStatement.PersonId
                    && x.TaxForm == Dtos.Base.TaxForms.Form1098
                    && x.TaxYear == expectedStatement.TaxYear).ToList();
                Assert.AreEqual(1, actualStatements.Count);
            }
        }

        [TestMethod]
        public async Task GetAsync_1098_Success()
        {
            var actualTaxFormStatements = await service.GetAsync("1", Dtos.Base.TaxForms.Form1098);

            foreach (var expectedStatement in expectedTaxFormStatements)
            {
                var actualStatements = actualTaxFormStatements.Where(x =>
                    x.Notation == Dtos.Base.TaxFormNotations2.None
                    && x.PdfRecordId == expectedStatement.PdfRecordId
                    && x.PersonId == expectedStatement.PersonId
                    && x.TaxForm == Dtos.Base.TaxForms.Form1098
                    && x.TaxYear == expectedStatement.TaxYear).ToList();
                Assert.AreEqual(1, actualStatements.Count);
            }
        }
        [TestMethod]
        public async Task GetAsync_1098_NoReportableBoxAmountsFor2019()
        {
            expectedTaxFormStatements = new List<TaxFormStatement2>();
            expectedTaxFormStatements.Add(new TaxFormStatement2("1", "2019", TaxForms.Form1098, "1234"));
            expectedTaxFormPdf = null;
            
            var actualTaxFormStatements = await service.GetAsync("1", Dtos.Base.TaxForms.Form1098);

            foreach (var expectedStatement in expectedTaxFormStatements)
            {
                var actualStatements = actualTaxFormStatements.Where(x =>
                    x.Notation == Dtos.Base.TaxFormNotations2.None
                    && x.PdfRecordId == expectedStatement.PdfRecordId
                    && x.PersonId == expectedStatement.PersonId
                    && x.TaxForm == Dtos.Base.TaxForms.Form1098
                    && x.TaxYear == expectedStatement.TaxYear).ToList();
                Assert.AreEqual(0, actualStatements.Count);
            }
        }

        #endregion

        #region GetAsync T2202a

        [TestMethod]
        public async Task GetAsync_T2202a_NullPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await t2202aService.GetAsync(null, Dtos.Base.TaxForms.FormT2202A);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_T2202a_EmptyPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await t2202aService.GetAsync("", Dtos.Base.TaxForms.FormT2202A);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetAsync_MissingT2202aPermission()
        {
            BuildT2202aService(false);
            var actualTaxFormStatements = await t2202aService.GetAsync("1", Dtos.Base.TaxForms.FormT2202A);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetAsync_PersonId_DoesNotMatch_CurrentUserT2202aException()
        {
            var actualTaxFormStatements = await t2202aService.GetAsync("3", Dtos.Base.TaxForms.FormT2202A);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAsync_T2202a_NullConfiguration()
        {
            taxFormT2202aConfiguration = null;
            var actualTaxFormStatements = await t2202aService.GetAsync("1", Dtos.Base.TaxForms.FormT2202A);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAsync_T2202a_NullStatements()
        {
            expectedTaxFormT2202aStatements = null;
            var actualTaxFormStatements = await t2202aService.GetAsync("1", Dtos.Base.TaxForms.FormT2202A);
        }
        [TestMethod]
        public async Task GetAsync_T2202a_NullConfigurationAvailabilities()
        {
            taxFormT2202aConfiguration.RemoveAvailability("2016");
            taxFormT2202aConfiguration.RemoveAvailability("2015");
            var actualTaxFormStatements = await t2202aService.GetAsync("1", Dtos.Base.TaxForms.FormT2202A);

            foreach (var expectedStatement in expectedTaxFormT2202aStatements)
            {
                var actualStatements = actualTaxFormStatements.Where(x =>
                    x.Notation == Dtos.Base.TaxFormNotations2.NotAvailable
                    && x.PdfRecordId == string.Empty
                    && x.PersonId == expectedStatement.PersonId
                    && x.TaxForm == Dtos.Base.TaxForms.FormT2202A
                    && x.TaxYear == expectedStatement.TaxYear).ToList();
                Assert.AreEqual(1, actualStatements.Count);
            }
        }

        [TestMethod]
        public async Task GetAsync_T2202a_Success()
        {
            var actualTaxFormStatements = await t2202aService.GetAsync("1", Dtos.Base.TaxForms.FormT2202A);

            foreach (var expectedStatement in expectedTaxFormT2202aStatements)
            {
                var actualStatements = actualTaxFormStatements.Where(x =>
                    x.Notation == Dtos.Base.TaxFormNotations2.None
                    && x.PdfRecordId == expectedStatement.PdfRecordId
                    && x.PersonId == expectedStatement.PersonId
                    && x.TaxForm == Dtos.Base.TaxForms.FormT2202A
                    && x.TaxYear == expectedStatement.TaxYear).ToList();
                Assert.AreEqual(1, actualStatements.Count);
            }
        }
        #endregion

        #region Build service method
        /// <summary>
        /// Builds multiple 1098 tax form service objects.
        /// </summary>
        private void Build1098Service(bool isPermissionsRequired = true)
        {
            var statementRepositoryMock = new Mock<IStudentTaxFormStatementRepository>();
            var taxFormPdfDataRepositoryMock = new Mock<IStudentTaxFormPdfDataRepository>();
            var configurationRepositoryMock = new Mock<IConfigurationRepository>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var userFactory = new StudentUserFactory.TaxInformationUserFactory();
            var roleRepositoryMock = new Mock<IRoleRepository>();
            var loggerMock = new Mock<ILogger>();


            var roles = new List<Domain.Entities.Role>();

            var role = new Domain.Entities.Role(1, "VIEW.1098");
            if (isPermissionsRequired)
            {
                role.AddPermission(new Domain.Entities.Permission("VIEW.1098"));

            }
            roles.Add(role);

            role = new Domain.Entities.Role(1, "VIEW.T2202A");
            if (isPermissionsRequired)
            {
                role.AddPermission(new Domain.Entities.Permission("VIEW.T2202A"));

            }
            roles.Add(role);

            roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

            // Set up and mock the adapter, and setup the GetAdapter method.
            var taxFormStatementDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>()).Returns(taxFormStatementDtoAdapter);

            statementRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Domain.Base.Entities.TaxForms>())).Returns(() =>
                {
                    return Task.FromResult(getStatements());
                });

            taxFormPdfDataRepositoryMock.Setup(x => x.Get1098PdfAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(() =>
            {
                return Task.FromResult(GetForm1098PdfData());
            });

            configurationRepositoryMock.Setup(x => x.GetTaxFormAvailabilityConfigurationAsync(It.IsAny<Domain.Base.Entities.TaxForms>())).Returns(() =>
                {
                    return Task.FromResult(taxForm1098Configuration);
                });

            service = new StudentTaxFormStatementService(statementRepositoryMock.Object, taxFormPdfDataRepositoryMock.Object, configurationRepositoryMock.Object,
                adapterRegistryMock.Object, userFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        private List<Domain.Base.Entities.TaxFormStatement2> expectedTaxFormStatements;

        private IEnumerable<Domain.Base.Entities.TaxFormStatement2> getStatements()
        {
            return expectedTaxFormStatements;
        }

        private Domain.Student.Entities.Form1098PdfData expectedTaxFormPdf;

        private Domain.Student.Entities.Form1098PdfData GetForm1098PdfData()
        {
            return expectedTaxFormPdf;
        }

        private TaxFormConfiguration taxForm1098Configuration;

        /// <summary>
        /// Build multiple T2202a tax form service objects.
        /// </summary>
        private void BuildT2202aService(bool isPermissionsRequired = true)
        {
            var t2202aStatementRepositoryMock = new Mock<IStudentTaxFormStatementRepository>();
            var taxFormPdfDataRepositoryMock = new Mock<IStudentTaxFormPdfDataRepository>();
            var t2202aConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var userFactory = new StudentUserFactory.TaxInformationUserFactory();
            var roleRepositoryMock = new Mock<IRoleRepository>();
            var loggerMock = new Mock<ILogger>();


            var roles = new List<Domain.Entities.Role>();

            var role = new Domain.Entities.Role(1, "VIEW.1098");
            if (isPermissionsRequired)
            {
                role.AddPermission(new Domain.Entities.Permission("VIEW.1098"));
            }
            roles.Add(role);

            role = new Domain.Entities.Role(2, "VIEW.T2202A");
            if (isPermissionsRequired)
            {
                role.AddPermission(new Domain.Entities.Permission("VIEW.T2202A"));
            }
            roles.Add(role);

            roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

            // Set up and mock the adapter, and setup the GetAdapter method.
            var taxFormStatementDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>()).Returns(taxFormStatementDtoAdapter);


            t2202aStatementRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Domain.Base.Entities.TaxForms>())).Returns(() =>
            {
                return Task.FromResult(getT2202aStatements());
            });

            t2202aConfigurationRepositoryMock.Setup(x => x.GetTaxFormAvailabilityConfigurationAsync(It.IsAny<Domain.Base.Entities.TaxForms>())).Returns(() =>
            {
                return Task.FromResult(taxFormT2202aConfiguration);
            });

            t2202aService = new StudentTaxFormStatementService(t2202aStatementRepositoryMock.Object, taxFormPdfDataRepositoryMock.Object, t2202aConfigurationRepositoryMock.Object,
                adapterRegistryMock.Object, userFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        private List<Domain.Base.Entities.TaxFormStatement2> expectedTaxFormT2202aStatements;

        private IEnumerable<Domain.Base.Entities.TaxFormStatement2> getT2202aStatements()
        {
            return expectedTaxFormT2202aStatements;
        }

        private TaxFormConfiguration taxFormT2202aConfiguration;
        #endregion
    }
}